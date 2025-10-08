import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/presentation/bloc/auth/auth_bloc.dart';
import 'package:getir_mobile/domain/usecases/auth_usecases.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([
  LoginUseCase,
  RegisterUseCase,
  LogoutUseCase,
  RefreshTokenUseCase,
  GetCurrentUserUseCase,
  ForgotPasswordUseCase,
  ResetPasswordUseCase,
  CheckAuthenticationUseCase,
  CheckTokenValidityUseCase,
  AnalyticsService,
])
import 'auth_bloc_test.mocks.dart';

void main() {
  late AuthBloc authBloc;
  late MockLoginUseCase mockLoginUseCase;
  late MockRegisterUseCase mockRegisterUseCase;
  late MockLogoutUseCase mockLogoutUseCase;
  late MockGetCurrentUserUseCase mockGetCurrentUserUseCase;
  late MockAnalyticsService mockAnalyticsService;

  setUp(() {
    mockLoginUseCase = MockLoginUseCase();
    mockRegisterUseCase = MockRegisterUseCase();
    mockLogoutUseCase = MockLogoutUseCase();
    mockGetCurrentUserUseCase = MockGetCurrentUserUseCase();
    mockAnalyticsService = MockAnalyticsService();

    // Stub analytics methods to prevent errors
    when(mockAnalyticsService.logLogin(method: anyNamed('method')))
        .thenAnswer((_) async => {});
    when(mockAnalyticsService.logSignUp(method: anyNamed('method')))
        .thenAnswer((_) async => {});
    when(mockAnalyticsService.logLogout()).thenAnswer((_) async => {});
    when(mockAnalyticsService.setUserId(any)).thenAnswer((_) async => {});
    when(
      mockAnalyticsService.logError(
        error: anyNamed('error'),
        stackTrace: anyNamed('stackTrace'),
        reason: anyNamed('reason'),
        context: anyNamed('context'),
        fatal: anyNamed('fatal'),
      ),
    ).thenAnswer((_) async => {});

    authBloc = AuthBloc(
      mockLoginUseCase,
      mockRegisterUseCase,
      mockLogoutUseCase,
      MockRefreshTokenUseCase(),
      MockForgotPasswordUseCase(),
      MockResetPasswordUseCase(),
      mockGetCurrentUserUseCase,
      MockCheckAuthenticationUseCase(),
      MockCheckTokenValidityUseCase(),
      mockAnalyticsService,
    );
  });

  tearDown(() {
    authBloc.close();
  });

  group('AuthBloc Tests', () {
    final testUser = MockData.testUser;
    const testEmail = 'test@getir.com';
    const testPassword = 'Test123456';

    test('initial state should be AuthInitial', () {
      expect(authBloc.state, equals(AuthInitial()));
    });

    group('Login', () {
      blocTest<AuthBloc, AuthState>(
        'emits [AuthLoading, AuthAuthenticated] when login succeeds',
        build: () {
          when(mockLoginUseCase(any, any)).thenAnswer((_) async => testUser);
          return authBloc;
        },
        act: (bloc) => bloc.add(
          const AuthLoginRequested(email: testEmail, password: testPassword),
        ),
        expect: () => [AuthLoading(), AuthAuthenticated(testUser)],
        verify: (_) {
          verify(mockLoginUseCase(testEmail, testPassword)).called(1);
        },
      );

      blocTest<AuthBloc, AuthState>(
        'emits [AuthLoading, AuthError] when login fails',
        build: () {
          when(
            mockLoginUseCase(any, any),
          ).thenThrow(Exception('Invalid credentials'));
          return authBloc;
        },
        act: (bloc) => bloc.add(
          const AuthLoginRequested(email: testEmail, password: testPassword),
        ),
        expect: () => [
          AuthLoading(),
          isA<AuthError>().having(
            (s) => s.message,
            'message',
            contains('Invalid credentials'),
          ),
        ],
      );
    });

    group('Register', () {
      const firstName = 'John';
      const lastName = 'Doe';

      blocTest<AuthBloc, AuthState>(
        'emits [AuthLoading, AuthAuthenticated] when registration succeeds',
        build: () {
          when(
            mockRegisterUseCase(
              any,
              any,
              any,
              any,
              phoneNumber: anyNamed('phoneNumber'),
            ),
          ).thenAnswer((_) async => testUser);
          return authBloc;
        },
        act: (bloc) => bloc.add(
          const AuthRegisterRequested(
            email: testEmail,
            password: testPassword,
            firstName: firstName,
            lastName: lastName,
          ),
        ),
        expect: () => [AuthLoading(), AuthAuthenticated(testUser)],
      );
    });

    group('Logout', () {
      blocTest<AuthBloc, AuthState>(
        'emits [AuthLoading, AuthUnauthenticated] when logout succeeds',
        build: () {
          when(mockLogoutUseCase()).thenAnswer((_) async => {});
          return authBloc;
        },
        seed: () => AuthAuthenticated(testUser),
        act: (bloc) => bloc.add(AuthLogoutRequested()),
        expect: () => [AuthLoading(), AuthUnauthenticated()],
      );
    });

    group('Check Auth Status', () {
      blocTest<AuthBloc, AuthState>(
        'emits [AuthLoading, AuthAuthenticated] when user is logged in',
        build: () {
          when(mockGetCurrentUserUseCase()).thenAnswer((_) async => testUser);
          return authBloc;
        },
        act: (bloc) => bloc.add(AuthCheckAuthenticationRequested()),
        expect: () => [AuthLoading(), AuthAuthenticated(testUser)],
      );

      blocTest<AuthBloc, AuthState>(
        'emits [AuthLoading, AuthUnauthenticated] when no user is logged in',
        build: () {
          when(mockGetCurrentUserUseCase()).thenAnswer((_) async => null);
          return authBloc;
        },
        act: (bloc) => bloc.add(AuthCheckAuthenticationRequested()),
        expect: () => [AuthLoading(), AuthUnauthenticated()],
      );
    });
  });
}
