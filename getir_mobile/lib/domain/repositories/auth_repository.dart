import '../../core/errors/result.dart';
import '../entities/user_entity.dart';

/// Authentication Repository Interface
///
/// Returns [Result<T>] for all operations that can fail
/// This provides type-safe error handling without throwing exceptions
abstract class IAuthRepository {
  // Authentication Methods
  Future<Result<UserEntity>> login(String email, String password);
  Future<Result<UserEntity>> register(
    String email,
    String password,
    String firstName,
    String lastName, {
    String? phoneNumber,
  });
  Future<Result<void>> logout();
  Future<Result<UserEntity>> refreshToken(String refreshToken);

  // Password Management
  Future<Result<void>> forgotPassword(String email);
  Future<Result<void>> resetPassword(String token, String newPassword);
  Future<Result<void>> changePassword(
    String currentPassword,
    String newPassword,
  );

  // Token Management (local operations, rarely fail)
  Future<String?> getAccessToken();
  Future<String?> getRefreshToken();
  Future<Result<void>> saveTokens(String accessToken, String refreshToken);
  Future<Result<void>> clearTokens();

  // User Management
  Future<UserEntity?> getCurrentUser();
  Future<Result<void>> saveCurrentUser(UserEntity user);
  Future<Result<void>> clearCurrentUser();

  // Authentication State (local checks, don't fail)
  Future<bool> isAuthenticated();
  Future<bool> isTokenValid();
}
