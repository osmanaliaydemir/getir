import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/presentation/bloc/product/product_bloc.dart';
import 'package:getir_mobile/domain/services/product_service.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([ProductService])
import 'product_bloc_test.mocks.dart';

void main() {
  late ProductBloc productBloc;
  late MockProductService mockProductService;

  setUp(() {
    mockProductService = MockProductService();
    productBloc = ProductBloc(mockProductService);
  });

  tearDown(() {
    productBloc.close();
  });

  group('ProductBloc -', () {
    group('LoadProducts', () {
      test('initial state is ProductInitial', () {
        expect(productBloc.state, equals(ProductInitial()));
      });

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductsLoaded] when products are loaded',
        build: () {
          when(
            mockProductService.getProducts(
              page: anyNamed('page'),
              limit: anyNamed('limit'),
              merchantId: anyNamed('merchantId'),
              category: anyNamed('category'),
            ),
          ).thenAnswer(
            (_) async =>
                Result.success([MockData.testProduct, MockData.testProduct2]),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProducts()),
        expect: () => [
          ProductLoading(),
          ProductsLoaded([MockData.testProduct, MockData.testProduct2]),
        ],
        verify: (_) {
          verify(
            mockProductService.getProducts(
              page: 1,
              limit: 20,
              merchantId: null,
              category: null,
            ),
          ).called(1);
        },
      );

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductsLoaded] with empty list',
        build: () {
          when(
            mockProductService.getProducts(
              page: anyNamed('page'),
              limit: anyNamed('limit'),
              merchantId: anyNamed('merchantId'),
              category: anyNamed('category'),
            ),
          ).thenAnswer((_) async => Result.success([]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProducts()),
        expect: () => [ProductLoading(), const ProductsLoaded([])],
      );

      blocTest<ProductBloc, ProductState>(
        'loads products with custom page and limit',
        build: () {
          when(
            mockProductService.getProducts(
              page: 2,
              limit: 50,
              merchantId: null,
              category: null,
            ),
          ).thenAnswer((_) async => Result.success([MockData.testProduct]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProducts(page: 2, limit: 50)),
        expect: () => [
          ProductLoading(),
          ProductsLoaded([MockData.testProduct]),
        ],
      );

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductError] when loading fails',
        build: () {
          when(
            mockProductService.getProducts(
              page: anyNamed('page'),
              limit: anyNamed('limit'),
              merchantId: anyNamed('merchantId'),
              category: anyNamed('category'),
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Connection failed'),
            ),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProducts()),
        expect: () => [
          ProductLoading(),
          const ProductError('Connection failed'),
        ],
      );
    });

    group('LoadProductById', () {
      const String testProductId = 'product-123';

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductLoaded] when product is loaded',
        build: () {
          when(
            mockProductService.getProductById(testProductId),
          ).thenAnswer((_) async => Result.success(MockData.testProduct));
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductById(testProductId)),
        expect: () => [ProductLoading(), ProductLoaded(MockData.testProduct)],
        verify: (_) {
          verify(mockProductService.getProductById(testProductId)).called(1);
        },
      );

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductError] when product not found',
        build: () {
          when(mockProductService.getProductById(testProductId)).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(
                message: 'Product not found',
                code: 'PRODUCT_NOT_FOUND',
              ),
            ),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductById(testProductId)),
        expect: () => [
          ProductLoading(),
          const ProductError('Product not found'),
        ],
      );
    });

    group('LoadProductsByMerchant', () {
      const String testMerchantId = 'merchant-123';

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductsLoaded] for merchant products',
        build: () {
          when(
            mockProductService.getProductsByMerchant(testMerchantId),
          ).thenAnswer(
            (_) async =>
                Result.success([MockData.testProduct, MockData.testProduct2]),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductsByMerchant(testMerchantId)),
        expect: () => [
          ProductLoading(),
          ProductsLoaded([MockData.testProduct, MockData.testProduct2]),
        ],
      );

      blocTest<ProductBloc, ProductState>(
        'emits empty list when merchant has no products',
        build: () {
          when(
            mockProductService.getProductsByMerchant(testMerchantId),
          ).thenAnswer((_) async => Result.success([]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductsByMerchant(testMerchantId)),
        expect: () => [ProductLoading(), const ProductsLoaded([])],
      );

      blocTest<ProductBloc, ProductState>(
        'emits [ProductError] when merchant not found',
        build: () {
          when(
            mockProductService.getProductsByMerchant(testMerchantId),
          ).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(
                message: 'Merchant not found',
                code: 'MERCHANT_NOT_FOUND',
              ),
            ),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductsByMerchant(testMerchantId)),
        expect: () => [
          ProductLoading(),
          const ProductError('Merchant not found'),
        ],
      );
    });

    group('SearchProducts', () {
      const String testQuery = 'test';

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductsLoaded] with search results',
        build: () {
          when(
            mockProductService.searchProducts(testQuery),
          ).thenAnswer((_) async => Result.success([MockData.testProduct]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const SearchProducts(testQuery)),
        expect: () => [
          ProductLoading(),
          ProductsLoaded([MockData.testProduct]),
        ],
        verify: (_) {
          verify(mockProductService.searchProducts(testQuery)).called(1);
        },
      );

      blocTest<ProductBloc, ProductState>(
        'emits empty list when no products match search',
        build: () {
          when(
            mockProductService.searchProducts(testQuery),
          ).thenAnswer((_) async => Result.success([]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const SearchProducts(testQuery)),
        expect: () => [ProductLoading(), const ProductsLoaded([])],
      );

      blocTest<ProductBloc, ProductState>(
        'emits [ProductError] when search fails',
        build: () {
          when(mockProductService.searchProducts(testQuery)).thenAnswer(
            (_) async => Result.failure(
              const ServerException(message: 'Search failed', code: '500'),
            ),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(const SearchProducts(testQuery)),
        expect: () => [ProductLoading(), const ProductError('Search failed')],
      );
    });

    group('LoadProductsByCategory', () {
      const String testCategory = 'İçecek';

      blocTest<ProductBloc, ProductState>(
        'emits [ProductLoading, ProductsLoaded] for category',
        build: () {
          when(
            mockProductService.getProducts(category: testCategory),
          ).thenAnswer((_) async => Result.success([MockData.testProduct]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductsByCategory(testCategory)),
        expect: () => [
          ProductLoading(),
          ProductsLoaded([MockData.testProduct]),
        ],
      );

      blocTest<ProductBloc, ProductState>(
        'emits empty list when category has no products',
        build: () {
          when(
            mockProductService.getProducts(category: testCategory),
          ).thenAnswer((_) async => Result.success([]));
          return productBloc;
        },
        act: (bloc) => bloc.add(const LoadProductsByCategory(testCategory)),
        expect: () => [ProductLoading(), const ProductsLoaded([])],
      );
    });

    group('LoadCategories', () {
      final List<String> testCategories = ['İçecek', 'Gıda', 'Temizlik'];

      blocTest<ProductBloc, ProductState>(
        'emits [ProductCategoriesLoaded] when categories are loaded',
        build: () {
          when(
            mockProductService.getCategories(),
          ).thenAnswer((_) async => Result.success(testCategories));
          return productBloc;
        },
        act: (bloc) => bloc.add(LoadCategories()),
        expect: () => [ProductCategoriesLoaded(testCategories)],
        verify: (_) {
          verify(mockProductService.getCategories()).called(1);
        },
      );

      blocTest<ProductBloc, ProductState>(
        'emits [ProductError] when loading categories fails',
        build: () {
          when(mockProductService.getCategories()).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Failed to load categories'),
            ),
          );
          return productBloc;
        },
        act: (bloc) => bloc.add(LoadCategories()),
        expect: () => [const ProductError('Failed to load categories')],
      );
    });
  });
}
