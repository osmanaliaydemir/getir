import 'package:flutter/material.dart';
import 'package:flutter/semantics.dart';

/// Accessibility service for screen reader support and high contrast
class AccessibilityService {
  static final AccessibilityService _instance =
      AccessibilityService._internal();
  factory AccessibilityService() => _instance;
  AccessibilityService._internal();

  /// Check if screen reader is enabled
  static bool get isScreenReaderEnabled {
    return WidgetsBinding.instance.accessibilityFeatures.accessibleNavigation;
  }

  /// Check if high contrast is enabled
  static bool get isHighContrastEnabled {
    return WidgetsBinding.instance.accessibilityFeatures.highContrast;
  }

  /// Check if bold text is enabled
  static bool get isBoldTextEnabled {
    return WidgetsBinding.instance.accessibilityFeatures.boldText;
  }

  /// Check if reduce motion is enabled
  static bool get isReduceMotionEnabled {
    return WidgetsBinding.instance.accessibilityFeatures.reduceMotion;
  }

  /// Get accessibility features
  static AccessibilityFeatures get accessibilityFeatures {
    return WidgetsBinding.instance.accessibilityFeatures;
  }

  /// Announce message to screen reader
  static void announce(String message, {bool assertiveness = false}) {
    if (isScreenReaderEnabled) {
      SemanticsService.announce(
        message,
        assertiveness ? TextDirection.ltr : TextDirection.ltr,
      );
    }
  }

  /// Announce error message
  static void announceError(String message) {
    announce('Error: $message', assertiveness: true);
  }

  /// Announce success message
  static void announceSuccess(String message) {
    announce('Success: $message');
  }

  /// Announce navigation
  static void announceNavigation(String pageName) {
    announce('Navigated to $pageName');
  }

  /// Announce button action
  static void announceButtonAction(String action) {
    announce('$action button pressed');
  }

  /// Announce form field
  static void announceFormField(String fieldName, String value) {
    announce('$fieldName: $value');
  }

  /// Announce loading state
  static void announceLoading(String message) {
    announce('Loading: $message');
  }

  /// Announce completion
  static void announceCompletion(String message) {
    announce('Completed: $message');
  }
}

/// Accessibility mixin for widgets
mixin AccessibilityMixin {
  /// Get accessibility label
  String? get accessibilityLabel;

  /// Get accessibility hint
  String? get accessibilityHint;

  /// Get accessibility value
  String? get accessibilityValue;

  /// Get accessibility role
  SemanticsRole? get accessibilityRole;

  /// Get accessibility actions
  Map<CustomSemanticsAction, VoidCallback>? get accessibilityActions;

  /// Get accessibility flags
  Set<SemanticsFlag>? get accessibilityFlags;

  /// Check if widget is accessible
  bool get isAccessible => accessibilityLabel != null;

  /// Announce widget state
  void announceState(String state) {
    if (AccessibilityService.isScreenReaderEnabled) {
      AccessibilityService.announce('$accessibilityLabel: $state');
    }
  }

  /// Announce widget action
  void announceAction(String action) {
    if (AccessibilityService.isScreenReaderEnabled) {
      AccessibilityService.announce('$accessibilityLabel: $action');
    }
  }
}

/// Accessible button widget
class AccessibleButton extends StatelessWidget {
  final Widget child;
  final VoidCallback? onPressed;
  final String? label;
  final String? hint;
  final SemanticsRole? role;
  final Map<CustomSemanticsAction, VoidCallback>? actions;
  final Set<SemanticsFlag>? flags;

  const AccessibleButton({
    super.key,
    required this.child,
    required this.onPressed,
    this.label,
    this.hint,
    this.role,
    this.actions,
    this.flags,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      role: role,
      customSemanticsActions: actions,
      button: true,
      enabled: onPressed != null,
      child: GestureDetector(onTap: onPressed, child: child),
    );
  }
}

/// Accessible text field
class AccessibleTextField extends StatelessWidget {
  final TextEditingController? controller;
  final String? label;
  final String? hint;
  final String? helperText;
  final String? errorText;
  final bool obscureText;
  final TextInputType? keyboardType;
  final ValueChanged<String>? onChanged;
  final ValueChanged<String>? onSubmitted;
  final VoidCallback? onTap;

  const AccessibleTextField({
    super.key,
    this.controller,
    this.label,
    this.hint,
    this.helperText,
    this.errorText,
    this.obscureText = false,
    this.keyboardType,
    this.onChanged,
    this.onSubmitted,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      textField: true,
      maxValueLength: controller?.text.length,
      child: TextField(
        controller: controller,
        obscureText: obscureText,
        keyboardType: keyboardType,
        onChanged: onChanged,
        onSubmitted: onSubmitted,
        onTap: onTap,
        decoration: InputDecoration(
          labelText: label,
          hintText: hint,
          helperText: helperText,
          errorText: errorText,
        ),
      ),
    );
  }
}

/// Accessible image
class AccessibleImage extends StatelessWidget {
  final ImageProvider image;
  final String? label;
  final String? hint;
  final double? width;
  final double? height;
  final BoxFit? fit;

  const AccessibleImage({
    super.key,
    required this.image,
    this.label,
    this.hint,
    this.width,
    this.height,
    this.fit,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      image: true,
      child: Image(image: image, width: width, height: height, fit: fit),
    );
  }
}

/// Accessible card
class AccessibleCard extends StatelessWidget {
  final Widget child;
  final String? label;
  final String? hint;
  final VoidCallback? onTap;
  final SemanticsRole? role;

  const AccessibleCard({
    super.key,
    required this.child,
    this.label,
    this.hint,
    this.onTap,
    this.role,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      role: role,
      button: onTap != null,
      child: Card(
        child: InkWell(onTap: onTap, child: child),
      ),
    );
  }
}

/// Accessible list tile
class AccessibleListTile extends StatelessWidget {
  final Widget? leading;
  final Widget? title;
  final Widget? subtitle;
  final Widget? trailing;
  final String? label;
  final String? hint;
  final VoidCallback? onTap;
  final VoidCallback? onLongPress;

  const AccessibleListTile({
    super.key,
    this.leading,
    this.title,
    this.subtitle,
    this.trailing,
    this.label,
    this.hint,
    this.onTap,
    this.onLongPress,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      role: SemanticsRole.list,
      button: onTap != null,
      child: ListTile(
        leading: leading,
        title: title,
        subtitle: subtitle,
        trailing: trailing,
        onTap: onTap,
        onLongPress: onLongPress,
      ),
    );
  }
}

/// Accessible switch
class AccessibleSwitch extends StatelessWidget {
  final bool value;
  final ValueChanged<bool>? onChanged;
  final String? label;
  final String? hint;

  const AccessibleSwitch({
    super.key,
    required this.value,
    required this.onChanged,
    this.label,
    this.hint,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      toggled: value,
      child: Switch(value: value, onChanged: onChanged),
    );
  }
}

/// Accessible checkbox
class AccessibleCheckbox extends StatelessWidget {
  final bool value;
  final ValueChanged<bool?>? onChanged;
  final String? label;
  final String? hint;

  const AccessibleCheckbox({
    super.key,
    required this.value,
    required this.onChanged,
    this.label,
    this.hint,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      checked: value,
      child: Checkbox(value: value, onChanged: onChanged),
    );
  }
}

/// Accessible slider
class AccessibleSlider extends StatelessWidget {
  final double value;
  final ValueChanged<double>? onChanged;
  final double min;
  final double max;
  final int? divisions;
  final String? label;
  final String? hint;

  const AccessibleSlider({
    super.key,
    required this.value,
    required this.onChanged,
    required this.min,
    required this.max,
    this.divisions,
    this.label,
    this.hint,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      hint: hint,
      value: value.toString(),
      child: Slider(
        value: value,
        onChanged: onChanged,
        min: min,
        max: max,
        divisions: divisions,
      ),
    );
  }
}

/// High contrast theme extension
extension HighContrastTheme on ThemeData {
  /// Get high contrast color scheme
  ColorScheme get highContrastColorScheme {
    if (AccessibilityService.isHighContrastEnabled) {
      return colorScheme.copyWith(
        primary: Colors.white,
        onPrimary: Colors.black,
        secondary: Colors.yellow,
        onSecondary: Colors.black,
        error: Colors.red,
        onError: Colors.white,
        surface: Colors.black,
        onSurface: Colors.white,
      );
    }
    return colorScheme;
  }
}

/// Accessibility utilities
class AccessibilityUtils {
  /// Get appropriate text style for accessibility
  static TextStyle getAccessibleTextStyle(
    BuildContext context,
    TextStyle baseStyle,
  ) {
    final theme = Theme.of(context);
    final isHighContrast = AccessibilityService.isHighContrastEnabled;
    final isBoldText = AccessibilityService.isBoldTextEnabled;

    TextStyle accessibleStyle = baseStyle;

    if (isHighContrast) {
      accessibleStyle = accessibleStyle.copyWith(
        color: theme.colorScheme.onSurface,
        fontWeight: FontWeight.bold,
      );
    }

    if (isBoldText) {
      accessibleStyle = accessibleStyle.copyWith(fontWeight: FontWeight.bold);
    }

    return accessibleStyle;
  }

  /// Get appropriate color for accessibility
  static Color getAccessibleColor(BuildContext context, Color baseColor) {
    final isHighContrast = AccessibilityService.isHighContrastEnabled;

    if (isHighContrast) {
      // Use high contrast colors
      if (baseColor == Theme.of(context).colorScheme.primary) {
        return Colors.white;
      } else if (baseColor == Theme.of(context).colorScheme.secondary) {
        return Colors.yellow;
      } else if (baseColor == Theme.of(context).colorScheme.error) {
        return Colors.red;
      }
    }

    return baseColor;
  }

  /// Get appropriate icon size for accessibility
  static double getAccessibleIconSize(double baseSize) {
    final isHighContrast = AccessibilityService.isHighContrastEnabled;

    if (isHighContrast) {
      return baseSize * 1.2; // 20% larger for high contrast
    }

    return baseSize;
  }

  /// Get appropriate padding for accessibility
  static EdgeInsets getAccessiblePadding(EdgeInsets basePadding) {
    final isHighContrast = AccessibilityService.isHighContrastEnabled;

    if (isHighContrast) {
      return EdgeInsets.all(
        basePadding.left * 1.2, // 20% larger padding
      );
    }

    return basePadding;
  }
}
