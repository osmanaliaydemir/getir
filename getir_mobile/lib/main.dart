import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:provider/provider.dart';
import 'core/navigation/app_router.dart';
import 'core/theme/app_theme.dart';
import 'core/localization/app_localizations.dart';
import 'core/providers/language_provider.dart';
import 'core/providers/theme_provider.dart';
import 'core/services/notification_badge_service.dart';
import 'presentation/bloc/auth/auth_bloc.dart';
import 'presentation/bloc/merchant/merchant_bloc.dart';
import 'presentation/bloc/product/product_bloc.dart';
import 'presentation/bloc/cart/cart_bloc.dart';
import 'presentation/bloc/address/address_bloc.dart';
import 'presentation/bloc/order/order_bloc.dart';
import 'domain/usecases/auth_usecases.dart';
import 'domain/usecases/merchant_usecases.dart';
import 'domain/usecases/product_usecases.dart';
import 'domain/usecases/cart_usecases.dart';
import 'domain/usecases/address_usecases.dart';
import 'domain/usecases/order_usecases.dart';
import 'domain/usecases/profile_usecases.dart';
import 'data/repositories/auth_repository_impl.dart';
import 'data/repositories/merchant_repository_impl.dart';
import 'data/repositories/product_repository_impl.dart';
import 'data/repositories/cart_repository_impl.dart';
import 'data/repositories/address_repository_impl.dart';
import 'data/repositories/order_repository_impl.dart';
import 'data/repositories/profile_repository_impl.dart';
import 'data/repositories/notification_repository_impl.dart';
import 'data/datasources/auth_datasource_impl.dart';
import 'data/datasources/merchant_datasource.dart';
import 'data/datasources/product_datasource.dart';
import 'data/datasources/cart_datasource.dart';
import 'data/datasources/address_datasource.dart';
import 'data/datasources/order_datasource.dart';
import 'data/datasources/profile_datasource.dart';
import 'data/datasources/notification_preferences_datasource.dart';
import 'data/datasources/notifications_feed_datasource.dart';
import 'data/repositories/notifications_feed_repository_impl.dart';
import 'presentation/bloc/profile/profile_bloc.dart';
import 'presentation/bloc/notification_preferences/notification_preferences_bloc.dart';
import 'presentation/bloc/search/search_bloc.dart';
import 'presentation/bloc/notifications_feed/notifications_feed_bloc.dart';
import 'presentation/bloc/working_hours/working_hours_bloc.dart';
import 'presentation/bloc/review/review_bloc.dart';
import 'domain/usecases/notification_usecases.dart';
import 'domain/usecases/working_hours_usecases.dart';
import 'domain/usecases/review_usecases.dart';
import 'data/datasources/working_hours_datasource.dart';
import 'data/datasources/review_datasource.dart';
import 'data/repositories/working_hours_repository_impl.dart';
import 'data/repositories/review_repository_impl.dart';
import 'core/services/firebase_service.dart';
import 'core/services/search_history_service.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'core/providers/network_provider.dart';
import 'core/services/network_service.dart';
import 'core/services/local_storage_service.dart';
import 'core/services/sync_service.dart';
import 'core/services/api_cache_service.dart';
import 'core/services/performance_service.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'l10n/app_localizations.g.dart';
import 'core/services/global_keys_service.dart';
import 'core/services/analytics_service.dart';
import 'core/services/api_client.dart';
import 'core/services/order_realtime_binder.dart';
import 'core/services/encryption_service.dart';
import 'core/config/environment_config.dart';
import 'core/interceptors/ssl_pinning_interceptor.dart';
import 'core/interceptors/cache_interceptor.dart';
import 'package:timeago/timeago.dart' as timeago;

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // 🔐 SECURITY & PERFORMANCE INITIALIZATION
  // Initialize Environment Configuration
  await EnvironmentConfig.initialize(environment: EnvironmentConfig.dev);
  EnvironmentConfig.printConfig();

  // Initialize Encryption Service (Secure Storage)
  await EncryptionService().initialize();

  // Initialize SSL Pinning (Production only)
  await SslPinningInterceptor.initialize();

  // Initialize API Cache
  await ApiCacheInterceptor.initialize();

  // Initialize timeago locales
  timeago.setLocaleMessages('tr', timeago.TrMessages());

  // Initialize Hive
  await Hive.initFlutter();

  // Initialize Services
  await FirebaseService.initialize();
  await NetworkService().initialize();
  await LocalStorageService().initialize();
  await SyncService().initialize();
  await ApiCacheService().initialize();

  // Initialize Performance Monitoring
  AppStartupTracker().markAppStart();
  await AnalyticsService().initialize();

  // Initialize SharedPreferences
  final prefs = await SharedPreferences.getInstance();

  // 🎉 All systems ready
  debugPrint('✅ All services initialized successfully');

  runApp(GetirApp(prefs: prefs));
}

class GetirApp extends StatelessWidget {
  final SharedPreferences prefs;

  const GetirApp({super.key, required this.prefs});

  @override
  Widget build(BuildContext context) {
    // Create Dio instance via ApiClient
    final dio = ApiClient().dio;

    // Create SearchHistoryService
    final searchHistoryService = SearchHistoryService(prefs);

    // Create repository instances
    final authRepository = AuthRepositoryImpl(AuthDataSourceImpl(dio: dio));
    final merchantRepository = MerchantRepositoryImpl(
      MerchantDataSourceImpl(dio),
    );
    final productRepository = ProductRepositoryImpl(ProductDataSourceImpl(dio));
    final cartRepository = CartRepositoryImpl(CartDataSourceImpl(dio));
    final addressRepository = AddressRepositoryImpl(AddressDataSourceImpl(dio));
    final orderRepository = OrderRepositoryImpl(OrderDataSourceImpl(dio));
    final profileRepository = ProfileRepositoryImpl(ProfileDataSourceImpl(dio));
    final notificationRepository = NotificationRepositoryImpl(
      NotificationPreferencesDataSourceImpl(dio),
    );
    final notificationsFeedRepository = NotificationsFeedRepositoryImpl(
      NotificationsFeedDataSourceImpl(dio),
    );
    final workingHoursRepository = WorkingHoursRepositoryImpl(
      WorkingHoursDataSourceImpl(dio: dio),
    );
    final reviewRepository = ReviewRepositoryImpl(
      ReviewDataSourceImpl(dio: dio),
    );

    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => NetworkProvider()..initialize()),
        ChangeNotifierProvider(create: (_) => LanguageProvider()),
        ChangeNotifierProvider(create: (_) => ThemeProvider()),
        ChangeNotifierProvider(create: (_) => NotificationBadgeService()),
        BlocProvider<AuthBloc>(
          create: (context) => AuthBloc(
            loginUseCase: LoginUseCase(authRepository),
            registerUseCase: RegisterUseCase(authRepository),
            logoutUseCase: LogoutUseCase(authRepository),
            refreshTokenUseCase: RefreshTokenUseCase(authRepository),
            forgotPasswordUseCase: ForgotPasswordUseCase(authRepository),
            resetPasswordUseCase: ResetPasswordUseCase(authRepository),
            getCurrentUserUseCase: GetCurrentUserUseCase(authRepository),
            checkAuthenticationUseCase: CheckAuthenticationUseCase(
              authRepository,
            ),
            checkTokenValidityUseCase: CheckTokenValidityUseCase(
              authRepository,
            ),
          ),
        ),
        BlocProvider<MerchantBloc>(
          create: (context) => MerchantBloc(
            getMerchantsUseCase: GetMerchantsUseCase(merchantRepository),
            getMerchantByIdUseCase: GetMerchantByIdUseCase(merchantRepository),
            searchMerchantsUseCase: SearchMerchantsUseCase(merchantRepository),
            getNearbyMerchantsUseCase: GetNearbyMerchantsUseCase(
              merchantRepository,
            ),
            getNearbyMerchantsByCategoryUseCase:
                GetNearbyMerchantsByCategoryUseCase(merchantRepository),
          ),
        ),
        BlocProvider<ProductBloc>(
          create: (context) => ProductBloc(
            getProductsUseCase: GetProductsUseCase(productRepository),
            getProductByIdUseCase: GetProductByIdUseCase(productRepository),
            getProductsByMerchantUseCase: GetProductsByMerchantUseCase(
              productRepository,
            ),
            searchProductsUseCase: SearchProductsUseCase(productRepository),
            getCategoriesUseCase: GetCategoriesUseCase(productRepository),
          ),
        ),
        BlocProvider<CartBloc>(
          create: (context) => CartBloc(
            getCartUseCase: GetCartUseCase(cartRepository),
            addToCartUseCase: AddToCartUseCase(cartRepository),
            updateCartItemUseCase: UpdateCartItemUseCase(cartRepository),
            removeFromCartUseCase: RemoveFromCartUseCase(cartRepository),
            clearCartUseCase: ClearCartUseCase(cartRepository),
            applyCouponUseCase: ApplyCouponUseCase(cartRepository),
            removeCouponUseCase: RemoveCouponUseCase(cartRepository),
          ),
        ),
        BlocProvider<AddressBloc>(
          create: (context) => AddressBloc(
            getUserAddressesUseCase: GetUserAddressesUseCase(addressRepository),
            getAddressByIdUseCase: GetAddressByIdUseCase(addressRepository),
            createAddressUseCase: CreateAddressUseCase(addressRepository),
            updateAddressUseCase: UpdateAddressUseCase(addressRepository),
            deleteAddressUseCase: DeleteAddressUseCase(addressRepository),
            setDefaultAddressUseCase: SetDefaultAddressUseCase(
              addressRepository,
            ),
          ),
        ),
        BlocProvider<OrderBloc>(
          create: (context) => OrderBloc(
            getUserOrdersUseCase: GetUserOrdersUseCase(orderRepository),
            getOrderByIdUseCase: GetOrderByIdUseCase(orderRepository),
            createOrderUseCase: CreateOrderUseCase(orderRepository),
            cancelOrderUseCase: CancelOrderUseCase(orderRepository),
            processPaymentUseCase: ProcessPaymentUseCase(orderRepository),
            getPaymentStatusUseCase: GetPaymentStatusUseCase(orderRepository),
          ),
        ),
        BlocProvider<ProfileBloc>(
          create: (context) => ProfileBloc(
            getUserProfileUseCase: GetUserProfileUseCase(profileRepository),
            updateUserProfileUseCase: UpdateUserProfileUseCase(
              profileRepository,
            ),
          ),
        ),
        BlocProvider<NotificationPreferencesBloc>(
          create: (context) => NotificationPreferencesBloc(
            getUseCase: GetNotificationPreferencesUseCase(
              notificationRepository,
            ),
            updateUseCase: UpdateNotificationPreferencesUseCase(
              notificationRepository,
            ),
          ),
        ),
        BlocProvider<SearchBloc>(
          create: (context) => SearchBloc(
            searchMerchantsUseCase: SearchMerchantsUseCase(merchantRepository),
            searchProductsUseCase: SearchProductsUseCase(productRepository),
            searchHistoryService: searchHistoryService,
          ),
        ),
        BlocProvider<NotificationsFeedBloc>(
          create: (context) =>
              NotificationsFeedBloc(repository: notificationsFeedRepository),
        ),
        BlocProvider<WorkingHoursBloc>(
          create: (context) => WorkingHoursBloc(
            getWorkingHoursUseCase: GetWorkingHoursUseCase(
              workingHoursRepository,
            ),
            checkIfMerchantOpenUseCase: CheckIfMerchantOpenUseCase(
              workingHoursRepository,
            ),
            getNextOpenTimeUseCase: GetNextOpenTimeUseCase(
              workingHoursRepository,
            ),
          ),
        ),
        BlocProvider<ReviewBloc>(
          create: (context) => ReviewBloc(
            submitReviewUseCase: SubmitReviewUseCase(reviewRepository),
            getMerchantReviewsUseCase: GetMerchantReviewsUseCase(
              reviewRepository,
            ),
            markReviewAsHelpfulUseCase: MarkReviewAsHelpfulUseCase(
              reviewRepository,
            ),
          ),
        ),
      ],
      child: Consumer2<LanguageProvider, ThemeProvider>(
        builder: (context, languageProvider, themeProvider, child) {
          return MaterialApp.router(
            title: 'Getir Mobile',
            debugShowCheckedModeBanner: false,
            theme: AppTheme.lightTheme,
            darkTheme: AppTheme.darkTheme,
            themeMode: themeProvider.themeMode,
            locale: languageProvider.currentLocale,
            scaffoldMessengerKey: GlobalKeysService.scaffoldMessengerKey,
            localizationsDelegates: [
              ...AppLocalizations.localizationsDelegates,
              ...GeneratedLocalizations.localizationsDelegates,
            ],
            supportedLocales: GeneratedLocalizations.supportedLocales,
            routerConfig: AppRouter.router,
            builder: (context, child) {
              // Mark first frame rendered
              WidgetsBinding.instance.addPostFrameCallback((_) {
                AppStartupTracker().markFirstFrame();
                // Start order realtime binder once we have a context with blocs
                OrderRealtimeBinder().start(context);
              });
              return child!;
            },
          );
        },
      ),
    );
  }
}
