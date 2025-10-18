import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:get_it/get_it.dart';

import 'package:getir_mobile/core/cubits/language/language_cubit.dart';
import 'package:getir_mobile/core/services/logger_service.dart';

import 'language_cubit_test.mocks.dart';

@GenerateMocks([SharedPreferences, LoggerService])
void main() {
  final getIt = GetIt.instance;

  group('LanguageCubit', () {
    late LanguageCubit cubit;
    late MockSharedPreferences mockPrefs;
    late MockLoggerService mockLoggerService;

    setUpAll(() {
      // Register LoggerService mock for the entire test suite
      mockLoggerService = MockLoggerService();
      getIt.registerSingleton<LoggerService>(mockLoggerService);
    });

    setUp(() {
      mockPrefs = MockSharedPreferences();
    });

    tearDownAll(() {
      getIt.reset();
    });

    tearDown(() {
      cubit.close();
    });

    // ==================== Initial State Tests ====================
    group('Initial State', () {
      test('initial state should be Turkish (tr_TR)', () {
        when(mockPrefs.getString(any)).thenReturn(null);
        cubit = LanguageCubit(mockPrefs);

        expect(cubit.state.locale, equals(const Locale('tr', 'TR')));
        expect(cubit.state.languageCode, equals('tr'));
      });

      test('should load saved Turkish language from preferences', () async {
        when(mockPrefs.getString('selected_language')).thenReturn('tr');
        cubit = LanguageCubit(mockPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(cubit.state.locale, equals(const Locale('tr', 'TR')));
      });

      test('should load saved English language from preferences', () async {
        when(mockPrefs.getString('selected_language')).thenReturn('en');
        cubit = LanguageCubit(mockPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(cubit.state.locale, equals(const Locale('en', 'US')));
      });

      test('should load saved Arabic language from preferences', () async {
        when(mockPrefs.getString('selected_language')).thenReturn('ar');
        cubit = LanguageCubit(mockPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(cubit.state.locale, equals(const Locale('ar', 'SA')));
      });

      test(
        'should handle null saved language and use default (Turkish)',
        () async {
          when(mockPrefs.getString('selected_language')).thenReturn(null);
          cubit = LanguageCubit(mockPrefs);

          await Future.delayed(const Duration(milliseconds: 100));

          expect(cubit.state.locale, equals(const Locale('tr', 'TR')));
        },
      );

      test('should handle error loading saved language gracefully', () async {
        when(
          mockPrefs.getString('selected_language'),
        ).thenThrow(Exception('Preferences error'));
        cubit = LanguageCubit(mockPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(cubit.state.locale, equals(const Locale('tr', 'TR')));
      });
    });

    // ==================== changeLanguage Tests ====================
    group('changeLanguage', () {
      setUp(() {
        when(mockPrefs.getString(any)).thenReturn(null);
        when(mockPrefs.setString(any, any)).thenAnswer((_) async => true);
        cubit = LanguageCubit(mockPrefs);
      });

      blocTest<LanguageCubit, LanguageState>(
        'should change language to English',
        build: () => cubit,
        act: (cubit) => cubit.changeLanguage(const Locale('en', 'US')),
        expect: () => [const LanguageState(locale: Locale('en', 'US'))],
        verify: (_) {
          verify(mockPrefs.setString('selected_language', 'en')).called(1);
        },
      );

      blocTest<LanguageCubit, LanguageState>(
        'should change language to Arabic',
        build: () => cubit,
        act: (cubit) => cubit.changeLanguage(const Locale('ar', 'SA')),
        expect: () => [const LanguageState(locale: Locale('ar', 'SA'))],
        verify: (_) {
          verify(mockPrefs.setString('selected_language', 'ar')).called(1);
        },
      );

      blocTest<LanguageCubit, LanguageState>(
        'should change language to Turkish',
        build: () {
          when(mockPrefs.getString(any)).thenReturn(null);
          return LanguageCubit(mockPrefs);
        },
        seed: () => const LanguageState(locale: Locale('en', 'US')),
        act: (cubit) => cubit.changeLanguage(const Locale('tr', 'TR')),
        expect: () => [const LanguageState(locale: Locale('tr', 'TR'))],
        verify: (_) {
          verify(mockPrefs.setString('selected_language', 'tr')).called(1);
        },
      );

      blocTest<LanguageCubit, LanguageState>(
        'should not emit state if language is already selected',
        build: () => cubit,
        act: (cubit) => cubit.changeLanguage(const Locale('tr', 'TR')),
        expect: () => [],
        verify: (_) {
          verifyNever(mockPrefs.setString(any, any));
        },
      );

      blocTest<LanguageCubit, LanguageState>(
        'should save language to preferences when changed',
        build: () => cubit,
        act: (cubit) => cubit.changeLanguage(const Locale('en', 'US')),
        verify: (_) {
          verify(mockPrefs.setString('selected_language', 'en')).called(1);
        },
      );

      test('should handle preference save error gracefully', () async {
        when(mockPrefs.setString(any, any)).thenThrow(Exception('Save failed'));

        expect(
          () => cubit.changeLanguage(const Locale('en', 'US')),
          throwsA(isA<Exception>()),
        );
      });
    });

    // ==================== changeLanguageByCode Tests ====================
    group('changeLanguageByCode', () {
      setUp(() {
        when(mockPrefs.getString(any)).thenReturn(null);
        when(mockPrefs.setString(any, any)).thenAnswer((_) async => true);
        cubit = LanguageCubit(mockPrefs);
      });

      blocTest<LanguageCubit, LanguageState>(
        'should change language by code "en"',
        build: () => cubit,
        act: (cubit) => cubit.changeLanguageByCode('en'),
        expect: () => [const LanguageState(locale: Locale('en', 'US'))],
      );

      blocTest<LanguageCubit, LanguageState>(
        'should change language by code "ar"',
        build: () => cubit,
        act: (cubit) => cubit.changeLanguageByCode('ar'),
        expect: () => [const LanguageState(locale: Locale('ar', 'SA'))],
      );

      blocTest<LanguageCubit, LanguageState>(
        'should change language by code "tr"',
        build: () {
          when(mockPrefs.getString(any)).thenReturn(null);
          return LanguageCubit(mockPrefs);
        },
        seed: () => const LanguageState(locale: Locale('en', 'US')),
        act: (cubit) => cubit.changeLanguageByCode('tr'),
        expect: () => [const LanguageState(locale: Locale('tr', 'TR'))],
      );

      blocTest<LanguageCubit, LanguageState>(
        'should default to Turkish for invalid language code',
        build: () {
          when(mockPrefs.getString(any)).thenReturn(null);
          return LanguageCubit(mockPrefs);
        },
        seed: () => const LanguageState(locale: Locale('en', 'US')),
        act: (cubit) => cubit.changeLanguageByCode('invalid'),
        expect: () => [const LanguageState(locale: Locale('tr', 'TR'))],
      );

      blocTest<LanguageCubit, LanguageState>(
        'should handle empty language code and default to Turkish',
        build: () {
          when(mockPrefs.getString(any)).thenReturn(null);
          return LanguageCubit(mockPrefs);
        },
        seed: () => const LanguageState(locale: Locale('en', 'US')),
        act: (cubit) => cubit.changeLanguageByCode(''),
        expect: () => [const LanguageState(locale: Locale('tr', 'TR'))],
      );
    });

    // ==================== getLanguageName Tests ====================
    group('getLanguageName', () {
      setUp(() {
        when(mockPrefs.getString(any)).thenReturn(null);
        cubit = LanguageCubit(mockPrefs);
      });

      test('should return "Türkçe" for Turkish', () {
        final name = cubit.getLanguageName('tr');
        expect(name, equals('Türkçe'));
      });

      test('should return "English" for English', () {
        final name = cubit.getLanguageName('en');
        expect(name, equals('English'));
      });

      test('should return "العربية" for Arabic', () {
        final name = cubit.getLanguageName('ar');
        expect(name, equals('العربية'));
      });

      test('should return "Türkçe" for invalid language code', () {
        final name = cubit.getLanguageName('invalid');
        expect(name, equals('Türkçe'));
      });

      test('should return "Türkçe" for empty language code', () {
        final name = cubit.getLanguageName('');
        expect(name, equals('Türkçe'));
      });

      test('should handle uppercase language codes', () {
        // Note: The method is case-sensitive, so this will return default
        final name = cubit.getLanguageName('TR');
        expect(name, equals('Türkçe')); // Returns default
      });
    });

    // ==================== getLanguageFlag Tests ====================
    group('getLanguageFlag', () {
      setUp(() {
        when(mockPrefs.getString(any)).thenReturn(null);
        cubit = LanguageCubit(mockPrefs);
      });

      test('should return Turkish flag for "tr"', () {
        final flag = cubit.getLanguageFlag('tr');
        expect(flag, equals('🇹🇷'));
      });

      test('should return US flag for "en"', () {
        final flag = cubit.getLanguageFlag('en');
        expect(flag, equals('🇺🇸'));
      });

      test('should return Saudi Arabia flag for "ar"', () {
        final flag = cubit.getLanguageFlag('ar');
        expect(flag, equals('🇸🇦'));
      });

      test('should return Turkish flag for invalid language code', () {
        final flag = cubit.getLanguageFlag('invalid');
        expect(flag, equals('🇹🇷'));
      });

      test('should return Turkish flag for empty language code', () {
        final flag = cubit.getLanguageFlag('');
        expect(flag, equals('🇹🇷'));
      });
    });

    // ==================== LanguageState Tests ====================
    group('LanguageState', () {
      test('should have correct languageCode getter', () {
        const state = LanguageState(locale: Locale('en', 'US'));
        expect(state.languageCode, equals('en'));
      });

      test('should support equality comparison', () {
        const state1 = LanguageState(locale: Locale('tr', 'TR'));
        const state2 = LanguageState(locale: Locale('tr', 'TR'));
        const state3 = LanguageState(locale: Locale('en', 'US'));

        expect(state1, equals(state2));
        expect(state1, isNot(equals(state3)));
      });

      test('props should contain locale', () {
        const state = LanguageState(locale: Locale('tr', 'TR'));
        expect(state.props, contains(const Locale('tr', 'TR')));
        expect(state.props.length, equals(1));
      });

      test('should handle Turkish locale correctly', () {
        const state = LanguageState(locale: Locale('tr', 'TR'));
        expect(state.locale.languageCode, equals('tr'));
        expect(state.locale.countryCode, equals('TR'));
      });

      test('should handle English locale correctly', () {
        const state = LanguageState(locale: Locale('en', 'US'));
        expect(state.locale.languageCode, equals('en'));
        expect(state.locale.countryCode, equals('US'));
      });

      test('should handle Arabic locale correctly', () {
        const state = LanguageState(locale: Locale('ar', 'SA'));
        expect(state.locale.languageCode, equals('ar'));
        expect(state.locale.countryCode, equals('SA'));
      });
    });

    // ==================== Integration Tests ====================
    group('Integration Tests', () {
      setUp(() {
        when(mockPrefs.getString(any)).thenReturn(null);
        when(mockPrefs.setString(any, any)).thenAnswer((_) async => true);
        cubit = LanguageCubit(mockPrefs);
      });

      blocTest<LanguageCubit, LanguageState>(
        'should switch between multiple languages',
        build: () => cubit,
        act: (cubit) async {
          await cubit.changeLanguage(const Locale('en', 'US'));
          await cubit.changeLanguage(const Locale('ar', 'SA'));
          await cubit.changeLanguage(const Locale('tr', 'TR'));
        },
        expect: () => [
          const LanguageState(locale: Locale('en', 'US')),
          const LanguageState(locale: Locale('ar', 'SA')),
          const LanguageState(locale: Locale('tr', 'TR')),
        ],
        verify: (_) {
          verify(mockPrefs.setString('selected_language', 'en')).called(1);
          verify(mockPrefs.setString('selected_language', 'ar')).called(1);
          verify(mockPrefs.setString('selected_language', 'tr')).called(1);
        },
      );

      test('should get correct name and flag for current language', () async {
        await cubit.changeLanguage(const Locale('en', 'US'));

        expect(cubit.getLanguageName('en'), equals('English'));
        expect(cubit.getLanguageFlag('en'), equals('🇺🇸'));
      });

      test('should persist language across multiple changes', () async {
        await cubit.changeLanguage(const Locale('en', 'US'));
        await cubit.changeLanguage(const Locale('ar', 'SA'));

        expect(cubit.state.locale, equals(const Locale('ar', 'SA')));
        verify(mockPrefs.setString('selected_language', 'en')).called(1);
        verify(mockPrefs.setString('selected_language', 'ar')).called(1);
      });
    });

    // ==================== Edge Cases ====================
    group('Edge Cases', () {
      setUp(() {
        when(mockPrefs.getString(any)).thenReturn(null);
        when(mockPrefs.setString(any, any)).thenAnswer((_) async => true);
        cubit = LanguageCubit(mockPrefs);
      });

      blocTest<LanguageCubit, LanguageState>(
        'should handle rapid language changes',
        build: () => cubit,
        act: (cubit) async {
          await cubit.changeLanguageByCode('en');
          await cubit.changeLanguageByCode('ar');
          await cubit.changeLanguageByCode('tr');
          await cubit.changeLanguageByCode('en');
        },
        expect: () => [
          const LanguageState(locale: Locale('en', 'US')),
          const LanguageState(locale: Locale('ar', 'SA')),
          const LanguageState(locale: Locale('tr', 'TR')),
          const LanguageState(locale: Locale('en', 'US')),
        ],
      );

      test('should handle all supported language names', () {
        expect(cubit.getLanguageName('tr'), equals('Türkçe'));
        expect(cubit.getLanguageName('en'), equals('English'));
        expect(cubit.getLanguageName('ar'), equals('العربية'));
      });

      test('should handle all supported language flags', () {
        expect(cubit.getLanguageFlag('tr'), equals('🇹🇷'));
        expect(cubit.getLanguageFlag('en'), equals('🇺🇸'));
        expect(cubit.getLanguageFlag('ar'), equals('🇸🇦'));
      });

      blocTest<LanguageCubit, LanguageState>(
        'should ignore duplicate language changes',
        build: () => cubit,
        act: (cubit) async {
          await cubit.changeLanguage(const Locale('en', 'US'));
          await cubit.changeLanguage(const Locale('en', 'US'));
          await cubit.changeLanguage(const Locale('en', 'US'));
        },
        expect: () => [const LanguageState(locale: Locale('en', 'US'))],
        verify: (_) {
          // Should only save once
          verify(mockPrefs.setString('selected_language', 'en')).called(1);
        },
      );

      test('should handle locale with only language code', () async {
        await cubit.changeLanguage(const Locale('en'));

        expect(cubit.state.locale.languageCode, equals('en'));
      });
    });
  });
}
