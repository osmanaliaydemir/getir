import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class OrderDetailPage extends StatelessWidget {
  final String orderId;
  
  const OrderDetailPage({
    super.key,
    required this.orderId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Sipariş $orderId'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.receipt_long,
              size: 100,
              color: AppColors.primary,
            ),
            const SizedBox(height: 24),
            Text(
              'Sipariş Detayı',
              style: AppTypography.headlineLarge,
            ),
            const SizedBox(height: 8),
            Text(
              'Sipariş ID: $orderId',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
