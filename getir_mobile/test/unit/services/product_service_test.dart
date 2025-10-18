import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/domain/services/product_service.dart';
import 'package:getir_mobile/domain/repositories/product_repository.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([IProductRepository])
import 'product_service_test.mocks.dart';

void main() {
  late ProductService service;
  late MockIProductRepository mockRepository;

  setUp(() {
    mockRepository = MockIProductRepository();
    service = ProductService(mockRepository);
  });

  group('ProductService -', () {
    group('getProducts', () {
      test('returns products when repository succeeds', () async {
        // Arrange
        final products = [MockData.testProduct, MockData.testProduct2];
        when(
          mockRepository.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.success(products));

        // Act
        final result = await service.getProducts();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, products);
        verify(
          mockRepository.getProducts(
            page: 1,
            limit: 20,
            merchantId: null,
            category: null,
          ),
        ).called(1);
      });

      test('returns products with custom pagination', () async {
        // Arrange
        final products = [MockData.testProduct];
        when(
          mockRepository.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.success(products));

        // Act
        final result = await service.getProducts(page: 2, limit: 10);

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.getProducts(
            page: 2,
            limit: 10,
            merchantId: null,
            category: null,
          ),
        ).called(1);
      });

      test('returns products filtered by merchant and category', () async {
        // Arrange
        final products = [MockData.testProduct];
        when(
          mockRepository.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.success(products));

        // Act
        final result = await service.getProducts(
          merchantId: 'merchant-123',
          category: 'Food',
        );

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.getProducts(
            page: 1,
            limit: 20,
            merchantId: 'merchant-123',
            category: 'Food',
          ),
        ).called(1);
      });

      test('propagates error from repository', () async {
        // Arrange
        const exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getProducts();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getProductById', () {
      test('returns product when found', () async {
        // Arrange
        when(
          mockRepository.getProductById(any),
        ).thenAnswer((_) async => Result.success(MockData.testProduct));

        // Act
        final result = await service.getProductById('product-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testProduct);
        verify(mockRepository.getProductById('product-123')).called(1);
      });

      test('returns failure when product not found', () async {
        // Arrange
        const exception = NotFoundException(message: 'Product not found');
        when(
          mockRepository.getProductById(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getProductById('invalid-id');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getProductsByMerchant', () {
      test('returns merchant products successfully', () async {
        // Arrange
        final products = [MockData.testProduct, MockData.testProduct2];
        when(
          mockRepository.getProductsByMerchant(any),
        ).thenAnswer((_) async => Result.success(products));

        // Act
        final result = await service.getProductsByMerchant('merchant-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, products);
        expect(result.data?.length, 2);
        verify(mockRepository.getProductsByMerchant('merchant-123')).called(1);
      });

      test('returns empty list when merchant has no products', () async {
        // Arrange
        when(
          mockRepository.getProductsByMerchant(any),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.getProductsByMerchant('merchant-456');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, isEmpty);
      });
    });

    group('searchProducts', () {
      test('returns matching products', () async {
        // Arrange
        final products = [MockData.testProduct];
        when(
          mockRepository.searchProducts(any),
        ).thenAnswer((_) async => Result.success(products));

        // Act
        final result = await service.searchProducts('Test');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, products);
        verify(mockRepository.searchProducts('Test')).called(1);
      });

      test('returns empty list when no matches', () async {
        // Arrange
        when(
          mockRepository.searchProducts(any),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.searchProducts('NonExistent');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, isEmpty);
      });
    });

    group('getCategories', () {
      test('returns categories successfully', () async {
        // Arrange
        final categories = ['Food', 'Drinks', 'Snacks'];
        when(
          mockRepository.getCategories(),
        ).thenAnswer((_) async => Result.success(categories));

        // Act
        final result = await service.getCategories();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, categories);
        expect(result.data?.length, 3);
        verify(mockRepository.getCategories()).called(1);
      });

      test('propagates error when loading categories fails', () async {
        // Arrange
        const exception = NetworkException(
          message: 'Failed to load categories',
        );
        when(
          mockRepository.getCategories(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getCategories();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });
  });
}
