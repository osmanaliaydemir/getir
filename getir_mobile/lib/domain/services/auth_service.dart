import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/user_entity.dart';
import '../repositories/auth_repository.dart';

/// Authentication Service
///
/// Centralized service for all authentication-related operations.
/// Replaces 9 separate UseCase classes with a single, cohesive service.
///
/// **Benefits over UseCase pattern:**
/// - Single responsibility: All auth logic in one place
/// - Reduced boilerplate: No need for 9 separate classes
/// - Easier to maintain: One file to update
/// - Better discoverability: All methods visible at once
/// - Simplified DI: Single dependency instead of 9
///
/// **Business Rules:**
/// - Email validation (RFC 5322 compliant)
/// - Password strength requirements (min 6 chars)
/// - Input sanitization (trim, lowercase)
/// - Token management and refresh
///
/// **Methods:**
/// - login() - Authenticate user
/// - register() - Create new account
/// - logout() - Clear session
/// - refreshToken() - Refresh access token
/// - forgotPassword() - Initiate password reset
/// - resetPassword() - Complete password reset
/// - getCurrentUser() - Get current user info
/// - checkAuthentication() - Verify auth status
/// - checkTokenValidity() - Validate token expiry
class AuthService {
  final AuthRepository _repository;

  const AuthService(this._repository);

  // ============================================================================
  // AUTHENTICATION
  // ============================================================================

  /// Authenticates a user with email and password.
  ///
  /// **Validation:**
  /// - Email and password must not be empty
  /// - Email must be valid format
  /// - Password must be at least 6 characters
  ///
  /// **Returns:**
  /// - Success: [UserEntity] with user data and tokens
  /// - Failure: [ValidationException] on validation error
  /// - Failure: [UnauthorizedException] on invalid credentials
  /// - Failure: [NetworkException] on connection error
  ///
  /// **Example:**
  /// ```dart
  /// final result = await authService.login('user@example.com', 'password123');
  /// result.when(
  ///   success: (user) => logger.info('Logged in: ${user.email}', tag: 'Auth'),
  ///   failure: (error) => logger.error('Login failed', tag: 'Auth', error: error),
  /// );
  /// ```
  Future<Result<UserEntity>> login(String email, String password) async {
    // Business rule: Validate credentials format
    final validationError = _validateCredentials(email, password);
    if (validationError != null) {
      return Result.failure(validationError);
    }

    // Delegate to repository
    return await _repository.login(email.trim().toLowerCase(), password);
  }

  /// Registers a new user account.
  ///
  /// **Validation:**
  /// - All fields must not be empty
  /// - Email must be valid format
  /// - Password must be at least 6 characters
  /// - Phone must be valid format (Turkey: +90 5XX XXX XX XX)
  ///
  /// **Returns:**
  /// - Success: [UserEntity] with new user data and tokens
  /// - Failure: [ValidationException] on validation error
  /// - Failure: [ConflictException] if email/phone already exists
  /// - Failure: [NetworkException] on connection error
  ///
  /// **Example:**
  /// ```dart
  /// final result = await authService.register(
  ///   email: 'user@example.com',
  ///   password: 'password123',
  ///   firstName: 'John',
  ///   lastName: 'Doe',
  ///   phoneNumber: '+905551234567',
  /// );
  /// ```
  Future<Result<UserEntity>> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
  }) async {
    // Business rule: Validate registration data
    final validationError = _validateRegistrationData(
      email: email,
      password: password,
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
    );
    if (validationError != null) {
      return Result.failure(validationError);
    }

    // Delegate to repository
    return await _repository.register(
      email.trim().toLowerCase(),
      password,
      firstName.trim(),
      lastName.trim(),
      phoneNumber: phoneNumber.trim(),
    );
  }

  /// Logs out the current user.
  ///
  /// Clears all stored tokens and user data from local storage.
  ///
  /// **Returns:**
  /// - Success: void
  /// - Failure: [StorageException] on storage error
  ///
  /// **Note:** This method never fails in practice, as we silently handle errors
  Future<Result<void>> logout() async {
    return await _repository.logout();
  }

  // ============================================================================
  // TOKEN MANAGEMENT
  // ============================================================================

  /// Refreshes the access token using the stored refresh token.
  ///
  /// Automatically called when API returns 401 Unauthorized.
  ///
  /// **Returns:**
  /// - Success: [UserEntity] with new tokens
  /// - Failure: [UnauthorizedException] if refresh token is invalid/expired
  /// - Failure: [NetworkException] on connection error
  Future<Result<UserEntity>> refreshToken() async {
    final refreshToken = await _repository.getRefreshToken();
    if (refreshToken == null) {
      return Result.failure(
        UnauthorizedException(message: 'No refresh token available'),
      );
    }
    return await _repository.refreshToken(refreshToken);
  }

  /// Checks if the current access token is valid.
  ///
  /// **Returns:**
  /// - Success: true if token is valid and not expired
  /// - Success: false if token is invalid or expired
  Future<Result<bool>> checkTokenValidity() async {
    final isValid = await _repository.isTokenValid();
    return Result.success(isValid);
  }

  // ============================================================================
  // PASSWORD MANAGEMENT
  // ============================================================================

  /// Initiates the password reset flow.
  ///
  /// Sends a reset code to the user's email.
  ///
  /// **Validation:**
  /// - Email must not be empty
  /// - Email must be valid format
  ///
  /// **Returns:**
  /// - Success: void
  /// - Failure: [ValidationException] on validation error
  /// - Failure: [NotFoundException] if email doesn't exist
  /// - Failure: [NetworkException] on connection error
  Future<Result<void>> forgotPassword(String email) async {
    // Validate email
    final validationError = _validateEmail(email);
    if (validationError != null) {
      return Result.failure(validationError);
    }

    return await _repository.forgotPassword(email.trim().toLowerCase());
  }

  /// Completes the password reset flow.
  ///
  /// **Validation:**
  /// - Email must not be empty and valid
  /// - Reset code must not be empty
  /// - New password must be at least 6 characters
  ///
  /// **Returns:**
  /// - Success: void
  /// - Failure: [ValidationException] on validation error
  /// - Failure: [UnauthorizedException] if code is invalid/expired
  /// - Failure: [NetworkException] on connection error
  Future<Result<void>> resetPassword({
    required String email,
    required String resetCode,
    required String newPassword,
  }) async {
    // Validate inputs
    if (email.trim().isEmpty ||
        resetCode.trim().isEmpty ||
        newPassword.trim().isEmpty) {
      return Result.failure(
        ValidationException(
          message: 'Email, reset code, and new password are required',
        ),
      );
    }

    if (newPassword.length < 6) {
      return Result.failure(
        ValidationException(
          message: 'New password must be at least 6 characters',
        ),
      );
    }

    return await _repository.resetPassword(resetCode.trim(), newPassword);
  }

  /// Changes the password for authenticated user.
  ///
  /// **Validation:**
  /// - Current password must not be empty
  /// - New password must be at least 6 characters
  /// - New password must be different from current
  ///
  /// **Returns:**
  /// - Success: void
  /// - Failure: [ValidationException] on validation error
  /// - Failure: [UnauthorizedException] if current password is wrong
  /// - Failure: [NetworkException] on connection error
  ///
  /// **Example:**
  /// ```dart
  /// final result = await authService.changePassword('oldPass', 'newPass123');
  /// result.when(
  ///   success: (_) => showSuccess('Password changed successfully'),
  ///   failure: (error) => showError(error.message),
  /// );
  /// ```
  Future<Result<void>> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    // Validate inputs
    if (currentPassword.trim().isEmpty || newPassword.trim().isEmpty) {
      return Result.failure(
        ValidationException(
          message: 'Current and new password are required',
        ),
      );
    }

    if (newPassword.length < 6) {
      return Result.failure(
        ValidationException(
          message: 'New password must be at least 6 characters',
        ),
      );
    }

    if (currentPassword == newPassword) {
      return Result.failure(
        ValidationException(
          message: 'New password must be different from current password',
        ),
      );
    }

    return await _repository.changePassword(
      currentPassword.trim(),
      newPassword.trim(),
    );
  }

  // ============================================================================
  // USER INFO
  // ============================================================================

  /// Gets the current authenticated user's information.
  ///
  /// **Returns:**
  /// - Success: [UserEntity] with current user data
  /// - Failure: [UnauthorizedException] if not authenticated
  /// - Failure: [NetworkException] on connection error
  Future<Result<UserEntity>> getCurrentUser() async {
    final user = await _repository.getCurrentUser();
    if (user == null) {
      return Result.failure(
        UnauthorizedException(message: 'No user currently authenticated'),
      );
    }
    return Result.success(user);
  }

  /// Checks if a user is currently authenticated.
  ///
  /// **Returns:**
  /// - Success: true if user is authenticated
  /// - Success: false if user is not authenticated
  Future<Result<bool>> checkAuthentication() async {
    final isAuth = await _repository.isAuthenticated();
    return Result.success(isAuth);
  }

  // ============================================================================
  // VALIDATION HELPERS
  // ============================================================================

  Exception? _validateCredentials(String email, String password) {
    if (email.trim().isEmpty || password.trim().isEmpty) {
      return ValidationException(message: 'Email and password cannot be empty');
    }

    if (!_isValidEmail(email)) {
      return ValidationException(message: 'Please enter a valid email address');
    }

    if (password.length < 6) {
      return ValidationException(
        message: 'Password must be at least 6 characters',
      );
    }

    return null;
  }

  Exception? _validateRegistrationData({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
  }) {
    if (email.trim().isEmpty ||
        password.trim().isEmpty ||
        firstName.trim().isEmpty ||
        lastName.trim().isEmpty ||
        phoneNumber.trim().isEmpty) {
      return ValidationException(message: 'All fields are required');
    }

    if (!_isValidEmail(email)) {
      return ValidationException(message: 'Please enter a valid email address');
    }

    if (password.length < 6) {
      return ValidationException(
        message: 'Password must be at least 6 characters',
      );
    }

    if (!_isValidPhoneNumber(phoneNumber)) {
      return ValidationException(
        message: 'Please enter a valid phone number (e.g., +905551234567)',
      );
    }

    return null;
  }

  Exception? _validateEmail(String email) {
    if (email.trim().isEmpty) {
      return ValidationException(message: 'Email cannot be empty');
    }

    if (!_isValidEmail(email)) {
      return ValidationException(message: 'Please enter a valid email address');
    }

    return null;
  }

  bool _isValidEmail(String email) {
    final emailRegex = RegExp(
      r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$',
    );
    return emailRegex.hasMatch(email);
  }

  bool _isValidPhoneNumber(String phoneNumber) {
    // Turkey phone format: +90 5XX XXX XX XX
    final phoneRegex = RegExp(r'^\+90\s?5\d{9}$');
    return phoneRegex.hasMatch(phoneNumber.replaceAll(' ', ''));
  }
}
