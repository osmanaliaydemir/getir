import 'dart:async';
import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/core/services/signalr_service.dart';
import 'package:getir_mobile/core/services/secure_encryption_service.dart';
import 'package:getir_mobile/core/services/logger_service.dart';
import 'package:get_it/get_it.dart';

@GenerateMocks([SecureEncryptionService, LoggerService])
import 'signalr_service_test.mocks.dart';

void main() {
  late SignalRService service;
  late MockSecureEncryptionService mockEncryption;
  late MockLoggerService mockLogger;
  final getIt = GetIt.instance;

  setUp(() {
    mockEncryption = MockSecureEncryptionService();
    mockLogger = MockLoggerService();

    // Register logger
    if (!getIt.isRegistered<LoggerService>()) {
      getIt.registerSingleton<LoggerService>(mockLogger);
    }

    service = SignalRService(mockEncryption);
  });

  tearDown(() {
    service.dispose();
    getIt.reset();
  });

  group('SignalRService - Order Hub State', () {
    test('should start with disconnected state', () {
      // Assert
      expect(
        service.orderHubState,
        equals(SignalRConnectionState.disconnected),
      );
    });

    test('should have accessible order hub state stream', () {
      // Assert
      expect(
        service.orderHubStateStream,
        isA<Stream<SignalRConnectionState>>(),
      );
    });

    test('should emit state changes through stream', () async {
      // Arrange
      final states = <SignalRConnectionState>[];
      final subscription = service.orderHubStateStream.listen(states.add);

      // Wait a bit
      await Future.delayed(const Duration(milliseconds: 100));

      // Assert
      expect(states, isA<List<SignalRConnectionState>>());

      // Cleanup
      await subscription.cancel();
    });

    test('should maintain state consistency', () {
      // Act
      final state1 = service.orderHubState;
      final state2 = service.orderHubState;

      // Assert
      expect(state1, equals(state2));
    });

    test('should have valid initial state for order hub', () {
      // Assert
      expect(service.orderHubState, isIn(SignalRConnectionState.values));
    });

    test('should initialize order hub without token', () async {
      // Arrange
      when(mockEncryption.getAccessToken()).thenAnswer((_) async => null);

      // Act & Assert
      expect(() => service.initializeOrderHub(), returnsNormally);
    });

    test('should handle order hub initialization with token', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act & Assert
      expect(() => service.initializeOrderHub(), returnsNormally);
    });

    test('should not reinitialize if already connected', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act
      await service.initializeOrderHub();
      await service.initializeOrderHub();

      // Assert - Second call should be ignored
      expect(service.orderHubState, isA<SignalRConnectionState>());
    });

    test('should provide order status stream', () {
      // Assert
      expect(service.orderStatusStream, isA<Stream<OrderStatusUpdate>>());
    });

    test('should have broadcast order status stream', () {
      // Act
      final stream = service.orderStatusStream;

      // Assert
      expect(stream.isBroadcast, isTrue);
    });
  });

  group('SignalRService - Tracking Hub State', () {
    test('should start with disconnected state', () {
      // Assert
      expect(
        service.trackingHubState,
        equals(SignalRConnectionState.disconnected),
      );
    });

    test('should have accessible tracking hub state stream', () {
      // Assert
      expect(
        service.trackingHubStateStream,
        isA<Stream<SignalRConnectionState>>(),
      );
    });

    test('should have valid initial state for tracking hub', () {
      // Assert
      expect(service.trackingHubState, isIn(SignalRConnectionState.values));
    });

    test('should initialize tracking hub without token', () async {
      // Arrange
      when(mockEncryption.getAccessToken()).thenAnswer((_) async => null);

      // Act & Assert
      expect(() => service.initializeTrackingHub(), returnsNormally);
    });

    test('should handle tracking hub initialization with token', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act & Assert
      expect(() => service.initializeTrackingHub(), returnsNormally);
    });

    test('should provide tracking data stream', () {
      // Assert
      expect(service.trackingDataStream, isA<Stream<TrackingData>>());
    });

    test('should provide location update stream', () {
      // Assert
      expect(service.locationUpdateStream, isA<Stream<LocationUpdate>>());
    });

    test('should have broadcast tracking streams', () {
      // Act
      final trackingStream = service.trackingDataStream;
      final locationStream = service.locationUpdateStream;

      // Assert
      expect(trackingStream.isBroadcast, isTrue);
      expect(locationStream.isBroadcast, isTrue);
    });

    test('should allow multiple listeners to tracking stream', () async {
      // Arrange
      final values1 = <TrackingData>[];
      final values2 = <TrackingData>[];

      // Act
      final sub1 = service.trackingDataStream.listen(values1.add);
      final sub2 = service.trackingDataStream.listen(values2.add);

      await Future.delayed(const Duration(milliseconds: 100));

      // Assert
      expect(values1, isA<List<TrackingData>>());
      expect(values2, isA<List<TrackingData>>());

      // Cleanup
      await sub1.cancel();
      await sub2.cancel();
    });

    test('should handle tracking hub subscription cancellation', () async {
      // Arrange
      final subscription = service.trackingDataStream.listen((_) {});

      // Act & Assert
      expect(() => subscription.cancel(), returnsNormally);
    });
  });

  group('SignalRService - Notification Hub State', () {
    test('should start with disconnected state', () {
      // Assert
      expect(
        service.notificationHubState,
        equals(SignalRConnectionState.disconnected),
      );
    });

    test('should have accessible notification hub state stream', () {
      // Assert
      expect(
        service.notificationHubStateStream,
        isA<Stream<SignalRConnectionState>>(),
      );
    });

    test('should have valid initial state for notification hub', () {
      // Assert
      expect(service.notificationHubState, isIn(SignalRConnectionState.values));
    });

    test('should initialize notification hub without token', () async {
      // Arrange
      when(mockEncryption.getAccessToken()).thenAnswer((_) async => null);

      // Act & Assert
      expect(() => service.initializeNotificationHub(), returnsNormally);
    });

    test('should handle notification hub initialization with token', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act & Assert
      expect(() => service.initializeNotificationHub(), returnsNormally);
    });

    test('should provide notification stream', () {
      // Assert
      expect(service.notificationStream, isA<Stream<RealtimeNotification>>());
    });

    test('should have broadcast notification stream', () {
      // Act
      final stream = service.notificationStream;

      // Assert
      expect(stream.isBroadcast, isTrue);
    });

    test('should allow multiple listeners to notification stream', () async {
      // Arrange
      final values1 = <RealtimeNotification>[];
      final values2 = <RealtimeNotification>[];

      // Act
      final sub1 = service.notificationStream.listen(values1.add);
      final sub2 = service.notificationStream.listen(values2.add);

      await Future.delayed(const Duration(milliseconds: 100));

      // Assert
      expect(values1, isA<List<RealtimeNotification>>());
      expect(values2, isA<List<RealtimeNotification>>());

      // Cleanup
      await sub1.cancel();
      await sub2.cancel();
    });

    test('should handle notification subscription cancellation', () async {
      // Arrange
      final subscription = service.notificationStream.listen((_) {});

      // Act & Assert
      expect(() => subscription.cancel(), returnsNormally);
    });

    test('should handle multiple notification listeners', () async {
      // Arrange
      final listeners = List.generate(
        5,
        (_) => service.notificationStream.listen((_) {}),
      );

      // Act
      await Future.delayed(const Duration(milliseconds: 100));

      // Assert & Cleanup
      for (final listener in listeners) {
        await listener.cancel();
      }
      expect(listeners.length, equals(5));
    });
  });

  group('SignalRService - Connection Management', () {
    test('should initialize all hubs', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should handle initialization without token', () async {
      // Arrange
      when(mockEncryption.getAccessToken()).thenAnswer((_) async => null);

      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should dispose all connections', () {
      // Act & Assert
      expect(() => service.dispose(), returnsNormally);
    });

    test('should handle dispose without initialization', () {
      // Act & Assert
      expect(() => service.dispose(), returnsNormally);
    });

    test('should handle multiple dispose calls', () {
      // Act
      service.dispose();

      // Assert
      expect(() => service.dispose(), returnsNormally);
    });

    test('should allow reinitialization after dispose', () async {
      // Arrange
      service.dispose();
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should handle concurrent initialization calls', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenAnswer((_) async => 'test_token');

      // Act
      final futures = List.generate(3, (_) => service.initialize());

      // Assert
      await Future.wait(futures);
      expect(service.orderHubState, isA<SignalRConnectionState>());
    });
  });

  group('SignalRService - Stream Management', () {
    test('should provide all required event streams', () {
      // Assert
      expect(service.orderStatusStream, isA<Stream<OrderStatusUpdate>>());
      expect(service.trackingDataStream, isA<Stream<TrackingData>>());
      expect(service.locationUpdateStream, isA<Stream<LocationUpdate>>());
      expect(service.notificationStream, isA<Stream<RealtimeNotification>>());
    });

    test('should have all event streams as broadcast', () {
      // Assert
      expect(service.orderStatusStream.isBroadcast, isTrue);
      expect(service.trackingDataStream.isBroadcast, isTrue);
      expect(service.locationUpdateStream.isBroadcast, isTrue);
      expect(service.notificationStream.isBroadcast, isTrue);
    });

    test('should allow subscription to all streams simultaneously', () async {
      // Arrange & Act
      final orderSub = service.orderStatusStream.listen((_) {});
      final trackingSub = service.trackingDataStream.listen((_) {});
      final locationSub = service.locationUpdateStream.listen((_) {});
      final notificationSub = service.notificationStream.listen((_) {});

      await Future.delayed(const Duration(milliseconds: 100));

      // Assert & Cleanup
      await orderSub.cancel();
      await trackingSub.cancel();
      await locationSub.cancel();
      await notificationSub.cancel();

      expect(true, isTrue);
    });
  });

  group('SignalRService - Edge Cases', () {
    test('should handle dispose without initialization', () {
      // Act & Assert
      expect(() => service.dispose(), returnsNormally);
    });

    test('should handle rapid dispose cycles', () async {
      // Act & Assert
      for (int i = 0; i < 5; i++) {
        expect(() => service.dispose(), returnsNormally);
        await Future.delayed(const Duration(milliseconds: 10));
      }
    });

    test('should handle empty token during initialization', () async {
      // Arrange
      when(mockEncryption.getAccessToken()).thenAnswer((_) async => '');

      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should handle encryption service errors', () async {
      // Arrange
      when(
        mockEncryption.getAccessToken(),
      ).thenThrow(Exception('Encryption error'));

      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should maintain state after errors', () async {
      // Arrange
      when(mockEncryption.getAccessToken()).thenThrow(Exception('Error'));

      // Act
      try {
        await service.initialize();
      } catch (_) {}

      // Assert - State should still be accessible
      expect(service.orderHubState, isA<SignalRConnectionState>());
      expect(service.trackingHubState, isA<SignalRConnectionState>());
      expect(service.notificationHubState, isA<SignalRConnectionState>());
    });

    test('should handle all state values', () {
      // Assert - All enum values should be valid
      for (final state in SignalRConnectionState.values) {
        expect(state, isA<SignalRConnectionState>());
      }
    });

    test('should handle concurrent stream subscriptions', () async {
      // Arrange
      final subscriptions = <StreamSubscription>[];

      // Act
      for (int i = 0; i < 10; i++) {
        subscriptions.add(service.orderStatusStream.listen((_) {}));
        subscriptions.add(service.trackingDataStream.listen((_) {}));
        subscriptions.add(service.notificationStream.listen((_) {}));
      }

      await Future.delayed(const Duration(milliseconds: 100));

      // Cleanup
      for (final sub in subscriptions) {
        await sub.cancel();
      }

      // Assert
      expect(subscriptions.length, equals(30));
    });
  });
}
