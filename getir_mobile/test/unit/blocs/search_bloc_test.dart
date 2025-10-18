import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/bloc/search/search_bloc.dart';
import 'package:getir_mobile/presentation/bloc/search/search_event.dart';
import 'package:getir_mobile/presentation/bloc/search/search_state.dart';
import 'package:getir_mobile/domain/services/merchant_service.dart';
import 'package:getir_mobile/domain/services/product_service.dart';
import 'package:getir_mobile/core/services/search_history_service.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([MerchantService, ProductService, SearchHistoryService])
import 'search_bloc_test.mocks.dart';

void main() {
  late SearchBloc bloc;
  late MockMerchantService mockMerchantService;
  late MockProductService mockProductService;
  late MockSearchHistoryService mockSearchHistoryService;

  setUp(() {
    mockMerchantService = MockMerchantService();
    mockProductService = MockProductService();
    mockSearchHistoryService = MockSearchHistoryService();
    bloc = SearchBloc(
      mockMerchantService,
      mockProductService,
      mockSearchHistoryService,
    );
  });

  group('SearchBloc -', () {
    test('initial state is SearchInitial', () {
      expect(bloc.state, const SearchInitial());
    });

    blocTest<SearchBloc, SearchState>(
      'SearchHistoryLoaded emits [SearchInitial] with history',
      build: () {
        when(mockSearchHistoryService.getSearchHistory()).thenAnswer(
          (_) async => ['Pizza', 'Burger'],
        );
        return bloc;
      },
      act: (bloc) => bloc.add(SearchHistoryLoaded()),
      expect: () => [
        const SearchInitial(searchHistory: ['Pizza', 'Burger']),
      ],
    );

    blocTest<SearchBloc, SearchState>(
      'SearchSubmitted emits [SearchLoading, SearchSuccess] for merchants',
      build: () {
        when(mockMerchantService.searchMerchants(any)).thenAnswer(
          (_) async => Result.success([MockData.testMerchant]),
        );
        when(mockProductService.searchProducts(any)).thenAnswer(
          (_) async => Result.success([]),
        );
        when(mockSearchHistoryService.addSearchQuery(any)).thenAnswer(
          (_) async => {},
        );
        return bloc;
      },
      act: (bloc) => bloc.add(const SearchSubmitted('Restaurant')),
      expect: () => [
        const SearchLoading(),
        isA<SearchSuccess>(),
      ],
    );

    blocTest<SearchBloc, SearchState>(
      'SearchSubmitted emits [SearchLoading, SearchSuccess] for products',
      build: () {
        when(mockMerchantService.searchMerchants(any)).thenAnswer(
          (_) async => Result.success([]),
        );
        when(mockProductService.searchProducts(any)).thenAnswer(
          (_) async => Result.success([MockData.testProduct]),
        );
        when(mockSearchHistoryService.addSearchQuery(any)).thenAnswer(
          (_) async => {},
        );
        return bloc;
      },
      act: (bloc) => bloc.add(const SearchSubmitted('Pizza')),
      expect: () => [
        const SearchLoading(),
        isA<SearchSuccess>(),
      ],
    );

    blocTest<SearchBloc, SearchState>(
      'SearchSubmitted emits [SearchLoading, SearchError] when search fails',
      build: () {
        when(mockMerchantService.searchMerchants(any)).thenAnswer(
          (_) async => Result.failure(const NetworkException(message: 'Network error')),
        );
        when(mockProductService.searchProducts(any)).thenAnswer(
          (_) async => Result.failure(const NetworkException(message: 'Network error')),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(const SearchSubmitted('Error')),
      expect: () => [
        const SearchLoading(),
        isA<SearchError>(),
      ],
    );

    blocTest<SearchBloc, SearchState>(
      'SearchHistoryCleared emits [SearchInitial] with empty history',
      build: () {
        when(mockSearchHistoryService.clearSearchHistory()).thenAnswer(
          (_) async => {},
        );
        return bloc;
      },
      act: (bloc) => bloc.add(SearchHistoryCleared()),
      expect: () => [
        const SearchInitial(searchHistory: []),
      ],
    );

    blocTest<SearchBloc, SearchState>(
      'SearchHistoryItemRemoved emits [SearchInitial] with updated history',
      build: () {
        when(mockSearchHistoryService.removeSearchQuery(any)).thenAnswer(
          (_) async => {},
        );
        when(mockSearchHistoryService.getSearchHistory()).thenAnswer(
          (_) async => ['Burger'],
        );
        return bloc;
      },
      act: (bloc) => bloc.add(const SearchHistoryItemRemoved('Pizza')),
      expect: () => [
        const SearchInitial(searchHistory: ['Burger']),
      ],
    );

    blocTest<SearchBloc, SearchState>(
      'SearchTypeChanged updates search type in state',
      build: () {
        when(mockSearchHistoryService.getSearchHistory()).thenAnswer(
          (_) async => [],
        );
        return bloc;
      },
      seed: () => const SearchInitial(searchHistory: []),
      act: (bloc) => bloc.add(const SearchTypeChanged(SearchType.merchants)),
      expect: () => [
        const SearchInitial(
          searchHistory: [],
          currentSearchType: SearchType.merchants,
        ),
      ],
    );
  });
}
