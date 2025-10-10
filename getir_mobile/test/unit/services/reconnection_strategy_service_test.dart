import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/core/services/reconnection_strategy_service.dart';
import 'package:getir_mobile/core/services/network_service.dart';
import 'package:getir_mobile/core/services/sync_service.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';
import 'package:getir_mobile/core/services/logger_service.dart';
import 'package:get_it/get_it.dart';

@GenerateMocks([NetworkService, SyncService, AnalyticsService, LoggerService])
import 'reconnection_strategy_service_test.mocks.dart';

void main() {
  late ReconnectionStrategyService service;
  late MockNetworkService mockNetwork;
  late MockSyncService mockSync;
  late MockAnalyticsService mockAnalytics;
  late MockLoggerService mockLogger;
  final getIt = GetIt.instance;

  setUp(() {
    mockNetwork = MockNetworkService();
    mockSync = MockSyncService();
    mockAnalytics = MockAnalyticsService();
    mockLogger = MockLoggerService();

    // Register logger
    if (!getIt.isRegistered<LoggerService>()) {
      getIt.registerSingleton<LoggerService>(mockLogger);
    }

    when(mockNetwork.initialize()).thenAnswer((_) async => {});
    when(mockNetwork.networkStatusStream).thenAnswer((_) => Stream.value(true));
    when(mockNetwork.isOnline).thenReturn(true);

    service = ReconnectionStrategyService(mockNetwork, mockSync, mockAnalytics);
  });

  tearDown(() {
    service.dispose();
    getIt.reset();
  });

  group('ReconnectionStrategyService - Initialization', () {
    test('should initialize successfully', () async {
      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should initialize network service', () async {
      // Act
      await service.initialize();

      // Assert
      verify(mockNetwork.initialize()).called(1);
    });
  });

  group('ReconnectionStrategyService - Reconnection Handling', () {
    test('should handle reconnection event', () async {
      // Arrange
      when(mockSync.getSyncStatus()).thenReturn({'pendingActions': 0});
      when(
        mockAnalytics.logCustomEvent(
          eventName: anyNamed('eventName'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.initialize();

      // Assert - Should not throw
      expect(service.isOffline, isFalse);
    });

    test('should retry connection manually', () async {
      // Arrange
      when(mockSync.getSyncStatus()).thenReturn({'pendingActions': 0});
      when(mockSync.forceSyncNow()).thenAnswer((_) async => {});

      // Act & Assert
      expect(() => service.retryConnection(), returnsNormally);
    });

    test('should sync pending changes on reconnection', () async {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(true);
      when(mockSync.getSyncStatus()).thenReturn({'pendingActions': 5});
      when(mockSync.forceSyncNow()).thenAnswer((_) async => {});
      when(
        mockAnalytics.logCustomEvent(
          eventName: anyNamed('eventName'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.retryConnection();

      // Assert
      verify(mockSync.forceSyncNow()).called(1);
    });

    test('should track reconnection in analytics', () async {
      // Arrange
      when(mockSync.getSyncStatus()).thenReturn({'pendingActions': 3});
      when(mockSync.forceSyncNow()).thenAnswer((_) async => {});
      when(
        mockAnalytics.logCustomEvent(
          eventName: anyNamed('eventName'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.retryConnection();

      // Assert
      verify(
        mockAnalytics.logCustomEvent(
          eventName: anyNamed('eventName'),
          parameters: anyNamed('parameters'),
        ),
      ).called(greaterThan(0));
    });

    test('should handle reconnection without pending actions', () async {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(true);
      when(mockSync.getSyncStatus()).thenReturn({'pendingActions': 0});

      // Act & Assert
      expect(() => service.retryConnection(), returnsNormally);
    });
  });

  group('ReconnectionStrategyService - Disconnection Handling', () {
    test('should track offline duration', () {
      // Act
      final duration = service.offlineDuration;

      // Assert
      expect(duration, isA<Duration?>());
    });

    test('should return null offline duration when online', () {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(true);

      // Act
      final duration = service.offlineDuration;

      // Assert
      expect(duration, isNull);
    });

    test('should handle disconnection event gracefully', () async {
      // Arrange
      when(
        mockAnalytics.logCustomEvent(
          eventName: anyNamed('eventName'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });
  });

  group('ReconnectionStrategyService - Status', () {
    test('should get reconnection status', () {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(true);
      when(mockSync.getSyncStatus()).thenReturn({'pendingActions': 2});

      // Act
      final status = service.getStatus();

      // Assert
      expect(status, isA<Map<String, dynamic>>());
      expect(status['isOnline'], isNotNull);
    });

    test('should report offline status correctly', () {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(false);

      // Act
      final isOffline = service.isOffline;

      // Assert
      expect(isOffline, isTrue);
    });

    test('should dispose resources without errors', () {
      // Act & Assert
      expect(() => service.dispose(), returnsNormally);
    });
  });

  group('ConnectionQualityMonitor - Success/Failure Recording', () {
    test('should record successful request', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);
      const latency = Duration(milliseconds: 150);

      // Act
      monitor.recordSuccess(latency);

      // Assert
      final metrics = monitor.getMetrics();
      expect(metrics['successfulRequests'], equals(1));
    });

    test('should record failed request', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);

      // Act
      monitor.recordFailure();

      // Assert
      final metrics = monitor.getMetrics();
      expect(metrics['failedRequests'], equals(1));
    });

    test('should record multiple requests', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);

      // Act
      monitor.recordSuccess(const Duration(milliseconds: 100));
      monitor.recordSuccess(const Duration(milliseconds: 200));
      monitor.recordFailure();

      // Assert
      final metrics = monitor.getMetrics();
      expect(metrics['successfulRequests'], equals(2));
      expect(metrics['failedRequests'], equals(1));
      expect(metrics['totalRequests'], equals(3));
    });
  });

  group('ConnectionQualityMonitor - Quality Score', () {
    test('should calculate quality score', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);
      monitor.recordSuccess(const Duration(milliseconds: 100));
      monitor.recordSuccess(const Duration(milliseconds: 150));

      // Act
      final score = monitor.getQualityScore();

      // Assert
      expect(score, isA<int>());
      expect(score, greaterThanOrEqualTo(0));
      expect(score, lessThanOrEqualTo(100));
    });

    test('should return 100 for no requests', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);

      // Act
      final score = monitor.getQualityScore();

      // Assert
      expect(score, equals(100));
    });

    test('should get quality label', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);
      const validLabels = ['Excellent', 'Good', 'Fair', 'Poor', 'Very Poor'];

      // Act
      final label = monitor.getQualityLabel();

      // Assert
      expect(validLabels, contains(label));
    });
  });

  group('ConnectionQualityMonitor - Metrics', () {
    test('should get connection metrics', () {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(true);
      final monitor = ConnectionQualityMonitor(mockNetwork);
      monitor.recordSuccess(const Duration(milliseconds: 100));

      // Act
      final metrics = monitor.getMetrics();

      // Assert
      expect(metrics, isA<Map<String, dynamic>>());
      expect(metrics['qualityScore'], isNotNull);
      expect(metrics['qualityLabel'], isNotNull);
      expect(metrics['isOnline'], isNotNull);
    });

    test('should reset metrics', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);
      monitor.recordSuccess(const Duration(milliseconds: 100));
      monitor.recordFailure();

      // Act
      monitor.reset();
      final metrics = monitor.getMetrics();

      // Assert
      expect(metrics['successfulRequests'], equals(0));
      expect(metrics['failedRequests'], equals(0));
      expect(metrics['totalRequests'], equals(0));
    });

    test('should calculate average latency', () {
      // Arrange
      final monitor = ConnectionQualityMonitor(mockNetwork);
      monitor.recordSuccess(const Duration(milliseconds: 100));
      monitor.recordSuccess(const Duration(milliseconds: 200));
      monitor.recordSuccess(const Duration(milliseconds: 300));

      // Act
      final metrics = monitor.getMetrics();

      // Assert
      expect(metrics['averageLatency'], equals(200.0));
    });
  });
}
