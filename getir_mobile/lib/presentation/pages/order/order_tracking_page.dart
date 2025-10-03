import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class OrderTrackingPage extends StatelessWidget {
  final String orderId;
  
  const OrderTrackingPage({
    super.key,
    required this.orderId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Sipariş Takibi $orderId'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.local_shipping,
              size: 100,
              color: AppColors.primary,
            ),
            const SizedBox(height: 24),
            Text(
              'Sipariş Takibi',
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
