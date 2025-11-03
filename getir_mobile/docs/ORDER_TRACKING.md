# 📦 Order Tracking Flow

**Tarih:** 2 Kasım 2025  
**Konu:** Sipariş Takip ve Real-time Güncellemeler

---

## 📋 İçindekiler

- [Real-time Tracking](#real-time-tracking)
- [SignalR Integration](#signalr-integration)
- [GPS Integration](#gps-integration)
- [Order States](#order-states)

---

## 🔄 Real-time Tracking

### SignalR Hubs

**3 Hub Connection:**

1. **OrderHub** - Sipariş durum güncellemeleri
2. **TrackingHub** - Kurye konum takibi
3. **NotificationHub** - Push bildirimleri

### Connection Flow

```
App Start
    ↓
SignalRService.initialize()
    ↓
[Initialize 3 hubs in parallel]
    ↓
OrderHub.connect()
TrackingHub.connect()
NotificationHub.connect()
    ↓
[Authentication with Bearer token]
    ↓
[Hubs connected]
    ↓
OrderRealtimeBinder.start()
    ↓
[Listen to streams]
```

---

## 📡 SignalR Integration

### OrderHub

```dart
// Initialize
final hubUrl = '${apiBaseUrl}/hubs/orders';
_orderHubConnection = HubConnectionBuilder()
  .withUrl(hubUrl, options: HttpConnectionOptions(...))
  .build();

// Subscribe to events
_orderHubConnection.on('OrderStatusUpdated', (data) {
  _orderStatusController.add(OrderStatusUpdate.fromJson(data));
});

// Start connection
await _orderHubConnection.start();
```

### TrackingHub

```dart
// Subscribe to tracking updates
_trackingHubConnection.on('CourierLocationUpdated', (data) {
  _trackingDataController.add(TrackingData.fromJson(data));
});

// Join tracking group for specific order
await _trackingHubConnection.invoke('JoinTrackingGroup', args: [orderId]);
```

---

## 🗺️ GPS Integration

### Map Display

```dart
GoogleMap(
  initialCameraPosition: CameraPosition(
    target: LatLng(lat, lng),
    zoom: 15.0,
  ),
  markers: _markers,
  polylines: _polylines,
)
```

### Markers

- **Merchant:** Red marker
- **Courier:** Blue marker (moving)
- **Destination:** Green marker

### Polyline

Route from merchant to destination displayed as blue line.

---

## 📊 Order States

### Status Flow

```
Pending → Confirmed → Preparing → OnTheWay → Delivered
                               ↓
                         Cancelled
```

### State Handlers

```dart
BlocListener<OrderBloc, OrderState>(
  listener: (context, state) {
    if (state is OrderLoaded) {
      // Update UI based on status
      switch (state.order.status) {
        case OrderStatus.preparing:
          showPreparingStatus();
        case OrderStatus.onTheWay:
          startTracking();
        case OrderStatus.delivered:
          showSuccessDialog();
      }
    }
  },
)
```

---

**Hazırlayan:** Backend Team  
**Tarih:** 2 Kasım 2025

