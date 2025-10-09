import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/merchant_datasource.dart';
import 'package:getir_mobile/data/repositories/merchant_repository_impl.dart';

import '../../helpers/mock_data.dart';
import 'merchant_repository_impl_test.mocks.dart';

@GenerateMocks([MerchantDataSource])
void main() {
  late MerchantRepositoryImpl repository;
  late MockMerchantDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockMerchantDataSource();
    repository = MerchantRepositoryImpl(mockDataSource);
  });

  group('MerchantRepositoryImpl -', () {
    group('getMerchants', () {
      test('returns success with merchants when datasource succeeds', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockDataSource.getMerchants(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            search: anyNamed('search'),
            category: anyNamed('category'),
            latitude: anyNamed('latitude'),
            longitude: anyNamed('longitude'),
            radius: anyNamed('radius'),
          ),
        ).thenAnswer((_) async => merchants);

        // Act
        final result = await repository.getMerchants();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/merchants'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.getMerchants(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            search: anyNamed('search'),
            category: anyNamed('category'),
            latitude: anyNamed('latitude'),
            longitude: anyNamed('longitude'),
            radius: anyNamed('radius'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getMerchants();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });

    group('getMerchantById', () {
      const merchantId = 'merchant-123';

      test('returns success with merchant when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.getMerchantById(merchantId),
        ).thenAnswer((_) async => MockData.testMerchant);

        // Act
        final result = await repository.getMerchantById(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testMerchant);
        verify(mockDataSource.getMerchantById(merchantId)).called(1);
      });

      test('returns NotFoundException when merchant not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/merchants/$merchantId'),
          response: Response(
            requestOptions: RequestOptions(path: '/merchants/$merchantId'),
            statusCode: 404,
            data: {'message': 'Merchant not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.getMerchantById(merchantId),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getMerchantById(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('searchMerchants', () {
      const query = 'Pizza';

      test('returns success with merchants when datasource succeeds', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockDataSource.searchMerchants(query),
        ).thenAnswer((_) async => merchants);

        // Act
        final result = await repository.searchMerchants(query);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
        verify(mockDataSource.searchMerchants(query)).called(1);
      });

      test('returns empty list when no results found', () async {
        // Arrange
        when(mockDataSource.searchMerchants(query)).thenAnswer((_) async => []);

        // Act
        final result = await repository.searchMerchants(query);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/merchants/search'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.searchMerchants(query)).thenThrow(dioException);

        // Act
        final result = await repository.searchMerchants(query);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });

    group('getNearbyMerchants', () {
      const latitude = 41.0082;
      const longitude = 28.9784;
      const radius = 5.0;

      test('returns success with merchants when datasource succeeds', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockDataSource.getNearbyMerchants(
            latitude: latitude,
            longitude: longitude,
            radius: radius,
          ),
        ).thenAnswer((_) async => merchants);

        // Act
        final result = await repository.getNearbyMerchants(
          latitude: latitude,
          longitude: longitude,
          radius: radius,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
      });

      test('returns empty list when no nearby merchants', () async {
        // Arrange
        when(
          mockDataSource.getNearbyMerchants(
            latitude: latitude,
            longitude: longitude,
            radius: radius,
          ),
        ).thenAnswer((_) async => []);

        // Act
        final result = await repository.getNearbyMerchants(
          latitude: latitude,
          longitude: longitude,
          radius: radius,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/merchants/nearby'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.getNearbyMerchants(
            latitude: latitude,
            longitude: longitude,
            radius: radius,
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getNearbyMerchants(
          latitude: latitude,
          longitude: longitude,
          radius: radius,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });

    group('getNearbyMerchantsByCategory', () {
      const latitude = 41.0082;
      const longitude = 28.9784;
      const categoryType = 1;
      const radius = 5.0;

      test('returns success with merchants when datasource succeeds', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockDataSource.getNearbyMerchantsByCategory(
            latitude: latitude,
            longitude: longitude,
            categoryType: categoryType,
            radius: radius,
          ),
        ).thenAnswer((_) async => merchants);

        // Act
        final result = await repository.getNearbyMerchantsByCategory(
          latitude: latitude,
          longitude: longitude,
          categoryType: categoryType,
          radius: radius,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/merchants/nearby/category'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.getNearbyMerchantsByCategory(
            latitude: latitude,
            longitude: longitude,
            categoryType: categoryType,
            radius: radius,
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getNearbyMerchantsByCategory(
          latitude: latitude,
          longitude: longitude,
          categoryType: categoryType,
          radius: radius,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });
  });
}
