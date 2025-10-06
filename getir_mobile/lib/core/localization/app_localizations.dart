import 'package:flutter/material.dart';

class AppLocalizations {
  final Locale locale;

  AppLocalizations(this.locale);

  static AppLocalizations of(BuildContext context) {
    return Localizations.of<AppLocalizations>(context, AppLocalizations)!;
  }

  static const LocalizationsDelegate<AppLocalizations> delegate =
      _AppLocalizationsDelegate();

  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates = [
    delegate,
  ];

  static const List<Locale> supportedLocales = [
    Locale('tr', 'TR'), // Türkçe (Default)
    Locale('en', 'US'), // İngilizce
    Locale('ar', 'SA'), // Arapça
  ];

  // Auth Screen Texts
  String get welcome => _localizedValues[locale.languageCode]!['welcome']!;
  String get login => _localizedValues[locale.languageCode]!['login']!;
  String get register => _localizedValues[locale.languageCode]!['register']!;
  String get email => _localizedValues[locale.languageCode]!['email']!;
  String get password => _localizedValues[locale.languageCode]!['password']!;
  String get confirmPassword =>
      _localizedValues[locale.languageCode]!['confirmPassword']!;
  String get firstName => _localizedValues[locale.languageCode]!['firstName']!;
  String get lastName => _localizedValues[locale.languageCode]!['lastName']!;
  String get phoneNumber =>
      _localizedValues[locale.languageCode]!['phoneNumber']!;
  String get forgotPassword =>
      _localizedValues[locale.languageCode]!['forgotPassword']!;
  String get dontHaveAccount =>
      _localizedValues[locale.languageCode]!['dontHaveAccount']!;
  String get alreadyHaveAccount =>
      _localizedValues[locale.languageCode]!['alreadyHaveAccount']!;
  String get signUp => _localizedValues[locale.languageCode]!['signUp']!;
  String get signIn => _localizedValues[locale.languageCode]!['signIn']!;
  String get orContinueWith =>
      _localizedValues[locale.languageCode]!['orContinueWith']!;
  String get termsAndConditions =>
      _localizedValues[locale.languageCode]!['termsAndConditions']!;
  String get privacyPolicy =>
      _localizedValues[locale.languageCode]!['privacyPolicy']!;
  String get iAgreeTo => _localizedValues[locale.languageCode]!['iAgreeTo']!;
  String get and => _localizedValues[locale.languageCode]!['and']!;

  // Home Screen Texts
  String get home => _localizedValues[locale.languageCode]!['home']!;
  String get search => _localizedValues[locale.languageCode]!['search']!;
  String get orders => _localizedValues[locale.languageCode]!['orders']!;
  String get profile => _localizedValues[locale.languageCode]!['profile']!;
  String get categories =>
      _localizedValues[locale.languageCode]!['categories']!;
  String get popularProducts =>
      _localizedValues[locale.languageCode]!['popularProducts']!;
  String get nearbyMerchants =>
      _localizedValues[locale.languageCode]!['nearbyMerchants']!;
  String get deliveryTime =>
      _localizedValues[locale.languageCode]!['deliveryTime']!;
  String get minutes => _localizedValues[locale.languageCode]!['minutes']!;
  String get addToCart => _localizedValues[locale.languageCode]!['addToCart']!;
  String get viewAll => _localizedValues[locale.languageCode]!['viewAll']!;
  String get searchHint =>
      _localizedValues[locale.languageCode]!['searchHint']!;
  String get selectLocation =>
      _localizedValues[locale.languageCode]!['selectLocation']!;
  String get changeLanguage =>
      _localizedValues[locale.languageCode]!['changeLanguage']!;

  // Common Texts
  String get loading => _localizedValues[locale.languageCode]!['loading']!;
  String get error => _localizedValues[locale.languageCode]!['error']!;
  String get success => _localizedValues[locale.languageCode]!['success']!;
  String get cancel => _localizedValues[locale.languageCode]!['cancel']!;
  String get ok => _localizedValues[locale.languageCode]!['ok']!;
  String get retry => _localizedValues[locale.languageCode]!['retry']!;
  String get save => _localizedValues[locale.languageCode]!['save']!;
  String get delete => _localizedValues[locale.languageCode]!['delete']!;
  String get edit => _localizedValues[locale.languageCode]!['edit']!;
  String get back => _localizedValues[locale.languageCode]!['back']!;
  String get next => _localizedValues[locale.languageCode]!['next']!;
  String get previous => _localizedValues[locale.languageCode]!['previous']!;
  String get done => _localizedValues[locale.languageCode]!['done']!;

  // Error Messages
  String get emailRequired =>
      _localizedValues[locale.languageCode]!['emailRequired']!;
  String get passwordRequired =>
      _localizedValues[locale.languageCode]!['passwordRequired']!;
  String get invalidEmail =>
      _localizedValues[locale.languageCode]!['invalidEmail']!;
  String get passwordTooShort =>
      _localizedValues[locale.languageCode]!['passwordTooShort']!;
  String get passwordsDoNotMatch =>
      _localizedValues[locale.languageCode]!['passwordsDoNotMatch']!;
  String get firstNameRequired =>
      _localizedValues[locale.languageCode]!['firstNameRequired']!;
  String get lastNameRequired =>
      _localizedValues[locale.languageCode]!['lastNameRequired']!;
  String get phoneNumberRequired =>
      _localizedValues[locale.languageCode]!['phoneNumberRequired']!;
  String get networkError =>
      _localizedValues[locale.languageCode]!['networkError']!;
  String get serverError =>
      _localizedValues[locale.languageCode]!['serverError']!;
  String get unknownError =>
      _localizedValues[locale.languageCode]!['unknownError']!;
  String get somethingWentWrong =>
      _localizedValues[locale.languageCode]!['somethingWentWrong']!;
  String get failed => _localizedValues[locale.languageCode]!['failed']!;
  String get dismiss => _localizedValues[locale.languageCode]!['dismiss']!;

  // Network Errors
  String get noInternetConnection =>
      _localizedValues[locale.languageCode]!['noInternetConnection']!;
  String get checkInternetConnection =>
      _localizedValues[locale.languageCode]!['checkInternetConnection']!;
  String get requestTimeout =>
      _localizedValues[locale.languageCode]!['requestTimeout']!;

  // API Errors
  String get unauthorizedAccess =>
      _localizedValues[locale.languageCode]!['unauthorizedAccess']!;
  String get accessForbidden =>
      _localizedValues[locale.languageCode]!['accessForbidden']!;
  String get resourceNotFound =>
      _localizedValues[locale.languageCode]!['resourceNotFound']!;
  String get validationError =>
      _localizedValues[locale.languageCode]!['validationError']!;

  // Business Errors
  String get insufficientFunds =>
      _localizedValues[locale.languageCode]!['insufficientFunds']!;
  String get productUnavailable =>
      _localizedValues[locale.languageCode]!['productUnavailable']!;
  String get orderLimitExceeded =>
      _localizedValues[locale.languageCode]!['orderLimitExceeded']!;

  // Storage Errors
  String get storageError =>
      _localizedValues[locale.languageCode]!['storageError']!;
  String get cacheError =>
      _localizedValues[locale.languageCode]!['cacheError']!;

  // Cart
  String get cart => _localizedValues[locale.languageCode]!['cart']!;
  String get emptyCart => _localizedValues[locale.languageCode]!['emptyCart']!;
  String get emptyCartMessage =>
      _localizedValues[locale.languageCode]!['emptyCartMessage']!;
  String get startShopping =>
      _localizedValues[locale.languageCode]!['startShopping']!;
  String get clearCart => _localizedValues[locale.languageCode]!['clearCart']!;
  String get clearCartMessage =>
      _localizedValues[locale.languageCode]!['clearCartMessage']!;
  String get removeItem =>
      _localizedValues[locale.languageCode]!['removeItem']!;
  String get removeItemMessage =>
      _localizedValues[locale.languageCode]!['removeItemMessage']!;
  String get couponCode =>
      _localizedValues[locale.languageCode]!['couponCode']!;
  String get couponApplied =>
      _localizedValues[locale.languageCode]!['couponApplied']!;
  String get subtotal => _localizedValues[locale.languageCode]!['subtotal']!;
  String get deliveryFee =>
      _localizedValues[locale.languageCode]!['deliveryFee']!;
  String get discount => _localizedValues[locale.languageCode]!['discount']!;
  String get total => _localizedValues[locale.languageCode]!['total']!;
  String get proceedToCheckout =>
      _localizedValues[locale.languageCode]!['proceedToCheckout']!;
  String get checkoutComingSoon =>
      _localizedValues[locale.languageCode]!['checkoutComingSoon']!;
  String get addedToCart =>
      _localizedValues[locale.languageCode]!['addedToCart']!;
  String get remove => _localizedValues[locale.languageCode]!['remove']!;
  String get apply => _localizedValues[locale.languageCode]!['apply']!;

  // Product
  String get variants => _localizedValues[locale.languageCode]!['variants']!;
  String get required => _localizedValues[locale.languageCode]!['required']!;
  String get nutritionalInfo =>
      _localizedValues[locale.languageCode]!['nutritionalInfo']!;
  String get outOfStock =>
      _localizedValues[locale.languageCode]!['outOfStock']!;
  String get inStock => _localizedValues[locale.languageCode]!['inStock']!;
  String get searchProducts =>
      _localizedValues[locale.languageCode]!['searchProducts']!;
  String get noProductsFound =>
      _localizedValues[locale.languageCode]!['noProductsFound']!;
  String get reviews => _localizedValues[locale.languageCode]!['reviews']!;

  // Merchant
  String get noMerchantsFound =>
      _localizedValues[locale.languageCode]!['noMerchantsFound']!;
  String get workingHours =>
      _localizedValues[locale.languageCode]!['workingHours']!;
  String get contactInfo =>
      _localizedValues[locale.languageCode]!['contactInfo']!;
  String get address => _localizedValues[locale.languageCode]!['address']!;
  String get phone => _localizedValues[locale.languageCode]!['phone']!;
  String get deliveryInfo =>
      _localizedValues[locale.languageCode]!['deliveryInfo']!;
  String get estimatedDeliveryTime =>
      _localizedValues[locale.languageCode]!['estimatedDeliveryTime']!;
  String get minimumOrderAmount =>
      _localizedValues[locale.languageCode]!['minimumOrderAmount']!;
  String get open => _localizedValues[locale.languageCode]!['open']!;
  String get closed => _localizedValues[locale.languageCode]!['closed']!;

  // Address
  String get addresses => _localizedValues[locale.languageCode]!['addresses']!;
  String get addAddress =>
      _localizedValues[locale.languageCode]!['addAddress']!;
  String get editAddress =>
      _localizedValues[locale.languageCode]!['editAddress']!;
  String get updateAddress =>
      _localizedValues[locale.languageCode]!['updateAddress']!;
  String get deleteAddress =>
      _localizedValues[locale.languageCode]!['deleteAddress']!;
  String get deleteAddressMessage =>
      _localizedValues[locale.languageCode]!['deleteAddressMessage']!;
  String get noAddressesFound =>
      _localizedValues[locale.languageCode]!['noAddressesFound']!;
  String get noAddressesMessage =>
      _localizedValues[locale.languageCode]!['noAddressesMessage']!;
  String get defaultAddress =>
      _localizedValues[locale.languageCode]!['defaultAddress']!;
  String get setAsDefault =>
      _localizedValues[locale.languageCode]!['setAsDefault']!;
  String get setAsDefaultMessage =>
      _localizedValues[locale.languageCode]!['setAsDefaultMessage']!;
  String get addressType =>
      _localizedValues[locale.languageCode]!['addressType']!;
  String get addressTitle =>
      _localizedValues[locale.languageCode]!['addressTitle']!;
  String get addressTitleHint =>
      _localizedValues[locale.languageCode]!['addressTitleHint']!;
  String get addressTitleRequired =>
      _localizedValues[locale.languageCode]!['addressTitleRequired']!;
  String get fullAddress =>
      _localizedValues[locale.languageCode]!['fullAddress']!;
  String get fullAddressHint =>
      _localizedValues[locale.languageCode]!['fullAddressHint']!;
  String get fullAddressRequired =>
      _localizedValues[locale.languageCode]!['fullAddressRequired']!;
  String get buildingNumber =>
      _localizedValues[locale.languageCode]!['buildingNumber']!;
  String get floor => _localizedValues[locale.languageCode]!['floor']!;
  String get apartment => _localizedValues[locale.languageCode]!['apartment']!;
  String get landmark => _localizedValues[locale.languageCode]!['landmark']!;
  String get landmarkHint =>
      _localizedValues[locale.languageCode]!['landmarkHint']!;
  String get getCurrentLocation =>
      _localizedValues[locale.languageCode]!['getCurrentLocation']!;
  String get gettingLocation =>
      _localizedValues[locale.languageCode]!['gettingLocation']!;
  String get locationSet =>
      _localizedValues[locale.languageCode]!['locationSet']!;
  String get locationRequired =>
      _localizedValues[locale.languageCode]!['locationRequired']!;
  String get locationError =>
      _localizedValues[locale.languageCode]!['locationError']!;
  String get locationPermissionDenied =>
      _localizedValues[locale.languageCode]!['locationPermissionDenied']!;
  String get locationPermissionDeniedForever =>
      _localizedValues[locale
          .languageCode]!['locationPermissionDeniedForever']!;

  // Checkout & Order
  String get checkout => _localizedValues[locale.languageCode]!['checkout']!;
  String get deliveryAddress =>
      _localizedValues[locale.languageCode]!['deliveryAddress']!;
  String get paymentMethod =>
      _localizedValues[locale.languageCode]!['paymentMethod']!;
  String get orderNotes =>
      _localizedValues[locale.languageCode]!['orderNotes']!;
  String get orderNotesHint =>
      _localizedValues[locale.languageCode]!['orderNotesHint']!;
  String get orderSummary =>
      _localizedValues[locale.languageCode]!['orderSummary']!;
  String get placeOrder =>
      _localizedValues[locale.languageCode]!['placeOrder']!;
  String get changeAddress =>
      _localizedValues[locale.languageCode]!['changeAddress']!;
  String get selectAddress =>
      _localizedValues[locale.languageCode]!['selectAddress']!;
  String get cashPaymentDescription =>
      _localizedValues[locale.languageCode]!['cashPaymentDescription']!;
  String get cardPaymentDescription =>
      _localizedValues[locale.languageCode]!['cardPaymentDescription']!;
  String get onlinePaymentDescription =>
      _localizedValues[locale.languageCode]!['onlinePaymentDescription']!;
  String get orderConfirmation =>
      _localizedValues[locale.languageCode]!['orderConfirmation']!;
  String get orderConfirmed =>
      _localizedValues[locale.languageCode]!['orderConfirmed']!;
  String get orderNumber =>
      _localizedValues[locale.languageCode]!['orderNumber']!;
  String get orderItems =>
      _localizedValues[locale.languageCode]!['orderItems']!;
  String get trackOrder =>
      _localizedValues[locale.languageCode]!['trackOrder']!;
  String get continueShopping =>
      _localizedValues[locale.languageCode]!['continueShopping']!;
  String get orderStatusInfo =>
      _localizedValues[locale.languageCode]!['orderStatusInfo']!;

  // Order History & Tracking
  String get orderDetails =>
      _localizedValues[locale.languageCode]!['orderDetails']!;
  String get orderInformation =>
      _localizedValues[locale.languageCode]!['orderInformation']!;
  String get orderDate => _localizedValues[locale.languageCode]!['orderDate']!;
  String get estimatedDelivery =>
      _localizedValues[locale.languageCode]!['estimatedDelivery']!;
  String get viewDetails =>
      _localizedValues[locale.languageCode]!['viewDetails']!;
  String get cancelOrder =>
      _localizedValues[locale.languageCode]!['cancelOrder']!;
  String get cancelOrderMessage =>
      _localizedValues[locale.languageCode]!['cancelOrderMessage']!;
  String get noOrdersFound =>
      _localizedValues[locale.languageCode]!['noOrdersFound']!;
  String get noOrdersMessage =>
      _localizedValues[locale.languageCode]!['noOrdersMessage']!;
  String get noActiveOrders =>
      _localizedValues[locale.languageCode]!['noActiveOrders']!;
  String get noActiveOrdersMessage =>
      _localizedValues[locale.languageCode]!['noActiveOrdersMessage']!;
  String get noCompletedOrders =>
      _localizedValues[locale.languageCode]!['noCompletedOrders']!;
  String get noCompletedOrdersMessage =>
      _localizedValues[locale.languageCode]!['noCompletedOrdersMessage']!;
  String get trackingProgress =>
      _localizedValues[locale.languageCode]!['trackingProgress']!;

  // Settings
  String get settings => _localizedValues[locale.languageCode]!['settings']!;
  String get account => _localizedValues[locale.languageCode]!['account']!;
  String get manageYourProfile =>
      _localizedValues[locale.languageCode]!['manageYourProfile']!;
  String get manageYourAddresses =>
      _localizedValues[locale.languageCode]!['manageYourAddresses']!;
  String get orderHistory =>
      _localizedValues[locale.languageCode]!['orderHistory']!;
  String get viewYourOrders =>
      _localizedValues[locale.languageCode]!['viewYourOrders']!;
  String get notificationSettings =>
      _localizedValues[locale.languageCode]!['notificationSettings']!;
  String get customizeNotifications =>
      _localizedValues[locale.languageCode]!['customizeNotifications']!;
  String get appSettings =>
      _localizedValues[locale.languageCode]!['appSettings']!;
  String get language => _localizedValues[locale.languageCode]!['language']!;
  String get selectLanguage =>
      _localizedValues[locale.languageCode]!['selectLanguage']!;
  String get theme => _localizedValues[locale.languageCode]!['theme']!;
  String get systemTheme =>
      _localizedValues[locale.languageCode]!['systemTheme']!;
  String get lightTheme =>
      _localizedValues[locale.languageCode]!['lightTheme']!;
  String get darkTheme => _localizedValues[locale.languageCode]!['darkTheme']!;
  String get selectTheme =>
      _localizedValues[locale.languageCode]!['selectTheme']!;
  String get support => _localizedValues[locale.languageCode]!['support']!;
  String get helpCenter =>
      _localizedValues[locale.languageCode]!['helpCenter']!;
  String get getHelpAndSupport =>
      _localizedValues[locale.languageCode]!['getHelpAndSupport']!;
  String get contactUs => _localizedValues[locale.languageCode]!['contactUs']!;
  String get getInTouch =>
      _localizedValues[locale.languageCode]!['getInTouch']!;
  String get about => _localizedValues[locale.languageCode]!['about']!;
  String get appVersion =>
      _localizedValues[locale.languageCode]!['appVersion']!;
  String get logout => _localizedValues[locale.languageCode]!['logout']!;
  String get logoutConfirmation =>
      _localizedValues[locale.languageCode]!['logoutConfirmation']!;
  String get comingSoon =>
      _localizedValues[locale.languageCode]!['comingSoon']!;
  String get aboutDescription =>
      _localizedValues[locale.languageCode]!['aboutDescription']!;
  String get offlineMessage =>
      _localizedValues[locale.languageCode]!['offlineMessage']!;
  String get orderReceived =>
      _localizedValues[locale.languageCode]!['orderReceived']!;
  String get orderReceivedDescription =>
      _localizedValues[locale.languageCode]!['orderReceivedDescription']!;

  String get orderConfirmedDescription =>
      _localizedValues[locale.languageCode]!['orderConfirmedDescription']!;
  String get orderPreparing =>
      _localizedValues[locale.languageCode]!['orderPreparing']!;
  String get orderPreparingDescription =>
      _localizedValues[locale.languageCode]!['orderPreparingDescription']!;
  String get orderOnTheWay =>
      _localizedValues[locale.languageCode]!['orderOnTheWay']!;
  String get orderOnTheWayDescription =>
      _localizedValues[locale.languageCode]!['orderOnTheWayDescription']!;
  String get orderDelivered =>
      _localizedValues[locale.languageCode]!['orderDelivered']!;
  String get orderDeliveredDescription =>
      _localizedValues[locale.languageCode]!['orderDeliveredDescription']!;
  String get needHelp => _localizedValues[locale.languageCode]!['needHelp']!;
  String get callMerchant =>
      _localizedValues[locale.languageCode]!['callMerchant']!;
  String get contactSupport =>
      _localizedValues[locale.languageCode]!['contactSupport']!;
  String get paymentPending =>
      _localizedValues[locale.languageCode]!['paymentPending']!;
  String get paymentCompleted =>
      _localizedValues[locale.languageCode]!['paymentCompleted']!;
  String get paymentFailed =>
      _localizedValues[locale.languageCode]!['paymentFailed']!;
  String get pendingDescription =>
      _localizedValues[locale.languageCode]!['pendingDescription']!;
  String get confirmedDescription =>
      _localizedValues[locale.languageCode]!['confirmedDescription']!;
  String get preparingDescription =>
      _localizedValues[locale.languageCode]!['preparingDescription']!;
  String get onTheWayDescription =>
      _localizedValues[locale.languageCode]!['onTheWayDescription']!;
  String get deliveredDescription =>
      _localizedValues[locale.languageCode]!['deliveredDescription']!;
  String get cancelledDescription =>
      _localizedValues[locale.languageCode]!['cancelledDescription']!;
  String get unknownDescription =>
      _localizedValues[locale.languageCode]!['unknownDescription']!;

  // Order Status
  String get all => _localizedValues[locale.languageCode]!['all']!;
  String get active => _localizedValues[locale.languageCode]!['active']!;
  String get completed => _localizedValues[locale.languageCode]!['completed']!;
  String get pending => _localizedValues[locale.languageCode]!['pending']!;
  String get confirmed => _localizedValues[locale.languageCode]!['confirmed']!;
  String get preparing => _localizedValues[locale.languageCode]!['preparing']!;
  String get onTheWay => _localizedValues[locale.languageCode]!['onTheWay']!;
  String get delivered => _localizedValues[locale.languageCode]!['delivered']!;
  String get cancelled => _localizedValues[locale.languageCode]!['cancelled']!;
  String get unknown => _localizedValues[locale.languageCode]!['unknown']!;
  String get items => _localizedValues[locale.languageCode]!['items']!;

  // Notifications
  String get notifications =>
      _localizedValues[locale.languageCode]!['notifications']!;
  String get orderUpdates =>
      _localizedValues[locale.languageCode]!['orderUpdates']!;
  String get promotions =>
      _localizedValues[locale.languageCode]!['promotions']!;
  String get systemNotifications =>
      _localizedValues[locale.languageCode]!['systemNotifications']!;
  String get marketing => _localizedValues[locale.languageCode]!['marketing']!;

  static final Map<String, Map<String, String>> _localizedValues = {
    'tr': {
      // Auth Screen Texts
      'welcome': 'Hoş Geldiniz',
      'login': 'Giriş Yap',
      'register': 'Kayıt Ol',
      'email': 'E-posta',
      'password': 'Şifre',
      'confirmPassword': 'Şifreyi Onayla',
      'firstName': 'Ad',
      'lastName': 'Soyad',
      'phoneNumber': 'Telefon Numarası',
      'forgotPassword': 'Şifremi Unuttum',
      'dontHaveAccount': 'Hesabınız yok mu?',
      'alreadyHaveAccount': 'Zaten hesabınız var mı?',
      'signUp': 'Kayıt Ol',
      'signIn': 'Giriş Yap',
      'orContinueWith': 'veya ile devam et',
      'termsAndConditions': 'Kullanım Şartları',
      'privacyPolicy': 'Gizlilik Politikası',
      'iAgreeTo': 'Kabul ediyorum',
      'and': 've',

      // Home Screen Texts
      'home': 'Ana Sayfa',
      'search': 'Ara',
      'orders': 'Siparişlerim',
      'profile': 'Profil',
      'categories': 'Kategoriler',
      'popularProducts': 'Popüler Ürünler',
      'nearbyMerchants': 'Yakındaki İşletmeler',
      'deliveryTime': 'Teslimat Süresi',
      'minutes': 'dakika',
      'addToCart': 'Sepete Ekle',
      'viewAll': 'Tümünü Gör',
      'searchHint': 'Ürün, işletme ara...',
      'selectLocation': 'Konum Seç',
      'changeLanguage': 'Dil Değiştir',

      // Common Texts
      'loading': 'Yükleniyor...',
      'error': 'Hata',
      'success': 'Başarılı',
      'cancel': 'İptal',
      'ok': 'Tamam',
      'retry': 'Tekrar Dene',
      'save': 'Kaydet',
      'delete': 'Sil',
      'edit': 'Düzenle',
      'back': 'Geri',
      'next': 'İleri',
      'previous': 'Önceki',
      'done': 'Tamam',

      // Error Messages
      'emailRequired': 'E-posta adresi gereklidir',
      'passwordRequired': 'Şifre gereklidir',
      'invalidEmail': 'Geçerli bir e-posta adresi giriniz',
      'passwordTooShort': 'Şifre en az 6 karakter olmalıdır',
      'passwordsDoNotMatch': 'Şifreler eşleşmiyor',
      'firstNameRequired': 'Ad gereklidir',
      'lastNameRequired': 'Soyad gereklidir',
      'phoneNumberRequired': 'Telefon numarası gereklidir',
      'networkError': 'Ağ bağlantısı hatası',
      'serverError': 'Sunucu hatası',
      'unknownError': 'Bilinmeyen hata',
      'somethingWentWrong': 'Bir şeyler ters gitti',
      'error': 'Hata',
      'retry': 'Tekrar Dene',
      'cancel': 'İptal',
      'ok': 'Tamam',
      'success': 'Başarılı',
      'failed': 'Başarısız',
      'dismiss': 'Kapat',

      // Network Errors
      'noInternetConnection': 'İnternet bağlantısı yok',
      'checkInternetConnection': 'İnternet bağlantınızı kontrol edin',
      'requestTimeout': 'İstek zaman aşımına uğradı',

      // API Errors
      'unauthorizedAccess': 'Yetkisiz erişim',
      'accessForbidden': 'Erişim yasak',
      'resourceNotFound': 'Kaynak bulunamadı',
      'validationError': 'Doğrulama hatası',

      // Business Errors
      'insufficientFunds': 'Yetersiz bakiye',
      'productUnavailable': 'Ürün mevcut değil',
      'orderLimitExceeded': 'Sipariş limiti aşıldı',

      // Storage Errors
      'storageError': 'Depolama hatası',
      'cacheError': 'Önbellek hatası',

      // Cart
      'cart': 'Sepet',
      'emptyCart': 'Sepetiniz Boş',
      'emptyCartMessage':
          'Sepetinizde henüz ürün bulunmuyor. Alışverişe başlamak için ürünleri keşfedin.',
      'startShopping': 'Alışverişe Başla',
      'clearCart': 'Sepeti Temizle',
      'clearCartMessage':
          'Sepetinizdeki tüm ürünleri kaldırmak istediğinizden emin misiniz?',
      'removeItem': 'Ürünü Kaldır',
      'removeItemMessage':
          'Bu ürünü sepetinizden kaldırmak istediğinizden emin misiniz?',
      'couponCode': 'Kupon Kodu',
      'couponApplied': 'Kupon Uygulandı',
      'subtotal': 'Ara Toplam',
      'deliveryFee': 'Teslimat Ücreti',
      'discount': 'İndirim',
      'total': 'Toplam',
      'proceedToCheckout': 'Siparişi Tamamla',
      'checkoutComingSoon': 'Sipariş tamamlama özelliği yakında gelecek!',
      'addedToCart': 'Sepete eklendi',
      'remove': 'Kaldır',
      'apply': 'Uygula',

      // Product
      'variants': 'Varyantlar',
      'required': 'Zorunlu',
      'nutritionalInfo': 'Besin Değerleri',
      'outOfStock': 'Stokta Yok',
      'inStock': 'Stokta Var',
      'searchProducts': 'Ürün Ara',
      'noProductsFound': 'Ürün bulunamadı',
      'reviews': 'Yorumlar',

      // Merchant
      'noMerchantsFound': 'İşletme bulunamadı',
      'workingHours': 'Çalışma Saatleri',
      'contactInfo': 'İletişim Bilgileri',
      'address': 'Adres',
      'phone': 'Telefon',
      'deliveryInfo': 'Teslimat Bilgileri',
      'estimatedDeliveryTime': 'Tahmini Teslimat Süresi',
      'minimumOrderAmount': 'Minimum Sipariş Tutarı',
      'open': 'Açık',
      'closed': 'Kapalı',

      // Address
      'addresses': 'Adreslerim',
      'addAddress': 'Adres Ekle',
      'editAddress': 'Adres Düzenle',
      'updateAddress': 'Adres Güncelle',
      'deleteAddress': 'Adres Sil',
      'deleteAddressMessage': 'Bu adresi silmek istediğinizden emin misiniz?',
      'noAddressesFound': 'Adres Bulunamadı',
      'noAddressesMessage':
          'Henüz kayıtlı adresiniz bulunmuyor. Yeni adres ekleyerek sipariş verebilirsiniz.',
      'defaultAddress': 'Varsayılan',
      'setAsDefault': 'Varsayılan Yap',
      'setAsDefaultMessage': 'Bu adresi varsayılan adres olarak ayarla',
      'addressType': 'Adres Türü',
      'addressTitle': 'Adres Başlığı',
      'addressTitleHint': 'Örn: Ev, İş, Anne Evi',
      'addressTitleRequired': 'Adres başlığı gereklidir',
      'fullAddress': 'Tam Adres',
      'fullAddressHint': 'Mahalle, sokak, cadde bilgilerini yazın',
      'fullAddressRequired': 'Tam adres gereklidir',
      'buildingNumber': 'Bina No',
      'floor': 'Kat',
      'apartment': 'Daire',
      'landmark': 'Yakın Yer',
      'landmarkHint': 'Örn: Metro çıkışı, AVM karşısı',
      'getCurrentLocation': 'Mevcut Konumu Al',
      'gettingLocation': 'Konum alınıyor...',
      'locationSet': 'Konum ayarlandı',
      'locationRequired': 'Konum bilgisi gereklidir',
      'locationError': 'Konum alınamadı',
      'locationPermissionDenied': 'Konum izni reddedildi',
      'locationPermissionDeniedForever': 'Konum izni kalıcı olarak reddedildi',

      // Checkout & Order
      'checkout': 'Sipariş Tamamla',
      'deliveryAddress': 'Teslimat Adresi',
      'paymentMethod': 'Ödeme Yöntemi',
      'orderNotes': 'Sipariş Notları',
      'orderNotesHint':
          'Siparişinizle ilgili özel notlarınızı yazabilirsiniz...',
      'orderSummary': 'Sipariş Özeti',
      'placeOrder': 'Siparişi Ver',
      'changeAddress': 'Adresi Değiştir',
      'selectAddress': 'Adres Seç',
      'cashPaymentDescription': 'Kapıda nakit ödeme',
      'cardPaymentDescription': 'Kapıda kart ile ödeme',
      'onlinePaymentDescription': 'Online ödeme',
      'orderConfirmation': 'Sipariş Onayı',
      'orderConfirmed': 'Siparişiniz Onaylandı!',
      'orderNumber': 'Sipariş No',
      'orderItems': 'Sipariş Ürünleri',
      'trackOrder': 'Siparişi Takip Et',
      'continueShopping': 'Alışverişe Devam Et',
      'orderStatusInfo':
          'Sipariş durumunuzu takip edebilir ve güncellemeleri alabilirsiniz.',

      // Order Status
      'all': 'Tümü',
      'active': 'Aktif',
      'completed': 'Tamamlanan',
      'pending': 'Beklemede',
      'confirmed': 'Onaylandı',
      'preparing': 'Hazırlanıyor',
      'onTheWay': 'Yolda',
      'delivered': 'Teslim Edildi',
      'cancelled': 'İptal Edildi',
      'unknown': 'Bilinmiyor',
      'items': 'ürün',
      'notifications': 'Bildirimler',
      'orderUpdates': 'Sipariş Güncellemeleri',
      'promotions': 'Kampanyalar',
      'systemNotifications': 'Sistem Bildirimleri',
      'marketing': 'Pazarlama',

      // Order History & Tracking
      'orderDetails': 'Sipariş Detayları',
      'orderInformation': 'Sipariş Bilgileri',
      'orderDate': 'Sipariş Tarihi',
      'estimatedDelivery': 'Tahmini Teslimat',
      'viewDetails': 'Detayları Gör',
      'cancelOrder': 'Siparişi İptal Et',
      'cancelOrderMessage':
          'Bu siparişi iptal etmek istediğinizden emin misiniz?',
      'noOrdersFound': 'Sipariş Bulunamadı',
      'noOrdersMessage': 'Henüz sipariş vermediniz.',
      'noActiveOrders': 'Aktif Sipariş Yok',
      'noActiveOrdersMessage': 'Aktif siparişiniz bulunmuyor.',
      'noCompletedOrders': 'Tamamlanan Sipariş Yok',
      'noCompletedOrdersMessage': 'Tamamlanan siparişiniz bulunmuyor.',
      'trackingProgress': 'Takip Durumu',

      // Settings
      'settings': 'Ayarlar',
      'account': 'Hesap',
      'manageYourProfile': 'Profilinizi yönetin',
      'manageYourAddresses': 'Adreslerinizi yönetin',
      'orderHistory': 'Sipariş Geçmişi',
      'viewYourOrders': 'Siparişlerinizi görüntüleyin',
      'notificationSettings': 'Bildirim Ayarları',
      'customizeNotifications': 'Bildirimleri özelleştirin',
      'appSettings': 'Uygulama Ayarları',
      'language': 'Dil',
      'selectLanguage': 'Dil Seçin',
      'theme': 'Tema',
      'systemTheme': 'Sistem Teması',
      'lightTheme': 'Açık Tema',
      'darkTheme': 'Koyu Tema',
      'selectTheme': 'Tema Seçin',
      'support': 'Destek',
      'helpCenter': 'Yardım Merkezi',
      'getHelpAndSupport': 'Yardım ve destek alın',
      'contactUs': 'İletişim',
      'getInTouch': 'İletişime geçin',
      'about': 'Hakkında',
      'appVersion': 'Uygulama Sürümü',
      'logout': 'Çıkış Yap',
      'logoutConfirmation': 'Çıkış yapmak istediğinizden emin misiniz?',
      'comingSoon': 'Yakında Gelecek',
      'aboutDescription': 'Getir Mobile - Hızlı teslimat uygulaması',
      'offlineMessage':
          'İnternet bağlantınız yok. Lütfen bağlantınızı kontrol edin.',

      // Order Tracking
      'orderReceived': 'Sipariş Alındı',
      'orderReceivedDescription': 'Siparişiniz alındı ve işleme alındı',
      'orderConfirmed': 'Sipariş Onaylandı',
      'orderConfirmedDescription':
          'Siparişiniz onaylandı ve hazırlanmaya başlandı',
      'orderPreparing': 'Sipariş Hazırlanıyor',
      'orderPreparingDescription': 'Siparişiniz hazırlanıyor',
      'orderOnTheWay': 'Yolda',
      'orderOnTheWayDescription': 'Siparişiniz yolda',
      'orderDelivered': 'Teslim Edildi',
      'orderDeliveredDescription': 'Siparişiniz teslim edildi',
      'callMerchant': 'Satıcıyı Ara',
      'contactSupport': 'Destek İletişim',
    },
    'en': {
      // Auth Screen Texts
      'welcome': 'Welcome',
      'login': 'Login',
      'register': 'Register',
      'email': 'Email',
      'password': 'Password',
      'confirmPassword': 'Confirm Password',
      'firstName': 'First Name',
      'lastName': 'Last Name',
      'phoneNumber': 'Phone Number',
      'forgotPassword': 'Forgot Password?',
      'dontHaveAccount': "Don't have an account?",
      'alreadyHaveAccount': 'Already have an account?',
      'signUp': 'Sign Up',
      'signIn': 'Sign In',
      'orContinueWith': 'or continue with',
      'termsAndConditions': 'Terms and Conditions',
      'privacyPolicy': 'Privacy Policy',
      'iAgreeTo': 'I agree to',
      'and': 'and',

      // Home Screen Texts
      'home': 'Home',
      'search': 'Search',
      'orders': 'My Orders',
      'profile': 'Profile',
      'categories': 'Categories',
      'popularProducts': 'Popular Products',
      'nearbyMerchants': 'Nearby Merchants',
      'deliveryTime': 'Delivery Time',
      'minutes': 'minutes',
      'addToCart': 'Add to Cart',
      'viewAll': 'View All',
      'searchHint': 'Search products, merchants...',
      'selectLocation': 'Select Location',
      'changeLanguage': 'Change Language',

      // Common Texts
      'loading': 'Loading...',
      'error': 'Error',
      'success': 'Success',
      'cancel': 'Cancel',
      'ok': 'OK',
      'retry': 'Retry',
      'save': 'Save',
      'delete': 'Delete',
      'edit': 'Edit',
      'back': 'Back',
      'next': 'Next',
      'previous': 'Previous',
      'done': 'Done',

      // Error Messages
      'emailRequired': 'Email is required',
      'passwordRequired': 'Password is required',
      'invalidEmail': 'Please enter a valid email address',
      'passwordTooShort': 'Password must be at least 6 characters',
      'passwordsDoNotMatch': 'Passwords do not match',
      'firstNameRequired': 'First name is required',
      'lastNameRequired': 'Last name is required',
      'phoneNumberRequired': 'Phone number is required',
      'networkError': 'Network connection error',
      'serverError': 'Server error',
      'unknownError': 'Unknown error',
      'somethingWentWrong': 'Something went wrong',
      'error': 'Error',
      'retry': 'Retry',
      'cancel': 'Cancel',
      'ok': 'OK',
      'success': 'Success',
      'failed': 'Failed',
      'dismiss': 'Dismiss',

      // Network Errors
      'noInternetConnection': 'No internet connection',
      'checkInternetConnection': 'Please check your internet connection',
      'requestTimeout': 'Request timeout',

      // API Errors
      'unauthorizedAccess': 'Unauthorized access',
      'accessForbidden': 'Access forbidden',
      'resourceNotFound': 'Resource not found',
      'validationError': 'Validation error',

      // Business Errors
      'insufficientFunds': 'Insufficient funds',
      'productUnavailable': 'Product unavailable',
      'orderLimitExceeded': 'Order limit exceeded',

      // Storage Errors
      'storageError': 'Storage error',
      'cacheError': 'Cache error',

      // Cart
      'cart': 'Cart',
      'emptyCart': 'Your Cart is Empty',
      'emptyCartMessage': 'Your cart is empty. Start shopping to add products.',
      'startShopping': 'Start Shopping',
      'clearCart': 'Clear Cart',
      'clearCartMessage':
          'Are you sure you want to remove all items from your cart?',
      'removeItem': 'Remove Item',
      'removeItemMessage':
          'Are you sure you want to remove this item from your cart?',
      'couponCode': 'Coupon Code',
      'couponApplied': 'Coupon Applied',
      'subtotal': 'Subtotal',
      'deliveryFee': 'Delivery Fee',
      'discount': 'Discount',
      'total': 'Total',
      'proceedToCheckout': 'Proceed to Checkout',
      'checkoutComingSoon': 'Checkout feature coming soon!',
      'addedToCart': 'Added to cart',
      'remove': 'Remove',
      'apply': 'Apply',

      // Product
      'variants': 'Variants',
      'required': 'Required',
      'nutritionalInfo': 'Nutritional Information',
      'outOfStock': 'Out of Stock',
      'inStock': 'In Stock',
      'searchProducts': 'Search Products',
      'noProductsFound': 'No products found',
      'reviews': 'Reviews',

      // Merchant
      'noMerchantsFound': 'No merchants found',
      'workingHours': 'Working Hours',
      'contactInfo': 'Contact Information',
      'address': 'Address',
      'phone': 'Phone',
      'deliveryInfo': 'Delivery Information',
      'estimatedDeliveryTime': 'Estimated Delivery Time',
      'minimumOrderAmount': 'Minimum Order Amount',
      'open': 'Open',
      'closed': 'Closed',

      // Address
      'addresses': 'My Addresses',
      'addAddress': 'Add Address',
      'editAddress': 'Edit Address',
      'updateAddress': 'Update Address',
      'deleteAddress': 'Delete Address',
      'deleteAddressMessage': 'Are you sure you want to delete this address?',
      'noAddressesFound': 'No Addresses Found',
      'noAddressesMessage':
          'You don\'t have any saved addresses yet. Add a new address to place orders.',
      'defaultAddress': 'Default',
      'setAsDefault': 'Set as Default',
      'setAsDefaultMessage': 'Set this address as your default address',
      'addressType': 'Address Type',
      'addressTitle': 'Address Title',
      'addressTitleHint': 'e.g. Home, Work, Mom\'s House',
      'addressTitleRequired': 'Address title is required',
      'fullAddress': 'Full Address',
      'fullAddressHint': 'Enter neighborhood, street, avenue details',
      'fullAddressRequired': 'Full address is required',
      'buildingNumber': 'Building No',
      'floor': 'Floor',
      'apartment': 'Apartment',
      'landmark': 'Landmark',
      'landmarkHint': 'e.g. Metro exit, opposite mall',
      'getCurrentLocation': 'Get Current Location',
      'gettingLocation': 'Getting location...',
      'locationSet': 'Location set',
      'locationRequired': 'Location information is required',
      'locationError': 'Could not get location',
      'locationPermissionDenied': 'Location permission denied',
      'locationPermissionDeniedForever':
          'Location permission permanently denied',

      // Checkout & Order
      'checkout': 'Checkout',
      'deliveryAddress': 'Delivery Address',
      'paymentMethod': 'Payment Method',
      'orderNotes': 'Order Notes',
      'orderNotesHint': 'You can write special notes about your order...',
      'orderSummary': 'Order Summary',
      'placeOrder': 'Place Order',
      'changeAddress': 'Change Address',
      'selectAddress': 'Select Address',
      'cashPaymentDescription': 'Cash on delivery',
      'cardPaymentDescription': 'Card on delivery',
      'onlinePaymentDescription': 'Online payment',
      'orderConfirmation': 'Order Confirmation',
      'orderConfirmed': 'Your Order is Confirmed!',
      'orderNumber': 'Order No',
      'orderItems': 'Order Items',
      'trackOrder': 'Track Order',
      'continueShopping': 'Continue Shopping',
      'orderStatusInfo': 'You can track your order status and receive updates.',

      // Order History & Tracking
      'orderDetails': 'Order Details',
      'orderInformation': 'Order Information',
      'orderDate': 'Order Date',
      'estimatedDelivery': 'Estimated Delivery',
      'viewDetails': 'View Details',
      'cancelOrder': 'Cancel Order',
      'cancelOrderMessage': 'Are you sure you want to cancel this order?',
      'noOrdersFound': 'No Orders Found',
      'noOrdersMessage': 'You haven\'t placed any orders yet.',
      'noActiveOrders': 'No Active Orders',
      'noActiveOrdersMessage': 'You don\'t have any active orders.',
      'noCompletedOrders': 'No Completed Orders',
      'noCompletedOrdersMessage': 'You don\'t have any completed orders.',
      'trackingProgress': 'Tracking Progress',

      // Settings
      'settings': 'Settings',
      'account': 'Account',
      'manageYourProfile': 'Manage your profile',
      'manageYourAddresses': 'Manage your addresses',
      'orderHistory': 'Order History',
      'viewYourOrders': 'View your orders',
      'notificationSettings': 'Notification Settings',
      'customizeNotifications': 'Customize notifications',
      'appSettings': 'App Settings',
      'language': 'Language',
      'selectLanguage': 'Select Language',
      'theme': 'Theme',
      'systemTheme': 'System Theme',
      'lightTheme': 'Light Theme',
      'darkTheme': 'Dark Theme',
      'selectTheme': 'Select Theme',
      'support': 'Support',
      'helpCenter': 'Help Center',
      'getHelpAndSupport': 'Get help and support',
      'contactUs': 'Contact Us',
      'getInTouch': 'Get in touch',
      'about': 'About',
      'appVersion': 'App Version',
      'logout': 'Logout',
      'logoutConfirmation': 'Are you sure you want to logout?',
      'comingSoon': 'Coming Soon',
      'aboutDescription': 'Getir Mobile - Fast delivery app',
      'offlineMessage': 'No internet connection. Please check your connection.',
      'orderReceived': 'Order Received',
      'orderReceivedDescription':
          'Your order has been received and is being processed.',
      'orderConfirmed': 'Order Confirmed',
      'orderConfirmedDescription':
          'Your order has been confirmed by the merchant.',
      'orderPreparing': 'Preparing',
      'orderPreparingDescription': 'Your order is being prepared.',
      'orderOnTheWay': 'On the Way',
      'orderOnTheWayDescription': 'Your order is on its way to you.',
      'orderDelivered': 'Delivered',
      'orderDeliveredDescription':
          'Your order has been delivered successfully.',
      'needHelp': 'Need Help?',
      'callMerchant': 'Call Merchant',
      'contactSupport': 'Contact Support',
      'paymentPending': 'Payment Pending',
      'paymentCompleted': 'Payment Completed',
      'paymentFailed': 'Payment Failed',
      'pendingDescription': 'Your order is pending confirmation.',
      'confirmedDescription': 'Your order has been confirmed.',
      'preparingDescription': 'Your order is being prepared.',
      'onTheWayDescription': 'Your order is on its way.',
      'deliveredDescription': 'Your order has been delivered.',
      'cancelledDescription': 'Your order has been cancelled.',
      'unknownDescription': 'Order status is unknown.',

      // Order Status
      'all': 'All',
      'active': 'Active',
      'completed': 'Completed',
      'pending': 'Pending',
      'confirmed': 'Confirmed',
      'preparing': 'Preparing',
      'onTheWay': 'On the Way',
      'delivered': 'Delivered',
      'cancelled': 'Cancelled',
      'unknown': 'Unknown',
      'items': 'items',
      'notifications': 'Notifications',
      'orderUpdates': 'Order Updates',
      'promotions': 'Promotions',
      'systemNotifications': 'System Notifications',
      'marketing': 'Marketing',
    },
    'ar': {
      // Auth Screen Texts
      'welcome': 'مرحباً',
      'login': 'تسجيل الدخول',
      'register': 'إنشاء حساب',
      'email': 'البريد الإلكتروني',
      'password': 'كلمة المرور',
      'confirmPassword': 'تأكيد كلمة المرور',
      'firstName': 'الاسم الأول',
      'lastName': 'اسم العائلة',
      'phoneNumber': 'رقم الهاتف',
      'forgotPassword': 'نسيت كلمة المرور؟',
      'dontHaveAccount': 'ليس لديك حساب؟',
      'alreadyHaveAccount': 'لديك حساب بالفعل؟',
      'signUp': 'إنشاء حساب',
      'signIn': 'تسجيل الدخول',
      'orContinueWith': 'أو متابعة مع',
      'termsAndConditions': 'الشروط والأحكام',
      'privacyPolicy': 'سياسة الخصوصية',
      'iAgreeTo': 'أوافق على',
      'and': 'و',

      // Home Screen Texts
      'home': 'الرئيسية',
      'search': 'البحث',
      'orders': 'طلباتي',
      'profile': 'الملف الشخصي',
      'categories': 'الفئات',
      'popularProducts': 'المنتجات الشائعة',
      'nearbyMerchants': 'التجار القريبون',
      'deliveryTime': 'وقت التوصيل',
      'minutes': 'دقيقة',
      'addToCart': 'أضف للسلة',
      'viewAll': 'عرض الكل',
      'searchHint': 'البحث عن المنتجات، التجار...',
      'selectLocation': 'اختر الموقع',
      'changeLanguage': 'تغيير اللغة',

      // Common Texts
      'loading': 'جاري التحميل...',
      'error': 'خطأ',
      'success': 'نجح',
      'cancel': 'إلغاء',
      'ok': 'موافق',
      'retry': 'إعادة المحاولة',
      'save': 'حفظ',
      'delete': 'حذف',
      'edit': 'تعديل',
      'back': 'رجوع',
      'next': 'التالي',
      'previous': 'السابق',
      'done': 'تم',

      // Error Messages
      'emailRequired': 'البريد الإلكتروني مطلوب',
      'passwordRequired': 'كلمة المرور مطلوبة',
      'invalidEmail': 'يرجى إدخال عنوان بريد إلكتروني صحيح',
      'passwordTooShort': 'كلمة المرور يجب أن تكون 6 أحرف على الأقل',
      'passwordsDoNotMatch': 'كلمات المرور غير متطابقة',
      'firstNameRequired': 'الاسم الأول مطلوب',
      'lastNameRequired': 'اسم العائلة مطلوب',
      'phoneNumberRequired': 'رقم الهاتف مطلوب',
      'networkError': 'خطأ في الاتصال بالشبكة',
      'serverError': 'خطأ في الخادم',
      'unknownError': 'خطأ غير معروف',
      'somethingWentWrong': 'حدث خطأ ما',
      'error': 'خطأ',
      'retry': 'إعادة المحاولة',
      'cancel': 'إلغاء',
      'ok': 'موافق',
      'success': 'نجح',
      'failed': 'فشل',
      'dismiss': 'إغلاق',

      // Network Errors
      'noInternetConnection': 'لا يوجد اتصال بالإنترنت',
      'checkInternetConnection': 'يرجى التحقق من اتصال الإنترنت',
      'requestTimeout': 'انتهت مهلة الطلب',

      // API Errors
      'unauthorizedAccess': 'وصول غير مصرح به',
      'accessForbidden': 'الوصول محظور',
      'resourceNotFound': 'المورد غير موجود',
      'validationError': 'خطأ في التحقق',

      // Business Errors
      'insufficientFunds': 'رصيد غير كافي',
      'productUnavailable': 'المنتج غير متوفر',
      'orderLimitExceeded': 'تم تجاوز حد الطلب',

      // Storage Errors
      'storageError': 'خطأ في التخزين',
      'cacheError': 'خطأ في التخزين المؤقت',

      // Cart
      'cart': 'السلة',
      'emptyCart': 'سلتك فارغة',
      'emptyCartMessage': 'سلتك فارغة. ابدأ التسوق لإضافة المنتجات.',
      'startShopping': 'ابدأ التسوق',
      'clearCart': 'مسح السلة',
      'clearCartMessage':
          'هل أنت متأكد من أنك تريد إزالة جميع العناصر من سلتك؟',
      'removeItem': 'إزالة العنصر',
      'removeItemMessage': 'هل أنت متأكد من أنك تريد إزالة هذا العنصر من سلتك؟',
      'couponCode': 'رمز الكوبون',
      'couponApplied': 'تم تطبيق الكوبون',
      'subtotal': 'المجموع الفرعي',
      'deliveryFee': 'رسوم التوصيل',
      'discount': 'خصم',
      'total': 'المجموع',
      'proceedToCheckout': 'المتابعة للدفع',
      'checkoutComingSoon': 'نظام الدفع قادم قريباً!',
      'addedToCart': 'تمت الإضافة للسلة',
      'remove': 'إزالة',
      'apply': 'تطبيق',

      // Product
      'variants': 'المتغيرات',
      'required': 'مطلوب',
      'nutritionalInfo': 'المعلومات الغذائية',
      'outOfStock': 'نفد المخزون',
      'inStock': 'متوفر',
      'searchProducts': 'البحث عن المنتجات',
      'noProductsFound': 'لم يتم العثور على منتجات',
      'reviews': 'التقييمات',

      // Merchant
      'noMerchantsFound': 'لم يتم العثور على تجار',
      'workingHours': 'ساعات العمل',
      'contactInfo': 'معلومات الاتصال',
      'address': 'العنوان',
      'phone': 'الهاتف',
      'deliveryInfo': 'معلومات التوصيل',
      'estimatedDeliveryTime': 'الوقت المتوقع للتوصيل',
      'minimumOrderAmount': 'الحد الأدنى لمبلغ الطلب',
      'open': 'مفتوح',
      'closed': 'مغلق',

      // Address
      'addresses': 'عناويني',
      'addAddress': 'إضافة عنوان',
      'editAddress': 'تعديل العنوان',
      'updateAddress': 'تحديث العنوان',
      'deleteAddress': 'حذف العنوان',
      'deleteAddressMessage': 'هل أنت متأكد من أنك تريد حذف هذا العنوان؟',
      'noAddressesFound': 'لم يتم العثور على عناوين',
      'noAddressesMessage':
          'ليس لديك عناوين محفوظة بعد. أضف عنوان جديد لتتمكن من الطلب.',
      'defaultAddress': 'افتراضي',
      'setAsDefault': 'تعيين كافتراضي',
      'setAsDefaultMessage': 'تعيين هذا العنوان كعنوان افتراضي',
      'addressType': 'نوع العنوان',
      'addressTitle': 'عنوان العنوان',
      'addressTitleHint': 'مثال: المنزل، العمل، منزل الأم',
      'addressTitleRequired': 'عنوان العنوان مطلوب',
      'fullAddress': 'العنوان الكامل',
      'fullAddressHint': 'أدخل تفاصيل الحي، الشارع، الجادة',
      'fullAddressRequired': 'العنوان الكامل مطلوب',
      'buildingNumber': 'رقم المبنى',
      'floor': 'الطابق',
      'apartment': 'الشقة',
      'landmark': 'معلم',
      'landmarkHint': 'مثال: مخرج المترو، مقابل المول',
      'getCurrentLocation': 'الحصول على الموقع الحالي',
      'gettingLocation': 'جاري الحصول على الموقع...',
      'locationSet': 'تم تعيين الموقع',
      'locationRequired': 'معلومات الموقع مطلوبة',
      'locationError': 'لا يمكن الحصول على الموقع',
      'locationPermissionDenied': 'تم رفض إذن الموقع',
      'locationPermissionDeniedForever': 'تم رفض إذن الموقع نهائياً',

      // Checkout & Order
      'checkout': 'إتمام الطلب',
      'deliveryAddress': 'عنوان التوصيل',
      'paymentMethod': 'طريقة الدفع',
      'orderNotes': 'ملاحظات الطلب',
      'orderNotesHint': 'يمكنك كتابة ملاحظات خاصة حول طلبك...',
      'orderSummary': 'ملخص الطلب',
      'placeOrder': 'تأكيد الطلب',
      'changeAddress': 'تغيير العنوان',
      'selectAddress': 'اختر العنوان',
      'cashPaymentDescription': 'الدفع نقداً عند التوصيل',
      'cardPaymentDescription': 'الدفع بالبطاقة عند التوصيل',
      'onlinePaymentDescription': 'الدفع الإلكتروني',
      'orderConfirmation': 'تأكيد الطلب',
      'orderConfirmed': 'تم تأكيد طلبك!',
      'orderNumber': 'رقم الطلب',
      'orderItems': 'عناصر الطلب',
      'trackOrder': 'تتبع الطلب',
      'continueShopping': 'متابعة التسوق',
      'orderStatusInfo': 'يمكنك تتبع حالة طلبك والحصول على التحديثات.',

      // Order History & Tracking
      'orderDetails': 'تفاصيل الطلب',
      'orderInformation': 'معلومات الطلب',
      'orderDate': 'تاريخ الطلب',
      'estimatedDelivery': 'التوصيل المتوقع',
      'viewDetails': 'عرض التفاصيل',
      'cancelOrder': 'إلغاء الطلب',
      'cancelOrderMessage': 'هل أنت متأكد من إلغاء هذا الطلب؟',
      'noOrdersFound': 'لا توجد طلبات',
      'noOrdersMessage': 'لم تقم بطلب أي شيء بعد.',
      'noActiveOrders': 'لا توجد طلبات نشطة',
      'noActiveOrdersMessage': 'ليس لديك أي طلبات نشطة.',
      'noCompletedOrders': 'لا توجد طلبات مكتملة',
      'noCompletedOrdersMessage': 'ليس لديك أي طلبات مكتملة.',
      'trackingProgress': 'تقدم التتبع',

      // Settings
      'settings': 'الإعدادات',
      'account': 'الحساب',
      'manageYourProfile': 'إدارة ملفك الشخصي',
      'manageYourAddresses': 'إدارة عناوينك',
      'orderHistory': 'تاريخ الطلبات',
      'viewYourOrders': 'عرض طلباتك',
      'notificationSettings': 'إعدادات الإشعارات',
      'customizeNotifications': 'تخصيص الإشعارات',
      'appSettings': 'إعدادات التطبيق',
      'language': 'اللغة',
      'selectLanguage': 'اختر اللغة',
      'theme': 'المظهر',
      'systemTheme': 'مظهر النظام',
      'lightTheme': 'المظهر الفاتح',
      'darkTheme': 'المظهر الداكن',
      'selectTheme': 'اختر المظهر',
      'support': 'الدعم',
      'helpCenter': 'مركز المساعدة',
      'getHelpAndSupport': 'احصل على المساعدة والدعم',
      'contactUs': 'اتصل بنا',
      'getInTouch': 'تواصل معنا',
      'about': 'حول',
      'appVersion': 'إصدار التطبيق',
      'logout': 'تسجيل الخروج',
      'logoutConfirmation': 'هل أنت متأكد من أنك تريد تسجيل الخروج؟',
      'comingSoon': 'قريباً',
      'aboutDescription': 'Getir Mobile - تطبيق التوصيل السريع',
      'offlineMessage': 'لا يوجد اتصال بالإنترنت. يرجى التحقق من اتصالك.',
      'orderReceived': 'تم استلام الطلب',
      'orderReceivedDescription': 'تم استلام طلبك وهو قيد المعالجة.',
      'orderConfirmed': 'تم تأكيد الطلب',
      'orderConfirmedDescription': 'تم تأكيد طلبك من قبل التاجر.',
      'orderPreparing': 'قيد التحضير',
      'orderPreparingDescription': 'طلبك قيد التحضير.',
      'orderOnTheWay': 'في الطريق',
      'orderOnTheWayDescription': 'طلبك في طريقه إليك.',
      'orderDelivered': 'تم التسليم',
      'orderDeliveredDescription': 'تم تسليم طلبك بنجاح.',
      'needHelp': 'تحتاج مساعدة؟',
      'callMerchant': 'اتصل بالتاجر',
      'contactSupport': 'اتصل بالدعم',
      'paymentPending': 'الدفع معلق',
      'paymentCompleted': 'تم الدفع',
      'paymentFailed': 'فشل الدفع',
      'pendingDescription': 'طلبك في انتظار التأكيد.',
      'confirmedDescription': 'تم تأكيد طلبك.',
      'preparingDescription': 'طلبك قيد التحضير.',
      'onTheWayDescription': 'طلبك في الطريق.',
      'deliveredDescription': 'تم تسليم طلبك.',
      'cancelledDescription': 'تم إلغاء طلبك.',
      'unknownDescription': 'حالة الطلب غير معروفة.',

      // Order Status
      'all': 'الكل',
      'active': 'نشط',
      'completed': 'مكتمل',
      'pending': 'في الانتظار',
      'confirmed': 'مؤكد',
      'preparing': 'قيد التحضير',
      'onTheWay': 'في الطريق',
      'delivered': 'تم التسليم',
      'cancelled': 'ملغي',
      'unknown': 'غير معروف',
      'items': 'عناصر',
      'notifications': 'الإشعارات',
      'orderUpdates': 'تحديثات الطلب',
      'promotions': 'العروض',
      'systemNotifications': 'إشعارات النظام',
      'marketing': 'التسويق',
    },
  };
}

class _AppLocalizationsDelegate
    extends LocalizationsDelegate<AppLocalizations> {
  const _AppLocalizationsDelegate();

  @override
  bool isSupported(Locale locale) {
    return ['tr', 'en', 'ar'].contains(locale.languageCode);
  }

  @override
  Future<AppLocalizations> load(Locale locale) async {
    return AppLocalizations(locale);
  }

  @override
  bool shouldReload(_AppLocalizationsDelegate old) => false;
}
