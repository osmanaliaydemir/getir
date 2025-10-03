import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class OrdersPage extends StatelessWidget {
  const OrdersPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Siparişlerim'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.receipt_long,
              size: 100,
              color: AppColors.primary,
            ),
            SizedBox(height: 24),
            Text(
              'Siparişlerim',
              style: AppTypography.headlineLarge,
            ),
            SizedBox(height: 8),
            Text(
              'Sipariş geçmişiniz',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
