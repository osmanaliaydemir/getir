// ============================================
// React + TypeScript SignalR Integration Example
// ============================================

import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

// Types
interface Notification {
    title: string;
    message: string;
    type: string;
    timestamp: string;
}

interface OrderUpdate {
    orderId: string;
    status: string;
    message: string;
    timestamp: string;
}

interface CourierLocation {
    orderId: string;
    latitude: number;
    longitude: number;
    timestamp: string;
}

// ============================================
// Custom Hook: useSignalR
// ============================================

export const useSignalR = (bearerToken: string | null) => {
    const [notifications, setNotifications] = useState<Notification[]>([]);
    const [orderUpdates, setOrderUpdates] = useState<OrderUpdate[]>([]);
    const [courierLocation, setCourierLocation] = useState<CourierLocation | null>(null);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        if (!bearerToken) return;

        const connections: signalR.HubConnection[] = [];

        // Notification Hub
        const notificationHub = new signalR.HubConnectionBuilder()
            .withUrl('http://ajilgo.runasp.net/hubs/notifications', {
                accessTokenFactory: () => bearerToken,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        notificationHub.on('ReceiveNotification', (notification: Notification) => {
            console.log('📬 Notification:', notification);
            setNotifications(prev => [notification, ...prev]);
        });

        // Order Hub
        const orderHub = new signalR.HubConnectionBuilder()
            .withUrl('https://ajilgo.runasp.net/hubs/orders', {
                accessTokenFactory: () => bearerToken,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        orderHub.on('OrderStatusChanged', (update: OrderUpdate) => {
            console.log('📦 Order Update:', update);
            setOrderUpdates(prev => [update, ...prev]);
        });

        // Courier Hub
        const courierHub = new signalR.HubConnectionBuilder()
            .withUrl('https://ajilgo.runasp.net/hubs/courier', {
                accessTokenFactory: () => bearerToken,
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        courierHub.on('CourierLocationUpdated', (location: CourierLocation) => {
            console.log('🚴 Courier Location:', location);
            setCourierLocation(location);
        });

        // Connect all
        Promise.all([
            notificationHub.start(),
            orderHub.start(),
            courierHub.start()
        ])
            .then(() => {
                console.log('✅ All SignalR hubs connected');
                setIsConnected(true);
                connections.push(notificationHub, orderHub, courierHub);
            })
            .catch(err => {
                console.error('❌ SignalR connection error:', err);
                setIsConnected(false);
            });

        // Cleanup
        return () => {
            connections.forEach(conn => conn.stop());
            setIsConnected(false);
        };
    }, [bearerToken]);

    return {
        notifications,
        orderUpdates,
        courierLocation,
        isConnected
    };
};

// ============================================
// Example Component Usage
// ============================================

export const OrderTrackingComponent = () => {
    const token = localStorage.getItem('accessToken'); // Get from login
    const { notifications, orderUpdates, courierLocation, isConnected } = useSignalR(token);

    return (
        <div>
            <h2>📡 Connection Status: {isConnected ? '✅ Connected' : '❌ Disconnected'}</h2>

            {/* Notifications */}
            <section>
                <h3>🔔 Notifications</h3>
                {notifications.map((notif, index) => (
                    <div key={index} className="notification">
                        <strong>{notif.title}</strong>
                        <p>{notif.message}</p>
                        <small>{new Date(notif.timestamp).toLocaleTimeString()}</small>
                    </div>
                ))}
            </section>

            {/* Order Updates */}
            <section>
                <h3>📦 Order Updates</h3>
                {orderUpdates.map((update, index) => (
                    <div key={index} className="order-update">
                        <strong>Order: {update.orderId.substring(0, 8)}...</strong>
                        <p>Status: <span className="status">{update.status}</span></p>
                        <p>{update.message}</p>
                        <small>{new Date(update.timestamp).toLocaleTimeString()}</small>
                    </div>
                ))}
            </section>

            {/* Courier Location */}
            <section>
                <h3>🚴 Courier Location</h3>
                {courierLocation && (
                    <div className="courier-tracking">
                        <p>📍 Latitude: {courierLocation.latitude}</p>
                        <p>📍 Longitude: {courierLocation.longitude}</p>
                        <small>Updated: {new Date(courierLocation.timestamp).toLocaleTimeString()}</small>
                        {/* Integrate with Google Maps or Mapbox here */}
                    </div>
                )}
            </section>
        </div>
    );
};

// ============================================
// Standalone Client Class (for vanilla JS)
// ============================================

export class GetirRealTimeClient {
    private token: string;
    private connections: Map<string, signalR.HubConnection>;

    constructor(bearerToken: string) {
        this.token = bearerToken;
        this.connections = new Map();
    }

    async connect(): Promise<void> {
        await this.connectNotifications();
        await this.connectOrders();
        await this.connectCourier();
    }

    private async connectNotifications(): Promise<void> {
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_CONFIG.baseUrl}${SIGNALR_CONFIG.hubs.notifications}`, {
                accessTokenFactory: () => this.token
            })
            .withAutomaticReconnect()
            .build();

        await conn.start();
        this.connections.set('notifications', conn);
    }

    private async connectOrders(): Promise<void> {
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_CONFIG.baseUrl}${SIGNALR_CONFIG.hubs.orders}`, {
                accessTokenFactory: () => this.token
            })
            .withAutomaticReconnect()
            .build();

        await conn.start();
        this.connections.set('orders', conn);
    }

    private async connectCourier(): Promise<void> {
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_CONFIG.baseUrl}${SIGNALR_CONFIG.hubs.courier}`, {
                accessTokenFactory: () => this.token
            })
            .withAutomaticReconnect()
            .build();

        await conn.start();
        this.connections.set('courier', conn);
    }

    onNotification(callback: (notification: Notification) => void): void {
        this.connections.get('notifications')?.on('ReceiveNotification', callback);
    }

    onOrderUpdate(callback: (update: OrderUpdate) => void): void {
        this.connections.get('orders')?.on('OrderStatusChanged', callback);
    }

    onCourierLocation(callback: (location: CourierLocation) => void): void {
        this.connections.get('courier')?.on('CourierLocationUpdated', callback);
    }

    async subscribeToOrder(orderId: string): Promise<void> {
        await this.connections.get('orders')?.invoke('SubscribeToOrder', orderId);
    }

    async trackCourier(orderId: string): Promise<void> {
        await this.connections.get('courier')?.invoke('TrackOrder', orderId);
    }

    async disconnect(): Promise<void> {
        for (const conn of this.connections.values()) {
            await conn.stop();
        }
        this.connections.clear();
    }
}

// ============================================
// Usage Example
// ============================================

/*
// In your React app:

import { GetirRealTimeClient } from './signalr-client';

const App = () => {
    const [client, setClient] = useState<GetirRealTimeClient | null>(null);

    useEffect(() => {
        const token = localStorage.getItem('accessToken');
        if (!token) return;

        const signalRClient = new GetirRealTimeClient(token);
        
        signalRClient.connect().then(() => {
            // Listen to notifications
            signalRClient.onNotification((notif) => {
                console.log('New notification:', notif);
                // Show toast, update state, etc.
            });

            // Listen to order updates
            signalRClient.onOrderUpdate((update) => {
                console.log('Order update:', update);
                // Update order status in UI
            });

            // Listen to courier location
            signalRClient.onCourierLocation((location) => {
                console.log('Courier location:', location);
                // Update map marker
            });

            setClient(signalRClient);
        });

        return () => {
            signalRClient.disconnect();
        };
    }, []);

    return <div>Your App</div>;
};
*/
