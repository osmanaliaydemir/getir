import 'package:flutter/material.dart';
import '../services/dynamic_content_service.dart';
import '../di/injection.dart';
import 'app_localizations.dart';

/// Enhanced Localizations
///
/// Extends AppLocalizations with dynamic content support
/// Features:
/// - Backend-driven translations
/// - Fallback to static translations
/// - Parameter replacement
/// - RTL support
/// - Caching
class EnhancedLocalizations {
  final BuildContext context;
  final String languageCode;

  late final AppLocalizations _staticL10n;
  late final DynamicContentService _dynamicContent;

  EnhancedLocalizations(this.context, this.languageCode) {
    _staticL10n = AppLocalizations.of(context);
    _dynamicContent = getIt<DynamicContentService>();
  }

  /// Get string with fallback to static translation
  String getString(String key, {Map<String, String>? params}) {
    // Try dynamic content first
    final dynamicValue = _dynamicContent.getString(
      key: key,
      languageCode: languageCode,
      fallbackLanguage: _getFallbackLanguage(),
      params: params,
    );

    // If dynamic content returns the key itself (meaning not found),
    // fall back to static translation
    if (dynamicValue == key) {
      return _getStaticString(key) ?? key;
    }

    return dynamicValue;
  }

  /// Get static translation
  String? _getStaticString(String key) {
    // Map common keys to AppLocalizations getters
    try {
      switch (key) {
        case 'welcome':
          return _staticL10n.welcome;
        case 'login':
          return _staticL10n.login;
        case 'register':
          return _staticL10n.register;
        case 'email':
          return _staticL10n.email;
        case 'password':
          return _staticL10n.password;
        case 'home':
          return _staticL10n.home;
        case 'search':
          return _staticL10n.search;
        case 'orders':
          return _staticL10n.orders;
        case 'profile':
          return _staticL10n.profile;
        case 'addToCart':
          return _staticL10n.addToCart;
        case 'loading':
          return _staticL10n.loading;
        case 'error':
          return _staticL10n.error;
        case 'success':
          return _staticL10n.success;
        case 'cancel':
          return _staticL10n.cancel;
        case 'save':
          return _staticL10n.save;
        case 'retry':
          return _staticL10n.retry;
        // Add more mappings as needed
        default:
          return null;
      }
    } catch (e) {
      return null;
    }
  }

  /// Get fallback language
  String _getFallbackLanguage() {
    // TR → EN → AR
    // EN → TR → AR
    // AR → EN → TR
    switch (languageCode) {
      case 'tr':
        return 'en';
      case 'en':
        return 'tr';
      case 'ar':
        return 'en';
      default:
        return 'en';
    }
  }

  /// Check if RTL language
  bool get isRTL => languageCode == 'ar';

  /// Get text direction
  TextDirection get textDirection =>
      isRTL ? TextDirection.rtl : TextDirection.ltr;

  /// Get alignment
  Alignment getAlignment({
    Alignment ltr = Alignment.centerLeft,
    Alignment rtl = Alignment.centerRight,
  }) {
    return isRTL ? rtl : ltr;
  }

  /// Get text align
  TextAlign getTextAlign({
    TextAlign ltr = TextAlign.left,
    TextAlign rtl = TextAlign.right,
  }) {
    return isRTL ? rtl : ltr;
  }
}

/// Extension for easy access
extension EnhancedLocalizationsExtension on BuildContext {
  EnhancedLocalizations get enhancedL10n {
    final locale = Localizations.localeOf(this);
    return EnhancedLocalizations(this, locale.languageCode);
  }
}

/// Localized Text Widget
///
/// Widget that displays text with dynamic content support
class LocalizedText extends StatelessWidget {
  final String translationKey;
  final Map<String, String>? params;
  final TextStyle? style;
  final TextAlign? textAlign;
  final int? maxLines;
  final TextOverflow? overflow;

  const LocalizedText(
    this.translationKey, {
    super.key,
    this.params,
    this.style,
    this.textAlign,
    this.maxLines,
    this.overflow,
  });

  @override
  Widget build(BuildContext context) {
    final enhanced = context.enhancedL10n;
    final text = enhanced.getString(translationKey, params: params);

    return Text(
      text,
      style: style,
      textAlign: textAlign ?? enhanced.getTextAlign(),
      maxLines: maxLines,
      overflow: overflow,
    );
  }
}

/// RTL Aware Widget
///
/// Automatically adjusts layout for RTL languages
class RTLAwareWidget extends StatelessWidget {
  final Widget child;
  final bool forceRTL;

  const RTLAwareWidget({super.key, required this.child, this.forceRTL = false});

  @override
  Widget build(BuildContext context) {
    final locale = Localizations.localeOf(context);
    final isRTL = forceRTL || locale.languageCode == 'ar';

    return Directionality(
      textDirection: isRTL ? TextDirection.rtl : TextDirection.ltr,
      child: child,
    );
  }
}

/// Language-Specific Image
///
/// Widget that loads language-specific images
class LanguageSpecificImage extends StatelessWidget {
  final String baseAssetPath;
  final double? width;
  final double? height;
  final BoxFit? fit;

  const LanguageSpecificImage({
    super.key,
    required this.baseAssetPath,
    this.width,
    this.height,
    this.fit,
  });

  @override
  Widget build(BuildContext context) {
    final locale = Localizations.localeOf(context);
    final assetService = getIt<LanguageAssetService>();

    final assetPath = assetService.getAssetPath(
      baseAsset: baseAssetPath,
      languageCode: locale.languageCode,
    );

    return Image.asset(
      assetPath,
      width: width,
      height: height,
      fit: fit,
      // Fallback to base asset if language-specific not found
      errorBuilder: (context, error, stackTrace) {
        return Image.asset(
          baseAssetPath,
          width: width,
          height: height,
          fit: fit,
        );
      },
    );
  }
}
