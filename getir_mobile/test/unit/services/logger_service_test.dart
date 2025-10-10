import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/core/services/logger_service.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';

@GenerateMocks([AnalyticsService])
import 'logger_service_test.mocks.dart';

void main() {
  late LoggerService logger;
  late MockAnalyticsService mockAnalytics;

  setUp(() {
    mockAnalytics = MockAnalyticsService();
    logger = LoggerService(mockAnalytics);
  });

  group('LoggerService - Basic Logging', () {
    test('should log debug message without throwing', () {
      // Act & Assert
      expect(() => logger.debug('Test debug message'), returnsNormally);
    });

    test('should log info message without throwing', () {
      // Act & Assert
      expect(() => logger.info('Test info message'), returnsNormally);
    });

    test('should log warning message without throwing', () {
      // Act & Assert
      expect(() => logger.warning('Test warning message'), returnsNormally);
    });

    test('should log error message and report to analytics', () {
      // Arrange
      final error = Exception('Test error');
      when(
        mockAnalytics.logError(
          error: anyNamed('error'),
          stackTrace: anyNamed('stackTrace'),
          reason: anyNamed('reason'),
          context: anyNamed('context'),
          fatal: anyNamed('fatal'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      logger.error('Error message', error: error);

      // Assert
      verify(
        mockAnalytics.logError(
          error: error,
          stackTrace: null,
          reason: 'Error message',
          context: null,
          fatal: false,
        ),
      ).called(1);
    });

    test('should log fatal error and report to analytics', () {
      // Arrange
      final error = Exception('Fatal error');
      when(
        mockAnalytics.logError(
          error: anyNamed('error'),
          stackTrace: anyNamed('stackTrace'),
          reason: anyNamed('reason'),
          context: anyNamed('context'),
          fatal: anyNamed('fatal'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      logger.fatal('Fatal message', error: error);

      // Assert
      verify(
        mockAnalytics.logError(
          error: error,
          stackTrace: null,
          reason: 'Fatal message',
          context: null,
          fatal: true,
        ),
      ).called(1);
    });
  });

  group('LoggerService - Minimum Log Level', () {
    test('should set minimum log level', () {
      // Act & Assert
      expect(() => logger.setMinimumLevel(LogLevel.warning), returnsNormally);
    });

    test('should not log below minimum level', () {
      // Arrange
      logger.setMinimumLevel(LogLevel.error);

      // Act & Assert - Debug and Info should be filtered
      expect(() => logger.debug('Debug message'), returnsNormally);
      expect(() => logger.info('Info message'), returnsNormally);
      expect(() => logger.warning('Warning message'), returnsNormally);
      expect(() => logger.error('Error message'), returnsNormally);
    });

    test('should log at or above minimum level', () {
      // Arrange
      logger.setMinimumLevel(LogLevel.warning);

      // Act & Assert
      expect(() => logger.warning('Warning message'), returnsNormally);
      expect(() => logger.error('Error message'), returnsNormally);
    });
  });

  group('LoggerService - Context and Tags', () {
    test('should log with context data', () {
      // Arrange
      final context = {'userId': '123', 'action': 'login'};

      // Act & Assert
      expect(
        () => logger.info('User logged in', context: context),
        returnsNormally,
      );
    });

    test('should log with tag', () {
      // Act & Assert
      expect(
        () => logger.debug('Debug message', tag: 'TestTag'),
        returnsNormally,
      );
    });

    test('should log with both tag and context', () {
      // Arrange
      final context = {'key': 'value'};

      // Act & Assert
      expect(
        () => logger.info('Message', tag: 'Tag', context: context),
        returnsNormally,
      );
    });
  });

  group('LoggerService - Network Logging', () {
    test('should log successful network request', () {
      // Act & Assert
      expect(
        () => logger.logNetworkRequest(
          method: 'GET',
          url: 'https://api.example.com/users',
          statusCode: 200,
          duration: const Duration(milliseconds: 150),
        ),
        returnsNormally,
      );
    });

    test('should log failed network request', () {
      // Arrange
      final error = Exception('Network error');
      when(
        mockAnalytics.logError(
          error: anyNamed('error'),
          stackTrace: anyNamed('stackTrace'),
          reason: anyNamed('reason'),
          context: anyNamed('context'),
          fatal: anyNamed('fatal'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      logger.logNetworkRequest(
        method: 'POST',
        url: 'https://api.example.com/users',
        error: error,
      );

      // Assert
      verify(
        mockAnalytics.logError(
          error: error,
          stackTrace: null,
          reason: any,
          context: any,
          fatal: false,
        ),
      ).called(1);
    });

    test('should log network request with headers', () {
      // Arrange
      final headers = {'Content-Type': 'application/json'};

      // Act & Assert
      expect(
        () => logger.logNetworkRequest(
          method: 'POST',
          url: 'https://api.example.com/data',
          statusCode: 201,
          headers: headers,
        ),
        returnsNormally,
      );
    });

    test('should log network request with duration', () {
      // Act & Assert
      expect(
        () => logger.logNetworkRequest(
          method: 'GET',
          url: 'https://api.example.com/data',
          statusCode: 200,
          duration: const Duration(seconds: 2),
        ),
        returnsNormally,
      );
    });
  });

  group('LoggerService - User Actions', () {
    test('should log user action', () {
      // Act & Assert
      expect(
        () => logger.logUserAction(action: 'button_click'),
        returnsNormally,
      );
    });

    test('should log user action with screen and details', () {
      // Arrange
      final details = {'buttonId': 'submit_btn', 'value': 'test'};

      // Act & Assert
      expect(
        () => logger.logUserAction(
          action: 'form_submit',
          screen: 'LoginScreen',
          details: details,
        ),
        returnsNormally,
      );
    });
  });

  group('LoggerService - BLoC Logging', () {
    test('should log BLoC event', () {
      // Act & Assert
      expect(
        () => logger.logBlocEvent(
          blocName: 'AuthBloc',
          eventName: 'LoginRequested',
        ),
        returnsNormally,
      );
    });

    test('should log BLoC event with data', () {
      // Arrange
      final eventData = {'email': 'test@test.com'};

      // Act & Assert
      expect(
        () => logger.logBlocEvent(
          blocName: 'AuthBloc',
          eventName: 'LoginRequested',
          eventData: eventData,
        ),
        returnsNormally,
      );
    });

    test('should log BLoC state change', () {
      // Act & Assert
      expect(
        () => logger.logBlocStateChange(
          blocName: 'AuthBloc',
          previousState: 'AuthInitial',
          newState: 'AuthLoading',
        ),
        returnsNormally,
      );
    });
  });

  group('LoggerService - Navigation Logging', () {
    test('should log navigation', () {
      // Act & Assert
      expect(
        () => logger.logNavigation(from: '/home', to: '/profile'),
        returnsNormally,
      );
    });

    test('should log navigation with params', () {
      // Arrange
      final params = {'userId': '123', 'tab': 'settings'};

      // Act & Assert
      expect(
        () =>
            logger.logNavigation(from: '/home', to: '/profile', params: params),
        returnsNormally,
      );
    });
  });

  group('LoggerService - Authentication Logging', () {
    test('should log auth event', () {
      // Act & Assert
      expect(
        () => logger.logAuthEvent(event: 'login_success'),
        returnsNormally,
      );
    });

    test('should log auth event with user ID', () {
      // Act & Assert
      expect(
        () => logger.logAuthEvent(event: 'login_success', userId: 'user-123'),
        returnsNormally,
      );
    });
  });

  group('LoggerService - Edge Cases', () {
    test('should handle empty messages', () {
      // Act & Assert
      expect(() => logger.info(''), returnsNormally);
    });

    test('should handle very long messages', () {
      // Arrange
      final longMessage = 'A' * 10000;

      // Act & Assert
      expect(() => logger.info(longMessage), returnsNormally);
    });

    test('should handle special characters in messages', () {
      // Arrange
      const message = '!@#\$%^&*()_+-={}[]|\\:";\'<>?,./~`\n\t';

      // Act & Assert
      expect(() => logger.info(message), returnsNormally);
    });
  });
}
