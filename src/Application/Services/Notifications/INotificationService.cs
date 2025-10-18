using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Notifications;

public interface INotificationService
{
    Task<Result<PagedResult<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result> MarkAsReadAsync(Guid userId, MarkAsReadRequest request, CancellationToken cancellationToken = default);
    Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    
    // SignalR Hub methods
    Task<Result> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<int>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<List<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, int count, CancellationToken cancellationToken = default);
    Task<Result> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result> SendOrderStatusNotificationAsync(Guid userId, Guid orderId, Domain.Enums.OrderStatus status, CancellationToken cancellationToken = default);
}
