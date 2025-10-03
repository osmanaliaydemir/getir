import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class AddressesPage extends StatelessWidget {
  const AddressesPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Adreslerim'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.location_on,
              size: 100,
              color: AppColors.primary,
            ),
            SizedBox(height: 24),
            Text(
              'Adreslerim',
              style: AppTypography.headlineLarge,
            ),
            SizedBox(height: 8),
            Text(
              'Kayıtlı adresleriniz',
              style: AppTypography.bodyLarge,
            ),
          ],
        ),
      ),
    );
  }
}
