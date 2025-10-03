class ErrorConstants {
  // Network Errors
  static const String networkError = 'NETWORK_ERROR';
  static const String connectionTimeout = 'CONNECTION_TIMEOUT';
  static const String receiveTimeout = 'RECEIVE_TIMEOUT';
  static const String sendTimeout = 'SEND_TIMEOUT';
  static const String noInternetConnection = 'NO_INTERNET_CONNECTION';
  static const String serverError = 'SERVER_ERROR';
  static const String badRequest = 'BAD_REQUEST';
  static const String unauthorized = 'UNAUTHORIZED';
  static const String forbidden = 'FORBIDDEN';
  static const String notFound = 'NOT_FOUND';
  static const String conflict = 'CONFLICT';
  static const String unprocessableEntity = 'UNPROCESSABLE_ENTITY';
  static const String internalServerError = 'INTERNAL_SERVER_ERROR';
  
  // Authentication Errors
  static const String invalidCredentials = 'INVALID_CREDENTIALS';
  static const String tokenExpired = 'TOKEN_EXPIRED';
  static const String tokenInvalid = 'TOKEN_INVALID';
  static const String accountLocked = 'ACCOUNT_LOCKED';
  static const String accountNotVerified = 'ACCOUNT_NOT_VERIFIED';
  static const String emailAlreadyExists = 'EMAIL_ALREADY_EXISTS';
  static const String phoneAlreadyExists = 'PHONE_ALREADY_EXISTS';
  
  // Validation Errors
  static const String validationError = 'VALIDATION_ERROR';
  static const String requiredField = 'REQUIRED_FIELD';
  static const String invalidEmail = 'INVALID_EMAIL';
  static const String invalidPhone = 'INVALID_PHONE';
  static const String invalidPassword = 'INVALID_PASSWORD';
  static const String passwordTooShort = 'PASSWORD_TOO_SHORT';
  static const String passwordTooLong = 'PASSWORD_TOO_LONG';
  static const String passwordsDoNotMatch = 'PASSWORDS_DO_NOT_MATCH';
  
  // Location Errors
  static const String locationPermissionDenied = 'LOCATION_PERMISSION_DENIED';
  static const String locationServiceDisabled = 'LOCATION_SERVICE_DISABLED';
  static const String locationNotFound = 'LOCATION_NOT_FOUND';
  static const String locationTimeout = 'LOCATION_TIMEOUT';
  
  // File Upload Errors
  static const String fileTooLarge = 'FILE_TOO_LARGE';
  static const String invalidFileType = 'INVALID_FILE_TYPE';
  static const String uploadFailed = 'UPLOAD_FAILED';
  static const String fileNotFound = 'FILE_NOT_FOUND';
  
  // Order Errors
  static const String orderNotFound = 'ORDER_NOT_FOUND';
  static const String orderCancelled = 'ORDER_CANCELLED';
  static const String orderCompleted = 'ORDER_COMPLETED';
  static const String insufficientStock = 'INSUFFICIENT_STOCK';
  static const String merchantClosed = 'MERCHANT_CLOSED';
  static const String deliveryZoneNotAvailable = 'DELIVERY_ZONE_NOT_AVAILABLE';
  
  // Payment Errors
  static const String paymentFailed = 'PAYMENT_FAILED';
  static const String paymentCancelled = 'PAYMENT_CANCELLED';
  static const String insufficientFunds = 'INSUFFICIENT_FUNDS';
  static const String paymentMethodNotSupported = 'PAYMENT_METHOD_NOT_SUPPORTED';
  
  // Cart Errors
  static const String cartEmpty = 'CART_EMPTY';
  static const String cartItemNotFound = 'CART_ITEM_NOT_FOUND';
  static const String cartItemOutOfStock = 'CART_ITEM_OUT_OF_STOCK';
  
  // Coupon Errors
  static const String couponNotFound = 'COUPON_NOT_FOUND';
  static const String couponExpired = 'COUPON_EXPIRED';
  static const String couponAlreadyUsed = 'COUPON_ALREADY_USED';
  static const String couponMinimumAmountNotMet = 'COUPON_MINIMUM_AMOUNT_NOT_MET';
  
  // General Errors
  static const String unknownError = 'UNKNOWN_ERROR';
  static const String operationFailed = 'OPERATION_FAILED';
  static const String dataNotFound = 'DATA_NOT_FOUND';
  static const String permissionDenied = 'PERMISSION_DENIED';
  static const String featureNotAvailable = 'FEATURE_NOT_AVAILABLE';
  
  // Error Messages
  static const Map<String, String> errorMessages = {
    networkError: 'Ağ bağlantısı hatası. Lütfen internet bağlantınızı kontrol edin.',
    connectionTimeout: 'Bağlantı zaman aşımı. Lütfen tekrar deneyin.',
    receiveTimeout: 'Veri alma zaman aşımı. Lütfen tekrar deneyin.',
    sendTimeout: 'Veri gönderme zaman aşımı. Lütfen tekrar deneyin.',
    noInternetConnection: 'İnternet bağlantısı bulunamadı.',
    serverError: 'Sunucu hatası. Lütfen daha sonra tekrar deneyin.',
    badRequest: 'Geçersiz istek.',
    unauthorized: 'Yetkisiz erişim. Lütfen giriş yapın.',
    forbidden: 'Bu işlem için yetkiniz bulunmuyor.',
    notFound: 'Aradığınız veri bulunamadı.',
    conflict: 'Veri çakışması oluştu.',
    unprocessableEntity: 'İşlenemeyen veri.',
    internalServerError: 'Sunucu hatası. Lütfen daha sonra tekrar deneyin.',
    
    invalidCredentials: 'Geçersiz kullanıcı adı veya şifre.',
    tokenExpired: 'Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.',
    tokenInvalid: 'Geçersiz oturum. Lütfen tekrar giriş yapın.',
    accountLocked: 'Hesabınız kilitlenmiş. Lütfen destek ekibiyle iletişime geçin.',
    accountNotVerified: 'Hesabınız doğrulanmamış. Lütfen e-postanızı kontrol edin.',
    emailAlreadyExists: 'Bu e-posta adresi zaten kullanılıyor.',
    phoneAlreadyExists: 'Bu telefon numarası zaten kullanılıyor.',
    
    validationError: 'Doğrulama hatası.',
    requiredField: 'Bu alan zorunludur.',
    invalidEmail: 'Geçersiz e-posta adresi.',
    invalidPhone: 'Geçersiz telefon numarası.',
    invalidPassword: 'Geçersiz şifre.',
    passwordTooShort: 'Şifre çok kısa.',
    passwordTooLong: 'Şifre çok uzun.',
    passwordsDoNotMatch: 'Şifreler eşleşmiyor.',
    
    locationPermissionDenied: 'Konum izni reddedildi.',
    locationServiceDisabled: 'Konum servisleri kapalı.',
    locationNotFound: 'Konum bulunamadı.',
    locationTimeout: 'Konum alma zaman aşımı.',
    
    fileTooLarge: 'Dosya çok büyük.',
    invalidFileType: 'Geçersiz dosya türü.',
    uploadFailed: 'Dosya yükleme başarısız.',
    fileNotFound: 'Dosya bulunamadı.',
    
    orderNotFound: 'Sipariş bulunamadı.',
    orderCancelled: 'Sipariş iptal edilmiş.',
    orderCompleted: 'Sipariş tamamlanmış.',
    insufficientStock: 'Yetersiz stok.',
    merchantClosed: 'İşletme kapalı.',
    deliveryZoneNotAvailable: 'Bu bölgeye teslimat yapılmıyor.',
    
    paymentFailed: 'Ödeme başarısız.',
    paymentCancelled: 'Ödeme iptal edildi.',
    insufficientFunds: 'Yetersiz bakiye.',
    paymentMethodNotSupported: 'Bu ödeme yöntemi desteklenmiyor.',
    
    cartEmpty: 'Sepetiniz boş.',
    cartItemNotFound: 'Sepet öğesi bulunamadı.',
    cartItemOutOfStock: 'Bu ürün stokta yok.',
    
    couponNotFound: 'Kupon bulunamadı.',
    couponExpired: 'Kupon süresi dolmuş.',
    couponAlreadyUsed: 'Bu kupon zaten kullanılmış.',
    couponMinimumAmountNotMet: 'Minimum tutar şartı sağlanmadı.',
    
    unknownError: 'Bilinmeyen hata oluştu.',
    operationFailed: 'İşlem başarısız.',
    dataNotFound: 'Veri bulunamadı.',
    permissionDenied: 'İzin reddedildi.',
    featureNotAvailable: 'Bu özellik mevcut değil.',
  };
  
  static String getErrorMessage(String errorCode) {
    return errorMessages[errorCode] ?? errorMessages[unknownError]!;
  }
}
