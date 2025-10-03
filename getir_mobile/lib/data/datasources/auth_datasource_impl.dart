import 'package:shared_preferences/shared_preferences.dart';
import 'auth_datasource.dart';
import '../models/auth_models.dart';

class AuthDataSourceImpl implements AuthDataSource {
  SharedPreferences? _prefs;
  
  Future<SharedPreferences> get prefs async {
    _prefs ??= await SharedPreferences.getInstance();
    return _prefs!;
  }
  
  @override
  Future<AuthResponse> login(LoginRequest request) async {
    // TODO: Implement actual API call
    // For now, return mock data
    await Future.delayed(const Duration(seconds: 1));
    
    return AuthResponse(
      accessToken: 'mock_access_token',
      refreshToken: 'mock_refresh_token',
      expiresAt: DateTime.now().add(const Duration(hours: 1)),
      user: UserModel(
        id: '1',
        email: request.email,
        firstName: 'Test',
        lastName: 'User',
        phoneNumber: null,
        role: 'Customer',
        isEmailVerified: true,
        isActive: true,
        createdAt: DateTime.now(),
        updatedAt: null,
        lastLoginAt: DateTime.now(),
      ),
    );
  }
  
  @override
  Future<AuthResponse> register(RegisterRequest request) async {
    // TODO: Implement actual API call
    // For now, return mock data
    await Future.delayed(const Duration(seconds: 1));
    
    return AuthResponse(
      accessToken: 'mock_access_token',
      refreshToken: 'mock_refresh_token',
      expiresAt: DateTime.now().add(const Duration(hours: 1)),
      user: UserModel(
        id: '1',
        email: request.email,
        firstName: request.firstName,
        lastName: request.lastName,
        phoneNumber: request.phoneNumber,
        role: 'Customer',
        isEmailVerified: false,
        isActive: true,
        createdAt: DateTime.now(),
        updatedAt: null,
        lastLoginAt: null,
      ),
    );
  }
  
  @override
  Future<void> logout() async {
    // TODO: Implement actual API call
    await Future.delayed(const Duration(seconds: 1));
  }
  
  @override
  Future<RefreshTokenResponse> refreshToken(RefreshTokenRequest request) async {
    // TODO: Implement actual API call
    // For now, return mock data
    await Future.delayed(const Duration(seconds: 1));
    
    return RefreshTokenResponse(
      accessToken: 'new_mock_access_token',
      refreshToken: 'new_mock_refresh_token',
      expiresAt: DateTime.now().add(const Duration(hours: 1)),
    );
  }
  
  @override
  Future<void> forgotPassword(ForgotPasswordRequest request) async {
    // TODO: Implement actual API call
    await Future.delayed(const Duration(seconds: 1));
  }
  
  @override
  Future<void> resetPassword(ResetPasswordRequest request) async {
    // TODO: Implement actual API call
    await Future.delayed(const Duration(seconds: 1));
  }
  
  @override
  Future<String?> getAccessToken() async {
    final prefs = await this.prefs;
    return prefs.getString('access_token');
  }
  
  @override
  Future<String?> getRefreshToken() async {
    final prefs = await this.prefs;
    return prefs.getString('refresh_token');
  }
  
  @override
  Future<void> saveTokens(String accessToken, String refreshToken) async {
    final prefs = await this.prefs;
    await prefs.setString('access_token', accessToken);
    await prefs.setString('refresh_token', refreshToken);
  }
  
  @override
  Future<void> clearTokens() async {
    final prefs = await this.prefs;
    await prefs.remove('access_token');
    await prefs.remove('refresh_token');
  }
  
  @override
  Future<UserModel?> getCurrentUser() async {
    final prefs = await this.prefs;
    final userJson = prefs.getString('current_user');
    if (userJson != null) {
      // TODO: Implement proper JSON parsing
      return null;
    }
    return null;
  }
  
  @override
  Future<void> saveCurrentUser(UserModel user) async {
    final prefs = await this.prefs;
    // TODO: Implement proper JSON serialization
    await prefs.setString('current_user', '{}');
  }
  
  @override
  Future<void> clearCurrentUser() async {
    final prefs = await this.prefs;
    await prefs.remove('current_user');
  }
  
  @override
  Future<bool> isAuthenticated() async {
    final accessToken = await getAccessToken();
    return accessToken != null && accessToken.isNotEmpty;
  }
  
  @override
  Future<bool> isTokenValid() async {
    final accessToken = await getAccessToken();
    if (accessToken == null || accessToken.isEmpty) {
      return false;
    }
    
    // TODO: Implement proper token validation
    // For now, return true if token exists
    return true;
  }
}
