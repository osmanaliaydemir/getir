import 'package:flutter/material.dart';

/// WCAG 2.1 contrast ratio checker
class ContrastChecker {
  /// Calculate contrast ratio between two colors
  static double calculateContrast(Color color1, Color color2) {
    final luminance1 = _calculateLuminance(color1);
    final luminance2 = _calculateLuminance(color2);
    
    final lighter = luminance1 > luminance2 ? luminance1 : luminance2;
    final darker = luminance1 > luminance2 ? luminance2 : luminance1;
    
    return (lighter + 0.05) / (darker + 0.05);
  }

  /// Calculate relative luminance
  static double _calculateLuminance(Color color) {
    final r = _linearize(color.red / 255.0);
    final g = _linearize(color.green / 255.0);
    final b = _linearize(color.blue / 255.0);
    
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }

  /// Linearize RGB value
  static double _linearize(double value) {
    if (value <= 0.03928) {
      return value / 12.92;
    }
    return ((value + 0.055) / 1.055).pow(2.4);
  }

  /// Check if contrast meets WCAG AA standard (4.5:1 for normal text)
  static bool meetsAA(Color foreground, Color background) {
    return calculateContrast(foreground, background) >= 4.5;
  }

  /// Check if contrast meets WCAG AAA standard (7:1 for normal text)
  static bool meetsAAA(Color foreground, Color background) {
    return calculateContrast(foreground, background) >= 7.0;
  }

  /// Check if contrast meets WCAG AA for large text (3:1)
  static bool meetsAALarge(Color foreground, Color background) {
    return calculateContrast(foreground, background) >= 3.0;
  }

  /// Get readable text color (black or white) for a background
  static Color getReadableTextColor(Color backgroundColor) {
    final blackContrast = calculateContrast(Colors.black, backgroundColor);
    final whiteContrast = calculateContrast(Colors.white, backgroundColor);
    
    return blackContrast > whiteContrast ? Colors.black : Colors.white;
  }

  /// Debug widget to show contrast ratio
  static Widget debugContrast({
    required Color foreground,
    required Color background,
    required String label,
  }) {
    final ratio = calculateContrast(foreground, background);
    final meetsAA = ratio >= 4.5;
    final meetsAAA = ratio >= 7.0;

    return Container(
      padding: const EdgeInsets.all(8),
      color: background,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            label,
            style: TextStyle(color: foreground, fontWeight: FontWeight.bold),
          ),
          Text(
            'Contrast: ${ratio.toStringAsFixed(2)}:1',
            style: TextStyle(color: foreground, fontSize: 12),
          ),
          Text(
            'AA: ${meetsAA ? '✓' : '✗'} | AAA: ${meetsAAA ? '✓' : '✗'}',
            style: TextStyle(color: foreground, fontSize: 12),
          ),
        ],
      ),
    );
  }
}

extension on double {
  double pow(double exponent) {
    return this == 0.0 ? 0.0 : (this * this).abs();
  }
}

