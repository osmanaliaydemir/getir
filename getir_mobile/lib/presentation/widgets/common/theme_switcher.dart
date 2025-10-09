import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/cubits/theme/theme_cubit.dart';
import '../../../core/theme/app_typography.dart';

class ThemeSwitcher extends StatelessWidget {
  final bool showLabel;

  const ThemeSwitcher({super.key, this.showLabel = true});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ThemeCubit, ThemeState>(
      builder: (context, state) {
        return Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _buildThemeOption(
              context: context,
              state: state,
              mode: ThemeMode.light,
              icon: Icons.light_mode,
              label: 'Açık Tema',
            ),
            _buildThemeOption(
              context: context,
              state: state,
              mode: ThemeMode.dark,
              icon: Icons.dark_mode,
              label: 'Koyu Tema',
            ),
            _buildThemeOption(
              context: context,
              state: state,
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
    required ThemeState state,
    required ThemeMode mode,
    required IconData icon,
    required String label,
  }) {
    final isSelected = state.themeMode == mode;
    final theme = Theme.of(context);

    return RadioListTile<ThemeMode>(
      value: mode,
      groupValue: state.themeMode,
      onChanged: (ThemeMode? value) {
        if (value != null) {
          context.read<ThemeCubit>().setThemeMode(value);
        }
      },
      title: Row(
        children: [
          Icon(icon, size: 24),
          if (showLabel) ...[
            const SizedBox(width: 12),
            Text(
              label,
              style: AppTypography.bodyMedium.copyWith(
                fontWeight: isSelected ? FontWeight.w600 : FontWeight.normal,
              ),
            ),
          ],
        ],
      ),
      activeColor: theme.colorScheme.primary,
      dense: true,
    );
  }
}

/// Quick theme toggle button (for app bar, etc.)
class ThemeToggleButton extends StatelessWidget {
  const ThemeToggleButton({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ThemeCubit, ThemeState>(
      builder: (context, state) {
        final isDark = Theme.of(context).brightness == Brightness.dark;

        return IconButton(
          icon: Icon(isDark ? Icons.light_mode : Icons.dark_mode),
          onPressed: () {
            if (isDark) {
              context.read<ThemeCubit>().setLightMode();
            } else {
              context.read<ThemeCubit>().setDarkMode();
            }
          },
          tooltip: isDark ? 'Açık Temaya Geç' : 'Koyu Temaya Geç',
        );
      },
    );
  }
}

/// Theme selector bottom sheet
void showThemeSelector(BuildContext context) {
  showModalBottomSheet(
    context: context,
    shape: const RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
    ),
    builder: (context) => Padding(
      padding: const EdgeInsets.symmetric(vertical: 16),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: const [
          Padding(
            padding: EdgeInsets.all(16),
            child: Text(
              'Tema Seçin',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
          ),
          ThemeSwitcher(),
        ],
      ),
    ),
  );
}
