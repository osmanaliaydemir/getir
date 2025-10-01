namespace Getir.Application.DTO;

public record MerchantDashboardResponse(
    MerchantDashboardStats Stats,
    List<RecentOrderResponse> RecentOrders,
    List<TopProductResponse> TopProducts,
    MerchantPerformanceMetrics Performance);

public record MerchantDashboardStats(
    int TotalOrders,
    int TodayOrders,
    decimal TodayRevenue,
    decimal TotalRevenue,
    int ActiveProducts,
    int TotalProducts,
    decimal AverageRating,
    int TotalReviews,
    bool IsOpen,
    int PendingOrders);

public record RecentOrderResponse(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    decimal Total,
    string Status,
    DateTime CreatedAt);

public record TopProductResponse(
    Guid Id,
    string Name,
    int QuantitySold,
    decimal Revenue,
    string ImageUrl);

public record MerchantPerformanceMetrics(
    decimal AverageOrderValue,
    int OrdersPerDay,
    decimal CompletionRate,
    int AveragePreparationTime,
    decimal CustomerSatisfactionScore);
