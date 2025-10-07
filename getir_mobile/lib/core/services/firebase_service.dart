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

      if (kDebugMode) {
        print('Firebase initialized successfully');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Firebase initialization failed: $e');
      }
      rethrow;
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

    if (kDebugMode) {
      print('FCM Permission status: ${settings.authorizationStatus}');
    }

    // Request local notification permission
    await Permission.notification.request();
  }

  // Configure FCM
  static Future<void> _configureFCM() async {
    // Get FCM token
    final token = await messaging.getToken();
    if (kDebugMode) {
      print('FCM Token: $token');
    }
    if (token != null) {
      await _sendTokenToServer(token);
    }

    // Handle token refresh
    messaging.onTokenRefresh.listen((newToken) async {
      if (kDebugMode) {
        print('FCM Token refreshed: $newToken');
      }
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
      if (kDebugMode) {
        print('FCM Token sent to server: $token');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to send FCM token to server: $e');
      }
    }
  }

  // Handle foreground messages
  static Future<void> _handleForegroundMessage(RemoteMessage message) async {
    if (kDebugMode) {
      print('Received foreground message: ${message.messageId}');
    }

    // Show local notification for foreground messages
    await _showLocalNotification(message);
  }

  // Handle notification tap
  static Future<void> _handleNotificationTap(RemoteMessage message) async {
    if (kDebugMode) {
      print('Notification tapped: ${message.messageId}');
    }

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
      if (kDebugMode) {
        print('Notification tap routing failed: $e');
      }
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
    if (kDebugMode) {
      print('Local notification tapped: ${response.payload}');
    }
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
      if (kDebugMode) {
        print('Failed to route on local notification tap: $e');
      }
      AppRouter.router.go(RouteConstants.notifications);
    }
  }

  // Subscribe to topic
  static Future<void> subscribeToTopic(String topic) async {
    try {
      await messaging.subscribeToTopic(topic);
      if (kDebugMode) {
        print('Subscribed to topic: $topic');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to subscribe to topic $topic: $e');
      }
    }
  }

  // Unsubscribe from topic
  static Future<void> unsubscribeFromTopic(String topic) async {
    try {
      await messaging.unsubscribeFromTopic(topic);
      if (kDebugMode) {
        print('Unsubscribed from topic: $topic');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to unsubscribe from topic $topic: $e');
      }
    }
  }

  // Get FCM token
  static Future<String?> getFCMToken() async {
    try {
      return await messaging.getToken();
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get FCM token: $e');
      }
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
  if (kDebugMode) {
    print('Handling background message: ${message.messageId}');
  }
}
