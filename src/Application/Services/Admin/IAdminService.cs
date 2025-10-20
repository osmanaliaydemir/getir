using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Admin;

/// <summary>
/// Admin servisi interface'i: dashboard, merchant/kullanıcı yönetimi, sistem izleme, audit log, raporlar ve sistem operasyonları.
/// </summary>
public interface IAdminService
{
    // Dashboard
    /// <summary>Admin dashboard verilerini getirir.</summary>
    Task<Result<AdminDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default);
    /// <summary>Sistem istatistiklerini getirir.</summary>
    Task<Result<SystemStatisticsResponse>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
    
    // Merchant Management
    /// <summary>Merchant başvurularını listeler.</summary>
    Task<Result<PagedResult<RecentMerchantApplicationResponse>>> GetMerchantApplicationsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Merchant başvuru detaylarını getirir.</summary>
    Task<Result<MerchantApplicationDetailsResponse>> GetMerchantApplicationDetailsAsync(Guid applicationId, CancellationToken cancellationToken = default);
    /// <summary>Merchant başvurusunu onaylar.</summary>
    Task<Result<MerchantApprovalResponse>> ApproveMerchantApplicationAsync(MerchantApprovalRequest request, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Merchant başvurusunu reddeder.</summary>
    Task<Result> RejectMerchantApplicationAsync(MerchantApprovalRequest request, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Merchant listesini getirir.</summary>
    Task<Result<PagedResult<AdminUserResponse>>> GetMerchantsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    
    // User Management
    /// <summary>Kullanıcıları listeler.</summary>
    Task<Result<PagedResult<AdminUserResponse>>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı detaylarını getirir.</summary>
    Task<Result<AdminUserResponse>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Yeni kullanıcı oluşturur.</summary>
    Task<Result<AdminUserResponse>> CreateUserAsync(AdminCreateUserRequest request, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcıyı günceller.</summary>
    Task<Result<AdminUserResponse>> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcıyı siler (soft delete).</summary>
    Task<Result> DeleteUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcıyı aktive eder.</summary>
    Task<Result> ActivateUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcıyı deaktive eder.</summary>
    Task<Result> DeactivateUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı istatistiklerini getirir.</summary>
    Task<Result<AdminUserStatsResponse>> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // System Monitoring
    /// <summary>Performans metriklerini getirir.</summary>
    Task<Result<PerformanceMetricsResponse>> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default);
    /// <summary>Sistem bildirimlerini getirir.</summary>
    Task<Result<List<AdminNotificationResponse>>> GetSystemNotificationsAsync(CancellationToken cancellationToken = default);
    /// <summary>Bildirimi okundu işaretler.</summary>
    Task<Result> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    // Audit Logs
    /// <summary>Audit logları getirir.</summary>
    Task<Result<PagedResult<AuditLogResponse>>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    /// <summary>Audit log istatistiklerini getirir.</summary>
    Task<Result<AuditLogStatsResponse>> GetAuditLogStatsAsync(CancellationToken cancellationToken = default);
    /// <summary>Audit log oluşturur.</summary>
    Task<Result> CreateAuditLogAsync(string userId, string action, string entityType, string entityId, string details, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    
    // Search
    /// <summary>Admin arama yapar.</summary>
    Task<Result<AdminSearchResponse>> SearchAsync(AdminSearchQuery query, CancellationToken cancellationToken = default);
    
    // Reports
    /// <summary>Kullanıcı büyüme verilerini getirir.</summary>
    Task<Result<List<UserGrowthDataResponse>>> GetUserGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    /// <summary>Merchant büyüme verilerini getirir.</summary>
    Task<Result<List<MerchantGrowthDataResponse>>> GetMerchantGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    /// <summary>Sipariş trend verilerini getirir.</summary>
    Task<Result<List<AdminOrderTrendDataResponse>>> GetOrderTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    /// <summary>Gelir trend verilerini getirir.</summary>
    Task<Result<List<RevenueTrendDataResponse>>> GetRevenueTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // System Operations
    /// <summary>Sistem bildirimi gönderir.</summary>
    Task<Result> SendSystemNotificationAsync(string title, string message, string type, List<string> targetRoles, CancellationToken cancellationToken = default);
    /// <summary>Sistem mesajını yayınlar.</summary>
    Task<Result> BroadcastSystemMessageAsync(string message, List<string> targetRoles, CancellationToken cancellationToken = default);
    /// <summary>Önbelleği temizler.</summary>
    Task<Result> ClearCacheAsync(CancellationToken cancellationToken = default);
    /// <summary>Veritabanı yedeği alır.</summary>
    Task<Result> BackupDatabaseAsync(CancellationToken cancellationToken = default);
}
