import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../bloc/cart/cart_bloc.dart';

class MainNavigation extends StatelessWidget {
  final Widget child;

  const MainNavigation({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final currentRoute = GoRouterState.of(context).uri.toString();

    return Scaffold(
      body: child,
      bottomNavigationBar: Container(
        decoration: const BoxDecoration(
          color: AppColors.surface,
          boxShadow: [
            BoxShadow(
              color: AppColors.shadowLight,
              blurRadius: 8,
              offset: Offset(0, -2),
            ),
          ],
        ),
        child: SafeArea(
          child: Container(
            height: 80,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            child: BlocBuilder<CartBloc, CartState>(
              builder: (context, cartState) {
                final cartItemCount = cartState is CartLoaded
                    ? cartState.cart.items.length
                    : 0;

                return Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildNavItem(
                      context: context,
                      icon: Icons.home_outlined,
                      activeIcon: Icons.home,
                      label: l10n.home,
                      route: '/home',
                      index: 0,
                      isActive: currentRoute == '/home',
                    ),
                    _buildNavItem(
                      context: context,
                      icon: Icons.search_outlined,
                      activeIcon: Icons.search,
                      label: l10n.search,
                      route: '/search',
                      index: 1,
                      isActive: currentRoute == '/search',
                    ),
                    _buildNavItem(
                      context: context,
                      icon: Icons.shopping_cart_outlined,
                      activeIcon: Icons.shopping_cart,
                      label: l10n.cart,
                      route: '/cart',
                      index: 2,
                      badgeCount: cartItemCount,
                      isActive: currentRoute == '/cart',
                    ),
                    _buildNavItem(
                      context: context,
                      icon: Icons.receipt_long_outlined,
                      activeIcon: Icons.receipt_long,
                      label: l10n.orders,
                      route: '/orders',
                      index: 3,
                      isActive: currentRoute == '/orders',
                    ),
                    _buildNavItem(
                      context: context,
                      icon: Icons.person_outline,
                      activeIcon: Icons.person,
                      label: l10n.profile,
                      route: '/profile',
                      index: 4,
                      isActive: currentRoute == '/profile',
                    ),
                  ],
                );
              },
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildNavItem({
    required BuildContext context,
    required IconData icon,
    required IconData activeIcon,
    required String label,
    required String route,
    required int index,
    required bool isActive,
    int? badgeCount,
  }) {
    final iconColor = isActive ? AppColors.primary : AppColors.textSecondary;
    final textColor = isActive ? AppColors.primary : AppColors.textSecondary;
    final displayIcon = isActive ? activeIcon : icon;

    return Expanded(
      child: GestureDetector(
        onTap: () {
          debugPrint('🔘 [BottomNav] Tab $index tapped (route: $route)');

          // GoRouter ile navigate et (context.go root'a döner)
          debugPrint('   Navigating to $route via GoRouter');
          context.go(route);
        },
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 8),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Icon with badge
              Stack(
                children: [
                  Icon(displayIcon, color: iconColor, size: 24),
                  if (badgeCount != null && badgeCount > 0)
                    Positioned(
                      right: -2,
                      top: -2,
                      child: Container(
                        padding: const EdgeInsets.all(2),
                        decoration: const BoxDecoration(
                          color: AppColors.error,
                          shape: BoxShape.circle,
                        ),
                        constraints: const BoxConstraints(
                          minWidth: 16,
                          minHeight: 16,
                        ),
                        child: Text(
                          badgeCount.toString(),
                          style: const TextStyle(
                            color: AppColors.white,
                            fontSize: 10,
                            fontWeight: FontWeight.bold,
                          ),
                          textAlign: TextAlign.center,
                        ),
                      ),
                    ),
                ],
              ),
              const SizedBox(height: 4),
              Text(
                label,
                style: AppTypography.labelSmall.copyWith(
                  color: textColor,
                  fontWeight: isActive ? FontWeight.w600 : FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
