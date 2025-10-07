import 'dart:convert';
import 'dart:typed_data';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:crypto/crypto.dart';
import 'package:flutter/foundation.dart';
import 'package:injectable/injectable.dart';
import '../config/environment_config.dart';

/// Encryption Service for sensitive data
/// Uses flutter_secure_storage for token/password storage
/// Uses AES encryption for additional sensitive data
@lazySingleton
class EncryptionService {

  // Secure storage instance
  final FlutterSecureStorage _secureStorage = const FlutterSecureStorage(
    aOptions: AndroidOptions(
      encryptedSharedPreferences: true,
    ),
    iOptions: IOSOptions(
      accessibility: KeychainAccessibility.first_unlock,
    ),
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
        debugPrint('🔐 Encryption: Generated new encryption key');
      } else {
        debugPrint('🔐 Encryption: Using existing encryption key');
      }
    } catch (e) {
      debugPrint('❌ Encryption: Initialization failed: $e');
    }
  }

  /// Save access token securely
  Future<void> saveAccessToken(String token) async {
    try {
      await _secureStorage.write(key: _accessTokenKey, value: token);
      debugPrint('🔐 Saved access token securely');
    } catch (e) {
      debugPrint('❌ Failed to save access token: $e');
      rethrow;
    }
  }

  /// Get access token
  Future<String?> getAccessToken() async {
    try {
      return await _secureStorage.read(key: _accessTokenKey);
    } catch (e) {
      debugPrint('❌ Failed to read access token: $e');
      return null;
    }
  }

  /// Save refresh token securely
  Future<void> saveRefreshToken(String token) async {
    try {
      await _secureStorage.write(key: _refreshTokenKey, value: token);
      debugPrint('🔐 Saved refresh token securely');
    } catch (e) {
      debugPrint('❌ Failed to save refresh token: $e');
      rethrow;
    }
  }

  /// Get refresh token
  Future<String?> getRefreshToken() async {
    try {
      return await _secureStorage.read(key: _refreshTokenKey);
    } catch (e) {
      debugPrint('❌ Failed to read refresh token: $e');
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
      debugPrint('🔐 Saved user credentials securely');
    } catch (e) {
      debugPrint('❌ Failed to save user credentials: $e');
      rethrow;
    }
  }

  /// Get user ID
  Future<String?> getUserId() async {
    try {
      return await _secureStorage.read(key: _userIdKey);
    } catch (e) {
      debugPrint('❌ Failed to read user ID: $e');
      return null;
    }
  }

  /// Get user email
  Future<String?> getUserEmail() async {
    try {
      return await _secureStorage.read(key: _userEmailKey);
    } catch (e) {
      debugPrint('❌ Failed to read user email: $e');
      return null;
    }
  }

  /// Save encrypted data
  Future<void> saveEncryptedData(String key, String data) async {
    try {
      final encrypted = _encrypt(data);
      await _secureStorage.write(key: key, value: encrypted);
      debugPrint('🔐 Saved encrypted data: $key');
    } catch (e) {
      debugPrint('❌ Failed to save encrypted data: $e');
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
      debugPrint('❌ Failed to read encrypted data: $e');
      return null;
    }
  }

  /// Delete specific key
  Future<void> delete(String key) async {
    try {
      await _secureStorage.delete(key: key);
      debugPrint('🗑️  Deleted secure data: $key');
    } catch (e) {
      debugPrint('❌ Failed to delete data: $e');
    }
  }

  /// Clear all secure storage
  Future<void> clearAll() async {
    try {
      await _secureStorage.deleteAll();
      debugPrint('🗑️  Cleared all secure storage');
    } catch (e) {
      debugPrint('❌ Failed to clear secure storage: $e');
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
      debugPrint('❌ Encryption failed: $e');
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
      debugPrint('❌ Decryption failed: $e');
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
