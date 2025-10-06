import 'package:flutter/material.dart';

/// Custom button animations and interactions
class ButtonAnimations {
  /// Animated button with scale effect
  static Widget animatedButton({
    required Widget child,
    required VoidCallback onPressed,
    Duration duration = const Duration(milliseconds: 150),
    double scale = 0.95,
    Curve curve = Curves.easeInOut,
  }) {
    return _AnimatedButton(
      onPressed: onPressed,
      duration: duration,
      scale: scale,
      curve: curve,
      child: child,
    );
  }

  /// Bounce animation for success actions
  static Widget bounceButton({
    required Widget child,
    required VoidCallback onPressed,
    Duration duration = const Duration(milliseconds: 200),
    double bounceScale = 1.1,
  }) {
    return _BounceButton(
      onPressed: onPressed,
      duration: duration,
      bounceScale: bounceScale,
      child: child,
    );
  }

  /// Pulse animation for important actions
  static Widget pulseButton({
    required Widget child,
    required VoidCallback onPressed,
    Duration duration = const Duration(milliseconds: 1000),
    double pulseScale = 1.05,
  }) {
    return _PulseButton(
      onPressed: onPressed,
      duration: duration,
      pulseScale: pulseScale,
      child: child,
    );
  }

  /// Shake animation for error states
  static Widget shakeButton({
    required Widget child,
    required VoidCallback onPressed,
    Duration duration = const Duration(milliseconds: 500),
    double shakeIntensity = 10.0,
  }) {
    return _ShakeButton(
      onPressed: onPressed,
      duration: duration,
      shakeIntensity: shakeIntensity,
      child: child,
    );
  }

  /// Loading button with spinner
  static Widget loadingButton({
    required Widget child,
    required bool isLoading,
    required VoidCallback onPressed,
    Color? loadingColor,
    double size = 20.0,
  }) {
    return _LoadingButton(
      onPressed: isLoading ? null : onPressed,
      isLoading: isLoading,
      loadingColor: loadingColor,
      size: size,
      child: child,
    );
  }

  /// Ripple effect button
  static Widget rippleButton({
    required Widget child,
    required VoidCallback onPressed,
    Color? rippleColor,
    double borderRadius = 8.0,
  }) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onPressed,
        borderRadius: BorderRadius.circular(borderRadius),
        splashColor: rippleColor ?? Colors.white.withOpacity(0.2),
        highlightColor:
            rippleColor?.withOpacity(0.1) ?? Colors.white.withOpacity(0.1),
        child: child,
      ),
    );
  }
}

/// Animated button with scale effect
class _AnimatedButton extends StatefulWidget {
  final Widget child;
  final VoidCallback onPressed;
  final Duration duration;
  final double scale;
  final Curve curve;

  const _AnimatedButton({
    required this.child,
    required this.onPressed,
    required this.duration,
    required this.scale,
    required this.curve,
  });

  @override
  State<_AnimatedButton> createState() => _AnimatedButtonState();
}

class _AnimatedButtonState extends State<_AnimatedButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _scaleAnimation = Tween<double>(
      begin: 1.0,
      end: widget.scale,
    ).animate(CurvedAnimation(parent: _controller, curve: widget.curve));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTap() {
    _controller.forward().then((_) {
      _controller.reverse();
    });
    widget.onPressed();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: _handleTap,
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

/// Bounce button animation
class _BounceButton extends StatefulWidget {
  final Widget child;
  final VoidCallback onPressed;
  final Duration duration;
  final double bounceScale;

  const _BounceButton({
    required this.child,
    required this.onPressed,
    required this.duration,
    required this.bounceScale,
  });

  @override
  State<_BounceButton> createState() => _BounceButtonState();
}

class _BounceButtonState extends State<_BounceButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _bounceAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _bounceAnimation = TweenSequence<double>([
      TweenSequenceItem(
        tween: Tween<double>(begin: 1.0, end: widget.bounceScale),
        weight: 50,
      ),
      TweenSequenceItem(
        tween: Tween<double>(begin: widget.bounceScale, end: 1.0),
        weight: 50,
      ),
    ]).animate(CurvedAnimation(parent: _controller, curve: Curves.elasticOut));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTap() {
    _controller.forward().then((_) {
      _controller.reset();
    });
    widget.onPressed();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: _handleTap,
      child: AnimatedBuilder(
        animation: _bounceAnimation,
        builder: (context, child) {
          return Transform.scale(
            scale: _bounceAnimation.value,
            child: widget.child,
          );
        },
      ),
    );
  }
}

/// Pulse button animation
class _PulseButton extends StatefulWidget {
  final Widget child;
  final VoidCallback onPressed;
  final Duration duration;
  final double pulseScale;

  const _PulseButton({
    required this.child,
    required this.onPressed,
    required this.duration,
    required this.pulseScale,
  });

  @override
  State<_PulseButton> createState() => _PulseButtonState();
}

class _PulseButtonState extends State<_PulseButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _pulseAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _pulseAnimation = Tween<double>(
      begin: 1.0,
      end: widget.pulseScale,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
    _controller.repeat(reverse: true);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: widget.onPressed,
      child: AnimatedBuilder(
        animation: _pulseAnimation,
        builder: (context, child) {
          return Transform.scale(
            scale: _pulseAnimation.value,
            child: widget.child,
          );
        },
      ),
    );
  }
}

/// Shake button animation
class _ShakeButton extends StatefulWidget {
  final Widget child;
  final VoidCallback onPressed;
  final Duration duration;
  final double shakeIntensity;

  const _ShakeButton({
    required this.child,
    required this.onPressed,
    required this.duration,
    required this.shakeIntensity,
  });

  @override
  State<_ShakeButton> createState() => _ShakeButtonState();
}

class _ShakeButtonState extends State<_ShakeButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _shakeAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _shakeAnimation = Tween<double>(
      begin: 0.0,
      end: widget.shakeIntensity,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.elasticIn));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTap() {
    _controller.forward().then((_) {
      _controller.reset();
    });
    widget.onPressed();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: _handleTap,
      child: AnimatedBuilder(
        animation: _shakeAnimation,
        builder: (context, child) {
          return Transform.translate(
            offset: Offset(
              _shakeAnimation.value *
                  (0.5 - (DateTime.now().millisecondsSinceEpoch % 2)),
              0,
            ),
            child: widget.child,
          );
        },
      ),
    );
  }
}

/// Loading button with spinner
class _LoadingButton extends StatelessWidget {
  final Widget child;
  final VoidCallback? onPressed;
  final bool isLoading;
  final Color? loadingColor;
  final double size;

  const _LoadingButton({
    required this.child,
    required this.onPressed,
    required this.isLoading,
    this.loadingColor,
    required this.size,
  });

  @override
  Widget build(BuildContext context) {
    return Stack(
      alignment: Alignment.center,
      children: [
        Opacity(opacity: isLoading ? 0.0 : 1.0, child: child),
        if (isLoading)
          SizedBox(
            width: size,
            height: size,
            child: CircularProgressIndicator(
              strokeWidth: 2.0,
              valueColor: AlwaysStoppedAnimation<Color>(
                loadingColor ?? Theme.of(context).primaryColor,
              ),
            ),
          ),
      ],
    );
  }
}
