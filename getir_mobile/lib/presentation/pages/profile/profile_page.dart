import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../bloc/profile/profile_bloc.dart';
import 'edit_profile_page.dart';
import 'favorite_products_page.dart';
import 'order_history_page.dart';
import 'notification_preferences_page.dart';
import 'account_settings_page.dart';
import 'language_settings_page.dart';

class ProfilePage extends StatefulWidget {
  const ProfilePage({super.key});

  @override
  State<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends State<ProfilePage> {
  @override
  void initState() {
    super.initState();
    context.read<ProfileBloc>().add(LoadProfile());
  }

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
              child: BlocConsumer<ProfileBloc, ProfileState>(
                listener: (context, state) {
                  if (state is ProfileError) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(state.message),
                        backgroundColor: AppColors.error,
                      ),
                    );
                  }
                },
                builder: (context, state) {
                  if (state is ProfileLoading || state is ProfileInitial) {
                    return const Center(
                      child: CircularProgressIndicator(
                        valueColor: AlwaysStoppedAnimation<Color>(
                          AppColors.primary,
                        ),
                      ),
                    );
                  }

                  if (state is ProfileError) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(
                            Icons.error_outline,
                            color: AppColors.error,
                            size: 56,
                          ),
                          const SizedBox(height: 16),
                          Text(
                            'Profil yüklenirken hata oluştu',
                            style: AppTypography.bodyLarge.copyWith(
                              color: AppColors.error,
                            ),
                          ),
                          const SizedBox(height: 16),
                          ElevatedButton(
                            onPressed: () =>
                                context.read<ProfileBloc>().add(LoadProfile()),
                            child: const Text('Tekrar Dene'),
                          ),
                        ],
                      ),
                    );
                  }

                  if (state is ProfileLoaded) {
                    return _buildProfileContent(context, state);
                  }

                  return const SizedBox.shrink();
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProfileContent(BuildContext context, ProfileLoaded state) {
    return SingleChildScrollView(
      child: Column(
        children: [
          // User Info Section
          Container(
            color: AppColors.white,
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                // Profile Picture
                Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: AppColors.primary.withOpacity(0.1),
                    shape: BoxShape.circle,
                  ),
                  child: state.profile.avatarUrl != null
                      ? ClipOval(
                          child: Image.network(
                            state.profile.avatarUrl!,
                            fit: BoxFit.cover,
                          ),
                        )
                      : Icon(Icons.person, size: 40, color: AppColors.primary),
                ),

                const SizedBox(width: 16),

                // User Details
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '${state.profile.firstName} ${state.profile.lastName}',
                        style: AppTypography.headlineSmall.copyWith(
                          fontWeight: FontWeight.w600,
                          color: AppColors.textPrimary,
                        ),
                      ),

                      const SizedBox(height: 8),

                      // Email
                      Row(
                        children: [
                          Container(
                            width: 16,
                            height: 16,
                            decoration: BoxDecoration(
                              color: Colors.amber,
                              shape: BoxShape.circle,
                            ),
                            child: const Icon(
                              Icons.email,
                              size: 10,
                              color: AppColors.white,
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              state.profile.email,
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ),
                        ],
                      ),

                      if (state.profile.phoneNumber != null) ...[
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            Container(
                              width: 16,
                              height: 16,
                              decoration: BoxDecoration(
                                color: Colors.amber,
                                shape: BoxShape.circle,
                              ),
                              child: const Icon(
                                Icons.phone,
                                size: 10,
                                color: AppColors.white,
                              ),
                            ),
                            const SizedBox(width: 8),
                            Text(
                              state.profile.phoneNumber!,
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),

                // Edit Button
                GestureDetector(
                  onTap: () => _navigateToEditProfile(context),
                  child: Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: AppColors.primary.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Icon(Icons.edit, color: AppColors.primary, size: 20),
                  ),
                ),
              ],
            ),
          ),

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
                  onTap: () => _navigateToFavoriteProducts(context),
                ),
                _buildMenuItem(
                  icon: Icons.shopping_bag,
                  iconColor: AppColors.primary,
                  title: 'Geçmiş Siparişlerim',
                  onTap: () => _navigateToOrderHistory(context),
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
                  onTap: () => _navigateToNotificationPreferences(context),
                ),
                _buildMenuItem(
                  icon: Icons.lock,
                  iconColor: AppColors.primary,
                  title: 'Hesap Ayarları',
                  onTap: () => _navigateToAccountSettings(context),
                ),
                _buildMenuItem(
                  icon: Icons.chat,
                  iconColor: AppColors.primary,
                  title: 'Canlı Destek',
                  onTap: () => _showComingSoon(context),
                ),
              ],
            ),
          ),

          // Language and Version Section
          Container(
            color: AppColors.white,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: Column(
              children: [
                _buildMenuItem(
                  icon: null,
                  iconColor: null,
                  title: 'Dil - Language',
                  subtitle: 'Türkçe',
                  onTap: () => _navigateToLanguageSettings(context),
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

  void _navigateToEditProfile(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const EditProfilePage()),
    );
  }

  void _navigateToFavoriteProducts(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const FavoriteProductsPage()),
    );
  }

  void _navigateToOrderHistory(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const OrderHistoryPage()),
    );
  }

  void _navigateToNotificationPreferences(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => const NotificationPreferencesPage(),
      ),
    );
  }

  void _navigateToAccountSettings(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const AccountSettingsPage()),
    );
  }

  void _navigateToLanguageSettings(BuildContext context) {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const LanguageSettingsPage()),
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
}
