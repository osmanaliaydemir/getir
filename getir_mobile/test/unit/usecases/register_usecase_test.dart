import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/domain/repositories/auth_repository.dart';
import 'package:getir_mobile/domain/usecases/auth_usecases.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([AuthRepository])
import 'register_usecase_test.mocks.dart';

void main() {
  late RegisterUseCase useCase;
  late MockAuthRepository mockRepository;

  setUp(() {
    mockRepository = MockAuthRepository();
    useCase = RegisterUseCase(mockRepository);
  });

  group('RegisterUseCase', () {
    const testEmail = 'newuser@getir.com';
    const testPassword = 'SecurePass123';
    const testFirstName = 'John';
    const testLastName = 'Doe';
    const testPhone = '+905551234567';

    test('should return UserEntity when registration data is valid', () async {
      // Arrange
      final expectedUser = MockData.testUser;
      when(
        mockRepository.register(
          any,
          any,
          any,
          any,
          phoneNumber: anyNamed('phoneNumber'),
        ),
      ).thenAnswer((_) async => expectedUser);

      // Act
      final result = await useCase(
        testEmail,
        testPassword,
        testFirstName,
        testLastName,
        phoneNumber: testPhone,
      );

      // Assert
      expect(result, equals(expectedUser));
      verify(
        mockRepository.register(
          testEmail.trim().toLowerCase(),
          testPassword,
          testFirstName.trim(),
          testLastName.trim(),
          phoneNumber: testPhone.trim(),
        ),
      ).called(1);
    });

    test('should sanitize inputs (trim and lowercase email)', () async {
      // Arrange
      const dirtyEmail = '  NEW USER@GETIR.COM  ';
      const dirtyFirstName = '  John  ';
      const dirtyLastName = '  Doe  ';
      const dirtyPhone = '  +905551234567  ';
      final expectedUser = MockData.testUser;

      when(
        mockRepository.register(
          any,
          any,
          any,
          any,
          phoneNumber: anyNamed('phoneNumber'),
        ),
      ).thenAnswer((_) async => expectedUser);

      // Act
      await useCase(
        dirtyEmail,
        testPassword,
        dirtyFirstName,
        dirtyLastName,
        phoneNumber: dirtyPhone,
      );

      // Assert
      verify(
        mockRepository.register(
          'newuser@getir.com',
          testPassword,
          'John',
          'Doe',
          phoneNumber: '+905551234567',
        ),
      ).called(1);
    });

    test('should throw ArgumentError when required fields are empty', () async {
      // Test empty email
      expect(
        () => useCase('', testPassword, testFirstName, testLastName),
        throwsA(isA<ArgumentError>()),
      );

      // Test empty password
      expect(
        () => useCase(testEmail, '', testFirstName, testLastName),
        throwsA(isA<ArgumentError>()),
      );

      // Test empty first name
      expect(
        () => useCase(testEmail, testPassword, '', testLastName),
        throwsA(isA<ArgumentError>()),
      );

      // Test empty last name
      expect(
        () => useCase(testEmail, testPassword, testFirstName, ''),
        throwsA(isA<ArgumentError>()),
      );

      verifyNever(
        mockRepository.register(
          any,
          any,
          any,
          any,
          phoneNumber: anyNamed('phoneNumber'),
        ),
      );
    });

    test('should throw ArgumentError when email format is invalid', () async {
      // Arrange
      const invalidEmail = 'invalid-email';

      // Act & Assert
      expect(
        () => useCase(invalidEmail, testPassword, testFirstName, testLastName),
        throwsA(
          predicate(
            (e) =>
                e is ArgumentError &&
                e.message.toString().contains('Invalid email format'),
          ),
        ),
      );
    });

    test('should throw ArgumentError when password is too short', () async {
      // Arrange
      const shortPassword = '12345'; // Less than 6 chars

      // Act & Assert
      expect(
        () => useCase(testEmail, shortPassword, testFirstName, testLastName),
        throwsA(
          predicate(
            (e) =>
                e is ArgumentError &&
                e.message.toString().contains('at least 6 characters'),
          ),
        ),
      );
    });

    test('should throw ArgumentError when name is too short', () async {
      // Test short first name
      expect(
        () => useCase(testEmail, testPassword, 'J', testLastName),
        throwsA(
          predicate(
            (e) =>
                e is ArgumentError &&
                e.message.toString().contains('at least 2 characters'),
          ),
        ),
      );

      // Test short last name
      expect(
        () => useCase(testEmail, testPassword, testFirstName, 'D'),
        throwsA(
          predicate(
            (e) =>
                e is ArgumentError &&
                e.message.toString().contains('at least 2 characters'),
          ),
        ),
      );
    });

    test('should throw ArgumentError when phone format is invalid', () async {
      // Arrange
      const invalidPhones = [
        '123', // Too short
        'abc', // Not numeric
        '++123', // Invalid format
      ];

      // Act & Assert
      for (final phone in invalidPhones) {
        expect(
          () => useCase(
            testEmail,
            testPassword,
            testFirstName,
            testLastName,
            phoneNumber: phone,
          ),
          throwsA(
            predicate(
              (e) =>
                  e is ArgumentError &&
                  e.message.toString().contains('Invalid phone number'),
            ),
          ),
        );
      }
    });

    test('should accept registration without phone number', () async {
      // Arrange
      final expectedUser = MockData.testUser;
      when(
        mockRepository.register(
          any,
          any,
          any,
          any,
          phoneNumber: anyNamed('phoneNumber'),
        ),
      ).thenAnswer((_) async => expectedUser);

      // Act
      final result = await useCase(
        testEmail,
        testPassword,
        testFirstName,
        testLastName,
      ); // No phone number

      // Assert
      expect(result, equals(expectedUser));
      verify(
        mockRepository.register(
          testEmail.trim().toLowerCase(),
          testPassword,
          testFirstName.trim(),
          testLastName.trim(),
          phoneNumber: null,
        ),
      ).called(1);
    });

    test('should propagate repository exceptions', () async {
      // Arrange
      when(
        mockRepository.register(
          any,
          any,
          any,
          any,
          phoneNumber: anyNamed('phoneNumber'),
        ),
      ).thenThrow(Exception('Email already exists'));

      // Act & Assert
      expect(
        () => useCase(testEmail, testPassword, testFirstName, testLastName),
        throwsException,
      );
    });
  });
}
