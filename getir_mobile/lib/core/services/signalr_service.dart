import 'dart:async';
import 'package:flutter/foundation.dart';
import 'package:signalr_core/signalr_core.dart';
import '../config/environment_config.dart';
import 'encryption_service.dart';

/// SignalR Service for Real-time Communication
/// Manages connections to OrderHub, RealtimeTrackingHub, NotificationHub
class SignalRService {
  static final SignalRService _instance = SignalRService._internal();
  factory SignalRService() => _instance;
  SignalRService._internal();

  HubConnection? _orderHubConnection;
  HubConnection? _trackingHubConnection;
  HubConnection? _notificationHubConnection;

  bool _isOrderHubConnected = false;
  bool _isTrackingHubConnected = false;
  bool _isNotificationHubConnected = false;

  final EncryptionService _encryptionService = EncryptionService();

  // Event streams
  final _orderStatusController =
      StreamController<OrderStatusUpdate>.broadcast();
  final _trackingDataController = StreamController<TrackingData>.broadcast();
  final _locationUpdateController =
      StreamController<LocationUpdate>.broadcast();
  final _notificationController =
      StreamController<RealtimeNotification>.broadcast();

  Stream<OrderStatusUpdate> get orderStatusStream =>
      _orderStatusController.stream;
  Stream<TrackingData> get trackingDataStream => _trackingDataController.stream;
  Stream<LocationUpdate> get locationUpdateStream =>
      _locationUpdateController.stream;
  Stream<RealtimeNotification> get notificationStream =>
      _notificationController.stream;

  /// Initialize all SignalR hubs
  Future<void> initialize() async {
    await initializeOrderHub();
    await initializeTrackingHub();
    await initializeNotificationHub();
  }

  /// Initialize Order Hub
  Future<void> initializeOrderHub() async {
    if (_isOrderHubConnected) return;

    try {
      final accessToken = await _encryptionService.getAccessToken();
      if (accessToken == null) {
        debugPrint(
          '⚠️  No access token found, skipping OrderHub initialization',
        );
        return;
      }

      final hubUrl = '${EnvironmentConfig.apiBaseUrl}/hubs/order';

      _orderHubConnection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(
              accessTokenFactory: () async => accessToken,
              logging: EnvironmentConfig.debugMode
                  ? (level, message) => debugPrint('OrderHub: $message')
                  : null,
            ),
          )
          .withAutomaticReconnect()
          .build();

      // Register event handlers
      _orderHubConnection!.on('OrderStatusUpdated', _handleOrderStatusUpdate);
      _orderHubConnection!.on('OrderTrackingInfo', _handleOrderTrackingInfo);
      _orderHubConnection!.on(
        'NotificationMarkedAsRead',
        _handleNotificationMarkedAsRead,
      );

      // Start connection
      await _orderHubConnection!.start();
      _isOrderHubConnected = true;
      debugPrint('✅ OrderHub connected successfully');
    } catch (e) {
      debugPrint('❌ OrderHub connection failed: $e');
      _isOrderHubConnected = false;
    }
  }

  /// Initialize Realtime Tracking Hub
  Future<void> initializeTrackingHub() async {
    if (_isTrackingHubConnected) return;

    try {
      final accessToken = await _encryptionService.getAccessToken();
      if (accessToken == null) return;

      final hubUrl = '${EnvironmentConfig.apiBaseUrl}/hubs/tracking';

      _trackingHubConnection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(
              accessTokenFactory: () async => accessToken,
              logging: EnvironmentConfig.debugMode
                  ? (level, message) => debugPrint('TrackingHub: $message')
                  : null,
            ),
          )
          .withAutomaticReconnect()
          .build();

      // Register event handlers
      _trackingHubConnection!.on('TrackingData', _handleTrackingData);
      _trackingHubConnection!.on('LocationUpdated', _handleLocationUpdate);
      _trackingHubConnection!.on('StatusUpdated', _handleStatusUpdate);
      _trackingHubConnection!.on('TrackingNotFound', _handleTrackingNotFound);

      // Start connection
      await _trackingHubConnection!.start();
      _isTrackingHubConnected = true;
      debugPrint('✅ TrackingHub connected successfully');
    } catch (e) {
      debugPrint('❌ TrackingHub connection failed: $e');
      _isTrackingHubConnected = false;
    }
  }

  /// Initialize Notification Hub
  Future<void> initializeNotificationHub() async {
    if (_isNotificationHubConnected) return;

    try {
      final accessToken = await _encryptionService.getAccessToken();
      if (accessToken == null) return;

      final hubUrl = '${EnvironmentConfig.apiBaseUrl}/hubs/notifications';

      _notificationHubConnection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(
              accessTokenFactory: () async => accessToken,
              logging: EnvironmentConfig.debugMode
                  ? (level, message) => debugPrint('NotificationHub: $message')
                  : null,
            ),
          )
          .withAutomaticReconnect()
          .build();

      // Register event handlers
      _notificationHubConnection!.on(
        'ReceiveNotification',
        _handleNotification,
      );
      _notificationHubConnection!.on(
        'UserNotifications',
        _handleUserNotifications,
      );

      // Start connection
      await _notificationHubConnection!.start();
      _isNotificationHubConnected = true;
      debugPrint('✅ NotificationHub connected successfully');
    } catch (e) {
      debugPrint('❌ NotificationHub connection failed: $e');
      _isNotificationHubConnected = false;
    }
  }

  /// Subscribe to order updates
  Future<void> subscribeToOrder(String orderId) async {
    if (!_isOrderHubConnected || _orderHubConnection == null) {
      await initializeOrderHub();
    }

    try {
      await _orderHubConnection?.invoke('SubscribeToOrder', args: [orderId]);
      debugPrint('📡 Subscribed to order: $orderId');
    } catch (e) {
      debugPrint('❌ Failed to subscribe to order: $e');
    }
  }

  /// Subscribe to order tracking
  Future<void> joinOrderTrackingGroup(String orderId) async {
    if (!_isTrackingHubConnected || _trackingHubConnection == null) {
      await initializeTrackingHub();
    }

    try {
      await _trackingHubConnection?.invoke(
        'JoinOrderTrackingGroup',
        args: [orderId],
      );
      debugPrint('📡 Joined order tracking group: $orderId');
    } catch (e) {
      debugPrint('❌ Failed to join tracking group: $e');
    }
  }

  /// Request current tracking data
  Future<void> requestTrackingData(String orderId) async {
    try {
      await _trackingHubConnection?.invoke(
        'RequestTrackingData',
        args: [orderId],
      );
      debugPrint('📡 Requested tracking data for order: $orderId');
    } catch (e) {
      debugPrint('❌ Failed to request tracking data: $e');
    }
  }

  /// Unsubscribe from order
  Future<void> unsubscribeFromOrder(String orderId) async {
    try {
      await _orderHubConnection?.invoke(
        'UnsubscribeFromOrder',
        args: [orderId],
      );
      debugPrint('📡 Unsubscribed from order: $orderId');
    } catch (e) {
      debugPrint('❌ Failed to unsubscribe from order: $e');
    }
  }

  /// Leave order tracking group
  Future<void> leaveOrderTrackingGroup(String orderId) async {
    try {
      await _trackingHubConnection?.invoke(
        'LeaveOrderTrackingGroup',
        args: [orderId],
      );
      debugPrint('📡 Left order tracking group: $orderId');
    } catch (e) {
      debugPrint('❌ Failed to leave tracking group: $e');
    }
  }

  /// Mark notification as read
  Future<void> markNotificationAsRead(String notificationId) async {
    try {
      await _orderHubConnection?.invoke(
        'MarkNotificationAsRead',
        args: [notificationId],
      );
      debugPrint('📡 Marked notification as read: $notificationId');
    } catch (e) {
      debugPrint('❌ Failed to mark notification as read: $e');
    }
  }

  // Event handlers
  void _handleOrderStatusUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final update = OrderStatusUpdate.fromJson(data);
      _orderStatusController.add(update);
      debugPrint('📬 Order status updated: ${update.orderId}');
    } catch (e) {
      debugPrint('❌ Failed to parse order status update: $e');
    }
  }

  void _handleOrderTrackingInfo(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      debugPrint('📬 Order tracking info received: $data');
    } catch (e) {
      debugPrint('❌ Failed to parse order tracking info: $e');
    }
  }

  void _handleTrackingData(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final tracking = TrackingData.fromJson(data);
      _trackingDataController.add(tracking);
      debugPrint('📬 Tracking data received: ${tracking.orderId}');
    } catch (e) {
      debugPrint('❌ Failed to parse tracking data: $e');
    }
  }

  void _handleLocationUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final location = LocationUpdate.fromJson(data);
      _locationUpdateController.add(location);
      debugPrint(
        '📍 Location updated: ${location.latitude}, ${location.longitude}',
      );
    } catch (e) {
      debugPrint('❌ Failed to parse location update: $e');
    }
  }

  void _handleStatusUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final tracking = TrackingData.fromJson(data);
      _trackingDataController.add(tracking);
      debugPrint('📬 Status updated: ${tracking.status}');
    } catch (e) {
      debugPrint('❌ Failed to parse status update: $e');
    }
  }

  void _handleTrackingNotFound(List<Object?>? arguments) {
    debugPrint('⚠️  Tracking not found for order');
  }

  void _handleNotification(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final notification = RealtimeNotification.fromJson(data);
      _notificationController.add(notification);
      debugPrint('🔔 Notification received: ${notification.message}');
    } catch (e) {
      debugPrint('❌ Failed to parse notification: $e');
    }
  }

  void _handleUserNotifications(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      debugPrint('📬 User notifications received');
    } catch (e) {
      debugPrint('❌ Failed to parse user notifications: $e');
    }
  }

  void _handleNotificationMarkedAsRead(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      debugPrint('✅ Notification marked as read');
    } catch (e) {
      debugPrint('❌ Failed to handle notification read: $e');
    }
  }

  /// Disconnect all hubs
  Future<void> disconnectAll() async {
    await _orderHubConnection?.stop();
    await _trackingHubConnection?.stop();
    await _notificationHubConnection?.stop();

    _isOrderHubConnected = false;
    _isTrackingHubConnected = false;
    _isNotificationHubConnected = false;

    debugPrint('🔌 All SignalR hubs disconnected');
  }

  /// Dispose service
  void dispose() {
    _orderStatusController.close();
    _trackingDataController.close();
    _locationUpdateController.close();
    _notificationController.close();
  }

  /// Check connection status
  bool get isConnected =>
      _isOrderHubConnected ||
      _isTrackingHubConnected ||
      _isNotificationHubConnected;
}

/// Order Status Update Model
class OrderStatusUpdate {
  final String orderId;
  final String status;
  final String message;
  final DateTime timestamp;

  OrderStatusUpdate({
    required this.orderId,
    required this.status,
    required this.message,
    required this.timestamp,
  });

  factory OrderStatusUpdate.fromJson(Map<String, dynamic> json) {
    return OrderStatusUpdate(
      orderId: json['orderId']?.toString() ?? '',
      status: json['status']?.toString() ?? '',
      message: json['message']?.toString() ?? '',
      timestamp: json['timestamp'] != null
          ? DateTime.parse(json['timestamp'])
          : DateTime.now(),
    );
  }
}

/// Tracking Data Model
class TrackingData {
  final String id;
  final String orderId;
  final String? courierId;
  final String status;
  final double? courierLatitude;
  final double? courierLongitude;
  final double? destinationLatitude;
  final double? destinationLongitude;
  final double? distanceFromDestination;
  final int? estimatedArrivalMinutes;
  final DateTime? estimatedArrivalTime;
  final DateTime lastUpdated;

  TrackingData({
    required this.id,
    required this.orderId,
    this.courierId,
    required this.status,
    this.courierLatitude,
    this.courierLongitude,
    this.destinationLatitude,
    this.destinationLongitude,
    this.distanceFromDestination,
    this.estimatedArrivalMinutes,
    this.estimatedArrivalTime,
    required this.lastUpdated,
  });

  factory TrackingData.fromJson(Map<String, dynamic> json) {
    return TrackingData(
      id: json['id']?.toString() ?? '',
      orderId: json['orderId']?.toString() ?? '',
      courierId: json['courierId']?.toString(),
      status: json['status']?.toString() ?? '',
      courierLatitude: json['courierLatitude']?.toDouble(),
      courierLongitude: json['courierLongitude']?.toDouble(),
      destinationLatitude: json['destinationLatitude']?.toDouble(),
      destinationLongitude: json['destinationLongitude']?.toDouble(),
      distanceFromDestination: json['distanceFromDestination']?.toDouble(),
      estimatedArrivalMinutes: json['estimatedArrivalMinutes']?.toInt(),
      estimatedArrivalTime: json['estimatedArrivalTime'] != null
          ? DateTime.parse(json['estimatedArrivalTime'])
          : null,
      lastUpdated: json['lastUpdated'] != null
          ? DateTime.parse(json['lastUpdated'])
          : DateTime.now(),
    );
  }
}

/// Location Update Model
class LocationUpdate {
  final double latitude;
  final double longitude;
  final String? address;
  final DateTime timestamp;

  LocationUpdate({
    required this.latitude,
    required this.longitude,
    this.address,
    required this.timestamp,
  });

  factory LocationUpdate.fromJson(Map<String, dynamic> json) {
    return LocationUpdate(
      latitude: json['latitude']?.toDouble() ?? 0.0,
      longitude: json['longitude']?.toDouble() ?? 0.0,
      address: json['address']?.toString(),
      timestamp: json['timestamp'] != null
          ? DateTime.parse(json['timestamp'])
          : DateTime.now(),
    );
  }
}

/// Realtime Notification Model
class RealtimeNotification {
  final String id;
  final String type;
  final String title;
  final String message;
  final Map<String, dynamic>? data;
  final DateTime timestamp;

  RealtimeNotification({
    required this.id,
    required this.type,
    required this.title,
    required this.message,
    this.data,
    required this.timestamp,
  });

  factory RealtimeNotification.fromJson(Map<String, dynamic> json) {
    return RealtimeNotification(
      id: json['id']?.toString() ?? '',
      type: json['type']?.toString() ?? '',
      title: json['title']?.toString() ?? '',
      message: json['message']?.toString() ?? '',
      data: json['data'] as Map<String, dynamic>?,
      timestamp: json['timestamp'] != null
          ? DateTime.parse(json['timestamp'])
          : DateTime.now(),
    );
  }
}
