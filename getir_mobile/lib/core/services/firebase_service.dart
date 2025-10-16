import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:dio/dio.dart';
import 'dart:convert';
import '../../core/navigation/app_router.dart';
import '../../core/constants/route_constants.dart';
import '../di/injection.dart';
import 'logger_service.dart';

class FirebaseService {
  static final FirebaseService _instance = FirebaseService._internal();
  factory FirebaseService() => _instance;
  FirebaseService._internal();

  static FirebaseMessaging get messaging => FirebaseMessaging.instance;
  static FlutterLocalNotificationsPlugin get localNotifications =>
      FlutterLocalNotificationsPlugin();

  // Initialize Firebase and FCM
  static Future<void> initialize() async {
    try {
      // Initialize Firebase
      await Firebase.initializeApp();

      // Initialize local notifications
      await _initializeLocalNotifications();

      // Request notification permissions
      await _requestNotificationPermissions();

      // Configure FCM
      await _configureFCM();

      debugPrint('✅ [Firebase] Firebase initialized successfully');
    } catch (e, stackTrace) {
      // ⚠️ TEMPORARY: Allow app to run without Firebase for development
      debugPrint('⚠️ [Firebase] Firebase initialization failed: $e');
      debugPrint('💡 [Firebase] App will continue without Firebase features');
      debugPrint(
        '📝 [Firebase] To fix: Add google-services.json to android/app/',
      );
      // DON'T rethrow - allow app to continue without Firebase
    }
  }

  // Initialize local notifications
  static Future<void> _initializeLocalNotifications() async {
    const AndroidInitializationSettings initializationSettingsAndroid =
        AndroidInitializationSettings('@mipmap/ic_launcher');

    const DarwinInitializationSettings initializationSettingsIOS =
        DarwinInitializationSettings(
          requestAlertPermission: true,
          requestBadgePermission: true,
          requestSoundPermission: true,
        );

    const InitializationSettings initializationSettings =
        InitializationSettings(
          android: initializationSettingsAndroid,
          iOS: initializationSettingsIOS,
        );

    await localNotifications.initialize(
      initializationSettings,
      onDidReceiveNotificationResponse: _onNotificationTapped,
    );
  }

  // Request notification permissions
  static Future<void> _requestNotificationPermissions() async {
    // Request FCM permission
    final settings = await messaging.requestPermission(
      alert: true,
      announcement: false,
      badge: true,
      carPlay: false,
      criticalAlert: false,
      provisional: false,
      sound: true,
    );

    logger.debug(
      'FCM permission status: ${settings.authorizationStatus}',
      tag: 'FCM',
    );

    // Request local notification permission
    await Permission.notification.request();
  }

  // Configure FCM
  static Future<void> _configureFCM() async {
    // Get FCM token
    final token = await messaging.getToken();
    logger.debug('FCM token obtained', tag: 'FCM');
    if (token != null) {
      await _sendTokenToServer(token);
    }

    // Handle token refresh
    messaging.onTokenRefresh.listen((newToken) async {
      logger.debug('FCM token refreshed', tag: 'FCM');
      await _sendTokenToServer(newToken);
    });

    // Handle background messages
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);

    // Handle foreground messages
    FirebaseMessaging.onMessage.listen(_handleForegroundMessage);

    // Handle notification tap when app is in background
    FirebaseMessaging.onMessageOpenedApp.listen(_handleNotificationTap);

    // Handle notification tap when app is terminated
    final initialMessage = await messaging.getInitialMessage();
    if (initialMessage != null) {
      _handleNotificationTap(initialMessage);
    }
  }

  // Send FCM token to server
  static Future<void> _sendTokenToServer(String token) async {
    try {
      // Get Dio from DI instead of ApiClient singleton
      final dio = getIt<Dio>();
      await dio.post('/api/v1/notifications/token', data: {'token': token});
      logger.debug('FCM token sent to server', tag: 'FCM');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to send FCM token to server',
        tag: 'FCM',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  // Handle foreground messages
  static Future<void> _handleForegroundMessage(RemoteMessage message) async {
    logger.debug(
      'Received foreground message: ${message.messageId}',
      tag: 'FCM',
    );

    // Show local notification for foreground messages
    await _showLocalNotification(message);
  }

  // Handle notification tap
  static Future<void> _handleNotificationTap(RemoteMessage message) async {
    logger.debug('Notification tapped: ${message.messageId}', tag: 'FCM');

    final data = message.data;
    try {
      final type = (data['type'] ?? data['notification_type'] ?? '').toString();
      switch (type) {
        case 'order_update':
          // Prefer order detail/tracking if orderId exists; otherwise go to orders list
          final orderId = (data['orderId'] ?? data['order_id'] ?? '')
              .toString();
          if (orderId.isNotEmpty) {
            AppRouter.router.go('/order/$orderId');
          } else {
            AppRouter.router.go(RouteConstants.orders);
          }
          break;
        case 'promotion':
          // Route to notifications list (promotions surface there)
          AppRouter.router.go(RouteConstants.notifications);
          break;
        case 'general':
        default:
          AppRouter.router.go(RouteConstants.notifications);
          break;
      }
    } catch (e) {
      logger.error('Notification tap routing failed', tag: 'FCM', error: e);
      // Safe fallback
      AppRouter.router.go(RouteConstants.notifications);
    }
  }

  // Show local notification
  static Future<void> _showLocalNotification(RemoteMessage message) async {
    final notification = message.notification;
    if (notification == null) return;

    const AndroidNotificationDetails androidDetails =
        AndroidNotificationDetails(
          'getir_notifications',
          'Getir Notifications',
          channelDescription: 'Notifications from Getir app',
          importance: Importance.high,
          priority: Priority.high,
          icon: '@mipmap/ic_launcher',
        );

    const DarwinNotificationDetails iosDetails = DarwinNotificationDetails(
      presentAlert: true,
      presentBadge: true,
      presentSound: true,
    );

    const NotificationDetails platformDetails = NotificationDetails(
      android: androidDetails,
      iOS: iosDetails,
    );

    await localNotifications.show(
      message.hashCode,
      notification.title,
      notification.body,
      platformDetails,
      payload: jsonEncode(message.data),
    );
  }

  // Handle notification tap (local)
  static void _onNotificationTapped(NotificationResponse response) {
    logger.debug('Local notification tapped: ${response.payload}', tag: 'FCM');
    try {
      final payload = response.payload;
      if (payload == null || payload.isEmpty) {
        AppRouter.router.go(RouteConstants.notifications);
        return;
      }
      final Map<String, dynamic> data =
          jsonDecode(payload) as Map<String, dynamic>;
      final type = (data['type'] ?? data['notification_type'] ?? '').toString();
      switch (type) {
        case 'order_update':
          final orderId = (data['orderId'] ?? data['order_id'] ?? '')
              .toString();
          if (orderId.isNotEmpty) {
            AppRouter.router.go('/order/$orderId');
          } else {
            AppRouter.router.go(RouteConstants.orders);
          }
          break;
        case 'promotion':
          AppRouter.router.go(RouteConstants.notifications);
          break;
        case 'general':
        default:
          AppRouter.router.go(RouteConstants.notifications);
          break;
      }
    } catch (e) {
      logger.error(
        'Failed to route on local notification tap',
        tag: 'FCM',
        error: e,
      );
      AppRouter.router.go(RouteConstants.notifications);
    }
  }

  // Subscribe to topic
  static Future<void> subscribeToTopic(String topic) async {
    try {
      await messaging.subscribeToTopic(topic);
      logger.debug('Subscribed to topic: $topic', tag: 'FCM');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to subscribe to topic $topic',
        tag: 'FCM',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  // Unsubscribe from topic
  static Future<void> unsubscribeFromTopic(String topic) async {
    try {
      await messaging.unsubscribeFromTopic(topic);
      logger.debug('Unsubscribed from topic: $topic', tag: 'FCM');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to unsubscribe from topic $topic',
        tag: 'FCM',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  // Get FCM token
  static Future<String?> getFCMToken() async {
    try {
      return await messaging.getToken();
    } catch (e) {
      logger.error('Failed to get FCM token', tag: 'FCM', error: e);
      return null;
    }
  }

  // Clear all notifications
  static Future<void> clearAllNotifications() async {
    await localNotifications.cancelAll();
  }

  // Clear specific notification
  static Future<void> clearNotification(int id) async {
    await localNotifications.cancel(id);
  }
}

// Background message handler (must be top-level function)
@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  await Firebase.initializeApp();
  // Background handler - minimal logging
  if (kDebugMode) {
    logger.debug(
      'Handling background message',
      tag: 'FCM',
      context: {'messageId': message.messageId},
    );
  }
}
