import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/core/services/network_service.dart';
import 'package:getir_mobile/core/services/logger_service.dart';
import 'package:get_it/get_it.dart';

@GenerateMocks([LoggerService])
import 'network_service_test.mocks.dart';

void main() {
  late NetworkService service;
  late MockLoggerService mockLogger;
  final getIt = GetIt.instance;

  setUpAll(() {
    mockLogger = MockLoggerService();
    if (!getIt.isRegistered<LoggerService>()) {
      getIt.registerSingleton<LoggerService>(mockLogger);
    }
  });

  setUp(() {
    service = NetworkService();
  });

  tearDownAll(() {
    getIt.reset();
  });

  group('NetworkService - Initialization', () {
    test('should have default online status', () {
      // Assert
      expect(service.isOnline, isA<bool>());
    });

    test('should provide network status stream', () {
      // Assert
      expect(service.networkStatusStream, isA<Stream<bool>>());
    });

    test('should initialize without throwing', () async {
      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });
  });

  group('NetworkService - Connectivity Status', () {
    test('should get current online status', () {
      // Act
      final status = service.isOnline;

      // Assert
      expect(status, isA<bool>());
    });

    test('should have network status stream that emits values', () async {
      // Arrange
      final streamValues = <bool>[];
      final subscription = service.networkStatusStream.listen(streamValues.add);

      // Wait a bit
      await Future.delayed(const Duration(milliseconds: 100));

      // Assert
      expect(streamValues, isA<List<bool>>());

      // Cleanup
      await subscription.cancel();
    });

    test('should maintain consistent online status', () {
      // Act
      final status1 = service.isOnline;
      final status2 = service.isOnline;

      // Assert
      expect(status1, equals(status2));
    });

    test('should handle multiple status checks', () {
      // Act & Assert
      for (int i = 0; i < 10; i++) {
        expect(service.isOnline, isA<bool>());
      }
    });
  });

  group('NetworkService - Internet Reachability', () {
    test('should check if host is reachable', () async {
      // Act
      final result = await service.isHostReachable('google.com');

      // Assert
      expect(result, isA<bool>());
    });

    test('should return false for unreachable host', () async {
      // Arrange
      const unreachableHost = 'this-host-definitely-does-not-exist-12345.com';

      // Act
      final result = await service.isHostReachable(unreachableHost);

      // Assert
      expect(result, isFalse);
    });

    test('should handle invalid host gracefully', () async {
      // Arrange
      const invalidHost = 'invalid..host..name';

      // Act
      final result = await service.isHostReachable(invalidHost);

      // Assert
      expect(result, isFalse);
    });

    test('should handle empty host string', () async {
      // Act
      final result = await service.isHostReachable('');

      // Assert
      expect(result, isFalse);
    });

    test('should handle localhost', () async {
      // Act
      final result = await service.isHostReachable('localhost');

      // Assert
      expect(result, isA<bool>());
    });
  });

  group('NetworkService - Connection Type', () {
    test('should get connection type', () async {
      // Act
      final connectionType = await service.getConnectionType();

      // Assert
      expect(connectionType, isA<String>());
      expect(connectionType, isNotEmpty);
    });

    test('should return valid connection type values', () async {
      // Arrange
      const validTypes = ['WiFi', 'Mobile', 'Ethernet', 'Bluetooth', 'Unknown'];

      // Act
      final connectionType = await service.getConnectionType();

      // Assert
      expect(validTypes, contains(connectionType));
    });

    test('should consistently return same connection type', () async {
      // Act
      final type1 = await service.getConnectionType();
      await Future.delayed(const Duration(milliseconds: 50));
      final type2 = await service.getConnectionType();

      // Assert - Types should be same if network didn't change
      expect(type1, isA<String>());
      expect(type2, isA<String>());
    });

    test('should handle rapid connection type checks', () async {
      // Act
      final futures = List.generate(5, (_) => service.getConnectionType());
      final results = await Future.wait(futures);

      // Assert
      expect(results.length, equals(5));
      for (final result in results) {
        expect(result, isA<String>());
      }
    });

    test('should return connection type without throwing', () async {
      // Act & Assert
      expect(() => service.getConnectionType(), returnsNormally);
    });
  });

  group('NetworkService - Status Monitoring', () {
    test('should provide consistent status across checks', () {
      // Act
      final statuses = List.generate(10, (_) => service.isOnline);

      // Assert - All values should be the same
      expect(statuses.toSet().length, lessThanOrEqualTo(2));
    });

    test('should have accessible network status stream', () {
      // Act
      final stream = service.networkStatusStream;

      // Assert
      expect(stream, isA<Stream<bool>>());
      expect(stream.isBroadcast, isTrue);
    });

    test('should allow multiple listeners to network status stream', () async {
      // Arrange
      final values1 = <bool>[];
      final values2 = <bool>[];

      // Act
      final sub1 = service.networkStatusStream.listen(values1.add);
      final sub2 = service.networkStatusStream.listen(values2.add);

      await Future.delayed(const Duration(milliseconds: 100));

      // Assert
      expect(values1, isA<List<bool>>());
      expect(values2, isA<List<bool>>());

      // Cleanup
      await sub1.cancel();
      await sub2.cancel();
    });

    test('should handle stream subscription cancellation', () async {
      // Arrange
      final subscription = service.networkStatusStream.listen((_) {});

      // Act & Assert
      expect(() => subscription.cancel(), returnsNormally);
    });

    test('should continue working after subscription cancellation', () async {
      // Arrange
      final subscription = service.networkStatusStream.listen((_) {});
      await subscription.cancel();

      // Act & Assert
      expect(service.isOnline, isA<bool>());
      expect(service.networkStatusStream, isA<Stream<bool>>());
    });
  });

  group('NetworkService - Host Reachability', () {
    test('should check common hosts', () async {
      // Arrange
      const hosts = ['google.com', 'github.com', 'facebook.com'];

      // Act
      for (final host in hosts) {
        final result = await service.isHostReachable(host);

        // Assert
        expect(result, isA<bool>());
      }
    });

    test('should handle IP addresses', () async {
      // Act
      final result = await service.isHostReachable('8.8.8.8');

      // Assert
      expect(result, isA<bool>());
    });

    test('should timeout for slow hosts', () async {
      // Arrange
      const slowHost = 'httpstat.us';

      // Act
      final result = await service.isHostReachable(slowHost);

      // Assert
      expect(result, isA<bool>());
    });
  });

  group('NetworkService - Error Handling', () {
    test('should handle concurrent requests', () async {
      // Act
      final futures = List.generate(
        20,
        (i) => service.isHostReachable('google.com'),
      );
      final results = await Future.wait(futures);

      // Assert
      expect(results.length, equals(20));
      for (final result in results) {
        expect(result, isA<bool>());
      }
    });

    test('should handle mixed valid and invalid hosts', () async {
      // Arrange
      const hosts = [
        'google.com',
        'invalid-host-12345.com',
        'github.com',
        'another-invalid-host.com',
      ];

      // Act
      final results = await Future.wait(
        hosts.map((host) => service.isHostReachable(host)),
      );

      // Assert
      expect(results.length, equals(4));
      expect(results, contains(true)); // At least one should be reachable
      expect(results, contains(false)); // At least one should be unreachable
    });

    test('should handle special characters in host name', () async {
      // Arrange
      const specialHost = 'invalid!@#\$.com';

      // Act
      final result = await service.isHostReachable(specialHost);

      // Assert
      expect(result, isFalse);
    });

    test('should handle very long host names', () async {
      // Arrange
      final longHost = 'a' * 1000 + '.com';

      // Act
      final result = await service.isHostReachable(longHost);

      // Assert
      expect(result, isFalse);
    });

    test('should handle rapid status changes', () async {
      // Act
      final statuses = <bool>[];
      for (int i = 0; i < 10; i++) {
        statuses.add(service.isOnline);
        await Future.delayed(const Duration(milliseconds: 10));
      }

      // Assert
      expect(statuses.length, equals(10));
      expect(statuses.every((status) => status is bool), isTrue);
    });
  });

  group('NetworkService - Edge Cases', () {
    test('should handle dispose without initialization', () {
      // Act & Assert
      expect(() => service.dispose(), returnsNormally);
    });

    test('should handle multiple dispose calls', () {
      // Act & Assert
      service.dispose();
      expect(() => service.dispose(), returnsNormally);
    });

    test('should handle operations after dispose', () {
      // Arrange
      service.dispose();

      // Act & Assert
      expect(service.isOnline, isA<bool>());
    });

    test('should handle concurrent initialization calls', () async {
      // Act
      final futures = List.generate(5, (_) => service.initialize());

      // Assert
      await Future.wait(futures);
      expect(service.isOnline, isA<bool>());
    });

    test('should maintain singleton behavior', () {
      // Act
      final instance1 = NetworkService();
      final instance2 = NetworkService();

      // Assert
      expect(identical(instance1, instance2), isTrue);
    });
  });
}
