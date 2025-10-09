import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/domain/entities/working_hours.dart';
import 'package:getir_mobile/domain/repositories/working_hours_repository.dart';
import 'package:getir_mobile/domain/services/working_hours_service.dart';

import '../../helpers/mock_data.dart';
import 'working_hours_service_test.mocks.dart';

@GenerateMocks([WorkingHoursRepository])
void main() {
  late WorkingHoursService service;
  late MockWorkingHoursRepository mockRepository;

  setUp(() {
    mockRepository = MockWorkingHoursRepository();
    service = WorkingHoursService(mockRepository);
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

  group('WorkingHoursService -', () {
    group('getWorkingHours', () {
      const merchantId = 'merchant-123';

      test('returns working hours when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.getWorkingHoursByMerchant(merchantId),
        ).thenAnswer((_) async => Result.success([testWorkingHours]));

        // Act
        final result = await service.getWorkingHours(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, [testWorkingHours]);
        verify(mockRepository.getWorkingHoursByMerchant(merchantId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getWorkingHoursByMerchant(merchantId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getWorkingHours(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.getWorkingHoursByMerchant(merchantId)).called(1);
      });

      test('returns empty list when merchant has no working hours', () async {
        // Arrange
        when(
          mockRepository.getWorkingHoursByMerchant(merchantId),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.getWorkingHours(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });
    });

    group('checkIfMerchantOpen', () {
      const merchantId = 'merchant-123';

      test('returns true when merchant is open', () async {
        // Arrange
        when(
          mockRepository.isMerchantOpen(
            merchantId,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenAnswer((_) async => Result.success(true));

        // Act
        final result = await service.checkIfMerchantOpen(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, true);
      });

      test('returns false when merchant is closed', () async {
        // Arrange
        when(
          mockRepository.isMerchantOpen(
            merchantId,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenAnswer((_) async => Result.success(false));

        // Act
        final result = await service.checkIfMerchantOpen(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, false);
      });

      test('checks specific time when provided', () async {
        // Arrange
        final checkTime = DateTime(2025, 1, 1, 10, 0);
        when(
          mockRepository.isMerchantOpen(merchantId, checkTime: checkTime),
        ).thenAnswer((_) async => Result.success(true));

        // Act
        final result = await service.checkIfMerchantOpen(
          merchantId,
          checkTime: checkTime,
        );

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.isMerchantOpen(merchantId, checkTime: checkTime),
        ).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Merchant not found');
        when(
          mockRepository.isMerchantOpen(
            merchantId,
            checkTime: anyNamed('checkTime'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.checkIfMerchantOpen(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getWorkingHoursById', () {
      const whId = 'wh-123';

      test('returns working hours when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.getWorkingHoursById(whId),
        ).thenAnswer((_) async => Result.success(testWorkingHours));

        // Act
        final result = await service.getWorkingHoursById(whId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testWorkingHours);
        verify(mockRepository.getWorkingHoursById(whId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Working hours not found');
        when(
          mockRepository.getWorkingHoursById(whId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getWorkingHoursById(whId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.getWorkingHoursById(whId)).called(1);
      });
    });

    group('getNextOpenTime', () {
      const merchantId = 'merchant-123';

      test('returns null when calculation succeeds', () async {
        // Arrange
        when(
          mockRepository.getWorkingHoursByMerchant(merchantId),
        ).thenAnswer((_) async => Result.success([testWorkingHours]));

        // Act
        final result = await service.getNextOpenTime(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(
          result.data,
          null,
        ); // Currently returns null as per implementation
        verify(mockRepository.getWorkingHoursByMerchant(merchantId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Merchant not found');
        when(
          mockRepository.getWorkingHoursByMerchant(merchantId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getNextOpenTime(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });
  });
}
