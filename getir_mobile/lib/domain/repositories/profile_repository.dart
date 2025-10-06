import '../entities/user_profile.dart';

abstract class ProfileRepository {
  Future<UserProfile> getUserProfile();
  Future<UserProfile> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  });
}
