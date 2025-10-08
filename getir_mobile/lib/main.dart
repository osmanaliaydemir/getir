import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:provider/provider.dart';
import 'package:hive_flutter/hive_flutter.dart';
import 'package:timeago/timeago.dart' as timeago;

// Core
import 'core/navigation/app_router.dart';
import 'core/theme/app_theme.dart';
import 'core/localization/app_localizations.dart';
import 'core/providers/language_provider.dart';
import 'core/providers/theme_provider.dart';
import 'core/providers/network_provider.dart';
import 'core/config/environment_config.dart';
import 'core/interceptors/ssl_pinning_interceptor.dart';
import 'core/interceptors/cache_interceptor.dart';

// Services
import 'core/services/notification_badge_service.dart';
import 'core/services/firebase_service.dart';
import 'core/services/network_service.dart';
import 'core/services/local_storage_service.dart';
import 'core/services/sync_service.dart';
import 'core/services/api_cache_service.dart';
import 'core/services/performance_service.dart';
import 'core/services/analytics_service.dart';
import 'core/services/encryption_service.dart';
import 'core/services/order_realtime_binder.dart';
import 'core/services/global_keys_service.dart';

// Dependency Injection
import 'core/di/injection.dart';

// BLoC imports (for type annotations only)
import 'presentation/bloc/auth/auth_bloc.dart';
import 'presentation/bloc/merchant/merchant_bloc.dart';
import 'presentation/bloc/product/product_bloc.dart';
import 'presentation/bloc/cart/cart_bloc.dart';
import 'presentation/bloc/address/address_bloc.dart';
import 'presentation/bloc/order/order_bloc.dart';
import 'presentation/bloc/profile/profile_bloc.dart';
import 'presentation/bloc/notification_preferences/notification_preferences_bloc.dart';
import 'presentation/bloc/search/search_bloc.dart';
import 'presentation/bloc/notifications_feed/notifications_feed_bloc.dart';
import 'presentation/bloc/working_hours/working_hours_bloc.dart';
import 'presentation/bloc/review/review_bloc.dart';

// Localization
import 'l10n/app_localizations.g.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  try {
    // 🚀 PARALLEL INITIALIZATION - Independent services
    await Future.wait([
      EnvironmentConfig.initialize(environment: EnvironmentConfig.dev),
      Hive.initFlutter(),
      FirebaseService.initialize(),
    ]);

    EnvironmentConfig.printConfig();

    // 🔐 DEPENDENCY INJECTION SETUP
    debugPrint('🔧 Setting up Dependency Injection...');
    await configureDependencies();
    debugPrint('✅ Dependency Injection configured');

    // 🎯 SEQUENTIAL INITIALIZATION - Dependent services
    await getIt<EncryptionService>().initialize();
    await getIt<LocalStorageService>().initialize();

    // Initialize SSL Pinning (Production only)
    await SslPinningInterceptor.initialize();
    await ApiCacheInterceptor.initialize();

    // Initialize timeago locales
    timeago.setLocaleMessages('tr', timeago.TrMessages());

    // Initialize remaining services
    await NetworkService().initialize();
    await SyncService().initialize();
    await ApiCacheService().initialize();

    // 📊 ANALYTICS & CRASH REPORTING
    AppStartupTracker().markAppStart();

    // Initialize Firebase Crashlytics
    final analytics = getIt<AnalyticsService>();
    await analytics.setCrashlyticsEnabled(true);

    // Set automatic crash reporting
    FlutterError.onError = (errorDetails) {
      analytics.logError(
        error: errorDetails.exception,
        stackTrace: errorDetails.stack,
        reason: 'Flutter Framework Error',
        fatal: true,
      );
    };

    // 🎉 All systems ready
    debugPrint('✅ All services initialized successfully');
    debugPrint('🚀 Launching app...');

    runApp(const GetirApp());
  } catch (e, stackTrace) {
    debugPrint('❌ App initialization failed: $e');
    debugPrint('Stack trace: $stackTrace');

    // Show error screen
    runApp(
      MaterialApp(
        home: Scaffold(
          body: Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                const Icon(Icons.error_outline, size: 64, color: Colors.red),
                const SizedBox(height: 16),
                const Text(
                  'Uygulama başlatılamadı',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Text('Hata: $e'),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class GetirApp extends StatelessWidget {
  const GetirApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => NetworkProvider()..initialize()),
        ChangeNotifierProvider(create: (_) => LanguageProvider()),
        ChangeNotifierProvider(create: (_) => ThemeProvider()),
        ChangeNotifierProvider(create: (_) => NotificationBadgeService()),

        // ✅ DI-powered BLoCs - All migrated!
        BlocProvider<AuthBloc>(create: (_) => getIt<AuthBloc>()),
        BlocProvider<MerchantBloc>(create: (_) => getIt<MerchantBloc>()),
        BlocProvider<ProductBloc>(create: (_) => getIt<ProductBloc>()),
        BlocProvider<CartBloc>(create: (_) => getIt<CartBloc>()),
        BlocProvider<AddressBloc>(create: (_) => getIt<AddressBloc>()),
        BlocProvider<OrderBloc>(create: (_) => getIt<OrderBloc>()),
        BlocProvider<ProfileBloc>(create: (_) => getIt<ProfileBloc>()),
        BlocProvider<NotificationPreferencesBloc>(
          create: (_) => getIt<NotificationPreferencesBloc>(),
        ),
        BlocProvider<SearchBloc>(create: (_) => getIt<SearchBloc>()),
        BlocProvider<NotificationsFeedBloc>(
          create: (_) => getIt<NotificationsFeedBloc>(),
        ),
        BlocProvider<WorkingHoursBloc>(
          create: (_) => getIt<WorkingHoursBloc>(),
        ),
        BlocProvider<ReviewBloc>(create: (_) => getIt<ReviewBloc>()),
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
