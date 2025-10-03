import 'package:flutter/material.dart';

class AppTypography {
  // Font Family
  static const String fontFamily = 'Inter'; // Modern, clean font
  
  // Font Weights
  static const FontWeight light = FontWeight.w300;
  static const FontWeight regular = FontWeight.w400;
  static const FontWeight medium = FontWeight.w500;
  static const FontWeight semiBold = FontWeight.w600;
  static const FontWeight bold = FontWeight.w700;
  static const FontWeight extraBold = FontWeight.w800;
  
  // Font Sizes
  static const double fontSize10 = 10.0;
  static const double fontSize12 = 12.0;
  static const double fontSize14 = 14.0;
  static const double fontSize16 = 16.0;
  static const double fontSize18 = 18.0;
  static const double fontSize20 = 20.0;
  static const double fontSize24 = 24.0;
  static const double fontSize28 = 28.0;
  static const double fontSize32 = 32.0;
  static const double fontSize36 = 36.0;
  
  // Text Styles
  static const TextStyle displayLarge = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize36,
    fontWeight: extraBold,
    height: 1.2,
    letterSpacing: -0.5,
  );
  
  static const TextStyle displayMedium = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize32,
    fontWeight: bold,
    height: 1.2,
    letterSpacing: -0.25,
  );
  
  static const TextStyle displaySmall = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize28,
    fontWeight: bold,
    height: 1.3,
    letterSpacing: 0,
  );
  
  static const TextStyle headlineLarge = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize24,
    fontWeight: semiBold,
    height: 1.3,
    letterSpacing: 0,
  );
  
  static const TextStyle headlineMedium = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize20,
    fontWeight: semiBold,
    height: 1.4,
    letterSpacing: 0.15,
  );
  
  static const TextStyle headlineSmall = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize18,
    fontWeight: medium,
    height: 1.4,
    letterSpacing: 0.15,
  );
  
  static const TextStyle titleLarge = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize16,
    fontWeight: medium,
    height: 1.5,
    letterSpacing: 0.15,
  );
  
  static const TextStyle titleMedium = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize14,
    fontWeight: medium,
    height: 1.5,
    letterSpacing: 0.1,
  );
  
  static const TextStyle titleSmall = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: medium,
    height: 1.5,
    letterSpacing: 0.1,
  );
  
  static const TextStyle bodyLarge = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize16,
    fontWeight: regular,
    height: 1.5,
    letterSpacing: 0.5,
  );
  
  static const TextStyle bodyMedium = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize14,
    fontWeight: regular,
    height: 1.5,
    letterSpacing: 0.25,
  );
  
  static const TextStyle bodySmall = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: regular,
    height: 1.5,
    letterSpacing: 0.4,
  );
  
  static const TextStyle labelLarge = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize14,
    fontWeight: medium,
    height: 1.4,
    letterSpacing: 0.1,
  );
  
  static const TextStyle labelMedium = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: medium,
    height: 1.4,
    letterSpacing: 0.5,
  );
  
  static const TextStyle labelSmall = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize10,
    fontWeight: medium,
    height: 1.4,
    letterSpacing: 0.5,
  );
  
  // Custom Text Styles for Getir App
  static const TextStyle appBarTitle = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize18,
    fontWeight: semiBold,
    height: 1.3,
    letterSpacing: 0,
  );
  
  static const TextStyle buttonText = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize16,
    fontWeight: semiBold,
    height: 1.3,
    letterSpacing: 0.1,
  );
  
  static const TextStyle priceText = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize18,
    fontWeight: bold,
    height: 1.3,
    letterSpacing: 0,
  );
  
  static const TextStyle discountText = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: medium,
    height: 1.3,
    letterSpacing: 0.1,
    decoration: TextDecoration.lineThrough,
  );
  
  static const TextStyle categoryTitle = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize16,
    fontWeight: medium,
    height: 1.4,
    letterSpacing: 0.15,
  );
  
  static const TextStyle merchantName = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize18,
    fontWeight: semiBold,
    height: 1.3,
    letterSpacing: 0,
  );
  
  static const TextStyle productName = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize14,
    fontWeight: medium,
    height: 1.4,
    letterSpacing: 0.1,
  );
  
  static const TextStyle orderStatus = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: medium,
    height: 1.3,
    letterSpacing: 0.1,
  );
  
  static const TextStyle deliveryTime = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: regular,
    height: 1.4,
    letterSpacing: 0.25,
  );
  
  static const TextStyle rating = TextStyle(
    fontFamily: fontFamily,
    fontSize: fontSize12,
    fontWeight: medium,
    height: 1.3,
    letterSpacing: 0.1,
  );
  
  // Text Theme
  static const TextTheme textTheme = TextTheme(
    displayLarge: displayLarge,
    displayMedium: displayMedium,
    displaySmall: displaySmall,
    headlineLarge: headlineLarge,
    headlineMedium: headlineMedium,
    headlineSmall: headlineSmall,
    titleLarge: titleLarge,
    titleMedium: titleMedium,
    titleSmall: titleSmall,
    bodyLarge: bodyLarge,
    bodyMedium: bodyMedium,
    bodySmall: bodySmall,
    labelLarge: labelLarge,
    labelMedium: labelMedium,
    labelSmall: labelSmall,
  );
}
