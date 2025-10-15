import 'package:dio/dio.dart';
import '../../domain/entities/user_profile.dart';

abstract class ProfileDataSource {
  Future<UserProfile> getUserProfile();
  Future<UserProfile> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  });
}

class ProfileDataSourceImpl implements ProfileDataSource {
  final Dio _dio;
  ProfileDataSourceImpl(this._dio);

  @override
  Future<UserProfile> getUserProfile() async {
    final response = await _dio.get('/api/v1/user/profile');
    final data = response.data['data'] ?? response.data;
    return UserProfile.fromJson(data as Map<String, dynamic>);
  }

  @override
  Future<UserProfile> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    final response = await _dio.put(
      '/api/v1/user/profile',
      data: {
        'firstName': firstName,
        'lastName': lastName,
        'phoneNumber': phoneNumber,
        'avatarUrl': avatarUrl,
      },
    );
    final data = response.data['data'] ?? response.data;
    return UserProfile.fromJson(data as Map<String, dynamic>);
  }
}
