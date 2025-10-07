import 'package:hive/hive.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter/foundation.dart';
import 'package:injectable/injectable.dart';

@lazySingleton
class LocalStorageService {

  late Box<Map> _cacheBox;
  late Box<String> _userBox;
  late Box<List> _queueBox;
  final FlutterSecureStorage _secure = const FlutterSecureStorage();

  // Initialize Hive boxes
  Future<void> initialize() async {
    try {
      // Open cache box for API responses
      _cacheBox = await Hive.openBox<Map>('cache');

      // Open user data box
      _userBox = await Hive.openBox<String>('user_data');

      // Open sync queue box for offline actions
      _queueBox = await Hive.openBox<List>('sync_queue');

      if (kDebugMode) {
        print('Local storage initialized successfully');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Local storage initialization failed: $e');
      }
      rethrow;
    }
  }

  // Cache API responses
  Future<void> cacheApiResponse(
    String endpoint,
    Map<String, dynamic> data, {
    Duration? expiry,
  }) async {
    try {
      final cacheKey = _generateCacheKey(endpoint);
      final cacheData = {
        'data': data,
        'timestamp': DateTime.now().millisecondsSinceEpoch,
        'expiry': expiry?.inMilliseconds,
      };
      await _cacheBox.put(cacheKey, cacheData);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to cache API response: $e');
      }
    }
  }

  // Cache API response by explicit cache key
  Future<void> putCacheByKey(
    String cacheKey,
    Map<String, dynamic> data, {
    Duration? expiry,
  }) async {
    try {
      final cacheData = {
        'data': data,
        'timestamp': DateTime.now().millisecondsSinceEpoch,
        'expiry': expiry?.inMilliseconds,
      };
      await _cacheBox.put(cacheKey, cacheData);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to cache (by key) API response: $e');
      }
    }
  }

  // Get cached API response
  Future<Map<String, dynamic>?> getCachedApiResponse(String endpoint) async {
    try {
      final cacheKey = _generateCacheKey(endpoint);
      final cached = _cacheBox.get(cacheKey);

      if (cached == null) return null;

      // Check if cache is expired
      final timestamp = cached['timestamp'] as int?;
      final expiry = cached['expiry'] as int?;

      if (timestamp != null && expiry != null) {
        final now = DateTime.now().millisecondsSinceEpoch;
        if (now - timestamp > expiry) {
          await _cacheBox.delete(cacheKey);
          return null;
        }
      }

      return Map<String, dynamic>.from(cached['data'] as Map);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get cached API response: $e');
      }
      return null;
    }
  }

  // Get cached API response by explicit cache key
  Future<Map<String, dynamic>?> getCacheByKey(String cacheKey) async {
    try {
      final cached = _cacheBox.get(cacheKey);
      if (cached == null) return null;
      return Map<String, dynamic>.from(cached['data'] as Map);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get cached (by key) API response: $e');
      }
      return null;
    }
  }

  // Get raw cached record (includes timestamp/expiry)
  Future<Map<String, dynamic>?> getRawCacheByKey(String cacheKey) async {
    try {
      final cached = _cacheBox.get(cacheKey);
      if (cached == null) return null;
      return Map<String, dynamic>.from(cached);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get raw cache by key: $e');
      }
      return null;
    }
  }

  // Clear expired cache entries
  Future<void> clearExpiredCache() async {
    try {
      final now = DateTime.now().millisecondsSinceEpoch;
      final keysToDelete = <String>[];

      for (final key in _cacheBox.keys) {
        final cached = _cacheBox.get(key);
        if (cached != null) {
          final timestamp = cached['timestamp'] as int?;
          final expiry = cached['expiry'] as int?;

          if (timestamp != null && expiry != null && now - timestamp > expiry) {
            keysToDelete.add(key.toString());
          }
        }
      }

      for (final key in keysToDelete) {
        await _cacheBox.delete(key);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to clear expired cache: $e');
      }
    }
  }

  // Store user data
  Future<void> storeUserData(String key, String value) async {
    try {
      if (key.contains('token')) {
        await _secure.write(key: key, value: value);
      } else {
        await _userBox.put(key, value);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to store user data: $e');
      }
    }
  }

  // Get user data
  String? getUserData(String key) {
    try {
      // Note: secure storage is async; provide sync fallback for non-secure keys
      if (key.contains('token')) {
        // Caller should prefer getUserDataAsync for secure keys
        return null;
      }
      return _userBox.get(key);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get user data: $e');
      }
      return null;
    }
  }

  // Async variant to read secure keys (e.g., tokens)
  Future<String?> getUserDataAsync(String key) async {
    try {
      if (key.contains('token')) {
        return await _secure.read(key: key);
      }
      return _userBox.get(key);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get user data async: $e');
      }
      return null;
    }
  }

  // Remove user data
  Future<void> removeUserData(String key) async {
    try {
      if (key.contains('token')) {
        await _secure.delete(key: key);
      } else {
        await _userBox.delete(key);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to remove user data: $e');
      }
    }
  }

  // Clear all user data
  Future<void> clearUserData() async {
    try {
      await _userBox.clear();
      await _secure.deleteAll();
    } catch (e) {
      if (kDebugMode) {
        print('Failed to clear user data: $e');
      }
    }
  }

  // Add action to sync queue
  Future<void> addToSyncQueue(String action, Map<String, dynamic> data) async {
    try {
      final queueItem = {
        'action': action,
        'data': data,
        'timestamp': DateTime.now().millisecondsSinceEpoch,
        'retryCount': 0,
      };

      final queue =
          _queueBox.get('pending_actions', defaultValue: <dynamic>[]) ??
          <dynamic>[];
      queue.add(queueItem);
      await _queueBox.put('pending_actions', queue);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to add to sync queue: $e');
      }
    }
  }

  // Get sync queue
  List<Map<String, dynamic>> getSyncQueue() {
    try {
      final queue =
          _queueBox.get('pending_actions', defaultValue: <dynamic>[]) ??
          <dynamic>[];
      return queue
          .map((item) => Map<String, dynamic>.from(item as Map))
          .toList();
    } catch (e) {
      if (kDebugMode) {
        print('Failed to get sync queue: $e');
      }
      return [];
    }
  }

  // Remove item from sync queue
  Future<void> removeFromSyncQueue(int index) async {
    try {
      final queue =
          _queueBox.get('pending_actions', defaultValue: <dynamic>[]) ??
          <dynamic>[];
      if (index >= 0 && index < queue.length) {
        queue.removeAt(index);
        await _queueBox.put('pending_actions', queue);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to remove from sync queue: $e');
      }
    }
  }

  // Update retry count for queue item
  Future<void> updateSyncQueueRetryCount(int index, int retryCount) async {
    try {
      final queue =
          _queueBox.get('pending_actions', defaultValue: <dynamic>[]) ??
          <dynamic>[];
      if (index >= 0 && index < queue.length) {
        final item = queue[index] as Map;
        item['retryCount'] = retryCount;
        await _queueBox.put('pending_actions', queue);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to update sync queue retry count: $e');
      }
    }
  }

  // Clear sync queue
  Future<void> clearSyncQueue() async {
    try {
      await _queueBox.put('pending_actions', <dynamic>[]);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to clear sync queue: $e');
      }
    }
  }

  // Generate cache key from endpoint
  String _generateCacheKey(String endpoint) {
    return endpoint.replaceAll(RegExp(r'[^a-zA-Z0-9]'), '_');
  }

  // Get cache size
  int getCacheSize() {
    return _cacheBox.length;
  }

  // Get user data size
  int getUserDataSize() {
    return _userBox.length;
  }

  // Get sync queue size
  int getSyncQueueSize() {
    return getSyncQueue().length;
  }

  // Clear all cache
  Future<void> clearAllCache() async {
    try {
      await _cacheBox.clear();
    } catch (e) {
      if (kDebugMode) {
        print('Failed to clear all cache: $e');
      }
    }
  }

  // Remove specific cached item by cache key
  Future<void> removeCachedItem(String cacheKey) async {
    try {
      await _cacheBox.delete(cacheKey);
    } catch (e) {
      if (kDebugMode) {
        print('Failed to remove cached item: $e');
      }
    }
  }

  // Trim cache to a maximum number of entries by evicting oldest first
  Future<void> trimCacheToSize(int maxSize) async {
    try {
      if (_cacheBox.length <= maxSize) return;
      final entries = <MapEntry<String, Map>>[];
      for (final key in _cacheBox.keys) {
        final value = _cacheBox.get(key);
        if (value is Map) {
          entries.add(
            MapEntry(key.toString(), Map<String, dynamic>.from(value)),
          );
        }
      }
      // Sort by timestamp ascending (oldest first)
      entries.sort((a, b) {
        final ta = (a.value['timestamp'] as int?) ?? 0;
        final tb = (b.value['timestamp'] as int?) ?? 0;
        return ta.compareTo(tb);
      });
      final toEvict = entries.length - maxSize;
      for (var i = 0; i < toEvict; i++) {
        await _cacheBox.delete(entries[i].key);
      }
    } catch (e) {
      if (kDebugMode) {
        print('Failed to trim cache: $e');
      }
    }
  }

  // Close all boxes
  Future<void> dispose() async {
    try {
      await _cacheBox.close();
      await _userBox.close();
      await _queueBox.close();
    } catch (e) {
      if (kDebugMode) {
        print('Failed to close storage boxes: $e');
      }
    }
  }
}
