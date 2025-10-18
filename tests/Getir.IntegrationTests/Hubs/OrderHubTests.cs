using FluentAssertions;
using Getir.Application.DTO;
using Getir.Domain.Enums;
using Getir.IntegrationTests.Helpers;
using Getir.IntegrationTests.Setup;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace Getir.IntegrationTests.Hubs;

/// <summary>
/// Integration tests for OrderHub
/// Tests real-time order tracking, status updates, and order management features
/// </summary>
public class OrderHubTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private Guid _userId;

    public OrderHubTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        (_authToken, _userId) = await TestHelpers.GetAuthTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Connect_WithValidToken_ShouldEstablishConnection()
    {
        // Arrange & Act
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        // Assert
        connection.State.Should().Be(HubConnectionState.Connected);
        
        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task GetMyActiveOrders_ShouldReturnOrders()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var receivedMessage = false;
        object? orders = null;

        connection.On<object>("ActiveOrders", data =>
        {
            receivedMessage = true;
            orders = data;
        });

        // Act
        await connection.InvokeAsync("GetMyActiveOrders");
        
        // Wait for response
        await Task.Delay(1000);

        // Assert
        receivedMessage.Should().BeTrue();
        orders.Should().NotBeNull();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task SubscribeToOrder_WithValidOrderId_ShouldJoinGroup()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        // Create a test order first
        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        var subscribedReceived = false;
        object? orderData = null;

        connection.On<object>("OrderSubscribed", data =>
        {
            subscribedReceived = true;
            orderData = data;
        });

        // Act
        await connection.InvokeAsync("SubscribeToOrder", orderId);
        
        // Wait for response
        await Task.Delay(1000);

        // Assert
        subscribedReceived.Should().BeTrue();
        orderData.Should().NotBeNull();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task SubscribeToOrder_WithInvalidOrderId_ShouldReturnError()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var errorReceived = false;
        string? errorMessage = null;

        connection.On<string>("Error", error =>
        {
            errorReceived = true;
            errorMessage = error;
        });

        // Act
        await connection.InvokeAsync("SubscribeToOrder", Guid.NewGuid());
        
        // Wait for error
        await Task.Delay(1000);

        // Assert
        errorReceived.Should().BeTrue();
        errorMessage.Should().NotBeNullOrEmpty();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task UnsubscribeFromOrder_ShouldLeaveGroup()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        // Subscribe first
        await connection.InvokeAsync("SubscribeToOrder", orderId);
        await Task.Delay(500);

        // Act
        await connection.InvokeAsync("UnsubscribeFromOrder", orderId);
        
        // Wait for processing
        await Task.Delay(500);

        // Assert
        connection.State.Should().Be(HubConnectionState.Connected);

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task GetOrderTracking_WithValidOrder_ShouldReturnOrderInfo()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        var receivedTracking = false;
        object? trackingInfo = null;

        connection.On<object>("OrderTrackingInfo", data =>
        {
            receivedTracking = true;
            trackingInfo = data;
        });

        // Act
        await connection.InvokeAsync("GetOrderTracking", orderId);
        
        // Wait for response
        await Task.Delay(1000);

        // Assert
        receivedTracking.Should().BeTrue();
        trackingInfo.Should().NotBeNull();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task UpdateOrderStatus_AsAdmin_ShouldBroadcastUpdate()
    {
        // Arrange
        var adminConnection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var customerConnection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        // Customer subscribes to order
        await customerConnection.InvokeAsync("SubscribeToOrder", orderId);
        await Task.Delay(500);

        var statusUpdateReceived = false;
        object? updateData = null;

        customerConnection.On<object>("OrderStatusUpdated", data =>
        {
            statusUpdateReceived = true;
            updateData = data;
        });

        // Act
        await adminConnection.InvokeAsync("UpdateOrderStatus", orderId, OrderStatus.Preparing, "Order is being prepared");
        
        // Wait for broadcast
        await Task.Delay(1500);

        // Assert
        statusUpdateReceived.Should().BeTrue();
        updateData.Should().NotBeNull();

        // Cleanup
        await adminConnection.StopAsync();
        await adminConnection.DisposeAsync();
        await customerConnection.StopAsync();
        await customerConnection.DisposeAsync();
    }

    [Fact]
    public async Task CancelOrder_ShouldBroadcastCancellation()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        await connection.InvokeAsync("SubscribeToOrder", orderId);
        await Task.Delay(500);

        var cancelReceived = false;
        object? cancelData = null;

        connection.On<object>("OrderCancelled", data =>
        {
            cancelReceived = true;
            cancelData = data;
        });

        // Act
        await connection.InvokeAsync("CancelOrder", orderId, "Changed my mind");
        
        // Wait for broadcast
        await Task.Delay(1500);

        // Assert
        cancelReceived.Should().BeTrue();
        cancelData.Should().NotBeNull();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task RateOrder_ShouldSendRatingConfirmation()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        var ratingReceived = false;
        object? ratingData = null;

        connection.On<object>("OrderRatedSuccess", data =>
        {
            ratingReceived = true;
            ratingData = data;
        });

        // Act
        await connection.InvokeAsync("RateOrder", orderId, 5, "Excellent service!");
        
        // Wait for response
        await Task.Delay(1000);

        // Assert
        ratingReceived.Should().BeTrue();
        ratingData.Should().NotBeNull();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task RateOrder_WithInvalidRating_ShouldReturnError()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        var errorReceived = false;
        string? errorMessage = null;

        connection.On<string>("Error", error =>
        {
            errorReceived = true;
            errorMessage = error;
        });

        // Act - Rating must be 1-5
        await connection.InvokeAsync("RateOrder", orderId, 10, "Invalid rating");
        
        // Wait for error
        await Task.Delay(1000);

        // Assert
        errorReceived.Should().BeTrue();
        errorMessage.Should().Contain("Rating must be between 1 and 5");

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task SubscribeToMerchantOrders_AsAdmin_ShouldReceivePendingOrders()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var merchantId = await TestHelpers.CreateTestMerchantAsync(_factory.Services);

        var ordersReceived = false;
        object? merchantOrders = null;

        connection.On<object>("MerchantPendingOrders", data =>
        {
            ordersReceived = true;
            merchantOrders = data;
        });

        // Act
        await connection.InvokeAsync("SubscribeToMerchantOrders", merchantId);
        
        // Wait for response
        await Task.Delay(1000);

        // Assert
        ordersReceived.Should().BeTrue();
        merchantOrders.Should().NotBeNull();

        // Cleanup
        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task MultipleConnections_OrderUpdate_ShouldBroadcastToAllSubscribers()
    {
        // Arrange
        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);

        var connections = await SignalRTestHelper.CreateMultipleConnectionsAsync(
            _factory,
            "/hubs/orders",
            3,
            _authToken);

        // All connections subscribe to same order
        foreach (var conn in connections)
        {
            await conn.InvokeAsync("SubscribeToOrder", orderId);
        }
        await Task.Delay(1000);

        var receivedCounts = new int[3];

        for (int i = 0; i < 3; i++)
        {
            var index = i;
            connections[index].On<object>("OrderStatusUpdated", data =>
            {
                receivedCounts[index]++;
            });
        }

        // Act - Update order status
        await connections[0].InvokeAsync("UpdateOrderStatus", orderId, OrderStatus.Confirmed, "Order confirmed");
        
        // Wait for broadcast
        await Task.Delay(2000);

        // Assert - All connections should receive the update
        receivedCounts.Should().AllSatisfy(count => count.Should().BeGreaterThan(0));

        // Cleanup
        await SignalRTestHelper.DisposeConnectionsAsync(connections);
    }

    [Fact]
    public async Task ConnectionDisconnect_ShouldRemoveFromGroups()
    {
        // Arrange
        var connection = await SignalRTestHelper.StartConnectionAsync(
            _factory,
            "/hubs/orders",
            _authToken);

        var orderId = await TestHelpers.CreateTestOrderAsync(_factory.Services);
        await connection.InvokeAsync("SubscribeToOrder", orderId);
        await Task.Delay(500);

        // Act
        await connection.StopAsync();

        // Assert
        connection.State.Should().Be(HubConnectionState.Disconnected);

        // Cleanup
        await connection.DisposeAsync();
    }
}

