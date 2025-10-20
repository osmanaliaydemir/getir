using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stok senkronizasyon servisi implementasyonu: harici sistem entegrasyonu, transaction, sync geçmişi, otomatik sync.
/// </summary>
public class StockSyncService : BaseService, IStockSyncService
{
    private new readonly ILogger<StockSyncService> _logger;
    public StockSyncService(IUnitOfWork unitOfWork, ILogger<StockSyncService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _logger = logger;
    }
    /// <summary>
    /// Harici sistem ile stok senkronizasyonu yapar (transaction, hata yönetimi, sync geçmişi).
    /// </summary>
    public async Task<Result<StockSyncResponse>> SynchronizeWithExternalSystemAsync(StockSyncRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == request.MerchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.EnableStockSync)
            {
                return Result.Fail<StockSyncResponse>("Stock synchronization not enabled", "SYNC_NOT_ENABLED");
            }

            var syncSession = new StockSyncSession
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                ExternalSystemId = request.ExternalSystemId,
                SyncType = StockSyncType.Manual,
                Status = StockSyncStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<StockSyncSession>().AddAsync(syncSession, cancellationToken);

            var syncedItems = 0;
            var failedItems = 0;
            var errors = new List<StockSyncError>();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var externalItem in request.ExternalItems)
                {
                    try
                    {
                        // Find matching product by external ID
                        var product = await FindProductByExternalIdAsync(
                            request.MerchantId,
                            externalItem.ExternalProductId,
                            cancellationToken);

                        if (product == null)
                        {
                            errors.Add(new StockSyncError(
                                externalItem.ExternalProductId,
                                externalItem.ExternalVariantId,
                                "Product not found",
                                "PRODUCT_NOT_FOUND"));
                            failedItems++;
                            continue;
                        }

                        // Update stock quantity
                        var previousQuantity = product.StockQuantity;
                        product.StockQuantity = externalItem.StockQuantity;
                        product.IsAvailable = externalItem.IsAvailable;
                        product.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Repository<Product>().Update(product);

                        // Create stock history
                        await CreateStockHistoryAsync(
                            product.Id,
                            null,
                            previousQuantity,
                            externalItem.StockQuantity,
                            Domain.Enums.StockChangeType.Sync,
                            "External system synchronization",
                            null,
                            syncSession.Id.ToString(),
                            cancellationToken);

                        // Create sync detail
                        var syncDetail = new StockSyncDetail
                        {
                            Id = Guid.NewGuid(),
                            SyncSessionId = syncSession.Id,
                            ProductId = product.Id,
                            ExternalProductId = externalItem.ExternalProductId,
                            ExternalVariantId = externalItem.ExternalVariantId,
                            PreviousQuantity = previousQuantity,
                            NewQuantity = externalItem.StockQuantity,
                            SyncStatus = StockSyncDetailStatus.Success,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Repository<StockSyncDetail>().AddAsync(syncDetail, cancellationToken);
                        syncedItems++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing product {ExternalProductId}", externalItem.ExternalProductId);

                        errors.Add(new StockSyncError(
                            externalItem.ExternalProductId,
                            externalItem.ExternalVariantId,
                            ex.Message,
                            "SYNC_ERROR"));

                        failedItems++;
                    }
                }

                syncSession.Status = failedItems > 0 ? StockSyncStatus.PartialSuccess : StockSyncStatus.Success;
                syncSession.CompletedAt = DateTime.UtcNow;
                syncSession.SyncedItemsCount = syncedItems;
                syncSession.FailedItemsCount = failedItems;

                _unitOfWork.Repository<StockSyncSession>().Update(syncSession);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                // Update last sync time
                settings.LastSyncAt = DateTime.UtcNow;
                settings.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<StockSettings>().Update(settings);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = new StockSyncResponse(
                    syncedItems + failedItems,
                    syncedItems,
                    failedItems,
                    errors);

                return Result.Ok(response);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing stock for merchant {MerchantId}", request.MerchantId);
            return Result.Fail<StockSyncResponse>("Failed to synchronize stock", "STOCK_SYNC_ERROR");
        }
    }
    /// <summary>
    /// Senkronizasyon geçmişini getirir (tarih filtresi ile, zaman sıralı).
    /// </summary>
    public async Task<Result<List<StockSyncHistoryResponse>>> GetSynchronizationHistoryAsync(Guid merchantId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _unitOfWork.ReadRepository<StockSyncSession>()
                .ListAsync(s => s.MerchantId == merchantId &&
                               (!fromDate.HasValue || s.StartedAt >= fromDate.Value) &&
                               (!toDate.HasValue || s.StartedAt <= toDate.Value),
                    orderBy: s => s.StartedAt,
                    ascending: false,
                    cancellationToken: cancellationToken);

            var responses = sessions.Select(s => new StockSyncHistoryResponse(
                s.Id,
                s.ExternalSystemId,
                s.SyncType,
                s.Status,
                s.StartedAt,
                s.CompletedAt,
                s.SyncedItemsCount,
                s.FailedItemsCount,
                s.ErrorMessage)).ToList();

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting synchronization history for merchant {MerchantId}", merchantId);
            return Result.Fail<List<StockSyncHistoryResponse>>("Failed to get synchronization history", "SYNC_HISTORY_ERROR");
        }
    }
    /// <summary>
    /// Senkronizasyon durumunu getirir (etkin mi, son sync zamanı, aralık).
    /// </summary>
    public async Task<Result<StockSyncStatusResponse>> GetSynchronizationStatusAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null)
            {
                return Result.Fail<StockSyncStatusResponse>("Stock synchronization not configured", "SYNC_NOT_CONFIGURED");
            }

            var lastSession = await _unitOfWork.ReadRepository<StockSyncSession>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId, cancellationToken: cancellationToken);

            var status = new StockSyncStatusResponse(
                settings.EnableStockSync,
                settings.ExternalSystemId,
                settings.LastSyncAt,
                lastSession?.Status,
                settings.SyncIntervalMinutes,
                lastSession?.StartedAt);

            return Result.Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting synchronization status for merchant {MerchantId}", merchantId);
            return Result.Fail<StockSyncStatusResponse>("Failed to get synchronization status", "SYNC_STATUS_ERROR");
        }
    }
    /// <summary>
    /// Harici sistem bağlantısını yapılandırır (API key/URL, sync aralığı, ownership kontrolü).
    /// </summary>
    public async Task<Result> ConfigureExternalSystemAsync(ExternalSystemConfigRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
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

            settings.EnableStockSync = request.EnableStockSync;
            settings.ExternalSystemId = request.ExternalSystemId;
            settings.SyncApiKey = request.ApiKey;
            settings.SyncApiUrl = request.ApiUrl;
            settings.SyncIntervalMinutes = request.SyncIntervalMinutes;
            settings.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<StockSettings>().Update(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring external system for merchant {MerchantId}", merchantOwnerId);
            return Result.Fail("Failed to configure external system", "EXTERNAL_SYSTEM_CONFIG_ERROR");
        }
    }
    /// <summary>
    /// Harici sistem bağlantısını test eder (mock implementasyon).
    /// </summary>
    public async Task<Result<ConnectionTestResponse>> TestExternalSystemConnectionAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.EnableStockSync)
            {
                return Result.Fail<ConnectionTestResponse>("Stock synchronization not configured", "SYNC_NOT_CONFIGURED");
            }

            // In a real implementation, you would test the actual API connection here
            // For now, we'll simulate a successful connection test
            var response = new ConnectionTestResponse(
                true,
                "Connection successful",
                DateTime.UtcNow,
                null);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing external system connection for merchant {MerchantId}", merchantId);
            return Result.Fail<ConnectionTestResponse>("Failed to test connection", "CONNECTION_TEST_ERROR");
        }
    }
    /// <summary>
    /// Otomatik senkronizasyon zamanlar (dakika aralığı ile, background job).
    /// </summary>
    public async Task<Result> ScheduleAutomaticSyncAsync(Guid merchantId, int intervalMinutes, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null)
            {
                return Result.Fail("Stock settings not found", "SETTINGS_NOT_FOUND");
            }

            settings.EnableStockSync = true;
            settings.SyncIntervalMinutes = intervalMinutes;
            settings.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<StockSettings>().Update(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // In a real implementation, you would schedule a background job here
            // For example, using Hangfire, Quartz, or Azure Functions

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling automatic sync for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to schedule automatic sync", "SCHEDULE_SYNC_ERROR");
        }
    }
    /// <summary>
    /// Otomatik senkronizasyonu iptal eder (background job cancel).
    /// </summary>
    public async Task<Result> CancelAutomaticSyncAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null)
            {
                return Result.Fail("Stock settings not found", "SETTINGS_NOT_FOUND");
            }

            settings.EnableStockSync = false;
            settings.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<StockSettings>().Update(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // In a real implementation, you would cancel the background job here

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling automatic sync for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to cancel automatic sync", "CANCEL_SYNC_ERROR");
        }
    }
    private async Task<Product?> FindProductByExternalIdAsync(Guid merchantId, string externalProductId, CancellationToken cancellationToken)
    {
        // In a real implementation, you would have a mapping table or field
        // For now, we'll try to find by name or a custom field
        return await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.MerchantId == merchantId &&
                                     p.IsActive &&
                                     (p.Name.Contains(externalProductId) ||
                                      p.ExternalId == externalProductId),
                cancellationToken: cancellationToken);
    }
    private async Task CreateStockHistoryAsync(Guid productId, Guid? productVariantId, int previousQuantity, int newQuantity,
        Domain.Enums.StockChangeType changeType, string? reason, Guid? orderId, string? referenceNumber, CancellationToken cancellationToken)
    {
        var history = new StockHistory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ProductVariantId = productVariantId,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            ChangeAmount = newQuantity - previousQuantity,
            ChangeType = changeType,
            Reason = reason,
            ChangedAt = DateTime.UtcNow,
            OrderId = orderId,
            ReferenceNumber = referenceNumber
        };

        await _unitOfWork.Repository<StockHistory>().AddAsync(history, cancellationToken);
    }
}
