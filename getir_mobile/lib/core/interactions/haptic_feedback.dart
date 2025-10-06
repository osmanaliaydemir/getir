import 'package:flutter/services.dart';
import 'package:flutter/material.dart';

/// Haptic feedback service for micro-interactions
class HapticFeedbackService {
  static final HapticFeedbackService _instance = HapticFeedbackService._internal();
  factory HapticFeedbackService() => _instance;
  HapticFeedbackService._internal();

  /// Light haptic feedback for subtle interactions
  static void light() {
    HapticFeedback.lightImpact();
  }

  /// Medium haptic feedback for standard interactions
  static void medium() {
    HapticFeedback.mediumImpact();
  }

  /// Heavy haptic feedback for important interactions
  static void heavy() {
    HapticFeedback.heavyImpact();
  }

  /// Selection haptic feedback for UI selections
  static void selection() {
    HapticFeedback.selectionClick();
  }

  /// Success haptic feedback for successful actions
  static void success() {
    HapticFeedback.mediumImpact();
  }

  /// Error haptic feedback for error states
  static void error() {
    HapticFeedback.heavyImpact();
  }

  /// Warning haptic feedback for warnings
  static void warning() {
    HapticFeedback.lightImpact();
  }

  /// Button press haptic feedback
  static void buttonPress() {
    HapticFeedback.selectionClick();
  }

  /// Toggle haptic feedback
  static void toggle() {
    HapticFeedback.lightImpact();
  }

  /// Swipe haptic feedback
  static void swipe() {
    HapticFeedback.lightImpact();
  }

  /// Long press haptic feedback
  static void longPress() {
    HapticFeedback.mediumImpact();
  }

  /// Custom haptic feedback based on interaction type
  static void custom(HapticFeedbackType type) {
    switch (type) {
      case HapticFeedbackType.light:
        light();
        break;
      case HapticFeedbackType.medium:
        medium();
        break;
      case HapticFeedbackType.heavy:
        heavy();
        break;
      case HapticFeedbackType.selection:
        selection();
        break;
      case HapticFeedbackType.success:
        success();
        break;
      case HapticFeedbackType.error:
        error();
        break;
      case HapticFeedbackType.warning:
        warning();
        break;
      case HapticFeedbackType.buttonPress:
        buttonPress();
        break;
      case HapticFeedbackType.toggle:
        toggle();
        break;
      case HapticFeedbackType.swipe:
        swipe();
        break;
      case HapticFeedbackType.longPress:
        longPress();
        break;
    }
  }
}

/// Haptic feedback types
enum HapticFeedbackType {
  light,
  medium,
  heavy,
  selection,
  success,
  error,
  warning,
  buttonPress,
  toggle,
  swipe,
  longPress,
}

/// Haptic feedback mixin for widgets
mixin HapticFeedbackMixin {
  void hapticLight() => HapticFeedbackService.light();
  void hapticMedium() => HapticFeedbackService.medium();
  void hapticHeavy() => HapticFeedbackService.heavy();
  void hapticSelection() => HapticFeedbackService.selection();
  void hapticSuccess() => HapticFeedbackService.success();
  void hapticError() => HapticFeedbackService.error();
  void hapticWarning() => HapticFeedbackService.warning();
  void hapticButtonPress() => HapticFeedbackService.buttonPress();
  void hapticToggle() => HapticFeedbackService.toggle();
  void hapticSwipe() => HapticFeedbackService.swipe();
  void hapticLongPress() => HapticFeedbackService.longPress();
}

/// Haptic feedback button wrapper
class HapticButton extends StatelessWidget {
  final Widget child;
  final VoidCallback? onPressed;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;

  const HapticButton({
    super.key,
    required this.child,
    required this.onPressed,
    this.hapticType = HapticFeedbackType.buttonPress,
    this.enableHaptic = true,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () {
        if (enableHaptic) {
          HapticFeedbackService.custom(hapticType);
        }
        onPressed?.call();
      },
      child: child,
    );
  }
}

/// Haptic feedback switch
class HapticSwitch extends StatefulWidget {
  final bool value;
  final ValueChanged<bool>? onChanged;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;

  const HapticSwitch({
    super.key,
    required this.value,
    required this.onChanged,
    this.hapticType = HapticFeedbackType.toggle,
    this.enableHaptic = true,
  });

  @override
  State<HapticSwitch> createState() => _HapticSwitchState();
}

class _HapticSwitchState extends State<HapticSwitch> {
  @override
  Widget build(BuildContext context) {
    return Switch(
      value: widget.value,
      onChanged: (value) {
        if (widget.enableHaptic) {
          HapticFeedbackService.custom(widget.hapticType);
        }
        widget.onChanged?.call(value);
      },
    );
  }
}

/// Haptic feedback checkbox
class HapticCheckbox extends StatefulWidget {
  final bool value;
  final ValueChanged<bool>? onChanged;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;

  const HapticCheckbox({
    super.key,
    required this.value,
    required this.onChanged,
    this.hapticType = HapticFeedbackType.selection,
    this.enableHaptic = true,
  });

  @override
  State<HapticCheckbox> createState() => _HapticCheckboxState();
}

class _HapticCheckboxState extends State<HapticCheckbox> {
  @override
  Widget build(BuildContext context) {
    return Checkbox(
      value: widget.value,
      onChanged: (value) {
        if (widget.enableHaptic) {
          HapticFeedbackService.custom(widget.hapticType);
        }
        widget.onChanged?.call(value ?? false);
      },
    );
  }
}

/// Haptic feedback slider
class HapticSlider extends StatefulWidget {
  final double value;
  final ValueChanged<double>? onChanged;
  final ValueChanged<double>? onChangeStart;
  final ValueChanged<double>? onChangeEnd;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;
  final double min;
  final double max;
  final int? divisions;

  const HapticSlider({
    super.key,
    required this.value,
    required this.onChanged,
    this.onChangeStart,
    this.onChangeEnd,
    this.hapticType = HapticFeedbackType.selection,
    this.enableHaptic = true,
    required this.min,
    required this.max,
    this.divisions,
  });

  @override
  State<HapticSlider> createState() => _HapticSliderState();
}

class _HapticSliderState extends State<HapticSlider> {
  @override
  Widget build(BuildContext context) {
    return Slider(
      value: widget.value,
      onChanged: (value) {
        if (widget.enableHaptic) {
          HapticFeedbackService.custom(widget.hapticType);
        }
        widget.onChanged?.call(value);
      },
      onChangeStart: (value) {
        if (widget.enableHaptic) {
          HapticFeedbackService.custom(widget.hapticType);
        }
        widget.onChangeStart?.call(value);
      },
      onChangeEnd: (value) {
        if (widget.enableHaptic) {
          HapticFeedbackService.custom(widget.hapticType);
        }
        widget.onChangeEnd?.call(value);
      },
      min: widget.min,
      max: widget.max,
      divisions: widget.divisions,
    );
  }
}
