using Microsoft.AspNetCore.SignalR.Client;

namespace Getir.MerchantPortal.Services;

public interface ISignalRService
{
    Task<HubConnection> CreateOrderHubConnectionAsync(string token);
    Task<HubConnection> CreateNotificationHubConnectionAsync(string token);
    Task<HubConnection> CreateCourierHubConnectionAsync(string token);
    Task StartConnectionAsync(HubConnection connection);
    Task StopConnectionAsync(HubConnection connection);
}

