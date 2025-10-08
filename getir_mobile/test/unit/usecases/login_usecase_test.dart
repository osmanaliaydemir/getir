import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/domain/repositories/auth_repository.dart';
import 'package:getir_mobile/domain/usecases/auth_usecases.dart';
import '../../helpers/mock_data.dart';

// Generate mocks
@GenerateMocks([AuthRepository])
import 'login_usecase_test.mocks.dart';

void main() {
  late LoginUseCase useCase;
  late MockAuthRepository mockRepository;

  setUp(() {
    mockRepository = MockAuthRepository();
    useCase = LoginUseCase(mockRepository);
  });

  group('LoginUseCase', () {
    const testEmail = 'test@getir.com';
    const testPassword = 'Test123456';

    test('should return UserEntity when credentials are valid', () async {
      // Arrange
      final expectedUser = MockData.testUser;
      when(
        mockRepository.login(any, any),
      ).thenAnswer((_) async => expectedUser);

      // Act
      final result = await useCase(testEmail, testPassword);

      // Assert
      expect(result, equals(expectedUser));
      verify(
        mockRepository.login(testEmail.trim().toLowerCase(), testPassword),
      ).called(1);
    });

    test('should sanitize email (trim and lowercase)', () async {
      // Arrange
      const dirtyEmail = '  TEST@GETIR.COM  ';
      final expectedUser = MockData.testUser;
      when(
        mockRepository.login(any, any),
      ).thenAnswer((_) async => expectedUser);

      // Act
      await useCase(dirtyEmail, testPassword);

      // Assert
      verify(mockRepository.login('test@getir.com', testPassword)).called(1);
    });

    test('should throw ArgumentError when email is empty', () async {
      // Act & Assert
      expect(() => useCase('', testPassword), throwsA(isA<ArgumentError>()));
      verifyNever(mockRepository.login(any, any));
    });

    test('should throw ArgumentError when password is empty', () async {
      // Act & Assert
      expect(() => useCase(testEmail, ''), throwsA(isA<ArgumentError>()));
      verifyNever(mockRepository.login(any, any));
    });

    test('should throw ArgumentError when email format is invalid', () async {
      // Arrange
      const invalidEmail = 'invalid-email-format';

      // Act & Assert
      expect(
        () => useCase(invalidEmail, testPassword),
        throwsA(
          predicate(
            (e) =>
                e is ArgumentError &&
                e.message.toString().contains('Invalid email format'),
          ),
        ),
      );
      verifyNever(mockRepository.login(any, any));
    });

    test('should throw ArgumentError when password is too short', () async {
      // Arrange
      const shortPassword = '12345'; // Less than 6 chars

      // Act & Assert
      expect(
        () => useCase(testEmail, shortPassword),
        throwsA(
          predicate(
            (e) =>
                e is ArgumentError &&
                e.message.toString().contains('at least 6 characters'),
          ),
        ),
      );
      verifyNever(mockRepository.login(any, any));
    });

    test('should propagate repository exceptions', () async {
      // Arrange
      when(
        mockRepository.login(any, any),
      ).thenThrow(Exception('Network error'));

      // Act & Assert
      expect(() => useCase(testEmail, testPassword), throwsException);
    });

    test('should accept valid email formats', () async {
      // Arrange
      final validEmails = [
        'user@example.com',
        'test.user@getir.com',
        'user+tag@example.co.uk',
        'user_name@test-domain.com',
      ];
      final expectedUser = MockData.testUser;
      when(
        mockRepository.login(any, any),
      ).thenAnswer((_) async => expectedUser);

      // Act & Assert
      for (final email in validEmails) {
        final result = await useCase(email, testPassword);
        expect(result, equals(expectedUser));
      }
    });

    test('should reject invalid email formats', () async {
      // Arrange
      final invalidEmails = [
        'not-an-email',
        '@example.com',
        'user@',
        'user @example.com',
        'user@.com',
      ];

      // Act & Assert
      for (final email in invalidEmails) {
        expect(
          () => useCase(email, testPassword),
          throwsA(isA<ArgumentError>()),
        );
      }
    });
  });
}
