import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/domain/entities/user_profile.dart';
import 'package:getir_mobile/domain/repositories/profile_repository.dart';
import 'package:getir_mobile/domain/services/profile_service.dart';

import 'profile_service_test.mocks.dart';

@GenerateMocks([ProfileRepository])
void main() {
  late ProfileService service;
  late MockProfileRepository mockRepository;

  setUp(() {
    mockRepository = MockProfileRepository();
    service = ProfileService(mockRepository);
  });

  final testProfile = UserProfile(
    id: 'user-123',
    email: 'test@getir.com',
    firstName: 'Test',
    lastName: 'User',
  );

  group('ProfileService -', () {
    group('getUserProfile', () {
      test('returns profile when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.getUserProfile(),
        ).thenAnswer((_) async => Result.success(testProfile));

        // Act
        final result = await service.getUserProfile();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testProfile);
        verify(mockRepository.getUserProfile()).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getUserProfile(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getUserProfile();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.getUserProfile()).called(1);
      });

      test('returns failure when profile not found', () async {
        // Arrange
        final exception = NotFoundException(message: 'Profile not found');
        when(
          mockRepository.getUserProfile(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getUserProfile();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns failure when not authenticated', () async {
        // Arrange
        final exception = UnauthorizedException(message: 'Not authenticated');
        when(
          mockRepository.getUserProfile(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getUserProfile();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('updateUserProfile', () {
      test('updates profile when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenAnswer((_) async => Result.success(testProfile));

        // Act
        final result = await service.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
          phoneNumber: '+905551234567',
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testProfile);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = ValidationException(message: 'Invalid profile data');
        when(
          mockRepository.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns failure when not authenticated', () async {
        // Arrange
        final exception = UnauthorizedException(message: 'Not authenticated');
        when(
          mockRepository.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns failure when network error occurs', () async {
        // Arrange
        final exception = NetworkException(message: 'Network error');
        when(
          mockRepository.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });
  });
}
