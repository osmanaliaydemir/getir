import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/core/services/encryption_service.dart';
import 'package:getir_mobile/core/services/logger_service.dart';
import 'package:get_it/get_it.dart';

@GenerateMocks([LoggerService])
import 'encryption_service_test.mocks.dart';

void main() {
  late EncryptionService service;
  late MockLoggerService mockLogger;
  final getIt = GetIt.instance;

  setUp(() {
    mockLogger = MockLoggerService();

    // Register logger
    if (!getIt.isRegistered<LoggerService>()) {
      getIt.registerSingleton<LoggerService>(mockLogger);
    }

    service = EncryptionService();
  });

  tearDown() async {
    // Clean up storage after each test
    await service.clearAll();
    getIt.reset();
  }

  group('EncryptionService - Token Operations', () {
    test('should save and retrieve access token', () async {
      // Arrange
      const token = 'test_access_token_123';

      // Act
      await service.saveAccessToken(token);
      final result = await service.getAccessToken();

      // Assert
      expect(result, equals(token));
    });

    test('should return null when access token does not exist', () async {
      // Act
      final result = await service.getAccessToken();

      // Assert
      expect(result, isNull);
    });

    test('should save and retrieve refresh token', () async {
      // Arrange
      const token = 'test_refresh_token_456';

      // Act
      await service.saveRefreshToken(token);
      final result = await service.getRefreshToken();

      // Assert
      expect(result, equals(token));
    });

    test('should return null when refresh token does not exist', () async {
      // Act
      final result = await service.getRefreshToken();

      // Assert
      expect(result, isNull);
    });

    test('should check if access token exists when token is saved', () async {
      // Arrange
      const token = 'test_token';
      await service.saveAccessToken(token);

      // Act
      final result = await service.hasAccessToken();

      // Assert
      expect(result, isTrue);
    });

    test('should return false when no access token exists', () async {
      // Act
      final result = await service.hasAccessToken();

      // Assert
      expect(result, isFalse);
    });

    test('should overwrite existing access token', () async {
      // Arrange
      const token1 = 'first_token';
      const token2 = 'second_token';
      await service.saveAccessToken(token1);

      // Act
      await service.saveAccessToken(token2);
      final result = await service.getAccessToken();

      // Assert
      expect(result, equals(token2));
    });

    test('should handle empty token strings', () async {
      // Arrange
      const token = '';

      // Act
      await service.saveAccessToken(token);
      final result = await service.getAccessToken();

      // Assert
      expect(result, equals(token));
    });
  });

  group('EncryptionService - User Credentials', () {
    test('should save and retrieve user credentials', () async {
      // Arrange
      const userId = 'user-123';
      const email = 'test@example.com';

      // Act
      await service.saveUserCredentials(userId: userId, email: email);
      final retrievedUserId = await service.getUserId();
      final retrievedEmail = await service.getUserEmail();

      // Assert
      expect(retrievedUserId, equals(userId));
      expect(retrievedEmail, equals(email));
    });

    test('should return null when user ID does not exist', () async {
      // Act
      final result = await service.getUserId();

      // Assert
      expect(result, isNull);
    });

    test('should return null when user email does not exist', () async {
      // Act
      final result = await service.getUserEmail();

      // Assert
      expect(result, isNull);
    });

    test('should overwrite existing user credentials', () async {
      // Arrange
      const userId1 = 'user-123';
      const email1 = 'test1@example.com';
      const userId2 = 'user-456';
      const email2 = 'test2@example.com';
      await service.saveUserCredentials(userId: userId1, email: email1);

      // Act
      await service.saveUserCredentials(userId: userId2, email: email2);
      final resultUserId = await service.getUserId();
      final resultEmail = await service.getUserEmail();

      // Assert
      expect(resultUserId, equals(userId2));
      expect(resultEmail, equals(email2));
    });
  });

  group('EncryptionService - Encrypted Data', () {
    test('should save and retrieve encrypted data', () async {
      // Arrange
      const key = 'sensitive_data_key';
      const data = 'sensitive_information';

      // Act
      await service.saveEncryptedData(key, data);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, equals(data));
    });

    test('should return null when encrypted data does not exist', () async {
      // Arrange
      const key = 'non_existent_key';

      // Act
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, isNull);
    });

    test('should handle empty string encryption', () async {
      // Arrange
      const key = 'empty_key';
      const data = '';

      // Act
      await service.saveEncryptedData(key, data);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, equals(data));
    });

    test('should handle long string encryption', () async {
      // Arrange
      const key = 'long_key';
      final data = 'A' * 1000;

      // Act
      await service.saveEncryptedData(key, data);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, equals(data));
    });

    test('should handle special characters in data', () async {
      // Arrange
      const key = 'special_chars';
      const data = '!@#\$%^&*()_+-={}[]|\\:";\'<>?,./~`';

      // Act
      await service.saveEncryptedData(key, data);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, equals(data));
    });

    test('should handle unicode characters in data', () async {
      // Arrange
      const key = 'unicode_key';
      const data = '你好世界 🌍 مرحبا العالم';

      // Act
      await service.saveEncryptedData(key, data);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, equals(data));
    });

    test('should overwrite existing encrypted data', () async {
      // Arrange
      const key = 'overwrite_key';
      const data1 = 'first_data';
      const data2 = 'second_data';
      await service.saveEncryptedData(key, data1);

      // Act
      await service.saveEncryptedData(key, data2);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, equals(data2));
    });
  });

  group('EncryptionService - Deletion Operations', () {
    test('should delete specific key', () async {
      // Arrange
      const key = 'key_to_delete';
      const data = 'test_data';
      await service.saveEncryptedData(key, data);

      // Act
      await service.delete(key);
      final result = await service.getEncryptedData(key);

      // Assert
      expect(result, isNull);
    });

    test('should not throw when deleting non-existent key', () async {
      // Arrange
      const key = 'non_existent_key';

      // Act & Assert
      expect(() => service.delete(key), returnsNormally);
    });

    test('should clear all secure storage', () async {
      // Arrange
      await service.saveAccessToken('token1');
      await service.saveRefreshToken('token2');
      await service.saveUserCredentials(
        userId: 'user1',
        email: 'test@test.com',
      );

      // Act
      await service.clearAll();

      // Assert
      final token1 = await service.getAccessToken();
      final token2 = await service.getRefreshToken();
      final userId = await service.getUserId();

      expect(token1, isNull);
      expect(token2, isNull);
      expect(userId, isNull);
    });

    test('should not throw when clearing empty storage', () async {
      // Act & Assert
      expect(() => service.clearAll(), returnsNormally);
    });
  });

  group('EncryptionService - Initialization', () {
    test('should initialize successfully', () async {
      // Act & Assert
      expect(() => service.initialize(), returnsNormally);
    });

    test('should not throw on repeated initialization', () async {
      // Act
      await service.initialize();

      // Assert
      expect(() => service.initialize(), returnsNormally);
    });
  });

  group('EncryptionService - Edge Cases', () {
    test('should handle multiple operations in sequence', () async {
      // Arrange & Act
      await service.saveAccessToken('token1');
      final t1 = await service.getAccessToken();
      await service.saveRefreshToken('token2');
      final t2 = await service.getRefreshToken();
      await service.saveUserCredentials(
        userId: 'user1',
        email: 'test@test.com',
      );
      final user = await service.getUserId();

      // Assert
      expect(t1, equals('token1'));
      expect(t2, equals('token2'));
      expect(user, equals('user1'));
    });

    test('should handle rapid save and read operations', () async {
      // Arrange
      const key = 'rapid_key';

      // Act & Assert
      for (int i = 0; i < 10; i++) {
        final data = 'data_$i';
        await service.saveEncryptedData(key, data);
        final result = await service.getEncryptedData(key);
        expect(result, equals(data));
      }
    });

    test('should handle very long keys', () async {
      // Arrange
      final longKey = 'key' * 100;
      const data = 'test_data';

      // Act
      await service.saveEncryptedData(longKey, data);
      final result = await service.getEncryptedData(longKey);

      // Assert
      expect(result, equals(data));
    });

    test('should maintain data isolation between different keys', () async {
      // Arrange & Act
      await service.saveEncryptedData('key1', 'data1');
      await service.saveEncryptedData('key2', 'data2');
      await service.saveEncryptedData('key3', 'data3');

      // Assert
      expect(await service.getEncryptedData('key1'), equals('data1'));
      expect(await service.getEncryptedData('key2'), equals('data2'));
      expect(await service.getEncryptedData('key3'), equals('data3'));
    });
  });
}
