import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/profile_datasource.dart';
import 'package:getir_mobile/data/repositories/profile_repository_impl.dart';
import 'package:getir_mobile/domain/entities/user_profile.dart';

import 'profile_repository_impl_test.mocks.dart';

@GenerateMocks([ProfileDataSource])
void main() {
  late ProfileRepositoryImpl repository;
  late MockProfileDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockProfileDataSource();
    repository = ProfileRepositoryImpl(mockDataSource);
  });

  final testProfile = UserProfile(
    id: 'user-123',
    email: 'test@getir.com',
    firstName: 'Test',
    lastName: 'User',
  );

  group('ProfileRepositoryImpl -', () {
    group('getUserProfile', () {
      test('returns success with profile when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.getUserProfile(),
        ).thenAnswer((_) async => testProfile);

        // Act
        final result = await repository.getUserProfile();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testProfile);
        verify(mockDataSource.getUserProfile()).called(1);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/profile'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.getUserProfile()).thenThrow(dioException);

        // Act
        final result = await repository.getUserProfile();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns NotFoundException when profile not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/profile'),
          response: Response(
            requestOptions: RequestOptions(path: '/profile'),
            statusCode: 404,
            data: {'message': 'Profile not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.getUserProfile()).thenThrow(dioException);

        // Act
        final result = await repository.getUserProfile();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });

      test('returns UnauthorizedException when not authenticated', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/profile'),
          response: Response(
            requestOptions: RequestOptions(path: '/profile'),
            statusCode: 401,
            data: {'message': 'Unauthorized'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.getUserProfile()).thenThrow(dioException);

        // Act
        final result = await repository.getUserProfile();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<UnauthorizedException>());
      });
    });

    group('updateUserProfile', () {
      test(
        'returns success with updated profile when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.updateUserProfile(
              firstName: anyNamed('firstName'),
              lastName: anyNamed('lastName'),
              phoneNumber: anyNamed('phoneNumber'),
              avatarUrl: anyNamed('avatarUrl'),
            ),
          ).thenAnswer((_) async => testProfile);

          // Act
          final result = await repository.updateUserProfile(
            firstName: 'Test',
            lastName: 'User',
          );

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, testProfile);
        },
      );

      test('returns ValidationException when validation fails', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/profile'),
          response: Response(
            requestOptions: RequestOptions(path: '/profile'),
            statusCode: 400,
            data: {'message': 'Invalid profile data'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/profile'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns failure when AppException occurs', () async {
        // Arrange
        const appException = ApiException(message: 'Update error');
        when(
          mockDataSource.updateUserProfile(
            firstName: anyNamed('firstName'),
            lastName: anyNamed('lastName'),
            phoneNumber: anyNamed('phoneNumber'),
            avatarUrl: anyNamed('avatarUrl'),
          ),
        ).thenThrow(appException);

        // Act
        final result = await repository.updateUserProfile(
          firstName: 'Test',
          lastName: 'User',
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });
  });
}
