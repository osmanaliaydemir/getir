class ApiEndpoints {
  // Authentication
  static const String login = '/auth/login';
  static const String register = '/auth/register';
  static const String refreshToken = '/auth/refresh';
  static const String logout = '/auth/logout';
  static const String forgotPassword = '/auth/forgot-password';
  static const String resetPassword = '/auth/reset-password';
  
  // User Management
  static const String userProfile = '/users/profile';
  static const String updateProfile = '/users/update';
  static const String changePassword = '/users/change-password';
  
  // Geo Location
  static const String nearbyMerchants = '/geo/merchants/nearby';
  static const String locationSuggestions = '/geo/suggestions';
  static const String deliveryEstimate = '/geo/delivery/estimate';
  static const String deliveryFee = '/geo/delivery/fee';
  static const String saveUserLocation = '/geo/location/save';
  static const String merchantsInArea = '/geo/merchants/in-area';
  
  // Merchants
  static const String merchants = '/merchants';
  static const String merchantDetails = '/merchants/{id}';
  static const String merchantProducts = '/merchants/{id}/products';
  static const String merchantReviews = '/merchants/{id}/reviews';
  
  // Products
  static const String products = '/products';
  static const String productDetails = '/products/{id}';
  static const String searchProducts = '/products/search';
  static const String productCategories = '/products/categories';
  
  // Orders
  static const String orders = '/orders';
  static const String orderDetails = '/orders/{id}';
  static const String orderHistory = '/orders/history';
  static const String createOrder = '/orders';
  static const String cancelOrder = '/orders/{id}/cancel';
  static const String trackOrder = '/orders/{id}/track';
  
  // Cart
  static const String cart = '/cart';
  static const String addToCart = '/cart/add';
  static const String updateCartItem = '/cart/update';
  static const String removeFromCart = '/cart/remove';
  static const String clearCart = '/cart/clear';
  
  // Payments
  static const String payments = '/payments';
  static const String createPayment = '/payments';
  static const String paymentStatus = '/payments/{id}/status';
  static const String orderPayments = '/payments/order/{orderId}';
  
  // Coupons
  static const String coupons = '/coupons';
  static const String validateCoupon = '/coupons/validate';
  static const String applyCoupon = '/coupons/apply';
  
  // Reviews
  static const String reviews = '/reviews';
  static const String createReview = '/reviews';
  static const String updateReview = '/reviews/{id}';
  static const String deleteReview = '/reviews/{id}';
  
  // Notifications
  static const String notifications = '/notifications';
  static const String notificationPreferences = '/notifications/preferences';
  static const String markAsRead = '/notifications/mark-as-read';
  static const String sendEmail = '/notifications/send/email';
  static const String sendSms = '/notifications/send/sms';
  static const String sendPush = '/notifications/send/push';
  
  // File Upload
  static const String uploadFile = '/files/upload';
  static const String uploadMultiple = '/files/upload-multiple';
  static const String uploadMerchantLogo = '/files/upload/merchant/logo';
  static const String uploadMerchantCover = '/files/upload/merchant/cover';
  static const String uploadProductImage = '/files/upload/product/image';
  static const String getFileUrl = '/files/{containerName}/{fileName}';
  static const String deleteFile = '/files/{containerName}/{fileName}';
  
  // Search
  static const String search = '/search';
  static const String searchMerchants = '/search/merchants';
  static const String searchSuggestions = '/search/suggestions';
  
  // Delivery Zones
  static const String deliveryZones = '/delivery-zones';
  static const String merchantDeliveryZones = '/delivery-zones/merchant/{merchantId}';
  static const String createDeliveryZone = '/delivery-zones';
  static const String updateDeliveryZone = '/delivery-zones/{id}';
  static const String deleteDeliveryZone = '/delivery-zones/{id}';
  
  // Addresses
  static const String addresses = '/addresses';
  static const String createAddress = '/addresses';
  static const String updateAddress = '/addresses/{id}';
  static const String deleteAddress = '/addresses/{id}';
  static const String setDefaultAddress = '/addresses/{id}/set-default';
}
