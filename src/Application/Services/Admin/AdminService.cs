// System namespaces
using Microsoft.Extensions.Logging;

// Application namespaces
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;

// Domain namespaces
using Getir.Domain.Entities;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Admin;

public class AdminService : BaseService, IAdminService
{
    private readonly ISignalRService? _signalRService;
    private readonly IBackgroundTaskService _backgroundTaskService;

    public AdminService(
        IUnitOfWork unitOfWork,
        ILogger<AdminService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IBackgroundTaskService backgroundTaskService,
        ISignalRService? signalRService = null) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
        _backgroundTaskService = backgroundTaskService;
    }

    public async Task<Result<AdminDashboardResponse>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        // Get basic statistics
        var totalUsers = await _unitOfWork.ReadRepository<User>()
            .CountAsync(cancellationToken: cancellationToken);
        
        var totalMerchants = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.Role == UserRole.MerchantOwner, cancellationToken: cancellationToken);
        
        var totalCouriers = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.Role == UserRole.Courier, cancellationToken: cancellationToken);
        
        var totalOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(cancellationToken: cancellationToken);
        
        var pendingApplications = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .CountAsync(m => !m.IsApproved && m.RejectionReason == null, cancellationToken: cancellationToken);
        
        var activeOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Preparing, cancellationToken: cancellationToken);

        var stats = new AdminDashboardStats(
            totalUsers,
            totalMerchants,
            totalCouriers,
            totalOrders,
            pendingApplications,
            activeOrders,
            0m, // Total revenue placeholder
            0m, // Today revenue placeholder
            99 // System uptime placeholder
        );

        // Get recent merchant applications
        var recentApplications = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .ListAsync(
                m => !m.IsApproved && m.RejectionReason == null,
                orderBy: m => m.CreatedAt,
                ascending: false,
                take: 5,
                include: "Owner",
                cancellationToken: cancellationToken);

        var applicationResponses = recentApplications.Select(app => new RecentMerchantApplicationResponse(
            app.Id,
            "Business Name", // Placeholder
            string.Concat(app.Owner.FirstName, " ", app.Owner.LastName),
            app.Owner.Email,
            "Pending",
            app.CreatedAt,
            app.RejectionReason
        )).ToList();

        // Get system metrics
        var systemMetrics = new List<SystemMetricsResponse>
        {
            new("Active Users", totalUsers, "users", DateTime.UtcNow, "up"),
            new("System Load", 45.2m, "%", DateTime.UtcNow, "stable"),
            new("Response Time", 120m, "ms", DateTime.UtcNow, "down")
        };

        // Get notifications (placeholder)
        var notifications = new List<AdminNotificationResponse>();

        var dashboard = new AdminDashboardResponse(
            stats,
            applicationResponses,
            systemMetrics,
            notifications
        );

        return Result.Ok(dashboard);
    }

    public async Task<Result<SystemStatisticsResponse>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default)
    {
        // User Statistics
        var totalUsers = await _unitOfWork.ReadRepository<User>()
            .CountAsync(cancellationToken: cancellationToken);
        
        var activeUsers = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.IsActive, cancellationToken: cancellationToken);
        
        var newUsersThisMonth = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30), cancellationToken: cancellationToken);
        
        var newUsersToday = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-1), cancellationToken: cancellationToken);

        var userStats = new UserStatisticsResponse(
            totalUsers,
            activeUsers,
            newUsersThisMonth,
            newUsersToday,
            new List<UserGrowthDataResponse>()
        );

        // Merchant Statistics
        var totalMerchants = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.Role == UserRole.MerchantOwner, cancellationToken: cancellationToken);
        
        var activeMerchants = await _unitOfWork.ReadRepository<User>()
            .CountAsync(u => u.Role == UserRole.MerchantOwner && u.IsActive, cancellationToken: cancellationToken);
        
        var pendingApplications = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .CountAsync(m => !m.IsApproved && m.RejectionReason == null, cancellationToken: cancellationToken);
        
        var approvedThisMonth = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .CountAsync(m => m.IsApproved && m.ApprovedAt.HasValue && m.ApprovedAt.Value >= DateTime.UtcNow.AddDays(-30), cancellationToken: cancellationToken);

        var merchantStats = new MerchantStatisticsResponse(
            totalMerchants,
            activeMerchants,
            pendingApplications,
            approvedThisMonth,
            new List<MerchantGrowthDataResponse>()
        );

        // Order Statistics
        var totalOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(cancellationToken: cancellationToken);
        
        var completedOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken: cancellationToken);
        
        var cancelledOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken: cancellationToken);
        
        var pendingOrders = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Preparing, cancellationToken: cancellationToken);

        var allOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(cancellationToken: cancellationToken);
        
        var averageOrderValue = allOrders.Any() ? allOrders.Average(o => o.Total) : 0m;

        var orderStats = new AdminOrderStatisticsResponse(
            totalOrders,
            completedOrders,
            cancelledOrders,
            pendingOrders,
            averageOrderValue,
            new List<AdminOrderTrendDataResponse>()
        );

        // Revenue Statistics
        var totalRevenue = allOrders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total);
        var monthlyOrders = allOrders.Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-30) && o.Status == OrderStatus.Delivered);
        var dailyOrders = allOrders.Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-1) && o.Status == OrderStatus.Delivered);

        var revenueStats = new RevenueStatisticsResponse(
            totalRevenue,
            monthlyOrders.Sum(o => o.Total),
            dailyOrders.Sum(o => o.Total),
            averageOrderValue,
            new List<RevenueTrendDataResponse>()
        );

        // Performance Metrics
        var performanceMetrics = new PerformanceMetricsResponse(
            99.9m, 120m, 150, 45.2m, 67.8m, 25
        );

        var statistics = new SystemStatisticsResponse(
            userStats,
            merchantStats,
            orderStats,
            revenueStats,
            performanceMetrics
        );

        return Result.Ok(statistics);
    }

    public async Task<Result<PagedResult<RecentMerchantApplicationResponse>>> GetMerchantApplicationsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var applications = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .GetPagedAsync(
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: m => m.CreatedAt,
                ascending: false,
                include: "Owner",
                cancellationToken: cancellationToken);

        var totalCount = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .CountAsync(cancellationToken: cancellationToken);

        var applicationResponses = applications.Select(app => new RecentMerchantApplicationResponse(
            app.Id,
            "Business Name", // Placeholder
            string.Concat(app.Owner.FirstName, " ", app.Owner.LastName),
            app.Owner.Email,
            app.IsApproved ? "Approved" : "Pending",
            app.CreatedAt,
            app.RejectionReason
        )).ToList();

        var pagedResult = PagedResult<RecentMerchantApplicationResponse>.Create(
            applicationResponses,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<MerchantApplicationDetailsResponse>> GetMerchantApplicationDetailsAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(
                m => m.Id == applicationId,
                include: "Owner",
                cancellationToken: cancellationToken);

        if (application == null)
        {
            return Result.Fail<MerchantApplicationDetailsResponse>("Application not found", "NOT_FOUND");
        }

        var businessInfo = new MerchantBusinessInfoResponse(
            "", "", "", "", "", "", ""
        );

        var documents = new List<MerchantDocumentResponse>();

        var response = new MerchantApplicationDetailsResponse(
            application.Id,
            "Business Name", // Placeholder
            "Business Type", // Placeholder
            string.Concat(application.Owner.FirstName, " ", application.Owner.LastName),
            application.Owner.Email,
            application.Owner.PhoneNumber ?? "",
            "", // Business address placeholder
            application.IsApproved ? "Approved" : "Pending",
            application.CreatedAt,
            application.UpdatedAt,
            application.RejectionReason,
            documents,
            businessInfo
        );

        return Result.Ok(response);
    }

    public async Task<Result<MerchantApprovalResponse>> ApproveMerchantApplicationAsync(MerchantApprovalRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(m => m.Id == request.ApplicationId, cancellationToken: cancellationToken);

        if (application == null)
        {
            return Result.Fail<MerchantApprovalResponse>("Application not found", "NOT_FOUND");
        }

        application.IsApproved = true;
        application.UpdatedAt = DateTime.UtcNow;
        application.ApprovedAt = DateTime.UtcNow;
        application.RejectionReason = request.Comments;

        // Activate the merchant owner user
        application.Owner.IsActive = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "APPROVE_MERCHANT",
            "MerchantOnboarding",
            application.Id.ToString(),
            $"Approved merchant application",
            "",
            "",
            cancellationToken);

        var admin = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == adminId, cancellationToken: cancellationToken);

        var response = new MerchantApprovalResponse(
            application.Id,
            true,
            request.Comments ?? "",
            string.Concat(admin?.FirstName, " ", admin?.LastName),
            DateTime.UtcNow
        );

        return Result.Ok(response);
    }

    public async Task<Result> RejectMerchantApplicationAsync(MerchantApprovalRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(m => m.Id == request.ApplicationId, cancellationToken: cancellationToken);

        if (application == null)
        {
            return Result.Fail("Application not found", "NOT_FOUND");
        }

        application.IsApproved = false;
        application.UpdatedAt = DateTime.UtcNow;
        application.RejectionReason = request.Comments;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "REJECT_MERCHANT",
            "MerchantOnboarding",
            application.Id.ToString(),
            string.Concat("Rejected merchant application. Reason: ", request.Comments),
            "",
            "",
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<PagedResult<AdminUserResponse>>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var users = await _unitOfWork.ReadRepository<User>()
            .GetPagedAsync(
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: u => u.CreatedAt,
                ascending: false,
                cancellationToken: cancellationToken);

        var totalCount = await _unitOfWork.ReadRepository<User>()
            .CountAsync(cancellationToken: cancellationToken);

        var userResponses = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            var userOrders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(o => o.UserId == user.Id, cancellationToken: cancellationToken);

            var response = new AdminUserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber ?? "",
                user.Role.ToString(),
                user.IsActive,
                user.CreatedAt,
                user.LastLoginAt,
                userOrders.Count,
                userOrders.Sum(o => o.Total)
            );

            userResponses.Add(response);
        }

        var pagedResult = PagedResult<AdminUserResponse>.Create(
            userResponses,
            totalCount,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<AdminUserResponse>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail<AdminUserResponse>("User not found", "NOT_FOUND");
        }

        var userOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.UserId == user.Id, cancellationToken: cancellationToken);

        var response = new AdminUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber ?? "",
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            userOrders.Count,
            userOrders.Sum(o => o.Total)
        );

        return Result.Ok(response);
    }

    public async Task<Result<AdminUserResponse>> CreateUserAsync(AdminCreateUserRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

        if (existingUser != null)
        {
            return Result.Fail<AdminUserResponse>("User with this email already exists", "EMAIL_EXISTS");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            PasswordHash = "", // TODO: Hash password
            Role = Enum.Parse<UserRole>(request.Role),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "CREATE_USER",
            "User",
            user.Id.ToString(),
            string.Concat("Created user: ", user.Email, " with role: ", user.Role),
            "",
            "",
            cancellationToken);

        var response = new AdminUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber ?? "",
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            0,
            0
        );

        return Result.Ok(response);
    }

    public async Task<Result<AdminUserResponse>> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail<AdminUserResponse>("User not found", "NOT_FOUND");
        }

        var oldData = string.Concat("Email: ", user.Email, ", Role: ", user.Role, ", Active: ", user.IsActive);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.Phone;
        user.Role = Enum.Parse<UserRole>(request.Role);
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "UPDATE_USER",
            "User",
            user.Id.ToString(),
            string.Concat("Updated user: ", user.Email, ". Old: ", oldData),
            "",
            "",
            cancellationToken);

        var userOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.UserId == user.Id, cancellationToken: cancellationToken);

        var response = new AdminUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber ?? "",
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            userOrders.Count,
            userOrders.Sum(o => o.Total)
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail("User not found", "NOT_FOUND");
        }

        // Soft delete
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "DELETE_USER",
            "User",
            user.Id.ToString(),
            string.Concat("Deleted user: ", user.Email),
            "",
            "",
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ActivateUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail("User not found", "NOT_FOUND");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "ACTIVATE_USER",
            "User",
            user.Id.ToString(),
            string.Concat("Activated user: ", user.Email),
            "",
            "",
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> DeactivateUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail("User not found", "NOT_FOUND");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create audit log
        await CreateAuditLogAsync(
            adminId.ToString(),
            "DEACTIVATE_USER",
            "User",
            user.Id.ToString(),
            string.Concat("Deactivated user: ", user.Email),
            "",
            "",
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<AdminUserStatsResponse>> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail<AdminUserStatsResponse>("User not found", "NOT_FOUND");
        }

        var userOrders = await _unitOfWork.ReadRepository<Order>()
            .ListAsync(o => o.UserId == user.Id, cancellationToken: cancellationToken);

        var completedOrders = userOrders.Count(o => o.Status == OrderStatus.Delivered);
        var cancelledOrders = userOrders.Count(o => o.Status == OrderStatus.Cancelled);
        var lastOrder = userOrders.OrderByDescending(o => o.CreatedAt).FirstOrDefault();

        var stats = new AdminUserStatsResponse(
            userId,
            userOrders.Count,
            userOrders.Sum(o => o.Total),
            userOrders.Any() ? userOrders.Average(o => o.Total) : 0,
            completedOrders,
            cancelledOrders,
            lastOrder?.CreatedAt
        );

        return Result.Ok(stats);
    }

    // Placeholder implementations for remaining methods
    public async Task<Result<PagedResult<AdminUserResponse>>> GetMerchantsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await GetUsersAsync(query, cancellationToken); // Filter by role in implementation
    }

    public async Task<Result<PerformanceMetricsResponse>> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new PerformanceMetricsResponse(
            99.9m, 120m, 150, 45.2m, 67.8m, 25);
        return Result.Ok(metrics);
    }

    public async Task<Result<List<AdminNotificationResponse>>> GetSystemNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.ReadRepository<SystemNotification>()
            .ListAsync(n => n.IsActive, cancellationToken: cancellationToken);

        var responses = notifications.Select(n => new AdminNotificationResponse(
            n.Id, n.Title, n.Message, n.Type, n.CreatedAt, false)).ToList();

        return Result.Ok(responses);
    }

    public async Task<Result> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement notification read tracking
        return Result.Ok();
    }

    public async Task<Result<PagedResult<AuditLogResponse>>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.ReadRepository<AuditLog>()
            .GetPagedAsync(
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: al => al.Timestamp,
                ascending: false,
                cancellationToken: cancellationToken);

        var totalCount = await _unitOfWork.ReadRepository<AuditLog>()
            .CountAsync(cancellationToken: cancellationToken);

        var responses = logs.Select(log => new AuditLogResponse(
            log.Id, log.UserId, log.UserName, log.Action, log.EntityType,
            log.EntityId, log.Details ?? "", log.IpAddress ?? "", log.UserAgent ?? "", log.Timestamp)).ToList();

        var pagedResult = PagedResult<AuditLogResponse>.Create(responses, totalCount, query.Page, query.PageSize);
        return Result.Ok(pagedResult);
    }

    public async Task<Result<AuditLogStatsResponse>> GetAuditLogStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalLogs = await _unitOfWork.ReadRepository<AuditLog>()
            .CountAsync(cancellationToken: cancellationToken);

        var logsToday = await _unitOfWork.ReadRepository<AuditLog>()
            .CountAsync(al => al.Timestamp >= DateTime.UtcNow.AddDays(-1), cancellationToken: cancellationToken);

        var logsThisWeek = await _unitOfWork.ReadRepository<AuditLog>()
            .CountAsync(al => al.Timestamp >= DateTime.UtcNow.AddDays(-7), cancellationToken: cancellationToken);

        var logsThisMonth = await _unitOfWork.ReadRepository<AuditLog>()
            .CountAsync(al => al.Timestamp >= DateTime.UtcNow.AddDays(-30), cancellationToken: cancellationToken);

        var stats = new AuditLogStatsResponse(
            totalLogs, logsToday, logsThisWeek, logsThisMonth,
            new List<AuditLogSummaryResponse>(), new List<AuditLogSummaryResponse>());

        return Result.Ok(stats);
    }

    public async Task<Result> CreateAuditLogAsync(string userId, string action, string entityType, string entityId, string details, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken: cancellationToken);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = user != null ? string.Concat(user.FirstName, " ", user.LastName) : "System",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.Repository<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    // Placeholder implementations for remaining methods
    public async Task<Result<AdminSearchResponse>> SearchAsync(AdminSearchQuery query, CancellationToken cancellationToken = default)
    {
        return Result.Ok(new AdminSearchResponse(new List<AdminSearchResultResponse>(), 0, 1, 10, 0));
    }

    public async Task<Result<List<UserGrowthDataResponse>>> GetUserGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return Result.Ok(new List<UserGrowthDataResponse>());
    }

    public async Task<Result<List<MerchantGrowthDataResponse>>> GetMerchantGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return Result.Ok(new List<MerchantGrowthDataResponse>());
    }

    public async Task<Result<List<AdminOrderTrendDataResponse>>> GetOrderTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return Result.Ok(new List<AdminOrderTrendDataResponse>());
    }

    public async Task<Result<List<RevenueTrendDataResponse>>> GetRevenueTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return Result.Ok(new List<RevenueTrendDataResponse>());
    }

    public async Task<Result> SendSystemNotificationAsync(string title, string message, string type, List<string> targetRoles, CancellationToken cancellationToken = default)
    {
        var notification = new SystemNotification
        {
            Id = Guid.NewGuid(),
            Title = title,
            Message = message,
            Type = type,
            TargetRoles = string.Join(",", targetRoles),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Priority = 1
        };

        await _unitOfWork.Repository<SystemNotification>().AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> BroadcastSystemMessageAsync(string message, List<string> targetRoles, CancellationToken cancellationToken = default)
    {
        // TODO: Implement broadcast messaging
        return Result.Ok();
    }

    public async Task<Result> ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement cache clearing
        return Result.Ok();
    }

    public async Task<Result> BackupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement database backup
        return Result.Ok();
    }
}
