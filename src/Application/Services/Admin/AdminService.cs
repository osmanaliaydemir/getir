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

/// <summary>
/// Yönetim (Admin) işlemleri servisi: dashboard, istatistikler, kullanıcı/merchant yönetimi,
/// denetim kayıtları (audit), bildirimler ve sistem operasyonlarını sağlar.
/// </summary>
public class AdminService : BaseService, IAdminService
{
    private readonly ISignalRService? _signalRService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IPasswordHasher _passwordHasher;

    public AdminService(
        IUnitOfWork unitOfWork, 
        ILogger<AdminService> logger, 
        ILoggingService loggingService, 
        ICacheService cacheService, 
        IBackgroundTaskService backgroundTaskService, 
        IPasswordHasher passwordHasher,
        ISignalRService? signalRService = null)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
        _backgroundTaskService = backgroundTaskService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Admin dashboard için özet verileri (kullanıcı, merchant, kurye, sipariş vb.) getirir.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Dashboard yanıtı</returns>
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

    /// <summary>
    /// Sistem genel istatistiklerini (kullanıcı, merchant, sipariş, gelir, performans) getirir.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>İstatistikler</returns>
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

    /// <summary>
    /// Merchant başvurularını sayfalama ile listeler.
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Sayfalanmış başvurular</returns>
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

    /// <summary>
    /// Merchant başvurusunun detaylarını getirir.
    /// </summary>
    /// <param name="applicationId">Başvuru ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başvuru detayları</returns>
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

    /// <summary>
    /// Merchant başvurusunu onaylar ve audit log oluşturur.
    /// </summary>
    /// <param name="request">Onay isteği</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Onay yanıtı</returns>
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

    /// <summary>
    /// Merchant başvurusunu reddeder ve audit log oluşturur.
    /// </summary>
    /// <param name="request">Red isteği</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
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

    /// <summary>
    /// Kullanıcıları sayfalama ile listeler.
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Sayfalanmış kullanıcılar</returns>
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

    /// <summary>
    /// Kullanıcı detaylarını getirir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Kullanıcı detayı</returns>
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

    /// <summary>
    /// Yeni kullanıcı oluşturur ve audit log yazar.
    /// </summary>
    /// <param name="request">Kullanıcı oluşturma isteği</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Oluşturulan kullanıcı</returns>
    public async Task<Result<AdminUserResponse>> CreateUserAsync(AdminCreateUserRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

        if (existingUser != null)
        {
            return Result.Fail<AdminUserResponse>("User with this email already exists", "EMAIL_EXISTS");
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Fail<AdminUserResponse>("Password is required", "PASSWORD_REQUIRED");
        }

        if (request.Password.Length < 8)
        {
            return Result.Fail<AdminUserResponse>("Password must be at least 8 characters long", "PASSWORD_TOO_SHORT");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
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

    /// <summary>
    /// Kullanıcıyı günceller ve audit log yazar.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Güncelleme isteği</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Güncellenen kullanıcı</returns>
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

    /// <summary>
    /// Kullanıcıyı soft delete ile pasifleştirir ve audit log yazar.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
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

    /// <summary>
    /// Kullanıcıyı aktive eder ve audit log oluşturur.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
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

    /// <summary>
    /// Kullanıcıyı deaktive eder ve audit log oluşturur.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
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

    /// <summary>
    /// Kullanıcının sipariş ve davranış istatistiklerini getirir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Kullanıcı istatistikleri</returns>
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
    /// <summary>
    /// Merchant listesini sayfalama ile getirir (uygulamada role filtrelemesi yapılmalıdır).
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Sayfalanmış merchant listesi</returns>
    public async Task<Result<PagedResult<AdminUserResponse>>> GetMerchantsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await GetUsersAsync(query, cancellationToken); // Filter by role in implementation
    }

    /// <summary>
    /// Sistem performans metriklerini döner (ör. uptime, response time vb.).
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Performans metrikleri</returns>
    public Task<Result<PerformanceMetricsResponse>> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new PerformanceMetricsResponse(
            99.9m, 120m, 150, 45.2m, 67.8m, 25);
        return Task.FromResult(Result.Ok(metrics));
    }

    /// <summary>
    /// Sistem bildirimlerini listeler.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Bildirim listesi</returns>
    public async Task<Result<List<AdminNotificationResponse>>> GetSystemNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.ReadRepository<SystemNotification>()
            .ListAsync(n => n.IsActive, cancellationToken: cancellationToken);

        var responses = notifications.Select(n => new AdminNotificationResponse(
            n.Id, n.Title, n.Message, n.Type, n.CreatedAt, false)).ToList();

        return Result.Ok(responses);
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretler.
    /// </summary>
    /// <param name="notificationId">Bildirim ID</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
    public async Task<Result> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _unitOfWork.ReadRepository<SystemNotification>()
                .FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken: cancellationToken);

            if (notification == null)
            {
                return Result.Fail("Notification not found", "NOTIFICATION_NOT_FOUND");
            }

            // Deactivate notification to mark as read
            notification.IsActive = false;
            notification.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<SystemNotification>().Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("NotificationMarkedAsRead", new { NotificationId = notificationId });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            return Result.Fail("Failed to mark notification as read", "NOTIFICATION_READ_ERROR");
        }
    }

    /// <summary>
    /// Denetim kayıtlarını (audit logs) sayfalama ile getirir.
    /// </summary>
    /// <param name="query">Audit sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Sayfalanmış audit log</returns>
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

    /// <summary>
    /// Denetim kayıtları istatistiklerini döner.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Audit istatistikleri</returns>
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

    /// <summary>
    /// Yeni bir denetim kaydı oluşturur.
    /// </summary>
    /// <param name="userId">Kullanıcı ID (string)</param>
    /// <param name="action">Aksiyon</param>
    /// <param name="entityType">Varlık türü</param>
    /// <param name="entityId">Varlık ID</param>
    /// <param name="details">Detay</param>
    /// <param name="ipAddress">IP adresi</param>
    /// <param name="userAgent">User-Agent</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
    public async Task<Result> CreateAuditLogAsync(string userId, string action, string entityType, string entityId, string details, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken: cancellationToken);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId != null ? Guid.Parse(userId) : Guid.NewGuid(),
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
    /// <summary>
    /// Admin panelinde çok amaçlı arama: Users, Merchants, Orders arası arama yapabilir.
    /// </summary>
    /// <param name="query">Arama sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Arama sonucu</returns>
    public async Task<Result<AdminSearchResponse>> SearchAsync(AdminSearchQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query.SearchTerm) || query.SearchTerm.Length < 2)
            {
                return Result.Ok(new AdminSearchResponse(new List<AdminSearchResultResponse>(), 0, query.Page, query.PageSize, 0));
            }

            var results = new List<AdminSearchResultResponse>();
            var searchTerm = query.SearchTerm.ToLower();

            // Search Users (if EntityType is null or "User")
            if (string.IsNullOrEmpty(query.EntityType) || query.EntityType.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                var users = await _unitOfWork.ReadRepository<User>()
                    .ListAsync(
                        u => (u.FirstName.ToLower().Contains(searchTerm) || 
                              u.LastName.ToLower().Contains(searchTerm) || 
                              u.Email.ToLower().Contains(searchTerm)),
                        take: 50,
                        cancellationToken: cancellationToken);

                results.AddRange(users.Select(u => new AdminSearchResultResponse(
                    "User",
                    u.Id,
                    $"{u.FirstName} {u.LastName}",
                    u.Email,
                    u.IsActive ? "Active" : "Inactive",
                    u.CreatedAt,
                    "System"
                )));
            }

            // Search Orders (if EntityType is null or "Order")
            if (string.IsNullOrEmpty(query.EntityType) || query.EntityType.Equals("Order", StringComparison.OrdinalIgnoreCase))
            {
                var orders = await _unitOfWork.ReadRepository<Order>()
                    .ListAsync(
                        o => o.OrderNumber.ToLower().Contains(searchTerm),
                        take: 50,
                        include: "User",
                        cancellationToken: cancellationToken);

                results.AddRange(orders.Select(o => new AdminSearchResultResponse(
                    "Order",
                    o.Id,
                    $"Order #{o.OrderNumber}",
                    $"Customer: {o.User?.FirstName} {o.User?.LastName} - Total: {o.Total:C}",
                    o.Status.ToString(),
                    o.CreatedAt,
                    o.User?.Email ?? "Unknown"
                )));
            }

            // Apply date filter if provided
            if (query.FromDate.HasValue)
            {
                results = results.Where(r => r.CreatedAt >= query.FromDate.Value).ToList();
            }

            if (query.ToDate.HasValue)
            {
                results = results.Where(r => r.CreatedAt <= query.ToDate.Value).ToList();
            }

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(query.Status))
            {
                results = results.Where(r => r.Status.Equals(query.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Order by creation date (newest first)
            results = results.OrderByDescending(r => r.CreatedAt).ToList();

            // Pagination
            var totalCount = results.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
            var pagedResults = results
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            _loggingService.LogBusinessEvent("AdminSearchPerformed", new 
            { 
                query.SearchTerm, 
                query.EntityType, 
                ResultCount = totalCount 
            });

            return Result.Ok(new AdminSearchResponse(pagedResults, totalCount, query.Page, query.PageSize, totalPages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing admin search: {SearchTerm}", query.SearchTerm);
            return Result.Fail<AdminSearchResponse>("Search failed", "SEARCH_ERROR");
        }
    }

    /// <summary>
    /// Kullanıcı büyüme verilerini döner: Günlük yeni kullanıcı sayısı ve toplam kullanıcı sayısı.
    /// Dashboard'da line chart için kullanılır. Cache'lenir (30 dk TTL).
    /// </summary>
    /// <param name="fromDate">Başlangıç</param>
    /// <param name="toDate">Bitiş</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Büyüme verileri</returns>
    public async Task<Result<List<UserGrowthDataResponse>>> GetUserGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (fromDate > toDate)
            {
                return Result.Fail<List<UserGrowthDataResponse>>("FromDate cannot be greater than ToDate", "INVALID_DATE_RANGE");
            }

            // Limit date range to prevent performance issues
            var daysDiff = (toDate - fromDate).Days;
            if (daysDiff > 365)
            {
                return Result.Fail<List<UserGrowthDataResponse>>("Date range cannot exceed 365 days", "DATE_RANGE_TOO_LARGE");
            }

            // Check cache first (30 minutes TTL for analytics data)
            var cacheKey = $"admin:user-growth:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
            var cachedData = await _cacheService.GetAsync<List<UserGrowthDataResponse>>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                _logger.LogDebug("User growth data served from cache: {CacheKey}", cacheKey);
                return Result.Ok(cachedData);
            }

            // Get all users in range
            var allUsers = await _unitOfWork.ReadRepository<User>()
                .ListAsync(
                    u => u.CreatedAt >= fromDate && u.CreatedAt <= toDate.AddDays(1),
                    orderBy: u => u.CreatedAt,
                    ascending: true,
                    cancellationToken: cancellationToken);

            var growthData = new List<UserGrowthDataResponse>();
            var currentDate = fromDate.Date;
            var cumulativeTotal = await _unitOfWork.ReadRepository<User>()
                .CountAsync(u => u.CreatedAt < fromDate, cancellationToken: cancellationToken);

            // Generate daily data points
            while (currentDate <= toDate.Date)
            {
                var newUsersToday = allUsers.Count(u => u.CreatedAt.Date == currentDate);
                cumulativeTotal += newUsersToday;

                growthData.Add(new UserGrowthDataResponse(
                    currentDate,
                    newUsersToday,
                    cumulativeTotal
                ));

                currentDate = currentDate.AddDays(1);
            }

            // Cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, growthData, TimeSpan.FromMinutes(30), cancellationToken);

            return Result.Ok(growthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user growth data: {FromDate} - {ToDate}", fromDate, toDate);
            return Result.Fail<List<UserGrowthDataResponse>>("Failed to get user growth data", "USER_GROWTH_ERROR");
        }
    }

    /// <summary>
    /// Merchant büyüme verilerini döner: Günlük yeni merchant sayısı ve toplam merchant sayısı.
    /// Dashboard'da line chart için kullanılır. Cache'lenir (30 dk TTL).
    /// </summary>
    /// <param name="fromDate">Başlangıç</param>
    /// <param name="toDate">Bitiş</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Büyüme verileri</returns>
    public async Task<Result<List<MerchantGrowthDataResponse>>> GetMerchantGrowthDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (fromDate > toDate)
            {
                return Result.Fail<List<MerchantGrowthDataResponse>>("FromDate cannot be greater than ToDate", "INVALID_DATE_RANGE");
            }

            // Limit date range to prevent performance issues
            if ((toDate - fromDate).Days > 365)
            {
                return Result.Fail<List<MerchantGrowthDataResponse>>("Date range cannot exceed 365 days", "DATE_RANGE_TOO_LARGE");
            }

            // Check cache first
            var cacheKey = $"admin:merchant-growth:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
            var cachedData = await _cacheService.GetAsync<List<MerchantGrowthDataResponse>>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                return Result.Ok(cachedData);
            }

            // Get all merchant owners
            var allMerchants = await _unitOfWork.ReadRepository<User>()
                .ListAsync(
                    u => u.Role == UserRole.MerchantOwner && 
                         u.CreatedAt >= fromDate && 
                         u.CreatedAt <= toDate.AddDays(1),
                    orderBy: u => u.CreatedAt,
                    ascending: true,
                    cancellationToken: cancellationToken);

            var growthData = new List<MerchantGrowthDataResponse>();
            var currentDate = fromDate.Date;
            var cumulativeTotal = await _unitOfWork.ReadRepository<User>()
                .CountAsync(u => u.Role == UserRole.MerchantOwner && u.CreatedAt < fromDate, cancellationToken: cancellationToken);

            // Generate daily data points
            while (currentDate <= toDate.Date)
            {
                var newMerchantsToday = allMerchants.Count(m => m.CreatedAt.Date == currentDate);
                cumulativeTotal += newMerchantsToday;

                growthData.Add(new MerchantGrowthDataResponse(
                    currentDate,
                    newMerchantsToday,
                    cumulativeTotal
                ));

                currentDate = currentDate.AddDays(1);
            }

            // Cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, growthData, TimeSpan.FromMinutes(30), cancellationToken);

            return Result.Ok(growthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant growth data: {FromDate} - {ToDate}", fromDate, toDate);
            return Result.Fail<List<MerchantGrowthDataResponse>>("Failed to get merchant growth data", "MERCHANT_GROWTH_ERROR");
        }
    }

    /// <summary>
    /// Sipariş trend verilerini döner: Günlük sipariş sayısı ve toplam değer.
    /// Dashboard'da bar/line chart için kullanılır. Cache'lenir (15 dk TTL - daha sık güncellenmeli).
    /// </summary>
    /// <param name="fromDate">Başlangıç</param>
    /// <param name="toDate">Bitiş</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Trend verileri</returns>
    public async Task<Result<List<AdminOrderTrendDataResponse>>> GetOrderTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (fromDate > toDate)
            {
                return Result.Fail<List<AdminOrderTrendDataResponse>>("FromDate cannot be greater than ToDate", "INVALID_DATE_RANGE");
            }

            // Limit to 180 days (orders change frequently)
            if ((toDate - fromDate).Days > 180)
            {
                return Result.Fail<List<AdminOrderTrendDataResponse>>("Date range cannot exceed 180 days", "DATE_RANGE_TOO_LARGE");
            }

            // Check cache (15 minutes for order data - more dynamic)
            var cacheKey = $"admin:order-trend:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
            var cachedData = await _cacheService.GetAsync<List<AdminOrderTrendDataResponse>>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                return Result.Ok(cachedData);
            }

            // Get all orders in date range
            var allOrders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate.AddDays(1),
                    orderBy: o => o.CreatedAt,
                    ascending: true,
                    cancellationToken: cancellationToken);

            var trendData = new List<AdminOrderTrendDataResponse>();
            var currentDate = fromDate.Date;

            // Generate daily data points
            while (currentDate <= toDate.Date)
            {
                var ordersToday = allOrders.Where(o => o.CreatedAt.Date == currentDate).ToList();
                var orderCount = ordersToday.Count;
                var totalValue = ordersToday.Sum(o => o.Total);

                trendData.Add(new AdminOrderTrendDataResponse(
                    currentDate,
                    orderCount,
                    totalValue
                ));

                currentDate = currentDate.AddDays(1);
            }

            // Cache for 15 minutes (order data changes frequently)
            await _cacheService.SetAsync(cacheKey, trendData, TimeSpan.FromMinutes(15), cancellationToken);

            return Result.Ok(trendData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order trend data: {FromDate} - {ToDate}", fromDate, toDate);
            return Result.Fail<List<AdminOrderTrendDataResponse>>("Failed to get order trend data", "ORDER_TREND_ERROR");
        }
    }

    /// <summary>
    /// Gelir trend verilerini döner: Günlük gelir (sadece tamamlanan siparişler) ve sipariş sayısı.
    /// Dashboard'da revenue line chart için kullanılır. Cache'lenir (20 dk TTL).
    /// </summary>
    /// <param name="fromDate">Başlangıç</param>
    /// <param name="toDate">Bitiş</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Trend verileri</returns>
    public async Task<Result<List<RevenueTrendDataResponse>>> GetRevenueTrendDataAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (fromDate > toDate)
            {
                return Result.Fail<List<RevenueTrendDataResponse>>("FromDate cannot be greater than ToDate", "INVALID_DATE_RANGE");
            }

            // Limit to 180 days (revenue data is critical but static after delivery)
            if ((toDate - fromDate).Days > 180)
            {
                return Result.Fail<List<RevenueTrendDataResponse>>("Date range cannot exceed 180 days", "DATE_RANGE_TOO_LARGE");
            }

            // Check cache (20 minutes for revenue data)
            var cacheKey = $"admin:revenue-trend:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";
            var cachedData = await _cacheService.GetAsync<List<RevenueTrendDataResponse>>(cacheKey, cancellationToken);
            
            if (cachedData != null)
            {
                return Result.Ok(cachedData);
            }

            // Get only delivered orders (actual revenue)
            var completedOrders = await _unitOfWork.ReadRepository<Order>()
                .ListAsync(
                    o => o.Status == OrderStatus.Delivered &&
                         o.CreatedAt >= fromDate && 
                         o.CreatedAt <= toDate.AddDays(1),
                    orderBy: o => o.CreatedAt,
                    ascending: true,
                    cancellationToken: cancellationToken);

            var revenueTrendData = new List<RevenueTrendDataResponse>();
            var currentDate = fromDate.Date;

            // Generate daily data points
            while (currentDate <= toDate.Date)
            {
                var ordersToday = completedOrders.Where(o => o.CreatedAt.Date == currentDate).ToList();
                var orderCount = ordersToday.Count;
                var revenue = ordersToday.Sum(o => o.Total);

                revenueTrendData.Add(new RevenueTrendDataResponse(
                    currentDate,
                    revenue,
                    orderCount
                ));

                currentDate = currentDate.AddDays(1);
            }

            _loggingService.LogBusinessEvent("RevenueTrendDataGenerated", new 
            { 
                FromDate = fromDate,
                ToDate = toDate,
                TotalRevenue = revenueTrendData.Sum(r => r.Revenue),
                TotalOrders = revenueTrendData.Sum(r => r.OrderCount)
            });

            // Cache for 20 minutes
            await _cacheService.SetAsync(cacheKey, revenueTrendData, TimeSpan.FromMinutes(20), cancellationToken);

            return Result.Ok(revenueTrendData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue trend data: {FromDate} - {ToDate}", fromDate, toDate);
            return Result.Fail<List<RevenueTrendDataResponse>>("Failed to get revenue trend data", "REVENUE_TREND_ERROR");
        }
    }

    /// <summary>
    /// Sistem bildirimi gönderir (kalıcı kayıt oluşturur).
    /// </summary>
    /// <param name="title">Başlık</param>
    /// <param name="message">Mesaj</param>
    /// <param name="type">Tür</param>
    /// <param name="targetRoles">Hedef roller</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
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

    /// <summary>
    /// Sistem mesajını tüm kullanıcılara veya belirli rollere yayınlar.
    /// </summary>
    /// <param name="message">Mesaj</param>
    /// <param name="targetRoles">Hedef roller (boş ise herkese gönderilir)</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
    public async Task<Result> BroadcastSystemMessageAsync(string message, List<string> targetRoles, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Result.Fail("Message cannot be empty", "EMPTY_MESSAGE");
            }

            // Create system notification record for persistence
            var notification = new SystemNotification
            {
                Id = Guid.NewGuid(),
                Title = "System Announcement",
                Message = message,
                Type = "Broadcast",
                TargetRoles = targetRoles?.Any() == true ? string.Join(",", targetRoles) : "All",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Priority = 2
            };

            await _unitOfWork.Repository<SystemNotification>().AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send via SignalR if available
            if (_signalRService != null)
            {
                var notificationEvent = new RealtimeNotificationEvent(
                    notification.Id,
                    notification.Title,
                    message,
                    "system_broadcast",
                    DateTime.UtcNow,
                    false, // IsRead
                    new Dictionary<string, object>
                    {
                        ["Priority"] = notification.Priority,
                        ["ActionUrl"] = notification.ActionUrl ?? "",
                        ["ActionText"] = notification.ActionText ?? "",
                        ["TargetRoles"] = targetRoles ?? new List<string>()
                    });

                if (targetRoles?.Any() == true)
                {
                    // Send to specific roles
                    foreach (var role in targetRoles)
                    {
                        await _signalRService.SendNotificationToRoleAsync(role, notificationEvent);
                    }
                }
                else
                {
                    // Send to all users (broadcast to all groups)
                    // Using existing BroadcastOrderStatusChangeAsync as fallback for broadcast
                    await _signalRService.SendDashboardUpdateAsync("SystemBroadcast", notificationEvent);
                }
            }

            _loggingService.LogBusinessEvent("SystemMessageBroadcasted", new 
            { 
                Message = message, 
                TargetRoles = targetRoles, 
                NotificationId = notification.Id 
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting system message: {Message}", message);
            return Result.Fail("Failed to broadcast system message", "BROADCAST_ERROR");
        }
    }

    /// <summary>
    /// Sistem önbelleğini tamamen temizler.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
    public async Task<Result> ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Admin initiated cache clear operation");

            // Clear all cache entries
            await _cacheService.ClearAsync(cancellationToken);

            _loggingService.LogBusinessEvent("CacheCleared", new 
            { 
                Timestamp = DateTime.UtcNow,
                InitiatedBy = "Admin"
            });

            _logger.LogInformation("Cache cleared successfully");

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Result.Fail("Failed to clear cache", "CACHE_CLEAR_ERROR");
        }
    }

    /// <summary>
    /// Veritabanı yedeği alır (background task olarak).
    /// NOT: Gerçek backup işlemi Infrastructure layer'da background service tarafından yapılır.
    /// Bu metod backup işini queue'ya ekler.
    /// </summary>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
    public async Task<Result> BackupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Admin initiated database backup operation");

            // Create backup job parameters
            var backupJobId = Guid.NewGuid();
            var backupFileName = $"Getir_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";

            // Queue database backup as background task
            // The actual backup implementation should be in Infrastructure layer
            // using SQL Server backup commands or Azure SQL Database backup APIs
            var backupTask = new
            {
                JobId = backupJobId,
                FileName = backupFileName,
                TaskType = "DatabaseBackup",
                Timestamp = DateTime.UtcNow
            };

            await _backgroundTaskService.EnqueueTaskAsync(backupTask, TaskPriority.High, cancellationToken);
            
            _logger.LogInformation("Database backup task queued: {BackupJobId}, {BackupFileName}", backupJobId, backupFileName);

            _loggingService.LogBusinessEvent("DatabaseBackupQueued", new 
            { 
                BackupJobId = backupJobId,
                BackupFileName = backupFileName,
                Timestamp = DateTime.UtcNow,
                InitiatedBy = "Admin"
            });

            _logger.LogInformation("Database backup queued with job ID: {BackupJobId}", backupJobId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing database backup");
            return Result.Fail("Failed to queue database backup", "DATABASE_BACKUP_ERROR");
        }
    }
}
