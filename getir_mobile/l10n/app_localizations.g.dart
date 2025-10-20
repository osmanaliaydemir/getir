import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:intl/intl.dart' as intl;

import 'app_localizations_ar.g.dart';
import 'app_localizations_en.g.dart';
import 'app_localizations_tr.g.dart';

// ignore_for_file: type=lint

/// Callers can lookup localized strings with an instance of GeneratedLocalizations
/// returned by `GeneratedLocalizations.of(context)`.
///
/// Applications need to include `GeneratedLocalizations.delegate()` in their app's
/// `localizationDelegates` list, and the locales they support in the app's
/// `supportedLocales` list. For example:
///
/// ```dart
/// import 'l10n/app_localizations.g.dart';
///
/// return MaterialApp(
///   localizationsDelegates: GeneratedLocalizations.localizationsDelegates,
///   supportedLocales: GeneratedLocalizations.supportedLocales,
///   home: MyApplicationHome(),
/// );
/// ```
///
/// ## Update pubspec.yaml
///
/// Please make sure to update your pubspec.yaml to include the following
/// packages:
///
/// ```yaml
/// dependencies:
///   # Internationalization support.
///   flutter_localizations:
///     sdk: flutter
///   intl: any # Use the pinned version from flutter_localizations
///
///   # Rest of dependencies
/// ```
///
/// ## iOS Applications
///
/// iOS applications define key application metadata, including supported
/// locales, in an Info.plist file that is built into the application bundle.
/// To configure the locales supported by your app, you’ll need to edit this
/// file.
///
/// First, open your project’s ios/Runner.xcworkspace Xcode workspace file.
/// Then, in the Project Navigator, open the Info.plist file under the Runner
/// project’s Runner folder.
///
/// Next, select the Information Property List item, select Add Item from the
/// Editor menu, then select Localizations from the pop-up menu.
///
/// Select and expand the newly-created Localizations item then, for each
/// locale your application supports, add a new item and select the locale
/// you wish to add from the pop-up menu in the Value field. This list should
/// be consistent with the languages listed in the GeneratedLocalizations.supportedLocales
/// property.
abstract class GeneratedLocalizations {
  GeneratedLocalizations(String locale)
    : localeName = intl.Intl.canonicalizedLocale(locale.toString());

  final String localeName;

  static GeneratedLocalizations of(BuildContext context) {
    return Localizations.of<GeneratedLocalizations>(
      context,
      GeneratedLocalizations,
    )!;
  }

  static const LocalizationsDelegate<GeneratedLocalizations> delegate =
      _GeneratedLocalizationsDelegate();

  /// A list of this localizations delegate along with the default localizations
  /// delegates.
  ///
  /// Returns a list of localizations delegates containing this delegate along with
  /// GlobalMaterialLocalizations.delegate, GlobalCupertinoLocalizations.delegate,
  /// and GlobalWidgetsLocalizations.delegate.
  ///
  /// Additional delegates can be added by appending to this list in
  /// MaterialApp. This list does not have to be used at all if a custom list
  /// of delegates is preferred or required.
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates =
      <LocalizationsDelegate<dynamic>>[
        delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
      ];

  /// A list of this localizations delegate's supported locales.
  static const List<Locale> supportedLocales = <Locale>[
    Locale('ar'),
    Locale('en'),
    Locale('tr'),
  ];

  /// No description provided for @welcome.
  ///
  /// In en, this message translates to:
  /// **'Welcome'**
  String get welcome;

  /// No description provided for @home.
  ///
  /// In en, this message translates to:
  /// **'Home'**
  String get home;

  /// No description provided for @orders.
  ///
  /// In en, this message translates to:
  /// **'My Orders'**
  String get orders;

  /// No description provided for @notifications.
  ///
  /// In en, this message translates to:
  /// **'Notifications'**
  String get notifications;

  /// No description provided for @profile.
  ///
  /// In en, this message translates to:
  /// **'Profile'**
  String get profile;

  /// No description provided for @search.
  ///
  /// In en, this message translates to:
  /// **'Search'**
  String get search;

  /// No description provided for @addToCart.
  ///
  /// In en, this message translates to:
  /// **'Add to Cart'**
  String get addToCart;

  /// No description provided for @retry.
  ///
  /// In en, this message translates to:
  /// **'Retry'**
  String get retry;

  /// No description provided for @ok.
  ///
  /// In en, this message translates to:
  /// **'OK'**
  String get ok;

  /// No description provided for @categories.
  ///
  /// In en, this message translates to:
  /// **'Categories'**
  String get categories;

  /// No description provided for @loadingCategories.
  ///
  /// In en, this message translates to:
  /// **'Loading categories...'**
  String get loadingCategories;

  /// No description provided for @categoriesLoadError.
  ///
  /// In en, this message translates to:
  /// **'Failed to load categories'**
  String get categoriesLoadError;

  /// No description provided for @noCategoriesFound.
  ///
  /// In en, this message translates to:
  /// **'No categories found yet'**
  String get noCategoriesFound;

  /// No description provided for @loadingMerchants.
  ///
  /// In en, this message translates to:
  /// **'Loading merchants...'**
  String get loadingMerchants;

  /// No description provided for @noMerchantsNearby.
  ///
  /// In en, this message translates to:
  /// **'No merchants found nearby'**
  String get noMerchantsNearby;

  /// No description provided for @merchantsLoadError.
  ///
  /// In en, this message translates to:
  /// **'Failed to load merchants'**
  String get merchantsLoadError;

  /// No description provided for @locationPermissionRequired.
  ///
  /// In en, this message translates to:
  /// **'Please grant location permission'**
  String get locationPermissionRequired;

  /// No description provided for @locationError.
  ///
  /// In en, this message translates to:
  /// **'Unable to get location. Please check location permissions.'**
  String get locationError;

  /// No description provided for @nearbyMerchants.
  ///
  /// In en, this message translates to:
  /// **'Nearby Merchants'**
  String get nearbyMerchants;

  /// No description provided for @popularProducts.
  ///
  /// In en, this message translates to:
  /// **'Popular Products'**
  String get popularProducts;

  /// No description provided for @selectLocation.
  ///
  /// In en, this message translates to:
  /// **'Select Location'**
  String get selectLocation;

  /// No description provided for @searchHint.
  ///
  /// In en, this message translates to:
  /// **'Search for stores or products...'**
  String get searchHint;

  /// No description provided for @cart.
  ///
  /// In en, this message translates to:
  /// **'Cart'**
  String get cart;

  /// No description provided for @clearCart.
  ///
  /// In en, this message translates to:
  /// **'Clear Cart'**
  String get clearCart;

  /// No description provided for @emptyCart.
  ///
  /// In en, this message translates to:
  /// **'Your cart is empty'**
  String get emptyCart;

  /// No description provided for @emptyCartMessage.
  ///
  /// In en, this message translates to:
  /// **'Start shopping to add items to your cart'**
  String get emptyCartMessage;

  /// No description provided for @startShopping.
  ///
  /// In en, this message translates to:
  /// **'Start Shopping'**
  String get startShopping;

  /// No description provided for @couponCode.
  ///
  /// In en, this message translates to:
  /// **'Coupon Code'**
  String get couponCode;

  /// No description provided for @apply.
  ///
  /// In en, this message translates to:
  /// **'Apply'**
  String get apply;

  /// No description provided for @couponApplied.
  ///
  /// In en, this message translates to:
  /// **'Coupon Applied'**
  String get couponApplied;

  /// No description provided for @remove.
  ///
  /// In en, this message translates to:
  /// **'Remove'**
  String get remove;

  /// No description provided for @subtotal.
  ///
  /// In en, this message translates to:
  /// **'Subtotal'**
  String get subtotal;

  /// No description provided for @deliveryFee.
  ///
  /// In en, this message translates to:
  /// **'Delivery Fee'**
  String get deliveryFee;

  /// No description provided for @discount.
  ///
  /// In en, this message translates to:
  /// **'Discount'**
  String get discount;

  /// No description provided for @total.
  ///
  /// In en, this message translates to:
  /// **'Total'**
  String get total;

  /// No description provided for @proceedToCheckout.
  ///
  /// In en, this message translates to:
  /// **'Proceed to Checkout'**
  String get proceedToCheckout;

  /// No description provided for @removeItem.
  ///
  /// In en, this message translates to:
  /// **'Remove Item'**
  String get removeItem;

  /// No description provided for @removeItemMessage.
  ///
  /// In en, this message translates to:
  /// **'Are you sure you want to remove this item from your cart?'**
  String get removeItemMessage;

  /// No description provided for @cancel.
  ///
  /// In en, this message translates to:
  /// **'Cancel'**
  String get cancel;

  /// No description provided for @clearCartMessage.
  ///
  /// In en, this message translates to:
  /// **'Are you sure you want to remove all items from your cart?'**
  String get clearCartMessage;

  /// No description provided for @checkout.
  ///
  /// In en, this message translates to:
  /// **'Checkout'**
  String get checkout;

  /// No description provided for @deliveryAddress.
  ///
  /// In en, this message translates to:
  /// **'Delivery Address'**
  String get deliveryAddress;

  /// No description provided for @paymentMethod.
  ///
  /// In en, this message translates to:
  /// **'Payment Method'**
  String get paymentMethod;

  /// No description provided for @orderNotes.
  ///
  /// In en, this message translates to:
  /// **'Order Notes'**
  String get orderNotes;

  /// No description provided for @orderSummary.
  ///
  /// In en, this message translates to:
  /// **'Order Summary'**
  String get orderSummary;

  /// No description provided for @placeOrder.
  ///
  /// In en, this message translates to:
  /// **'Place Order'**
  String get placeOrder;

  /// No description provided for @cash.
  ///
  /// In en, this message translates to:
  /// **'Cash'**
  String get cash;

  /// No description provided for @card.
  ///
  /// In en, this message translates to:
  /// **'Card'**
  String get card;

  /// No description provided for @products.
  ///
  /// In en, this message translates to:
  /// **'Products'**
  String get products;

  /// No description provided for @searchProducts.
  ///
  /// In en, this message translates to:
  /// **'Search Products'**
  String get searchProducts;

  /// No description provided for @noProductsFound.
  ///
  /// In en, this message translates to:
  /// **'No products found'**
  String get noProductsFound;

  /// No description provided for @viewAll.
  ///
  /// In en, this message translates to:
  /// **'View All'**
  String get viewAll;

  /// No description provided for @inStock.
  ///
  /// In en, this message translates to:
  /// **'In Stock'**
  String get inStock;

  /// No description provided for @outOfStock.
  ///
  /// In en, this message translates to:
  /// **'Out of Stock'**
  String get outOfStock;

  /// No description provided for @reviews.
  ///
  /// In en, this message translates to:
  /// **'Reviews'**
  String get reviews;

  /// No description provided for @productDetails.
  ///
  /// In en, this message translates to:
  /// **'Product Details'**
  String get productDetails;

  /// No description provided for @variants.
  ///
  /// In en, this message translates to:
  /// **'Variants'**
  String get variants;

  /// No description provided for @required.
  ///
  /// In en, this message translates to:
  /// **'Required'**
  String get required;

  /// No description provided for @nutritionalInfo.
  ///
  /// In en, this message translates to:
  /// **'Nutritional Info'**
  String get nutritionalInfo;

  /// No description provided for @addedToCart.
  ///
  /// In en, this message translates to:
  /// **'Added to cart'**
  String get addedToCart;
}

class _GeneratedLocalizationsDelegate
    extends LocalizationsDelegate<GeneratedLocalizations> {
  const _GeneratedLocalizationsDelegate();

  @override
  Future<GeneratedLocalizations> load(Locale locale) {
    return SynchronousFuture<GeneratedLocalizations>(
      lookupGeneratedLocalizations(locale),
    );
  }

  @override
  bool isSupported(Locale locale) =>
      <String>['ar', 'en', 'tr'].contains(locale.languageCode);

  @override
  bool shouldReload(_GeneratedLocalizationsDelegate old) => false;
}

GeneratedLocalizations lookupGeneratedLocalizations(Locale locale) {
  // Lookup logic when only language code is specified.
  switch (locale.languageCode) {
    case 'ar':
      return GeneratedLocalizationsAr();
    case 'en':
      return GeneratedLocalizationsEn();
    case 'tr':
      return GeneratedLocalizationsTr();
  }

  throw FlutterError(
    'GeneratedLocalizations.delegate failed to load unsupported locale "$locale". This is likely '
    'an issue with the localizations generation tool. Please file an issue '
    'on GitHub with a reproducible sample app and the gen-l10n configuration '
    'that was used.',
  );
}
