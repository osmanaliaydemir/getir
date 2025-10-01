using Getir.Application.Abstractions;

namespace Getir.Infrastructure.SignalR;

public class SignalRService : ISignalRService
{
    private readonly ISignalRNotificationSender _notificationSender;
    private readonly ISignalROrderSender _orderSender;
    private readonly ISignalRCourierSender _courierSender;

    public SignalRService(
        ISignalRNotificationSender notificationSender,
        ISignalROrderSender orderSender,
        ISignalRCourierSender courierSender)
    {
        _notificationSender = notificationSender;
        _orderSender = orderSender;
        _courierSender = courierSender;
    }

    public async Task SendNotificationToUserAsync(Guid userId, string title, string message, string type)
    {
        await _notificationSender.SendToUserAsync(userId, title, message, type);
    }

    public async Task SendOrderStatusUpdateAsync(Guid orderId, Guid userId, string status, string message)
    {
        await _orderSender.SendStatusUpdateAsync(orderId, userId, status, message);
    }

    public async Task SendCourierLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude)
    {
        await _courierSender.SendLocationUpdateAsync(orderId, latitude, longitude);
    }
}
