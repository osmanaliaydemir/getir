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
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
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
    public Guid MerchantId { get; set; }
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

public class StockReportRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string ReportType { get; set; } = "CurrentStock";
    public List<Guid>? ProductIds { get; set; }
}

