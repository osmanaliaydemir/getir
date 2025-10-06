import '../entities/user_profile.dart';
import '../repositories/profile_repository.dart';

class GetUserProfileUseCase {
  final ProfileRepository repository;
  GetUserProfileUseCase(this.repository);
  Future<UserProfile> call() => repository.getUserProfile();
}

class UpdateUserProfileUseCase {
  final ProfileRepository repository;
  UpdateUserProfileUseCase(this.repository);
  Future<UserProfile> call({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) => repository.updateUserProfile(
    firstName: firstName,
    lastName: lastName,
    phoneNumber: phoneNumber,
    avatarUrl: avatarUrl,
  );
}
