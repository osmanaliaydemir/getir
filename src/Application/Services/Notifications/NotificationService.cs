using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Bildirim servisi: kullanıcı bildirimlerinin CRUD işlemleri ve SignalR gerçek zamanlı bildirim.
/// </summary>
public class NotificationService : BaseService, INotificationService
{
    private readonly ISignalRService? _signalRService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService, ISignalRService? signalRService = null)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
        _backgroundTaskService = backgroundTaskService;
    }
    /// <summary>
    /// Kullanıcı bildirimlerini sayfalama ile getirir.
    /// </summary>
    public async Task<Result<PagedResult<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.Repository<Notification>().GetPagedAsync(
            filter: n => n.UserId == userId,
            orderBy: n => n.CreatedAt,
            ascending: false,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Notification>()
            .CountAsync(n => n.UserId == userId, cancellationToken);

        var response = notifications.Select(n => new NotificationResponse(
            n.Id,
            n.Title,
            n.Message,
            n.Type,
            n.RelatedEntityId,
            n.RelatedEntityType,
            n.IsRead,
            n.ImageUrl,
            n.ActionUrl,
            n.CreatedAt
        )).ToList();

        var pagedResult = PagedResult<NotificationResponse>.Create(response, total, query.Page, query.PageSize);

        return Result.Ok(pagedResult);
    }
    /// <summary>
    /// Bildirimleri okundu olarak işaretler (toplu).
    /// </summary>
    public async Task<Result> MarkAsReadAsync(Guid userId, MarkAsReadRequest request, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.Repository<Notification>().GetPagedAsync(
            filter: n => n.UserId == userId && request.NotificationIds.Contains(n.Id) && !n.IsRead,
            cancellationToken: cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Repository<Notification>().Update(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    /// <summary>
    /// Yeni bildirim oluşturur ve SignalR ile gerçek zamanlı gönderir.
    /// </summary>
    public async Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            ImageUrl = request.ImageUrl,
            ActionUrl = request.ActionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send real-time notification via SignalR
        if (_signalRService != null)
        {
            await _signalRService.SendNotificationToUserAsync(
                request.UserId,
                request.Title,
                request.Message,
                request.Type);
        }

        var response = new NotificationResponse(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.RelatedEntityId,
            notification.RelatedEntityType,
            notification.IsRead,
            notification.ImageUrl,
            notification.ActionUrl,
            notification.CreatedAt
        );

        return Result.Ok(response);
    }
    // SignalR Hub-specific methods
    /// <summary>
    /// Bildirimi okundu olarak işaretler (tekil).
    /// </summary>
    public async Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Repository<Notification>()
            .FirstOrDefaultAsync(
                n => n.Id == notificationId && n.UserId == userId,
                null,
                cancellationToken);

        if (notification == null)
        {
            return Result.Fail("Notification not found", "NOTIFICATION_NOT_FOUND");
        }

        if (notification.IsRead)
        {
            return Result.Ok(); // Already read
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _unitOfWork.Repository<Notification>().Update(notification);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _loggingService.LogBusinessEvent("NotificationMarkedAsRead", new
        {
            notificationId,
            userId
        });

        return Result.Ok();
    }
    /// <summary>
    /// Okunmamış bildirim sayısını getirir.
    /// </summary>
    public async Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.ReadRepository<Notification>()
            .CountAsync(
                n => n.UserId == userId && !n.IsRead,
                cancellationToken);

        return Result.Ok(count);
    }
    /// <summary>
    /// Kullanıcı bildirimlerini belirtilen sayıda getirir.
    /// </summary>
    public async Task<Result<List<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, int count, CancellationToken cancellationToken = default)
    {
        var notifications = await _unitOfWork.Repository<Notification>()
            .GetPagedAsync(
                filter: n => n.UserId == userId,
                orderBy: n => n.CreatedAt,
                ascending: false,
                page: 1,
                pageSize: count,
                cancellationToken: cancellationToken);

        var response = notifications.Select(n => new NotificationResponse(
            n.Id,
            n.Title,
            n.Message,
            n.Type,
            n.RelatedEntityId,
            n.RelatedEntityType,
            n.IsRead,
            n.ImageUrl,
            n.ActionUrl,
            n.CreatedAt
        )).ToList();

        return Result.Ok(response);
    }
    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretler.
    /// </summary>
    public async Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _unitOfWork.Repository<Notification>()
            .GetPagedAsync(
                filter: n => n.UserId == userId && !n.IsRead,
                cancellationToken: cancellationToken);

        if (!unreadNotifications.Any())
        {
            return Result.Ok(); // No unread notifications
        }

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _unitOfWork.Repository<Notification>().Update(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _loggingService.LogBusinessEvent("AllNotificationsMarkedAsRead", new
        {
            userId,
            count = unreadNotifications.Count
        });

        return Result.Ok();
    }
    /// <summary>
    /// Bildirimi siler.
    /// </summary>
    public async Task<Result> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Repository<Notification>()
            .FirstOrDefaultAsync(
                n => n.Id == notificationId && n.UserId == userId,
                null,
                cancellationToken);

        if (notification == null)
        {
            return Result.Fail("Notification not found", "NOTIFICATION_NOT_FOUND");
        }

        _unitOfWork.Repository<Notification>().Delete(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _loggingService.LogBusinessEvent("NotificationDeleted", new
        {
            notificationId,
            userId
        });

        return Result.Ok();
    }
    /// <summary>
    /// Sipariş durum bildirimi gönderir (SignalR ile).
    /// </summary>
    public async Task<Result> SendOrderStatusNotificationAsync(Guid userId, Guid orderId, Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default)
    {
        // Create notification for order status change
        var title = $"Order Status Update";
        var message = $"Your order status has been updated to: {status}";

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = "ORDER_UPDATE",
            RelatedEntityId = orderId,
            RelatedEntityType = "Order",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send real-time notification via SignalR
        if (_signalRService != null)
        {
            await _signalRService.SendNotificationToUserAsync(
                userId,
                title,
                message,
                "ORDER_UPDATE");
        }

        _loggingService.LogBusinessEvent("OrderStatusNotificationSent", new
        {
            userId,
            orderId,
            status = status.ToString()
        });

        return Result.Ok();
    }
}
