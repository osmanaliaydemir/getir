import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/working_hours_datasource.dart';
import 'package:getir_mobile/data/repositories/working_hours_repository_impl.dart';
import 'package:getir_mobile/domain/entities/working_hours.dart';

import '../../helpers/mock_data.dart';
import 'working_hours_repository_impl_test.mocks.dart';

@GenerateMocks([WorkingHoursDataSource])
void main() {
  late WorkingHoursRepositoryImpl repository;
  late MockWorkingHoursDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockWorkingHoursDataSource();
    repository = WorkingHoursRepositoryImpl(mockDataSource);
  });

  final testWorkingHours = WorkingHours(
    id: 'wh-123',
    merchantId: 'merchant-123',
    dayOfWeek: 1,
    openTime: const TimeOfDay(hour: 9, minute: 0),
    closeTime: const TimeOfDay(hour: 17, minute: 0),
    isClosed: false,
    createdAt: MockData.now,
  );

  group('WorkingHoursRepositoryImpl -', () {
    group('getWorkingHoursByMerchant', () {
      const merchantId = 'merchant-123';

      test(
        'returns success with working hours when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.getWorkingHoursByMerchant(merchantId),
          ).thenAnswer((_) async => [testWorkingHours]);

          // Act
          final result = await repository.getWorkingHoursByMerchant(merchantId);

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, [testWorkingHours]);
          verify(
            mockDataSource.getWorkingHoursByMerchant(merchantId),
          ).called(1);
        },
      );

      test('returns empty list when no working hours', () async {
        // Arrange
        when(
          mockDataSource.getWorkingHoursByMerchant(merchantId),
        ).thenAnswer((_) async => []);

        // Act
        final result = await repository.getWorkingHoursByMerchant(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/working-hours/merchant/$merchantId',
          ),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.getWorkingHoursByMerchant(merchantId),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getWorkingHoursByMerchant(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns NotFoundException when merchant not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/working-hours/merchant/$merchantId',
          ),
          response: Response(
            requestOptions: RequestOptions(
              path: '/working-hours/merchant/$merchantId',
            ),
            statusCode: 404,
            data: {'message': 'Merchant not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.getWorkingHoursByMerchant(merchantId),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getWorkingHoursByMerchant(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('isMerchantOpen', () {
      const merchantId = 'merchant-123';

      test('returns success with true when merchant is open', () async {
        // Arrange
        when(
          mockDataSource.isMerchantOpen(
            merchantId,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenAnswer((_) async => true);

        // Act
        final result = await repository.isMerchantOpen(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, true);
      });

      test('returns success with false when merchant is closed', () async {
        // Arrange
        when(
          mockDataSource.isMerchantOpen(
            merchantId,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenAnswer((_) async => false);

        // Act
        final result = await repository.isMerchantOpen(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, false);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/working-hours/merchant/$merchantId/is-open',
          ),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.isMerchantOpen(
            merchantId,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.isMerchantOpen(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });

    group('getWorkingHoursById', () {
      const whId = 'wh-123';

      test(
        'returns success with working hours when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.getWorkingHoursById(whId),
          ).thenAnswer((_) async => testWorkingHours);

          // Act
          final result = await repository.getWorkingHoursById(whId);

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, testWorkingHours);
          verify(mockDataSource.getWorkingHoursById(whId)).called(1);
        },
      );

      test('returns NotFoundException when working hours not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/working-hours/$whId'),
          response: Response(
            requestOptions: RequestOptions(path: '/working-hours/$whId'),
            statusCode: 404,
            data: {'message': 'Working hours not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.getWorkingHoursById(whId)).thenThrow(dioException);

        // Act
        final result = await repository.getWorkingHoursById(whId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/working-hours/$whId'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.getWorkingHoursById(whId)).thenThrow(dioException);

        // Act
        final result = await repository.getWorkingHoursById(whId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });
  });
}
