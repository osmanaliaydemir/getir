import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/auth_datasource.dart';
import 'package:getir_mobile/data/models/auth_models.dart';
import 'package:getir_mobile/data/repositories/auth_repository_impl.dart';
import 'package:getir_mobile/domain/entities/user_entity.dart';

import '../../helpers/mock_data.dart';
import 'auth_repository_impl_test.mocks.dart';

@GenerateMocks([AuthDataSource])
void main() {
  late AuthRepositoryImpl repository;
  late MockAuthDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockAuthDataSource();
    repository = AuthRepositoryImpl(mockDataSource);
  });

  final testAuthResponse = AuthResponse(
    accessToken: 'access-token-123',
    refreshToken: 'refresh-token-123',
    expiresAt: DateTime.now().add(const Duration(hours: 1)),
    role: 'Customer',
    userId: 'user-123',
    email: 'test@getir.com',
    fullName: 'Test User',
  );

  group('AuthRepositoryImpl -', () {
    group('login', () {
      const email = 'test@getir.com';
      const password = 'password123';

      test('returns success with user when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.login(any),
        ).thenAnswer((_) async => testAuthResponse);

        // Act
        final result = await repository.login(email, password);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data.email, email);
        verify(mockDataSource.login(any)).called(1);
      });

      test('returns UnauthorizedException when credentials invalid', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/auth/login'),
          response: Response(
            requestOptions: RequestOptions(path: '/auth/login'),
            statusCode: 401,
            data: {'message': 'Invalid credentials'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.login(any)).thenThrow(dioException);

        // Act
        final result = await repository.login(email, password);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<UnauthorizedException>());
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/auth/login'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.login(any)).thenThrow(dioException);

        // Act
        final result = await repository.login(email, password);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });

    group('register', () {
      const email = 'test@getir.com';
      const password = 'password123';
      const firstName = 'Test';
      const lastName = 'User';

      test('returns success with user when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.register(any),
        ).thenAnswer((_) async => testAuthResponse);

        // Act
        final result = await repository.register(
          email,
          password,
          firstName,
          lastName,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data.email, email);
        verify(mockDataSource.register(any)).called(1);
      });

      test('returns ValidationException when email exists', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/auth/register'),
          response: Response(
            requestOptions: RequestOptions(path: '/auth/register'),
            statusCode: 400,
            data: {'message': 'Email already exists'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.register(any)).thenThrow(dioException);

        // Act
        final result = await repository.register(
          email,
          password,
          firstName,
          lastName,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });
    });

    group('logout', () {
      test(
        'returns success and clears tokens when datasource succeeds',
        () async {
          // Arrange
          when(mockDataSource.logout()).thenAnswer((_) async => {});
          when(mockDataSource.clearTokens()).thenAnswer((_) async {});
          when(mockDataSource.clearCurrentUser()).thenAnswer((_) async {});

          // Act
          final result = await repository.logout();

          // Assert
          expect(result.isSuccess, true);
          verify(mockDataSource.logout()).called(1);
          verify(mockDataSource.clearTokens()).called(1);
          verify(mockDataSource.clearCurrentUser()).called(1);
        },
      );

      test('clears local data even when API call fails', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/auth/logout'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.logout()).thenThrow(dioException);
        when(mockDataSource.clearTokens()).thenAnswer((_) async => {});
        when(mockDataSource.clearCurrentUser()).thenAnswer((_) async => {});

        // Act
        final result = await repository.logout();

        // Assert
        expect(result.isFailure, true);
        verify(mockDataSource.clearTokens()).called(1);
        verify(mockDataSource.clearCurrentUser()).called(1);
      });
    });

    group('refreshToken', () {
      const refreshToken = 'refresh-token-123';

      final testRefreshResponse = RefreshTokenResponse(
        accessToken: 'new-access-token',
        refreshToken: 'new-refresh-token',
        expiresAt: DateTime.now().add(const Duration(hours: 1)),
      );

      test('returns success with new user when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.refreshToken(any),
        ).thenAnswer((_) async => testRefreshResponse);
        when(
          mockDataSource.getCurrentUser(),
        ).thenAnswer((_) async => UserModel.fromDomain(MockData.testUser));

        // Act
        final result = await repository.refreshToken(refreshToken);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, isA<UserEntity>());
        verify(mockDataSource.refreshToken(any)).called(1);
      });

      test(
        'returns UnauthorizedException when refresh token invalid',
        () async {
          // Arrange
          final dioException = DioException(
            requestOptions: RequestOptions(path: '/auth/refresh'),
            response: Response(
              requestOptions: RequestOptions(path: '/auth/refresh'),
              statusCode: 401,
              data: {'message': 'Invalid refresh token'},
            ),
            type: DioExceptionType.badResponse,
          );
          when(mockDataSource.refreshToken(any)).thenThrow(dioException);

          // Act
          final result = await repository.refreshToken(refreshToken);

          // Assert
          expect(result.isFailure, true);
          expect(result.exception, isA<UnauthorizedException>());
        },
      );
    });

    group('forgotPassword', () {
      const email = 'test@getir.com';

      test('returns success when datasource succeeds', () async {
        // Arrange
        when(mockDataSource.forgotPassword(any)).thenAnswer((_) async => {});

        // Act
        final result = await repository.forgotPassword(email);

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.forgotPassword(any)).called(1);
      });

      test('returns NotFoundException when email not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/auth/forgot-password'),
          response: Response(
            requestOptions: RequestOptions(path: '/auth/forgot-password'),
            statusCode: 404,
            data: {'message': 'User not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.forgotPassword(any)).thenThrow(dioException);

        // Act
        final result = await repository.forgotPassword(email);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('resetPassword', () {
      const token = 'reset-token-123';
      const newPassword = 'newpassword123';

      test('returns success when datasource succeeds', () async {
        // Arrange
        when(mockDataSource.resetPassword(any)).thenAnswer((_) async => {});

        // Act
        final result = await repository.resetPassword(token, newPassword);

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.resetPassword(any)).called(1);
      });

      test('returns UnauthorizedException when token invalid', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/auth/reset-password'),
          response: Response(
            requestOptions: RequestOptions(path: '/auth/reset-password'),
            statusCode: 401,
            data: {'message': 'Invalid reset token'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.resetPassword(any)).thenThrow(dioException);

        // Act
        final result = await repository.resetPassword(token, newPassword);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<UnauthorizedException>());
      });
    });

    group('getAccessToken', () {
      test('returns token when stored', () async {
        // Arrange
        when(
          mockDataSource.getAccessToken(),
        ).thenAnswer((_) async => 'access-token-123');

        // Act
        final result = await repository.getAccessToken();

        // Assert
        expect(result, 'access-token-123');
      });

      test('returns null when no token stored', () async {
        // Arrange
        when(mockDataSource.getAccessToken()).thenAnswer((_) async => null);

        // Act
        final result = await repository.getAccessToken();

        // Assert
        expect(result, null);
      });
    });

    group('getCurrentUser', () {
      test('returns user when stored', () async {
        // Arrange
        when(
          mockDataSource.getCurrentUser(),
        ).thenAnswer((_) async => UserModel.fromDomain(MockData.testUser));

        // Act
        final result = await repository.getCurrentUser();

        // Assert
        expect(result, isA<UserEntity>());
        expect(result?.email, MockData.testUser.email);
      });

      test('returns null when no user stored', () async {
        // Arrange
        when(mockDataSource.getCurrentUser()).thenAnswer((_) async => null);

        // Act
        final result = await repository.getCurrentUser();

        // Assert
        expect(result, null);
      });
    });

    group('isAuthenticated', () {
      test('returns true when token exists', () async {
        // Arrange
        when(mockDataSource.isAuthenticated()).thenAnswer((_) async => true);

        // Act
        final result = await repository.isAuthenticated();

        // Assert
        expect(result, true);
      });

      test('returns false when no token', () async {
        // Arrange
        when(mockDataSource.isAuthenticated()).thenAnswer((_) async => false);

        // Act
        final result = await repository.isAuthenticated();

        // Assert
        expect(result, false);
      });
    });

    group('isTokenValid', () {
      test('returns true when token exists', () async {
        // Arrange
        when(mockDataSource.isTokenValid()).thenAnswer((_) async => true);

        // Act
        final result = await repository.isTokenValid();

        // Assert
        expect(result, true);
      });

      test('returns false when no token', () async {
        // Arrange
        when(mockDataSource.isTokenValid()).thenAnswer((_) async => false);

        // Act
        final result = await repository.isTokenValid();

        // Assert
        expect(result, false);
      });
    });
  });
}
