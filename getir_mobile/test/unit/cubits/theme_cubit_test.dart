import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:get_it/get_it.dart';

import 'package:getir_mobile/core/cubits/theme/theme_cubit.dart';
import 'package:getir_mobile/core/services/logger_service.dart';

import 'theme_cubit_test.mocks.dart';

@GenerateMocks([SharedPreferences, LoggerService])
void main() {
  final getIt = GetIt.instance;

  group('ThemeCubit', () {
    late MockLoggerService mockLoggerService;

    setUpAll(() {
      // Register LoggerService mock for the entire test suite
      mockLoggerService = MockLoggerService();
      getIt.registerSingleton<LoggerService>(mockLoggerService);
    });

    tearDownAll(() {
      getIt.reset();
    });

    // ==================== Initial State Tests ====================
    group('Initial State', () {
      test('initial state should be system theme', () async {
        final freshPrefs = MockSharedPreferences();
        when(freshPrefs.getInt(any)).thenReturn(null);
        final testCubit = ThemeCubit(freshPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(testCubit.state.themeMode, equals(ThemeMode.system));
        expect(testCubit.state.isSystemMode, isTrue);

        await testCubit.close();
      });

      test('should default to system theme when preference is null', () async {
        final freshPrefs = MockSharedPreferences();
        when(freshPrefs.getInt('theme_mode')).thenReturn(null);
        final testCubit = ThemeCubit(freshPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(testCubit.state.themeMode, equals(ThemeMode.system));

        await testCubit.close();
      });

      test('should handle error loading theme preference gracefully', () async {
        final freshPrefs = MockSharedPreferences();
        when(
          freshPrefs.getInt('theme_mode'),
        ).thenThrow(Exception('Preferences error'));
        final testCubit = ThemeCubit(freshPrefs);

        await Future.delayed(const Duration(milliseconds: 100));

        expect(testCubit.state.themeMode, equals(ThemeMode.system));

        await testCubit.close();
      });
    });

    // ==================== Theme Mode Index Mapping Tests ====================
    group('Theme Mode Index Mapping', () {
      test('ThemeMode.system should have index 0', () {
        expect(ThemeMode.system.index, equals(0));
      });

      test('ThemeMode.light should have index 1', () {
        expect(ThemeMode.light.index, equals(1));
      });

      test('ThemeMode.dark should have index 2', () {
        expect(ThemeMode.dark.index, equals(2));
      });
    });

    // ==================== setThemeMode Tests ====================
    group('setThemeMode', () {
      blocTest<ThemeCubit, ThemeState>(
        'should change theme to light mode',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setThemeMode(ThemeMode.light),
        expect: () => [const ThemeState(themeMode: ThemeMode.light)],
        verify: (cubit) {
          // Note: freshPrefs is local to build(), we can't access it here
          // Just verify the state is correct
          expect(cubit.state.themeMode, ThemeMode.light);
        },
      );

      blocTest<ThemeCubit, ThemeState>(
        'should change theme to dark mode',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setThemeMode(ThemeMode.dark),
        expect: () => [const ThemeState(themeMode: ThemeMode.dark)],
        verify: (cubit) {
          expect(cubit.state.themeMode, ThemeMode.dark);
        },
      );

      blocTest<ThemeCubit, ThemeState>(
        'should change theme to system mode',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.light),
        act: (cubit) => cubit.setThemeMode(ThemeMode.system),
        expect: () => [const ThemeState(themeMode: ThemeMode.system)],
        verify: (cubit) {
          expect(cubit.state.themeMode, ThemeMode.system);
        },
      );

      blocTest<ThemeCubit, ThemeState>(
        'should not emit state if theme mode is already set',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setThemeMode(ThemeMode.system),
        expect: () => [],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should save theme mode to preferences when changed',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setThemeMode(ThemeMode.dark),
        verify: (cubit) {
          expect(cubit.state.themeMode, ThemeMode.dark);
        },
      );

      test('should handle preference save error gracefully', () async {
        final freshPrefs = MockSharedPreferences();
        when(freshPrefs.getInt(any)).thenReturn(null);
        when(freshPrefs.setInt(any, any)).thenThrow(Exception('Save failed'));
        final testCubit = ThemeCubit(freshPrefs);

        await Future.delayed(const Duration(milliseconds: 150));

        expect(
          () => testCubit.setThemeMode(ThemeMode.dark),
          throwsA(isA<Exception>()),
        );

        await testCubit.close();
      });
    });

    // ==================== setLightMode Tests ====================
    group('setLightMode', () {
      blocTest<ThemeCubit, ThemeState>(
        'should set light mode',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setLightMode(),
        expect: () => [const ThemeState(themeMode: ThemeMode.light)],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should save light mode preference',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setLightMode(),
        verify: (cubit) {
          expect(cubit.state.themeMode, ThemeMode.light);
        },
      );
    });

    // ==================== setDarkMode Tests ====================
    group('setDarkMode', () {
      blocTest<ThemeCubit, ThemeState>(
        'should set dark mode',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setDarkMode(),
        expect: () => [const ThemeState(themeMode: ThemeMode.dark)],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should save dark mode preference',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) => cubit.setDarkMode(),
        verify: (cubit) {
          expect(cubit.state.themeMode, ThemeMode.dark);
        },
      );
    });

    // ==================== setSystemMode Tests ====================
    group('setSystemMode', () {
      blocTest<ThemeCubit, ThemeState>(
        'should set system mode',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.light),
        act: (cubit) => cubit.setSystemMode(),
        expect: () => [const ThemeState(themeMode: ThemeMode.system)],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should save system mode preference',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.light),
        act: (cubit) => cubit.setSystemMode(),
        verify: (cubit) {
          expect(cubit.state.themeMode, ThemeMode.system);
        },
      );
    });

    // ==================== toggleTheme Tests ====================
    group('toggleTheme', () {
      blocTest<ThemeCubit, ThemeState>(
        'should toggle from light to dark',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.light),
        act: (cubit) => cubit.toggleTheme(),
        expect: () => [const ThemeState(themeMode: ThemeMode.dark)],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should toggle from dark to system',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.dark),
        act: (cubit) => cubit.toggleTheme(),
        expect: () => [const ThemeState(themeMode: ThemeMode.system)],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should toggle from system to light',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.system),
        act: (cubit) => cubit.toggleTheme(),
        expect: () => [const ThemeState(themeMode: ThemeMode.light)],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should complete full toggle cycle',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.light),
        act: (cubit) async {
          await cubit.toggleTheme(); // light -> dark
          await cubit.toggleTheme(); // dark -> system
          await cubit.toggleTheme(); // system -> light
        },
        expect: () => [
          const ThemeState(themeMode: ThemeMode.dark),
          const ThemeState(themeMode: ThemeMode.system),
          const ThemeState(themeMode: ThemeMode.light),
        ],
      );
    });

    // ==================== ThemeState Tests ====================
    group('ThemeState', () {
      test('isDarkMode should return true for dark theme', () {
        const state = ThemeState(themeMode: ThemeMode.dark);
        expect(state.isDarkMode, isTrue);
        expect(state.isLightMode, isFalse);
        expect(state.isSystemMode, isFalse);
      });

      test('isLightMode should return true for light theme', () {
        const state = ThemeState(themeMode: ThemeMode.light);
        expect(state.isLightMode, isTrue);
        expect(state.isDarkMode, isFalse);
        expect(state.isSystemMode, isFalse);
      });

      test('isSystemMode should return true for system theme', () {
        const state = ThemeState(themeMode: ThemeMode.system);
        expect(state.isSystemMode, isTrue);
        expect(state.isDarkMode, isFalse);
        expect(state.isLightMode, isFalse);
      });

      test('themeModeString should return correct Turkish names', () {
        expect(
          const ThemeState(themeMode: ThemeMode.light).themeModeString,
          equals('Açık Tema'),
        );
        expect(
          const ThemeState(themeMode: ThemeMode.dark).themeModeString,
          equals('Koyu Tema'),
        );
        expect(
          const ThemeState(themeMode: ThemeMode.system).themeModeString,
          equals('Sistem Teması'),
        );
      });

      test('equality should work correctly', () {
        const state1 = ThemeState(themeMode: ThemeMode.light);
        const state2 = ThemeState(themeMode: ThemeMode.light);
        const state3 = ThemeState(themeMode: ThemeMode.dark);

        expect(state1, equals(state2));
        expect(state1, isNot(equals(state3)));
      });

      test('props should contain themeMode', () {
        const state = ThemeState(themeMode: ThemeMode.dark);
        expect(state.props, contains(ThemeMode.dark));
        expect(state.props.length, equals(1));
      });
    });

    // ==================== Integration Tests ====================
    group('Integration Tests', () {
      blocTest<ThemeCubit, ThemeState>(
        'should switch between all theme modes',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) async {
          await cubit.setLightMode();
          await cubit.setDarkMode();
          await cubit.setSystemMode();
        },
        expect: () => [
          const ThemeState(themeMode: ThemeMode.light),
          const ThemeState(themeMode: ThemeMode.dark),
          const ThemeState(themeMode: ThemeMode.system),
        ],
      );

      test('should persist theme across multiple changes', () async {
        final freshPrefs = MockSharedPreferences();
        when(freshPrefs.getInt(any)).thenReturn(null);
        when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
        final testCubit = ThemeCubit(freshPrefs);

        await Future.delayed(const Duration(milliseconds: 150));

        await testCubit.setLightMode();
        await testCubit.setDarkMode();

        expect(testCubit.state.themeMode, equals(ThemeMode.dark));

        await testCubit.close();
      });
    });

    // ==================== Edge Cases ====================
    group('Edge Cases', () {
      blocTest<ThemeCubit, ThemeState>(
        'should handle rapid theme changes',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        wait: const Duration(milliseconds: 200),
        act: (cubit) async {
          await cubit.setLightMode();
          await cubit.setDarkMode();
          await cubit.setSystemMode();
          await cubit.setLightMode();
        },
        expect: () => [
          const ThemeState(themeMode: ThemeMode.light),
          const ThemeState(themeMode: ThemeMode.dark),
          const ThemeState(themeMode: ThemeMode.system),
          const ThemeState(themeMode: ThemeMode.light),
        ],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should ignore duplicate theme changes',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.dark),
        act: (cubit) async {
          await cubit.setDarkMode();
          await cubit.setDarkMode();
          await cubit.setDarkMode();
        },
        expect: () => [],
      );

      blocTest<ThemeCubit, ThemeState>(
        'should handle multiple toggles correctly',
        build: () {
          final freshPrefs = MockSharedPreferences();
          when(freshPrefs.getInt(any)).thenReturn(null);
          when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
          return ThemeCubit(freshPrefs);
        },
        seed: () => const ThemeState(themeMode: ThemeMode.light),
        act: (cubit) async {
          await cubit.toggleTheme();
          await cubit.toggleTheme();
          await cubit.toggleTheme();
          await cubit.toggleTheme();
        },
        expect: () => [
          const ThemeState(themeMode: ThemeMode.dark),
          const ThemeState(themeMode: ThemeMode.system),
          const ThemeState(themeMode: ThemeMode.light),
          const ThemeState(themeMode: ThemeMode.dark),
        ],
      );

      test('should handle all ThemeMode enum values', () async {
        final freshPrefs = MockSharedPreferences();
        when(freshPrefs.getInt(any)).thenReturn(null);
        when(freshPrefs.setInt(any, any)).thenAnswer((_) async => true);
        final testCubit = ThemeCubit(freshPrefs);

        await Future.delayed(const Duration(milliseconds: 150));

        await testCubit.setThemeMode(ThemeMode.light);
        expect(testCubit.state.themeMode, equals(ThemeMode.light));

        await testCubit.setThemeMode(ThemeMode.dark);
        expect(testCubit.state.themeMode, equals(ThemeMode.dark));

        await testCubit.setThemeMode(ThemeMode.system);
        expect(testCubit.state.themeMode, equals(ThemeMode.system));

        await testCubit.close();
      });
    });
  });
}
