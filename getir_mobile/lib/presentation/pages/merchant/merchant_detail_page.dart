import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class MerchantDetailPage extends StatelessWidget {
  final String merchantId;
  
  const MerchantDetailPage({
    super.key,
    required this.merchantId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Mağaza $merchantId'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(
              Icons.store,
              size: 100,
              color: AppColors.primary,
            ),
            const SizedBox(height: 24),
            Text(
              'Mağaza Detayı',
              style: AppTypography.headlineLarge,
            ),
            const SizedBox(height: 8),
            Text(
              'Mağaza ID: $merchantId',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
