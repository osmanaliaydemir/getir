using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Notifications;

public interface INotificationService
{
    Task<Result<PagedResult<NotificationResponse>>> GetUserNotificationsAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result> MarkAsReadAsync(Guid userId, MarkAsReadRequest request, CancellationToken cancellationToken = default);
    Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
}
