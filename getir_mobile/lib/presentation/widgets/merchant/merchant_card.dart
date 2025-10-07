import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../domain/entities/merchant.dart';
import '../../../domain/entities/service_category_type.dart';
import '../../pages/merchant/merchant_detail_page.dart';

/// Reusable Merchant Card Widget
/// Merchant bilgilerini gösterir ve detay sayfasına yönlendirir
class MerchantCard extends StatelessWidget {
  final Merchant merchant;
  final bool showCategoryBadge;

  const MerchantCard({
    super.key,
    required this.merchant,
    this.showCategoryBadge = true,
  });

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Stack(
        children: [
          InkWell(
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) =>
                      MerchantDetailPage(merchantId: merchant.id),
                ),
              );
            },
            borderRadius: BorderRadius.circular(12),
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  // Merchant logo
                  _buildLogo(),
                  const SizedBox(width: 16),
                  // Merchant info
                  Expanded(child: _buildMerchantInfo(l10n)),
                  const Icon(
                    Icons.arrow_forward_ios,
                    color: AppColors.textSecondary,
                    size: 16,
                  ),
                ],
              ),
            ),
          ),
          // Kategori badge'i (sağ üst köşe)
          if (showCategoryBadge && merchant.categoryType != null)
            Positioned(top: 8, right: 8, child: _buildCategoryBadge()),
        ],
      ),
    );
  }

  Widget _buildLogo() {
    return ClipRRect(
      borderRadius: BorderRadius.circular(8),
      child: Container(
        width: 60,
        height: 60,
        color: Colors.grey[200],
        child: merchant.logoUrl.isNotEmpty
            ? Image.network(
                merchant.logoUrl,
                fit: BoxFit.cover,
                errorBuilder: (context, error, stackTrace) {
                  return const Icon(Icons.store, size: 30, color: Colors.grey);
                },
              )
            : const Icon(Icons.store, size: 30, color: Colors.grey),
      ),
    );
  }

  Widget _buildMerchantInfo(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Merchant name
        Text(
          merchant.name,
          style: AppTypography.bodyLarge.copyWith(fontWeight: FontWeight.w600),
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        const SizedBox(height: 4),
        // Category and distance
        Text(
          '${merchant.categoryType?.displayName ?? 'Market'} • ${merchant.distance.toStringAsFixed(1)} km',
          style: AppTypography.bodyMedium.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: 4),
        // Status, rating, delivery time
        Row(
          children: [
            // Open/Closed status
            _buildStatusIndicator(l10n),
            const Spacer(),
            // Rating
            if (merchant.rating > 0) ...[
              const Icon(Icons.star, color: Colors.amber, size: 16),
              const SizedBox(width: 2),
              Text(
                '${merchant.rating}',
                style: AppTypography.bodySmall.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(width: 8),
            ],
            // Delivery time
            Text(
              '${merchant.estimatedDeliveryTime} ${l10n.minutes}',
              style: AppTypography.bodySmall.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildStatusIndicator(AppLocalizations l10n) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 8,
          height: 8,
          decoration: BoxDecoration(
            color: merchant.isOpen ? Colors.green : Colors.red,
            shape: BoxShape.circle,
          ),
        ),
        const SizedBox(width: 4),
        Text(
          merchant.isOpen ? l10n.open : l10n.closed,
          style: AppTypography.bodySmall.copyWith(
            color: merchant.isOpen ? Colors.green : Colors.red,
            fontWeight: FontWeight.w500,
          ),
        ),
      ],
    );
  }

  Widget _buildCategoryBadge() {
    final categoryType = merchant.categoryType!;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: _getCategoryColor(categoryType),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(_getCategoryIcon(categoryType), color: Colors.white, size: 12),
          const SizedBox(width: 4),
          Text(
            categoryType.displayName,
            style: const TextStyle(
              color: Colors.white,
              fontSize: 10,
              fontWeight: FontWeight.w600,
            ),
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
