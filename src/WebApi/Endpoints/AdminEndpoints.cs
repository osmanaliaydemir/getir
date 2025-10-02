using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/admin")
            .WithTags("Admin")
            .RequireAuthorization("Admin");

        // Dashboard
        group.MapGet("/dashboard", GetDashboard)
            .WithName("GetAdminDashboard")
            .WithSummary("Get admin dashboard data");

        group.MapGet("/statistics", GetSystemStatistics)
            .WithName("GetSystemStatistics")
            .WithSummary("Get system statistics");

        // Merchant Management
        group.MapGet("/merchants/applications", GetMerchantApplications)
            .WithName("GetMerchantApplications")
            .WithSummary("Get merchant applications");

        group.MapGet("/merchants/applications/{applicationId:guid}", GetMerchantApplicationDetails)
            .WithName("GetMerchantApplicationDetails")
            .WithSummary("Get merchant application details");

        group.MapPost("/merchants/applications/{applicationId:guid}/approve", ApproveMerchantApplication)
            .WithName("ApproveMerchantApplication")
            .WithSummary("Approve merchant application");

        group.MapPost("/merchants/applications/{applicationId:guid}/reject", RejectMerchantApplication)
            .WithName("RejectMerchantApplication")
            .WithSummary("Reject merchant application");

        group.MapGet("/merchants", GetMerchants)
            .WithName("GetAdminMerchants")
            .WithSummary("Get all merchants");

        // User Management
        group.MapGet("/users", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Get all users");

        group.MapGet("/users/{userId:guid}", GetUserDetails)
            .WithName("GetUserDetails")
            .WithSummary("Get user details");

        group.MapPost("/users", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Create new user");

        group.MapPut("/users/{userId:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Update user");

        group.MapDelete("/users/{userId:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Delete user");

        group.MapPost("/users/{userId:guid}/activate", ActivateUser)
            .WithName("ActivateUser")
            .WithSummary("Activate user");

        group.MapPost("/users/{userId:guid}/deactivate", DeactivateUser)
            .WithName("DeactivateUser")
            .WithSummary("Deactivate user");

        group.MapGet("/users/{userId:guid}/stats", GetUserStats)
            .WithName("GetUserStats")
            .WithSummary("Get user statistics");

        // System Monitoring
        group.MapGet("/metrics", GetPerformanceMetrics)
            .WithName("GetPerformanceMetrics")
            .WithSummary("Get performance metrics");

        group.MapGet("/notifications", GetSystemNotifications)
            .WithName("GetSystemNotifications")
            .WithSummary("Get system notifications");

        group.MapPut("/notifications/{notificationId:guid}/read", MarkNotificationAsRead)
            .WithName("MarkNotificationAsRead")
            .WithSummary("Mark notification as read");

        // Audit Logs
        group.MapGet("/audit-logs", GetAuditLogs)
            .WithName("GetAuditLogs")
            .WithSummary("Get audit logs");

        group.MapGet("/audit-logs/stats", GetAuditLogStats)
            .WithName("GetAuditLogStats")
            .WithSummary("Get audit log statistics");

        // Search
        group.MapPost("/search", Search)
            .WithName("AdminSearch")
            .WithSummary("Admin search");

        // Reports
        group.MapGet("/reports/user-growth", GetUserGrowthData)
            .WithName("GetUserGrowthData")
            .WithSummary("Get user growth data");

        group.MapGet("/reports/merchant-growth", GetMerchantGrowthData)
            .WithName("GetMerchantGrowthData")
            .WithSummary("Get merchant growth data");

        group.MapGet("/reports/order-trends", GetOrderTrendData)
            .WithName("GetOrderTrendData")
            .WithSummary("Get order trend data");

        group.MapGet("/reports/revenue-trends", GetRevenueTrendData)
            .WithName("GetRevenueTrendData")
            .WithSummary("Get revenue trend data");

        // System Operations
        group.MapPost("/notifications/send", SendSystemNotification)
            .WithName("SendSystemNotification")
            .WithSummary("Send system notification");

        group.MapPost("/broadcast", BroadcastSystemMessage)
            .WithName("BroadcastSystemMessage")
            .WithSummary("Broadcast system message");

        group.MapPost("/cache/clear", ClearCache)
            .WithName("ClearCache")
            .WithSummary("Clear system cache");

        group.MapPost("/backup", BackupDatabase)
            .WithName("BackupDatabase")
            .WithSummary("Backup database");
    }

    // Dashboard
    private static async Task<IResult> GetDashboard(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetDashboardAsync(ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetSystemStatistics(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetSystemStatisticsAsync(ct);
        return result.ToIResult();
    }

    // Merchant Management
    private static async Task<IResult> GetMerchantApplications(
        [AsParameters] PaginationQuery query,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetMerchantApplicationsAsync(query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetMerchantApplicationDetails(
        Guid applicationId,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetMerchantApplicationDetailsAsync(applicationId, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    private static async Task<IResult> ApproveMerchantApplication(
        Guid applicationId,
        [FromBody] MerchantApprovalRequest request,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.ApproveMerchantApplicationAsync(request, adminId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> RejectMerchantApplication(
        Guid applicationId,
        [FromBody] MerchantApprovalRequest request,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.RejectMerchantApplicationAsync(request, adminId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetMerchants(
        [AsParameters] PaginationQuery query,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetMerchantsAsync(query, ct);
        return result.ToIResult();
    }

    // User Management
    private static async Task<IResult> GetUsers(
        [AsParameters] PaginationQuery query,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetUsersAsync(query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetUserDetails(
        Guid userId,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetUserDetailsAsync(userId, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    private static async Task<IResult> CreateUser(
        [FromBody] AdminCreateUserRequest request,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.CreateUserAsync(request, adminId, ct);
        return result.Success ? Results.Created($"/api/admin/users/{result.Value.Id}", result.Value) : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateUser(
        Guid userId,
        [FromBody] AdminUpdateUserRequest request,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.UpdateUserAsync(userId, request, adminId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> DeleteUser(
        Guid userId,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.DeleteUserAsync(userId, adminId, ct);
        return result.Success ? Results.NoContent() : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> ActivateUser(
        Guid userId,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.ActivateUserAsync(userId, adminId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> DeactivateUser(
        Guid userId,
        [FromServices] IAdminService adminService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var adminId = GetUserId(user);
        var result = await adminService.DeactivateUserAsync(userId, adminId, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetUserStats(
        Guid userId,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetUserStatsAsync(userId, ct);
        return result.Success ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }

    // System Monitoring
    private static async Task<IResult> GetPerformanceMetrics(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetPerformanceMetricsAsync(ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetSystemNotifications(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetSystemNotificationsAsync(ct);
        return result.ToIResult();
    }

    private static async Task<IResult> MarkNotificationAsRead(
        Guid notificationId,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.MarkNotificationAsReadAsync(notificationId, ct);
        return result.ToIResult();
    }

    // Audit Logs
    private static async Task<IResult> GetAuditLogs(
        [AsParameters] AuditLogQuery query,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetAuditLogsAsync(query, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetAuditLogStats(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetAuditLogStatsAsync(ct);
        return result.ToIResult();
    }

    // Search
    private static async Task<IResult> Search(
        [FromBody] AdminSearchQuery query,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.SearchAsync(query, ct);
        return result.ToIResult();
    }

    // Reports
    private static async Task<IResult> GetUserGrowthData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetUserGrowthDataAsync(fromDate, toDate, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetMerchantGrowthData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetMerchantGrowthDataAsync(fromDate, toDate, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetOrderTrendData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetOrderTrendDataAsync(fromDate, toDate, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> GetRevenueTrendData(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.GetRevenueTrendDataAsync(fromDate, toDate, ct);
        return result.ToIResult();
    }

    // System Operations
    private static async Task<IResult> SendSystemNotification(
        [FromBody] SendSystemNotificationRequest request,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.SendSystemNotificationAsync(request.Title, request.Message, request.Type, request.TargetRoles, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> BroadcastSystemMessage(
        [FromBody] BroadcastSystemMessageRequest request,
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.BroadcastSystemMessageAsync(request.Message, request.TargetRoles, ct);
        return result.ToIResult();
    }

    private static async Task<IResult> ClearCache(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.ClearCacheAsync(ct);
        return result.ToIResult();
    }

    private static async Task<IResult> BackupDatabase(
        [FromServices] IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.BackupDatabaseAsync(ct);
        return result.ToIResult();
    }

    // Helper methods
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
    }
}

// Additional DTOs for endpoints
public record SendSystemNotificationRequest(
    string Title,
    string Message,
    string Type,
    List<string> TargetRoles);

public record BroadcastSystemMessageRequest(
    string Message,
    List<string> TargetRoles);
