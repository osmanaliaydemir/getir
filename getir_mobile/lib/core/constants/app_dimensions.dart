/// AppDimensions
///
/// Centralized dimension constants for consistent UI spacing and sizing
/// Eliminates magic numbers and improves maintainability
class AppDimensions {
  AppDimensions._();

  // ==================== SPACING ====================

  /// Extra small spacing (4.0)
  static const double spacingXs = 4.0;

  /// Small spacing (8.0)
  static const double spacingS = 8.0;

  /// Medium spacing (12.0)
  static const double spacingM = 12.0;

  /// Large spacing (16.0)
  static const double spacingL = 16.0;

  /// Extra large spacing (24.0)
  static const double spacingXl = 24.0;

  /// Extra extra large spacing (32.0)
  static const double spacingXxl = 32.0;

  // ==================== BORDER RADIUS ====================

  /// Small border radius (8.0)
  static const double radiusS = 8.0;

  /// Medium border radius (12.0)
  static const double radiusM = 12.0;

  /// Large border radius (16.0)
  static const double radiusL = 16.0;

  /// Extra large border radius (24.0)
  static const double radiusXl = 24.0;

  /// Circular border radius (999.0)
  static const double radiusCircular = 999.0;

  // ==================== ICON SIZES ====================

  /// Small icon size (16.0)
  static const double iconS = 16.0;

  /// Medium icon size (24.0)
  static const double iconM = 24.0;

  /// Large icon size (32.0)
  static const double iconL = 32.0;

  /// Extra large icon size (48.0)
  static const double iconXl = 48.0;

  /// Extra extra large icon size (64.0)
  static const double iconXxl = 64.0;

  // ==================== CARD DIMENSIONS ====================

  /// Standard card elevation (2.0)
  static const double cardElevation = 2.0;

  /// Card border radius (12.0)
  static const double cardBorderRadius = 12.0;

  /// Product card width (150.0)
  static const double productCardWidth = 150.0;

  /// Product card height (200.0)
  static const double productCardHeight = 200.0;

  /// Merchant card height (120.0)
  static const double merchantCardHeight = 120.0;

  // ==================== GRID ====================

  /// Categories grid cross axis count (4)
  static const int categoriesGridCount = 4;

  /// Products grid cross axis count (2)
  static const int productsGridCount = 2;

  // ==================== SHADOW ====================

  /// Shadow opacity (0.1)
  static const double shadowOpacity = 0.1;

  /// Shadow spread radius (1.0)
  static const double shadowSpreadRadius = 1.0;

  /// Shadow blur radius (4.0)
  static const double shadowBlurRadius = 4.0;

  /// Shadow offset Y (2.0)
  static const double shadowOffsetY = 2.0;

  // ==================== LIST ====================

  /// Popular products list height (200.0)
  static const double popularProductsHeight = 200.0;

  /// List item spacing (16.0)
  static const double listItemSpacing = 16.0;

  // ==================== BUSINESS LOGIC ====================

  /// Nearby merchant search radius in kilometers (5.0)
  static const double nearbyMerchantRadiusKm = 5.0;

  /// Maximum retry count for operations (3)
  static const int maxRetryCount = 3;

  /// Default page size for pagination (20)
  static const int defaultPageSize = 20;

  /// Debounce duration for search in milliseconds (300)
  static const int searchDebounceDurationMs = 300;

  // ==================== ANIMATION ====================

  /// Short animation duration in milliseconds (200)
  static const int animationDurationShortMs = 200;

  /// Medium animation duration in milliseconds (300)
  static const int animationDurationMediumMs = 300;

  /// Long animation duration in milliseconds (500)
  static const int animationDurationLongMs = 500;

  // ==================== FORM ====================

  /// Minimum password length (8)
  static const int minPasswordLength = 8;

  /// Maximum text field length (255)
  static const int maxTextFieldLength = 255;

  /// OTP code length (6)
  static const int otpCodeLength = 6;

  // ==================== IMAGE ====================

  /// Product image aspect ratio (1.0)
  static const double productImageAspectRatio = 1.0;

  /// Merchant banner aspect ratio (16:9 = 1.78)
  static const double merchantBannerAspectRatio = 1.78;

  /// Avatar size (40.0)
  static const double avatarSize = 40.0;

  /// Large avatar size (80.0)
  static const double avatarSizeLarge = 80.0;
}
