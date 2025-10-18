import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/notifications_feed_datasource.dart';
import 'package:getir_mobile/data/models/notification_dto.dart';
import 'package:getir_mobile/data/repositories/notifications_feed_repository_impl.dart';
import 'package:getir_mobile/domain/entities/notification.dart';

import 'notifications_feed_repository_impl_test.mocks.dart';

@GenerateMocks([NotificationsFeedDataSource])
void main() {
  late NotificationsFeedRepositoryImpl repository;
  late MockNotificationsFeedDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockNotificationsFeedDataSource();
    repository = NotificationsFeedRepositoryImpl(mockDataSource);
  });

  final testNotificationDto = AppNotificationDto(
    id: 'notif-123',
    title: 'Order Update',
    body: 'Your order is on the way',
    type: 'order',
    isRead: false,
    createdAt: DateTime(2025, 1, 1),
    data: {},
  );

  group('NotificationsFeedRepositoryImpl -', () {
    group('getNotifications', () {
      test(
        'returns success with notifications when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.getNotifications(
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer((_) async => [testNotificationDto]);

          // Act
          final result = await repository.getNotifications();

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, isA<List<AppNotification>>());
          expect(result.data.length, 1);
        },
      );

      test('returns empty list when no notifications', () async {
        // Arrange
        when(
          mockDataSource.getNotifications(
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => []);

        // Act
        final result = await repository.getNotifications();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });

      test('supports pagination', () async {
        // Arrange
        when(
          mockDataSource.getNotifications(page: 2, pageSize: 10),
        ).thenAnswer((_) async => [testNotificationDto]);

        // Act
        final result = await repository.getNotifications(page: 2, pageSize: 10);

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockDataSource.getNotifications(page: 2, pageSize: 10),
        ).called(1);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/notifications'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.getNotifications(
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getNotifications();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns failure when AppException occurs', () async {
        // Arrange
        const appException = ApiException(message: 'Notification error');
        when(
          mockDataSource.getNotifications(
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenThrow(appException);

        // Act
        final result = await repository.getNotifications();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });

    group('markAsRead', () {
      const notificationId = 'notif-123';

      test('returns success when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.markAsRead(notificationId),
        ).thenAnswer((_) async => {});

        // Act
        final result = await repository.markAsRead(notificationId);

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.markAsRead(notificationId)).called(1);
      });

      test('returns NotFoundException when notification not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/notifications/$notificationId/read',
          ),
          response: Response(
            requestOptions: RequestOptions(
              path: '/notifications/$notificationId/read',
            ),
            statusCode: 404,
            data: {'message': 'Notification not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.markAsRead(notificationId)).thenThrow(dioException);

        // Act
        final result = await repository.markAsRead(notificationId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/notifications/$notificationId/read',
          ),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.markAsRead(notificationId)).thenThrow(dioException);

        // Act
        final result = await repository.markAsRead(notificationId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });
  });
}
