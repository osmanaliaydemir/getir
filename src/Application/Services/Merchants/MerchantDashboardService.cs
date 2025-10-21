using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Merchants;

/// <summary>
/// Merchant dashboard servisi: dashboard verileri, istatistikler, performans metrikleri.
/// </summary>
public class MerchantDashboardService : BaseService, IMerchantDashboardService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public MerchantDashboardService(IUnitOfWork unitOfWork, ILogger<MerchantDashboardService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

    /// <summary>
    /// Merchant dashboard verilerini getirir (ownership kontrolü).
    /// </summary>
    public async Task<Result<MerchantDashboardResponse>> GetDashboardAsync(Guid merchantId, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantDashboardResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        // İstatistikleri hesapla
        var stats = await GetDashboardStatsAsync(merchantId, cancellationToken);
        var recentOrders = await GetRecentOrdersAsync(merchantId, merchantOwnerId, 5, cancellationToken);
        var topProducts = await GetTopProductsAsync(merchantId, merchantOwnerId, 5, cancellationToken);
        var performance = await GetPerformanceMetricsAsync(merchantId, merchantOwnerId, null, null, cancellationToken);

        var response = new MerchantDashboardResponse(
            stats.Value!,
            recentOrders.Value!,
            topProducts.Value!,
            performance.Value!
        );

        return Result.Ok(response);
    }

    /// <summary>
    /// Son siparişleri getirir (ownership kontrolü).
    /// </summary>
    public async Task<Result<List<RecentOrderResponse>>> GetRecentOrdersAsync(Guid merchantId, Guid merchantOwnerId, int limit = ApplicationConstants.MaxRecentItems, CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<List<RecentOrderResponse>>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        var recentOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(
                o => o.MerchantId == merchantId,
                orderBy: o => o.CreatedAt,
                ascending: false,
                take: limit,
                include: "User",
                cancellationToken: cancellationToken);

        var response = recentOrders.Select(o => new RecentOrderResponse(
            o.Id,
            o.OrderNumber,
            string.Concat(o.User.FirstName, " ", o.User.LastName),
            o.Total,
            o.Status.ToStringValue(),
            o.CreatedAt
        )).ToList();

        return Result.Ok(response);
    }

    /// <summary>
    /// En çok satılan ürünleri getirir (son 30 gün, ownership kontrolü).
    /// </summary>
    public async Task<Result<List<TopProductResponse>>> GetTopProductsAsync(Guid merchantId, Guid merchantOwnerId, int limit = ApplicationConstants.MaxRecentItems, CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<List<TopProductResponse>>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        // Son 30 günlük veriler
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-ApplicationConstants.RecentDataDays);

        var topProducts = await _unitOfWork.ReadRepository<OrderLine>()
            .ListAsync(
                ol => ol.Order.MerchantId == merchantId &&
                      ol.Order.CreatedAt >= thirtyDaysAgo &&
                      ol.Order.Status != OrderStatus.Cancelled,
                include: "Order,Product",
                cancellationToken: cancellationToken);

        var productStats = topProducts
            .GroupBy(ol => ol.ProductId)
            .Select(g => new
            {
                Product = g.First().Product,
                QuantitySold = g.Sum(ol => ol.Quantity),
                Revenue = g.Sum(ol => ol.TotalPrice)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(limit)
            .ToList();

        var response = productStats.Select(p => new TopProductResponse(
            p.Product.Id,
            p.Product.Name,
            p.QuantitySold,
            p.Revenue,
            p.Product.ImageUrl ?? string.Empty
        )).ToList();

        return Result.Ok(response);
    }

    /// <summary>
    /// Performans metriklerini hesaplar (ownership kontrolü).
    /// </summary>
    public async Task<Result<MerchantPerformanceMetrics>> GetPerformanceMetricsAsync(Guid merchantId, Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantPerformanceMetrics>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        var start = startDate ?? DateTime.UtcNow.AddDays(-ApplicationConstants.RecentDataDays);
        var end = endDate ?? DateTime.UtcNow;

        var orders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(
                o => o.MerchantId == merchantId &&
                     o.CreatedAt >= start &&
                     o.CreatedAt <= end,
                cancellationToken: cancellationToken);

        if (!orders.Any())
        {
            return Result.Ok(new MerchantPerformanceMetrics(0, 0, 0, 0, 0));
        }

        // Metrikleri hesapla
        var totalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);
        var completedOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
        var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);
        var totalOrders = orders.Count;

        var averageOrderValue = completedOrders > 0 ? totalRevenue / completedOrders : 0;
        var ordersPerDay = (decimal)totalOrders / (end - start).Days;
        var completionRate = totalOrders > 0 ? (decimal)completedOrders / totalOrders * ApplicationConstants.MaxCompletionRatePercentage : 0;

        // Ortalama hazırlık süresi (şimdilik sabit değer)
        var averagePreparationTime = 25; // dakika

        // Müşteri memnuniyeti (rating'e göre)
        var customerSatisfactionScore = merchant.Rating ?? 0;

        var metrics = new MerchantPerformanceMetrics(
            averageOrderValue,
            (int)Math.Round(ordersPerDay),
            completionRate,
            averagePreparationTime,
            customerSatisfactionScore
        );

        return Result.Ok(metrics);
    }

    private async Task<Result<MerchantDashboardStats>> GetDashboardStatsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var merchant = await _unitOfWork.Repository<Merchant>()
            .GetAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantDashboardStats>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // Bugünkü siparişler
        var todayOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.MerchantId == merchantId &&
                           o.CreatedAt >= today &&
                           o.CreatedAt < tomorrow,
                       cancellationToken);

        // Bugünkü gelir
        var todayRevenue = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.MerchantId == merchantId &&
                          o.CreatedAt >= today &&
                          o.CreatedAt < tomorrow &&
                          o.Status == OrderStatus.Delivered,
                      cancellationToken: cancellationToken);

        var todayRevenueSum = todayRevenue.Sum(o => o.Total);

        // Toplam gelir
        var totalRevenueOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.MerchantId == merchantId && o.Status == OrderStatus.Delivered,
                      cancellationToken: cancellationToken);

        var totalRevenueSum = totalRevenueOrders.Sum(o => o.Total);

        // Ürün sayıları
        var totalProducts = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.MerchantId == merchantId, cancellationToken);

        var activeProducts = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.MerchantId == merchantId && p.IsActive && p.IsAvailable, cancellationToken);

        // Bekleyen siparişler
        var pendingOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.MerchantId == merchantId &&
                           (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed),
                       cancellationToken);

        // Toplam sipariş sayısı
        var totalOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.MerchantId == merchantId, cancellationToken);

        var stats = new MerchantDashboardStats(
            totalOrders,
            todayOrders,
            todayRevenueSum,
            totalRevenueSum,
            activeProducts,
            totalProducts,
            merchant.Rating ?? 0,
            merchant.TotalReviews,
            merchant.IsOpen,
            pendingOrders
        );

        return Result.Ok(stats);
    }

    /// <summary>
    /// Satış trend verilerini getirir (günlük bazda, cache'lenir 15 dk).
    /// </summary>
    public async Task<Result<List<SalesTrendDataResponse>>> GetSalesTrendDataAsync(Guid merchantId, Guid merchantOwnerId, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ownership kontrolü
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<List<SalesTrendDataResponse>>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
            }

            // Limit days
            if (days > 90) days = 90;
            if (days < 7) days = 7;

            // Check cache
            var cacheKey = $"merchant:{merchantId}:sales-trend:{days}";
            var cachedData = await _cacheService.GetAsync<List<SalesTrendDataResponse>>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                return Result.Ok(cachedData);
            }

            var fromDate = DateTime.UtcNow.Date.AddDays(-days + 1);
            var toDate = DateTime.UtcNow.Date;

            // Get delivered orders only (actual revenue)
            var orders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    o => o.MerchantId == merchantId && 
                         o.Status == OrderStatus.Delivered &&
                         o.CreatedAt >= fromDate && 
                         o.CreatedAt <= toDate.AddDays(1),
                    orderBy: o => o.CreatedAt,
                    ascending: true,
                    cancellationToken: cancellationToken);

            var salesTrend = new List<SalesTrendDataResponse>();
            var currentDate = fromDate;

            while (currentDate <= toDate)
            {
                var ordersToday = orders.Where(o => o.CreatedAt.Date == currentDate).ToList();
                
                salesTrend.Add(new SalesTrendDataResponse(
                    currentDate,
                    ordersToday.Sum(o => o.Total),
                    ordersToday.Count
                ));

                currentDate = currentDate.AddDays(1);
            }

            // Cache for 15 minutes
            await _cacheService.SetAsync(cacheKey, salesTrend, TimeSpan.FromMinutes(15), cancellationToken);

            return Result.Ok(salesTrend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales trend for merchant {MerchantId}", merchantId);
            return Result.Fail<List<SalesTrendDataResponse>>("Failed to get sales trend data", "SALES_TREND_ERROR");
        }
    }

    /// <summary>
    /// Sipariş durumu dağılımını getirir (tüm zamanlar, cache'lenir 10 dk).
    /// </summary>
    public async Task<Result<OrderStatusDistributionResponse>> GetOrderStatusDistributionAsync(Guid merchantId, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ownership kontrolü
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<OrderStatusDistributionResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
            }

            // Check cache
            var cacheKey = $"merchant:{merchantId}:order-distribution";
            var cachedData = await _cacheService.GetAsync<OrderStatusDistributionResponse>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                return Result.Ok(cachedData);
            }

            // Get all orders for this merchant
            var allOrders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    o => o.MerchantId == merchantId,
                    cancellationToken: cancellationToken);

            var distribution = new OrderStatusDistributionResponse(
                allOrders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed),
                allOrders.Count(o => o.Status == OrderStatus.Preparing),
                allOrders.Count(o => o.Status == OrderStatus.Ready),
                allOrders.Count(o => o.Status == OrderStatus.OnTheWay),
                allOrders.Count(o => o.Status == OrderStatus.Delivered),
                allOrders.Count(o => o.Status == OrderStatus.Cancelled)
            );

            // Cache for 10 minutes (order status changes frequently)
            await _cacheService.SetAsync(cacheKey, distribution, TimeSpan.FromMinutes(10), cancellationToken);

            return Result.Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order distribution for merchant {MerchantId}", merchantId);
            return Result.Fail<OrderStatusDistributionResponse>("Failed to get order distribution", "ORDER_DISTRIBUTION_ERROR");
        }
    }

    /// <summary>
    /// Kategori performansını getirir (gelir bazlı, cache'lenir 20 dk).
    /// </summary>
    public async Task<Result<List<CategoryPerformanceResponse>>> GetCategoryPerformanceAsync(Guid merchantId, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ownership kontrolü
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<List<CategoryPerformanceResponse>>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
            }

            // Check cache
            var cacheKey = $"merchant:{merchantId}:category-performance";
            var cachedData = await _cacheService.GetAsync<List<CategoryPerformanceResponse>>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                return Result.Ok(cachedData);
            }

            // Get all delivered orders with order lines and products
            var orders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    o => o.MerchantId == merchantId && o.Status == OrderStatus.Delivered,
                    include: "OrderLines.Product.ProductCategory",
                    cancellationToken: cancellationToken);

            if (!orders.Any())
            {
                return Result.Ok(new List<CategoryPerformanceResponse>());
            }

            // Group by category and calculate totals
            var categoryPerformance = orders
                .SelectMany(o => o.OrderLines)
                .Where(ol => ol.Product?.ProductCategory != null)
                .GroupBy(ol => new 
                { 
                    CategoryId = ol.Product!.ProductCategory!.Id,
                    CategoryName = ol.Product.ProductCategory.Name
                })
                .Select(g => new CategoryPerformanceResponse(
                    g.Key.CategoryId,
                    g.Key.CategoryName,
                    g.Sum(ol => ol.TotalPrice),
                    g.Select(ol => ol.OrderId).Distinct().Count(),
                    g.Select(ol => ol.ProductId).Distinct().Count()
                ))
                .OrderByDescending(cp => cp.TotalRevenue)
                .Take(10) // Top 10 categories
                .ToList();

            // Cache for 20 minutes
            await _cacheService.SetAsync(cacheKey, categoryPerformance, TimeSpan.FromMinutes(20), cancellationToken);

            return Result.Ok(categoryPerformance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category performance for merchant {MerchantId}", merchantId);
            return Result.Fail<List<CategoryPerformanceResponse>>("Failed to get category performance", "CATEGORY_PERFORMANCE_ERROR");
        }
    }
}
