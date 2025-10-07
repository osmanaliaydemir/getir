import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../../core/providers/theme_provider.dart';
import '../../../core/theme/app_typography.dart';

class ThemeSwitcher extends StatelessWidget {
  final bool showLabel;

  const ThemeSwitcher({super.key, this.showLabel = true});

  @override
  Widget build(BuildContext context) {
    return Consumer<ThemeProvider>(
      builder: (context, themeProvider, child) {
        return Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _buildThemeOption(
              context: context,
              themeProvider: themeProvider,
              mode: ThemeMode.light,
              icon: Icons.light_mode,
              label: 'Açık Tema',
            ),
            _buildThemeOption(
              context: context,
              themeProvider: themeProvider,
              mode: ThemeMode.dark,
              icon: Icons.dark_mode,
              label: 'Koyu Tema',
            ),
            _buildThemeOption(
              context: context,
              themeProvider: themeProvider,
              mode: ThemeMode.system,
              icon: Icons.brightness_auto,
              label: 'Sistem Teması',
            ),
          ],
        );
      },
    );
  }

  Widget _buildThemeOption({
    required BuildContext context,
    required ThemeProvider themeProvider,
    required ThemeMode mode,
    required IconData icon,
    required String label,
  }) {
    final isSelected = themeProvider.themeMode == mode;
    final theme = Theme.of(context);

    return RadioListTile<ThemeMode>(
      value: mode,
      groupValue: themeProvider.themeMode,
      onChanged: (ThemeMode? value) {
        if (value != null) {
          themeProvider.setThemeMode(value);
        }
      },
      title: Row(
        children: [
          Icon(
            icon,
            color: isSelected
                ? theme.colorScheme.primary
                : theme.colorScheme.onSurfaceVariant,
          ),
          const SizedBox(width: 12),
          Text(
            label,
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: isSelected ? FontWeight.w600 : FontWeight.normal,
              color: isSelected
                  ? theme.colorScheme.primary
                  : theme.colorScheme.onSurface,
            ),
          ),
        ],
      ),
      activeColor: theme.colorScheme.primary,
    );
  }
}

/// Quick theme toggle button (for app bar, etc.)
class ThemeToggleButton extends StatelessWidget {
  const ThemeToggleButton({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<ThemeProvider>(
      builder: (context, themeProvider, child) {
        final isDark = Theme.of(context).brightness == Brightness.dark;

        return IconButton(
          icon: Icon(isDark ? Icons.light_mode : Icons.dark_mode),
          onPressed: () {
            if (isDark) {
              themeProvider.setLightMode();
            } else {
              themeProvider.setDarkMode();
            }
          },
          tooltip: isDark ? 'Açık Temaya Geç' : 'Koyu Temaya Geç',
        );
      },
    );
  }
}

/// Theme selector bottom sheet
class ThemeSelectorBottomSheet extends StatelessWidget {
  const ThemeSelectorBottomSheet({super.key});

  static Future<void> show(BuildContext context) {
    return showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (context) => const ThemeSelectorBottomSheet(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.symmetric(vertical: 20),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // Handle bar
          Container(
            width: 40,
            height: 4,
            decoration: BoxDecoration(
              color: theme.colorScheme.onSurfaceVariant.withOpacity(0.4),
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          const SizedBox(height: 20),

          // Title
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: Row(
              children: [
                Icon(Icons.palette, color: theme.colorScheme.primary),
                const SizedBox(width: 12),
                Text(
                  'Tema Seçimi',
                  style: AppTypography.headlineSmall.copyWith(
                    color: theme.colorScheme.onSurface,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // Theme options
          const ThemeSwitcher(showLabel: true),

          const SizedBox(height: 8),
        ],
      ),
    );
  }
}
