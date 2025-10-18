import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/presentation/bloc/merchant/merchant_bloc.dart';
import 'package:getir_mobile/domain/services/merchant_service.dart';
import 'package:getir_mobile/domain/entities/merchant.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([MerchantService])
import 'merchant_bloc_test.mocks.dart';

void main() {
  late MerchantBloc merchantBloc;
  late MockMerchantService mockMerchantService;

  setUp(() {
    mockMerchantService = MockMerchantService();
    merchantBloc = MerchantBloc(mockMerchantService);
  });

  tearDown(() {
    merchantBloc.close();
  });

  group('MerchantBloc -', () {
    group('LoadMerchants', () {
      test('initial state is MerchantInitial', () {
        expect(merchantBloc.state, equals(MerchantInitial()));
      });

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantsLoaded] when merchants are loaded',
        build: () {
          when(
            mockMerchantService.getMerchants(
              page: anyNamed('page'),
              limit: anyNamed('limit'),
              search: anyNamed('search'),
              category: anyNamed('category'),
            ),
          ).thenAnswer((_) async => Result.success([MockData.testMerchant]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const LoadMerchants()),
        expect: () => [
          MerchantLoading(),
          MerchantsLoaded([MockData.testMerchant]),
        ],
        verify: (_) {
          verify(
            mockMerchantService.getMerchants(
              page: 1,
              limit: 20,
              search: null,
              category: null,
            ),
          ).called(1);
        },
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantsLoaded] with empty list',
        build: () {
          when(
            mockMerchantService.getMerchants(
              page: anyNamed('page'),
              limit: anyNamed('limit'),
              search: anyNamed('search'),
              category: anyNamed('category'),
            ),
          ).thenAnswer((_) async => Result.success([]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const LoadMerchants()),
        expect: () => [MerchantLoading(), const MerchantsLoaded([])],
      );

      blocTest<MerchantBloc, MerchantState>(
        'loads merchants with custom parameters',
        build: () {
          when(
            mockMerchantService.getMerchants(
              page: 2,
              limit: 50,
              search: 'test',
              category: 'Market',
            ),
          ).thenAnswer((_) async => Result.success([MockData.testMerchant]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadMerchants(
            page: 2,
            limit: 50,
            search: 'test',
            category: 'Market',
          ),
        ),
        expect: () => [
          MerchantLoading(),
          MerchantsLoaded([MockData.testMerchant]),
        ],
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantError] when loading fails',
        build: () {
          when(
            mockMerchantService.getMerchants(
              page: anyNamed('page'),
              limit: anyNamed('limit'),
              search: anyNamed('search'),
              category: anyNamed('category'),
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'Connection failed'),
            ),
          );
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const LoadMerchants()),
        expect: () => [
          MerchantLoading(),
          const MerchantError('Connection failed'),
        ],
      );
    });

    group('LoadMerchantById', () {
      const String testMerchantId = 'merchant-123';

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantLoaded] when merchant is loaded',
        build: () {
          when(
            mockMerchantService.getMerchantById(testMerchantId),
          ).thenAnswer((_) async => Result.success(MockData.testMerchant));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantById(testMerchantId)),
        expect: () => [
          MerchantLoading(),
          MerchantLoaded(MockData.testMerchant),
        ],
        verify: (_) {
          verify(mockMerchantService.getMerchantById(testMerchantId)).called(1);
        },
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantError] when merchant not found',
        build: () {
          when(mockMerchantService.getMerchantById(testMerchantId)).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(
                message: 'Merchant not found',
                code: 'MERCHANT_NOT_FOUND',
              ),
            ),
          );
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantById(testMerchantId)),
        expect: () => [
          MerchantLoading(),
          const MerchantError('Merchant not found'),
        ],
      );
    });

    group('SearchMerchants', () {
      const String testQuery = 'test market';

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantsLoaded] with search results',
        build: () {
          when(
            mockMerchantService.searchMerchants(testQuery),
          ).thenAnswer((_) async => Result.success([MockData.testMerchant]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const SearchMerchants(testQuery)),
        expect: () => [
          MerchantLoading(),
          MerchantsLoaded([MockData.testMerchant]),
        ],
        verify: (_) {
          verify(mockMerchantService.searchMerchants(testQuery)).called(1);
        },
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits empty list when no merchants match search',
        build: () {
          when(
            mockMerchantService.searchMerchants(testQuery),
          ).thenAnswer((_) async => Result.success([]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const SearchMerchants(testQuery)),
        expect: () => [MerchantLoading(), const MerchantsLoaded([])],
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantError] when search fails',
        build: () {
          when(mockMerchantService.searchMerchants(testQuery)).thenAnswer(
            (_) async => Result.failure(
              const ServerException(message: 'Search failed', code: '500'),
            ),
          );
          return merchantBloc;
        },
        act: (bloc) => bloc.add(const SearchMerchants(testQuery)),
        expect: () => [MerchantLoading(), const MerchantError('Search failed')],
      );
    });

    group('LoadNearbyMerchants', () {
      const double testLatitude = 40.9923;
      const double testLongitude = 29.0287;
      const double testRadius = 5.0;

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantsLoaded] for nearby merchants',
        build: () {
          when(
            mockMerchantService.getNearbyMerchants(
              latitude: testLatitude,
              longitude: testLongitude,
              radius: testRadius,
            ),
          ).thenAnswer((_) async => Result.success([MockData.testMerchant]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchants(
            latitude: testLatitude,
            longitude: testLongitude,
            radius: testRadius,
          ),
        ),
        expect: () => [
          MerchantLoading(),
          MerchantsLoaded([MockData.testMerchant]),
        ],
        verify: (_) {
          verify(
            mockMerchantService.getNearbyMerchants(
              latitude: testLatitude,
              longitude: testLongitude,
              radius: testRadius,
            ),
          ).called(1);
        },
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits empty list when no nearby merchants found',
        build: () {
          when(
            mockMerchantService.getNearbyMerchants(
              latitude: testLatitude,
              longitude: testLongitude,
              radius: testRadius,
            ),
          ).thenAnswer((_) async => Result.success([]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchants(
            latitude: testLatitude,
            longitude: testLongitude,
          ),
        ),
        expect: () => [MerchantLoading(), const MerchantsLoaded([])],
      );

      blocTest<MerchantBloc, MerchantState>(
        'uses default radius when not specified',
        build: () {
          when(
            mockMerchantService.getNearbyMerchants(
              latitude: testLatitude,
              longitude: testLongitude,
              radius: 5.0, // default
            ),
          ).thenAnswer((_) async => Result.success([MockData.testMerchant]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchants(
            latitude: testLatitude,
            longitude: testLongitude,
          ),
        ),
        expect: () => [
          MerchantLoading(),
          MerchantsLoaded([MockData.testMerchant]),
        ],
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantError] when location service fails',
        build: () {
          when(
            mockMerchantService.getNearbyMerchants(
              latitude: testLatitude,
              longitude: testLongitude,
              radius: testRadius,
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const ServerException(
                message: 'Location service unavailable',
                code: 'LOCATION_ERROR',
              ),
            ),
          );
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchants(
            latitude: testLatitude,
            longitude: testLongitude,
          ),
        ),
        expect: () => [
          MerchantLoading(),
          const MerchantError('Location service unavailable'),
        ],
      );
    });

    group('LoadNearbyMerchantsByCategory', () {
      const double testLatitude = 40.9923;
      const double testLongitude = 29.0287;
      const int testCategoryType = 2; // Market
      const double testRadius = 5.0;

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantLoading, MerchantsLoaded] for category merchants',
        build: () {
          when(
            mockMerchantService.getNearbyMerchantsByCategory(
              latitude: testLatitude,
              longitude: testLongitude,
              categoryType: testCategoryType,
              radius: testRadius,
            ),
          ).thenAnswer((_) async => Result.success([MockData.testMerchant]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchantsByCategory(
            latitude: testLatitude,
            longitude: testLongitude,
            categoryType: testCategoryType,
            radius: testRadius,
          ),
        ),
        expect: () => [
          MerchantLoading(),
          MerchantsLoaded([MockData.testMerchant]),
        ],
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits empty list when no merchants in category',
        build: () {
          when(
            mockMerchantService.getNearbyMerchantsByCategory(
              latitude: testLatitude,
              longitude: testLongitude,
              categoryType: testCategoryType,
              radius: testRadius,
            ),
          ).thenAnswer((_) async => Result.success([]));
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchantsByCategory(
            latitude: testLatitude,
            longitude: testLongitude,
            categoryType: testCategoryType,
          ),
        ),
        expect: () => [MerchantLoading(), const MerchantsLoaded([])],
      );

      blocTest<MerchantBloc, MerchantState>(
        'emits [MerchantError] when category load fails',
        build: () {
          when(
            mockMerchantService.getNearbyMerchantsByCategory(
              latitude: testLatitude,
              longitude: testLongitude,
              categoryType: testCategoryType,
              radius: testRadius,
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Invalid category type',
                code: 'INVALID_CATEGORY',
              ),
            ),
          );
          return merchantBloc;
        },
        act: (bloc) => bloc.add(
          const LoadNearbyMerchantsByCategory(
            latitude: testLatitude,
            longitude: testLongitude,
            categoryType: testCategoryType,
          ),
        ),
        expect: () => [
          MerchantLoading(),
          const MerchantError('Invalid category type'),
        ],
      );
    });
  });
}
