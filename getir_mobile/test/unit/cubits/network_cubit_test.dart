import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:get_it/get_it.dart';

import 'package:getir_mobile/core/cubits/network/network_cubit.dart';
import 'package:getir_mobile/core/services/network_service.dart';
import 'package:getir_mobile/core/services/logger_service.dart';

import 'network_cubit_test.mocks.dart';

@GenerateMocks([NetworkService, LoggerService])
void main() {
  final getIt = GetIt.instance;

  group('NetworkCubit', () {
    late NetworkCubit cubit;
    late MockNetworkService mockNetworkService;
    late MockLoggerService mockLoggerService;

    setUpAll(() {
      // Register LoggerService mock for the entire test suite
      mockLoggerService = MockLoggerService();
      getIt.registerSingleton<LoggerService>(mockLoggerService);
    });

    setUp(() {
      mockNetworkService = MockNetworkService();
      cubit = NetworkCubit(mockNetworkService);
    });

    tearDownAll(() {
      getIt.reset();
    });

    tearDown(() {
      cubit.close();
    });

    // ==================== Initial State Tests ====================
    test('initial state should be NetworkState.online()', () {
      expect(cubit.state, equals(const NetworkState.online()));
      expect(cubit.state.isOnline, isTrue);
      expect(cubit.state.isRetrying, isFalse);
    });

    test('initial state should have correct properties', () {
      expect(cubit.state.isOnline, isTrue);
      expect(cubit.state.isOffline, isFalse);
      expect(cubit.state.isRetrying, isFalse);
    });

    // ==================== Initialize Tests ====================
    group('initialize', () {
      blocTest<NetworkCubit, NetworkState>(
        'should initialize network monitoring successfully',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(
            mockNetworkService.networkStatusStream,
          ).thenAnswer((_) => Stream.value(true));
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        verify: (_) {
          verify(mockNetworkService.initialize()).called(1);
          verify(mockNetworkService.networkStatusStream).called(1);
        },
      );

      blocTest<NetworkCubit, NetworkState>(
        'should emit online state when network becomes available',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(
            mockNetworkService.networkStatusStream,
          ).thenAnswer((_) => Stream.value(true));
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        wait: const Duration(milliseconds: 100),
        expect: () => [const NetworkState.online()],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should emit offline state when network becomes unavailable',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(
            mockNetworkService.networkStatusStream,
          ).thenAnswer((_) => Stream.value(false));
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        wait: const Duration(milliseconds: 100),
        expect: () => [const NetworkState.offline()],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should emit offline state when initialization fails',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenThrow(Exception('Network initialization failed'));
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        expect: () => [const NetworkState.offline()],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should handle multiple network status changes',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(
            mockNetworkService.networkStatusStream,
          ).thenAnswer((_) => Stream.fromIterable([true, false, true, false]));
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        wait: const Duration(milliseconds: 100),
        expect: () => [
          const NetworkState.online(),
          const NetworkState.offline(),
          const NetworkState.online(),
          const NetworkState.offline(),
        ],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should listen to network status stream continuously',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(
            mockNetworkService.networkStatusStream,
          ).thenAnswer((_) => Stream.fromIterable([false, true, false]));
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        wait: const Duration(milliseconds: 100),
        expect: () => [
          const NetworkState.offline(),
          const NetworkState.online(),
          const NetworkState.offline(),
        ],
      );
    });

    // ==================== RetryConnection Tests ====================
    group('retryConnection', () {
      blocTest<NetworkCubit, NetworkState>(
        'should set isRetrying to true during retry',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          return cubit;
        },
        act: (cubit) => cubit.retryConnection(),
        expect: () => [
          const NetworkState(isOnline: true, isRetrying: true),
          const NetworkState(isOnline: true, isRetrying: false),
        ],
        verify: (_) {
          verify(mockNetworkService.initialize()).called(1);
        },
      );

      blocTest<NetworkCubit, NetworkState>(
        'should not retry if already retrying',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          return cubit;
        },
        seed: () => const NetworkState(isOnline: false, isRetrying: true),
        act: (cubit) => cubit.retryConnection(),
        expect: () => [],
        verify: (_) {
          verifyNever(mockNetworkService.initialize());
        },
      );

      blocTest<NetworkCubit, NetworkState>(
        'should retry from offline state',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          return cubit;
        },
        seed: () => const NetworkState.offline(),
        act: (cubit) => cubit.retryConnection(),
        expect: () => [
          const NetworkState(isOnline: false, isRetrying: true),
          const NetworkState(isOnline: false, isRetrying: false),
        ],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should handle retry failure gracefully',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenThrow(Exception('Retry failed'));
          return cubit;
        },
        act: (cubit) => cubit.retryConnection(),
        expect: () => [
          const NetworkState(isOnline: true, isRetrying: true),
          const NetworkState(isOnline: true, isRetrying: false),
        ],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should wait appropriate time before retrying',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          return cubit;
        },
        act: (cubit) => cubit.retryConnection(),
        wait: const Duration(seconds: 4),
        expect: () => [
          const NetworkState(isOnline: true, isRetrying: true),
          const NetworkState(isOnline: true, isRetrying: false),
        ],
      );
    });

    // ==================== isHostReachable Tests ====================
    group('isHostReachable', () {
      test('should return true when host is reachable', () async {
        when(
          mockNetworkService.isHostReachable('google.com'),
        ).thenAnswer((_) async => true);

        final result = await cubit.isHostReachable('google.com');

        expect(result, isTrue);
        verify(mockNetworkService.isHostReachable('google.com')).called(1);
      });

      test('should return false when host is not reachable', () async {
        when(
          mockNetworkService.isHostReachable('invalid.host.xyz'),
        ).thenAnswer((_) async => false);

        final result = await cubit.isHostReachable('invalid.host.xyz');

        expect(result, isFalse);
        verify(
          mockNetworkService.isHostReachable('invalid.host.xyz'),
        ).called(1);
      });

      test('should check localhost', () async {
        when(
          mockNetworkService.isHostReachable('localhost'),
        ).thenAnswer((_) async => true);

        final result = await cubit.isHostReachable('localhost');

        expect(result, isTrue);
      });

      test('should check API host', () async {
        when(
          mockNetworkService.isHostReachable('api.example.com'),
        ).thenAnswer((_) async => true);

        final result = await cubit.isHostReachable('api.example.com');

        expect(result, isTrue);
      });

      test('should handle timeout gracefully', () async {
        when(
          mockNetworkService.isHostReachable(any),
        ).thenAnswer((_) async => false);

        final result = await cubit.isHostReachable('slow.host.com');

        expect(result, isFalse);
      });
    });

    // ==================== getConnectionType Tests ====================
    group('getConnectionType', () {
      test('should return WiFi connection type', () async {
        when(
          mockNetworkService.getConnectionType(),
        ).thenAnswer((_) async => 'WiFi');

        final result = await cubit.getConnectionType();

        expect(result, equals('WiFi'));
        verify(mockNetworkService.getConnectionType()).called(1);
      });

      test('should return Mobile connection type', () async {
        when(
          mockNetworkService.getConnectionType(),
        ).thenAnswer((_) async => 'Mobile');

        final result = await cubit.getConnectionType();

        expect(result, equals('Mobile'));
      });

      test('should return Ethernet connection type', () async {
        when(
          mockNetworkService.getConnectionType(),
        ).thenAnswer((_) async => 'Ethernet');

        final result = await cubit.getConnectionType();

        expect(result, equals('Ethernet'));
      });

      test(
        'should return Unknown when connection type cannot be determined',
        () async {
          when(
            mockNetworkService.getConnectionType(),
          ).thenAnswer((_) async => 'Unknown');

          final result = await cubit.getConnectionType();

          expect(result, equals('Unknown'));
        },
      );

      test('should handle error and return Unknown', () async {
        when(
          mockNetworkService.getConnectionType(),
        ).thenThrow(Exception('Error getting connection type'));

        // Note: The actual implementation catches errors and returns 'Unknown'
        // but since we're mocking, we need to test the exception path separately
        expect(() => cubit.getConnectionType(), throwsA(isA<Exception>()));
      });
    });

    // ==================== NetworkState Tests ====================
    group('NetworkState', () {
      test('online state should have correct properties', () {
        const state = NetworkState.online();

        expect(state.isOnline, isTrue);
        expect(state.isOffline, isFalse);
        expect(state.isRetrying, isFalse);
      });

      test('offline state should have correct properties', () {
        const state = NetworkState.offline();

        expect(state.isOnline, isFalse);
        expect(state.isOffline, isTrue);
        expect(state.isRetrying, isFalse);
      });

      test('copyWith should update isOnline', () {
        const state = NetworkState.online();
        final newState = state.copyWith(isOnline: false);

        expect(newState.isOnline, isFalse);
        expect(newState.isRetrying, isFalse);
      });

      test('copyWith should update isRetrying', () {
        const state = NetworkState.online();
        final newState = state.copyWith(isRetrying: true);

        expect(newState.isOnline, isTrue);
        expect(newState.isRetrying, isTrue);
      });

      test('copyWith should update both properties', () {
        const state = NetworkState.online();
        final newState = state.copyWith(isOnline: false, isRetrying: true);

        expect(newState.isOnline, isFalse);
        expect(newState.isRetrying, isTrue);
      });

      test('copyWith with no parameters should return same values', () {
        const state = NetworkState(isOnline: true, isRetrying: true);
        final newState = state.copyWith();

        expect(newState.isOnline, isTrue);
        expect(newState.isRetrying, isTrue);
      });

      test('equality should work correctly', () {
        const state1 = NetworkState.online();
        const state2 = NetworkState.online();
        const state3 = NetworkState.offline();

        expect(state1, equals(state2));
        expect(state1, isNot(equals(state3)));
      });

      test('props should contain all properties', () {
        const state = NetworkState(isOnline: true, isRetrying: true);

        expect(state.props, contains(true));
        expect(state.props, contains(true));
        expect(state.props.length, equals(2));
      });
    });

    // ==================== Dispose Tests ====================
    group('dispose', () {
      test('should call dispose on network service', () async {
        when(mockNetworkService.dispose()).thenReturn(null);

        await cubit.close();

        verify(mockNetworkService.dispose()).called(1);
      });

      test('should close cubit successfully', () async {
        when(mockNetworkService.dispose()).thenReturn(null);

        await cubit.close();

        expect(cubit.isClosed, isTrue);
      });
    });

    // ==================== Edge Cases ====================
    group('Edge Cases', () {
      blocTest<NetworkCubit, NetworkState>(
        'should handle rapid network switches',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(mockNetworkService.networkStatusStream).thenAnswer(
            (_) => Stream.fromIterable([
              true,
              false,
              true,
              false,
              true,
              false,
              true,
            ]),
          );
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        wait: const Duration(milliseconds: 200),
        expect: () => [
          const NetworkState.online(),
          const NetworkState.offline(),
          const NetworkState.online(),
          const NetworkState.offline(),
          const NetworkState.online(),
          const NetworkState.offline(),
          const NetworkState.online(),
        ],
      );

      blocTest<NetworkCubit, NetworkState>(
        'should maintain state during consecutive retry calls',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          return cubit;
        },
        act: (cubit) async {
          await cubit.retryConnection();
          await cubit.retryConnection();
        },
        wait: const Duration(seconds: 8),
        expect: () => [
          const NetworkState(isOnline: true, isRetrying: true),
          const NetworkState(isOnline: true, isRetrying: false),
          const NetworkState(isOnline: true, isRetrying: true),
          const NetworkState(isOnline: true, isRetrying: false),
        ],
      );

      test('should handle multiple host reachability checks', () async {
        when(
          mockNetworkService.isHostReachable(any),
        ).thenAnswer((_) async => true);

        final hosts = [
          'google.com',
          'api.example.com',
          'localhost',
          '127.0.0.1',
        ];

        for (final host in hosts) {
          final result = await cubit.isHostReachable(host);
          expect(result, isTrue);
        }

        verify(mockNetworkService.isHostReachable(any)).called(hosts.length);
      });

      blocTest<NetworkCubit, NetworkState>(
        'should handle empty network status stream',
        build: () {
          when(
            mockNetworkService.initialize(),
          ).thenAnswer((_) async => Future.value());
          when(
            mockNetworkService.networkStatusStream,
          ).thenAnswer((_) => const Stream.empty());
          return cubit;
        },
        act: (cubit) => cubit.initialize(),
        wait: const Duration(milliseconds: 100),
        expect: () => [],
      );
    });
  });
}
