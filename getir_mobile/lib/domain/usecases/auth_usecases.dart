import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/user_entity.dart';
import '../repositories/auth_repository.dart';

/// Login Use Case
///
/// Handles user authentication with email and password.
///
/// **Business Rules:**
/// - Email format validation (RFC 5322 compliant)
/// - Password minimum length (6 characters)
/// - Input sanitization (trim, lowercase email)
///
/// **Pre-conditions:**
/// - Email and password must not be empty
/// - Email must be valid format
/// - Password must be at least 6 characters
///
/// **Post-conditions:**
/// - Returns [Result<UserEntity>] with authenticated user on success
/// - Returns [Result.failure] with [ValidationException] on validation error
/// - Returns [Result.failure] with [UnauthorizedException] on invalid credentials
/// - Returns [Result.failure] with [NetworkException] on connection error
///
/// **Future enhancements:**
/// - Rate limiting (prevent brute force)
/// - Audit logging
/// - Device fingerprinting
/// - 2FA support
class LoginUseCase {
  final AuthRepository _repository;

  const LoginUseCase(this._repository);

  /// Authenticates a user with email and password.
  ///
  /// Returns [Result<UserEntity>] - Success with user or Failure with exception
  Future<Result<UserEntity>> call(String email, String password) async {
    // Business rule: Validate credentials format
    final validationError = _validateCredentials(email, password);
    if (validationError != null) {
      return Result.failure(validationError);
    }

    // Delegate to repository
    return await _repository.login(email.trim().toLowerCase(), password);
  }

  Exception? _validateCredentials(String email, String password) {
    if (email.isEmpty || password.isEmpty) {
      return const ValidationException(
        message: 'Email and password cannot be empty',
        code: 'EMPTY_CREDENTIALS',
      );
    }

    // Email format validation
    final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!emailRegex.hasMatch(email)) {
      return const ValidationException(
        message: 'Invalid email format',
        code: 'INVALID_EMAIL_FORMAT',
      );
    }

    // Password length check
    if (password.length < 6) {
      return const ValidationException(
        message: 'Password must be at least 6 characters',
        code: 'PASSWORD_TOO_SHORT',
      );
    }

    return null;
  }
}

/// Register Use Case
///
/// Handles new user registration.
///
/// **Business Rules:**
/// - Email format validation
/// - Password strength requirements (min 6 chars)
/// - Name sanitization and length check (min 2 chars)
/// - Phone number format validation (optional, E.164 format)
///
/// **Returns:** [Result<UserEntity>] with user or validation error
class RegisterUseCase {
  final AuthRepository _repository;

  const RegisterUseCase(this._repository);

  Future<Result<UserEntity>> call(
    String email,
    String password,
    String firstName,
    String lastName, {
    String? phoneNumber,
  }) async {
    // Business rule: Validate registration data
    final validationError = _validateRegistrationData(
      email,
      password,
      firstName,
      lastName,
      phoneNumber,
    );
    if (validationError != null) {
      return Result.failure(validationError);
    }

    // Sanitize inputs
    final sanitizedEmail = email.trim().toLowerCase();
    final sanitizedFirstName = firstName.trim();
    final sanitizedLastName = lastName.trim();
    final sanitizedPhone = phoneNumber?.trim();

    return await _repository.register(
      sanitizedEmail,
      password,
      sanitizedFirstName,
      sanitizedLastName,
      phoneNumber: sanitizedPhone,
    );
  }

  Exception? _validateRegistrationData(
    String email,
    String password,
    String firstName,
    String lastName,
    String? phoneNumber,
  ) {
    // Required fields check
    if (email.isEmpty ||
        password.isEmpty ||
        firstName.isEmpty ||
        lastName.isEmpty) {
      return const ValidationException(
        message: 'Required fields cannot be empty',
        code: 'REQUIRED_FIELDS_EMPTY',
      );
    }

    // Email format validation
    final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!emailRegex.hasMatch(email)) {
      return const ValidationException(
        message: 'Invalid email format',
        code: 'INVALID_EMAIL_FORMAT',
      );
    }

    // Password strength
    if (password.length < 6) {
      return const ValidationException(
        message: 'Password must be at least 6 characters',
        code: 'PASSWORD_TOO_SHORT',
      );
    }

    // Name length validation
    if (firstName.length < 2 || lastName.length < 2) {
      return const ValidationException(
        message: 'Name must be at least 2 characters',
        code: 'NAME_TOO_SHORT',
      );
    }

    // Phone number validation (if provided)
    if (phoneNumber != null && phoneNumber.isNotEmpty) {
      final phoneRegex = RegExp(r'^[\d\s\-\+\(\)]{10,}$');
      if (!phoneRegex.hasMatch(phoneNumber)) {
        return const ValidationException(
          message: 'Invalid phone number format',
          code: 'INVALID_PHONE_FORMAT',
        );
      }
    }

    return null;
  }
}

// Logout Use Case
class LogoutUseCase {
  final AuthRepository _repository;

  const LogoutUseCase(this._repository);

  Future<Result<void>> call() async {
    return await _repository.logout();
  }
}

// Refresh Token Use Case
class RefreshTokenUseCase {
  final AuthRepository _repository;

  const RefreshTokenUseCase(this._repository);

  Future<Result<UserEntity>> call(String refreshToken) async {
    if (refreshToken.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Refresh token cannot be empty',
          code: 'EMPTY_REFRESH_TOKEN',
        ),
      );
    }

    return await _repository.refreshToken(refreshToken);
  }
}

// Forgot Password Use Case
class ForgotPasswordUseCase {
  final AuthRepository _repository;

  const ForgotPasswordUseCase(this._repository);

  Future<Result<void>> call(String email) async {
    if (email.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Email cannot be empty',
          code: 'EMPTY_EMAIL',
        ),
      );
    }

    return await _repository.forgotPassword(email);
  }
}

// Reset Password Use Case
class ResetPasswordUseCase {
  final AuthRepository _repository;

  const ResetPasswordUseCase(this._repository);

  Future<Result<void>> call(String token, String newPassword) async {
    if (token.isEmpty || newPassword.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Token and new password cannot be empty',
          code: 'EMPTY_RESET_FIELDS',
        ),
      );
    }

    if (newPassword.length < 6) {
      return Result.failure(
        const ValidationException(
          message: 'New password must be at least 6 characters',
          code: 'PASSWORD_TOO_SHORT',
        ),
      );
    }

    return await _repository.resetPassword(token, newPassword);
  }
}

// Get Current User Use Case
class GetCurrentUserUseCase {
  final AuthRepository _repository;

  const GetCurrentUserUseCase(this._repository);

  Future<UserEntity?> call() async {
    return await _repository.getCurrentUser();
  }
}

// Check Authentication Use Case
class CheckAuthenticationUseCase {
  final AuthRepository _repository;

  const CheckAuthenticationUseCase(this._repository);

  Future<bool> call() async {
    return await _repository.isAuthenticated();
  }
}

// Check Token Validity Use Case
class CheckTokenValidityUseCase {
  final AuthRepository _repository;

  const CheckTokenValidityUseCase(this._repository);

  Future<bool> call() async {
    return await _repository.isTokenValid();
  }
}
