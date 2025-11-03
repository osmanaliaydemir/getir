using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace WebApp.Services;

public interface ISignalRService : IAsyncDisposable
{
    event Action<string, object>? NotificationReceived;
    event Action<string>? ConnectionStatusChanged;
    bool IsConnected { get; }
    HubConnectionState ConnectionState { get; }
    Task StartConnectionAsync();
    Task StopConnectionAsync();
    Task SendNotificationAsync(string method, object data);
    Task JoinGroupAsync(string groupName);
    Task LeaveGroupAsync(string groupName);
}

public class SignalRService : ISignalRService
{
    private HubConnection? _hubConnection;
    private readonly IAuthService _authService;
    private readonly ILogger<SignalRService> _logger;
    private readonly IConfiguration _configuration;

    public event Action<string, object>? NotificationReceived;
    public event Action<string>? ConnectionStatusChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public HubConnectionState ConnectionState => _hubConnection?.State ?? HubConnectionState.Disconnected;

    public SignalRService(IAuthService authService, ILogger<SignalRService> logger, IConfiguration configuration)
    {
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartConnectionAsync()
    {
        try
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                _logger.LogInformation("SignalR connection already established");
                return;
            }

            var token = await _authService.GetTokenAsync();
            var hubUrl = _configuration["ApiSettings:SignalRHubUrl"] ?? 
                (_configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development" 
                    ? "https://localhost:7001/hubs" 
                    : "https://ajilgo.runasp.net/hubs");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{hubUrl}/notification", options =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                    }
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .Build();

            // Register event handlers
            RegisterEventHandlers();

            await _hubConnection.StartAsync();
            
            ConnectionStatusChanged?.Invoke("Connected");
            _logger.LogInformation("SignalR connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start SignalR connection");
            ConnectionStatusChanged?.Invoke("Failed");
            throw;
        }
    }

    public async Task StopConnectionAsync()
    {
        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                ConnectionStatusChanged?.Invoke("Disconnected");
                _logger.LogInformation("SignalR connection stopped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping SignalR connection");
        }
    }

    public async Task SendNotificationAsync(string method, object data)
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync(method, data);
                _logger.LogDebug("Sent notification: {Method}", method);
            }
            else
            {
                _logger.LogWarning("Cannot send notification - SignalR not connected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification: {Method}", method);
        }
    }

    public async Task JoinGroupAsync(string groupName)
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("JoinGroup", groupName);
                _logger.LogInformation("Joined SignalR group: {GroupName}", groupName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join group: {GroupName}", groupName);
        }
    }

    public async Task LeaveGroupAsync(string groupName)
    {
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("LeaveGroup", groupName);
                _logger.LogInformation("Left SignalR group: {GroupName}", groupName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to leave group: {GroupName}", groupName);
        }
    }

    private void RegisterEventHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<string, object>("ReceiveNotification", (type, data) =>
        {
            _logger.LogDebug("Received notification: {Type}", type);
            NotificationReceived?.Invoke(type, data);
        });

        _hubConnection.On<string>("ReceiveMessage", (message) =>
        {
            _logger.LogDebug("Received message: {Message}", message);
            NotificationReceived?.Invoke("Message", message);
        });

        _hubConnection.On<string>("ReceiveOrderUpdate", (orderData) =>
        {
            _logger.LogDebug("Received order update: {OrderData}", orderData);
            NotificationReceived?.Invoke("OrderUpdate", orderData);
        });

        _hubConnection.Closed += async (error) =>
        {
            _logger.LogWarning("SignalR connection closed: {Error}", error?.Message);
            ConnectionStatusChanged?.Invoke("Disconnected");
            
            if (error != null)
            {
                // Attempt to reconnect after a delay
                await Task.Delay(5000);
                try
                {
                    await StartConnectionAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to reconnect SignalR");
                }
            }
        };

        _hubConnection.Reconnecting += async (error) =>
        {
            _logger.LogInformation("SignalR reconnecting: {Error}", error?.Message);
            ConnectionStatusChanged?.Invoke("Reconnecting");
            await Task.CompletedTask;
        };

        _hubConnection.Reconnected += async (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
            ConnectionStatusChanged?.Invoke("Connected");
            await Task.CompletedTask;
        };
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _logger.LogInformation("SignalR service disposed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing SignalR service");
        }
    }
}
