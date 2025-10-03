class RouteConstants {
  // Authentication Routes
  static const String splash = '/';
  static const String onboarding = '/onboarding';
  static const String login = '/login';
  static const String register = '/register';
  static const String forgotPassword = '/forgot-password';
  static const String resetPassword = '/reset-password';
  
  // Main App Routes
  static const String home = '/home';
  static const String search = '/search';
  static const String cart = '/cart';
  static const String orders = '/orders';
  static const String profile = '/profile';
  
  // Product Routes
  static const String merchant = '/merchant';
  static const String merchantDetail = '/merchant/:merchantId';
  static const String productDetail = '/product/:productId';
  static const String productCategory = '/category/:categoryId';
  
  // Order Routes
  static const String checkout = '/checkout';
  static const String orderDetail = '/order/:orderId';
  static const String orderTracking = '/order/:orderId/tracking';
  static const String payment = '/payment';
  static const String paymentSuccess = '/payment/success';
  static const String paymentFailed = '/payment/failed';
  
  // Profile Routes
  static const String editProfile = '/profile/edit';
  static const String addresses = '/addresses';
  static const String addAddress = '/addresses/add';
  static const String editAddress = '/addresses/edit/:addressId';
  static const String notifications = '/notifications';
  static const String notificationSettings = '/notifications/settings';
  static const String coupons = '/coupons';
  static const String reviews = '/reviews';
  static const String settings = '/settings';
  static const String help = '/help';
  static const String about = '/about';
  
  // Search Routes
  static const String searchResults = '/search/results';
  static const String searchHistory = '/search/history';
  static const String searchSuggestions = '/search/suggestions';
  
  // Location Routes
  static const String locationPicker = '/location/picker';
  static const String deliveryZones = '/delivery-zones';
  
  // File Upload Routes
  static const String imagePicker = '/image-picker';
  static const String camera = '/camera';
  
  // Admin Routes (for merchant/admin users)
  static const String merchantDashboard = '/merchant/dashboard';
  static const String merchantProducts = '/merchant/products';
  static const String merchantOrders = '/merchant/orders';
  static const String merchantAnalytics = '/merchant/analytics';
  
  // Error Routes
  static const String notFound = '/not-found';
  static const String maintenance = '/maintenance';
  
  // Deep Link Routes
  static const String deepLinkProduct = '/product';
  static const String deepLinkMerchant = '/merchant';
  static const String deepLinkCategory = '/category';
  static const String deepLinkOrder = '/order';
  
  // Route Names for GoRouter
  static const String splashRouteName = 'splash';
  static const String onboardingRouteName = 'onboarding';
  static const String loginRouteName = 'login';
  static const String registerRouteName = 'register';
  static const String forgotPasswordRouteName = 'forgot-password';
  static const String resetPasswordRouteName = 'reset-password';
  static const String homeRouteName = 'home';
  static const String searchRouteName = 'search';
  static const String cartRouteName = 'cart';
  static const String ordersRouteName = 'orders';
  static const String profileRouteName = 'profile';
  static const String merchantRouteName = 'merchant';
  static const String merchantDetailRouteName = 'merchant-detail';
  static const String productDetailRouteName = 'product-detail';
  static const String productCategoryRouteName = 'product-category';
  static const String checkoutRouteName = 'checkout';
  static const String orderDetailRouteName = 'order-detail';
  static const String orderTrackingRouteName = 'order-tracking';
  static const String paymentRouteName = 'payment';
  static const String paymentSuccessRouteName = 'payment-success';
  static const String paymentFailedRouteName = 'payment-failed';
  static const String editProfileRouteName = 'edit-profile';
  static const String addressesRouteName = 'addresses';
  static const String addAddressRouteName = 'add-address';
  static const String editAddressRouteName = 'edit-address';
  static const String notificationsRouteName = 'notifications';
  static const String notificationSettingsRouteName = 'notification-settings';
  static const String couponsRouteName = 'coupons';
  static const String reviewsRouteName = 'reviews';
  static const String settingsRouteName = 'settings';
  static const String helpRouteName = 'help';
  static const String aboutRouteName = 'about';
  static const String searchResultsRouteName = 'search-results';
  static const String searchHistoryRouteName = 'search-history';
  static const String searchSuggestionsRouteName = 'search-suggestions';
  static const String locationPickerRouteName = 'location-picker';
  static const String deliveryZonesRouteName = 'delivery-zones';
  static const String imagePickerRouteName = 'image-picker';
  static const String cameraRouteName = 'camera';
  static const String merchantDashboardRouteName = 'merchant-dashboard';
  static const String merchantProductsRouteName = 'merchant-products';
  static const String merchantOrdersRouteName = 'merchant-orders';
  static const String merchantAnalyticsRouteName = 'merchant-analytics';
  static const String notFoundRouteName = 'not-found';
  static const String maintenanceRouteName = 'maintenance';
}
