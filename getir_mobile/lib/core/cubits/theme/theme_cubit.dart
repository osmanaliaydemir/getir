import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../services/logger_service.dart';

part 'theme_state.dart';

/// ThemeCubit
///
/// Manages app theme (Light/Dark/System) using BLoC pattern
/// Replaces ThemeProvider for consistent state management architecture
class ThemeCubit extends Cubit<ThemeState> {
  static const String _themePreferenceKey = 'theme_mode';
  final SharedPreferences _prefs;

  ThemeCubit(this._prefs)
    : super(const ThemeState(themeMode: ThemeMode.system)) {
    _loadThemePreference();
  }

  /// Load theme preference from SharedPreferences
  Future<void> _loadThemePreference() async {
    try {
      final themeIndex = _prefs.getInt(_themePreferenceKey) ?? 0; // 0 = system
      final themeMode = ThemeMode.values[themeIndex];
      emit(ThemeState(themeMode: themeMode));
      logger.info(
        'Loaded saved theme',
        tag: 'ThemeCubit',
        context: {'themeMode': themeMode.name},
      );
    } catch (e, stackTrace) {
      logger.error(
        'Failed to load theme preference',
        tag: 'ThemeCubit',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  /// Set theme mode
  Future<void> setThemeMode(ThemeMode mode) async {
    if (state.themeMode == mode) return;

    emit(ThemeState(themeMode: mode));
    await _prefs.setInt(_themePreferenceKey, mode.index);

    logger.info(
      'Theme mode changed',
      tag: 'ThemeCubit',
      context: {'themeMode': mode.name},
    );
  }

  /// Toggle theme (Light → Dark → System → Light)
  Future<void> toggleTheme() async {
    if (state.themeMode == ThemeMode.light) {
      await setThemeMode(ThemeMode.dark);
    } else if (state.themeMode == ThemeMode.dark) {
      await setThemeMode(ThemeMode.system);
    } else {
      await setThemeMode(ThemeMode.light);
    }
  }

  /// Set light mode
  Future<void> setLightMode() async {
    await setThemeMode(ThemeMode.light);
  }

  /// Set dark mode
  Future<void> setDarkMode() async {
    await setThemeMode(ThemeMode.dark);
  }

  /// Set system mode
  Future<void> setSystemMode() async {
    await setThemeMode(ThemeMode.system);
  }
}
