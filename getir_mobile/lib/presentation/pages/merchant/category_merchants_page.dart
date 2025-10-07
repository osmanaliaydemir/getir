import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../domain/entities/service_category_type.dart';
import '../../bloc/merchant/merchant_bloc.dart';
import '../../widgets/merchant/merchant_card.dart';

class CategoryMerchantsPage extends StatefulWidget {
  final ServiceCategoryType categoryType;
  final String categoryName;
  final double latitude;
  final double longitude;

  const CategoryMerchantsPage({
    super.key,
    required this.categoryType,
    required this.categoryName,
    required this.latitude,
    required this.longitude,
  });

  @override
  State<CategoryMerchantsPage> createState() => _CategoryMerchantsPageState();
}

class _CategoryMerchantsPageState extends State<CategoryMerchantsPage> {
  @override
  void initState() {
    super.initState();
    _loadMerchants();
  }

  void _loadMerchants() {
    context.read<MerchantBloc>().add(
      LoadNearbyMerchantsByCategory(
        latitude: widget.latitude,
        longitude: widget.longitude,
        categoryType: widget.categoryType.value,
        radius: 10.0,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Row(
          children: [
            Icon(
              _getCategoryIcon(widget.categoryType),
              color: AppColors.white,
              size: 24,
            ),
            const SizedBox(width: 12),
            Text(widget.categoryName),
          ],
        ),
        backgroundColor: _getCategoryColor(widget.categoryType),
        foregroundColor: AppColors.white,
      ),
      body: BlocBuilder<MerchantBloc, MerchantState>(
        builder: (context, state) {
          if (state is MerchantLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is MerchantError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.error_outline, size: 64, color: AppColors.error),
                    const SizedBox(height: 16),
                    Text(
                      state.message,
                      style: AppTypography.bodyLarge.copyWith(
                        color: AppColors.error,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: _loadMerchants,
                      child: Text(l10n.retry),
                    ),
                  ],
                ),
              ),
            );
          }

          if (state is MerchantsLoaded) {
            final merchants = state.merchants;

            if (merchants.isEmpty) {
              return _buildEmptyState(l10n);
            }

            return RefreshIndicator(
              onRefresh: () async {
                _loadMerchants();
              },
              child: ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: merchants.length,
                itemBuilder: (context, index) {
                  final merchant = merchants[index];
                  return MerchantCard(
                    merchant: merchant,
                    showCategoryBadge: false, // Zaten kategori sayfasındayız
                  );
                },
              ),
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildEmptyState(AppLocalizations l10n) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            _getCategoryIcon(widget.categoryType),
            size: 80,
            color: AppColors.textSecondary,
          ),
          const SizedBox(height: 24),
          Text(
            l10n.noMerchantsFound,
            style: AppTypography.headlineSmall.copyWith(
              color: AppColors.textSecondary,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            '${widget.categoryName} kategorisinde yakınınızda işletme bulunamadı.',
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 32),
          ElevatedButton.icon(
            onPressed: _loadMerchants,
            icon: const Icon(Icons.refresh),
            label: Text(l10n.retry),
          ),
        ],
      ),
    );
  }

  IconData _getCategoryIcon(ServiceCategoryType type) {
    switch (type) {
      case ServiceCategoryType.restaurant:
        return Icons.restaurant;
      case ServiceCategoryType.market:
        return Icons.local_grocery_store;
      case ServiceCategoryType.pharmacy:
        return Icons.local_pharmacy;
      case ServiceCategoryType.water:
        return Icons.water_drop;
      case ServiceCategoryType.cafe:
        return Icons.local_cafe;
      case ServiceCategoryType.bakery:
        return Icons.bakery_dining;
      case ServiceCategoryType.other:
        return Icons.more_horiz;
    }
  }

  Color _getCategoryColor(ServiceCategoryType type) {
    switch (type) {
      case ServiceCategoryType.restaurant:
        return Colors.red;
      case ServiceCategoryType.market:
        return Colors.orange;
      case ServiceCategoryType.pharmacy:
        return Colors.green;
      case ServiceCategoryType.water:
        return Colors.blue;
      case ServiceCategoryType.cafe:
        return Colors.brown;
      case ServiceCategoryType.bakery:
        return Colors.pink;
      case ServiceCategoryType.other:
        return Colors.grey;
    }
  }
}
