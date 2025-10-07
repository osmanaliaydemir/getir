import 'package:flutter/material.dart';
import '../../../core/widgets/skeleton_loader.dart';

class MerchantCardSkeleton extends StatelessWidget {
  final bool showCategoryBadge;

  const MerchantCardSkeleton({super.key, this.showCategoryBadge = false});

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: isDark ? const Color(0xFF1E1E1E) : Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          // Merchant Logo Skeleton
          const SkeletonRectangle(
            width: 60,
            height: 60,
            borderRadius: BorderRadius.all(Radius.circular(8)),
          ),

          const SizedBox(width: 12),

          // Merchant Info Skeleton
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Name
                Row(
                  children: [
                    const SkeletonText(width: 150, height: 16),
                    if (showCategoryBadge) ...[
                      const SizedBox(width: 8),
                      const SkeletonRectangle(
                        width: 60,
                        height: 20,
                        borderRadius: BorderRadius.all(Radius.circular(10)),
                      ),
                    ],
                  ],
                ),

                const SizedBox(height: 8),

                // Rating & Delivery Time
                const Row(
                  children: [
                    SkeletonCircle(size: 16),
                    SizedBox(width: 4),
                    SkeletonText(width: 30, height: 12),
                    SizedBox(width: 16),
                    SkeletonCircle(size: 16),
                    SizedBox(width: 4),
                    SkeletonText(width: 40, height: 12),
                  ],
                ),

                const SizedBox(height: 8),

                // Delivery Info
                const Row(
                  children: [
                    SkeletonText(width: 100, height: 12),
                    SizedBox(width: 8),
                    SkeletonText(width: 60, height: 12),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

/// List of merchant card skeletons
class MerchantListSkeleton extends StatelessWidget {
  final int itemCount;
  final bool showCategoryBadge;

  const MerchantListSkeleton({
    super.key,
    this.itemCount = 5,
    this.showCategoryBadge = false,
  });

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      itemCount: itemCount,
      physics: const NeverScrollableScrollPhysics(),
      shrinkWrap: true,
      itemBuilder: (context, index) {
        return MerchantCardSkeleton(showCategoryBadge: showCategoryBadge);
      },
    );
  }
}
