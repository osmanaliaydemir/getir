import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'local_storage_service.dart';
import 'network_service.dart';

class SyncService {
  static final SyncService _instance = SyncService._internal();
  factory SyncService() => _instance;
  SyncService._internal();

  final LocalStorageService _localStorage = LocalStorageService();
  final NetworkService _networkService = NetworkService();
  final Dio _dio = Dio();

  bool _isSyncing = false;
  final int _maxRetries = 3;
  final Duration _retryDelay = const Duration(seconds: 5);

  // Getters
  bool get isSyncing => _isSyncing;

  // Initialize sync service
  Future<void> initialize() async {
    // Start periodic sync
    _startPeriodicSync();
  }

  // Start periodic sync (every 30 seconds when online)
  void _startPeriodicSync() {
    Stream.periodic(const Duration(seconds: 30)).listen((_) {
      if (_networkService.isOnline && !_isSyncing) {
        syncPendingActions();
      }
    });
  }

  // Sync pending actions when online
  Future<void> syncPendingActions() async {
    if (_isSyncing || !_networkService.isOnline) return;

    _isSyncing = true;

    try {
      final queue = _localStorage.getSyncQueue();

      for (int i = queue.length - 1; i >= 0; i--) {
        final item = queue[i];
        final action = item['action'] as String;
        final data = item['data'] as Map<String, dynamic>;
        final retryCount = item['retryCount'] as int? ?? 0;

        if (retryCount >= _maxRetries) {
          // Remove item after max retries
          await _localStorage.removeFromSyncQueue(i);
          if (kDebugMode) {
            print('Removed item from sync queue after max retries: $action');
          }
          continue;
        }

        try {
          final success = await _executeAction(action, data);

          if (success) {
            // Remove successful item from queue
            await _localStorage.removeFromSyncQueue(i);
            if (kDebugMode) {
              print('Successfully synced action: $action');
            }
          } else {
            // Increment retry count
            await _localStorage.updateSyncQueueRetryCount(i, retryCount + 1);
            if (kDebugMode) {
              print('Failed to sync action, retry count: ${retryCount + 1}');
            }
          }
        } catch (e) {
          // Increment retry count on error
          await _localStorage.updateSyncQueueRetryCount(i, retryCount + 1);
          if (kDebugMode) {
            print('Error syncing action $action: $e');
          }
        }

        // Add delay between sync attempts
        await Future.delayed(const Duration(milliseconds: 500));
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error in sync process: $e');
      }
    } finally {
      _isSyncing = false;
    }
  }

  // Execute specific action
  Future<bool> _executeAction(String action, Map<String, dynamic> data) async {
    try {
      switch (action) {
        case 'add_to_cart':
          return await _syncAddToCart(data);
        case 'update_cart_item':
          return await _syncUpdateCartItem(data);
        case 'remove_from_cart':
          return await _syncRemoveFromCart(data);
        case 'apply_coupon':
          return await _syncApplyCoupon(data);
        case 'create_order':
          return await _syncCreateOrder(data);
        case 'update_profile':
          return await _syncUpdateProfile(data);
        case 'update_notification_preferences':
          return await _syncUpdateNotificationPreferences(data);
        default:
          if (kDebugMode) {
            print('Unknown sync action: $action');
          }
          return false;
      }
    } catch (e) {
      if (kDebugMode) {
        print('Error executing sync action $action: $e');
      }
      return false;
    }
  }

  // Sync add to cart action
  Future<bool> _syncAddToCart(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/api/v1/cart/items', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      return false;
    }
  }

  // Sync update cart item action
  Future<bool> _syncUpdateCartItem(Map<String, dynamic> data) async {
    try {
      final itemId = data['itemId'] as String;
      final response = await _dio.put(
        '/api/v1/cart/items/$itemId',
        data: {'quantity': data['quantity']},
      );
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // Sync remove from cart action
  Future<bool> _syncRemoveFromCart(Map<String, dynamic> data) async {
    try {
      final itemId = data['itemId'] as String;
      final response = await _dio.delete('/api/v1/cart/items/$itemId');
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // Sync apply coupon action
  Future<bool> _syncApplyCoupon(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post(
        '/api/v1/coupons/apply',
        data: {'couponCode': data['couponCode']},
      );
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // Sync create order action
  Future<bool> _syncCreateOrder(Map<String, dynamic> data) async {
    try {
      final response = await _dio.post('/api/v1/orders', data: data);
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (e) {
      return false;
    }
  }

  // Sync update profile action
  Future<bool> _syncUpdateProfile(Map<String, dynamic> data) async {
    try {
      final response = await _dio.put('/api/v1/user/profile', data: data);
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // Sync update notification preferences action
  Future<bool> _syncUpdateNotificationPreferences(
    Map<String, dynamic> data,
  ) async {
    try {
      final response = await _dio.put(
        '/api/v1/notifications/preferences',
        data: data,
      );
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // Force sync now
  Future<void> forceSyncNow() async {
    if (_networkService.isOnline) {
      await syncPendingActions();
    }
  }

  // Get sync status
  Map<String, dynamic> getSyncStatus() {
    return {
      'isSyncing': _isSyncing,
      'isOnline': _networkService.isOnline,
      'pendingActions': _localStorage.getSyncQueueSize(),
      'cacheSize': _localStorage.getCacheSize(),
      'userDataSize': _localStorage.getUserDataSize(),
    };
  }

  // Clear all sync data
  Future<void> clearSyncData() async {
    await _localStorage.clearSyncQueue();
    await _localStorage.clearAllCache();
  }

  // Dispose resources
  void dispose() {
    // Stop periodic sync
  }
}
