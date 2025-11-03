namespace Getir.MerchantPortal.Models;

// API Response Models
public class ApiResponse<T>
{
    public bool isSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// Auth Models
public class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginResponse
{
    // API'den gelen property isimleri (camelCase)
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public int Role { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public Guid? MerchantId { get; set; } 
    
    // Backward compatibility
    public string Token => AccessToken;
    public UserInfo User => new UserInfo 
    { 
        Id = UserId, 
        Email = Email, 
        FullName = FullName, 
        Role = ((UserRole)Role).ToString() 
    };
}

public enum UserRole
{
    Customer = 1,
    Courier = 2,
    MerchantOwner = 3,
    Admin = 4
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Role { get; set; } = default!;
}

// Merchant Models
public class MerchantResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public string ServiceCategoryName { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Address { get; set; } = default!;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int AverageDeliveryTime { get; set; }
    public decimal? Rating { get; set; }
    public int TotalReviews { get; set; }
    public bool IsActive { get; set; }
    public bool IsBusy { get; set; }
    public bool IsOpen { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateMerchantRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Address { get; set; } = default!;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int AverageDeliveryTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsBusy { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
}

// Working Hours Models
public class WorkingHoursResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string DayOfWeek { get; set; } = default!;
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsClosed { get; set; }
    public bool IsOpen24Hours { get; set; }
}

public class UpdateWorkingHoursRequest
{
    public string DayOfWeek { get; set; } = default!;
    public string OpenTime { get; set; } = default!; // "09:00"
    public string CloseTime { get; set; } = default!; // "18:00"
    public bool IsClosed { get; set; }
    public bool IsOpen24Hours { get; set; }
}

public class MerchantSettingsViewModel
{
    public MerchantResponse Merchant { get; set; } = default!;
    public List<WorkingHoursResponse> WorkingHours { get; set; } = new();
}

// Dashboard Models
public class MerchantDashboardResponse
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ActiveProducts { get; set; }
    public decimal? AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal WeekRevenue { get; set; }
    public int WeekOrders { get; set; }
    public decimal MonthRevenue { get; set; }
    public int MonthOrders { get; set; }
}

public class RecentOrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class TopProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

// Product Models
public class ProductResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? ProductCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = default!;
    public string SKU { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public string? Unit { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductRequest
{
    public Guid? ProductCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Unit { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public class UpdateProductRequest
{
    public Guid? ProductCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Unit { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

// Category Models
public class ProductCategoryResponse
{
    public Guid Id { get; set; }
    public Guid? MerchantId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CategoryTreeNode
{
    public ProductCategoryResponse Category { get; set; } = default!;
    public List<CategoryTreeNode> Children { get; set; } = new();
    public int Level => GetLevel(this, 0);
    
    private int GetLevel(CategoryTreeNode node, int currentLevel)
    {
        if (node.Category.ParentCategoryId == null)
            return currentLevel;
        return currentLevel;
    }
}

public class CreateCategoryRequest
{
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryRequest
{
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

// Order Models
public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public Guid MerchantId { get; set; }
    public string Status { get; set; } = default!;
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = default!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class OrderDetailsResponse : OrderResponse
{
    public List<OrderLineResponse> OrderLines { get; set; } = new();
}

public class OrderLineResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = default!;
    public string? Notes { get; set; }
}

// Notification Preferences Models
public class MerchantNotificationPreferencesDto
{
    public bool SoundEnabled { get; set; }
    public bool DesktopNotifications { get; set; }
    public bool EmailNotifications { get; set; }
    public bool NewOrderNotifications { get; set; }
    public bool StatusChangeNotifications { get; set; }
    public bool CancellationNotifications { get; set; }
    public bool DoNotDisturbEnabled { get; set; }
    public string? DoNotDisturbStart { get; set; }
    public string? DoNotDisturbEnd { get; set; }
    public string NotificationSound { get; set; } = "default";
}

public class UpdateNotificationPreferencesDto
{
    public bool SoundEnabled { get; set; }
    public bool DesktopNotifications { get; set; }
    public bool EmailNotifications { get; set; }
    public bool NewOrderNotifications { get; set; }
    public bool StatusChangeNotifications { get; set; }
    public bool CancellationNotifications { get; set; }
    public bool DoNotDisturbEnabled { get; set; }
    public string? DoNotDisturbStart { get; set; }
    public string? DoNotDisturbEnd { get; set; }
    public string NotificationSound { get; set; } = "default";
}

// Payment Models
public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public string Status { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal? ChangeAmount { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CollectedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public string? CollectedByCourierName { get; set; }
    public string? Notes { get; set; }
    public string? FailureReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Notification Models
public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Type { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateNotificationPreferencesRequest
{
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
}

public class NotificationPreferencesResponse
{
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
}

public class MerchantCashSummaryResponse
{
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; } = default!;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal NetAmount { get; set; }
    public int TotalOrders { get; set; }
    public List<PaymentResponse> Payments { get; set; } = new();
}

public class SettlementResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public decimal Commission { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime SettlementDate { get; set; }
    public string Status { get; set; } = default!;
    public string? Notes { get; set; }
    public string? ProcessedByAdminName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? BankTransferReference { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Report Models
public class SalesDashboardModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal RevenueGrowth { get; set; }
    public decimal OrderGrowth { get; set; }
    public List<ProductPerformanceItem> TopProducts { get; set; } = new();
    public List<DailyData> RevenueByDay { get; set; } = new();
    public List<DailyData> OrdersByDay { get; set; } = new();
    public List<BreakdownItem> PaymentMethodBreakdown { get; set; } = new();
    public List<BreakdownItem> CategoryBreakdown { get; set; } = new();
}

public class RevenueAnalyticsModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<DailyData> DailyRevenue { get; set; } = new();
    public List<WeeklyData> WeeklyRevenue { get; set; } = new();
    public List<MonthlyData> MonthlyRevenue { get; set; } = new();
    public decimal RevenueTrend { get; set; }
    public List<BreakdownItem> PaymentMethodDistribution { get; set; } = new();
    public List<HourlyData> RevenueByHour { get; set; } = new();
    public List<DailyData> TopRevenueDays { get; set; } = new();
}

public class CustomerAnalyticsModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal CustomerRetentionRate { get; set; }
    public decimal AverageOrderFrequency { get; set; }
    public decimal CustomerLifetimeValue { get; set; }
    public List<CustomerItem> TopCustomers { get; set; } = new();
    public List<CustomerSegment> CustomerSegments { get; set; } = new();
    public List<CustomerGrowthData> CustomerGrowth { get; set; } = new();
    public List<CustomerLTVData> CustomerLTV { get; set; } = new();
}

public class ProductPerformanceModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalProducts { get; set; }
    public decimal TotalSales { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal SalesGrowth { get; set; }
    public List<ProductPerformanceItem> TopProducts { get; set; } = new();
    public List<ProductPerformanceItem> LowPerformanceProducts { get; set; } = new();
    public List<SalesByDayData> SalesByDay { get; set; } = new();
    public List<BreakdownItem> CategoryBreakdown { get; set; } = new();
    public List<ProductPerformanceItem> LowStockProducts { get; set; } = new();
    public List<ProductPerformanceItem> OutOfStockProducts { get; set; } = new();
    public List<ProductPerformanceItem> BestSellers { get; set; } = new();
    public List<ProductPerformanceItem> LowPerformers { get; set; } = new();
    public List<CategoryPerformance> CategoryPerformance { get; set; } = new();
    public List<ProductTrend> ProductTrends { get; set; } = new();
    public List<InventoryTurnover> InventoryTurnover { get; set; } = new();
    public List<ProfitMargin> ProfitMargins { get; set; } = new();
}

public class ChartDataModel
{
    public string ChartType { get; set; } = default!;
    public List<object> Data { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public List<string> Colors { get; set; } = new();
}

public class ReportExportRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ReportType { get; set; } = default!;
    public string Format { get; set; } = "excel";
}

// Supporting Models
public class DailyData
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public int Count { get; set; }
}

public class WeeklyData
{
    public int Week { get; set; }
    public int Year { get; set; }
    public decimal Value { get; set; }
    public int Count { get; set; }
}

public class MonthlyData
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Value { get; set; }
    public int Count { get; set; }
}

public class HourlyData
{
    public int Hour { get; set; }
    public decimal Value { get; set; }
    public int Count { get; set; }
}

public class BreakdownItem
{
    public string Label { get; set; } = default!;
    public decimal Value { get; set; }
    public decimal Percentage { get; set; }
    public string Color { get; set; } = default!;
}

public class ProductPerformanceItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public int QuantitySold { get; set; }
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
}

public class CustomerItem
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastOrderDate { get; set; }
}

public class CustomerSegment
{
    public string SegmentName { get; set; } = default!;
    public string Segment { get; set; } = default!;
    public int CustomerCount { get; set; }
    public int Count { get; set; }
    public decimal AverageValue { get; set; }
    public string Color { get; set; } = default!;
}

public class CategoryPerformance
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class ProductTrend
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal TrendValue { get; set; }
    public string TrendDirection { get; set; } = default!;
}

public class InventoryTurnover
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal TurnoverRate { get; set; }
    public int DaysInInventory { get; set; }
}

public class ProfitMargin
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Margin { get; set; }
    public decimal MarginPercentage { get; set; }
}

public class ReportDataItem
{
    public string ReportType { get; set; } = default!;
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string Details { get; set; } = default!;
}

public class PaymentStatisticsResponse
{
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int TodayPayments { get; set; }
    public int WeekPayments { get; set; }
    public int MonthPayments { get; set; }
    public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; } = new();
    public decimal PendingSettlement { get; set; }
    public decimal TotalCommission { get; set; }
}

// Stock Management Models
public class StockAlertResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public string AlertType { get; set; } = default!; // LowStock, OutOfStock, Overstock
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class StockHistoryResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int ChangeAmount { get; set; }
    public string ChangeType { get; set; } = default!; // OrderReduction, ManualAdjustment, etc.
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? OrderNumber { get; set; }
}

public class UpdateStockRequest
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int NewStockQuantity { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}

public class BulkUpdateStockRequest
{
    public List<UpdateStockRequest> StockUpdates { get; set; } = new();
    public string? Reason { get; set; }
}

public class StockSummaryResponse
{
    public int TotalProducts { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int OverstockItems { get; set; }
    public int ActiveAlerts { get; set; }
    public decimal TotalValue { get; set; }
}

// Stock Report Models (client-side equivalents for WebApi responses)
public class StockReportResponse
{
    public DateTime GeneratedAt { get; set; }
    public string ReportType { get; set; } = "CurrentStock";
    public StockSummaryResponse Summary { get; set; } = new();
    public List<StockItemReportResponse> Items { get; set; } = new();
    public List<StockMovementResponse> Movements { get; set; } = new();
    public List<StockAlertResponse> Alerts { get; set; } = new();
}


public class StockItemReportResponse
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastMovement { get; set; }
    public int MovementCount { get; set; }
    public decimal MovementValue { get; set; }
}

public class StockMovementResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
}

public class StockReportRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string ReportType { get; set; } = "CurrentStock";
    public List<Guid>? ProductIds { get; set; }
}

// Analytics Models for Dashboard Charts
public class SalesTrendDataResponse
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class OrderStatusDistributionResponse
{
    public int PendingCount { get; set; }
    public int PreparingCount { get; set; }
    public int ReadyCount { get; set; }
    public int OnWayCount { get; set; }
    public int DeliveredCount { get; set; }
    public int CancelledCount { get; set; }
}

public class CategoryPerformanceResponse
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public int ProductCount { get; set; }
}

// Product Review Models
public class ProductReviewResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public int HelpfulCount { get; set; }
    public int NotHelpfulCount { get; set; }
    public string? MerchantResponse { get; set; }
    public DateTime? MerchantRespondedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductReviewRequest
{
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public Guid? OrderId { get; set; }
}

public class UpdateProductReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ProductReviewStatsResponse
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    public int VerifiedPurchaseCount { get; set; }
    public int PendingApprovalCount { get; set; }
}

// Additional supporting models for Reports
public class CustomerGrowthData
{
    public DateTime Date { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
}

public class CustomerLTVData
{
    public DateTime Date { get; set; }
    public decimal AverageLTV { get; set; }
}

public class SalesByDayData
{
    public DateTime Date { get; set; }
    public int Sales { get; set; }
    public decimal Revenue { get; set; }
}

public class RespondToReviewRequest
{
    public string Response { get; set; } = default!;
}

// Delivery Zones
public class DeliveryZoneResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = default!;
    public string PolygonGeoJson { get; set; } = default!;
    public decimal DeliveryFee { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDeliveryZoneRequest
{
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = default!;
    public string PolygonGeoJson { get; set; } = default!;
    public decimal DeliveryFee { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateDeliveryZoneRequest : CreateDeliveryZoneRequest { }

public class CheckDeliveryZoneRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CheckDeliveryZoneResponse
{
    public bool IsInside { get; set; }
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }
}

// Delivery Optimization
public class DeliveryCapacityRequest
{
    public Guid MerchantId { get; set; }
    public Guid? DeliveryZoneId { get; set; }
    public int MaxActiveDeliveries { get; set; }
    public int MaxDailyDeliveries { get; set; }
}

public class DeliveryCapacityResponse : DeliveryCapacityRequest
{
    public Guid Id { get; set; }
    public int CurrentActiveDeliveries { get; set; }
    public int CurrentDailyDeliveries { get; set; }
}

public class DeliveryCapacityCheckRequest
{
    public Guid MerchantId { get; set; }
    public Guid? DeliveryZoneId { get; set; }
    public int RequestedDeliveries { get; set; }
}

public class DeliveryCapacityCheckResponse
{
    public bool Allowed { get; set; }
    public string? Reason { get; set; }
}

public class RouteWaypoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RouteOptimizationRequest
{
    public Guid MerchantId { get; set; }
    public List<RouteWaypoint> Waypoints { get; set; } = new();
}

public class DeliveryRouteResponse
{
    public Guid RouteId { get; set; }
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public List<RouteWaypoint> Path { get; set; } = new();
}

public class RouteOptimizationResponse
{
    public List<DeliveryRouteResponse> Routes { get; set; } = new();
}

// File Upload Models
public class FileUploadResponse
{
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public string? Container { get; set; }
    public long? Size { get; set; }
}

// Merchant Documents
public class MerchantDocumentResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string DocumentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
}

public class UploadMerchantDocumentRequest
{
    public Guid MerchantId { get; set; }
    public string DocumentType { get; set; } = default!;
    public string? Notes { get; set; }
}


// Campaign Models
public class CampaignResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

// Coupon Models
public class ValidateCouponRequest
{
    public string Code { get; set; } = default!;
    public decimal? OrderAmount { get; set; }
}

public class CouponValidationResponse
{
    public bool IsValid { get; set; }
    public string? Reason { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? DiscountType { get; set; }
}

public class CreateCouponRequest
{
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "Amount"; // Amount | Percentage
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UsageLimit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CouponResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = default!;
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
}

// Product Option Models
public class ProductOptionGroupResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
}

public class CreateProductOptionGroupRequest
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
}

public class UpdateProductOptionGroupRequest : CreateProductOptionGroupRequest { }

public class ProductOptionResponse
{
    public Guid Id { get; set; }
    public Guid ProductOptionGroupId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? ExtraPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductOptionRequest
{
    public Guid ProductOptionGroupId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? ExtraPrice { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductOptionRequest : CreateProductOptionRequest { }

public class BulkCreateProductOptionsRequest
{
    public Guid ProductId { get; set; }
    public List<CreateProductOptionGroupRequest> Groups { get; set; } = new();
    public List<CreateProductOptionRequest> Options { get; set; } = new();
}

public class BulkUpdateProductOptionsRequest
{
    public List<UpdateProductOptionGroupRequest> Groups { get; set; } = new();
    public List<UpdateProductOptionRequest> Options { get; set; } = new();
}

// Market Product Variant Models
public class MarketProductVariantResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class CreateMarketProductVariantRequest
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateMarketProductVariantRequest : CreateMarketProductVariantRequest { }

public class UpdateVariantStockRequest
{
    public Guid Id { get; set; }
    public int NewStockQuantity { get; set; }
}

// Onboarding Models
public class MerchantOnboardingResponse
{
    public Guid MerchantId { get; set; }
    public string Status { get; set; } = default!;
    public DateTime? SubmittedAt { get; set; }
}

public class OnboardingProgressResponse
{
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public decimal Progress => TotalSteps == 0 ? 0 : Math.Round((decimal)CompletedSteps / TotalSteps * 100, 2);
}

public class OnboardingStepResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsCompleted { get; set; }
}

public class CompleteOnboardingStepRequest
{
    public string Notes { get; set; } = string.Empty;
}
