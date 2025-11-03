import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/bloc/auth/auth_bloc.dart';
import 'package:getir_mobile/domain/services/auth_service.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([AuthService, AnalyticsService])
import 'auth_bloc_test.mocks.dart';

void main() {
  late AuthBloc bloc;
  late MockAuthService mockAuthService;
  late MockAnalyticsService mockAnalytics;

  setUp(() {
    mockAuthService = MockAuthService();
    mockAnalytics = MockAnalyticsService();
    bloc = AuthBloc(mockAuthService, mockAnalytics);
  });

  group('AuthBloc -', () {
    test('initial state is AuthInitial', () {
      expect(bloc.state, AuthInitial());
    });

    blocTest<AuthBloc, AuthState>(
      'Login emits [AuthLoading, AuthAuthenticated] when login succeeds',
      build: () {
        when(
          mockAuthService.login(any, any),
        ).thenAnswer((_) async => Result.success(MockData.testUser));
        when(mockAnalytics.logLogin(method: anyNamed('method'))).thenAnswer((_) async {});
        when(mockAnalytics.setUserId(any)).thenAnswer((_) async {});
        return bloc;
      },
      act: (bloc) => bloc.add(
        const AuthLoginRequested(
          email: 'test@test.com',
          password: 'password123',
        ),
      ),
      expect: () => [AuthLoading(), AuthAuthenticated(MockData.testUser)],
    );

    blocTest<AuthBloc, AuthState>(
      'Login emits [AuthLoading, AuthError] when login fails',
      build: () {
        when(mockAuthService.login(any, any)).thenAnswer(
          (_) async => Result.failure(const UnauthorizedException()),
        );
        when(mockAnalytics.logError(error: any, reason: anyNamed('reason'))).thenAnswer((_) async {});
        return bloc;
      },
      act: (bloc) => bloc.add(
        const AuthLoginRequested(
          email: 'test@test.com',
          password: 'wrong_password',
        ),
      ),
      expect: () => [AuthLoading(), isA<AuthError>()],
    );

    blocTest<AuthBloc, AuthState>(
      'Register emits [AuthLoading, AuthAuthenticated] when register succeeds',
      build: () {
        when(
          mockAuthService.register(
            email: anyNamed('email'),
            password: anyNamed('password'),
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
          ),
        ).thenAnswer((_) async => Result.success(MockData.testUser));
        when(mockAnalytics.logSignUp(method: anyNamed('method'))).thenAnswer((_) async {});
        when(mockAnalytics.setUserId(any)).thenAnswer((_) async {});
        return bloc;
      },
      act: (bloc) => bloc.add(
        const AuthRegisterRequested(
          email: 'new@test.com',
          password: 'password123',
          firstName: 'John',
          lastName: 'Doe',
        ),
      ),
      expect: () => [AuthLoading(), AuthAuthenticated(MockData.testUser)],
    );

    blocTest<AuthBloc, AuthState>(
      'Logout emits [AuthLoading, AuthUnauthenticated] when logout succeeds',
      build: () {
        when(mockAuthService.logout()).thenAnswer((_) async => Result.success(null));
        when(mockAnalytics.logLogout()).thenAnswer((_) async {});
        when(mockAnalytics.setUserId(any)).thenAnswer((_) async {});
        return bloc;
      },
      act: (bloc) => bloc.add(AuthLogoutRequested()),
      expect: () => [AuthLoading(), AuthUnauthenticated()],
    );

    blocTest<AuthBloc, AuthState>(
      'RefreshToken emits [AuthLoading, AuthAuthenticated] when refresh succeeds',
      build: () {
        when(
          mockAuthService.refreshToken(),
        ).thenAnswer((_) async => Result.success(MockData.testUser));
        return bloc;
      },
      act: (bloc) =>
          bloc.add(const AuthRefreshTokenRequested('refresh_token_123')),
      expect: () => [AuthLoading(), AuthAuthenticated(MockData.testUser)],
    );

    blocTest<AuthBloc, AuthState>(
      'ForgotPassword emits [AuthLoading, AuthPasswordResetSent] when email sent',
      build: () {
        when(
          mockAuthService.forgotPassword(any),
        ).thenAnswer((_) async => Result.success(null));
        return bloc;
      },
      act: (bloc) =>
          bloc.add(const AuthForgotPasswordRequested('test@test.com')),
      expect: () => [
        AuthLoading(),
        const AuthPasswordResetSent('test@test.com'),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'ResetPassword emits [AuthLoading, AuthPasswordResetSuccess] when reset succeeds',
      build: () {
        when(
          mockAuthService.resetPassword(
            email: anyNamed('email'),
            resetCode: anyNamed('resetCode'),
            newPassword: anyNamed('newPassword'),
          ),
        ).thenAnswer((_) async => Result.success(null));
        return bloc;
      },
      act: (bloc) => bloc.add(
        const AuthResetPasswordRequested(
          email: 'test@test.com',
          token: 'reset_token',
          newPassword: 'newpassword123',
        ),
      ),
      expect: () => [AuthLoading(), AuthPasswordResetSuccess()],
    );

    blocTest<AuthBloc, AuthState>(
      'CheckAuthentication emits [AuthAuthenticated] when user is authenticated',
      build: () {
        when(
          mockAuthService.checkAuthentication(),
        ).thenAnswer((_) async => Result.success(true));
        when(
          mockAuthService.getCurrentUser(),
        ).thenAnswer((_) async => Result.success(MockData.testUser));
        return bloc;
      },
      act: (bloc) => bloc.add(AuthCheckAuthenticationRequested()),
      expect: () => [AuthAuthenticated(MockData.testUser)],
    );

    blocTest<AuthBloc, AuthState>(
      'CheckAuthentication emits [AuthUnauthenticated] when no user found',
      build: () {
        when(
          mockAuthService.checkAuthentication(),
        ).thenAnswer((_) async => Result.success(false));
        return bloc;
      },
      act: (bloc) => bloc.add(AuthCheckAuthenticationRequested()),
      expect: () => [AuthUnauthenticated()],
    );
  });
}
