// ============================================
// Getir SignalR Client - JavaScript Example
// ============================================

const SIGNALR_CONFIG = {
    baseUrl: 'https://ajilgo.runasp.net',
    hubs: {
        notifications: '/hubs/notifications',
        orders: '/hubs/orders',
        courier: '/hubs/courier'
    }
};

class GetirSignalRClient {
    constructor(bearerToken) {
        this.token = bearerToken;
        this.connections = {};
    }

    // ============================================
    // NOTIFICATION HUB
    // ============================================

    async connectNotifications(onNotificationReceived) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_CONFIG.baseUrl}${SIGNALR_CONFIG.hubs.notifications}`, {
                accessTokenFactory: () => this.token,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    // Exponential backoff: 0, 2, 10, 30 seconds
                    return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Event: Receive notification
        connection.on("ReceiveNotification", (notification) => {
            console.log('📬 Notification received:', notification);
            if (onNotificationReceived) {
                onNotificationReceived(notification);
            }
        });

        // Event: Notification marked as read (by another device)
        connection.on("NotificationRead", (notificationId) => {
            console.log('✅ Notification read:', notificationId);
        });

        // Connection state handlers
        connection.onreconnecting(() => {
            console.log('🔄 Reconnecting to NotificationHub...');
        });

        connection.onreconnected(() => {
            console.log('✅ Reconnected to NotificationHub');
        });

        connection.onclose(() => {
            console.log('❌ NotificationHub connection closed');
        });

        await connection.start();
        console.log('✅ NotificationHub connected');
        
        this.connections.notifications = connection;
        return connection;
    }

    async markNotificationAsRead(notificationId) {
        if (!this.connections.notifications) {
            throw new Error('NotificationHub not connected');
        }
        
        await this.connections.notifications.invoke("MarkAsRead", notificationId);
    }

    // ============================================
    // ORDER HUB
    // ============================================

    async connectOrders(onOrderStatusChanged) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_CONFIG.baseUrl}${SIGNALR_CONFIG.hubs.orders}`, {
                accessTokenFactory: () => this.token,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        // Event: Order status changed
        connection.on("OrderStatusChanged", (update) => {
            console.log('📦 Order status changed:', update);
            if (onOrderStatusChanged) {
                onOrderStatusChanged(update);
            }
        });

        await connection.start();
        console.log('✅ OrderHub connected');
        
        this.connections.orders = connection;
        return connection;
    }

    async subscribeToOrder(orderId) {
        if (!this.connections.orders) {
            throw new Error('OrderHub not connected');
        }
        
        await this.connections.orders.invoke("SubscribeToOrder", orderId);
        console.log(`✅ Subscribed to order: ${orderId}`);
    }

    async unsubscribeFromOrder(orderId) {
        if (!this.connections.orders) {
            throw new Error('OrderHub not connected');
        }
        
        await this.connections.orders.invoke("UnsubscribeFromOrder", orderId);
        console.log(`🚫 Unsubscribed from order: ${orderId}`);
    }

    // ============================================
    // COURIER HUB
    // ============================================

    async connectCourier(onCourierLocationUpdated) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_CONFIG.baseUrl}${SIGNALR_CONFIG.hubs.courier}`, {
                accessTokenFactory: () => this.token,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        // Event: Courier location updated
        connection.on("CourierLocationUpdated", (location) => {
            console.log('🚴 Courier location updated:', location);
            if (onCourierLocationUpdated) {
                onCourierLocationUpdated(location);
            }
        });

        await connection.start();
        console.log('✅ CourierHub connected');
        
        this.connections.courier = connection;
        return connection;
    }

    async trackOrder(orderId) {
        if (!this.connections.courier) {
            throw new Error('CourierHub not connected');
        }
        
        await this.connections.courier.invoke("TrackOrder", orderId);
        console.log(`📍 Tracking courier for order: ${orderId}`);
    }

    async updateCourierLocation(latitude, longitude, orderId) {
        if (!this.connections.courier) {
            throw new Error('CourierHub not connected');
        }
        
        await this.connections.courier.invoke("UpdateLocation", latitude, longitude, orderId);
        console.log(`📍 Courier location updated: ${latitude}, ${longitude}`);
    }

    // ============================================
    // DISCONNECT ALL
    // ============================================

    async disconnectAll() {
        const promises = [];
        
        if (this.connections.notifications) {
            promises.push(this.connections.notifications.stop());
        }
        if (this.connections.orders) {
            promises.push(this.connections.orders.stop());
        }
        if (this.connections.courier) {
            promises.push(this.connections.courier.stop());
        }

        await Promise.all(promises);
        console.log('🔌 All connections closed');
    }
}

// ============================================
// USAGE EXAMPLE
// ============================================

/*
// 1. Get token from your auth endpoint
const token = 'your-jwt-token-here';

// 2. Create client
const client = new GetirSignalRClient(token);

// 3. Connect to hubs
await client.connectNotifications((notification) => {
    console.log('New notification:', notification.title);
    // Update UI
});

await client.connectOrders((orderUpdate) => {
    console.log('Order status:', orderUpdate.status);
    // Update UI
});

await client.connectCourier((location) => {
    console.log('Courier at:', location.latitude, location.longitude);
    // Update map
});

// 4. Subscribe to specific order
await client.subscribeToOrder('order-guid-here');
await client.trackOrder('order-guid-here');

// 5. Cleanup on page unload
window.addEventListener('beforeunload', async () => {
    await client.disconnectAll();
});
*/

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = GetirSignalRClient;
}

