using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Merchants;

public class MerchantDashboardService : BaseService, IMerchantDashboardService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public MerchantDashboardService(IUnitOfWork unitOfWork, ILogger<MerchantDashboardService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

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
}
