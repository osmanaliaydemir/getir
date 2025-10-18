import 'dart:io';
import 'package:dio/dio.dart';
import '../../domain/entities/user_profile.dart';
import '../../core/network/api_response_parser.dart';

abstract class ProfileDataSource {
  Future<UserProfile> getUserProfile();
  Future<UserProfile> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    File? avatarImage,
    String? avatarUrl,
  });
}

class ProfileDataSourceImpl implements ProfileDataSource {
  final Dio _dio;
  ProfileDataSourceImpl(this._dio);

  @override
  Future<UserProfile> getUserProfile() async {
    final response = await _dio.get('/api/v1/user/profile');

    return ApiResponseParser.parse<UserProfile>(
      responseData: response.data,
      fromJson: (json) => UserProfile.fromJson(json),
      endpointName: 'getUserProfile',
    );
  }

  @override
  Future<UserProfile> updateUserProfile({
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? avatarUrl,
    File? avatarImage,
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

    return ApiResponseParser.parse<UserProfile>(
      responseData: response.data,
      fromJson: (json) => UserProfile.fromJson(json),
      endpointName: 'updateUserProfile',
    );
  }
}
