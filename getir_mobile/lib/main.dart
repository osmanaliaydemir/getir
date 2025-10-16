import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

// Core
import 'core/navigation/app_router.dart';
import 'core/theme/app_theme.dart';
import 'core/localization/app_localizations.dart';
import 'core/cubits/network/network_cubit.dart';
import 'core/cubits/language/language_cubit.dart';
import 'core/cubits/theme/theme_cubit.dart';
import 'core/cubits/notification_badge/notification_badge_cubit.dart';
import 'core/services/order_realtime_binder.dart';
import 'core/services/global_keys_service.dart';
import 'core/services/performance_service.dart';

// Initialization
import 'core/initialization/app_initializer.dart';

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
    // 🚀 Initialize app with centralized initialization logic
    await AppInitializer.initialize();

    // 🎉 Launch app
    runApp(const GetirApp());
  } catch (e, stackTrace) {
    // Show error screen if initialization fails
    runApp(_buildErrorScreen(e, stackTrace));
  }
}

/// Build error screen for initialization failures
Widget _buildErrorScreen(dynamic error, StackTrace stackTrace) {
  return MaterialApp(
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
            Text('Hata: $error'),
          ],
        ),
      ),
    ),
  );
}

class GetirApp extends StatelessWidget {
  const GetirApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        // ✅ Global State Cubits (replaces Providers)
        BlocProvider<NetworkCubit>(
          create: (_) => getIt<NetworkCubit>()..initialize(),
        ),
        BlocProvider<LanguageCubit>(create: (_) => getIt<LanguageCubit>()),
        BlocProvider<ThemeCubit>(create: (_) => getIt<ThemeCubit>()),
        BlocProvider<NotificationBadgeCubit>(
          create: (_) => getIt<NotificationBadgeCubit>(),
        ),

        // ✅ Feature BLoCs (DI-powered)
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
      child: BlocBuilder<LanguageCubit, LanguageState>(
        builder: (context, languageState) {
          return BlocBuilder<ThemeCubit, ThemeState>(
            builder: (context, themeState) {
              return MaterialApp.router(
                title: 'Getir Mobile',
                debugShowCheckedModeBanner: false,
                theme: AppTheme.lightTheme,
                darkTheme: AppTheme.darkTheme,
                themeMode: themeState.themeMode,
                locale: languageState.locale,
                scaffoldMessengerKey: GlobalKeysService.scaffoldMessengerKey,
                localizationsDelegates: const [
                  GlobalMaterialLocalizations.delegate,
                  GlobalWidgetsLocalizations.delegate,
                  GlobalCupertinoLocalizations.delegate,
                  ...GeneratedLocalizations.localizationsDelegates,
                ],
                supportedLocales: const [
                  Locale('en', 'US'),
                  Locale('tr', 'TR'),
                  Locale('ar', 'SA'),
                ],
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
          );
        },
      ),
    );
  }
}
