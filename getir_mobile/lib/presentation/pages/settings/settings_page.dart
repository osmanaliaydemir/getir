import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../core/navigation/app_navigation.dart';
import '../../../core/cubits/language/language_cubit.dart';
import '../../../core/cubits/theme/theme_cubit.dart';
import '../../bloc/auth/auth_bloc.dart';
import '../../widgets/common/theme_switcher.dart';

class SettingsPage extends StatelessWidget {
  const SettingsPage({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(l10n.settings),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // Account Section
          _buildSectionHeader(l10n.account, Icons.person),
          _buildSettingsTile(
            icon: Icons.person_outline,
            title: l10n.profile,
            subtitle: l10n.manageYourProfile,
            onTap: () => AppNavigation.goToProfile(context),
          ),
          _buildSettingsTile(
            icon: Icons.location_on_outlined,
            title: l10n.addresses,
            subtitle: l10n.manageYourAddresses,
            onTap: () => AppNavigation.goToAddresses(context),
          ),
          _buildSettingsTile(
            icon: Icons.receipt_long_outlined,
            title: l10n.orderHistory,
            subtitle: l10n.viewYourOrders,
            onTap: () => AppNavigation.goToOrders(context),
          ),

          const SizedBox(height: 24),

          // Notifications Section
          _buildSectionHeader(l10n.notifications, Icons.notifications_outlined),
          _buildSettingsTile(
            icon: Icons.notifications_outlined,
            title: l10n.notificationSettings,
            subtitle: l10n.customizeNotifications,
            onTap: () => AppNavigation.goToNotificationSettings(context),
          ),

          const SizedBox(height: 24),

          // App Settings Section
          _buildSectionHeader(l10n.appSettings, Icons.settings_outlined),
          _buildLanguageTile(context),
          _buildThemeTile(context),

          const SizedBox(height: 24),

          // Support Section
          _buildSectionHeader(l10n.support, Icons.help_outline),
          _buildSettingsTile(
            icon: Icons.help_outline,
            title: l10n.helpCenter,
            subtitle: l10n.getHelpAndSupport,
            onTap: () => _launchHelpCenter(context),
          ),
          _buildSettingsTile(
            icon: Icons.contact_support_outlined,
            title: l10n.contactUs,
            subtitle: l10n.getInTouch,
            onTap: () => _launchContactUs(context),
          ),
          _buildSettingsTile(
            icon: Icons.info_outline,
            title: l10n.about,
            subtitle: l10n.appVersion,
            onTap: () => _showAboutDialog(context),
          ),

          const SizedBox(height: 32),

          // Logout Button
          BlocListener<AuthBloc, AuthState>(
            listener: (context, state) {
              if (state is AuthUnauthenticated) {
                AppNavigation.goToLogin(context);
              } else if (state is AuthError) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(state.message),
                    backgroundColor: AppColors.error,
                  ),
                );
              }
            },
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () => _showLogoutDialog(context),
                icon: const Icon(Icons.logout),
                label: Text(l10n.logout),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.error,
                  foregroundColor: AppColors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ),
          ),

          const SizedBox(height: 16),
        ],
      ),
    );
  }

  Widget _buildSectionHeader(String title, IconData icon) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 20),
          const SizedBox(width: 8),
          Text(
            title,
            style: AppTypography.bodyLarge.copyWith(
              fontWeight: FontWeight.w600,
              color: AppColors.primary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSettingsTile({
    required IconData icon,
    required String title,
    required String subtitle,
    Widget? trailing,
    VoidCallback? onTap,
  }) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 4,
            offset: const Offset(0, 1),
          ),
        ],
      ),
      child: ListTile(
        leading: Icon(icon, color: AppColors.textSecondary),
        title: Text(title, style: AppTypography.bodyLarge),
        subtitle: Text(subtitle, style: AppTypography.bodySmall),
        trailing: trailing ?? const Icon(Icons.arrow_forward_ios, size: 16),
        onTap: onTap,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      ),
    );
  }

  Widget _buildLanguageTile(BuildContext context) {
    return BlocBuilder<LanguageCubit, LanguageState>(
      builder: (context, state) {
        return Container(
          margin: const EdgeInsets.only(bottom: 8),
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(12),
            boxShadow: [
              BoxShadow(
                color: Colors.grey.withOpacity(0.1),
                spreadRadius: 1,
                blurRadius: 4,
                offset: const Offset(0, 1),
              ),
            ],
          ),
          child: ListTile(
            leading: const Icon(Icons.language, color: AppColors.textSecondary),
            title: Text(
              AppLocalizations.of(context).language,
              style: AppTypography.bodyLarge,
            ),
            subtitle: Text(
              _getLanguageName(state.languageCode),
              style: AppTypography.bodySmall,
            ),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () => _showLanguageDialog(context),
            contentPadding: const EdgeInsets.symmetric(
              horizontal: 16,
              vertical: 8,
            ),
          ),
        );
      },
    );
  }

  String _getLanguageName(String languageCode) {
    switch (languageCode) {
      case 'tr':
        return 'Türkçe';
      case 'en':
        return 'English';
      default:
        return 'Türkçe';
    }
  }

  void _showLanguageDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(AppLocalizations.of(context).selectLanguage),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              title: const Text('Türkçe'),
              onTap: () {
                context.read<LanguageCubit>().changeLanguageByCode('tr');
                Navigator.pop(context);
              },
            ),
            ListTile(
              title: const Text('English'),
              onTap: () {
                context.read<LanguageCubit>().changeLanguageByCode('en');
                Navigator.pop(context);
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildThemeTile(BuildContext context) {
    return BlocBuilder<ThemeCubit, ThemeState>(
      builder: (context, state) {
        return Container(
          margin: const EdgeInsets.only(bottom: 8),
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(12),
            boxShadow: [
              BoxShadow(
                color: Colors.grey.withOpacity(0.1),
                spreadRadius: 1,
                blurRadius: 4,
                offset: const Offset(0, 1),
              ),
            ],
          ),
          child: ListTile(
            leading: Icon(
              _getThemeIcon(state.themeMode),
              color: AppColors.textSecondary,
            ),
            title: Text(
              AppLocalizations.of(context).theme,
              style: AppTypography.bodyLarge,
            ),
            subtitle: Text(
              state.themeModeString,
              style: AppTypography.bodySmall,
            ),
            trailing: const Icon(Icons.arrow_forward_ios, size: 16),
            onTap: () => showThemeSelector(context),
            contentPadding: const EdgeInsets.symmetric(
              horizontal: 16,
              vertical: 8,
            ),
          ),
        );
      },
    );
  }

  IconData _getThemeIcon(ThemeMode mode) {
    switch (mode) {
      case ThemeMode.light:
        return Icons.light_mode;
      case ThemeMode.dark:
        return Icons.dark_mode;
      case ThemeMode.system:
        return Icons.brightness_auto;
    }
  }

  void _showLogoutDialog(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.logout),
        content: Text(l10n.logoutConfirmation),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(l10n.cancel),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              context.read<AuthBloc>().add(AuthLogoutRequested());
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.error,
              foregroundColor: AppColors.white,
            ),
            child: Text(l10n.logout),
          ),
        ],
      ),
    );
  }

  void _launchHelpCenter(BuildContext context) {
    // TODO: Implement help center launch
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(AppLocalizations.of(context).comingSoon)),
    );
  }

  void _launchContactUs(BuildContext context) {
    // TODO: Implement contact us launch
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(AppLocalizations.of(context).comingSoon)),
    );
  }

  void _showAboutDialog(BuildContext context) {
    // Navigate to About Page
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const AboutPage()),
    );
  }
}

class AboutPage extends StatefulWidget {
  const AboutPage({super.key});

  @override
  State<AboutPage> createState() => _AboutPageState();
}

class _AboutPageState extends State<AboutPage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('About'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: const Center(
        child: Text('About page - See about_page.dart for full implementation'),
      ),
    );
  }
}
