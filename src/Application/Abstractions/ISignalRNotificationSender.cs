namespace Getir.Application.Abstractions;

public interface ISignalRNotificationSender
{
    Task SendToUserAsync(Guid userId, string title, string message, string type);
}

public interface ISignalROrderSender
{
    Task SendStatusUpdateAsync(Guid orderId, Guid userId, string status, string message);
}

public interface ISignalRCourierSender
{
    Task SendLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude);
}
