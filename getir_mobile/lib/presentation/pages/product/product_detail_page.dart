import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class ProductDetailPage extends StatelessWidget {
  final String productId;
  
  const ProductDetailPage({
    super.key,
    required this.productId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Ürün $productId'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.shopping_bag,
              size: 100,
              color: AppColors.primary,
            ),
            const SizedBox(height: 24),
            Text(
              'Ürün Detayı',
              style: AppTypography.headlineLarge,
            ),
            const SizedBox(height: 8),
            Text(
              'Ürün ID: $productId',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
