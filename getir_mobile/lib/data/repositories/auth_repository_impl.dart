import '../../domain/entities/user_entity.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/auth_datasource.dart';
import '../models/auth_models.dart';

class AuthRepositoryImpl implements AuthRepository {
  final AuthDataSource _dataSource;

  const AuthRepositoryImpl(this._dataSource);

  @override
  Future<UserEntity> login(String email, String password) async {
    try {
      final request = LoginRequest(email: email, password: password);
      final response = await _dataSource.login(request);

      // Token'lar ve user data zaten datasource'da kaydedildi
      // AuthResponse'dan UserEntity oluştur
      final userModel = response.toUserModel();
      return userModel.toEntity();
    } catch (e) {
      throw Exception('Login failed: $e');
    }
  }

  @override
  Future<UserEntity> register(
    String email,
    String password,
    String firstName,
    String lastName, {
    String? phoneNumber,
  }) async {
    try {
      final request = RegisterRequest(
        email: email,
        password: password,
        firstName: firstName,
        lastName: lastName,
        phoneNumber: phoneNumber,
      );
      final response = await _dataSource.register(request);

      // Token'lar ve user data zaten datasource'da kaydedildi
      // AuthResponse'dan UserEntity oluştur
      final userModel = response.toUserModel();
      return userModel.toEntity();
    } catch (e) {
      throw Exception('Registration failed: $e');
    }
  }

  @override
  Future<void> logout() async {
    try {
      await _dataSource.logout();
      await _dataSource.clearTokens();
      await _dataSource.clearCurrentUser();
    } catch (e) {
      throw Exception('Logout failed: $e');
    }
  }

  @override
  Future<UserEntity> refreshToken(String refreshToken) async {
    try {
      final request = RefreshTokenRequest(refreshToken: refreshToken);
      final response = await _dataSource.refreshToken(request);

      // Save new tokens
      await _dataSource.saveTokens(response.accessToken, response.refreshToken);

      // Get current user
      final user = await _dataSource.getCurrentUser();
      if (user == null) {
        throw Exception('User not found after token refresh');
      }

      return user.toEntity();
    } catch (e) {
      throw Exception('Token refresh failed: $e');
    }
  }

  @override
  Future<void> forgotPassword(String email) async {
    try {
      final request = ForgotPasswordRequest(email: email);
      await _dataSource.forgotPassword(request);
    } catch (e) {
      throw Exception('Forgot password failed: $e');
    }
  }

  @override
  Future<void> resetPassword(String token, String newPassword) async {
    try {
      final request = ResetPasswordRequest(
        token: token,
        newPassword: newPassword,
      );
      await _dataSource.resetPassword(request);
    } catch (e) {
      throw Exception('Reset password failed: $e');
    }
  }

  @override
  Future<String?> getAccessToken() async {
    return await _dataSource.getAccessToken();
  }

  @override
  Future<String?> getRefreshToken() async {
    return await _dataSource.getRefreshToken();
  }

  @override
  Future<void> saveTokens(String accessToken, String refreshToken) async {
    await _dataSource.saveTokens(accessToken, refreshToken);
  }

  @override
  Future<void> clearTokens() async {
    await _dataSource.clearTokens();
  }

  @override
  Future<UserEntity?> getCurrentUser() async {
    final user = await _dataSource.getCurrentUser();
    return user?.toEntity();
  }

  @override
  Future<void> saveCurrentUser(UserEntity user) async {
    final userModel = UserModel.fromEntity(user);
    await _dataSource.saveCurrentUser(userModel);
  }

  @override
  Future<void> clearCurrentUser() async {
    await _dataSource.clearCurrentUser();
  }

  @override
  Future<bool> isAuthenticated() async {
    return await _dataSource.isAuthenticated();
  }

  @override
  Future<bool> isTokenValid() async {
    return await _dataSource.isTokenValid();
  }
}
