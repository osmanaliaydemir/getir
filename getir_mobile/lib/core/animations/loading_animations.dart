import 'package:flutter/material.dart';

/// Custom loading animations for the app
class LoadingAnimations {
  /// Shimmer loading effect
  static Widget shimmer({
    required Widget child,
    Color? baseColor,
    Color? highlightColor,
    Duration duration = const Duration(milliseconds: 1500),
  }) {
    return _ShimmerLoading(
      baseColor: baseColor,
      highlightColor: highlightColor,
      duration: duration,
      child: child,
    );
  }

  /// Pulse loading animation
  static Widget pulse({
    required Widget child,
    Color? color,
    Duration duration = const Duration(milliseconds: 1000),
    double minOpacity = 0.3,
    double maxOpacity = 1.0,
  }) {
    return _PulseLoading(
      color: color,
      duration: duration,
      minOpacity: minOpacity,
      maxOpacity: maxOpacity,
      child: child,
    );
  }

  /// Rotating loading animation
  static Widget rotate({
    required Widget child,
    Duration duration = const Duration(seconds: 2),
    Curve curve = Curves.linear,
  }) {
    return _RotateLoading(duration: duration, curve: curve, child: child);
  }

  /// Bounce loading animation
  static Widget bounce({
    required Widget child,
    Duration duration = const Duration(milliseconds: 600),
    double bounceHeight = 20.0,
  }) {
    return _BounceLoading(
      duration: duration,
      bounceHeight: bounceHeight,
      child: child,
    );
  }

  /// Fade in/out loading animation
  static Widget fade({
    required Widget child,
    Duration duration = const Duration(milliseconds: 800),
    double minOpacity = 0.0,
    double maxOpacity = 1.0,
  }) {
    return _FadeLoading(
      duration: duration,
      minOpacity: minOpacity,
      maxOpacity: maxOpacity,
      child: child,
    );
  }

  /// Scale loading animation
  static Widget scale({
    required Widget child,
    Duration duration = const Duration(milliseconds: 1000),
    double minScale = 0.8,
    double maxScale = 1.2,
  }) {
    return _ScaleLoading(
      duration: duration,
      minScale: minScale,
      maxScale: maxScale,
      child: child,
    );
  }

  /// Custom loading indicator with Getir branding
  static Widget getirLoader({
    double size = 50.0,
    Color? color,
    Duration duration = const Duration(milliseconds: 1200),
  }) {
    return _GetirLoader(size: size, color: color, duration: duration);
  }

  /// Skeleton loading for cards
  static Widget skeletonCard({
    double? width,
    double? height,
    double borderRadius = 8.0,
    EdgeInsets? margin,
  }) {
    return Container(
      width: width,
      height: height ?? 120.0,
      margin: margin,
      decoration: BoxDecoration(
        color: Colors.grey[300],
        borderRadius: BorderRadius.circular(borderRadius),
      ),
      child: shimmer(
        baseColor: Colors.grey[300],
        highlightColor: Colors.grey[100],
        child: Container(),
      ),
    );
  }

  /// Skeleton loading for text
  static Widget skeletonText({
    double? width,
    double height = 16.0,
    double borderRadius = 4.0,
    EdgeInsets? margin,
  }) {
    return Container(
      width: width,
      height: height,
      margin: margin,
      decoration: BoxDecoration(
        color: Colors.grey[300],
        borderRadius: BorderRadius.circular(borderRadius),
      ),
      child: shimmer(
        baseColor: Colors.grey[300],
        highlightColor: Colors.grey[100],
        child: Container(),
      ),
    );
  }
}

/// Shimmer loading effect
class _ShimmerLoading extends StatefulWidget {
  final Widget child;
  final Color? baseColor;
  final Color? highlightColor;
  final Duration duration;

  const _ShimmerLoading({
    required this.child,
    this.baseColor,
    this.highlightColor,
    required this.duration,
  });

  @override
  State<_ShimmerLoading> createState() => _ShimmerLoadingState();
}

class _ShimmerLoadingState extends State<_ShimmerLoading>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: -1.0,
      end: 2.0,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
    _controller.repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return ShaderMask(
          blendMode: BlendMode.srcATop,
          shaderCallback: (bounds) {
            return LinearGradient(
              begin: Alignment.centerLeft,
              end: Alignment.centerRight,
              colors: [
                widget.baseColor ?? Colors.grey[300]!,
                widget.highlightColor ?? Colors.grey[100]!,
                widget.baseColor ?? Colors.grey[300]!,
              ],
              stops: [
                _animation.value - 0.3,
                _animation.value,
                _animation.value + 0.3,
              ].map((stop) => stop.clamp(0.0, 1.0)).toList(),
            ).createShader(bounds);
          },
          child: widget.child,
        );
      },
    );
  }
}

/// Pulse loading animation
class _PulseLoading extends StatefulWidget {
  final Widget child;
  final Color? color;
  final Duration duration;
  final double minOpacity;
  final double maxOpacity;

  const _PulseLoading({
    required this.child,
    this.color,
    required this.duration,
    required this.minOpacity,
    required this.maxOpacity,
  });

  @override
  State<_PulseLoading> createState() => _PulseLoadingState();
}

class _PulseLoadingState extends State<_PulseLoading>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: widget.minOpacity,
      end: widget.maxOpacity,
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
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Opacity(opacity: _animation.value, child: widget.child);
      },
    );
  }
}

/// Rotate loading animation
class _RotateLoading extends StatefulWidget {
  final Widget child;
  final Duration duration;
  final Curve curve;

  const _RotateLoading({
    required this.child,
    required this.duration,
    required this.curve,
  });

  @override
  State<_RotateLoading> createState() => _RotateLoadingState();
}

class _RotateLoadingState extends State<_RotateLoading>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(parent: _controller, curve: widget.curve));
    _controller.repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Transform.rotate(
          angle: _animation.value * 2 * 3.14159,
          child: widget.child,
        );
      },
    );
  }
}

/// Bounce loading animation
class _BounceLoading extends StatefulWidget {
  final Widget child;
  final Duration duration;
  final double bounceHeight;

  const _BounceLoading({
    required this.child,
    required this.duration,
    required this.bounceHeight,
  });

  @override
  State<_BounceLoading> createState() => _BounceLoadingState();
}

class _BounceLoadingState extends State<_BounceLoading>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: 0.0,
      end: widget.bounceHeight,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.elasticOut));
    _controller.repeat(reverse: true);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Transform.translate(
          offset: Offset(0, -_animation.value),
          child: widget.child,
        );
      },
    );
  }
}

/// Fade loading animation
class _FadeLoading extends StatefulWidget {
  final Widget child;
  final Duration duration;
  final double minOpacity;
  final double maxOpacity;

  const _FadeLoading({
    required this.child,
    required this.duration,
    required this.minOpacity,
    required this.maxOpacity,
  });

  @override
  State<_FadeLoading> createState() => _FadeLoadingState();
}

class _FadeLoadingState extends State<_FadeLoading>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: widget.minOpacity,
      end: widget.maxOpacity,
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
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Opacity(opacity: _animation.value, child: widget.child);
      },
    );
  }
}

/// Scale loading animation
class _ScaleLoading extends StatefulWidget {
  final Widget child;
  final Duration duration;
  final double minScale;
  final double maxScale;

  const _ScaleLoading({
    required this.child,
    required this.duration,
    required this.minScale,
    required this.maxScale,
  });

  @override
  State<_ScaleLoading> createState() => _ScaleLoadingState();
}

class _ScaleLoadingState extends State<_ScaleLoading>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: widget.minScale,
      end: widget.maxScale,
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
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Transform.scale(scale: _animation.value, child: widget.child);
      },
    );
  }
}

/// Custom Getir loader
class _GetirLoader extends StatefulWidget {
  final double size;
  final Color? color;
  final Duration duration;

  const _GetirLoader({required this.size, this.color, required this.duration});

  @override
  State<_GetirLoader> createState() => _GetirLoaderState();
}

class _GetirLoaderState extends State<_GetirLoader>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(duration: widget.duration, vsync: this);
    _animation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeInOut));
    _controller.repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Container(
          width: widget.size,
          height: widget.size,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            gradient: SweepGradient(
              colors: [
                widget.color ?? Theme.of(context).primaryColor,
                widget.color?.withOpacity(0.3) ??
                    Theme.of(context).primaryColor.withOpacity(0.3),
                widget.color ?? Theme.of(context).primaryColor,
              ],
              stops: [
                _animation.value - 0.1,
                _animation.value,
                _animation.value + 0.1,
              ].map((stop) => stop.clamp(0.0, 1.0)).toList(),
            ),
          ),
          child: Center(
            child: Container(
              width: widget.size * 0.7,
              height: widget.size * 0.7,
              decoration: BoxDecoration(
                color: Theme.of(context).scaffoldBackgroundColor,
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.local_shipping,
                size: widget.size * 0.3,
                color: widget.color ?? Theme.of(context).primaryColor,
              ),
            ),
          ),
        );
      },
    );
  }
}
