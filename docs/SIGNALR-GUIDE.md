# 🔔 SignalR Real-Time Guide

## 🎯 Overview

Getir API'si **3 SignalR Hub** ile real-time özellikleri destekler:

1. **NotificationHub** - Kullanıcı bildirimleri
2. **OrderHub** - Sipariş durumu takibi
3. **CourierHub** - Kurye lokasyon tracking

---

## 📡 SignalR Endpoints

```
wss://localhost:7001/hubs/notifications
wss://localhost:7001/hubs/orders
wss://localhost:7001/hubs/courier
```

**Authentication:** JWT Bearer Token gerekli ✅

---

## 🔔 NotificationHub

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7001/hubs/notifications", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

await connection.start();
```

### Server → Client Events

#### `ReceiveNotification`
Kullanıcıya yeni bildirim geldiğinde tetiklenir.

```javascript
connection.on("ReceiveNotification", (notification) => {
    console.log(notification);
    // {
    //   title: "Order Created",
    //   message: "Your order ORD-20251001-ABC123 has been placed",
    //   type: "Order",
    //   timestamp: "2025-10-01T12:30:00Z"
    // }
});
```

#### `NotificationRead`
Başka cihazda notification okunduğunda senkronizasyon.

```javascript
connection.on("NotificationRead", (notificationId) => {
    // Mark as read in UI
});
```

### Client → Server Methods

#### `MarkAsRead`
Bildirimi okundu olarak işaretle.

```javascript
await connection.invoke("MarkAsRead", notificationId);
```

### Auto-Grouping

Kullanıcı connect olduğunda otomatik olarak `user_{userId}` grubuna eklenir.

---

## 📦 OrderHub

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7001/hubs/orders", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

await connection.start();
```

### Server → Client Events

#### `OrderStatusChanged`
Sipariş durumu değiştiğinde tetiklenir.

```javascript
connection.on("OrderStatusChanged", (update) => {
    console.log(update);
    // {
    //   orderId: "uuid",
    //   status: "Preparing",
    //   message: "Your order is being prepared",
    //   timestamp: "2025-10-01T12:35:00Z"
    // }
});
```

**Status Flow:**
```
Pending → Confirmed → Preparing → Ready → OnTheWay → Delivered
```

### Client → Server Methods

#### `SubscribeToOrder`
Belirli bir siparişin güncellemelerine abone ol.

```javascript
await connection.invoke("SubscribeToOrder", orderId);
```

#### `UnsubscribeFromOrder`
Sipariş takibini durdur.

```javascript
await connection.invoke("UnsubscribeFromOrder", orderId);
```

### Grouping

- `user_{userId}` - Kullanıcının tüm siparişleri
- `order_{orderId}` - Belirli sipariş takibi

---

## 🚴 CourierHub

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7001/hubs/courier", {
        accessTokenFactory: () => yourJwtToken
    })
    .build();

await connection.start();
```

### Server → Client Events

#### `CourierLocationUpdated`
Kurye lokasyonu güncellendiğinde tetiklenir.

```javascript
connection.on("CourierLocationUpdated", (location) => {
    console.log(location);
    // {
    //   orderId: "uuid",
    //   latitude: 40.9900,
    //   longitude: 29.0260,
    //   timestamp: "2025-10-01T12:40:00Z"
    // }
    
    // Update map marker
    updateMapMarker(location.latitude, location.longitude);
});
```

### Client → Server Methods (Courier Only)

#### `UpdateLocation`
Kurye kendi lokasyonunu güncelliyor.

```javascript
await connection.invoke("UpdateLocation", latitude, longitude, orderId);
```

#### `TrackOrder` (Customer)
Müşteri kurye takibine başlıyor.

```javascript
await connection.invoke("TrackOrder", orderId);
```

### Grouping

- `courier_{courierId}` - Kurye grubu
- `order_{orderId}` - Sipariş takip grubu (müşteriler)

---

## 🎯 Real-Time Flow Examples

### Scenario 1: Order Creation

```
1. Customer → POST /api/v1/orders
2. API → Create order in DB
3. API → SignalR.SendOrderStatusUpdateAsync()
4. SignalR → Broadcast to user_{userId}
5. All Customer Devices → Receive "Order Created" notification
```

### Scenario 2: Courier Location Tracking

```
1. Courier → POST /api/v1/courier/location/update
2. API → Update location in DB
3. API → SignalR.SendCourierLocationUpdateAsync()
4. SignalR → Broadcast to order_{orderId}
5. Customer → See live courier movement on map
```

### Scenario 3: Order Status Update

```
1. Merchant → Update order status (future endpoint)
2. API → Update in DB
3. API → SignalR.SendOrderStatusUpdateAsync()
4. SignalR → Broadcast to:
   - order_{orderId} group
   - user_{userId} group
5. Customer → Instant status update notification
```

---

## 💻 Client Integration

### HTML/JavaScript (Vanilla)

```html
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.0/dist/browser/signalr.min.js"></script>
<script src="examples/signalr-client.js"></script>
<script>
    const client = new GetirSignalRClient(yourToken);
    
    await client.connectNotifications((notif) => {
        alert(`New notification: ${notif.title}`);
    });
    
    await client.connectOrders((update) => {
        console.log('Order status:', update.status);
    });
    
    await client.subscribeToOrder(orderId);
</script>
```

### React/TypeScript

```tsx
import { useSignalR } from './hooks/useSignalR';

const OrderPage = () => {
    const token = useAuth(); // Your auth hook
    const { notifications, orderUpdates, isConnected } = useSignalR(token);

    return (
        <div>
            {isConnected && <Badge>🟢 Live</Badge>}
            {notifications.map(n => <Notification key={n.id} data={n} />)}
        </div>
    );
};
```

### Angular

```typescript
import * as signalR from '@microsoft/signalr';

export class SignalRService {
    private connection: signalR.HubConnection;

    connect(token: string) {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7001/hubs/notifications', {
                accessTokenFactory: () => token
            })
            .build();

        this.connection.on('ReceiveNotification', (notif) => {
            // Emit to Angular service
        });

        return this.connection.start();
    }
}
```

### Vue.js

```vue
<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';

const notifications = ref([]);
let connection;

onMounted(async () => {
    const token = localStorage.getItem('token');
    
    connection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7001/hubs/notifications', {
            accessTokenFactory: () => token
        })
        .build();

    connection.on('ReceiveNotification', (notif) => {
        notifications.value.unshift(notif);
    });

    await connection.start();
});

onUnmounted(() => {
    connection?.stop();
});
</script>
```

---

## 🧪 Testing SignalR

### 1. HTML Test Client

```bash
# Open examples/signalr-client.html in browser
# Enter token from Postman
# Click "Connect All Hubs"
# Create order in Postman → See real-time notification!
```

### 2. Postman Testing

**Cannot test SignalR directly**, but you can:
- Create order → Trigger SignalR broadcast
- Update courier location → Trigger location broadcast
- HTML client will receive updates

### 3. Browser Console

```javascript
// In browser console
const conn = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7001/hubs/notifications", {
        accessTokenFactory: () => "your-token-here"
    })
    .build();

conn.on("ReceiveNotification", console.log);
await conn.start();

// Create order in Postman → See console.log()
```

---

## 🔐 Authentication

### JWT Token Required

```javascript
// Correct
.withUrl("/hubs/notifications", {
    accessTokenFactory: () => yourJwtToken  // ✅
})

// Wrong
.withUrl("/hubs/notifications")  // ❌ 401 Unauthorized
```

### Token from Login

```javascript
// 1. Login via API
const response = await fetch('/api/v1/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password })
});

const { accessToken } = await response.json();

// 2. Use in SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications", {
        accessTokenFactory: () => accessToken
    })
    .build();
```

---

## ⚡ Connection Management

### Automatic Reconnection

```javascript
.withAutomaticReconnect({
    nextRetryDelayInMilliseconds: retryContext => {
        // Exponential backoff
        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
    }
})
```

**Retry Schedule:**
- 1st retry: 0 seconds
- 2nd retry: 2 seconds
- 3rd retry: 4 seconds
- 4th retry: 8 seconds
- 5th+ retry: 30 seconds (max)

### Connection State Events

```javascript
connection.onreconnecting(() => {
    console.log('🔄 Reconnecting...');
    // Show "Connecting..." in UI
});

connection.onreconnected(() => {
    console.log('✅ Reconnected');
    // Re-subscribe to orders if needed
});

connection.onclose(() => {
    console.log('❌ Connection closed');
    // Show "Offline" in UI
});
```

---

## 🐛 Troubleshooting

### Problem: 401 Unauthorized

**Cause:** Invalid or missing JWT token

**Solution:**
```javascript
// Check token
console.log('Token:', yourToken);

// Ensure accessTokenFactory returns valid token
accessTokenFactory: () => {
    const token = localStorage.getItem('token');
    console.log('Using token:', token);
    return token;
}
```

### Problem: Connection fails

**Cause:** CORS or SSL issues

**Solution:**
```javascript
// 1. Check CORS in Program.cs
// 2. Accept self-signed cert in browser
// 3. Use skipNegotiation for WebSockets
.withUrl("/hubs/notifications", {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets
})
```

### Problem: Events not received

**Cause:** Not subscribed to group

**Solution:**
```javascript
// For orders, explicitly subscribe
await connection.invoke("SubscribeToOrder", orderId);

// For notifications, auto-subscribed on connect
```

---

## 📊 Performance Tips

### 1. Connection Pooling

```javascript
// ✅ Good - Single connection per hub
const notifConn = createNotificationConnection();

// ❌ Bad - Multiple connections
setInterval(() => {
    const conn = createNotificationConnection(); // Memory leak!
}, 1000);
```

### 2. Cleanup

```javascript
// React
useEffect(() => {
    const conn = createConnection();
    
    return () => {
        conn.stop(); // ✅ Cleanup
    };
}, []);
```

### 3. Batching

```javascript
// Server-side (future)
// Send multiple notifications in batch
await Clients.Group("user_123").SendAsync("BatchNotifications", notifications);
```

---

## 🔥 Use Cases

### 1. Order Tracking Dashboard

```
Customer sees:
├─ Order Created ✅
├─ Order Confirmed ✅
├─ Preparing... ⏳
├─ Ready for Pickup ✅
├─ Courier on the way 🚴
│  └─ Live location updates 📍
└─ Delivered! 🎉
```

### 2. Merchant Dashboard

```
Merchant sees:
├─ New Order Notification 🔔
├─ Accept/Reject buttons
├─ Order preparation status
└─ Courier assignment notification
```

### 3. Courier App

```
Courier sees:
├─ New delivery assigned 🔔
├─ Pickup location 📍
├─ Delivery location 📍
├─ Live navigation
└─ Customer contact info
```

---

## 📱 Mobile Integration

### React Native

```javascript
import SignalR from '@react-native-signalr/signalr';

const connection = SignalR.createHubConnection({
    url: 'https://api.getir.com/hubs/notifications',
    accessTokenFactory: () => getAuthToken()
});

connection.on('ReceiveNotification', (notif) => {
    // Show push notification
    PushNotification.localNotification({
        title: notif.title,
        message: notif.message
    });
});

await connection.start();
```

### Flutter

```dart
import 'package:signalr_netcore/signalr_client.dart';

final hubConnection = HubConnectionBuilder()
    .withUrl("https://api.getir.com/hubs/notifications",
        options: HttpConnectionOptions(
            accessTokenFactory: () => getAuthToken()
        ))
    .build();

hubConnection.on("ReceiveNotification", (arguments) {
    print('Notification: ${arguments}');
});

await hubConnection.start();
```

---

## 🎯 Real-World Scenarios

### Scenario 1: Live Order Updates

**Customer Experience:**
```
12:30 - Order placed ✅
12:31 - Merchant confirmed ✅ (Real-time!)
12:35 - Preparing your food 🍔
12:45 - Ready for pickup ✅
12:46 - Courier on the way 🚴
12:46-12:55 - Live courier location 📍 (every 10 sec)
12:55 - Delivered! 🎉
```

### Scenario 2: Merchant Notification

```
New order comes in:
1. Merchant's dashboard shows red badge 🔴
2. Sound notification plays 🔔
3. Order details pop up
4. Accept/Reject buttons active
5. Real-time kitchen display update
```

### Scenario 3: Multi-Device Sync

```
Customer has 2 devices:
1. Marks notification as read on iPhone
2. Instantly marked as read on web browser
3. Badge count synced across devices
```

---

## 📊 SignalR vs Polling

| Feature | Polling (Old Way) | SignalR (New Way) |
|---------|------------------|-------------------|
| **Latency** | 5-30 seconds | <1 second ⚡ |
| **Server Load** | High (constant requests) | Low (persistent connection) |
| **Battery** | Drains fast | Efficient |
| **Bandwidth** | Wasteful | Minimal |
| **Real-time** | ❌ Delayed | ✅ Instant |

---

## 🔧 Configuration

### Server (Program.cs)

```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Development
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Map hubs
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<OrderHub>("/hubs/orders");
app.MapHub<CourierHub>("/hubs/courier");
```

### CORS (Important!)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR için gerekli!
    });
});
```

---

## 📈 Scalability

### Azure SignalR Service (Production)

```csharp
// For production scaling
builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        options.ConnectionString = "Endpoint=https://...";
    });
```

**Benefits:**
- Unlimited connections
- Auto-scaling
- Global distribution
- 99.9% SLA

---

## 🎉 Example Files

1. **`examples/signalr-client.html`** - Interactive test page
2. **`examples/signalr-client.js`** - Vanilla JS client
3. **`examples/react-signalr-example.tsx`** - React/TypeScript integration

---

## 🚀 Quick Test

### 1. Start API

```bash
dotnet run --project src/WebApi
```

### 2. Open Test Client

```bash
# Open examples/signalr-client.html in browser
```

### 3. Get Token

```bash
# Postman: POST /api/v1/auth/register
# Copy accessToken
```

### 4. Connect

```
1. Paste token in HTML page
2. Click "Connect All Hubs"
3. Status should show "✅ All Hubs Connected"
```

### 5. Test

```bash
# Postman: POST /api/v1/orders
# → HTML page instantly shows notification! 🎉
```

---

## 📝 Best Practices

### ✅ DO

- Use automatic reconnection
- Handle connection state (connecting, connected, disconnected)
- Cleanup connections on component unmount
- Use groups for targeted messaging
- Keep connection alive (KeepAlive intervals)
- Log connection events

### ❌ DON'T

- Create multiple connections to same hub
- Forget to stop connections
- Send large payloads (use compression)
- Poll while using SignalR (defeats purpose)
- Ignore connection errors

---

## 🎯 Summary

### ✅ **SignalR Features**

```
3 Hubs:
  ✅ NotificationHub - User notifications
  ✅ OrderHub - Order tracking
  ✅ CourierHub - Live location

Features:
  ✅ JWT Authentication
  ✅ Auto-reconnection
  ✅ Group messaging
  ✅ Bi-directional communication
  ✅ Cross-platform (Web, Mobile, Desktop)

Performance:
  ✅ <1 second latency
  ✅ WebSocket transport
  ✅ Efficient bandwidth
  ✅ Battery friendly
```

---

**Real-time features are now live! 🚀**

**Test with:** `examples/signalr-client.html`
