import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Provider for managing font size scale
class FontScaleProvider extends ChangeNotifier {
  static const String _fontScaleKey = 'font_scale';
  static const double _minScale = 0.8;
  static const double _maxScale = 1.5;
  static const double _defaultScale = 1.0;

  double _fontScale = _defaultScale;
  late SharedPreferences _prefs;

  FontScaleProvider() {
    _loadFontScale();
  }

  double get fontScale => _fontScale;

  String get fontScaleLabel {
    if (_fontScale <= 0.9) return 'Küçük';
    if (_fontScale <= 1.1) return 'Normal';
    if (_fontScale <= 1.3) return 'Büyük';
    return 'Çok Büyük';
  }

  Future<void> _loadFontScale() async {
    try {
      _prefs = await SharedPreferences.getInstance();
      _fontScale = _prefs.getDouble(_fontScaleKey) ?? _defaultScale;
      notifyListeners();
    } catch (e) {
      debugPrint('Error loading font scale: $e');
    }
  }

  Future<void> setFontScale(double scale) async {
    final clampedScale = scale.clamp(_minScale, _maxScale);
    if (_fontScale == clampedScale) return;

    _fontScale = clampedScale;
    await _prefs.setDouble(_fontScaleKey, clampedScale);
    notifyListeners();
  }

  Future<void> increase() async {
    await setFontScale(_fontScale + 0.1);
  }

  Future<void> decrease() async {
    await setFontScale(_fontScale - 0.1);
  }

  Future<void> reset() async {
    await setFontScale(_defaultScale);
  }

  /// Get scaled font size
  double scaledSize(double baseSize) {
    return baseSize * _fontScale;
  }

  /// Get text style with scaled font
  TextStyle scaleTextStyle(TextStyle baseStyle) {
    return baseStyle.copyWith(
      fontSize: baseStyle.fontSize != null
          ? baseStyle.fontSize! * _fontScale
          : null,
    );
  }
}

/// Widget to apply font scale to descendants
class FontScaleWrapper extends StatelessWidget {
  final Widget child;
  final double? scale;

  const FontScaleWrapper({super.key, required this.child, this.scale});

  @override
  Widget build(BuildContext context) {
    final fontScale = scale ?? 1.0;

    return MediaQuery(
      data: MediaQuery.of(
        context,
      ).copyWith(textScaler: TextScaler.linear(fontScale)),
      child: child,
    );
  }
}
