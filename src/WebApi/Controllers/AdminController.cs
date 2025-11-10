using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Sistem yönetimi için admin controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Admin")]
[Authorize(Policy = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    #region Dashboard

    /// <summary>
    /// Admin dashboard verilerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Dashboard verileri</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct = default)
    {
        var result = await _adminService.GetDashboardAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sistem istatistiklerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sistem istatistikleri</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(SystemStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemStatistics(CancellationToken ct = default)
    {
        var result = await _adminService.GetSystemStatisticsAsync(ct);
        return ToActionResult(result);
    }

    #endregion

    #region Merchant Management

    /// <summary>
    /// Mağaza başvurularını getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış mağaza başvuruları</returns>
    [HttpGet("merchants/applications")]
    [ProducesResponseType(typeof(PagedResult<RecentMerchantApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantApplications(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetMerchantApplicationsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza başvuru detaylarını getir
    /// </summary>
    /// <param name="applicationId">Başvuru ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başvuru detayları</returns>
    [HttpGet("merchants/applications/{applicationId:guid}")]
    [ProducesResponseType(typeof(MerchantApplicationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantApplicationDetails(
        [FromRoute] Guid applicationId,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetMerchantApplicationDetailsAsync(applicationId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza başvurusunu onayla
    /// </summary>
    /// <param name="applicationId">Başvuru ID'si</param>
    /// <param name="request">Onay talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("merchants/applications/{applicationId:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveMerchantApplication(
        [FromRoute] Guid applicationId,
        [FromBody] MerchantApprovalRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.ApproveMerchantApplicationAsync(request, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza başvurusunu reddet
    /// </summary>
    /// <param name="applicationId">Başvuru ID'si</param>
    /// <param name="request">Red talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("merchants/applications/{applicationId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectMerchantApplication(
        [FromRoute] Guid applicationId,
        [FromBody] MerchantApprovalRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.RejectMerchantApplicationAsync(request, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Tüm mağazaları getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış mağazalar</returns>
    [HttpGet("merchants")]
    [ProducesResponseType(typeof(PagedResult<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchants(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetMerchantsAsync(query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region User Management

    /// <summary>
    /// Tüm kullanıcıları getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış kullanıcılar</returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<AdminUserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetUsersAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcı detaylarını getir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kullanıcı detayları</returns>
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetUserDetailsAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Yeni kullanıcı oluştur
    /// </summary>
    /// <param name="request">Kullanıcı oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan kullanıcı</returns>
    [HttpPost("users")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.CreateUserAsync(request, adminId, ct);
        if (result.Success)
        {
            return Created($"/api/admin/users/{result.Value!.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcıyı güncelle
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="request">Kullanıcı güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen kullanıcı</returns>
    [HttpPut("users/{userId:guid}")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] Guid userId,
        [FromBody] AdminUpdateUserRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.UpdateUserAsync(userId, request, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcıyı sil
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.DeleteUserAsync(userId, adminId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcıyı aktifleştir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("users/{userId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser([FromRoute] Guid userId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.ActivateUserAsync(userId, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcıyı deaktifleştir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("users/{userId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser([FromRoute] Guid userId, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.DeactivateUserAsync(userId, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kullanıcı istatistiklerini getir
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kullanıcı istatistikleri</returns>
    [HttpGet("users/{userId:guid}/stats")]
    [ProducesResponseType(typeof(AdminUserStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserStats([FromRoute] Guid userId, CancellationToken ct = default)
    {
        var result = await _adminService.GetUserStatsAsync(userId, ct);
        return ToActionResult(result);
    }

    #endregion

    #region System Monitoring

    /// <summary>
    /// Performans metriklerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Performans metrikleri</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PerformanceMetricsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics(CancellationToken ct = default)
    {
        var result = await _adminService.GetPerformanceMetricsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sistem bildirimlerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sistem bildirimleri</returns>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IEnumerable<AdminNotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemNotifications(CancellationToken ct = default)
    {
        var result = await _adminService.GetSystemNotificationsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretle
    /// </summary>
    /// <param name="notificationId">Bildirim ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPut("notifications/{notificationId:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNotificationAsRead(
        [FromRoute] Guid notificationId,
        CancellationToken ct = default)
    {
        var result = await _adminService.MarkNotificationAsReadAsync(notificationId, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Audit Logs

    /// <summary>
    /// Denetim kayıtlarını getir
    /// </summary>
    /// <param name="query">Denetim kayıt sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış denetim kayıtları</returns>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PagedResult<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] AuditLogQuery query,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetAuditLogsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Denetim kayıt istatistiklerini getir
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Denetim kayıt istatistikleri</returns>
    [HttpGet("audit-logs/stats")]
    [ProducesResponseType(typeof(AuditLogStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogStats(CancellationToken ct = default)
    {
        var result = await _adminService.GetAuditLogStatsAsync(ct);
        return ToActionResult(result);
    }

    #endregion

    #region Search

    /// <summary>
    /// Admin arama
    /// </summary>
    /// <param name="query">Arama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Arama sonuçları</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(AdminSearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromBody] AdminSearchQuery query, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _adminService.SearchAsync(query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Reports

    /// <summary>
    /// Kullanıcı büyüme verilerini getir
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kullanıcı büyüme verileri</returns>
    [HttpGet("reports/user-growth")]
    [ProducesResponseType(typeof(UserGrowthDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserGrowthData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct = default)
    {
        var result = await _adminService.GetUserGrowthDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza büyüme verilerini getir
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Mağaza büyüme verileri</returns>
    [HttpGet("reports/merchant-growth")]
    [ProducesResponseType(typeof(MerchantGrowthDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantGrowthData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct = default)
    {
        var result = await _adminService.GetMerchantGrowthDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş trend verilerini getir
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sipariş trend verileri</returns>
    [HttpGet("reports/order-trends")]
    [ProducesResponseType(typeof(AdminOrderTrendDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderTrendData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct = default)
    {
        var result = await _adminService.GetOrderTrendDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gelir trend verilerini getir
    /// </summary>
    /// <param name="fromDate">Başlangıç tarihi</param>
    /// <param name="toDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Gelir trend verileri</returns>
    [HttpGet("reports/revenue-trends")]
    [ProducesResponseType(typeof(RevenueTrendDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueTrendData([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct = default)
    {
        var result = await _adminService.GetRevenueTrendDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    #endregion

    #region System Operations

    /// <summary>
    /// Sistem bildirimi gönder
    /// </summary>
    /// <param name="request">Bildirim gönderme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("notifications/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSystemNotification([FromBody] SendSystemNotificationRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _adminService.SendSystemNotificationAsync(request.Title, request.Message, request.Type, request.TargetRoles, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sistem mesajı yayınla
    /// </summary>
    /// <param name="request">Mesaj yayınlama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("broadcast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BroadcastSystemMessage([FromBody] BroadcastSystemMessageRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _adminService.BroadcastSystemMessageAsync(request.Message, request.TargetRoles, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sistem önbelleğini temizle
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("cache/clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache(CancellationToken ct = default)
    {
        var result = await _adminService.ClearCacheAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Veritabanını yedekle
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("backup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> BackupDatabase(CancellationToken ct = default)
    {
        var result = await _adminService.BackupDatabaseAsync(ct);
        return ToActionResult(result);
    }

    #endregion
}

// Additional DTOs for admin operations
public record SendSystemNotificationRequest(
    string Title,
    string Message,
    string Type,
    List<string> TargetRoles);

public record BroadcastSystemMessageRequest(
    string Message,
    List<string> TargetRoles);
