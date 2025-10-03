import '../entities/user_entity.dart';

abstract class AuthRepository {
  // Authentication Methods
  Future<UserEntity> login(String email, String password);
  Future<UserEntity> register(String email, String password, String firstName, String lastName, {String? phoneNumber});
  Future<void> logout();
  Future<UserEntity> refreshToken(String refreshToken);
  
  // Password Management
  Future<void> forgotPassword(String email);
  Future<void> resetPassword(String token, String newPassword);
  
  // Token Management
  Future<String?> getAccessToken();
  Future<String?> getRefreshToken();
  Future<void> saveTokens(String accessToken, String refreshToken);
  Future<void> clearTokens();
  
  // User Management
  Future<UserEntity?> getCurrentUser();
  Future<void> saveCurrentUser(UserEntity user);
  Future<void> clearCurrentUser();
  
  // Authentication State
  Future<bool> isAuthenticated();
  Future<bool> isTokenValid();
}
