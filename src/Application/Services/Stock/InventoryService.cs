using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Stock;

/// <summary>
/// Inventory management service implementation
/// </summary>
public class InventoryService : BaseService, IInventoryService
{
    private new readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IUnitOfWork unitOfWork,
        ILogger<InventoryService> logger,
        ILoggingService loggingService,
        ICacheService cacheService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _logger = logger;
    }

    public async Task<Result> PerformInventoryCountAsync(
        InventoryCountRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate merchant access
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail("Merchant not found", "MERCHANT_NOT_FOUND");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var countSession = new InventoryCountSession
                {
                    Id = Guid.NewGuid(),
                    MerchantId = merchant.Id,
                    CountDate = DateTime.UtcNow,
                    CountType = request.CountType,
                    Notes = request.Notes,
                    Status = InventoryCountStatus.InProgress,
                    CreatedBy = merchantOwnerId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<InventoryCountSession>().AddAsync(countSession, cancellationToken);

                var discrepancies = new List<InventoryDiscrepancy>();

                foreach (var item in request.Items)
                {
                    // Get current stock from database
                    int currentStock = 0;
                    if (item.ProductVariantId.HasValue)
                    {
                        var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                            .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId.Value, cancellationToken: cancellationToken);
                        currentStock = variant?.StockQuantity ?? 0;
                    }
                    else
                    {
                        var product = await _unitOfWork.ReadRepository<Product>()
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken: cancellationToken);
                        currentStock = product?.StockQuantity ?? 0;
                    }

                    var countItem = new InventoryCountItem
                    {
                        Id = Guid.NewGuid(),
                        CountSessionId = countSession.Id,
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ExpectedQuantity = currentStock,
                        CountedQuantity = item.CountedQuantity,
                        Variance = item.CountedQuantity - currentStock,
                        Notes = item.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<InventoryCountItem>().AddAsync(countItem, cancellationToken);

                    // Create discrepancy if there's a variance
                    if (countItem.Variance != 0)
                    {
                        var discrepancy = new InventoryDiscrepancy
                        {
                            Id = Guid.NewGuid(),
                            CountSessionId = countSession.Id,
                            ProductId = item.ProductId,
                            ProductVariantId = item.ProductVariantId,
                            ExpectedQuantity = currentStock,
                            ActualQuantity = item.CountedQuantity,
                            Variance = countItem.Variance,
                            VariancePercentage = currentStock > 0 ? (decimal)countItem.Variance / currentStock * 100 : 0,
                            Status = InventoryDiscrepancyStatus.Pending,
                            CreatedAt = DateTime.UtcNow
                        };

                        discrepancies.Add(discrepancy);
                        await _unitOfWork.Repository<InventoryDiscrepancy>().AddAsync(discrepancy, cancellationToken);
                    }
                }

                countSession.Status = InventoryCountStatus.Completed;
                countSession.CompletedAt = DateTime.UtcNow;
                countSession.DiscrepancyCount = discrepancies.Count;

                _unitOfWork.Repository<InventoryCountSession>().Update(countSession);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
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
            _logger.LogError(ex, "Error performing inventory count for merchant {MerchantId}", merchantOwnerId);
            return Result.Fail("Failed to perform inventory count", "INVENTORY_COUNT_ERROR");
        }
    }

    public async Task<Result<List<InventoryCountResponse>>> GetInventoryCountHistoryAsync(
        Guid merchantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _unitOfWork.ReadRepository<InventoryCountSession>()
                .ListAsync(s => s.MerchantId == merchantId &&
                               (!fromDate.HasValue || s.CountDate >= fromDate.Value) &&
                               (!toDate.HasValue || s.CountDate <= toDate.Value),
                    orderBy: s => s.CountDate,
                    ascending: false,
                    include: "CreatedByUser",
                    cancellationToken: cancellationToken);

            var responses = sessions.Select(s => new InventoryCountResponse(
                s.Id,
                s.CountDate,
                s.CountType,
                s.Status,
                s.DiscrepancyCount,
                s.Notes,
                s.CreatedByUser?.FirstName + " " + s.CreatedByUser?.LastName,
                s.CreatedAt,
                s.CompletedAt)).ToList();

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory count history for merchant {MerchantId}", merchantId);
            return Result.Fail<List<InventoryCountResponse>>("Failed to get inventory count history", "INVENTORY_HISTORY_ERROR");
        }
    }

    public async Task<Result<List<InventoryLevelResponse>>> GetCurrentInventoryLevelsAsync(
        Guid merchantId,
        bool includeVariants = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId && p.IsActive,
                    include: "ProductCategory",
                    cancellationToken: cancellationToken);

            var responses = new List<InventoryLevelResponse>();

            foreach (var product in products)
            {
                responses.Add(new InventoryLevelResponse(
                    product.Id,
                    null,
                    product.Name,
                    null,
                    product.ProductCategory?.Name ?? "Uncategorized",
                    product.StockQuantity,
                    10, // Default minimum
                    1000, // Default maximum
                    product.Price,
                    product.StockQuantity * product.Price,
                    GetStockStatus(product.StockQuantity),
                    product.UpdatedAt ?? product.CreatedAt));

                if (includeVariants)
                {
                    var variants = await _unitOfWork.ReadRepository<MarketProductVariant>()
                        .ListAsync(v => v.ProductId == product.Id, cancellationToken: cancellationToken);

                    foreach (var variant in variants)
                    {
                        responses.Add(new InventoryLevelResponse(
                            product.Id,
                            variant.Id,
                            product.Name,
                            variant.Name,
                            product.ProductCategory?.Name ?? "Uncategorized",
                            variant.StockQuantity,
                            10, // Default minimum
                            1000, // Default maximum
                            variant.Price,
                            variant.StockQuantity * variant.Price,
                            GetStockStatus(variant.StockQuantity),
                            variant.UpdatedAt ?? variant.CreatedAt));
                    }
                }
            }

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current inventory levels for merchant {MerchantId}", merchantId);
            return Result.Fail<List<InventoryLevelResponse>>("Failed to get inventory levels", "INVENTORY_LEVELS_ERROR");
        }
    }

    public async Task<Result<List<InventoryDiscrepancyResponse>>> GetInventoryDiscrepanciesAsync(
        Guid merchantId,
        DateTime? fromDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var discrepancies = await _unitOfWork.ReadRepository<InventoryDiscrepancy>()
                .ListAsync(d => d.CountSession.MerchantId == merchantId &&
                               (!fromDate.HasValue || d.CreatedAt >= fromDate.Value),
                    orderBy: d => d.CreatedAt,
                    ascending: false,
                    include: "Product,ProductVariant,CountSession",
                    cancellationToken: cancellationToken);

            var responses = discrepancies.Select(d => new InventoryDiscrepancyResponse(
                d.Id,
                d.ProductId,
                d.ProductVariantId,
                d.Product.Name,
                d.ProductVariant?.Name,
                d.ExpectedQuantity,
                d.ActualQuantity,
                d.Variance,
                d.VariancePercentage,
                d.Status,
                d.ResolutionNotes,
                d.CreatedAt,
                d.ResolvedAt)).ToList();

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory discrepancies for merchant {MerchantId}", merchantId);
            return Result.Fail<List<InventoryDiscrepancyResponse>>("Failed to get inventory discrepancies", "INVENTORY_DISCREPANCY_ERROR");
        }
    }

    public async Task<Result> AdjustInventoryLevelsAsync(
        List<InventoryAdjustmentRequest> adjustments,
        Guid merchantOwnerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var adjustment in adjustments)
                {
                    if (adjustment.ProductVariantId.HasValue)
                    {
                        var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                            .FirstOrDefaultAsync(v => v.Id == adjustment.ProductVariantId.Value,
                                include: "Product.Market.Merchant",
                                cancellationToken: cancellationToken);

                        if (variant?.Product.Market.Merchant.OwnerId != merchantOwnerId)
                        {
                            continue; // Skip unauthorized adjustments
                        }

                        var previousQuantity = variant.StockQuantity;
                        variant.StockQuantity = adjustment.NewQuantity;
                        variant.IsAvailable = variant.StockQuantity > 0;
                        variant.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Repository<MarketProductVariant>().Update(variant);

                        // Create stock history
                        await CreateStockHistoryAsync(
                            adjustment.ProductId,
                            adjustment.ProductVariantId,
                            previousQuantity,
                            adjustment.NewQuantity,
                            Domain.Enums.StockChangeType.Correction,
                            reason,
                            null,
                            null,
                            cancellationToken);
                    }
                    else
                    {
                        var product = await _unitOfWork.ReadRepository<Product>()
                            .FirstOrDefaultAsync(p => p.Id == adjustment.ProductId,
                                include: "Merchant",
                                cancellationToken: cancellationToken);

                        if (product?.Merchant.OwnerId != merchantOwnerId)
                        {
                            continue; // Skip unauthorized adjustments
                        }

                        var previousQuantity = product.StockQuantity;
                        product.StockQuantity = adjustment.NewQuantity;
                        product.IsAvailable = product.StockQuantity > 0;
                        product.UpdatedAt = DateTime.UtcNow;

                        _unitOfWork.Repository<Product>().Update(product);

                        // Create stock history
                        await CreateStockHistoryAsync(
                            adjustment.ProductId,
                            null,
                            previousQuantity,
                            adjustment.NewQuantity,
                            Domain.Enums.StockChangeType.Correction,
                            reason,
                            null,
                            null,
                            cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
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
            _logger.LogError(ex, "Error adjusting inventory levels");
            return Result.Fail("Failed to adjust inventory levels", "INVENTORY_ADJUSTMENT_ERROR");
        }
    }

    public async Task<Result<InventoryTurnoverResponse>> GetInventoryTurnoverReportAsync(
        Guid merchantId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId && p.IsActive, cancellationToken: cancellationToken);

            var turnoverItems = new List<InventoryTurnoverItem>();

            foreach (var product in products)
            {
                // Get stock movements for the period
                var movements = await _unitOfWork.ReadRepository<StockHistory>()
                    .ListAsync(h => h.ProductId == product.Id &&
                                   h.ChangedAt >= fromDate &&
                                   h.ChangedAt <= toDate,
                        cancellationToken: cancellationToken);

                var stockOutMovements = movements.Where(m => m.ChangeAmount < 0).Sum(m => Math.Abs(m.ChangeAmount));
                var averageStock = product.StockQuantity; // Simplified calculation

                var turnoverRate = averageStock > 0 ? (decimal)stockOutMovements / averageStock : 0;
                var daysToTurnover = turnoverRate > 0 ? 365 / turnoverRate : 999;

                turnoverItems.Add(new InventoryTurnoverItem(
                    product.Id,
                    product.Name,
                    product.StockQuantity,
                    stockOutMovements,
                    turnoverRate,
                    daysToTurnover,
                    product.Price,
                    product.StockQuantity * product.Price));
            }

            var response = new InventoryTurnoverResponse(
                fromDate,
                toDate,
                turnoverItems.Count,
                turnoverItems.Sum(i => i.StockValue),
                turnoverItems.Average(i => i.TurnoverRate),
                turnoverItems.OrderBy(i => i.DaysToTurnover).Take(10).ToList(),
                turnoverItems.OrderByDescending(i => i.DaysToTurnover).Take(10).ToList());

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory turnover report for merchant {MerchantId}", merchantId);
            return Result.Fail<InventoryTurnoverResponse>("Failed to get inventory turnover report", "INVENTORY_TURNOVER_ERROR");
        }
    }

    public async Task<Result<List<SlowMovingInventoryResponse>>> GetSlowMovingInventoryAsync(
        Guid merchantId,
        int daysThreshold = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);

            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId && p.IsActive, cancellationToken: cancellationToken);

            var slowMovingItems = new List<SlowMovingInventoryResponse>();

            foreach (var product in products)
            {
                // Get last movement date
                var lastMovement = await _unitOfWork.ReadRepository<StockHistory>()
                    .FirstOrDefaultAsync(h => h.ProductId == product.Id, cancellationToken: cancellationToken);

                if (lastMovement == null || lastMovement.ChangedAt < cutoffDate)
                {
                    var daysSinceLastMovement = lastMovement == null 
                        ? (DateTime.UtcNow - product.CreatedAt).Days
                        : (DateTime.UtcNow - lastMovement.ChangedAt).Days;

                    slowMovingItems.Add(new SlowMovingInventoryResponse(
                        product.Id,
                        product.Name,
                        product.StockQuantity,
                        product.Price,
                        product.StockQuantity * product.Price,
                        lastMovement?.ChangedAt,
                        daysSinceLastMovement));
                }
            }

            return Result.Ok(slowMovingItems.OrderByDescending(i => i.DaysSinceLastMovement).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting slow moving inventory for merchant {MerchantId}", merchantId);
            return Result.Fail<List<SlowMovingInventoryResponse>>("Failed to get slow moving inventory", "SLOW_MOVING_INVENTORY_ERROR");
        }
    }

    public async Task<Result<InventoryValuationResponse>> GetInventoryValuationAsync(
        Guid merchantId,
        ValuationMethod method = ValuationMethod.FIFO,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(p => p.MerchantId == merchantId && p.IsActive, cancellationToken: cancellationToken);

            decimal totalValue = 0;
            var valuationItems = new List<InventoryValuationItem>();

            foreach (var product in products)
            {
                // For simplicity, using current price as valuation
                // In a real scenario, you'd implement FIFO, LIFO, or weighted average
                var itemValue = product.StockQuantity * product.Price;
                totalValue += itemValue;

                valuationItems.Add(new InventoryValuationItem(
                    product.Id,
                    product.Name,
                    product.StockQuantity,
                    product.Price,
                    itemValue));
            }

            var response = new InventoryValuationResponse(
                method,
                DateTime.UtcNow,
                totalValue,
                valuationItems.Count,
                valuationItems.OrderByDescending(i => i.TotalValue).Take(10).ToList());

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory valuation for merchant {MerchantId}", merchantId);
            return Result.Fail<InventoryValuationResponse>("Failed to get inventory valuation", "INVENTORY_VALUATION_ERROR");
        }
    }

    private async Task CreateStockHistoryAsync(
        Guid productId,
        Guid? productVariantId,
        int previousQuantity,
        int newQuantity,
        Domain.Enums.StockChangeType changeType,
        string? reason,
        Guid? orderId,
        string? referenceNumber,
        CancellationToken cancellationToken)
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
