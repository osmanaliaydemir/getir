import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:provider/provider.dart';
import 'core/navigation/app_router.dart';
import 'core/theme/app_theme.dart';
import 'core/localization/app_localizations.dart';
import 'core/providers/language_provider.dart';
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
import 'presentation/bloc/profile/profile_bloc.dart';
import 'presentation/bloc/notification_preferences/notification_preferences_bloc.dart';
import 'domain/usecases/notification_usecases.dart';
import 'core/services/firebase_service.dart';
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

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

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

  runApp(const GetirApp());
}

class GetirApp extends StatelessWidget {
  const GetirApp({super.key});

  @override
  Widget build(BuildContext context) {
    // Create Dio instance via ApiClient
    final dio = ApiClient().dio;

    // Create repository instances
    final authRepository = AuthRepositoryImpl(AuthDataSourceImpl());
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

    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => NetworkProvider()..initialize()),
        ChangeNotifierProvider(create: (_) => LanguageProvider()),
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
      ],
      child: Consumer<LanguageProvider>(
        builder: (context, languageProvider, child) {
          return MaterialApp.router(
            title: 'Getir Mobile',
            debugShowCheckedModeBanner: false,
            theme: AppTheme.lightTheme,
            darkTheme: AppTheme.darkTheme,
            themeMode: ThemeMode.system,
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
