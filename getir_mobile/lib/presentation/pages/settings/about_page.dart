import 'package:flutter/material.dart';
import 'package:package_info_plus/package_info_plus.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

/// About Page
///
/// Displays app information, version, licenses, and legal links
class AboutPage extends StatefulWidget {
  const AboutPage({super.key});

  @override
  State<AboutPage> createState() => _AboutPageState();
}

class _AboutPageState extends State<AboutPage> {
  PackageInfo? _packageInfo;

  @override
  void initState() {
    super.initState();
    _loadPackageInfo();
  }

  Future<void> _loadPackageInfo() async {
    final info = await PackageInfo.fromPlatform();
    setState(() => _packageInfo = info);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('About'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: _packageInfo == null
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  // App Logo & Name
                  Container(
                    width: 120,
                    height: 120,
                    decoration: BoxDecoration(
                      color: AppColors.primary.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(24),
                    ),
                    child: const Icon(
                      Icons.local_shipping,
                      size: 64,
                      color: AppColors.primary,
                    ),
                  ),

                  const SizedBox(height: 24),

                  Text(
                    'Getir Mobile',
                    style: AppTypography.headlineMedium.copyWith(
                      fontWeight: FontWeight.bold,
                    ),
                  ),

                  const SizedBox(height: 8),

                  Text(
                    'Fast Delivery at Your Fingertips',
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),

                  const SizedBox(height: 32),

                  // Version Info Card
                  _buildInfoCard(
                    title: 'Version Information',
                    children: [
                      _buildInfoRow('Version', _packageInfo!.version),
                      _buildInfoRow('Build Number', _packageInfo!.buildNumber),
                      _buildInfoRow('Package Name', _packageInfo!.packageName),
                    ],
                  ),

                  const SizedBox(height: 16),

                  // App Info Card
                  _buildInfoCard(
                    title: 'Application Info',
                    children: [
                      _buildInfoRow(
                        'Platform',
                        Theme.of(context).platform.name,
                      ),
                      _buildInfoRow('Framework', 'Flutter 3.19.0'),
                      _buildInfoRow('Build Mode', _getBuildMode()),
                    ],
                  ),

                  const SizedBox(height: 16),

                  // Legal Links Card
                  _buildInfoCard(
                    title: 'Legal & Support',
                    children: [
                      _buildLinkTile(
                        icon: Icons.description_outlined,
                        title: 'Terms of Service',
                        onTap: () => _openUrl('https://getir.com/terms'),
                      ),
                      const Divider(),
                      _buildLinkTile(
                        icon: Icons.privacy_tip_outlined,
                        title: 'Privacy Policy',
                        onTap: () => _openUrl('https://getir.com/privacy'),
                      ),
                      const Divider(),
                      _buildLinkTile(
                        icon: Icons.gavel_outlined,
                        title: 'Open Source Licenses',
                        onTap: () => _showLicenses(),
                      ),
                      const Divider(),
                      _buildLinkTile(
                        icon: Icons.support_agent_outlined,
                        title: 'Support & Help',
                        onTap: () => _openUrl('https://getir.com/support'),
                      ),
                    ],
                  ),

                  const SizedBox(height: 16),

                  // Social & Contact
                  _buildInfoCard(
                    title: 'Connect With Us',
                    children: [
                      _buildLinkTile(
                        icon: Icons.web_outlined,
                        title: 'Website',
                        subtitle: 'www.getir.com',
                        onTap: () => _openUrl('https://getir.com'),
                      ),
                      const Divider(),
                      _buildLinkTile(
                        icon: Icons.email_outlined,
                        title: 'Email',
                        subtitle: 'support@getir.com',
                        onTap: () => _openUrl('mailto:support@getir.com'),
                      ),
                    ],
                  ),

                  const SizedBox(height: 32),

                  // Copyright
                  Text(
                    '© 2025 Getir. All rights reserved.',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                    textAlign: TextAlign.center,
                  ),

                  const SizedBox(height: 8),

                  Text(
                    'Made with ❤️ in Turkey',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
    );
  }

  Widget _buildInfoCard({
    required String title,
    required List<Widget> children,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.all(16),
            child: Text(
              title,
              style: AppTypography.bodyLarge.copyWith(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          const Divider(height: 1),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(children: children),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
          Text(
            value,
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLinkTile({
    required IconData icon,
    required String title,
    String? subtitle,
    required VoidCallback onTap,
  }) {
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(icon, color: AppColors.primary),
      title: Text(title),
      subtitle: subtitle != null ? Text(subtitle) : null,
      trailing: const Icon(Icons.chevron_right, size: 20),
      onTap: onTap,
    );
  }

  String _getBuildMode() {
    if (const bool.fromEnvironment('dart.vm.product')) {
      return 'Release';
    }
    return 'Debug';
  }

  void _openUrl(String url) {
    // TODO: Implement url_launcher
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text('Opening: $url')));
  }

  void _showLicenses() {
    showLicensePage(
      context: context,
      applicationName: 'Getir Mobile',
      applicationVersion: _packageInfo?.version ?? '1.0.0',
      applicationIcon: Container(
        width: 80,
        height: 80,
        decoration: BoxDecoration(
          color: AppColors.primary.withOpacity(0.1),
          borderRadius: BorderRadius.circular(16),
        ),
        child: const Icon(
          Icons.local_shipping,
          size: 40,
          color: AppColors.primary,
        ),
      ),
      applicationLegalese: '© 2025 Getir. All rights reserved.',
    );
  }
}
