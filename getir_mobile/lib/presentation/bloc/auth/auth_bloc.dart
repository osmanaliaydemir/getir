import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:injectable/injectable.dart';
import '../../../domain/entities/user_entity.dart';
import '../../../domain/usecases/auth_usecases.dart';
import '../../../core/services/analytics_service.dart';

// Events
abstract class AuthEvent extends Equatable {
  const AuthEvent();

  @override
  List<Object?> get props => [];
}

class AuthLoginRequested extends AuthEvent {
  final String email;
  final String password;

  const AuthLoginRequested({required this.email, required this.password});

  @override
  List<Object> get props => [email, password];
}

class AuthRegisterRequested extends AuthEvent {
  final String email;
  final String password;
  final String firstName;
  final String lastName;
  final String? phoneNumber;

  const AuthRegisterRequested({
    required this.email,
    required this.password,
    required this.firstName,
    required this.lastName,
    this.phoneNumber,
  });

  @override
  List<Object?> get props => [
    email,
    password,
    firstName,
    lastName,
    phoneNumber,
  ];
}

class AuthLogoutRequested extends AuthEvent {}

class AuthRefreshTokenRequested extends AuthEvent {
  final String refreshToken;

  const AuthRefreshTokenRequested(this.refreshToken);

  @override
  List<Object> get props => [refreshToken];
}

class AuthForgotPasswordRequested extends AuthEvent {
  final String email;

  const AuthForgotPasswordRequested(this.email);

  @override
  List<Object> get props => [email];
}

class AuthResetPasswordRequested extends AuthEvent {
  final String token;
  final String newPassword;

  const AuthResetPasswordRequested({
    required this.token,
    required this.newPassword,
  });

  @override
  List<Object> get props => [token, newPassword];
}

class AuthCheckAuthenticationRequested extends AuthEvent {}

class AuthCheckTokenValidityRequested extends AuthEvent {}

// States
abstract class AuthState extends Equatable {
  const AuthState();

  @override
  List<Object?> get props => [];
}

class AuthInitial extends AuthState {}

class AuthLoading extends AuthState {}

class AuthAuthenticated extends AuthState {
  final UserEntity user;

  const AuthAuthenticated(this.user);

  @override
  List<Object> get props => [user];
}

class AuthUnauthenticated extends AuthState {}

class AuthError extends AuthState {
  final String message;

  const AuthError(this.message);

  @override
  List<Object> get props => [message];
}

class AuthPasswordResetSent extends AuthState {
  final String email;

  const AuthPasswordResetSent(this.email);

  @override
  List<Object> get props => [email];
}

class AuthPasswordResetSuccess extends AuthState {}

// BLoC
@injectable
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final LoginUseCase _loginUseCase;
  final RegisterUseCase _registerUseCase;
  final LogoutUseCase _logoutUseCase;
  final RefreshTokenUseCase _refreshTokenUseCase;
  final ForgotPasswordUseCase _forgotPasswordUseCase;
  final ResetPasswordUseCase _resetPasswordUseCase;
  final GetCurrentUserUseCase _getCurrentUserUseCase;
  final CheckAuthenticationUseCase _checkAuthenticationUseCase;
  final CheckTokenValidityUseCase _checkTokenValidityUseCase;
  final AnalyticsService _analytics;

  AuthBloc(
    this._loginUseCase,
    this._registerUseCase,
    this._logoutUseCase,
    this._refreshTokenUseCase,
    this._forgotPasswordUseCase,
    this._resetPasswordUseCase,
    this._getCurrentUserUseCase,
    this._checkAuthenticationUseCase,
    this._checkTokenValidityUseCase,
    this._analytics,
  ) : super(AuthInitial()) {
    on<AuthLoginRequested>(_onLoginRequested);
    on<AuthRegisterRequested>(_onRegisterRequested);
    on<AuthLogoutRequested>(_onLogoutRequested);
    on<AuthRefreshTokenRequested>(_onRefreshTokenRequested);
    on<AuthForgotPasswordRequested>(_onForgotPasswordRequested);
    on<AuthResetPasswordRequested>(_onResetPasswordRequested);
    on<AuthCheckAuthenticationRequested>(_onCheckAuthenticationRequested);
    on<AuthCheckTokenValidityRequested>(_onCheckTokenValidityRequested);
  }

  Future<void> _onLoginRequested(
    AuthLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    try {
      final user = await _loginUseCase(event.email, event.password);

      // 📊 Analytics: Track login
      await _analytics.logLogin(method: 'email');
      await _analytics.setUserId(user.id);

      emit(AuthAuthenticated(user));
      // ✅ Cart merge logic moved to UI layer (login_page.dart)
    } catch (e) {
      emit(AuthError(e.toString()));
      await _analytics.logError(error: e, reason: 'Login failed');
    }
  }

  Future<void> _onRegisterRequested(
    AuthRegisterRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    try {
      final user = await _registerUseCase(
        event.email,
        event.password,
        event.firstName,
        event.lastName,
        phoneNumber: event.phoneNumber,
      );

      // 📊 Analytics: Track sign up
      await _analytics.logSignUp(method: 'email');
      await _analytics.setUserId(user.id);

      emit(AuthAuthenticated(user));
    } catch (e) {
      emit(AuthError(e.toString()));
      await _analytics.logError(error: e, reason: 'Registration failed');
    }
  }

  Future<void> _onLogoutRequested(
    AuthLogoutRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    try {
      await _logoutUseCase();

      // 📊 Analytics: Track logout
      await _analytics.logLogout();
      await _analytics.setUserId(null);

      emit(AuthUnauthenticated());
    } catch (e) {
      emit(AuthError(e.toString()));
      await _analytics.logError(error: e, reason: 'Logout failed');
    }
  }

  Future<void> _onRefreshTokenRequested(
    AuthRefreshTokenRequested event,
    Emitter<AuthState> emit,
  ) async {
    try {
      final user = await _refreshTokenUseCase(event.refreshToken);
      emit(AuthAuthenticated(user));
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  Future<void> _onForgotPasswordRequested(
    AuthForgotPasswordRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    try {
      await _forgotPasswordUseCase(event.email);
      emit(AuthPasswordResetSent(event.email));
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  Future<void> _onResetPasswordRequested(
    AuthResetPasswordRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    try {
      await _resetPasswordUseCase(event.token, event.newPassword);
      emit(AuthPasswordResetSuccess());
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  Future<void> _onCheckAuthenticationRequested(
    AuthCheckAuthenticationRequested event,
    Emitter<AuthState> emit,
  ) async {
    try {
      final isAuthenticated = await _checkAuthenticationUseCase();

      if (isAuthenticated) {
        final user = await _getCurrentUserUseCase();
        if (user != null) {
          emit(AuthAuthenticated(user));
        } else {
          emit(AuthUnauthenticated());
        }
      } else {
        emit(AuthUnauthenticated());
      }
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }

  Future<void> _onCheckTokenValidityRequested(
    AuthCheckTokenValidityRequested event,
    Emitter<AuthState> emit,
  ) async {
    try {
      final isTokenValid = await _checkTokenValidityUseCase();

      if (isTokenValid) {
        final user = await _getCurrentUserUseCase();
        if (user != null) {
          emit(AuthAuthenticated(user));
        } else {
          emit(AuthUnauthenticated());
        }
      } else {
        emit(AuthUnauthenticated());
      }
    } catch (e) {
      emit(AuthError(e.toString()));
    }
  }
}
