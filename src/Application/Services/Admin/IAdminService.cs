using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Admin;

public interface IAdminService
{
    // Dashboard
    Task<Result<AdminDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<SystemStatisticsResponse>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
    
    // Merchant Management
    Task<Result<PagedResult<RecentMerchantApplicationResponse>>> GetMerchantApplicationsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<MerchantApplicationDetailsResponse>> GetMerchantApplicationDetailsAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<Result<MerchantApprovalResponse>> ApproveMerchantApplicationAsync(MerchantApprovalRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result> RejectMerchantApplicationAsync(MerchantApprovalRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<AdminUserResponse>>> GetMerchantsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    
    // User Management
    Task<Result<PagedResult<AdminUserResponse>>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<AdminUserResponse>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<AdminUserResponse>> CreateUserAsync(AdminCreateUserRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result<AdminUserResponse>> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result> ActivateUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result> DeactivateUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    Task<Result<AdminUserStatsResponse>> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // System Monitoring
    Task<Result<PerformanceMetricsResponse>> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default);
    Task<Result<List<AdminNotificationResponse>>> GetSystemNotificationsAsync(CancellationToken cancellationToken = default);
    Task<Result> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    // Audit Logs
    Task<Result<PagedResult<AuditLogResponse>>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task<Result<AuditLogStatsResponse>> GetAuditLogStatsAsync(CancellationToken cancellationToken = default);
    Task<Result> CreateAuditLogAsync(string userId, string action, string entityType, string entityId, string details, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    
    // Search
    Task<Result<AdminSearchResponse>> SearchAsync(AdminSearchQuery query, CancellationToken cancellationToken = default);
    
    // Reports
    Task<Result<List<UserGrowthDataResponse>>> GetUserGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<Result<List<MerchantGrowthDataResponse>>> GetMerchantGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<Result<List<AdminOrderTrendDataResponse>>> GetOrderTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<Result<List<RevenueTrendDataResponse>>> GetRevenueTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // System Operations
    Task<Result> SendSystemNotificationAsync(string title, string message, string type, List<string> targetRoles, CancellationToken cancellationToken = default);
    Task<Result> BroadcastSystemMessageAsync(string message, List<string> targetRoles, CancellationToken cancellationToken = default);
    Task<Result> ClearCacheAsync(CancellationToken cancellationToken = default);
    Task<Result> BackupDatabaseAsync(CancellationToken cancellationToken = default);
}
