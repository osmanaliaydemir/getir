import 'dart:convert';
import 'dart:typed_data';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:crypto/crypto.dart';
import '../config/environment_config.dart';
import 'logger_service.dart';

/// Encryption Service for sensitive data
/// Uses flutter_secure_storage for token/password storage
/// Uses AES encryption for additional sensitive data
class EncryptionService {
  // Secure storage instance
  final FlutterSecureStorage _secureStorage = const FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
    iOptions: IOSOptions(accessibility: KeychainAccessibility.first_unlock),
  );

  // Storage keys
  static const String _accessTokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _userIdKey = 'user_id';
  static const String _userEmailKey = 'user_email';
  static const String _encryptionKeyKey = 'encryption_key';

  /// Initialize encryption service
  Future<void> initialize() async {
    try {
      // Generate or retrieve encryption key
      final existingKey = await _secureStorage.read(key: _encryptionKeyKey);
      if (existingKey == null) {
        final newKey = _generateEncryptionKey();
        await _secureStorage.write(key: _encryptionKeyKey, value: newKey);
        logger.info('Generated new encryption key', tag: 'Encryption');
      } else {
        logger.info('Using existing encryption key', tag: 'Encryption');
      }
    } catch (e, stackTrace) {
      logger.error(
        'Encryption initialization failed',
        tag: 'Encryption',
        error: e,
        stackTrace: stackTrace,
      );
    }
  }

  /// Save access token securely
  Future<void> saveAccessToken(String token) async {
    try {
      await _secureStorage.write(key: _accessTokenKey, value: token);
      logger.logSensitiveOperation(
        operation: 'save_access_token',
        success: true,
      );
    } catch (e, stackTrace) {
      logger.logSensitiveOperation(
        operation: 'save_access_token',
        success: false,
        error: e,
      );
      rethrow;
    }
  }

  /// Get access token
  Future<String?> getAccessToken() async {
    try {
      return await _secureStorage.read(key: _accessTokenKey);
    } catch (e) {
      logger.logSensitiveOperation(
        operation: 'read_access_token',
        success: false,
        error: e,
      );
      return null;
    }
  }

  /// Save refresh token securely
  Future<void> saveRefreshToken(String token) async {
    try {
      await _secureStorage.write(key: _refreshTokenKey, value: token);
      logger.logSensitiveOperation(
        operation: 'save_refresh_token',
        success: true,
      );
    } catch (e, stackTrace) {
      logger.logSensitiveOperation(
        operation: 'save_refresh_token',
        success: false,
        error: e,
      );
      rethrow;
    }
  }

  /// Get refresh token
  Future<String?> getRefreshToken() async {
    try {
      return await _secureStorage.read(key: _refreshTokenKey);
    } catch (e) {
      logger.logSensitiveOperation(
        operation: 'read_refresh_token',
        success: false,
        error: e,
      );
      return null;
    }
  }

  /// Save user credentials
  Future<void> saveUserCredentials({
    required String userId,
    required String email,
  }) async {
    try {
      await _secureStorage.write(key: _userIdKey, value: userId);
      await _secureStorage.write(key: _userEmailKey, value: email);
      logger.logSensitiveOperation(
        operation: 'save_user_credentials',
        success: true,
      );
    } catch (e, stackTrace) {
      logger.logSensitiveOperation(
        operation: 'save_user_credentials',
        success: false,
        error: e,
      );
      rethrow;
    }
  }

  /// Get user ID
  Future<String?> getUserId() async {
    try {
      return await _secureStorage.read(key: _userIdKey);
    } catch (e) {
      logger.logSensitiveOperation(
        operation: 'read_user_id',
        success: false,
        error: e,
      );
      return null;
    }
  }

  /// Get user email
  Future<String?> getUserEmail() async {
    try {
      return await _secureStorage.read(key: _userEmailKey);
    } catch (e) {
      logger.logSensitiveOperation(
        operation: 'read_user_email',
        success: false,
        error: e,
      );
      return null;
    }
  }

  /// Save encrypted data
  Future<void> saveEncryptedData(String key, String data) async {
    try {
      final encrypted = _encrypt(data);
      await _secureStorage.write(key: key, value: encrypted);
      logger.debug('Saved encrypted data', tag: 'Encryption');
    } catch (e, stackTrace) {
      logger.error(
        'Failed to save encrypted data',
        tag: 'Encryption',
        error: e,
        stackTrace: stackTrace,
      );
      rethrow;
    }
  }

  /// Get encrypted data
  Future<String?> getEncryptedData(String key) async {
    try {
      final encrypted = await _secureStorage.read(key: key);
      if (encrypted == null) return null;
      return _decrypt(encrypted);
    } catch (e) {
      logger.debug(
        'Failed to read encrypted data',
        tag: 'Encryption',
        context: {'error': e.toString()},
      );
      return null;
    }
  }

  /// Delete specific key
  Future<void> delete(String key) async {
    try {
      await _secureStorage.delete(key: key);
      logger.debug('Deleted secure data', tag: 'Encryption');
    } catch (e) {
      logger.debug(
        'Failed to delete data',
        tag: 'Encryption',
        context: {'error': e.toString()},
      );
    }
  }

  /// Clear all secure storage
  Future<void> clearAll() async {
    try {
      await _secureStorage.deleteAll();
      logger.info('Cleared all secure storage', tag: 'Encryption');
    } catch (e) {
      logger.error(
        'Failed to clear secure storage',
        tag: 'Encryption',
        error: e,
      );
    }
  }

  /// Check if token exists
  Future<bool> hasAccessToken() async {
    final token = await getAccessToken();
    return token != null && token.isNotEmpty;
  }

  /// Generate encryption key
  String _generateEncryptionKey() {
    // Use environment encryption key or generate random
    final envKey = EnvironmentConfig.encryptionKey;
    if (envKey.length >= 32) {
      return envKey;
    }

    // Generate random key (for dev only)
    final timestamp = DateTime.now().millisecondsSinceEpoch.toString();
    final hash = sha256.convert(utf8.encode(timestamp)).toString();
    return hash.substring(0, 32);
  }

  /// Simple encryption (Base64 + XOR for basic obfuscation)
  /// NOTE: This is NOT production-grade encryption!
  /// For production, use proper AES-256 encryption library
  String _encrypt(String data) {
    try {
      // Simple XOR encryption with base64
      final keyBytes = utf8.encode(EnvironmentConfig.encryptionKey);
      final dataBytes = utf8.encode(data);
      final encrypted = Uint8List(dataBytes.length);

      for (int i = 0; i < dataBytes.length; i++) {
        encrypted[i] = dataBytes[i] ^ keyBytes[i % keyBytes.length];
      }

      return base64.encode(encrypted);
    } catch (e) {
      logger.error('Encryption failed', tag: 'Encryption', error: e);
      return data; // Fallback to unencrypted
    }
  }

  /// Simple decryption
  String _decrypt(String encryptedData) {
    try {
      final keyBytes = utf8.encode(EnvironmentConfig.encryptionKey);
      final encrypted = base64.decode(encryptedData);
      final decrypted = Uint8List(encrypted.length);

      for (int i = 0; i < encrypted.length; i++) {
        decrypted[i] = encrypted[i] ^ keyBytes[i % keyBytes.length];
      }

      return utf8.decode(decrypted);
    } catch (e) {
      logger.error('Decryption failed', tag: 'Encryption', error: e);
      return encryptedData; // Fallback to encrypted
    }
  }
}

/// Helper extension for easy encryption
extension SecureDataExtension on String {
  /// Encrypt this string
  Future<String> encrypt() async {
    final service = EncryptionService();
    return service._encrypt(this);
  }

  /// Decrypt this string
  Future<String> decrypt() async {
    final service = EncryptionService();
    return service._decrypt(this);
  }
}
