import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../services/logger_service.dart';

part 'language_state.dart';

/// LanguageCubit
///
/// Manages app language/locale using BLoC pattern
/// Replaces LanguageProvider for consistent state management architecture
class LanguageCubit extends Cubit<LanguageState> {
  static const String _languageKey = 'selected_language';
  final SharedPreferences _prefs;

  LanguageCubit(this._prefs)
    : super(const LanguageState(locale: Locale('tr', 'TR'))) {
    _loadSavedLanguage();
  }

  /// Load saved language from SharedPreferences
  Future<void> _loadSavedLanguage() async {
    try {
      final languageCode = _prefs.getString(_languageKey);

      if (languageCode != null) {
        final locale = _getLocaleFromCode(languageCode);
        emit(LanguageState(locale: locale));
        logger.info(
          'Loaded saved language',
          tag: 'LanguageCubit',
          context: {'languageCode': languageCode},
        );
      }
    } catch (e, stackTrace) {
      logger.error(
        'Failed to load saved language',
        tag: 'LanguageCubit',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  /// Change language
  Future<void> changeLanguage(Locale locale) async {
    if (state.locale == locale) return;

    emit(LanguageState(locale: locale));

    // Save to preferences
    await _prefs.setString(_languageKey, locale.languageCode);

    logger.info(
      'Language changed',
      tag: 'LanguageCubit',
      context: {'languageCode': locale.languageCode},
    );
  }

  /// Change language by code
  Future<void> changeLanguageByCode(String languageCode) async {
    final locale = _getLocaleFromCode(languageCode);
    await changeLanguage(locale);
  }

  /// Get Locale from language code
  Locale _getLocaleFromCode(String languageCode) {
    switch (languageCode) {
      case 'en':
        return const Locale('en', 'US');
      case 'ar':
        return const Locale('ar', 'SA');
      case 'tr':
      default:
        return const Locale('tr', 'TR');
    }
  }

  /// Get language name
  String getLanguageName(String languageCode) {
    switch (languageCode) {
      case 'tr':
        return 'Türkçe';
      case 'en':
        return 'English';
      case 'ar':
        return 'العربية';
      default:
        return 'Türkçe';
    }
  }

  /// Get language flag
  String getLanguageFlag(String languageCode) {
    switch (languageCode) {
      case 'tr':
        return '🇹🇷';
      case 'en':
        return '🇺🇸';
      case 'ar':
        return '🇸🇦';
      default:
        return '🇹🇷';
    }
  }
}
