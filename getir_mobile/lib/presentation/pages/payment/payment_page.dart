import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class PaymentPage extends StatelessWidget {
  const PaymentPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Ödeme'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.payment,
              size: 100,
              color: AppColors.primary,
            ),
            SizedBox(height: 24),
            Text(
              'Ödeme Sayfası',
              style: AppTypography.headlineLarge,
            ),
            SizedBox(height: 8),
            Text(
              'Ödeme işlemleri',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
