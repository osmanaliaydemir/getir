import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/domain/services/merchant_service.dart';
import 'package:getir_mobile/domain/repositories/merchant_repository.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([MerchantRepository])
import 'merchant_service_test.mocks.dart';

void main() {
  late MerchantService service;
  late MockMerchantRepository mockRepository;

  setUp(() {
    mockRepository = MockMerchantRepository();
    service = MerchantService(mockRepository);
  });

  group('MerchantService -', () {
    group('getMerchants', () {
      test('returns merchants successfully', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.getMerchants(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            search: anyNamed('search'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.getMerchants();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
        verify(
          mockRepository.getMerchants(
            page: 1,
            limit: 20,
            search: null,
            category: null,
          ),
        ).called(1);
      });

      test('returns merchants with search filter', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.getMerchants(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            search: anyNamed('search'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.getMerchants(search: 'Pizza');

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.getMerchants(
            page: 1,
            limit: 20,
            search: 'Pizza',
            category: null,
          ),
        ).called(1);
      });

      test('returns merchants with category filter', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.getMerchants(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            search: anyNamed('search'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.getMerchants(category: 'Restaurant');

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.getMerchants(
            page: 1,
            limit: 20,
            search: null,
            category: 'Restaurant',
          ),
        ).called(1);
      });

      test('propagates error from repository', () async {
        // Arrange
        const exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getMerchants(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            search: anyNamed('search'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getMerchants();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getMerchantById', () {
      test('returns merchant when found', () async {
        // Arrange
        when(
          mockRepository.getMerchantById(any),
        ).thenAnswer((_) async => Result.success(MockData.testMerchant));

        // Act
        final result = await service.getMerchantById('merchant-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testMerchant);
        verify(mockRepository.getMerchantById('merchant-123')).called(1);
      });

      test('returns failure when merchant not found', () async {
        // Arrange
        const exception = NotFoundException(message: 'Merchant not found');
        when(
          mockRepository.getMerchantById(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getMerchantById('invalid-id');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('searchMerchants', () {
      test('returns matching merchants', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.searchMerchants(any),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.searchMerchants('Test');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
        verify(mockRepository.searchMerchants('Test')).called(1);
      });

      test('returns empty list when no matches', () async {
        // Arrange
        when(
          mockRepository.searchMerchants(any),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.searchMerchants('NonExistent');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, isEmpty);
      });
    });

    group('getNearbyMerchants', () {
      test('returns nearby merchants successfully', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.getNearbyMerchants(
            latitude: anyNamed('latitude'),
            longitude: anyNamed('longitude'),
            radius: anyNamed('radius'),
          ),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.getNearbyMerchants(
          latitude: 41.0082,
          longitude: 28.9784,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
        verify(
          mockRepository.getNearbyMerchants(
            latitude: 41.0082,
            longitude: 28.9784,
            radius: 5.0,
          ),
        ).called(1);
      });

      test('uses custom radius when provided', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.getNearbyMerchants(
            latitude: anyNamed('latitude'),
            longitude: anyNamed('longitude'),
            radius: anyNamed('radius'),
          ),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.getNearbyMerchants(
          latitude: 41.0082,
          longitude: 28.9784,
          radius: 10.0,
        );

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.getNearbyMerchants(
            latitude: 41.0082,
            longitude: 28.9784,
            radius: 10.0,
          ),
        ).called(1);
      });
    });

    group('getNearbyMerchantsByCategory', () {
      test('returns nearby merchants filtered by category', () async {
        // Arrange
        final merchants = [MockData.testMerchant];
        when(
          mockRepository.getNearbyMerchantsByCategory(
            latitude: anyNamed('latitude'),
            longitude: anyNamed('longitude'),
            categoryType: anyNamed('categoryType'),
            radius: anyNamed('radius'),
          ),
        ).thenAnswer((_) async => Result.success(merchants));

        // Act
        final result = await service.getNearbyMerchantsByCategory(
          latitude: 41.0082,
          longitude: 28.9784,
          categoryType: 1,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, merchants);
        verify(
          mockRepository.getNearbyMerchantsByCategory(
            latitude: 41.0082,
            longitude: 28.9784,
            categoryType: 1,
            radius: 5.0,
          ),
        ).called(1);
      });

      test('returns empty list when no merchants in category', () async {
        // Arrange
        when(
          mockRepository.getNearbyMerchantsByCategory(
            latitude: anyNamed('latitude'),
            longitude: anyNamed('longitude'),
            categoryType: anyNamed('categoryType'),
            radius: anyNamed('radius'),
          ),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.getNearbyMerchantsByCategory(
          latitude: 41.0082,
          longitude: 28.9784,
          categoryType: 999,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, isEmpty);
      });
    });
  });
}
