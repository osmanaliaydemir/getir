import '../entities/user_entity.dart';
import '../repositories/auth_repository.dart';

// Login Use Case
class LoginUseCase {
  final AuthRepository _repository;
  
  const LoginUseCase(this._repository);
  
  Future<UserEntity> call(String email, String password) async {
    if (email.isEmpty || password.isEmpty) {
      throw ArgumentError('Email and password cannot be empty');
    }
    
    return await _repository.login(email, password);
  }
}

// Register Use Case
class RegisterUseCase {
  final AuthRepository _repository;
  
  const RegisterUseCase(this._repository);
  
  Future<UserEntity> call(String email, String password, String firstName, String lastName, {String? phoneNumber}) async {
    if (email.isEmpty || password.isEmpty || firstName.isEmpty || lastName.isEmpty) {
      throw ArgumentError('Required fields cannot be empty');
    }
    
    if (password.length < 6) {
      throw ArgumentError('Password must be at least 6 characters');
    }
    
    return await _repository.register(email, password, firstName, lastName, phoneNumber: phoneNumber);
  }
}

// Logout Use Case
class LogoutUseCase {
  final AuthRepository _repository;
  
  const LogoutUseCase(this._repository);
  
  Future<void> call() async {
    await _repository.logout();
  }
}

// Refresh Token Use Case
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
