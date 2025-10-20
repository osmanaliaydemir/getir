using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Stok yönetim servisi implementasyonu: sipariş bazlı otomatik stok düşürme/iade, geçmiş, uyarılar, senkronizasyon.
/// </summary>
public class StockManagementService : BaseService, IStockManagementService
{
    private new readonly ILogger<StockManagementService> _logger;
    private readonly ISignalRService? _signalRService;
    public StockManagementService(IUnitOfWork unitOfWork, ILogger<StockManagementService> logger, ILoggingService loggingService, ICacheService cacheService, ISignalRService? signalRService = null)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _logger = logger;
        _signalRService = signalRService;
    }
    /// <summary>
    /// Sipariş onaylandığında stok düşürür (transaction, stok geçmişi, uyarı kontrolü).
    /// </summary>
    public async Task<Result> ReduceStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == orderId,
                    include: "OrderLines.Product,OrderLines.ProductVariant",
                    cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail("Order not found", "ORDER_NOT_FOUND");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var orderLine in order.OrderLines)
                {
                    if (orderLine.ProductVariantId.HasValue)
                    {
                        // Update variant stock
                        var variant = await _unitOfWork.Repository<MarketProductVariant>()
                            .GetByIdAsync(orderLine.ProductVariantId.Value, cancellationToken);

                        if (variant != null)
                        {
                            var previousQuantity = variant.StockQuantity;
                            variant.StockQuantity -= orderLine.Quantity;
                            variant.IsAvailable = variant.StockQuantity > 0;
                            variant.UpdatedAt = DateTime.UtcNow;

                            _unitOfWork.Repository<MarketProductVariant>().Update(variant);

                            // Create stock history
                            await CreateStockHistoryAsync(
                                orderLine.ProductId,
                                orderLine.ProductVariantId,
                                previousQuantity,
                                variant.StockQuantity,
                                Domain.Enums.StockChangeType.Sale,
                                "Order confirmed",
                                orderId,
                                order.OrderNumber,
                                cancellationToken);
                        }
                    }
                    else
                    {
                        // Update product stock
                        var product = await _unitOfWork.Repository<Product>()
                            .GetByIdAsync(orderLine.ProductId, cancellationToken);

                        if (product != null)
                        {
                            var previousQuantity = product.StockQuantity;
                            product.StockQuantity -= orderLine.Quantity;
                            product.IsAvailable = product.StockQuantity > 0;
                            product.UpdatedAt = DateTime.UtcNow;

                            _unitOfWork.Repository<Product>().Update(product);

                            // Create stock history
                            await CreateStockHistoryAsync(
                                orderLine.ProductId,
                                null,
                                previousQuantity,
                                product.StockQuantity,
                                Domain.Enums.StockChangeType.Sale,
                                "Order confirmed",
                                orderId,
                                order.OrderNumber,
                                cancellationToken);
                        }
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);

                // Check for stock alerts
                await CheckStockLevelsAndAlertAsync(order.MerchantId, cancellationToken);

                return Result.Ok();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reducing stock for order {OrderId}", orderId);
            return Result.Fail("Failed to reduce stock for order", "STOCK_REDUCTION_ERROR");
        }
    }
    /// <summary>
    /// Sipariş iptal edildiğinde stok iade eder (transaction, stok geçmişi).
    /// </summary>
    public async Task<Result> RestoreStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(o => o.Id == orderId,
                    include: "OrderLines.Product,OrderLines.ProductVariant",
                    cancellationToken: cancellationToken);

            if (order == null)
            {
                return Result.Fail("Order not found", "ORDER_NOT_FOUND");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var orderLine in order.OrderLines)
                {
                    if (orderLine.ProductVariantId.HasValue)
                    {
                        // Restore variant stock
                        var variant = await _unitOfWork.Repository<MarketProductVariant>()
                            .GetByIdAsync(orderLine.ProductVariantId.Value, cancellationToken);

                        if (variant != null)
                        {
                            var previousQuantity = variant.StockQuantity;
                            variant.StockQuantity += orderLine.Quantity;
                            variant.IsAvailable = variant.StockQuantity > 0;
                            variant.UpdatedAt = DateTime.UtcNow;

                            _unitOfWork.Repository<MarketProductVariant>().Update(variant);

                            // Create stock history
                            await CreateStockHistoryAsync(
                                orderLine.ProductId,
                                orderLine.ProductVariantId,
                                previousQuantity,
                                variant.StockQuantity,
                                Domain.Enums.StockChangeType.Return,
                                "Order cancelled",
                                orderId,
                                order.OrderNumber,
                                cancellationToken);
                        }
                    }
                    else
                    {
                        // Restore product stock
                        var product = await _unitOfWork.Repository<Product>()
                            .GetByIdAsync(orderLine.ProductId, cancellationToken);

                        if (product != null)
                        {
                            var previousQuantity = product.StockQuantity;
                            product.StockQuantity += orderLine.Quantity;
                            product.IsAvailable = product.StockQuantity > 0;
                            product.UpdatedAt = DateTime.UtcNow;

                            _unitOfWork.Repository<Product>().Update(product);

                            // Create stock history
                            await CreateStockHistoryAsync(
                                orderLine.ProductId,
                                null,
                                previousQuantity,
                                product.StockQuantity,
                                Domain.Enums.StockChangeType.Return,
                                "Order cancelled",
                                orderId,
                                order.OrderNumber,
                                cancellationToken);
                        }
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring stock for order {OrderId}", orderId);
            return Result.Fail("Failed to restore stock for order", "STOCK_RESTORATION_ERROR");
        }
    }
    /// <summary>
    /// Stok seviyelerini kontrol eder ve uyarı oluşturur (low/out of stock, SignalR bildirim).
    /// </summary>
    public async Task<Result> CheckStockLevelsAndAlertAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.LowStockAlerts)
            {
                return Result.Ok(); // No alerts configured
            }

            // Get products with low stock
            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId && p.IsActive, cancellationToken: cancellationToken);

            var alerts = new List<StockAlert>();

            foreach (var product in products)
            {
                if (product.StockQuantity <= settings.DefaultMinimumStock)
                {
                    var alertType = product.StockQuantity == 0
                        ? Domain.Enums.StockAlertType.OutOfStock
                        : Domain.Enums.StockAlertType.LowStock;

                    var message = product.StockQuantity == 0
                        ? $"Product '{product.Name}' is out of stock"
                        : $"Product '{product.Name}' has low stock ({product.StockQuantity} remaining)";

                    // Check if alert already exists
                    var existingAlert = await _unitOfWork.ReadRepository<StockAlert>()
                        .FirstOrDefaultAsync(a => a.ProductId == product.Id &&
                                                 a.AlertType == alertType &&
                                                 !a.IsResolved, cancellationToken: cancellationToken);

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
                            AlertType = alertType,
                            Message = message,
                            CreatedAt = DateTime.UtcNow
                        };

                        alerts.Add(alert);
                        await _unitOfWork.Repository<StockAlert>().AddAsync(alert, cancellationToken);
                    }
                }
            }

            if (alerts.Any())
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Send notifications
                await SendStockAlertsAsync(merchantId, alerts, cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock levels for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to check stock levels", "STOCK_CHECK_ERROR");
        }
    }
    /// <summary>
    /// Ürün stok geçmişini getirir (tarih filtresi ile, son 100 kayıt).
    /// </summary>
    public async Task<Result<List<StockHistoryResponse>>> GetStockHistoryAsync(Guid productId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var histories = await _unitOfWork.ReadRepository<StockHistory>()
                .ListAsync(h => h.ProductId == productId &&
                               (!fromDate.HasValue || h.ChangedAt >= fromDate.Value) &&
                               (!toDate.HasValue || h.ChangedAt <= toDate.Value),
                    orderBy: h => h.ChangedAt,
                    ascending: false,
                    take: 100,
                    cancellationToken: cancellationToken);

            var responses = histories.Select(h => new StockHistoryResponse(
                h.Id,
                h.ProductId,
                h.ProductVariantId,
                h.Product.Name,
                h.ProductVariant?.Name,
                h.PreviousQuantity,
                h.NewQuantity,
                h.ChangeAmount,
                h.ChangeType,
                h.Reason,
                h.Notes,
                h.ChangedBy,
                h.ChangedByUser?.FirstName + " " + h.ChangedByUser?.LastName,
                h.ChangedAt,
                h.OrderId?.ToString(),
                h.ReferenceNumber)).ToList();

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock history for product {ProductId}", productId);
            return Result.Fail<List<StockHistoryResponse>>("Failed to get stock history", "STOCK_HISTORY_ERROR");
        }
    }
    /// <summary>
    /// Merchant stok uyarılarını getirir (aktif olanlar, ürün bilgileri dahil).
    /// </summary>
    public async Task<Result<List<StockAlertResponse>>> GetStockAlertsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _unitOfWork.ReadRepository<StockAlert>()
                .ListAsync(a => a.MerchantId == merchantId && a.IsActive,
                    orderBy: a => a.CreatedAt,
                    ascending: false,
                    cancellationToken: cancellationToken);

            var responses = new List<StockAlertResponse>();
            
            foreach (var alert in alerts)
            {
                // Get product info separately
                var product = await _unitOfWork.ReadRepository<Product>()
                    .FirstOrDefaultAsync(p => p.Id == alert.ProductId, cancellationToken: cancellationToken);
                
                string? productVariantName = null;
                if (alert.ProductVariantId.HasValue)
                {
                    var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                        .FirstOrDefaultAsync(v => v.Id == alert.ProductVariantId.Value, cancellationToken: cancellationToken);
                    productVariantName = variant?.Name;
                }
                
                responses.Add(new StockAlertResponse(
                    alert.Id,
                    alert.ProductId,
                    alert.ProductVariantId,
                    product?.Name ?? "Unknown Product",
                    productVariantName,
                    alert.CurrentStock,
                    alert.MinimumStock,
                    alert.MaximumStock,
                    alert.AlertType,
                    alert.Message,
                    alert.CreatedAt,
                    alert.IsResolved,
                    alert.ResolvedAt));
            }

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock alerts for merchant {MerchantId}", merchantId);
            return Result.Fail<List<StockAlertResponse>>("Failed to get stock alerts", "STOCK_ALERTS_ERROR");
        }
    }
    /// <summary>
    /// Stok seviyesini manuel günceller (ownership kontrolü, stok geçmişi).
    /// </summary>
    public async Task<Result> UpdateStockLevelAsync(UpdateStockRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ProductVariantId.HasValue)
            {
                var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                    .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId.Value,
                        include: "Product.Market.Merchant",
                        cancellationToken: cancellationToken);

                if (variant == null)
                {
                    return Result.Fail("Product variant not found", "VARIANT_NOT_FOUND");
                }

                if (variant.Product.Market.Merchant.OwnerId != merchantOwnerId)
                {
                    return Result.Fail("Access denied", "ACCESS_DENIED");
                }

                var previousQuantity = variant.StockQuantity;
                variant.StockQuantity = request.NewStockQuantity;
                variant.IsAvailable = variant.StockQuantity > 0;
                variant.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<MarketProductVariant>().Update(variant);

                // Create stock history
                await CreateStockHistoryAsync(
                    request.ProductId,
                    request.ProductVariantId,
                    previousQuantity,
                    request.NewStockQuantity,
                    Domain.Enums.StockChangeType.ManualAdjustment,
                    request.Reason,
                    null,
                    null,
                    cancellationToken);
            }
            else
            {
                var product = await _unitOfWork.ReadRepository<Product>()
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId,
                        include: "Merchant",
                        cancellationToken: cancellationToken);

                if (product == null)
                {
                    return Result.Fail("Product not found", "PRODUCT_NOT_FOUND");
                }

                if (product.Merchant.OwnerId != merchantOwnerId)
                {
                    return Result.Fail("Access denied", "ACCESS_DENIED");
                }

                var previousQuantity = product.StockQuantity;
                product.StockQuantity = request.NewStockQuantity;
                product.IsAvailable = product.StockQuantity > 0;
                product.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Product>().Update(product);

                // Create stock history
                await CreateStockHistoryAsync(
                    request.ProductId,
                    null,
                    previousQuantity,
                    request.NewStockQuantity,
                    Domain.Enums.StockChangeType.ManualAdjustment,
                    request.Reason,
                    null,
                    null,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock level for product {ProductId}", request.ProductId);
            return Result.Fail("Failed to update stock level", "STOCK_UPDATE_ERROR");
        }
    }
    /// <summary>
    /// Toplu stok güncelleme yapar (transaction, rollback desteği).
    /// </summary>
    public async Task<Result> BulkUpdateStockLevelsAsync(List<UpdateStockRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var request in requests)
                {
                    var result = await UpdateStockLevelAsync(request, merchantOwnerId, cancellationToken);
                    if (!result.Success)
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        return result;
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating stock levels");
            return Result.Fail("Failed to bulk update stock levels", "BULK_STOCK_UPDATE_ERROR");
        }
    }
    /// <summary>
    /// Stok raporu oluşturur (özet istatistikler, ürün bazlı detaylar).
    /// </summary>
    public async Task<Result<StockReportResponse>> GetStockReportAsync(StockReportRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // This is a simplified implementation
            // In a real scenario, you'd want to use more efficient queries
            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == request.MerchantId && p.IsActive, cancellationToken: cancellationToken);

            var summary = new StockSummaryResponse(
                products.Count,
                0, // Total variants - would need separate query
                products.Count(p => p.StockQuantity <= 10),
                products.Count(p => p.StockQuantity == 0),
                products.Count(p => p.StockQuantity > 100),
                products.Sum(p => p.StockQuantity * p.Price),
                products.Average(p => p.StockQuantity * p.Price),
                0, // Total movements - would need separate query
                0, // Stock in movements
                0  // Stock out movements
            );

            var items = products.Select(p => new StockItemReportResponse(
                p.Id,
                null,
                p.Name,
                null,
                p.ProductCategory?.Name ?? "Uncategorized",
                p.StockQuantity,
                10, // Default minimum
                1000, // Default maximum
                p.Price,
                p.StockQuantity * p.Price,
                GetStockStatus(p.StockQuantity),
                p.UpdatedAt ?? p.CreatedAt,
                0, // Movement count
                0  // Movement value
            )).ToList();

            var response = new StockReportResponse(
                DateTime.UtcNow,
                request.ReportType,
                summary,
                items,
                new List<StockMovementResponse>(), // Would need separate query
                new List<StockAlertResponse>() // Would need separate query
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating stock report");
            return Result.Fail<StockReportResponse>("Failed to generate stock report", "STOCK_REPORT_ERROR");
        }
    }
    /// <summary>
    /// Harici sistemlerle stok senkronizasyonu yapar (mock: sadece son sync zamanını günceller).
    /// </summary>
    public async Task<Result> SynchronizeStockAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _unitOfWork.ReadRepository<StockSettings>()
                .FirstOrDefaultAsync(s => s.MerchantId == merchantId && s.IsActive, cancellationToken: cancellationToken);

            if (settings == null || !settings.EnableStockSync)
            {
                return Result.Fail("Stock synchronization not enabled", "SYNC_NOT_ENABLED");
            }

            // This would integrate with external systems
            // For now, just update the last sync time
            settings.LastSyncAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<StockSettings>().Update(settings);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing stock for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to synchronize stock", "STOCK_SYNC_ERROR");
        }
    }
    
    private async Task CreateStockHistoryAsync(Guid productId, Guid? productVariantId, int previousQuantity, int newQuantity, Domain.Enums.StockChangeType changeType,
        string? reason, Guid? orderId, string? referenceNumber, CancellationToken cancellationToken)
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
   
    private async Task SendStockAlertsAsync(Guid merchantId, List<StockAlert> alerts, CancellationToken cancellationToken)
    {
        if (_signalRService == null) return;

        try
        {
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
                        { "AlertType", alert.AlertType.ToString() }
                    });

                await _signalRService.SendRealtimeNotificationAsync(notification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending stock alerts");
        }
    }
    
    private StockStatus GetStockStatus(int stockQuantity)
    {
        return stockQuantity switch
        {
            0 => StockStatus.OutOfStock,
            <= 10 => StockStatus.LowStock,
            > 100 => StockStatus.Overstock,
            _ => StockStatus.InStock
        };
    }
}
