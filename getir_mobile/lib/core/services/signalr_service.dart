import 'dart:async';
import 'package:signalr_core/signalr_core.dart';
import '../config/environment_config.dart';
import 'secure_encryption_service.dart';
import 'logger_service.dart';

/// SignalR Connection State
enum SignalRConnectionState {
  disconnected,
  connecting,
  connected,
  reconnecting,
  failed,
}

/// SignalR Service for Real-time Communication
/// Manages connections to OrderHub, RealtimeTrackingHub, NotificationHub
class SignalRService {
  final SecureEncryptionService _encryptionService;

  SignalRService(this._encryptionService);

  HubConnection? _orderHubConnection;
  HubConnection? _trackingHubConnection;
  HubConnection? _notificationHubConnection;

  SignalRConnectionState _orderHubState = SignalRConnectionState.disconnected;
  SignalRConnectionState _trackingHubState =
      SignalRConnectionState.disconnected;
  SignalRConnectionState _notificationHubState =
      SignalRConnectionState.disconnected;

  // Event streams
  final _orderStatusController =
      StreamController<OrderStatusUpdate>.broadcast();
  final _trackingDataController = StreamController<TrackingData>.broadcast();
  final _locationUpdateController =
      StreamController<LocationUpdate>.broadcast();
  final _notificationController =
      StreamController<RealtimeNotification>.broadcast();

  // Connection state streams
  final _orderHubStateController =
      StreamController<SignalRConnectionState>.broadcast();
  final _trackingHubStateController =
      StreamController<SignalRConnectionState>.broadcast();
  final _notificationHubStateController =
      StreamController<SignalRConnectionState>.broadcast();

  Stream<OrderStatusUpdate> get orderStatusStream =>
      _orderStatusController.stream;
  Stream<TrackingData> get trackingDataStream => _trackingDataController.stream;
  Stream<LocationUpdate> get locationUpdateStream =>
      _locationUpdateController.stream;
  Stream<RealtimeNotification> get notificationStream =>
      _notificationController.stream;

  // Connection state streams
  Stream<SignalRConnectionState> get orderHubStateStream =>
      _orderHubStateController.stream;
  Stream<SignalRConnectionState> get trackingHubStateStream =>
      _trackingHubStateController.stream;
  Stream<SignalRConnectionState> get notificationHubStateStream =>
      _notificationHubStateController.stream;

  // Connection state getters
  SignalRConnectionState get orderHubState => _orderHubState;
  SignalRConnectionState get trackingHubState => _trackingHubState;
  SignalRConnectionState get notificationHubState => _notificationHubState;

  /// Initialize all SignalR hubs
  Future<void> initialize() async {
    await initializeOrderHub();
    await initializeTrackingHub();
    await initializeNotificationHub();
  }

  /// Initialize Order Hub
  Future<void> initializeOrderHub() async {
    if (_orderHubState == SignalRConnectionState.connected ||
        _orderHubState == SignalRConnectionState.connecting)
      return;

    _orderHubState = SignalRConnectionState.connecting;
    _orderHubStateController.add(_orderHubState);

    try {
      final accessToken = await _encryptionService.getAccessToken();
      if (accessToken == null) {
        logger.warning(
          'No access token found, skipping OrderHub initialization',
          tag: 'SignalR',
        );
        return;
      }

      final hubUrl = '${EnvironmentConfig.apiBaseUrl}/hubs/orders';

      _orderHubConnection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(
              accessTokenFactory: () async => accessToken,
              logging: EnvironmentConfig.debugMode
                  ? (level, message) => logger.debug(message, tag: 'OrderHub')
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
      _orderHubState = SignalRConnectionState.connected;
      _orderHubStateController.add(_orderHubState);
      logger.info('OrderHub connected successfully', tag: 'SignalR');
    } catch (e, stackTrace) {
      logger.warning(
        'OrderHub connection failed - continuing without real-time order updates',
        tag: 'SignalR',
        context: {'error': e.toString()},
      );
      _orderHubState = SignalRConnectionState.failed;
      _orderHubStateController.add(_orderHubState);
    }
  }

  /// Initialize Realtime Tracking Hub
  Future<void> initializeTrackingHub() async {
    if (_trackingHubState == SignalRConnectionState.connected ||
        _trackingHubState == SignalRConnectionState.connecting)
      return;

    _trackingHubState = SignalRConnectionState.connecting;
    _trackingHubStateController.add(_trackingHubState);

    try {
      final accessToken = await _encryptionService.getAccessToken();
      if (accessToken == null) return;

      final hubUrl = '${EnvironmentConfig.apiBaseUrl}/hubs/realtime-tracking';

      _trackingHubConnection = HubConnectionBuilder()
          .withUrl(
            hubUrl,
            HttpConnectionOptions(
              accessTokenFactory: () async => accessToken,
              logging: EnvironmentConfig.debugMode
                  ? (level, message) =>
                        logger.debug(message, tag: 'TrackingHub')
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
      _trackingHubState = SignalRConnectionState.connected;
      _trackingHubStateController.add(_trackingHubState);
      logger.info('TrackingHub connected successfully', tag: 'SignalR');
    } catch (e, stackTrace) {
      logger.warning(
        'TrackingHub connection failed - continuing without real-time tracking',
        tag: 'SignalR',
        context: {'error': e.toString()},
      );
      _trackingHubState = SignalRConnectionState.failed;
      _trackingHubStateController.add(_trackingHubState);
    }
  }

  /// Initialize Notification Hub
  Future<void> initializeNotificationHub() async {
    if (_notificationHubState == SignalRConnectionState.connected ||
        _notificationHubState == SignalRConnectionState.connecting)
      return;

    _notificationHubState = SignalRConnectionState.connecting;
    _notificationHubStateController.add(_notificationHubState);

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
                  ? (level, message) =>
                        logger.debug(message, tag: 'NotificationHub')
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
      _notificationHubState = SignalRConnectionState.connected;
      _notificationHubStateController.add(_notificationHubState);
      logger.info('NotificationHub connected successfully', tag: 'SignalR');
    } catch (e, stackTrace) {
      // Log the error but don't crash the app
      logger.warning(
        'NotificationHub connection failed - continuing without real-time notifications',
        tag: 'SignalR',
        context: {'error': e.toString()},
      );
      _notificationHubState = SignalRConnectionState.failed;
      _notificationHubStateController.add(_notificationHubState);

      // Don't rethrow - allow app to continue
    }
  }

  /// Subscribe to order updates
  Future<void> subscribeToOrder(String orderId) async {
    if (_orderHubState != SignalRConnectionState.connected ||
        _orderHubConnection == null) {
      await initializeOrderHub();
    }

    try {
      await _orderHubConnection?.invoke('SubscribeToOrder', args: [orderId]);
      logger.debug(
        'Subscribed to order',
        tag: 'SignalR',
        context: {'orderId': orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to subscribe to order',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
        context: {'orderId': orderId},
      );
    }
  }

  /// Subscribe to order tracking
  Future<void> joinOrderTrackingGroup(String orderId) async {
    if (_trackingHubState != SignalRConnectionState.connected ||
        _trackingHubConnection == null) {
      await initializeTrackingHub();
    }

    try {
      await _trackingHubConnection?.invoke(
        'JoinOrderTrackingGroup',
        args: [orderId],
      );
      logger.debug(
        'Joined order tracking group',
        tag: 'SignalR',
        context: {'orderId': orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to join tracking group',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
        context: {'orderId': orderId},
      );
    }
  }

  /// Request current tracking data
  Future<void> requestTrackingData(String orderId) async {
    try {
      await _trackingHubConnection?.invoke(
        'RequestTrackingData',
        args: [orderId],
      );
      logger.debug(
        'Requested tracking data',
        tag: 'SignalR',
        context: {'orderId': orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to request tracking data',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
        context: {'orderId': orderId},
      );
    }
  }

  /// Unsubscribe from order
  Future<void> unsubscribeFromOrder(String orderId) async {
    try {
      await _orderHubConnection?.invoke(
        'UnsubscribeFromOrder',
        args: [orderId],
      );
      logger.debug(
        'Unsubscribed from order',
        tag: 'SignalR',
        context: {'orderId': orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to unsubscribe from order',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
        context: {'orderId': orderId},
      );
    }
  }

  /// Leave order tracking group
  Future<void> leaveOrderTrackingGroup(String orderId) async {
    try {
      await _trackingHubConnection?.invoke(
        'LeaveOrderTrackingGroup',
        args: [orderId],
      );
      logger.debug(
        'Left order tracking group',
        tag: 'SignalR',
        context: {'orderId': orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to leave tracking group',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
        context: {'orderId': orderId},
      );
    }
  }

  /// Mark notification as read
  Future<void> markNotificationAsRead(String notificationId) async {
    try {
      await _orderHubConnection?.invoke(
        'MarkNotificationAsRead',
        args: [notificationId],
      );
      logger.debug(
        'Marked notification as read',
        tag: 'SignalR',
        context: {'notificationId': notificationId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to mark notification as read',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
        context: {'notificationId': notificationId},
      );
    }
  }

  // Event handlers
  void _handleOrderStatusUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final update = OrderStatusUpdate.fromJson(data);
      _orderStatusController.add(update);
      logger.debug(
        'Order status updated',
        tag: 'SignalR',
        context: {'orderId': update.orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse order status update',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleOrderTrackingInfo(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      logger.debug('Order tracking info received', tag: 'SignalR');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse order tracking info',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleTrackingData(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final tracking = TrackingData.fromJson(data);
      _trackingDataController.add(tracking);
      logger.debug(
        'Tracking data received',
        tag: 'SignalR',
        context: {'orderId': tracking.orderId},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse tracking data',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleLocationUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final location = LocationUpdate.fromJson(data);
      _locationUpdateController.add(location);
      logger.debug(
        'Location updated',
        tag: 'SignalR',
        context: {
          'latitude': location.latitude,
          'longitude': location.longitude,
        },
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse location update',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleStatusUpdate(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final tracking = TrackingData.fromJson(data);
      _trackingDataController.add(tracking);
      logger.debug(
        'Status updated',
        tag: 'SignalR',
        context: {'status': tracking.status},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse status update',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleTrackingNotFound(List<Object?>? arguments) {
    logger.warning('Tracking not found for order', tag: 'SignalR');
  }

  void _handleNotification(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      final data = arguments[0] as Map<String, dynamic>;
      final notification = RealtimeNotification.fromJson(data);
      _notificationController.add(notification);
      logger.debug(
        'Notification received',
        tag: 'SignalR',
        context: {'message': notification.message},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse notification',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleUserNotifications(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      logger.debug('User notifications received', tag: 'SignalR');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to parse user notifications',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  void _handleNotificationMarkedAsRead(List<Object?>? arguments) {
    if (arguments == null || arguments.isEmpty) return;

    try {
      logger.debug('Notification marked as read', tag: 'SignalR');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to handle notification read',
        tag: 'SignalR',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  /// Disconnect all hubs
  Future<void> disconnectAll() async {
    await _orderHubConnection?.stop();
    await _trackingHubConnection?.stop();
    await _notificationHubConnection?.stop();

    _orderHubState = SignalRConnectionState.disconnected;
    _trackingHubState = SignalRConnectionState.disconnected;
    _notificationHubState = SignalRConnectionState.disconnected;

    _orderHubStateController.add(_orderHubState);
    _trackingHubStateController.add(_trackingHubState);
    _notificationHubStateController.add(_notificationHubState);

    logger.info('All SignalR hubs disconnected', tag: 'SignalR');
  }

  /// Dispose service
  void dispose() {
    _orderStatusController.close();
    _trackingDataController.close();
    _locationUpdateController.close();
    _notificationController.close();
    _orderHubStateController.close();
    _trackingHubStateController.close();
    _notificationHubStateController.close();
  }

  /// Check connection status
  bool get isConnected =>
      _orderHubState == SignalRConnectionState.connected ||
      _trackingHubState == SignalRConnectionState.connected ||
      _notificationHubState == SignalRConnectionState.connected;
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
