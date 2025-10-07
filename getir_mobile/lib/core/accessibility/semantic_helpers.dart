import 'package:flutter/material.dart';

/// Helper for creating semantic widgets
class SemanticHelpers {
  /// Create a semantic button wrapper
  static Widget button({
    required Widget child,
    required VoidCallback onTap,
    required String label,
    String? hint,
    bool isEnabled = true,
  }) {
    return Semantics(
      button: true,
      enabled: isEnabled,
      label: label,
      hint: hint,
      onTap: isEnabled ? onTap : null,
      child: child,
    );
  }

  /// Create a semantic image wrapper
  static Widget image({
    required Widget child,
    required String label,
    String? hint,
  }) {
    return Semantics(image: true, label: label, hint: hint, child: child);
  }

  /// Create a semantic header wrapper
  static Widget header({
    required Widget child,
    required String label,
    bool isLarge = false,
  }) {
    return Semantics(header: true, label: label, child: child);
  }

  /// Create a semantic link wrapper
  static Widget link({
    required Widget child,
    required String label,
    required VoidCallback onTap,
  }) {
    return Semantics(link: true, label: label, onTap: onTap, child: child);
  }

  /// Create a semantic text field wrapper
  static Widget textField({
    required Widget child,
    required String label,
    String? hint,
    String? value,
    bool isObscured = false,
    bool isMultiline = false,
  }) {
    return Semantics(
      textField: true,
      label: label,
      hint: hint,
      value: value,
      obscured: isObscured,
      multiline: isMultiline,
      child: child,
    );
  }

  /// Create a semantic slider wrapper
  static Widget slider({
    required Widget child,
    required String label,
    required double value,
    String? hint,
  }) {
    return Semantics(
      slider: true,
      label: label,
      value: value.toString(),
      hint: hint,
      child: child,
    );
  }

  /// Create a semantic switch wrapper
  static Widget toggle({
    required Widget child,
    required String label,
    required bool isChecked,
    String? hint,
  }) {
    return Semantics(
      toggled: isChecked,
      label: label,
      hint: hint,
      child: child,
    );
  }

  /// Create a live region for dynamic content
  static Widget liveRegion({
    required Widget child,
    String? label,
    bool assertive = false,
  }) {
    return Semantics(liveRegion: true, label: label, child: child);
  }
}

/// Accessible button wrapper with all a11y features
class AccessibleButton extends StatelessWidget {
  final Widget child;
  final VoidCallback? onPressed;
  final String label;
  final String? hint;
  final String? tooltip;

  const AccessibleButton({
    super.key,
    required this.child,
    required this.onPressed,
    required this.label,
    this.hint,
    this.tooltip,
  });

  @override
  Widget build(BuildContext context) {
    Widget button = SemanticHelpers.button(
      onTap: onPressed ?? () {},
      label: label,
      hint: hint,
      isEnabled: onPressed != null,
      child: child,
    );

    if (tooltip != null) {
      button = Tooltip(message: tooltip!, child: button);
    }

    return button;
  }
}

/// Accessible icon button with semantic label
class AccessibleIconButton extends StatelessWidget {
  final IconData icon;
  final VoidCallback? onPressed;
  final String label;
  final String? tooltip;
  final Color? color;
  final double? size;

  const AccessibleIconButton({
    super.key,
    required this.icon,
    required this.onPressed,
    required this.label,
    this.tooltip,
    this.color,
    this.size,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      button: true,
      enabled: onPressed != null,
      label: label,
      hint: tooltip,
      onTap: onPressed,
      child: IconButton(
        icon: Icon(icon),
        onPressed: onPressed,
        tooltip: tooltip ?? label,
        color: color,
        iconSize: size,
      ),
    );
  }
}

/// Accessible card with semantic grouping
class AccessibleCard extends StatelessWidget {
  final Widget child;
  final String label;
  final String? hint;
  final VoidCallback? onTap;

  const AccessibleCard({
    super.key,
    required this.child,
    required this.label,
    this.hint,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      container: true,
      label: label,
      hint: hint,
      button: onTap != null,
      onTap: onTap,
      child: child,
    );
  }
}
