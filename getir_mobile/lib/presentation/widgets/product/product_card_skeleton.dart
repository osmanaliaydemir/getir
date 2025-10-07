import 'package:flutter/material.dart';
import '../../../core/widgets/skeleton_loader.dart';

class ProductCardSkeleton extends StatelessWidget {
  const ProductCardSkeleton({super.key});

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
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
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Product Image Skeleton
          const SkeletonRectangle(
            width: double.infinity,
            height: 120,
            borderRadius: BorderRadius.vertical(top: Radius.circular(12)),
          ),

          // Product Details
          Padding(
            padding: const EdgeInsets.all(8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Product Name (2 lines)
                const SkeletonText(
                  width: double.infinity,
                  height: 14,
                  margin: EdgeInsets.only(bottom: 4),
                ),
                const SkeletonText(
                  width: 100,
                  height: 14,
                  margin: EdgeInsets.only(bottom: 8),
                ),

                const Spacer(),

                // Price
                const SkeletonText(
                  width: 60,
                  height: 16,
                  margin: EdgeInsets.only(bottom: 8),
                ),

                // Add to Cart Button
                SkeletonRectangle(
                  width: double.infinity,
                  height: 32,
                  borderRadius: BorderRadius.circular(8),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

/// Grid of product card skeletons
class ProductGridSkeleton extends StatelessWidget {
  final int itemCount;

  const ProductGridSkeleton({super.key, this.itemCount = 6});

  @override
  Widget build(BuildContext context) {
    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 2,
        childAspectRatio: 0.75,
        crossAxisSpacing: 12,
        mainAxisSpacing: 12,
      ),
      itemCount: itemCount,
      itemBuilder: (context, index) {
        return const ProductCardSkeleton();
      },
    );
  }
}
