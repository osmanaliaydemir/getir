import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

/// Animated add to cart widget with success feedback
class AnimatedAddToCart extends StatefulWidget {
  final VoidCallback onPressed;
  final bool isLoading;
  final bool showSuccess;

  const AnimatedAddToCart({
    super.key,
    required this.onPressed,
    this.isLoading = false,
    this.showSuccess = false,
  });

  @override
  State<AnimatedAddToCart> createState() => _AnimatedAddToCartState();
}

class _AnimatedAddToCartState extends State<AnimatedAddToCart>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;
  late Animation<double> _rotationAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );

    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.9).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );

    _rotationAnimation = Tween<double>(begin: 0.0, end: 0.1).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTap() {
    _controller.forward().then((_) {
      _controller.reverse();
      widget.onPressed();
    });
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: widget.isLoading || widget.showSuccess ? null : _handleTap,
      child: AnimatedBuilder(
        animation: _controller,
        builder: (context, child) {
          return Transform.scale(
            scale: _scaleAnimation.value,
            child: Transform.rotate(
              angle: _rotationAnimation.value,
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 8,
                ),
                decoration: BoxDecoration(
                  color: widget.showSuccess
                      ? AppColors.success
                      : AppColors.primary,
                  borderRadius: BorderRadius.circular(20),
                  boxShadow: [
                    BoxShadow(
                      color: (widget.showSuccess
                              ? AppColors.success
                              : AppColors.primary)
                          .withOpacity(0.3),
                      blurRadius: 8,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    if (widget.isLoading)
                      const SizedBox(
                        width: 16,
                        height: 16,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          valueColor: AlwaysStoppedAnimation<Color>(
                            AppColors.white,
                          ),
                        ),
                      )
                    else if (widget.showSuccess)
                      const Icon(
                        Icons.check,
                        color: AppColors.white,
                        size: 16,
                      )
                    else
                      const Icon(
                        Icons.add_shopping_cart,
                        color: AppColors.white,
                        size: 16,
                      ),
                    const SizedBox(width: 8),
                    Text(
                      widget.isLoading
                          ? 'Ekleniyor...'
                          : widget.showSuccess
                              ? 'Eklendi!'
                              : 'Sepete Ekle',
                      style: const TextStyle(
                        color: AppColors.white,
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}

/// Simple add to cart icon button with pulse animation
class PulseAddToCartButton extends StatefulWidget {
  final VoidCallback onPressed;
  final bool showBadge;
  final int badgeCount;

  const PulseAddToCartButton({
    super.key,
    required this.onPressed,
    this.showBadge = false,
    this.badgeCount = 0,
  });

  @override
  State<PulseAddToCartButton> createState() => _PulseAddToCartButtonState();
}

class _PulseAddToCartButtonState extends State<PulseAddToCartButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 200),
      vsync: this,
    );

    _scaleAnimation = Tween<double>(begin: 1.0, end: 1.2).animate(
      CurvedAnimation(parent: _controller, curve: Curves.elasticOut),
    );
  }

  @override
  void didUpdateWidget(PulseAddToCartButton oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.badgeCount > oldWidget.badgeCount) {
      _controller.forward().then((_) => _controller.reverse());
    }
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
        animation: _scaleAnimation,
        builder: (context, child) {
          return Transform.scale(
            scale: _scaleAnimation.value,
            child: Badge(
              label: Text(widget.badgeCount.toString()),
              isLabelVisible: widget.showBadge && widget.badgeCount > 0,
              child: const Icon(
                Icons.add_shopping_cart,
                color: AppColors.primary,
              ),
            ),
          );
        },
      ),
    );
  }
}

