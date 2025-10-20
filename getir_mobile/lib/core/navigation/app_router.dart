import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:go_router/go_router.dart';
import '../constants/route_constants.dart';
import '../../presentation/pages/splash/splash_page.dart';
import '../../presentation/pages/onboarding/onboarding_page.dart';
import '../../presentation/pages/auth/login_page.dart';
import '../../presentation/pages/auth/register_page.dart';
import '../../presentation/pages/auth/forgot_password_page.dart';
import '../../presentation/pages/home/home_page.dart';
import '../../presentation/pages/search/search_page.dart';
import '../../presentation/pages/cart/cart_page.dart';
import '../../presentation/pages/orders/orders_page.dart';
import '../../presentation/pages/profile/profile_page.dart';
import '../../presentation/pages/merchant/merchant_detail_page.dart';
import '../../presentation/pages/product/product_detail_page.dart';
import '../../presentation/pages/checkout/checkout_page.dart';
import '../../presentation/pages/order/order_detail_page.dart';
import '../../presentation/pages/order/order_tracking_page.dart';
import '../../presentation/pages/order/order_confirmation_page.dart';
import '../../presentation/pages/payment/payment_page.dart';
import '../../../domain/entities/order.dart';
import '../../presentation/pages/addresses/addresses_page.dart';
import '../../presentation/pages/notifications/notifications_page.dart';
import '../../presentation/pages/notifications/notification_settings_page.dart';
import '../../presentation/pages/settings/settings_page.dart';
import '../../presentation/pages/error/not_found_page.dart';
import '../../presentation/widgets/common/main_navigation.dart';
import '../../presentation/pages/merchant/category_merchants_page.dart';
import '../../domain/entities/service_category_type.dart';
import '../services/local_storage_service.dart';
import '../services/analytics_service.dart';
import '../di/injection.dart';

class AppRouter {
  static final GoRouter _router = GoRouter(
    initialLocation: RouteConstants.splash,
    debugLogDiagnostics: true,
    observers: [_AnalyticsRouteObserver(getIt<AnalyticsService>())],
    routes: [
      // Splash Route
      GoRoute(
        path: RouteConstants.splash,
        name: RouteConstants.splashRouteName,
        builder: (context, state) => const SplashPage(),
      ),

      // Onboarding Route
      GoRoute(
        path: RouteConstants.onboarding,
        name: RouteConstants.onboardingRouteName,
        builder: (context, state) => const OnboardingPage(),
      ),

      // Authentication Routes
      GoRoute(
        path: RouteConstants.login,
        name: RouteConstants.loginRouteName,
        builder: (context, state) => const LoginPage(),
      ),
      GoRoute(
        path: RouteConstants.register,
        name: RouteConstants.registerRouteName,
        builder: (context, state) => const RegisterPage(),
      ),
      GoRoute(
        path: '/forgot-password',
        name: 'forgot-password',
        builder: (context, state) => const ForgotPasswordPage(),
      ),

      // Main App Routes with Bottom Navigation
      ShellRoute(
        builder: (context, state, child) => MainNavigation(child: child),
        routes: [
          GoRoute(
            path: RouteConstants.home,
            name: RouteConstants.homeRouteName,
            builder: (context, state) => const HomePage(),
          ),
          GoRoute(
            path: RouteConstants.search,
            name: RouteConstants.searchRouteName,
            builder: (context, state) => const SearchPage(),
          ),
          GoRoute(
            path: RouteConstants.cart,
            name: RouteConstants.cartRouteName,
            builder: (context, state) => const CartPage(),
          ),
          GoRoute(
            path: RouteConstants.orders,
            name: RouteConstants.ordersRouteName,
            builder: (context, state) => const OrdersPage(),
          ),
          GoRoute(
            path: RouteConstants.profile,
            name: RouteConstants.profileRouteName,
            builder: (context, state) => const ProfilePage(),
          ),
        ],
      ),

      // Product Routes
      GoRoute(
        path: '/category-merchants/:categoryType',
        name: 'category-merchants',
        builder: (context, state) {
          final categoryTypeValue = int.parse(
            state.pathParameters['categoryType']!,
          );
          final categoryName = state.uri.queryParameters['name']!;
          final latitude = double.parse(state.uri.queryParameters['lat']!);
          final longitude = double.parse(state.uri.queryParameters['lng']!);

          return CategoryMerchantsPage(
            categoryType: ServiceCategoryType.fromInt(categoryTypeValue),
            categoryName: categoryName,
            latitude: latitude,
            longitude: longitude,
          );
        },
      ),
      GoRoute(
        path: RouteConstants.merchantDetail,
        name: RouteConstants.merchantDetailRouteName,
        builder: (context, state) {
          final merchantId = state.pathParameters['merchantId']!;
          return MerchantDetailPage(merchantId: merchantId);
        },
      ),
      GoRoute(
        path: RouteConstants.productDetail,
        name: RouteConstants.productDetailRouteName,
        builder: (context, state) {
          final productId = state.pathParameters['productId']!;
          return ProductDetailPage(productId: productId);
        },
      ),

      // Order Routes
      GoRoute(
        path: RouteConstants.checkout,
        name: RouteConstants.checkoutRouteName,
        builder: (context, state) => const CheckoutPage(),
      ),
      GoRoute(
        path: '/order-confirmation',
        name: 'order-confirmation',
        builder: (context, state) {
          final order = state.extra as Order;
          return OrderConfirmationPage(order: order);
        },
      ),
      GoRoute(
        path: RouteConstants.orderDetail,
        name: RouteConstants.orderDetailRouteName,
        builder: (context, state) {
          final orderId = state.pathParameters['orderId']!;
          return OrderDetailPage(orderId: orderId);
        },
      ),
      GoRoute(
        path: RouteConstants.orderTracking,
        name: RouteConstants.orderTrackingRouteName,
        builder: (context, state) {
          final orderId = state.pathParameters['orderId']!;
          return OrderTrackingPage(orderId: orderId);
        },
      ),
      GoRoute(
        path: RouteConstants.payment,
        name: RouteConstants.paymentRouteName,
        builder: (context, state) => const PaymentPage(),
      ),

      // Profile Routes
      GoRoute(
        path: RouteConstants.addresses,
        name: RouteConstants.addressesRouteName,
        builder: (context, state) => const AddressesPage(),
      ),
      GoRoute(
        path: RouteConstants.notifications,
        name: RouteConstants.notificationsRouteName,
        builder: (context, state) => const NotificationsPage(),
      ),
      GoRoute(
        path: RouteConstants.notificationSettings,
        name: RouteConstants.notificationSettingsRouteName,
        builder: (context, state) => const NotificationSettingsPage(),
      ),
      GoRoute(
        path: RouteConstants.settings,
        name: RouteConstants.settingsRouteName,
        builder: (context, state) => const SettingsPage(),
      ),

      // Error Routes
      GoRoute(
        path: RouteConstants.notFound,
        name: RouteConstants.notFoundRouteName,
        builder: (context, state) => const NotFoundPage(),
      ),
    ],
    errorBuilder: (context, state) => const NotFoundPage(),
    redirect: (context, state) {
      // Minimal redirect - only handle onboarding
      // Auth kontrolü SplashPage'de yapılıyor (async token kontrolü için)
      final storage = getIt<LocalStorageService>();
      final hasOnboarded = storage.getUserData('has_seen_onboarding') == 'true';

      final currentPath = state.uri.path;
      final isSplash = currentPath == RouteConstants.splash;
      final isOnboarding = currentPath == RouteConstants.onboarding;

      // Onboarding kontrolü
      if (!hasOnboarded && !isSplash && !isOnboarding) {
        debugPrint('🔀 [GoRouter] Redirecting to onboarding (not completed)');
        return RouteConstants.onboarding;
      }

      // Token kontrolü SplashPage'de yapılıyor (async)
      return null;
    },
  );

  static GoRouter get router => _router;
}

class _AnalyticsRouteObserver extends NavigatorObserver {
  final AnalyticsService _analytics;

  _AnalyticsRouteObserver(this._analytics);

  @override
  void didPush(Route<dynamic> route, Route<dynamic>? previousRoute) {
    super.didPush(route, previousRoute);
    final name = route.settings.name ?? _getRouteName(route);
    if (name.isNotEmpty) {
      _analytics.logScreenView(screenName: name);
    }
  }

  @override
  void didPop(Route<dynamic> route, Route<dynamic>? previousRoute) {
    super.didPop(route, previousRoute);
    if (previousRoute != null) {
      final name = previousRoute.settings.name ?? _getRouteName(previousRoute);
      if (name.isNotEmpty) {
        _analytics.logScreenView(screenName: name);
      }
    }
  }

  @override
  void didReplace({Route<dynamic>? newRoute, Route<dynamic>? oldRoute}) {
    super.didReplace(newRoute: newRoute, oldRoute: oldRoute);
    if (newRoute != null) {
      final name = newRoute.settings.name ?? _getRouteName(newRoute);
      if (name.isNotEmpty) {
        _analytics.logScreenView(screenName: name);
      }
    }
  }

  String _getRouteName(Route route) {
    // Try to extract route name from route string
    final routeStr = route.toString();
    if (routeStr.contains('MaterialPageRoute')) {
      return routeStr.split('(').first.replaceAll('MaterialPageRoute', '');
    }
    return routeStr;
  }
}

// Navigation Helper Class
class AppNavigation {
  // Authentication Navigation
  static void goToSplash(BuildContext context) {
    context.go(RouteConstants.splash);
  }

  static void goToOnboarding(BuildContext context) {
    context.go(RouteConstants.onboarding);
  }

  static void goToLogin(BuildContext context) {
    context.go(RouteConstants.login);
  }

  static void goToRegister(BuildContext context) {
    context.go(RouteConstants.register);
  }

  // Main App Navigation
  static void goToHome(BuildContext context) {
    context.go(RouteConstants.home);
  }

  static void goToSearch(BuildContext context) {
    context.go(RouteConstants.search);
  }

  static void goToCart(BuildContext context) {
    context.go(RouteConstants.cart);
  }

  static void goToOrders(BuildContext context) {
    context.go(RouteConstants.orders);
  }

  static void goToProfile(BuildContext context) {
    context.go(RouteConstants.profile);
  }

  // Product Navigation
  static void goToMerchantDetail(BuildContext context, String merchantId) {
    context.go(
      RouteConstants.merchantDetail.replaceAll(':merchantId', merchantId),
    );
  }

  static void goToProductDetail(BuildContext context, String productId) {
    context.go(
      RouteConstants.productDetail.replaceAll(':productId', productId),
    );
  }

  // Order Navigation
  static void goToCheckout(BuildContext context) {
    context.go(RouteConstants.checkout);
  }

  static void goToOrderConfirmation(BuildContext context, dynamic order) {
    context.goNamed('order-confirmation', extra: order);
  }

  static void goToOrderDetail(BuildContext context, String orderId) {
    context.go(RouteConstants.orderDetail.replaceAll(':orderId', orderId));
  }

  static void goToOrderTracking(BuildContext context, String orderId) {
    context.go(RouteConstants.orderTracking.replaceAll(':orderId', orderId));
  }

  static void goToPayment(BuildContext context) {
    context.go(RouteConstants.payment);
  }

  // Profile Navigation
  static void goToAddresses(BuildContext context) {
    context.go(RouteConstants.addresses);
  }

  static void goToNotifications(BuildContext context) {
    context.go(RouteConstants.notifications);
  }

  // Utility Navigation
  static void goBack(BuildContext context) {
    context.pop();
  }

  static void goBackToRoot(BuildContext context) {
    context.go(RouteConstants.home);
  }

  // Deep Link Navigation
  static void handleDeepLink(BuildContext context, String deepLink) {
    try {
      context.go(deepLink);
    } catch (e) {
      // Handle invalid deep link
      context.go(RouteConstants.notFound);
    }
  }
}
