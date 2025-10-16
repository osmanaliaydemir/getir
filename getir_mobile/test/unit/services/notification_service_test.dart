import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/domain/entities/notification_preferences.dart';
import 'package:getir_mobile/domain/repositories/notification_repository.dart';
import 'package:getir_mobile/domain/services/notification_service.dart';

import 'notification_service_test.mocks.dart';

@GenerateMocks([INotificationRepository])
void main() {
  late NotificationService service;
  late MockINotificationRepository mockRepository;

  setUp(() {
    mockRepository = MockINotificationRepository();
    service = NotificationService(mockRepository);
  });

  const testPreferences = NotificationPreferences(
    orderUpdates: true,
    promotions: false,
    system: true,
    marketing: false,
  );

  group('NotificationService -', () {
    group('getPreferences', () {
      test('returns preferences when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.getPreferences(),
        ).thenAnswer((_) async => Result.success(testPreferences));

        // Act
        final result = await service.getPreferences();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testPreferences);
        verify(mockRepository.getPreferences()).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getPreferences(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getPreferences();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.getPreferences()).called(1);
      });

      test('returns failure when preferences not found', () async {
        // Arrange
        final exception = NotFoundException(message: 'Preferences not found');
        when(
          mockRepository.getPreferences(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getPreferences();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('updatePreferences', () {
      test('updates preferences when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.updatePreferences(testPreferences),
        ).thenAnswer((_) async => Result.success(testPreferences));

        // Act
        final result = await service.updatePreferences(testPreferences);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testPreferences);
        verify(mockRepository.updatePreferences(testPreferences)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = ValidationException(message: 'Invalid preferences');
        when(
          mockRepository.updatePreferences(testPreferences),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.updatePreferences(testPreferences);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.updatePreferences(testPreferences)).called(1);
      });

      test('returns failure when user not found', () async {
        // Arrange
        final exception = NotFoundException(message: 'User not found');
        when(
          mockRepository.updatePreferences(testPreferences),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.updatePreferences(testPreferences);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });
  });
}
