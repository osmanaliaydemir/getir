import '../../core/errors/result.dart';
import '../entities/user_profile.dart';

abstract class ProfileRepository {
  Future<Result<UserProfile>> getUserProfile();
  Future<Result<UserProfile>> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  });
}
