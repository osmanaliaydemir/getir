import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/bloc/auth/auth_bloc.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';
import '../helpers/mock_data.dart';

@GenerateMocks([AuthBloc])
import 'auth_flow_test.mocks.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  late MockAuthBloc mockAuthBloc;

  setUp(() {
    mockAuthBloc = MockAuthBloc();
  });

  group('Auth Flow Integration Tests -', () {
    testWidgets('auth initial state', (tester) async {
      // Arrange
      when(mockAuthBloc.state).thenReturn(AuthInitial());
      when(mockAuthBloc.stream).thenAnswer((_) => Stream.value(AuthInitial()));

      // Act
      await tester.pumpWidget(
        MaterialApp(
          localizationsDelegates: AppLocalizations.localizationsDelegates,
          supportedLocales: AppLocalizations.supportedLocales,
          home: BlocProvider<AuthBloc>.value(
            value: mockAuthBloc,
            child: const Scaffold(body: Center(child: Text('Auth Screen'))),
          ),
        ),
      );
      await tester.pump();

      // Assert
      expect(find.text('Auth Screen'), findsOneWidget);
    });

    testWidgets('authenticated user state', (tester) async {
      // Arrange
      final authenticatedState = AuthAuthenticated(MockData.testUser);
      when(mockAuthBloc.state).thenReturn(authenticatedState);
      when(
        mockAuthBloc.stream,
      ).thenAnswer((_) => Stream.value(authenticatedState));

      // Act
      await tester.pumpWidget(
        MaterialApp(
          localizationsDelegates: AppLocalizations.localizationsDelegates,
          supportedLocales: AppLocalizations.supportedLocales,
          home: BlocProvider<AuthBloc>.value(
            value: mockAuthBloc,
            child: Scaffold(
              body: Center(
                child: Text('Welcome ${MockData.testUser.firstName}'),
              ),
            ),
          ),
        ),
      );
      await tester.pump();

      // Assert
      expect(find.textContaining('Welcome'), findsOneWidget);
    });

    testWidgets('unauthenticated state shows login', (tester) async {
      // Arrange
      when(mockAuthBloc.state).thenReturn(AuthUnauthenticated());
      when(
        mockAuthBloc.stream,
      ).thenAnswer((_) => Stream.value(AuthUnauthenticated()));

      // Act
      await tester.pumpWidget(
        MaterialApp(
          localizationsDelegates: AppLocalizations.localizationsDelegates,
          supportedLocales: AppLocalizations.supportedLocales,
          home: BlocProvider<AuthBloc>.value(
            value: mockAuthBloc,
            child: const Scaffold(body: Center(child: Text('Please Login'))),
          ),
        ),
      );
      await tester.pump();

      // Assert
      expect(find.text('Please Login'), findsOneWidget);
    });
  });
}
