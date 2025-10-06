import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../constants/route_constants.dart';

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

  static void goToForgotPassword(BuildContext context) {
    context.go(RouteConstants.forgotPassword);
  }

  static void goToResetPassword(BuildContext context) {
    context.go(RouteConstants.resetPassword);
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

  static void goToSettings(BuildContext context) {
    context.go(RouteConstants.settings);
  }

  // Product Navigation
  static void goToMerchantDetail(BuildContext context, String merchantId) {
    context.go('/merchant/$merchantId');
  }

  static void goToProductDetail(BuildContext context, String productId) {
    context.go('/product/$productId');
  }

  static void goToProductCategory(BuildContext context, String categoryId) {
    context.go('/category/$categoryId');
  }

  // Order Navigation
  static void goToCheckout(BuildContext context) {
    context.go(RouteConstants.checkout);
  }

  static void goToOrderDetail(BuildContext context, String orderId) {
    context.go('/order/$orderId');
  }

  static void goToOrderTracking(BuildContext context, String orderId) {
    context.go('/order/$orderId/tracking');
  }

  static void goToPayment(BuildContext context) {
    context.go(RouteConstants.payment);
  }

  static void goToPaymentSuccess(BuildContext context) {
    context.go(RouteConstants.paymentSuccess);
  }

  static void goToPaymentFailed(BuildContext context) {
    context.go(RouteConstants.paymentFailed);
  }

  // Profile Navigation
  static void goToEditProfile(BuildContext context) {
    context.go(RouteConstants.editProfile);
  }

  static void goToAddresses(BuildContext context) {
    context.go(RouteConstants.addresses);
  }

  static void goToAddAddress(BuildContext context) {
    context.go(RouteConstants.addAddress);
  }

  static void goToEditAddress(BuildContext context, String addressId) {
    context.go('/addresses/edit/$addressId');
  }

  static void goToNotifications(BuildContext context) {
    context.go(RouteConstants.notifications);
  }

  static void goToNotificationSettings(BuildContext context) {
    context.go(RouteConstants.notificationSettings);
  }

  static void goToCoupons(BuildContext context) {
    context.go(RouteConstants.coupons);
  }

  static void goToReviews(BuildContext context) {
    context.go(RouteConstants.reviews);
  }

  static void goToHelp(BuildContext context) {
    context.go(RouteConstants.help);
  }

  static void goToAbout(BuildContext context) {
    context.go(RouteConstants.about);
  }

  // Search Navigation
  static void goToSearchResults(BuildContext context, String query) {
    context.go('/search/results?q=$query');
  }

  static void goToSearchHistory(BuildContext context) {
    context.go(RouteConstants.searchHistory);
  }

  static void goToSearchSuggestions(BuildContext context) {
    context.go(RouteConstants.searchSuggestions);
  }

  // Location Navigation
  static void goToLocationPicker(BuildContext context) {
    context.go(RouteConstants.locationPicker);
  }

  static void goToDeliveryZones(BuildContext context) {
    context.go(RouteConstants.deliveryZones);
  }

  // File Upload Navigation
  static void goToImagePicker(BuildContext context) {
    context.go(RouteConstants.imagePicker);
  }

  static void goToCamera(BuildContext context) {
    context.go(RouteConstants.camera);
  }

  // Admin Navigation
  static void goToMerchantDashboard(BuildContext context) {
    context.go(RouteConstants.merchantDashboard);
  }

  static void goToMerchantProducts(BuildContext context) {
    context.go(RouteConstants.merchantProducts);
  }

  static void goToMerchantOrders(BuildContext context) {
    context.go(RouteConstants.merchantOrders);
  }

  static void goToMerchantAnalytics(BuildContext context) {
    context.go(RouteConstants.merchantAnalytics);
  }

  // Error Navigation
  static void goToNotFound(BuildContext context) {
    context.go(RouteConstants.notFound);
  }

  static void goToMaintenance(BuildContext context) {
    context.go(RouteConstants.maintenance);
  }

  // Back Navigation
  static void goBack(BuildContext context) {
    context.pop();
  }

  // Push Navigation (for modals, sheets, etc.)
  static Future<T?> pushToMerchantDetail<T>(
    BuildContext context,
    String merchantId,
  ) {
    return Navigator.pushNamed(context, '/merchant/$merchantId');
  }

  static Future<T?> pushToProductDetail<T>(
    BuildContext context,
    String productId,
  ) {
    return Navigator.pushNamed(context, '/product/$productId');
  }

  static Future<T?> pushToOrderDetail<T>(BuildContext context, String orderId) {
    return Navigator.pushNamed(context, '/order/$orderId');
  }

  static Future<T?> pushToOrderTracking<T>(
    BuildContext context,
    String orderId,
  ) {
    return Navigator.pushNamed(context, '/order/$orderId/tracking');
  }

  static Future<T?> pushToCheckout<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.checkout);
  }

  static Future<T?> pushToPayment<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.payment);
  }

  static Future<T?> pushToAddresses<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.addresses);
  }

  static Future<T?> pushToAddAddress<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.addAddress);
  }

  static Future<T?> pushToEditAddress<T>(
    BuildContext context,
    String addressId,
  ) {
    return Navigator.pushNamed(context, '/addresses/edit/$addressId');
  }

  static Future<T?> pushToNotifications<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.notifications);
  }

  static Future<T?> pushToSettings<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.settings);
  }

  static Future<T?> pushToHelp<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.help);
  }

  static Future<T?> pushToAbout<T>(BuildContext context) {
    return Navigator.pushNamed(context, RouteConstants.about);
  }
}
