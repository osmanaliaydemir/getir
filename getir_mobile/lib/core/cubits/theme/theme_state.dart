part of 'theme_cubit.dart';

/// ThemeState
///
/// Represents current app theme mode
class ThemeState extends Equatable {
  final ThemeMode themeMode;

  const ThemeState({required this.themeMode});

  bool get isDarkMode => themeMode == ThemeMode.dark;
  bool get isLightMode => themeMode == ThemeMode.light;
  bool get isSystemMode => themeMode == ThemeMode.system;

  String get themeModeString {
    switch (themeMode) {
      case ThemeMode.light:
        return 'Açık Tema';
      case ThemeMode.dark:
        return 'Koyu Tema';
      case ThemeMode.system:
        return 'Sistem Teması';
    }
  }

  @override
  List<Object?> get props => [themeMode];
}
