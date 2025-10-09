import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:getir_mobile/presentation/widgets/common/language_selector.dart';

void main() {
  group('LanguageSelector Widget Tests -', () {
    testWidgets('renders current language', (tester) async {
      String? selectedLanguage;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: LanguageSelector(
              currentLanguage: 'tr',
              onLanguageChanged: (lang) {
                selectedLanguage = lang;
              },
            ),
          ),
        ),
      );

      expect(find.text('TR'), findsOneWidget);
      expect(find.text('🇹🇷'), findsOneWidget);
    });

    testWidgets('shows popup menu when tapped', (tester) async {
      String? selectedLanguage;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: LanguageSelector(
              currentLanguage: 'en',
              onLanguageChanged: (lang) {
                selectedLanguage = lang;
              },
            ),
          ),
        ),
      );

      // Tap to open menu
      await tester.tap(find.byType(LanguageSelector));
      await tester.pumpAndSettle();

      // Verify all language options are visible
      expect(find.text('Türkçe'), findsOneWidget);
      expect(find.text('English'), findsOneWidget);
      expect(find.text('العربية'), findsOneWidget);
    });

    testWidgets('shows check mark for current language', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: LanguageSelector(
              currentLanguage: 'tr',
              onLanguageChanged: (_) {},
            ),
          ),
        ),
      );

      // Open menu
      await tester.tap(find.byType(LanguageSelector));
      await tester.pumpAndSettle();

      // Find check icon
      expect(find.byIcon(Icons.check), findsOneWidget);
    });

    testWidgets('calls onLanguageChanged when language selected', (
      tester,
    ) async {
      String? selectedLanguage;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: LanguageSelector(
              currentLanguage: 'tr',
              onLanguageChanged: (lang) {
                selectedLanguage = lang;
              },
            ),
          ),
        ),
      );

      // Open menu
      await tester.tap(find.byType(LanguageSelector));
      await tester.pumpAndSettle();

      // Select English
      await tester.tap(find.text('English'));
      await tester.pumpAndSettle();

      expect(selectedLanguage, 'en');
    });

    testWidgets('displays all three language flags', (tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: LanguageSelector(
              currentLanguage: 'tr',
              onLanguageChanged: (_) {},
            ),
          ),
        ),
      );

      // Open menu
      await tester.tap(find.byType(LanguageSelector));
      await tester.pumpAndSettle();

      // Verify flags
      expect(find.text('🇹🇷'), findsWidgets);
      expect(find.text('🇺🇸'), findsOneWidget);
      expect(find.text('🇸🇦'), findsOneWidget);
    });

    testWidgets('handles Arabic language selection', (tester) async {
      String? selectedLanguage;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: LanguageSelector(
              currentLanguage: 'en',
              onLanguageChanged: (lang) {
                selectedLanguage = lang;
              },
            ),
          ),
        ),
      );

      // Open menu
      await tester.tap(find.byType(LanguageSelector));
      await tester.pumpAndSettle();

      // Select Arabic
      await tester.tap(find.text('العربية'));
      await tester.pumpAndSettle();

      expect(selectedLanguage, 'ar');
    });
  });
}
