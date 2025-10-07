import 'dart:async';
import 'package:flutter/widgets.dart';
import 'package:provider/provider.dart';
import '../../presentation/bloc/order/order_bloc.dart';
import 'signalr_service.dart';
import '../di/injection.dart';

/// Order Realtime Binder
/// Binds SignalR order status updates to OrderBloc
class OrderRealtimeBinder {
  static final OrderRealtimeBinder _instance = OrderRealtimeBinder._internal();
  factory OrderRealtimeBinder() => _instance;
  OrderRealtimeBinder._internal();

  bool _started = false;
  StreamSubscription<OrderStatusUpdate>? _subscription;
  StreamSubscription<TrackingData>? _trackingSubscription;

  /// Start listening to order updates
  Future<void> start(BuildContext context) async {
    if (_started) return;
    _started = true;

    final signalR = getIt<SignalRService>();

    // Initialize SignalR connections
    await signalR.initialize();

    // Listen to order status updates
    _subscription = signalR.orderStatusStream.listen((update) {
      final orderId = update.orderId;
      if (orderId.isEmpty) return;

      try {
        // Reload order when status changes
        context.read<OrderBloc>().add(LoadOrderById(orderId));
        debugPrint('📬 Order status updated: $orderId → ${update.status}');
      } catch (_) {
        // no-op if bloc not in tree
      }
    });

    // Listen to tracking data updates
    _trackingSubscription = signalR.trackingDataStream.listen((tracking) {
      debugPrint('📍 Tracking data updated: ${tracking.orderId}');
      // Additional tracking logic can be added here
    });

    debugPrint('✅ OrderRealtimeBinder started');
  }

  /// Stop listening to updates
  void stop() {
    _subscription?.cancel();
    _trackingSubscription?.cancel();
    _started = false;
    debugPrint('🛑 OrderRealtimeBinder stopped');
  }

  /// Check if binder is active
  bool get isActive => _started;
}
