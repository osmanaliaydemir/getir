using Getir.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Getir.Infrastructure.SignalR;

public class SignalRNotificationSender : ISignalRNotificationSender
{
    private readonly IHubContext<Hub> _hubContext;

    public SignalRNotificationSender(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, string type)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", new
            {
                title,
                message,
                type,
                timestamp = DateTime.UtcNow
            });
    }
}

public class SignalROrderSender : ISignalROrderSender
{
    private readonly IHubContext<Hub> _hubContext;

    public SignalROrderSender(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendStatusUpdateAsync(Guid orderId, Guid userId, string status, string message)
    {
        // Send to specific order subscribers
        await _hubContext.Clients
            .Group($"order_{orderId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                status,
                message,
                timestamp = DateTime.UtcNow
            });

        // Also send to user's group
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                status,
                message,
                timestamp = DateTime.UtcNow
            });
    }
}

public class SignalRCourierSender : ISignalRCourierSender
{
    private readonly IHubContext<Hub> _hubContext;

    public SignalRCourierSender(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude)
    {
        await _hubContext.Clients
            .Group($"order_{orderId}")
            .SendAsync("CourierLocationUpdated", new
            {
                orderId,
                latitude,
                longitude,
                timestamp = DateTime.UtcNow
            });
    }
}
