using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stock alert service implementation
/// </summary>
public class StockAlertService : BaseService, IStockAlertService
{
    private new readonly ILogger<StockAlertService> _logger;
    private readonly ISignalRService? _signalRService;
    public StockAlertService(IUnitOfWork unitOfWork, ILogger<StockAlertService> logger, ILoggingService loggingService, ICacheService cacheService, ISignalRService? signalRService = null)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _logger = logger;
        _signalRService = signalRService;
    }
    public async Task<Result> CreateLowStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant existence check
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail("Merchant not found", "MERCHANT_NOT_FOUND");
            }

            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.LowStockAlerts)
            {
                return Result.Ok(); // No alerts configured
            }

            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId &&
                               p.IsActive &&
                               p.StockQuantity > 0 &&
                               p.StockQuantity <= settings.DefaultMinimumStock,
                    cancellationToken: cancellationToken);

            var alerts = new List<StockAlert>();

            foreach (var product in products)
            {
                // Check if alert already exists
                var existingAlert = await _unitOfWork.ReadRepository<StockAlert>()
                    .FirstOrDefaultAsync(a => a.ProductId == product.Id &&
                                             a.AlertType == Domain.Enums.StockAlertType.LowStock &&
                                             !a.IsResolved,
                        cancellationToken: cancellationToken);

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        MerchantId = merchantId,
                        CurrentStock = product.StockQuantity,
                        MinimumStock = settings.DefaultMinimumStock,
                        MaximumStock = settings.DefaultMaximumStock,
                        AlertType = Domain.Enums.StockAlertType.LowStock,
                        Message = $"Product '{product.Name}' has low stock ({product.StockQuantity} remaining, minimum: {settings.DefaultMinimumStock})",
                        CreatedAt = DateTime.UtcNow
                    };

                    alerts.Add(alert);
                    await _unitOfWork.Repository<StockAlert>().AddAsync(alert, cancellationToken);
                }
            }

            if (alerts.Any())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await SendStockAlertNotificationsAsync(alerts.Select(a => a.Id).ToList(), cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating low stock alerts for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to create low stock alerts", "LOW_STOCK_ALERT_ERROR");
        }
    }
    public async Task<Result> CreateOutOfStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant existence check
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail("Merchant not found", "MERCHANT_NOT_FOUND");
            }

            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.LowStockAlerts)
            {
                return Result.Ok(); // No alerts configured
            }

            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId &&
                               p.IsActive &&
                               p.StockQuantity == 0,
                    cancellationToken: cancellationToken);

            var alerts = new List<StockAlert>();

            foreach (var product in products)
            {
                // Check if alert already exists
                var existingAlert = await _unitOfWork.ReadRepository<StockAlert>()
                    .FirstOrDefaultAsync(a => a.ProductId == product.Id &&
                                             a.AlertType == Domain.Enums.StockAlertType.OutOfStock &&
                                             !a.IsResolved,
                        cancellationToken: cancellationToken);

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        MerchantId = merchantId,
                        CurrentStock = product.StockQuantity,
                        MinimumStock = settings.DefaultMinimumStock,
                        MaximumStock = settings.DefaultMaximumStock,
                        AlertType = Domain.Enums.StockAlertType.OutOfStock,
                        Message = $"Product '{product.Name}' is out of stock",
                        CreatedAt = DateTime.UtcNow
                    };

                    alerts.Add(alert);
                    await _unitOfWork.Repository<StockAlert>().AddAsync(alert, cancellationToken);
                }
            }

            if (alerts.Any())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await SendStockAlertNotificationsAsync(alerts.Select(a => a.Id).ToList(), cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating out of stock alerts for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to create out of stock alerts", "OUT_OF_STOCK_ALERT_ERROR");
        }
    }
    public async Task<Result> CreateOverstockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant existence check
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail("Merchant not found", "MERCHANT_NOT_FOUND");
            }

            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.OverstockAlerts)
            {
                return Result.Ok(); // No alerts configured
            }

            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId &&
                               p.IsActive &&
                               p.StockQuantity >= settings.DefaultMaximumStock,
                    cancellationToken: cancellationToken);

            var alerts = new List<StockAlert>();

            foreach (var product in products)
            {
                // Check if alert already exists
                var existingAlert = await _unitOfWork.ReadRepository<StockAlert>()
                    .FirstOrDefaultAsync(a => a.ProductId == product.Id &&
                                             a.AlertType == Domain.Enums.StockAlertType.Overstock &&
                                             !a.IsResolved,
                        cancellationToken: cancellationToken);

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        MerchantId = merchantId,
                        CurrentStock = product.StockQuantity,
                        MinimumStock = settings.DefaultMinimumStock,
                        MaximumStock = settings.DefaultMaximumStock,
                        AlertType = Domain.Enums.StockAlertType.Overstock,
                        Message = $"Product '{product.Name}' is overstocked ({product.StockQuantity} units, maximum: {settings.DefaultMaximumStock})",
                        CreatedAt = DateTime.UtcNow
                    };

                    alerts.Add(alert);
                    await _unitOfWork.Repository<StockAlert>().AddAsync(alert, cancellationToken);
                }
            }

            if (alerts.Any())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await SendStockAlertNotificationsAsync(alerts.Select(a => a.Id).ToList(), cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating overstock alerts for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to create overstock alerts", "OVERSTOCK_ALERT_ERROR");
        }
    }
    public async Task<Result> ResolveStockAlertAsync(Guid alertId, Guid resolvedBy, string resolutionNotes, CancellationToken cancellationToken = default)
    {
        try
        {
            // User existence check
            var user = await _unitOfWork.ReadRepository<User>()
                .FirstOrDefaultAsync(u => u.Id == resolvedBy, cancellationToken: cancellationToken);

            if (user == null)
            {
                return Result.Fail("User not found", "USER_NOT_FOUND");
            }

            var alert = await _unitOfWork.ReadRepository<StockAlert>()
                .FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken: cancellationToken);

            if (alert == null)
            {
                return Result.Fail("Stock alert not found", "ALERT_NOT_FOUND");
            }

            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.ResolvedBy = resolvedBy;
            alert.ResolutionNotes = resolutionNotes;
            alert.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<StockAlert>().Update(alert);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving stock alert {AlertId}", alertId);
            return Result.Fail("Failed to resolve stock alert", "RESOLVE_ALERT_ERROR");
        }
    }
    public async Task<Result<StockAlertStatisticsResponse>> GetStockAlertStatisticsAsync(Guid merchantId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant existence check
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<StockAlertStatisticsResponse>("Merchant not found", "MERCHANT_NOT_FOUND");
            }

            var alerts = await _unitOfWork.ReadRepository<StockAlert>()
                .ListAsync(a => a.MerchantId == merchantId &&
                               (!fromDate.HasValue || a.CreatedAt >= fromDate.Value) &&
                               (!toDate.HasValue || a.CreatedAt <= toDate.Value),
                    cancellationToken: cancellationToken);

            var statistics = new StockAlertStatisticsResponse(
                alerts.Count,
                alerts.Count(a => a.AlertType == Domain.Enums.StockAlertType.LowStock),
                alerts.Count(a => a.AlertType == Domain.Enums.StockAlertType.OutOfStock),
                alerts.Count(a => a.AlertType == Domain.Enums.StockAlertType.Overstock),
                alerts.Count(a => a.IsResolved),
                alerts.Count(a => !a.IsResolved),
                alerts.GroupBy<StockAlert, DTO.StockAlertType>(a => (DTO.StockAlertType)a.AlertType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                fromDate ?? alerts.Min(a => a.CreatedAt),
                toDate ?? alerts.Max(a => a.CreatedAt));

            return Result.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock alert statistics for merchant {MerchantId}", merchantId);
            return Result.Fail<StockAlertStatisticsResponse>("Failed to get stock alert statistics", "ALERT_STATISTICS_ERROR");
        }
    }
    public async Task<Result> SendStockAlertNotificationsAsync(List<Guid> alertIds, CancellationToken cancellationToken = default)
    {
        if (_signalRService == null) return Result.Ok();

        try
        {
            var alerts = await _unitOfWork.ReadRepository<StockAlert>()
                .ListAsync(a => alertIds.Contains(a.Id),
                    include: "Product,ProductVariant",
                    cancellationToken: cancellationToken);

            foreach (var alert in alerts)
            {
                var notification = new RealtimeNotificationEvent(
                    Guid.NewGuid(),
                    "Stock Alert",
                    alert.Message,
                    "StockAlert",
                    DateTime.UtcNow,
                    false,
                    new Dictionary<string, object>
                    {
                        { "AlertId", alert.Id },
                        { "ProductId", alert.ProductId },
                        { "ProductName", alert.Product.Name },
                        { "AlertType", alert.AlertType.ToString() },
                        { "CurrentStock", alert.CurrentStock },
                        { "MinimumStock", alert.MinimumStock }
                    });

                await _signalRService.SendRealtimeNotificationAsync(notification);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending stock alert notifications");
            return Result.Fail("Failed to send stock alert notifications", "SEND_NOTIFICATIONS_ERROR");
        }
    }
    public async Task<Result> ConfigureStockAlertSettingsAsync(StockAlertSettingsRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // User existence check
            var user = await _unitOfWork.ReadRepository<User>()
                .FirstOrDefaultAsync(u => u.Id == merchantOwnerId, cancellationToken: cancellationToken);

            if (user == null)
            {
                return Result.Fail("User not found", "USER_NOT_FOUND");
            }

            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail("Merchant not found", "MERCHANT_NOT_FOUND");
            }

            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchant.Id, cancellationToken: cancellationToken);

            if (settings == null)
            {
                settings = new StockSettings
                {
                    Id = Guid.NewGuid(),
                    MerchantId = merchant.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<StockSettings>().AddAsync(settings, cancellationToken);
            }

            settings.AutoStockReduction = request.AutoStockReduction;
            settings.LowStockAlerts = request.LowStockAlerts;
            settings.OverstockAlerts = request.OverstockAlerts;
            settings.DefaultMinimumStock = request.DefaultMinimumStock;
            settings.DefaultMaximumStock = request.DefaultMaximumStock;
            settings.EnableStockSync = request.EnableStockSync;
            settings.ExternalSystemId = request.ExternalSystemId;
            settings.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<StockSettings>().Update(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring stock alert settings for merchant {MerchantId}", merchantOwnerId);
            return Result.Fail("Failed to configure stock alert settings", "CONFIGURE_SETTINGS_ERROR");
        }
    }
}
