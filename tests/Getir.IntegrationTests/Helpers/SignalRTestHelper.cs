using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace Getir.IntegrationTests.Helpers;

/// <summary>
/// Helper class for SignalR hub integration testing
/// </summary>
public static class SignalRTestHelper
{
    /// <summary>
    /// Creates a configured HubConnection for testing
    /// </summary>
    public static HubConnection CreateHubConnection<TProgram>(
        WebApplicationFactory<TProgram> factory,
        string hubPath,
        string? accessToken = null) where TProgram : class
    {
        // SignalR requires token in query string for negotiate endpoint
        var hubUrl = factory.Server.BaseAddress + hubPath.TrimStart('/');
        if (!string.IsNullOrEmpty(accessToken))
        {
            hubUrl += $"?access_token={accessToken}";
        }
        
        var connectionBuilder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                
                // Token is already in URL query string
                if (!string.IsNullOrEmpty(accessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                }
                
                // WebSocket transport is not available in test server
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            })
            .WithAutomaticReconnect();

        return connectionBuilder.Build();
    }

    /// <summary>
    /// Starts hub connection and waits for it to be established
    /// </summary>
    public static async Task<HubConnection> StartConnectionAsync<TProgram>(
        WebApplicationFactory<TProgram> factory,
        string hubPath,
        string? accessToken = null,
        int timeoutSeconds = 5) where TProgram : class
    {
        var connection = CreateHubConnection(factory, hubPath, accessToken);
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        await connection.StartAsync(cts.Token);
        
        return connection;
    }

    /// <summary>
    /// Waits for a specific message on the hub connection
    /// </summary>
    public static async Task<T> WaitForMessageAsync<T>(
        HubConnection connection,
        string methodName,
        int timeoutSeconds = 5)
    {
        var tcs = new TaskCompletionSource<T>();
        
        using var registration = connection.On<T>(methodName, data =>
        {
            tcs.TrySetResult(data);
        });
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        cts.Token.Register(() => tcs.TrySetCanceled());
        
        return await tcs.Task;
    }

    /// <summary>
    /// Waits for any message on the hub connection
    /// </summary>
    public static async Task<object?> WaitForAnyMessageAsync(
        HubConnection connection,
        string methodName,
        int timeoutSeconds = 5)
    {
        var tcs = new TaskCompletionSource<object?>();
        
        using var registration = connection.On(methodName, (object? data) =>
        {
            tcs.TrySetResult(data);
        });
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        cts.Token.Register(() => tcs.TrySetCanceled());
        
        return await tcs.Task;
    }

    /// <summary>
    /// Invokes a hub method and waits for response
    /// </summary>
    public static async Task<TResult> InvokeAndWaitAsync<TResult>(
        HubConnection connection,
        string methodName,
        string responseMethodName,
        params object?[] args)
    {
        var responseTask = WaitForMessageAsync<TResult>(connection, responseMethodName);
        await connection.InvokeAsync(methodName, args);
        return await responseTask;
    }

    /// <summary>
    /// Creates multiple hub connections for testing group broadcasting
    /// </summary>
    public static async Task<List<HubConnection>> CreateMultipleConnectionsAsync<TProgram>(
        WebApplicationFactory<TProgram> factory,
        string hubPath,
        int count,
        string? accessToken = null) where TProgram : class
    {
        var connections = new List<HubConnection>();
        
        for (int i = 0; i < count; i++)
        {
            var connection = await StartConnectionAsync(factory, hubPath, accessToken);
            connections.Add(connection);
        }
        
        return connections;
    }

    /// <summary>
    /// Disposes multiple connections
    /// </summary>
    public static async Task DisposeConnectionsAsync(List<HubConnection> connections)
    {
        foreach (var connection in connections)
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }
}

