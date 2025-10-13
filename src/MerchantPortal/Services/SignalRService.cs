using Microsoft.AspNetCore.SignalR.Client;

namespace Getir.MerchantPortal.Services;

public class SignalRService : ISignalRService
{
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(ApiSettings apiSettings, ILogger<SignalRService> logger)
    {
        _apiSettings = apiSettings;
        _logger = logger;
    }

    public Task<HubConnection> CreateOrderHubConnectionAsync(string token)
    {
        var hubUrl = $"{_apiSettings.SignalRHubUrl}/orders";
        var connection = BuildConnection(hubUrl, token);
        return Task.FromResult(connection);
    }

    public Task<HubConnection> CreateNotificationHubConnectionAsync(string token)
    {
        var hubUrl = $"{_apiSettings.SignalRHubUrl}/notifications";
        var connection = BuildConnection(hubUrl, token);
        return Task.FromResult(connection);
    }

    public Task<HubConnection> CreateCourierHubConnectionAsync(string token)
    {
        var hubUrl = $"{_apiSettings.SignalRHubUrl}/courier";
        var connection = BuildConnection(hubUrl, token);
        return Task.FromResult(connection);
    }

    public async Task StartConnectionAsync(HubConnection connection)
    {
        try
        {
            if (connection.State == HubConnectionState.Disconnected)
            {
                await connection.StartAsync();
                _logger.LogInformation("SignalR connection started successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection");
            throw;
        }
    }

    public async Task StopConnectionAsync(HubConnection connection)
    {
        try
        {
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.StopAsync();
                _logger.LogInformation("SignalR connection stopped successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping SignalR connection");
        }
    }

    private HubConnection BuildConnection(string hubUrl, string token)
    {
        return new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                options.Headers.Add("Authorization", $"Bearer {token}");
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();
    }
}

