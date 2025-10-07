import 'package:injectable/injectable.dart';
import '../entities/user_entity.dart';
import '../repositories/auth_repository.dart';

/// Login Use Case
/// 
/// Handles user authentication with email and password.
/// Includes business rules:
/// - Email format validation
/// - Password minimum length check
/// - Input sanitization
/// 
/// Future enhancements:
/// - Rate limiting (prevent brute force)
/// - Audit logging
/// - Device fingerprinting
@injectable
class LoginUseCase {
  final AuthRepository _repository;
  
  const LoginUseCase(this._repository);
  
  Future<UserEntity> call(String email, String password) async {
    // Business rule: Validate credentials format
    _validateCredentials(email, password);
    
    return await _repository.login(email.trim().toLowerCase(), password);
  }
  
  void _validateCredentials(String email, String password) {
    if (email.isEmpty || password.isEmpty) {
      throw ArgumentError('Email and password cannot be empty');
    }
    
    // Email format validation
    final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!emailRegex.hasMatch(email)) {
      throw ArgumentError('Invalid email format');
    }
    
    // Password length check
    if (password.length < 6) {
      throw ArgumentError('Password must be at least 6 characters');
    }
  }
}

/// Register Use Case
/// 
/// Handles new user registration.
/// Includes business rules:
/// - Email format validation
/// - Password strength requirements (min 6 chars, could be enhanced)
/// - Name sanitization
/// - Phone number format validation (optional)
/// 
/// Future enhancements:
/// - Password complexity requirements (uppercase, numbers, special chars)
/// - Duplicate account prevention
/// - Email domain blacklist
/// - Age verification
@injectable
class RegisterUseCase {
  final AuthRepository _repository;
  
  const RegisterUseCase(this._repository);
  
  Future<UserEntity> call(
    String email,
    String password,
    String firstName,
    String lastName, {
    String? phoneNumber,
  }) async {
    // Business rule: Validate registration data
    _validateRegistrationData(email, password, firstName, lastName, phoneNumber);
    
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
  
  void _validateRegistrationData(
    String email,
    String password,
    String firstName,
    String lastName,
    String? phoneNumber,
  ) {
    // Required fields check
    if (email.isEmpty || password.isEmpty || firstName.isEmpty || lastName.isEmpty) {
      throw ArgumentError('Required fields cannot be empty');
    }
    
    // Email format validation
    final emailRegex = RegExp(r'^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$');
    if (!emailRegex.hasMatch(email)) {
      throw ArgumentError('Invalid email format');
    }
    
    // Password strength
    if (password.length < 6) {
      throw ArgumentError('Password must be at least 6 characters');
    }
    
    // Name length validation
    if (firstName.length < 2 || lastName.length < 2) {
      throw ArgumentError('Name must be at least 2 characters');
    }
    
    // Phone number validation (if provided)
    if (phoneNumber != null && phoneNumber.isNotEmpty) {
      final phoneRegex = RegExp(r'^[\d\s\-\+\(\)]{10,}$');
      if (!phoneRegex.hasMatch(phoneNumber)) {
        throw ArgumentError('Invalid phone number format');
      }
    }
  }
}

// Logout Use Case
@injectable
class LogoutUseCase {
  final AuthRepository _repository;
  
  const LogoutUseCase(this._repository);
  
  Future<void> call() async {
    await _repository.logout();
  }
}

// Refresh Token Use Case
@injectable
class RefreshTokenUseCase {
  final AuthRepository _repository;
  
  const RefreshTokenUseCase(this._repository);
  
  Future<UserEntity> call(String refreshToken) async {
    if (refreshToken.isEmpty) {
      throw ArgumentError('Refresh token cannot be empty');
    }
    
    return await _repository.refreshToken(refreshToken);
  }
}

// Forgot Password Use Case
@injectable
class ForgotPasswordUseCase {
  final AuthRepository _repository;
  
  const ForgotPasswordUseCase(this._repository);
  
  Future<void> call(String email) async {
    if (email.isEmpty) {
      throw ArgumentError('Email cannot be empty');
    }
    
    await _repository.forgotPassword(email);
  }
}

// Reset Password Use Case
@injectable
class ResetPasswordUseCase {
  final AuthRepository _repository;
  
  const ResetPasswordUseCase(this._repository);
  
  Future<void> call(String token, String newPassword) async {
    if (token.isEmpty || newPassword.isEmpty) {
      throw ArgumentError('Token and new password cannot be empty');
    }
    
    if (newPassword.length < 6) {
      throw ArgumentError('New password must be at least 6 characters');
    }
    
    await _repository.resetPassword(token, newPassword);
  }
}

// Get Current User Use Case
@injectable
class GetCurrentUserUseCase {
  final AuthRepository _repository;
  
  const GetCurrentUserUseCase(this._repository);
  
  Future<UserEntity?> call() async {
    return await _repository.getCurrentUser();
  }
}

// Check Authentication Use Case
@injectable
class CheckAuthenticationUseCase {
  final AuthRepository _repository;
  
  const CheckAuthenticationUseCase(this._repository);
  
  Future<bool> call() async {
    return await _repository.isAuthenticated();
  }
}

// Check Token Validity Use Case
@injectable
class CheckTokenValidityUseCase {
  final AuthRepository _repository;
  
  const CheckTokenValidityUseCase(this._repository);
  
  Future<bool> call() async {
    return await _repository.isTokenValid();
  }
}
