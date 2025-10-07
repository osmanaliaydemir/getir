using Getir.Application.Common;

namespace Getir.Application.DTO;

// Admin Dashboard DTOs
public record AdminDashboardResponse(
    AdminDashboardStats Stats,
    List<RecentMerchantApplicationResponse> RecentApplications,
    List<SystemMetricsResponse> SystemMetrics,
    List<AdminNotificationResponse> Notifications);

public record AdminDashboardStats(
    int TotalUsers,
    int TotalMerchants,
    int TotalCouriers,
    int TotalOrders,
    int PendingMerchantApplications,
    int ActiveOrders,
    decimal TotalRevenue,
    decimal TodayRevenue,
    int SystemUptime);

public record RecentMerchantApplicationResponse(
    Guid Id,
    string BusinessName,
    string OwnerName,
    string OwnerEmail,
    string Status,
    DateTime SubmittedAt,
    string? RejectionReason);

public record SystemMetricsResponse(
    string MetricName,
    decimal Value,
    string Unit,
    DateTime Timestamp,
    string Trend); // "up", "down", "stable"

public record AdminNotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    DateTime CreatedAt,
    bool IsRead);

// Merchant Management DTOs
public record MerchantApprovalRequest(
    Guid ApplicationId,
    bool IsApproved,
    string? Comments = null);

public record MerchantApprovalResponse(
    Guid ApplicationId,
    bool IsApproved,
    string Comments,
    string ApprovedBy,
    DateTime ProcessedAt);

public record MerchantApplicationDetailsResponse(
    Guid Id,
    string BusinessName,
    string BusinessType,
    string OwnerName,
    string OwnerEmail,
    string Phone,
    string Address,
    string Status,
    DateTime SubmittedAt,
    DateTime? ProcessedAt,
    string? Comments,
    List<MerchantDocumentResponse> Documents,
    MerchantBusinessInfoResponse BusinessInfo);

public record MerchantBusinessInfoResponse(
    string TaxNumber,
    string BusinessRegistrationNumber,
    string BankAccount,
    string BankName,
    string BusinessAddress,
    string ContactPerson,
    string ContactPhone);

// User Management DTOs
public record AdminUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    int TotalOrders,
    decimal TotalSpent);

public record AdminCreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,
    string Role,
    Guid? MerchantId = null);

public record AdminUpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Role,
    bool IsActive,
    Guid? MerchantId = null);

public record AdminUserStatsResponse(
    Guid UserId,
    int TotalOrders,
    decimal TotalSpent,
    decimal AverageOrderValue,
    int CompletedOrders,
    int CancelledOrders,
    DateTime? LastOrderDate);

// System Statistics DTOs
public record SystemStatisticsResponse(
    UserStatisticsResponse UserStats,
    MerchantStatisticsResponse MerchantStats,
    AdminOrderStatisticsResponse OrderStats,
    RevenueStatisticsResponse RevenueStats,
    PerformanceMetricsResponse PerformanceMetrics);

public record UserStatisticsResponse(
    int TotalUsers,
    int ActiveUsers,
    int NewUsersThisMonth,
    int NewUsersToday,
    List<UserGrowthDataResponse> UserGrowthData);

public record MerchantStatisticsResponse(
    int TotalMerchants,
    int ActiveMerchants,
    int PendingApplications,
    int ApprovedThisMonth,
    List<MerchantGrowthDataResponse> MerchantGrowthData);

public record AdminOrderStatisticsResponse(
    int TotalOrders,
    int CompletedOrders,
    int CancelledOrders,
    int PendingOrders,
    decimal AverageOrderValue,
    List<AdminOrderTrendDataResponse> OrderTrendData);

public record RevenueStatisticsResponse(
    decimal TotalRevenue,
    decimal MonthlyRevenue,
    decimal DailyRevenue,
    decimal AverageOrderValue,
    List<RevenueTrendDataResponse> RevenueTrendData);

public record PerformanceMetricsResponse(
    decimal SystemUptime,
    decimal AverageResponseTime,
    int ActiveConnections,
    decimal CpuUsage,
    decimal MemoryUsage,
    int DatabaseConnections);

// Audit Log DTOs
public record AuditLogResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string Action,
    string EntityType,
    string EntityId,
    string Details,
    string IpAddress,
    string UserAgent,
    DateTime Timestamp);

public record AuditLogQuery(
    string? UserId = null,
    string? Action = null,
    string? EntityType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20);

public record AuditLogStatsResponse(
    int TotalLogs,
    int LogsToday,
    int LogsThisWeek,
    int LogsThisMonth,
    List<AuditLogSummaryResponse> ActionSummary,
    List<AuditLogSummaryResponse> UserSummary);

public record AuditLogSummaryResponse(
    string Key,
    int Count,
    decimal Percentage);

// Growth Data DTOs
public record UserGrowthDataResponse(
    DateTime Date,
    int NewUsers,
    int TotalUsers);

public record MerchantGrowthDataResponse(
    DateTime Date,
    int NewMerchants,
    int TotalMerchants);

public record AdminOrderTrendDataResponse(
    DateTime Date,
    int OrderCount,
    decimal TotalValue);

public record RevenueTrendDataResponse(
    DateTime Date,
    decimal Revenue,
    int OrderCount);

// Admin Search DTOs
public record AdminSearchQuery(
    string? SearchTerm = null,
    string? EntityType = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20);

public record AdminSearchResponse(
    List<AdminSearchResultResponse> Results,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record AdminSearchResultResponse(
    string EntityType,
    Guid EntityId,
    string Title,
    string Description,
    string Status,
    DateTime CreatedAt,
    string CreatedBy);
