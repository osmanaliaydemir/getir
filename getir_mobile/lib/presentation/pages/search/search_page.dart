import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../bloc/search/search_bloc.dart';
import '../../bloc/search/search_event.dart';
import '../../bloc/search/search_state.dart';
import '../../widgets/merchant/merchant_card.dart';
import '../../widgets/product/product_card.dart';
import '../../widgets/merchant/merchant_card_skeleton.dart';
import '../../widgets/product/product_card_skeleton.dart';
import '../../../core/widgets/error_state_widget.dart';

class SearchPage extends StatefulWidget {
  const SearchPage({super.key});

  @override
  State<SearchPage> createState() => _SearchPageState();
}

class _SearchPageState extends State<SearchPage>
    with SingleTickerProviderStateMixin {
  final TextEditingController _searchController = TextEditingController();
  final FocusNode _searchFocusNode = FocusNode();
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _tabController.addListener(_onTabChanged);

    // Load search history on init
    context.read<SearchBloc>().add(const SearchHistoryLoaded());
  }

  void _onTabChanged() {
    if (!_tabController.indexIsChanging) {
      final searchType = _getSearchTypeFromIndex(_tabController.index);
      context.read<SearchBloc>().add(SearchTypeChanged(searchType));
    }
  }

  SearchType _getSearchTypeFromIndex(int index) {
    switch (index) {
      case 0:
        return SearchType.all;
      case 1:
        return SearchType.merchants;
      case 2:
        return SearchType.products;
      default:
        return SearchType.all;
    }
  }

  @override
  void dispose() {
    _searchController.dispose();
    _searchFocusNode.dispose();
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Ara'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
        elevation: 0,
      ),
      body: Column(
        children: [
          // Search Bar
          Container(
            color: AppColors.primary,
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: TextField(
              controller: _searchController,
              focusNode: _searchFocusNode,
              autofocus: true,
              decoration: InputDecoration(
                hintText: 'Ürün veya mağaza ara...',
                hintStyle: AppTypography.bodyMedium.copyWith(
                  color: AppColors.textSecondary,
                ),
                prefixIcon: const Icon(Icons.search, color: AppColors.primary),
                suffixIcon: _searchController.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(
                          Icons.clear,
                          color: AppColors.textSecondary,
                        ),
                        onPressed: () {
                          _searchController.clear();
                          context.read<SearchBloc>().add(
                            const SearchQueryChanged(''),
                          );
                        },
                      )
                    : null,
                filled: true,
                fillColor: AppColors.white,
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(12),
                  borderSide: BorderSide.none,
                ),
                contentPadding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 12,
                ),
              ),
              style: AppTypography.bodyMedium,
              onChanged: (value) {
                setState(() {}); // To update suffix icon
                context.read<SearchBloc>().add(SearchQueryChanged(value));
              },
              onSubmitted: (value) {
                if (value.trim().isNotEmpty) {
                  context.read<SearchBloc>().add(SearchSubmitted(value.trim()));
                }
              },
            ),
          ),

          // Search Type Tabs
          Container(
            color: AppColors.white,
            child: TabBar(
              controller: _tabController,
              labelColor: AppColors.primary,
              unselectedLabelColor: AppColors.textSecondary,
              indicatorColor: AppColors.primary,
              indicatorWeight: 3,
              labelStyle: AppTypography.bodyMedium.copyWith(
                fontWeight: FontWeight.w600,
              ),
              tabs: const [
                Tab(text: 'Tümü'),
                Tab(text: 'Mağazalar'),
                Tab(text: 'Ürünler'),
              ],
            ),
          ),

          const Divider(height: 1),

          // Search Results / History
          Expanded(
            child: BlocBuilder<SearchBloc, SearchState>(
              builder: (context, state) {
                if (state is SearchLoading) {
                  final searchType = state.searchType;

                  return SingleChildScrollView(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      children: [
                        // Show merchant skeletons for all or merchants
                        if (searchType == SearchType.all ||
                            searchType == SearchType.merchants)
                          const MerchantListSkeleton(
                            itemCount: 3,
                            showCategoryBadge: true,
                          ),

                        // Show product skeletons for all or products
                        if (searchType == SearchType.all ||
                            searchType == SearchType.products) ...[
                          if (searchType == SearchType.all)
                            const SizedBox(height: 16),
                          const ProductGridSkeleton(itemCount: 4),
                        ],
                      ],
                    ),
                  );
                }

                if (state is SearchError) {
                  return ErrorStateWidget(
                    errorType: _getErrorTypeFromMessage(state.message),
                    customMessage: state.message,
                    onRetry: () {
                      context.read<SearchBloc>().add(
                        const SearchHistoryLoaded(),
                      );
                    },
                  );
                }

                if (state is SearchSuccess) {
                  return _buildSearchResults(state);
                }

                if (state is SearchInitial || state is SearchHistoryUpdated) {
                  final searchHistory = state is SearchInitial
                      ? state.searchHistory
                      : (state as SearchHistoryUpdated).searchHistory;
                  return _buildSearchHistory(searchHistory);
                }

                return const SizedBox.shrink();
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSearchHistory(List<String> history) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Search History
          if (history.isNotEmpty) ...[
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Son Aramalar',
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
                TextButton(
                  onPressed: () {
                    context.read<SearchBloc>().add(
                      const SearchHistoryCleared(),
                    );
                  },
                  child: Text(
                    'Temizle',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.error,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            ...history.map(
              (query) => ListTile(
                contentPadding: EdgeInsets.zero,
                leading: const Icon(
                  Icons.history,
                  color: AppColors.textSecondary,
                ),
                title: Text(query, style: AppTypography.bodyMedium),
                trailing: IconButton(
                  icon: const Icon(
                    Icons.close,
                    color: AppColors.textSecondary,
                    size: 20,
                  ),
                  onPressed: () {
                    context.read<SearchBloc>().add(
                      SearchHistoryItemRemoved(query),
                    );
                  },
                ),
                onTap: () {
                  _searchController.text = query;
                  context.read<SearchBloc>().add(SearchSubmitted(query));
                },
              ),
            ),
            const SizedBox(height: 24),
          ],

          // Popular Searches
          Text(
            'Popüler Aramalar',
            style: AppTypography.bodyLarge.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              'Pizza',
              'Burger',
              'Su',
              'Makarna',
              'Süt',
              'Ekmek',
              'Kahve',
              'Çay',
            ].map((item) => _buildPopularSearchChip(item)).toList(),
          ),
        ],
      ),
    );
  }

  Widget _buildPopularSearchChip(String label) {
    return ActionChip(
      label: Text(label),
      labelStyle: AppTypography.bodySmall.copyWith(color: AppColors.primary),
      backgroundColor: AppColors.primary.withOpacity(0.1),
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
        side: BorderSide(color: AppColors.primary.withOpacity(0.3)),
      ),
      onPressed: () {
        _searchController.text = label;
        context.read<SearchBloc>().add(SearchSubmitted(label));
      },
    );
  }

  Widget _buildSearchResults(SearchSuccess state) {
    if (state.isEmpty) {
      return _buildEmptyState();
    }

    final showMerchants =
        state.searchType == SearchType.all ||
        state.searchType == SearchType.merchants;
    final showProducts =
        state.searchType == SearchType.all ||
        state.searchType == SearchType.products;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Search Results Info
          Text(
            '"${state.query}" için sonuçlar',
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: 16),

          // Merchants Section
          if (showMerchants && state.merchants.isNotEmpty) ...[
            Row(
              children: [
                const Icon(Icons.store, color: AppColors.primary, size: 20),
                const SizedBox(width: 8),
                Text(
                  'Mağazalar (${state.merchants.length})',
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            ...state.merchants.map(
              (merchant) => Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: MerchantCard(
                  merchant: merchant,
                  showCategoryBadge: true,
                ),
              ),
            ),
            if (showProducts && state.products.isNotEmpty)
              const SizedBox(height: 24),
          ],

          // Products Section
          if (showProducts && state.products.isNotEmpty) ...[
            Row(
              children: [
                const Icon(
                  Icons.shopping_bag,
                  color: AppColors.primary,
                  size: 20,
                ),
                const SizedBox(width: 8),
                Text(
                  'Ürünler (${state.products.length})',
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                childAspectRatio: 0.75,
                crossAxisSpacing: 12,
                mainAxisSpacing: 12,
              ),
              itemCount: state.products.length,
              itemBuilder: (context, index) {
                return ProductCard(product: state.products[index]);
              },
            ),
          ],
        ],
      ),
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.search_off,
            size: 80,
            color: AppColors.textSecondary.withOpacity(0.5),
          ),
          const SizedBox(height: 16),
          Text(
            'Sonuç Bulunamadı',
            style: AppTypography.headlineSmall.copyWith(
              color: AppColors.textPrimary,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'Aradığınız ürün veya mağaza bulunamadı.\nBaşka bir arama yapın.',
            textAlign: TextAlign.center,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  ErrorType _getErrorTypeFromMessage(String message) {
    final lowerMessage = message.toLowerCase();

    if (lowerMessage.contains('network') ||
        lowerMessage.contains('connection') ||
        lowerMessage.contains('internet') ||
        lowerMessage.contains('bağlantı')) {
      return ErrorType.network;
    } else if (lowerMessage.contains('500') ||
        lowerMessage.contains('502') ||
        lowerMessage.contains('503') ||
        lowerMessage.contains('server') ||
        lowerMessage.contains('sunucu')) {
      return ErrorType.server;
    } else if (lowerMessage.contains('404') ||
        lowerMessage.contains('not found') ||
        lowerMessage.contains('bulunamadı')) {
      return ErrorType.notFound;
    } else if (lowerMessage.contains('401') ||
        lowerMessage.contains('403') ||
        lowerMessage.contains('unauthorized') ||
        lowerMessage.contains('yetkisiz')) {
      return ErrorType.unauthorized;
    }

    return ErrorType.generic;
  }
}
