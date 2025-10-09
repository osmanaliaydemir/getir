import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/cubits/theme/theme_cubit.dart';
import 'package:getir_mobile/presentation/widgets/common/theme_switcher.dart';

@GenerateMocks([ThemeCubit])
import 'theme_switcher_widget_test.mocks.dart';

void main() {
  late MockThemeCubit mockThemeCubit;

  setUp(() {
    mockThemeCubit = MockThemeCubit();
    when(
      mockThemeCubit.state,
    ).thenReturn(const ThemeState(themeMode: ThemeMode.light));
    when(mockThemeCubit.stream).thenAnswer(
      (_) => Stream.value(const ThemeState(themeMode: ThemeMode.light)),
    );
  });

  group('ThemeSwitcher Widget Tests -', () {
    testWidgets('renders all three theme options', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeSwitcher()),
          ),
        ),
      );

      expect(find.text('Açık Tema'), findsOneWidget);
      expect(find.text('Koyu Tema'), findsOneWidget);
      expect(find.text('Sistem Teması'), findsOneWidget);
    });

    testWidgets('displays correct icons for each theme', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeSwitcher()),
          ),
        ),
      );

      expect(find.byIcon(Icons.light_mode), findsOneWidget);
      expect(find.byIcon(Icons.dark_mode), findsOneWidget);
      expect(find.byIcon(Icons.brightness_auto), findsOneWidget);
    });

    testWidgets('selects light theme when tapped', (tester) async {
      when(
        mockThemeCubit.state,
      ).thenReturn(const ThemeState(themeMode: ThemeMode.dark));
      when(mockThemeCubit.stream).thenAnswer(
        (_) => Stream.value(const ThemeState(themeMode: ThemeMode.dark)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeSwitcher()),
          ),
        ),
      );

      // Tap on light theme
      await tester.tap(find.text('Açık Tema'));
      await tester.pumpAndSettle();

      verify(mockThemeCubit.setThemeMode(ThemeMode.light)).called(1);
    });

    testWidgets('selects dark theme when tapped', (tester) async {
      when(
        mockThemeCubit.state,
      ).thenReturn(const ThemeState(themeMode: ThemeMode.light));
      when(mockThemeCubit.stream).thenAnswer(
        (_) => Stream.value(const ThemeState(themeMode: ThemeMode.light)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeSwitcher()),
          ),
        ),
      );

      // Tap on dark theme
      await tester.tap(find.text('Koyu Tema'));
      await tester.pumpAndSettle();

      verify(mockThemeCubit.setThemeMode(ThemeMode.dark)).called(1);
    });

    testWidgets('selects system theme when tapped', (tester) async {
      when(
        mockThemeCubit.state,
      ).thenReturn(const ThemeState(themeMode: ThemeMode.light));
      when(mockThemeCubit.stream).thenAnswer(
        (_) => Stream.value(const ThemeState(themeMode: ThemeMode.light)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeSwitcher()),
          ),
        ),
      );

      // Tap on system theme
      await tester.tap(find.text('Sistem Teması'));
      await tester.pumpAndSettle();

      verify(mockThemeCubit.setThemeMode(ThemeMode.system)).called(1);
    });

    testWidgets('highlights currently selected theme', (tester) async {
      when(
        mockThemeCubit.state,
      ).thenReturn(const ThemeState(themeMode: ThemeMode.light));
      when(mockThemeCubit.stream).thenAnswer(
        (_) => Stream.value(const ThemeState(themeMode: ThemeMode.light)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeSwitcher()),
          ),
        ),
      );

      // RadioListTile for light theme should be selected
      final radioTiles = tester.widgetList<RadioListTile<ThemeMode>>(
        find.byType(RadioListTile<ThemeMode>),
      );

      expect(radioTiles.first.value, ThemeMode.light);
    });
  });

  group('ThemeToggleButton Widget Tests -', () {
    testWidgets('renders IconButton', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeToggleButton()),
          ),
        ),
      );

      expect(find.byType(IconButton), findsOneWidget);
    });

    testWidgets('shows dark mode icon in light theme', (tester) async {
      when(
        mockThemeCubit.state,
      ).thenReturn(const ThemeState(themeMode: ThemeMode.light));
      when(mockThemeCubit.stream).thenAnswer(
        (_) => Stream.value(const ThemeState(themeMode: ThemeMode.light)),
      );

      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<ThemeCubit>.value(
            value: mockThemeCubit,
            child: const Scaffold(body: ThemeToggleButton()),
          ),
        ),
      );

      await tester.pumpAndSettle();
      expect(find.byIcon(Icons.dark_mode), findsOneWidget);
    });
  });
}
