import 'package:flutter/material.dart';
import 'haptic_feedback.dart';

/// Touch feedback service for visual and haptic feedback
class TouchFeedbackService {
  static final TouchFeedbackService _instance =
      TouchFeedbackService._internal();
  factory TouchFeedbackService() => _instance;
  TouchFeedbackService._internal();

  /// Show ripple effect with haptic feedback
  static void ripple(
    BuildContext context, {
    HapticFeedbackType hapticType = HapticFeedbackType.light,
    Color? rippleColor,
    Duration duration = const Duration(milliseconds: 200),
  }) {
    HapticFeedbackService.custom(hapticType);

    // Show ripple effect
    final overlay = Overlay.of(context);
    final overlayEntry = OverlayEntry(
      builder: (context) => Positioned.fill(
        child: Material(
          color: Colors.transparent,
          child: Center(
            child: Container(
              width: 100,
              height: 100,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: (rippleColor ?? Theme.of(context).primaryColor)
                    .withOpacity(0.3),
              ),
            ),
          ),
        ),
      ),
    );

    overlay.insert(overlayEntry);

    Future.delayed(duration, () {
      overlayEntry.remove();
    });
  }

  /// Show scale feedback
  static void scale(
    BuildContext context, {
    HapticFeedbackType hapticType = HapticFeedbackType.medium,
    double scale = 0.95,
    Duration duration = const Duration(milliseconds: 150),
  }) {
    HapticFeedbackService.custom(hapticType);

    // This would be implemented with a custom widget
    // For now, we'll just trigger haptic feedback
  }

  /// Show bounce feedback
  static void bounce(
    BuildContext context, {
    HapticFeedbackType hapticType = HapticFeedbackType.medium,
    double bounceScale = 1.1,
    Duration duration = const Duration(milliseconds: 200),
  }) {
    HapticFeedbackService.custom(hapticType);

    // This would be implemented with a custom widget
    // For now, we'll just trigger haptic feedback
  }

  /// Show glow feedback
  static void glow(
    BuildContext context, {
    HapticFeedbackType hapticType = HapticFeedbackType.light,
    Color? glowColor,
    Duration duration = const Duration(milliseconds: 300),
  }) {
    HapticFeedbackService.custom(hapticType);

    // This would be implemented with a custom widget
    // For now, we'll just trigger haptic feedback
  }
}

/// Touch feedback mixin for widgets
mixin TouchFeedbackMixin {
  void touchRipple(BuildContext context, {HapticFeedbackType? hapticType}) {
    TouchFeedbackService.ripple(
      context,
      hapticType: hapticType ?? HapticFeedbackType.light,
    );
  }

  void touchScale(BuildContext context, {HapticFeedbackType? hapticType}) {
    TouchFeedbackService.scale(
      context,
      hapticType: hapticType ?? HapticFeedbackType.medium,
    );
  }

  void touchBounce(BuildContext context, {HapticFeedbackType? hapticType}) {
    TouchFeedbackService.bounce(
      context,
      hapticType: hapticType ?? HapticFeedbackType.medium,
    );
  }

  void touchGlow(BuildContext context, {HapticFeedbackType? hapticType}) {
    TouchFeedbackService.glow(
      context,
      hapticType: hapticType ?? HapticFeedbackType.light,
    );
  }
}

/// Touch feedback button with visual and haptic feedback
class TouchFeedbackButton extends StatefulWidget {
  final Widget child;
  final VoidCallback? onPressed;
  final TouchFeedbackType feedbackType;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;
  final bool enableVisual;
  final Color? rippleColor;
  final double scale;
  final Duration duration;

  const TouchFeedbackButton({
    super.key,
    required this.child,
    required this.onPressed,
    this.feedbackType = TouchFeedbackType.ripple,
    this.hapticType = HapticFeedbackType.buttonPress,
    this.enableHaptic = true,
    this.enableVisual = true,
    this.rippleColor,
    this.scale = 0.95,
    this.duration = const Duration(milliseconds: 150),
  });

  @override
  State<TouchFeedbackButton> createState() => _TouchFeedbackButtonState();
}

class _TouchFeedbackButtonState extends State<TouchFeedbackButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;
  // ignore: unused_field
  bool _isPressed = false; // Reserved for future haptic feedback feature

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _scaleAnimation = Tween<double>(
      begin: 1.0,
      end: widget.scale,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTapDown(TapDownDetails details) {
    if (widget.enableVisual) {
      setState(() {
        _isPressed = true;
      });
      _controller.forward();
    }

    if (widget.enableHaptic) {
      HapticFeedbackService.custom(widget.hapticType);
    }
  }

  void _handleTapUp(TapUpDetails details) {
    if (widget.enableVisual) {
      setState(() {
        _isPressed = false;
      });
      _controller.reverse();
    }
  }

  void _handleTapCancel() {
    if (widget.enableVisual) {
      setState(() {
        _isPressed = false;
      });
      _controller.reverse();
    }
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTapDown: _handleTapDown,
      onTapUp: _handleTapUp,
      onTapCancel: _handleTapCancel,
      onTap: widget.onPressed,
      child: AnimatedBuilder(
        animation: _scaleAnimation,
        builder: (context, child) {
          return Transform.scale(
            scale: _scaleAnimation.value,
            child: widget.child,
          );
        },
      ),
    );
  }
}

/// Touch feedback types
enum TouchFeedbackType { ripple, scale, bounce, glow }

/// Touch feedback container with ripple effect
class TouchFeedbackContainer extends StatefulWidget {
  final Widget child;
  final VoidCallback? onTap;
  final Color? rippleColor;
  final double borderRadius;
  final EdgeInsets? padding;
  final EdgeInsets? margin;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;

  const TouchFeedbackContainer({
    super.key,
    required this.child,
    this.onTap,
    this.rippleColor,
    this.borderRadius = 8.0,
    this.padding,
    this.margin,
    this.hapticType = HapticFeedbackType.light,
    this.enableHaptic = true,
  });

  @override
  State<TouchFeedbackContainer> createState() => _TouchFeedbackContainerState();
}

class _TouchFeedbackContainerState extends State<TouchFeedbackContainer> {
  @override
  Widget build(BuildContext context) {
    return Container(
      margin: widget.margin,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () {
            if (widget.enableHaptic) {
              HapticFeedbackService.custom(widget.hapticType);
            }
            widget.onTap?.call();
          },
          borderRadius: BorderRadius.circular(widget.borderRadius),
          splashColor:
              widget.rippleColor ??
              Theme.of(context).primaryColor.withOpacity(0.1),
          highlightColor:
              widget.rippleColor?.withOpacity(0.05) ??
              Theme.of(context).primaryColor.withOpacity(0.05),
          child: Container(padding: widget.padding, child: widget.child),
        ),
      ),
    );
  }
}

/// Touch feedback list tile
class TouchFeedbackListTile extends StatelessWidget {
  final Widget? leading;
  final Widget? title;
  final Widget? subtitle;
  final Widget? trailing;
  final VoidCallback? onTap;
  final Color? rippleColor;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;

  const TouchFeedbackListTile({
    super.key,
    this.leading,
    this.title,
    this.subtitle,
    this.trailing,
    this.onTap,
    this.rippleColor,
    this.hapticType = HapticFeedbackType.light,
    this.enableHaptic = true,
  });

  @override
  Widget build(BuildContext context) {
    return TouchFeedbackContainer(
      onTap: onTap,
      rippleColor: rippleColor,
      borderRadius: 0,
      hapticType: hapticType,
      enableHaptic: enableHaptic,
      child: ListTile(
        leading: leading,
        title: title,
        subtitle: subtitle,
        trailing: trailing,
      ),
    );
  }
}

/// Touch feedback card
class TouchFeedbackCard extends StatelessWidget {
  final Widget child;
  final VoidCallback? onTap;
  final Color? rippleColor;
  final double borderRadius;
  final EdgeInsets? margin;
  final EdgeInsets? padding;
  final HapticFeedbackType hapticType;
  final bool enableHaptic;

  const TouchFeedbackCard({
    super.key,
    required this.child,
    this.onTap,
    this.rippleColor,
    this.borderRadius = 12.0,
    this.margin,
    this.padding,
    this.hapticType = HapticFeedbackType.light,
    this.enableHaptic = true,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: margin,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(borderRadius),
      ),
      child: TouchFeedbackContainer(
        onTap: onTap,
        rippleColor: rippleColor,
        borderRadius: borderRadius,
        padding: padding,
        hapticType: hapticType,
        enableHaptic: enableHaptic,
        child: child,
      ),
    );
  }
}
