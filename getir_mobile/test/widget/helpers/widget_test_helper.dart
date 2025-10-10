import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';

/// Widget Test Helper
///
/// Provides utilities for creating testable widgets with all required dependencies
class WidgetTestHelper {
  /// Creates a MaterialApp wrapper with proper localization setup
  static Widget createTestApp({
    required Widget child,
    Locale locale = const Locale('tr', 'TR'),
    List<BlocProvider> providers = const [],
  }) {
    Widget wrappedChild = child;

    if (providers.isNotEmpty) {
      wrappedChild = MultiBlocProvider(providers: providers, child: child);
    }

    return MaterialApp(
      localizationsDelegates: const [
        ...AppLocalizations.localizationsDelegates,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: AppLocalizations.supportedLocales,
      locale: locale,
      home: wrappedChild,
    );
  }

  /// Creates a MaterialApp with Navigator for testing navigation
  static Widget createTestAppWithRoutes({
    required Widget home,
    Map<String, WidgetBuilder>? routes,
    List<BlocProvider> providers = const [],
  }) {
    Widget wrappedHome = home;

    if (providers.isNotEmpty) {
      wrappedHome = MultiBlocProvider(providers: providers, child: home);
    }

    return MaterialApp(
      localizationsDelegates: const [
        ...AppLocalizations.localizationsDelegates,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: AppLocalizations.supportedLocales,
      locale: const Locale('tr', 'TR'),
      home: wrappedHome,
      routes: routes ?? {},
    );
  }
}
