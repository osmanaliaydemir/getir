import 'package:flutter/widgets.dart';
import 'package:provider/provider.dart';
import '../../presentation/bloc/order/order_bloc.dart';
import 'signalr_service.dart';

class OrderRealtimeBinder {
  static final OrderRealtimeBinder _instance = OrderRealtimeBinder._internal();
  factory OrderRealtimeBinder() => _instance;
  OrderRealtimeBinder._internal();

  bool _started = false;

  Future<void> start(BuildContext context) async {
    if (_started) return;
    _started = true;

    final signalR = SignalRService();
    await signalR.startConnection('orderHub');
    signalR.on('orderHub', 'OrderStatusUpdated');
    signalR.events('orderHub').listen((event) {
      final orderId = (event['orderId'] ?? '').toString();
      if (orderId.isEmpty) return;
      try {
        context.read<OrderBloc>().add(LoadOrderById(orderId));
      } catch (_) {
        // no-op if bloc not in tree
      }
    });
  }
}
