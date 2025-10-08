import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/user_profile.dart';
import '../repositories/profile_repository.dart';

/// Get User Profile Use Case
class GetUserProfileUseCase {
  final ProfileRepository repository;

  GetUserProfileUseCase(this.repository);

  Future<Result<UserProfile>> call() async {
    return await repository.getUserProfile();
  }
}

/// Update User Profile Use Case
class UpdateUserProfileUseCase {
  final ProfileRepository repository;

  UpdateUserProfileUseCase(this.repository);

  Future<Result<UserProfile>> call({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    // Validate names
    if (firstName.isEmpty || lastName.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'First name and last name cannot be empty',
          code: 'EMPTY_NAME_FIELDS',
        ),
      );
    }

    if (firstName.length < 2 || lastName.length < 2) {
      return Result.failure(
        const ValidationException(
          message: 'Name must be at least 2 characters',
          code: 'NAME_TOO_SHORT',
        ),
      );
    }

    return await repository.updateUserProfile(
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
      avatarUrl: avatarUrl,
    );
  }
}
