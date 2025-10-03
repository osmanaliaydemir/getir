import '../models/auth_models.dart';

abstract class AuthDataSource {
  // Remote Data Source (API)
  Future<AuthResponse> login(LoginRequest request);
  Future<AuthResponse> register(RegisterRequest request);
  Future<void> logout();
  Future<RefreshTokenResponse> refreshToken(RefreshTokenRequest request);
  Future<void> forgotPassword(ForgotPasswordRequest request);
  Future<void> resetPassword(ResetPasswordRequest request);
  
  // Local Data Source (Storage)
  Future<String?> getAccessToken();
  Future<String?> getRefreshToken();
  Future<void> saveTokens(String accessToken, String refreshToken);
  Future<void> clearTokens();
  
  Future<UserModel?> getCurrentUser();
  Future<void> saveCurrentUser(UserModel user);
  Future<void> clearCurrentUser();
  
  Future<bool> isAuthenticated();
  Future<bool> isTokenValid();
}
