using Getir.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace Getir.WebApi.Extensions;

/// <summary>
/// Extension methods for registering SignalR services
/// This includes hub context senders for real-time communication
/// </summary>
public static class SignalRServiceExtensions
{
    /// <summary>
    /// Registers all SignalR related services with dependency injection
    /// </summary>
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        // SignalR Core Service
        services.AddScoped<ISignalRService, Getir.Infrastructure.SignalR.SignalRService>();
        
        // SignalR Hub Senders
        services.AddScoped<ISignalRNotificationSender>(sp => 
            new Getir.Infrastructure.SignalR.SignalRNotificationSender(
                sp.GetRequiredService<IHubContext<Getir.WebApi.Hubs.NotificationHub>>()));
                
        services.AddScoped<ISignalROrderSender>(sp => 
            new Getir.Infrastructure.SignalR.SignalROrderSender(
                sp.GetRequiredService<IHubContext<Getir.WebApi.Hubs.OrderHub>>()));
                
        services.AddScoped<ISignalRCourierSender>(sp => 
            new Getir.Infrastructure.SignalR.SignalRCourierSender(
                sp.GetRequiredService<IHubContext<Getir.WebApi.Hubs.CourierHub>>()));
        
        return services;
    }
}

