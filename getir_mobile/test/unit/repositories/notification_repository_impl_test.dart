import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/notification_preferences_datasource.dart';
import 'package:getir_mobile/data/repositories/notification_repository_impl.dart';
import 'package:getir_mobile/domain/entities/notification_preferences.dart';

import 'notification_repository_impl_test.mocks.dart';

@GenerateMocks([NotificationPreferencesDataSource])
void main() {
  late NotificationRepositoryImpl repository;
  late MockNotificationPreferencesDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockNotificationPreferencesDataSource();
    repository = NotificationRepositoryImpl(mockDataSource);
  });

  const testPreferences = NotificationPreferences(
    orderUpdates: true,
    promotions: false,
    system: true,
    marketing: false,
  );

  group('NotificationRepositoryImpl -', () {
    group('getPreferences', () {
      test(
        'returns success with preferences when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.getPreferences(),
          ).thenAnswer((_) async => testPreferences);

          // Act
          final result = await repository.getPreferences();

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, testPreferences);
          verify(mockDataSource.getPreferences()).called(1);
        },
      );

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/notification-preferences'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.getPreferences()).thenThrow(dioException);

        // Act
        final result = await repository.getPreferences();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns NotFoundException when preferences not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/notification-preferences'),
          response: Response(
            requestOptions: RequestOptions(path: '/notification-preferences'),
            statusCode: 404,
            data: {'message': 'Preferences not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.getPreferences()).thenThrow(dioException);

        // Act
        final result = await repository.getPreferences();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });

      test('returns failure when AppException occurs', () async {
        // Arrange
        const appException = ApiException(message: 'Preferences error');
        when(mockDataSource.getPreferences()).thenThrow(appException);

        // Act
        final result = await repository.getPreferences();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });

    group('updatePreferences', () {
      test(
        'returns success with updated preferences when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.updatePreferences(testPreferences),
          ).thenAnswer((_) async => testPreferences);

          // Act
          final result = await repository.updatePreferences(testPreferences);

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, testPreferences);
          verify(mockDataSource.updatePreferences(testPreferences)).called(1);
        },
      );

      test('returns ValidationException when validation fails', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/notification-preferences'),
          response: Response(
            requestOptions: RequestOptions(path: '/notification-preferences'),
            statusCode: 400,
            data: {'message': 'Invalid preferences'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.updatePreferences(testPreferences),
        ).thenThrow(dioException);

        // Act
        final result = await repository.updatePreferences(testPreferences);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/notification-preferences'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.updatePreferences(testPreferences),
        ).thenThrow(dioException);

        // Act
        final result = await repository.updatePreferences(testPreferences);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns failure when AppException occurs', () async {
        // Arrange
        const appException = ApiException(message: 'Update error');
        when(
          mockDataSource.updatePreferences(testPreferences),
        ).thenThrow(appException);

        // Act
        final result = await repository.updatePreferences(testPreferences);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });
  });
}
