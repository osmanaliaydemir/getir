import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../constants/route_constants.dart';
import '../../presentation/pages/splash/splash_page.dart';
import '../../presentation/pages/onboarding/onboarding_page.dart';
import '../../presentation/pages/auth/login_page.dart';
import '../../presentation/pages/auth/register_page.dart';
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
import '../../presentation/pages/payment/payment_page.dart';
import '../../presentation/pages/addresses/addresses_page.dart';
import '../../presentation/pages/notifications/notifications_page.dart';
import '../../presentation/pages/error/not_found_page.dart';
import '../../presentation/widgets/common/main_navigation.dart';

class AppRouter {
  static final GoRouter _router = GoRouter(
    initialLocation: RouteConstants.splash,
    debugLogDiagnostics: true,
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
      
      // Error Routes
      GoRoute(
        path: RouteConstants.notFound,
        name: RouteConstants.notFoundRouteName,
        builder: (context, state) => const NotFoundPage(),
      ),
    ],
    errorBuilder: (context, state) => const NotFoundPage(),
    redirect: (context, state) {
      // TODO: Implement authentication redirect logic
      // Check if user is authenticated and redirect accordingly
      return null;
    },
  );
  
  static GoRouter get router => _router;
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
    context.go(RouteConstants.merchantDetail.replaceAll(':merchantId', merchantId));
  }
  
  static void goToProductDetail(BuildContext context, String productId) {
    context.go(RouteConstants.productDetail.replaceAll(':productId', productId));
  }
  
  // Order Navigation
  static void goToCheckout(BuildContext context) {
    context.go(RouteConstants.checkout);
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
