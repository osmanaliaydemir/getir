using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Getir.Infrastructure.Services.Notifications;

/// <summary>
/// Push notification service implementation
/// Supports Firebase Cloud Messaging (FCM) for Android and iOS
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly ILoggingService _loggingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PushNotificationConfiguration _pushConfig;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly HttpClient _httpClient;

    public PushNotificationService(ILoggingService loggingService, IUnitOfWork unitOfWork, IOptions<PushNotificationConfiguration> pushConfig,
                                    ILogger<PushNotificationService> logger, HttpClient httpClient)
    {
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;
        _pushConfig = pushConfig.Value;
        _logger = logger;
        _httpClient = httpClient;

        // Configure HTTP client for FCM (only if ServerKey is configured)
        if (!string.IsNullOrWhiteSpace(_pushConfig.ServerKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"key={_pushConfig.ServerKey}");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }
        else
        {
            _logger.LogWarning("Push notification ServerKey is not configured. Push notifications will not work.");
        }
    }

    /// <summary>
    /// Push notification gönder
    /// </summary>
    /// <param name="request">Push notification isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Push notification gönderim sonucu</returns>
    public async Task<Result> SendPushNotificationAsync(PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if push notifications are configured
            if (string.IsNullOrWhiteSpace(_pushConfig.ServerKey))
            {
                _logger.LogWarning("Push notification requested but ServerKey is not configured");
                return Result.Fail("Push notifications are not configured", "PUSH_NOT_CONFIGURED");
            }

            // Validate device token
            if (string.IsNullOrWhiteSpace(request.DeviceToken))
            {
                return Result.Fail("Device token is required", "DEVICE_TOKEN_REQUIRED");
            }

            // Create FCM message
            var fcmMessage = CreateFcmMessage(request);

            // Send to FCM
            var result = await SendToFcmAsync(fcmMessage, cancellationToken);

            // Log push notification sending
            _loggingService.LogBusinessEvent("PushNotificationSent", new
            {
                DeviceToken = MaskDeviceToken(request.DeviceToken),
                Title = request.Title,
                Platform = GetPlatformFromToken(request.DeviceToken),
                Success = result.Success
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to device {DeviceToken}", MaskDeviceToken(request.DeviceToken));
            _loggingService.LogError("Push notification sending failed", ex, new { request.DeviceToken, request.Title });
            return Result.Fail("Failed to send push notification", "PUSH_SEND_ERROR");
        }
    }

    /// <summary>
    /// Birden fazla push notification gönder
    /// </summary>
    /// <param name="requests">Push notification istekleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Birden fazla push notification gönderim sonucu</returns>
    public async Task<Result> SendBulkPushNotificationAsync(IEnumerable<PushNotificationRequest> requests, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if push notifications are configured
            if (string.IsNullOrWhiteSpace(_pushConfig.ServerKey))
            {
                _logger.LogWarning("Bulk push notification requested but ServerKey is not configured");
                return Result.Fail("Push notifications are not configured", "PUSH_NOT_CONFIGURED");
            }

            var results = new List<Result>();
            var batchSize = 100; // FCM supports up to 1000 tokens per request
            var batches = requests.Chunk(batchSize);

            foreach (var batch in batches)
            {
                var batchTasks = batch.Select(request => SendPushNotificationAsync(request, cancellationToken));
                var batchResults = await Task.WhenAll(batchTasks);
                results.AddRange(batchResults);

                // Add delay between batches to respect rate limits
                if (batches.Count() > 1)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count - successCount;

            _loggingService.LogBusinessEvent("BulkPushNotificationSent", new
            {
                TotalCount = results.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            });

            if (failureCount == 0)
            {
                return Result.Ok();
            }

            return Result.Fail($"Bulk push notification completed with {failureCount} failures", "BULK_PUSH_PARTIAL_FAILURE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk push notifications");
            _loggingService.LogError("Bulk push notification sending failed", ex);
            return Result.Fail("Failed to send bulk push notifications", "BULK_PUSH_ERROR");
        }
    }

    /// <summary>
    /// Push notification to topic gönder
    /// </summary>
    /// <param name="request">Push notification to topic isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Push notification to topic gönderim sonucu</returns>
    public async Task<Result> SendPushToTopicAsync(PushTopicRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var fcmMessage = new
            {
                to = $"/topics/{request.Topic}",
                notification = new
                {
                    title = request.Title,
                    body = request.Body,
                    image = request.ImageUrl
                },
                data = request.Data,
                android = new
                {
                    priority = "high",
                    notification = new
                    {
                        click_action = request.ActionUrl,
                        sound = "default"
                    }
                },
                apns = new
                {
                    payload = new
                    {
                        aps = new
                        {
                            alert = new
                            {
                                title = request.Title,
                                body = request.Body
                            },
                            sound = "default",
                            badge = 1
                        }
                    }
                }
            };

            var result = await SendToFcmAsync(fcmMessage, cancellationToken);

            _loggingService.LogBusinessEvent("PushNotificationToTopicSent", new
            {
                Topic = request.Topic,
                Title = request.Title,
                Success = result.Success
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to topic {Topic}", request.Topic);
            _loggingService.LogError("Push notification to topic failed", ex, new { request.Topic, request.Title });
            return Result.Fail("Failed to send push notification to topic", "PUSH_TOPIC_SEND_ERROR");
        }
    }

    /// <summary>
    /// Push notification to user gönder
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Push notification to user isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Push notification to user gönderim sonucu</returns>
    public async Task<Result> SendPushToUserAsync(Guid userId, PushNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user's device tokens
            var deviceTokens = await _unitOfWork.ReadRepository<DeviceToken>()
                .ListAsync(
                    filter: dt => dt.UserId == userId && dt.IsActive,
                    cancellationToken: cancellationToken);

            if (!deviceTokens.Any())
            {
                return Result.Fail("User has no active device tokens", "NO_DEVICE_TOKENS");
            }

            // Send to all user's devices
            var results = new List<Result>();
            foreach (var deviceToken in deviceTokens)
            {
                var userRequest = request with { DeviceToken = deviceToken.Token };
                var result = await SendPushNotificationAsync(userRequest, cancellationToken);
                results.Add(result);
            }

            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count - successCount;

            _loggingService.LogBusinessEvent("PushNotificationToUserSent", new
            {
                UserId = userId,
                DeviceCount = deviceTokens.Count,
                SuccessCount = successCount,
                FailureCount = failureCount
            });

            if (failureCount == 0)
            {
                return Result.Ok();
            }

            return Result.Fail($"Push notification to user completed with {failureCount} failures", "PUSH_USER_PARTIAL_FAILURE");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            _loggingService.LogError("Push notification to user failed", ex, new { userId });
            return Result.Fail("Failed to send push notification to user", "PUSH_USER_SEND_ERROR");
        }
    }

    /// <summary>
    /// Device token kaydet
    /// </summary>
    /// <param name="request">Device token isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Device token kaydetme sonucu</returns>
    public async Task<Result> RegisterDeviceTokenAsync(DeviceTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if token already exists
            var existingToken = await _unitOfWork.ReadRepository<DeviceToken>()
                .FirstOrDefaultAsync(
                    filter: dt => dt.Token == request.DeviceToken,
                    cancellationToken: cancellationToken);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.UserId = request.UserId;
                existingToken.Platform = request.Platform.ToString();
                existingToken.DeviceModel = request.DeviceModel;
                existingToken.AppVersion = request.AppVersion;
                existingToken.IsActive = request.IsActive;
                existingToken.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeviceToken>().Update(existingToken);
            }
            else
            {
                // Create new token
                var deviceToken = new DeviceToken
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Token = request.DeviceToken,
                    Platform = request.Platform.ToString(),
                    DeviceModel = request.DeviceModel,
                    AppVersion = request.AppVersion,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<DeviceToken>().AddAsync(deviceToken, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("DeviceTokenRegistered", new
            {
                UserId = request.UserId,
                Platform = request.Platform,
                DeviceModel = request.DeviceModel,
                IsActive = request.IsActive
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user {UserId}", request.UserId);
            _loggingService.LogError("Device token registration failed", ex, new { request.UserId, request.Platform });
            return Result.Fail("Failed to register device token", "DEVICE_TOKEN_REGISTRATION_ERROR");
        }
    }

    /// <summary>
    /// Device token sil
    /// </summary>
    /// <param name="deviceToken">Device token</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Device token silme sonucu</returns>
    public async Task<Result> UnregisterDeviceTokenAsync(string deviceToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _unitOfWork.ReadRepository<DeviceToken>()
                .FirstOrDefaultAsync(
                    filter: dt => dt.Token == deviceToken,
                    cancellationToken: cancellationToken);

            if (token == null)
            {
                return Result.Fail("Device token not found", "DEVICE_TOKEN_NOT_FOUND");
            }

            token.IsActive = false;
            token.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<DeviceToken>().Update(token);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("DeviceTokenUnregistered", new
            {
                DeviceToken = MaskDeviceToken(deviceToken),
                UserId = token.UserId
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token {DeviceToken}", MaskDeviceToken(deviceToken));
            _loggingService.LogError("Device token unregistration failed", ex, new { deviceToken });
            return Result.Fail("Failed to unregister device token", "DEVICE_TOKEN_UNREGISTRATION_ERROR");
        }
    }

    /// <summary>
    /// Push notification durumunu getir
    /// </summary>
    /// <param name="messageId">Mesaj ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Push notification durumunu</returns>
    public Task<Result<PushNotificationStatus>> GetNotificationStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would check with FCM
            // For now, return a mock status
            var status = new PushNotificationStatus(
                messageId,
                PushStatus.Delivered,
                DateTime.UtcNow,
                null,
                null);

            return Task.FromResult(Result.Ok(status));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting push notification status for message {MessageId}", messageId);
            return Task.FromResult(Result.Fail<PushNotificationStatus>("Failed to get notification status", "NOTIFICATION_STATUS_ERROR"));
        }
    }

    #region Helper Methods

    /// <summary>
    /// FCM mesajı oluştur
    /// </summary>
    /// <param name="request">Push notification isteği</param>
    /// <returns>FCM mesajı</returns>
    private object CreateFcmMessage(PushNotificationRequest request)
    {
        return new
        {
            to = request.DeviceToken,
            notification = new
            {
                title = request.Title,
                body = request.Body,
                image = request.ImageUrl
            },
            data = request.Data,
            android = new
            {
                priority = request.Priority == PushNotificationPriority.High ? "high" : "normal",
                notification = new
                {
                    click_action = request.ActionUrl,
                    sound = "default"
                }
            },
            apns = new
            {
                payload = new
                {
                    aps = new
                    {
                        alert = new
                        {
                            title = request.Title,
                            body = request.Body
                        },
                        sound = "default",
                        badge = 1
                    }
                }
            }
        };
    }

    /// <summary>
    /// FCM API'ye gönder
    /// </summary>
    /// <param name="fcmMessage">FCM mesajı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>FCM API'ye gönderim sonucu</returns>
    private async Task<Result> SendToFcmAsync(object fcmMessage, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(fcmMessage);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_pushConfig.FcmUrl, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (result.TryGetProperty("success", out var successElement) && successElement.GetInt32() == 1)
                {
                    return Result.Ok();
                }
            }

            return Result.Fail($"FCM API error: {responseContent}", "FCM_API_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM API call failed");
            return Result.Fail("FCM API call failed", "FCM_API_ERROR");
        }
    }

    /// <summary>
    /// Device token'ı maskele
    /// </summary>
    /// <param name="deviceToken">Device token</param>
    /// <returns>Maskeleli device token</returns>
    private string MaskDeviceToken(string deviceToken)
    {
        if (string.IsNullOrEmpty(deviceToken) || deviceToken.Length < 8)
            return "***";
            
        return deviceToken.Substring(0, 4) + "***" + deviceToken.Substring(deviceToken.Length - 4);
    }

    private string GetPlatformFromToken(string deviceToken)
    {
        // Simple heuristic to determine platform from token format
        if (deviceToken.Length > 100)
            return "iOS";
        else if (deviceToken.Contains(":"))
            return "Android";
        else
            return "Web";
    }

    #endregion
}

/// <summary>
/// Push notification configuration
/// </summary>
public class PushNotificationConfiguration
{
    public string ServerKey { get; set; } = string.Empty;
    public string FcmUrl { get; set; } = "https://fcm.googleapis.com/fcm/send";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
}
