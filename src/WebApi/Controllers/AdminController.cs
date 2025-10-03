using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Admin controller for system administration
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
    /// Get admin dashboard data
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dashboard data</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct = default)
    {
        var result = await _adminService.GetDashboardAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>System statistics</returns>
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
    /// Get merchant applications
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged merchant applications</returns>
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
    /// Get merchant application details
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Application details</returns>
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
    /// Approve merchant application
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="request">Approval request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Reject merchant application
    /// </summary>
    /// <param name="applicationId">Application ID</param>
    /// <param name="request">Rejection request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Get all merchants
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged merchants</returns>
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
    /// Get all users
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged users</returns>
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
    /// Get user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User details</returns>
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
    /// Create new user
    /// </summary>
    /// <param name="request">Create user request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created user</returns>
    [HttpPost("users")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] AdminCreateUserRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.CreateUserAsync(request, adminId, ct);
        if (result.Success)
        {
            return Created($"/api/admin/users/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update user request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated user</returns>
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
    /// Delete user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
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
    /// Activate user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("users/{userId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.ActivateUserAsync(userId, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivate user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("users/{userId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _adminService.DeactivateUserAsync(userId, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get user statistics
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User statistics</returns>
    [HttpGet("users/{userId:guid}/stats")]
    [ProducesResponseType(typeof(AdminUserStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserStats(
        [FromRoute] Guid userId,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetUserStatsAsync(userId, ct);
        return ToActionResult(result);
    }

    #endregion

    #region System Monitoring

    /// <summary>
    /// Get performance metrics
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Performance metrics</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PerformanceMetricsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics(CancellationToken ct = default)
    {
        var result = await _adminService.GetPerformanceMetricsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get system notifications
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>System notifications</returns>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(IEnumerable<AdminNotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemNotifications(CancellationToken ct = default)
    {
        var result = await _adminService.GetSystemNotificationsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Get audit logs
    /// </summary>
    /// <param name="query">Audit log query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged audit logs</returns>
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
    /// Get audit log statistics
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Audit log statistics</returns>
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
    /// Admin search
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Search results</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(AdminSearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromBody] AdminSearchQuery query,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _adminService.SearchAsync(query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Reports

    /// <summary>
    /// Get user growth data
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User growth data</returns>
    [HttpGet("reports/user-growth")]
    [ProducesResponseType(typeof(UserGrowthDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserGrowthData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetUserGrowthDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get merchant growth data
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Merchant growth data</returns>
    [HttpGet("reports/merchant-growth")]
    [ProducesResponseType(typeof(MerchantGrowthDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMerchantGrowthData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetMerchantGrowthDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get order trend data
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Order trend data</returns>
    [HttpGet("reports/order-trends")]
    [ProducesResponseType(typeof(AdminOrderTrendDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderTrendData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetOrderTrendDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get revenue trend data
    /// </summary>
    /// <param name="fromDate">From date</param>
    /// <param name="toDate">To date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Revenue trend data</returns>
    [HttpGet("reports/revenue-trends")]
    [ProducesResponseType(typeof(RevenueTrendDataResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueTrendData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken ct = default)
    {
        var result = await _adminService.GetRevenueTrendDataAsync(fromDate, toDate, ct);
        return ToActionResult(result);
    }

    #endregion

    #region System Operations

    /// <summary>
    /// Send system notification
    /// </summary>
    /// <param name="request">Send notification request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("notifications/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSystemNotification(
        [FromBody] SendSystemNotificationRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _adminService.SendSystemNotificationAsync(
            request.Title, 
            request.Message, 
            request.Type, 
            request.TargetRoles, 
            ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Broadcast system message
    /// </summary>
    /// <param name="request">Broadcast message request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("broadcast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BroadcastSystemMessage(
        [FromBody] BroadcastSystemMessageRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _adminService.BroadcastSystemMessageAsync(
            request.Message, 
            request.TargetRoles, 
            ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Clear system cache
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("cache/clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache(CancellationToken ct = default)
    {
        var result = await _adminService.ClearCacheAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Backup database
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
