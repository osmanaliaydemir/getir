import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/user_entity.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/auth_datasource.dart';
import '../models/auth_models.dart';

class AuthRepositoryImpl implements AuthRepository {
  final AuthDataSource _dataSource;

  const AuthRepositoryImpl(this._dataSource);

  @override
  Future<Result<UserEntity>> login(String email, String password) async {
    try {
      final request = LoginRequest(email: email, password: password);
      final response = await _dataSource.login(request);

      // Token'lar ve user data zaten datasource'da kaydedildi
      // AuthResponse'dan UserEntity oluştur
      final userModel = response.toUserModel();
      final userEntity = userModel.toDomain();

      return Result.success(userEntity);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Login failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<UserEntity>> register(
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
      final userEntity = userModel.toDomain();

      return Result.success(userEntity);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Registration failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> logout() async {
    try {
      await _dataSource.logout();
      await _dataSource.clearTokens();
      await _dataSource.clearCurrentUser();

      return Result.success(null);
    } on DioException catch (e) {
      // Even if API call fails, clear local data
      await _dataSource.clearTokens();
      await _dataSource.clearCurrentUser();
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Logout failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<UserEntity>> refreshToken(String refreshToken) async {
    try {
      final request = RefreshTokenRequest(refreshToken: refreshToken);
      final response = await _dataSource.refreshToken(request);

      // Save new tokens
      await _dataSource.saveTokens(response.accessToken, response.refreshToken);

      // Get current user
      final user = await _dataSource.getCurrentUser();
      if (user == null) {
        return Result.failure(
          const UnauthorizedException(
            message: 'User not found after token refresh',
            code: 'USER_NOT_FOUND_AFTER_REFRESH',
          ),
        );
      }

      return Result.success(user.toDomain());
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Token refresh failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> forgotPassword(String email) async {
    try {
      final request = ForgotPasswordRequest(email: email);
      await _dataSource.forgotPassword(request);

      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Forgot password failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> resetPassword(String token, String newPassword) async {
    try {
      final request = ResetPasswordRequest(
        token: token,
        newPassword: newPassword,
      );
      await _dataSource.resetPassword(request);

      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Reset password failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    try {
      final request = ChangePasswordRequest(
        currentPassword: currentPassword,
        newPassword: newPassword,
      );
      await _dataSource.changePassword(request);

      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Change password failed: ${e.toString()}'),
      );
    }
  }

  @override
  Future<String?> getAccessToken() async {
    try {
      return await _dataSource.getAccessToken();
    } catch (e) {
      // Silently fail for local operations
      return null;
    }
  }

  @override
  Future<String?> getRefreshToken() async {
    try {
      return await _dataSource.getRefreshToken();
    } catch (e) {
      // Silently fail for local operations
      return null;
    }
  }

  @override
  Future<Result<void>> saveTokens(
    String accessToken,
    String refreshToken,
  ) async {
    try {
      await _dataSource.saveTokens(accessToken, refreshToken);
      return Result.success(null);
    } catch (e) {
      return Result.failure(
        StorageException(
          message: 'Failed to save tokens: ${e.toString()}',
          originalError: e,
        ),
      );
    }
  }

  @override
  Future<Result<void>> clearTokens() async {
    try {
      await _dataSource.clearTokens();
      return Result.success(null);
    } catch (e) {
      return Result.failure(
        StorageException(
          message: 'Failed to clear tokens: ${e.toString()}',
          originalError: e,
        ),
      );
    }
  }

  @override
  Future<UserEntity?> getCurrentUser() async {
    try {
      final user = await _dataSource.getCurrentUser();
      return user?.toDomain();
    } catch (e) {
      // Silently fail for local operations
      return null;
    }
  }

  @override
  Future<Result<void>> saveCurrentUser(UserEntity user) async {
    try {
      final userModel = UserModel.fromDomain(user);
      await _dataSource.saveCurrentUser(userModel);
      return Result.success(null);
    } catch (e) {
      return Result.failure(
        StorageException(
          message: 'Failed to save user: ${e.toString()}',
          originalError: e,
        ),
      );
    }
  }

  @override
  Future<Result<void>> clearCurrentUser() async {
    try {
      await _dataSource.clearCurrentUser();
      return Result.success(null);
    } catch (e) {
      return Result.failure(
        StorageException(
          message: 'Failed to clear user: ${e.toString()}',
          originalError: e,
        ),
      );
    }
  }

  @override
  Future<bool> isAuthenticated() async {
    try {
      return await _dataSource.isAuthenticated();
    } catch (e) {
      // Silently fail, assume not authenticated
      return false;
    }
  }

  @override
  Future<bool> isTokenValid() async {
    try {
      return await _dataSource.isTokenValid();
    } catch (e) {
      // Silently fail, assume token invalid
      return false;
    }
  }
}
