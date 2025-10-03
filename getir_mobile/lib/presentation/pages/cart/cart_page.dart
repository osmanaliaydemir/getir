import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class CartPage extends StatelessWidget {
  const CartPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Sepet'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.shopping_cart,
              size: 100,
              color: AppColors.primary,
            ),
            SizedBox(height: 24),
            Text(
              'Sepet Sayfası',
              style: AppTypography.headlineLarge,
            ),
            SizedBox(height: 8),
            Text(
              'Alışveriş sepetiniz',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
