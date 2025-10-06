// Temporary shim until gen-l10n runs. Mirrors existing AppLocalizations wiring.
import 'package:flutter/material.dart';
import '../core/localization/app_localizations.dart';

class GeneratedLocalizations {
  static const List<LocalizationsDelegate<dynamic>> localizationsDelegates = [
    ...AppLocalizations.localizationsDelegates,
  ];

  static const List<Locale> supportedLocales = [
    Locale('tr', 'TR'),
    Locale('en', 'US'),
    Locale('ar', 'SA'),
  ];
}
