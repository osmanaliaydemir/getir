using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<NotificationResponse>>> GetUserNotificationsAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
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

    public async Task<Result> MarkAsReadAsync(
        Guid userId,
        MarkAsReadRequest request,
        CancellationToken cancellationToken = default)
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

    public async Task<Result<NotificationResponse>> CreateNotificationAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
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
}
