import 'dart:async';
import 'package:signalr_core/signalr_core.dart';
import '../constants/app_config.dart';

class SignalRService {
  final Map<String, HubConnection> _hubConnections = {};
  final Map<String, StreamController<Map<String, dynamic>>> _eventControllers = {};

  Future<void> startConnection(String hubName, {Future<String?> Function()? accessTokenFactory}) async {
    if (_hubConnections.containsKey(hubName)) {
      return;
    }

    final connection = HubConnectionBuilder()
        .withUrl(
          '${AppConfig.signalRUrl}/$hubName',
          HttpConnectionOptions(
            transport: HttpTransportType.webSockets,
            logging: (level, message) {},
            accessTokenFactory: accessTokenFactory,
          ),
        )
        .withAutomaticReconnect()
        .build();

    _hubConnections[hubName] = connection;

    connection.onclose((error) async {
      // Let automatic reconnect handle, but keep a log hook if needed
    });

    await connection.start();
  }

  Future<void> stopConnection(String hubName) async {
    final connection = _hubConnections.remove(hubName);
    if (connection != null) {
      await connection.stop();
    }
    final controller = _eventControllers.remove(hubName);
    await controller?.close();
  }

  void on(String hubName, String methodName) {
    final connection = _hubConnections[hubName];
    if (connection == null) return;

    _eventControllers.putIfAbsent(hubName, () => StreamController<Map<String, dynamic>>.broadcast());

    connection.on(methodName, (arguments) {
      if (arguments == null || arguments.isEmpty) return;
      final payload = arguments.first;
      if (payload is Map<String, dynamic>) {
        _eventControllers[hubName]!.add(payload);
      } else {
        _eventControllers[hubName]!.add({'data': payload});
      }
    });
  }

  Stream<Map<String, dynamic>> events(String hubName) {
    _eventControllers.putIfAbsent(hubName, () => StreamController<Map<String, dynamic>>.broadcast());
    return _eventControllers[hubName]!.stream;
  }

  Future<void> invoke(String hubName, String methodName, [List<Object?>? args]) async {
    final connection = _hubConnections[hubName];
    if (connection == null) return;
    await connection.invoke(methodName, args: args);
  }
}
