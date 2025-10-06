import '../../domain/entities/user_profile.dart';
import '../../domain/repositories/profile_repository.dart';
import '../datasources/profile_datasource.dart';

class ProfileRepositoryImpl implements ProfileRepository {
  final ProfileDataSource _dataSource;
  ProfileRepositoryImpl(this._dataSource);

  @override
  Future<UserProfile> getUserProfile() {
    return _dataSource.getUserProfile();
  }

  @override
  Future<UserProfile> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) {
    return _dataSource.updateUserProfile(
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
      avatarUrl: avatarUrl,
    );
  }
}
