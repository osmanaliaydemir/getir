namespace Getir.Application.Abstractions;

public interface ISignalRService
{
    Task SendNotificationToUserAsync(Guid userId, string title, string message, string type);
    Task SendOrderStatusUpdateAsync(Guid orderId, Guid userId, string status, string message);
    Task SendCourierLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude);
}
