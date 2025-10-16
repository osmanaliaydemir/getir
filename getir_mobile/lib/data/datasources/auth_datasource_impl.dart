import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'auth_datasource.dart';
import '../models/auth_models.dart';
import '../../core/services/secure_encryption_service.dart';

class AuthDataSourceImpl implements AuthDataSource {
  final Dio _dio;
  final SharedPreferences _prefs;
  final SecureEncryptionService _encryptionService;

  AuthDataSourceImpl(
    this._dio,
    this._prefs, [
    SecureEncryptionService? encryptionService,
  ]) : _encryptionService = encryptionService ?? SecureEncryptionService();

  SharedPreferences get prefs => _prefs;

  @override
  Future<AuthResponse> login(LoginRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/auth/login',
        data: request.toJson(),
      );

      if (response.statusCode == 200) {
        // Backend returns: {success: true, value: {...}, error: null}
        final data =
            response.data['value'] ?? response.data['data'] ?? response.data;
        final authResponse = AuthResponse.fromJson(data);

        // Token'ları ve expiration'ı kaydet
        await saveTokens(authResponse.accessToken, authResponse.refreshToken);
        await _saveTokenExpiration(authResponse.expiresAt);

        // User bilgilerini kaydet
        final userModel = authResponse.toUserModel();
        await saveCurrentUser(userModel);

        return authResponse;
      } else {
        throw Exception('Login failed: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error during login: $e');
    }
  }

  @override
  Future<AuthResponse> register(RegisterRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/auth/register',
        data: request.toJson(),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        // Backend returns: {success: true, value: {...}, error: null}
        final data =
            response.data['value'] ?? response.data['data'] ?? response.data;
        final authResponse = AuthResponse.fromJson(data);

        // Token'ları ve expiration'ı kaydet
        await saveTokens(authResponse.accessToken, authResponse.refreshToken);
        await _saveTokenExpiration(authResponse.expiresAt);

        // User bilgilerini kaydet
        final userModel = authResponse.toUserModel();
        await saveCurrentUser(userModel);

        return authResponse;
      } else {
        throw Exception('Registration failed: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error during registration: $e');
    }
  }

  @override
  Future<void> logout() async {
    try {
      // Backend'e logout isteği gönder
      await _dio.post('/api/v1/auth/logout');

      // Local token'ları temizle
      await clearTokens();
      await clearCurrentUser();
    } on DioException catch (e) {
      // Logout'ta hata olsa bile local token'ları temizle
      await clearTokens();
      await clearCurrentUser();

      // 401 hatası beklenir (token geçersizse), bu normal
      if (e.response?.statusCode != 401) {
        throw _handleDioError(e);
      }
    } catch (e) {
      // Logout hatası kritik değil, local temizlik yeterli
      await clearTokens();
      await clearCurrentUser();
    }
  }

  @override
  Future<RefreshTokenResponse> refreshToken(RefreshTokenRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/auth/refresh',
        data: request.toJson(),
      );

      if (response.statusCode == 200) {
        // Backend returns: {success: true, value: {...}, error: null}
        final data =
            response.data['value'] ?? response.data['data'] ?? response.data;
        final refreshResponse = RefreshTokenResponse.fromJson(data);

        // Yeni token'ları kaydet
        await saveTokens(
          refreshResponse.accessToken,
          refreshResponse.refreshToken,
        );

        return refreshResponse;
      } else {
        throw Exception('Token refresh failed: ${response.statusCode}');
      }
    } on DioException catch (e) {
      // Refresh token geçersizse, logout yap
      if (e.response?.statusCode == 401) {
        await clearTokens();
        await clearCurrentUser();
      }
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error during token refresh: $e');
    }
  }

  @override
  Future<void> forgotPassword(ForgotPasswordRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/auth/forgot-password',
        data: request.toJson(),
      );

      if (response.statusCode != 200 && response.statusCode != 204) {
        throw Exception('Forgot password failed: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error during forgot password: $e');
    }
  }

  @override
  Future<void> resetPassword(ResetPasswordRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/auth/reset-password',
        data: request.toJson(),
      );

      if (response.statusCode != 200 && response.statusCode != 204) {
        throw Exception('Reset password failed: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error during reset password: $e');
    }
  }

  @override
  Future<void> changePassword(ChangePasswordRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/auth/change-password',
        data: request.toJson(),
      );

      if (response.statusCode != 200 && response.statusCode != 204) {
        throw Exception('Change password failed: ${response.statusCode}');
      }
    } on DioException catch (e) {
      throw _handleDioError(e);
    } catch (e) {
      throw Exception('Unexpected error during change password: $e');
    }
  }

  @override
  Future<String?> getAccessToken() async {
    // Try SecureEncryptionService first, fallback to SharedPreferences
    final secureToken = await _encryptionService.getAccessToken();
    return secureToken ?? prefs.getString('access_token');
  }

  @override
  Future<String?> getRefreshToken() async {
    // Try SecureEncryptionService first, fallback to SharedPreferences
    final secureToken = await _encryptionService.getRefreshToken();
    return secureToken ?? prefs.getString('refresh_token');
  }

  @override
  Future<void> saveTokens(String accessToken, String refreshToken) async {
    // Use SecureEncryptionService for token storage
    await _encryptionService.saveAccessToken(accessToken);
    await _encryptionService.saveRefreshToken(refreshToken);

    // Also save to SharedPreferences as fallback (deprecated but kept for compatibility)
    await prefs.setString('access_token', accessToken);
    await prefs.setString('refresh_token', refreshToken);
  }

  @override
  Future<void> clearTokens() async {
    // Clear from SecureEncryptionService
    await _encryptionService.clearAll();

    // Also clear from SharedPreferences
    await prefs.remove('access_token');
    await prefs.remove('refresh_token');
    await prefs.remove('token_expires_at');
  }

  @override
  Future<UserModel?> getCurrentUser() async {
    try {
      final userJson = prefs.getString('current_user');

      if (userJson != null && userJson.isNotEmpty) {
        // Basic parsing (SharedPreferences'ta pipe-separated values olarak saklıyoruz)
        // Daha iyi bir çözüm için json_serializable kullanılabilir
        final parts = userJson.split('|');
        if (parts.length >= 7) {
          return UserModel(
            id: parts[0],
            email: parts[1],
            firstName: parts[2],
            lastName: parts[3],
            phoneNumber: parts[4].isEmpty ? null : parts[4],
            role: parts[5],
            isEmailVerified: parts[6] == 'true',
            isActive: true,
            createdAt: DateTime.now(),
            updatedAt: null,
            lastLoginAt: null,
          );
        }
      }
      return null;
    } catch (e) {
      // Error durumunda null dön
      return null;
    }
  }

  @override
  Future<void> saveCurrentUser(UserModel user) async {
    try {
      // Simple serialization (pipe-separated values)
      final userString =
          '${user.id}|${user.email}|${user.firstName}|${user.lastName}|${user.phoneNumber ?? ''}|${user.role}|${user.isEmailVerified}';
      await prefs.setString('current_user', userString);
    } catch (e) {
      // Error handling
      throw Exception('Failed to save user: $e');
    }
  }

  @override
  Future<void> clearCurrentUser() async {
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

    // Token expiration kontrolü (SharedPreferences'tan)
    final expiresAtString = prefs.getString('token_expires_at');

    if (expiresAtString != null) {
      try {
        final expiresAt = DateTime.parse(expiresAtString);
        // Token expire olmadıysa valid
        return DateTime.now().isBefore(expiresAt);
      } catch (e) {
        // Parse hatası olursa token geçersiz say
        return false;
      }
    }

    // Expiration bilgisi yoksa, token'ı valid say
    return true;
  }

  /// DioException'ları user-friendly mesajlara çevir
  Exception _handleDioError(DioException e) {
    switch (e.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return Exception(
          'Bağlantı zaman aşımına uğradı. Lütfen tekrar deneyin.',
        );

      case DioExceptionType.connectionError:
        return Exception('İnternet bağlantınızı kontrol edin.');

      case DioExceptionType.badResponse:
        final statusCode = e.response?.statusCode;
        final message =
            e.response?.data?['message'] ??
            e.response?.data?['error'] ??
            'Bir hata oluştu';

        switch (statusCode) {
          case 400:
            return Exception('Geçersiz bilgiler: $message');
          case 401:
            return Exception('Email veya şifre hatalı');
          case 403:
            return Exception('Bu işlem için yetkiniz yok');
          case 404:
            return Exception('Kayıt bulunamadı');
          case 409:
            return Exception('Bu email adresi zaten kayıtlı');
          case 422:
            return Exception('Geçersiz veri: $message');
          case 500:
          case 502:
          case 503:
            return Exception(
              'Sunucu hatası. Lütfen daha sonra tekrar deneyin.',
            );
          default:
            return Exception(message);
        }

      case DioExceptionType.cancel:
        return Exception('İstek iptal edildi');

      case DioExceptionType.unknown:
      default:
        return Exception('Beklenmeyen bir hata oluştu: ${e.message}');
    }
  }

  /// Token expiration'ı kaydet
  Future<void> _saveTokenExpiration(DateTime expiresAt) async {
    await prefs.setString('token_expires_at', expiresAt.toIso8601String());
  }
}
