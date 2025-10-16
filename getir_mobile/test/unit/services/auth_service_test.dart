import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/domain/repositories/auth_repository.dart';
import 'package:getir_mobile/domain/services/auth_service.dart';

import '../../helpers/mock_data.dart';
import 'auth_service_test.mocks.dart';

@GenerateMocks([IAuthRepository])
void main() {
  late AuthService service;
  late MockAuthRepository mockRepository;

  setUp(() {
    mockRepository = MockAuthRepository();
    service = AuthService(mockRepository);
  });

  group('AuthService -', () {
    group('login', () {
      const email = 'test@getir.com';
      const password = 'password123';

      test('returns user when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.login(any, any),
        ).thenAnswer((_) async => Result.success(MockData.testUser));

        // Act
        final result = await service.login(email, password);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testUser);
        verify(mockRepository.login(email, password)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = UnauthorizedException(message: 'Invalid credentials');
        when(
          mockRepository.login(any, any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.login(email, password);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns validation error for empty email', () async {
        // Act
        final result = await service.login('', password);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
        verifyNever(mockRepository.login(any, any));
      });

      test('returns validation error for invalid email format', () async {
        // Act
        final result = await service.login('invalid-email', password);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
        verifyNever(mockRepository.login(any, any));
      });

      test('returns validation error for short password', () async {
        // Act
        final result = await service.login(email, '123');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
        verifyNever(mockRepository.login(any, any));
      });
    });

    group('register', () {
      const email = 'test@getir.com';
      const password = 'password123';
      const firstName = 'Test';
      const lastName = 'User';
      const phoneNumber = '+905551234567';

      test('returns user when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.register(
            any,
            any,
            any,
            any,
            phoneNumber: anyNamed('phoneNumber'),
          ),
        ).thenAnswer((_) async => Result.success(MockData.testUser));

        // Act
        final result = await service.register(
          email: email,
          password: password,
          firstName: firstName,
          lastName: lastName,
          phoneNumber: phoneNumber,
        );

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testUser);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = ValidationException(message: 'Email already exists');
        when(
          mockRepository.register(
            any,
            any,
            any,
            any,
            phoneNumber: anyNamed('phoneNumber'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.register(
          email: email,
          password: password,
          firstName: firstName,
          lastName: lastName,
          phoneNumber: phoneNumber,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns validation error for invalid email', () async {
        // Act
        final result = await service.register(
          email: 'invalid-email',
          password: password,
          firstName: firstName,
          lastName: lastName,
          phoneNumber: phoneNumber,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });
    });

    group('logout', () {
      test('returns success when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.logout(),
        ).thenAnswer((_) async => Result.success(null));

        // Act
        final result = await service.logout();

        // Assert
        expect(result.isSuccess, true);
        verify(mockRepository.logout()).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = StorageException(message: 'Storage error');
        when(
          mockRepository.logout(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.logout();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('refreshToken', () {
      test('returns user with new tokens when call succeeds', () async {
        // Arrange
        when(
          mockRepository.getRefreshToken(),
        ).thenAnswer((_) async => 'refresh-token-123');
        when(
          mockRepository.refreshToken(any),
        ).thenAnswer((_) async => Result.success(MockData.testUser));

        // Act
        final result = await service.refreshToken();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testUser);
        verify(mockRepository.refreshToken('refresh-token-123')).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        when(
          mockRepository.getRefreshToken(),
        ).thenAnswer((_) async => 'refresh-token-123');
        final exception = UnauthorizedException(
          message: 'Invalid refresh token',
        );
        when(
          mockRepository.refreshToken(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.refreshToken();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('checkTokenValidity', () {
      test('returns true when token is valid', () async {
        // Arrange
        when(mockRepository.isTokenValid()).thenAnswer((_) async => true);

        // Act
        final result = await service.checkTokenValidity();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, true);
        verify(mockRepository.isTokenValid()).called(1);
      });

      test('returns false when token is invalid', () async {
        // Arrange
        when(mockRepository.isTokenValid()).thenAnswer((_) async => false);

        // Act
        final result = await service.checkTokenValidity();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, false);
      });
    });

    group('forgotPassword', () {
      const email = 'test@getir.com';

      test('returns success when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.forgotPassword(any),
        ).thenAnswer((_) async => Result.success(null));

        // Act
        final result = await service.forgotPassword(email);

        // Assert
        expect(result.isSuccess, true);
        verify(mockRepository.forgotPassword(email)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'User not found');
        when(
          mockRepository.forgotPassword(any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.forgotPassword(email);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns validation error for invalid email', () async {
        // Act
        final result = await service.forgotPassword('invalid-email');

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });
    });

    group('resetPassword', () {
      const email = 'test@getir.com';
      const resetCode = 'reset-token-123';
      const newPassword = 'newpassword123';

      test('returns success when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.resetPassword(any, any),
        ).thenAnswer((_) async => Result.success(null));

        // Act
        final result = await service.resetPassword(
          email: email,
          resetCode: resetCode,
          newPassword: newPassword,
        );

        // Assert
        expect(result.isSuccess, true);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = UnauthorizedException(message: 'Invalid reset code');
        when(
          mockRepository.resetPassword(any, any),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.resetPassword(
          email: email,
          resetCode: resetCode,
          newPassword: newPassword,
        );

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getCurrentUser', () {
      test('returns current user when user exists', () async {
        // Arrange
        when(
          mockRepository.getCurrentUser(),
        ).thenAnswer((_) async => MockData.testUser);

        // Act
        final result = await service.getCurrentUser();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testUser);
        verify(mockRepository.getCurrentUser()).called(1);
      });

      test('returns failure when no user authenticated', () async {
        // Arrange
        when(mockRepository.getCurrentUser()).thenAnswer((_) async => null);

        // Act
        final result = await service.getCurrentUser();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<UnauthorizedException>());
      });
    });

    group('checkAuthentication', () {
      test('returns true when user is authenticated', () async {
        // Arrange
        when(mockRepository.isAuthenticated()).thenAnswer((_) async => true);

        // Act
        final result = await service.checkAuthentication();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, true);
        verify(mockRepository.isAuthenticated()).called(1);
      });

      test('returns false when user is not authenticated', () async {
        // Arrange
        when(mockRepository.isAuthenticated()).thenAnswer((_) async => false);

        // Act
        final result = await service.checkAuthentication();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, false);
      });
    });
  });
}
