import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class ProfileMenuPage extends StatelessWidget {
  const ProfileMenuPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.white,
      body: SafeArea(
        child: Column(
          children: [
            // Header
            Container(
              height: 56,
              width: double.infinity,
              color: AppColors.primary,
              child: Center(
                child: Text(
                  'Profil',
                  style: AppTypography.headlineSmall.copyWith(
                    color: AppColors.white,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ),

            Expanded(
              child: SingleChildScrollView(
                child: Column(
                  children: [
                    // Menu Items
                    Container(
                      color: AppColors.white,
                      child: Column(
                        children: [
                          _buildMenuItem(
                            icon: Icons.location_on,
                            iconColor: Colors.amber,
                            title: 'Adreslerim',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.favorite,
                            iconColor: AppColors.primary,
                            title: 'Favori Ürünlerim',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.shopping_bag,
                            iconColor: AppColors.primary,
                            title: 'Geçmiş Siparişlerim',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.credit_card,
                            iconColor: AppColors.primary,
                            title: 'Ödeme Yöntemlerim',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.receipt,
                            iconColor: AppColors.primary,
                            title: 'Fatura Bilgilerim',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.notifications,
                            iconColor: AppColors.primary,
                            title: 'İletişim Tercihleri',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.lock,
                            iconColor: AppColors.primary,
                            title: 'Hesap Ayarları',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.chat,
                            iconColor: AppColors.primary,
                            title: 'Canlı Destek',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.help,
                            iconColor: AppColors.primary,
                            title: 'Yardım',
                            onTap: () => _showComingSoon(context),
                          ),
                          _buildMenuItem(
                            icon: Icons.logout,
                            iconColor: AppColors.primary,
                            title: 'Çıkış Yap',
                            onTap: () => _showLogoutDialog(context),
                          ),
                        ],
                      ),
                    ),

                    // Language and Version Section
                    Container(
                      color: AppColors.white,
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 12,
                      ),
                      child: Column(
                        children: [
                          _buildMenuItem(
                            icon: null,
                            iconColor: null,
                            title: 'Dil - Language',
                            subtitle: 'Türkçe',
                            onTap: () => _showComingSoon(context),
                            showChevron: true,
                          ),
                          _buildMenuItem(
                            icon: null,
                            iconColor: null,
                            title: 'Versiyon',
                            subtitle: '25.21.0',
                            onTap: null,
                            showChevron: false,
                          ),
                        ],
                      ),
                    ),

                    const SizedBox(height: 20),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildMenuItem({
    IconData? icon,
    Color? iconColor,
    required String title,
    String? subtitle,
    required VoidCallback? onTap,
    bool showChevron = true,
  }) {
    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        decoration: BoxDecoration(
          border: Border(
            bottom: BorderSide(
              color: AppColors.textSecondary.withOpacity(0.2),
              width: 0.5,
            ),
          ),
        ),
        child: Row(
          children: [
            if (icon != null && iconColor != null) ...[
              Container(
                width: 24,
                height: 24,
                decoration: BoxDecoration(
                  color: iconColor,
                  shape: BoxShape.circle,
                ),
                child: Icon(icon, size: 16, color: AppColors.white),
              ),
              const SizedBox(width: 12),
            ],
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: AppTypography.bodyLarge.copyWith(
                      color: AppColors.textPrimary,
                    ),
                  ),
                  if (subtitle != null) ...[
                    const SizedBox(height: 2),
                    Text(
                      subtitle,
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ],
              ),
            ),
            if (showChevron && onTap != null)
              Icon(
                Icons.chevron_right,
                color: AppColors.textSecondary,
                size: 20,
              ),
          ],
        ),
      ),
    );
  }

  void _showComingSoon(BuildContext context) {
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Bu özellik yakında gelecek!'),
        backgroundColor: AppColors.primary,
      ),
    );
  }

  void _showLogoutDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Çıkış Yap'),
        content: const Text('Hesabınızdan çıkmak istediğinizden emin misiniz?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('İptal'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              _performLogout(context);
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              foregroundColor: AppColors.white,
            ),
            child: const Text('Çıkış Yap'),
          ),
        ],
      ),
    );
  }

  void _performLogout(BuildContext context) {
    // TODO: Implement logout logic
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Çıkış yapıldı'),
        backgroundColor: AppColors.primary,
      ),
    );
  }
}
