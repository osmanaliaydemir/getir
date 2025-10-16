import '../../core/errors/result.dart';
import '../entities/user_profile.dart';
import '../repositories/profile_repository.dart';

/// Profile Service
///
/// Centralized service for user profile operations.
/// Replaces 2 separate UseCase classes.
class ProfileService {
  final IProfileRepository _repository;

  const ProfileService(this._repository);

  Future<Result<UserProfile>> getUserProfile() async {
    return await _repository.getUserProfile();
  }

  Future<Result<UserProfile>> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    return await _repository.updateUserProfile(
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
      avatarUrl: avatarUrl,
    );
  }
}
