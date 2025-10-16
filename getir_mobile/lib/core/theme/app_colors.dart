import 'package:flutter/material.dart';

class AppColors {
  // Getir Brand Colors
  static const Color primary = Color(0xFF5D3EBC); // Getir Purple
  static const Color primaryDark = Color(0xFF4A2E9A);
  static const Color primaryLight = Color(0xFF7B5ED4);

  // Secondary Colors
  static const Color secondary = Color(0xFF00C851); // Success Green
  static const Color secondaryDark = Color(0xFF00A041);
  static const Color secondaryLight = Color(0xFF33D473);

  // Accent Colors
  static const Color accent = Color(0xFF2196F3); // Info Blue
  static const Color accentDark = Color(0xFF1976D2);
  static const Color accentLight = Color(0xFF64B5F6);

  // Neutral Colors
  static const Color white = Color(0xFFFFFFFF);
  static const Color black = Color(0xFF000000);
  static const Color grey = Color(0xFF9E9E9E);
  static const Color greyLight = Color(0xFFF5F5F5);
  static const Color greyDark = Color(0xFF616161);

  // Text Colors
  static const Color textPrimary = Color(0xFF212121);
  static const Color textSecondary = Color(0xFF757575);
  static const Color textHint = Color(0xFFBDBDBD);
  static const Color textDisabled = Color(0xFFE0E0E0);
  static const Color textOnPrimary = Color(0xFFFFFFFF);
  static const Color textOnSecondary = Color(0xFFFFFFFF);

  // Background Colors
  static const Color background = Color(0xFFFAFAFA);
  static const Color surface = Color(0xFFFFFFFF);
  static const Color surfaceVariant = Color(0xFFF5F5F5);

  // Status Colors
  static const Color success = Color(0xFF4CAF50);
  static const Color warning = Color(0xFFFF9800);
  static const Color error = Color(0xFFF44336);
  static const Color info = Color(0xFF2196F3);

  // Order Status Colors
  static const Color orderPending = Color(0xFFFF9800);
  static const Color orderConfirmed = Color(0xFF2196F3);
  static const Color orderPreparing = Color(0xFF9C27B0);
  static const Color orderOnTheWay = Color(0xFF3F51B5);
  static const Color orderDelivered = Color(0xFF4CAF50);
  static const Color orderCancelled = Color(0xFFF44336);

  // Payment Status Colors
  static const Color paymentPending = Color(0xFFFF9800);
  static const Color paymentCompleted = Color(0xFF4CAF50);
  static const Color paymentFailed = Color(0xFFF44336);
  static const Color paymentCancelled = Color(0xFF9E9E9E);

  // Category Colors
  static const Color categoryMarket = Color(0xFF4CAF50);
  static const Color categoryFood = Color(0xFFFF5722);
  static const Color categoryWater = Color(0xFF2196F3);
  static const Color categoryPharmacy = Color(0xFF9C27B0);
  static const Color categoryPet = Color(0xFF795548);

  // Shadow Colors
  static const Color shadowLight = Color(0x1A000000);
  static const Color shadowMedium = Color(0x33000000);
  static const Color shadowDark = Color(0x4D000000);

  // Gradient Colors
  static const LinearGradient primaryGradient = LinearGradient(
    colors: [primary, primaryDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static const LinearGradient secondaryGradient = LinearGradient(
    colors: [secondary, secondaryDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  // Material Design 3 Colors
  static const ColorScheme lightColorScheme = ColorScheme(
    brightness: Brightness.light,
    primary: primary,
    onPrimary: textOnPrimary,
    secondary: secondary,
    onSecondary: textOnSecondary,
    error: error,
    onError: white,
    surface: surface,
    onSurface: textPrimary,
    outline: grey,
    shadow: shadowLight,
    surfaceTint: primary,
  );

  static const ColorScheme darkColorScheme = ColorScheme(
    brightness: Brightness.dark,
    primary: primaryLight,
    onPrimary: black,
    secondary: secondaryLight,
    onSecondary: black,
    error: error,
    onError: white,
    surface: Color(0xFF1E1E1E),
    onSurface: white,
    outline: grey,
    shadow: shadowDark,
    surfaceTint: primaryLight,
  );

  // Dark Theme Specific Colors
  static const Color darkBackground = Color(0xFF121212);
  static const Color darkSurface = Color(0xFF1E1E1E);
  static const Color darkSurfaceVariant = Color(0xFF2C2C2C);
  static const Color darkTextPrimary = Color(0xFFE1E1E1);
  static const Color darkTextSecondary = Color(0xFFB0B0B0);
}

/// Extension to get theme-aware colors from context
extension AppColorsExtension on BuildContext {
  bool get isDarkMode => Theme.of(this).brightness == Brightness.dark;

  Color get scaffoldBackground {
    return isDarkMode ? AppColors.darkBackground : AppColors.background;
  }

  Color get cardBackground {
    return isDarkMode ? AppColors.darkSurface : AppColors.surface;
  }

  Color get textPrimaryColor {
    return isDarkMode ? AppColors.darkTextPrimary : AppColors.textPrimary;
  }

  Color get textSecondaryColor {
    return isDarkMode ? AppColors.darkTextSecondary : AppColors.textSecondary;
  }

  Color get primaryColor {
    return Theme.of(this).colorScheme.primary;
  }

  Color get onPrimaryColor {
    return Theme.of(this).colorScheme.onPrimary;
  }

  Color get surfaceColor {
    return Theme.of(this).colorScheme.surface;
  }

  Color get onSurfaceColor {
    return Theme.of(this).colorScheme.onSurface;
  }
}
