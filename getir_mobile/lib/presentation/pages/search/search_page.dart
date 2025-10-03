import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class SearchPage extends StatelessWidget {
  const SearchPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Ara'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.search,
              size: 100,
              color: AppColors.primary,
            ),
            SizedBox(height: 24),
            Text(
              'Arama Sayfası',
              style: AppTypography.headlineLarge,
            ),
            SizedBox(height: 8),
            Text(
              'Ürün ve mağaza arama',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
