import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/user_entity.dart';
import '../../../domain/services/auth_service.dart';
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
  final String email;
  final String token;
  final String newPassword;

  const AuthResetPasswordRequested({
    required this.email,
    required this.token,
    required this.newPassword,
  });

  @override
  List<Object> get props => [email, token, newPassword];
}

class AuthChangePasswordRequested extends AuthEvent {
  final String currentPassword;
  final String newPassword;

  const AuthChangePasswordRequested({
    required this.currentPassword,
    required this.newPassword,
  });

  @override
  List<Object> get props => [currentPassword, newPassword];
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

class AuthPasswordChangeSuccess extends AuthState {}

// BLoC
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final AuthService _authService;
  final AnalyticsService _analytics;

  AuthBloc(this._authService, this._analytics) : super(AuthInitial()) {
    on<AuthLoginRequested>(_onLoginRequested);
    on<AuthRegisterRequested>(_onRegisterRequested);
    on<AuthLogoutRequested>(_onLogoutRequested);
    on<AuthRefreshTokenRequested>(_onRefreshTokenRequested);
    on<AuthForgotPasswordRequested>(_onForgotPasswordRequested);
    on<AuthResetPasswordRequested>(_onResetPasswordRequested);
    on<AuthChangePasswordRequested>(_onChangePasswordRequested);
    on<AuthCheckAuthenticationRequested>(_onCheckAuthenticationRequested);
    on<AuthCheckTokenValidityRequested>(_onCheckTokenValidityRequested);
  }

  Future<void> _onLoginRequested(
    AuthLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.login(event.email, event.password);

    await result.when(
      success: (user) async {
        // 📊 Analytics: Track login
        await _analytics.logLogin(method: 'email');
        await _analytics.setUserId(user.id);

        if (!emit.isDone) {
          emit(AuthAuthenticated(user));
        }
        // ✅ Cart merge logic moved to UI layer (login_page.dart)
      },
      failure: (exception) async {
        final message = _getErrorMessage(exception);
        await _analytics.logError(error: exception, reason: 'Login failed');
        if (!emit.isDone) {
          emit(AuthError(message));
        }
      },
    );
  }

  Future<void> _onRegisterRequested(
    AuthRegisterRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.register(
      email: event.email,
      password: event.password,
      firstName: event.firstName,
      lastName: event.lastName,
      phoneNumber: event.phoneNumber ?? '',
    );

    await result.when(
      success: (user) async {
        // 📊 Analytics: Track sign up
        await _analytics.logSignUp(method: 'email');
        await _analytics.setUserId(user.id);

        if (!emit.isDone) {
          emit(AuthAuthenticated(user));
        }
      },
      failure: (exception) async {
        final message = _getErrorMessage(exception);
        await _analytics.logError(
          error: exception,
          reason: 'Registration failed',
        );
        if (!emit.isDone) {
          emit(AuthError(message));
        }
      },
    );
  }

  Future<void> _onLogoutRequested(
    AuthLogoutRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.logout();

    await result.when(
      success: (_) async {
        // 📊 Analytics: Track logout
        await _analytics.logLogout();
        await _analytics.setUserId(null);

        if (!emit.isDone) {
          emit(AuthUnauthenticated());
        }
      },
      failure: (exception) async {
        // Even if logout fails, clear local state
        await _analytics.logLogout();
        await _analytics.setUserId(null);

        await _analytics.logError(error: exception, reason: 'Logout failed');

        // Logout failed but still show unauthenticated
        if (!emit.isDone) {
          emit(AuthUnauthenticated());
        }
      },
    );
  }

  Future<void> _onRefreshTokenRequested(
    AuthRefreshTokenRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.refreshToken();

    result.when(
      success: (user) {
        if (!emit.isDone) {
          emit(AuthAuthenticated(user));
        }
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        if (!emit.isDone) {
          emit(AuthError(message));
        }
      },
    );
  }

  Future<void> _onForgotPasswordRequested(
    AuthForgotPasswordRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.forgotPassword(event.email);

    result.when(
      success: (_) => emit(AuthPasswordResetSent(event.email)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AuthError(message));
      },
    );
  }

  Future<void> _onResetPasswordRequested(
    AuthResetPasswordRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.resetPassword(
      email: event.email,
      resetCode: event.token,
      newPassword: event.newPassword,
    );

    result.when(
      success: (_) => emit(AuthPasswordResetSuccess()),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AuthError(message));
      },
    );
  }

  Future<void> _onChangePasswordRequested(
    AuthChangePasswordRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(AuthLoading());

    final result = await _authService.changePassword(
      event.currentPassword,
      event.newPassword,
    );

    await result.when(
      success: (_) async {
        // 📊 Analytics: Track password change
        await _analytics.logEvent(
          name: 'password_changed',
          parameters: {'method': 'in_app'},
        );

        if (!emit.isDone) {
          emit(AuthPasswordChangeSuccess());
        }
      },
      failure: (exception) async {
        final message = _getErrorMessage(exception);
        await _analytics.logError(
          error: exception,
          reason: 'Password change failed',
        );
        if (!emit.isDone) {
          emit(AuthError(message));
        }
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }

  Future<void> _onCheckAuthenticationRequested(
    AuthCheckAuthenticationRequested event,
    Emitter<AuthState> emit,
  ) async {
    final authResult = await _authService.checkAuthentication();

    await authResult.when(
      success: (isAuthenticated) async {
        if (isAuthenticated) {
          final userResult = await _authService.getCurrentUser();
          userResult.when(
            success: (user) {
              if (!emit.isDone) {
                emit(AuthAuthenticated(user));
              }
            },
            failure: (_) {
              if (!emit.isDone) {
                emit(AuthUnauthenticated());
              }
            },
          );
        } else {
          if (!emit.isDone) {
            emit(AuthUnauthenticated());
          }
        }
      },
      failure: (_) {
        if (!emit.isDone) {
          emit(AuthUnauthenticated());
        }
      },
    );
  }

  Future<void> _onCheckTokenValidityRequested(
    AuthCheckTokenValidityRequested event,
    Emitter<AuthState> emit,
  ) async {
    final tokenResult = await _authService.checkTokenValidity();

    tokenResult.when(
      success: (isValid) async {
        if (isValid) {
          final userResult = await _authService.getCurrentUser();
          userResult.when(
            success: (user) => emit(AuthAuthenticated(user)),
            failure: (_) => emit(AuthUnauthenticated()),
          );
        } else {
          emit(AuthUnauthenticated());
        }
      },
      failure: (_) => emit(AuthUnauthenticated()),
    );
  }
}
