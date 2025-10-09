import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';
import 'package:getir_mobile/data/repositories/product_repository_impl.dart';
import 'package:getir_mobile/data/datasources/product_datasource.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([ProductDataSource])
import 'product_repository_impl_test.mocks.dart';

void main() {
  late ProductRepositoryImpl repository;
  late MockProductDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockProductDataSource();
    repository = ProductRepositoryImpl(mockDataSource);
  });

  group('ProductRepositoryImpl -', () {
    group('getProducts', () {
      test('returns success with products when datasource succeeds', () async {
        // Arrange
        final products = [MockData.testProduct, MockData.testProduct2];
        when(
          mockDataSource.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
            search: anyNamed('search'),
          ),
        ).thenAnswer((_) async => products);

        // Act
        final result = await repository.getProducts();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, products);
        expect(result.data?.length, 2);
      });

      test('handles DioException correctly', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/products'),
          type: DioExceptionType.connectionError,
        );
        when(
          mockDataSource.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
            search: anyNamed('search'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getProducts();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NoInternetException>());
      });

      test('wraps AppException correctly', () async {
        // Arrange
        const appException = ApiException(message: 'Products error');
        when(
          mockDataSource.getProducts(
            page: anyNamed('page'),
            limit: anyNamed('limit'),
            merchantId: anyNamed('merchantId'),
            category: anyNamed('category'),
            search: anyNamed('search'),
          ),
        ).thenThrow(appException);

        // Act
        final result = await repository.getProducts();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });

    group('getProductById', () {
      test('returns success with product when found', () async {
        // Arrange
        when(
          mockDataSource.getProductById(any),
        ).thenAnswer((_) async => MockData.testProduct);

        // Act
        final result = await repository.getProductById('product-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testProduct);
      });

      test('handles 404 error as NotFoundException', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/products/123'),
          type: DioExceptionType.badResponse,
          response: Response(
            requestOptions: RequestOptions(path: '/products/123'),
            statusCode: 404,
            data: {'message': 'Product not found'},
          ),
        );
        when(mockDataSource.getProductById(any)).thenThrow(dioException);

        // Act
        final result = await repository.getProductById('invalid-id');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('getProductsByMerchant', () {
      test('returns success with merchant products', () async {
        // Arrange
        final products = [MockData.testProduct];
        when(
          mockDataSource.getProductsByMerchant(any),
        ).thenAnswer((_) async => products);

        // Act
        final result = await repository.getProductsByMerchant('merchant-123');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, products);
      });
    });

    group('searchProducts', () {
      test('returns success with matching products', () async {
        // Arrange
        final products = [MockData.testProduct];
        when(
          mockDataSource.searchProducts(any),
        ).thenAnswer((_) async => products);

        // Act
        final result = await repository.searchProducts('Test');

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, products);
      });
    });

    group('getCategories', () {
      test('returns success with categories', () async {
        // Arrange
        final categories = ['Food', 'Drinks'];
        when(
          mockDataSource.getCategories(),
        ).thenAnswer((_) async => categories);

        // Act
        final result = await repository.getCategories();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, categories);
      });

      test('handles error correctly', () async {
        // Arrange
        when(mockDataSource.getCategories()).thenThrow(Exception('Error'));

        // Act
        final result = await repository.getCategories();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ApiException>());
      });
    });
  });
}
