import 'package:flutter/foundation.dart';
import 'package:injectable/injectable.dart';
import 'local_storage_service.dart';
import 'network_service.dart';

/// Pending Actions Service
/// 
/// Manages offline actions and syncs them when back online.
/// Supports: Add to cart, remove from cart, favorites, ratings, etc.
@lazySingleton
class PendingActionsService {
  final LocalStorageService _localStorage;
  final NetworkService _networkService;

  PendingActionsService(
    this._localStorage,
    this._networkService,
  );

  /// Action types
  static const String actionAddToCart = 'add_to_cart';
  static const String actionRemoveFromCart = 'remove_from_cart';
  static const String actionUpdateCartItem = 'update_cart_item';
  static const String actionAddToFavorites = 'add_to_favorites';
  static const String actionRemoveFromFavorites = 'remove_from_favorites';
  static const String actionSubmitReview = 'submit_review';
  static const String actionCancelOrder = 'cancel_order';

  /// Add action to queue
  Future<void> queueAction({
    required String action,
    required Map<String, dynamic> data,
  }) async {
    if (_networkService.isOnline) {
      // If online, we might still want to queue for reliability
      // But typically you'd execute immediately
      if (kDebugMode) {
        debugPrint('📊 Action queued (online): $action');
      }
    } else {
      if (kDebugMode) {
        debugPrint('📴 Action queued (offline): $action');
      }
    }

    await _localStorage.addToSyncQueue(action, data);
  }

  /// Queue add to cart action
  Future<void> queueAddToCart({
    required String productId,
    required int quantity,
    String? variantId,
    List<String>? optionIds,
  }) async {
    await queueAction(
      action: actionAddToCart,
      data: {
        'productId': productId,
        'quantity': quantity,
        if (variantId != null) 'variantId': variantId,
        if (optionIds != null) 'optionIds': optionIds,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Queue remove from cart action
  Future<void> queueRemoveFromCart({
    required String itemId,
  }) async {
    await queueAction(
      action: actionRemoveFromCart,
      data: {
        'itemId': itemId,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Queue update cart item action
  Future<void> queueUpdateCartItem({
    required String itemId,
    required int quantity,
  }) async {
    await queueAction(
      action: actionUpdateCartItem,
      data: {
        'itemId': itemId,
        'quantity': quantity,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Queue add to favorites action
  Future<void> queueAddToFavorites({
    required String itemId,
    required String itemType, // 'product' or 'merchant'
  }) async {
    await queueAction(
      action: actionAddToFavorites,
      data: {
        'itemId': itemId,
        'itemType': itemType,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Queue remove from favorites action
  Future<void> queueRemoveFromFavorites({
    required String itemId,
    required String itemType,
  }) async {
    await queueAction(
      action: actionRemoveFromFavorites,
      data: {
        'itemId': itemId,
        'itemType': itemType,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Queue submit review action
  Future<void> queueSubmitReview({
    required String merchantId,
    required int rating,
    String? comment,
  }) async {
    await queueAction(
      action: actionSubmitReview,
      data: {
        'merchantId': merchantId,
        'rating': rating,
        if (comment != null) 'comment': comment,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Queue cancel order action
  Future<void> queueCancelOrder({
    required String orderId,
    String? reason,
  }) async {
    await queueAction(
      action: actionCancelOrder,
      data: {
        'orderId': orderId,
        if (reason != null) 'reason': reason,
        'timestamp': DateTime.now().toIso8601String(),
      },
    );
  }

  /// Get pending actions count
  int getPendingActionsCount() {
    return _localStorage.getSyncQueueSize();
  }

  /// Get all pending actions
  List<Map<String, dynamic>> getPendingActions() {
    return _localStorage.getSyncQueue();
  }

  /// Check if has pending actions
  bool hasPendingActions() {
    return getPendingActionsCount() > 0;
  }

  /// Get pending actions by type
  List<Map<String, dynamic>> getPendingActionsByType(String actionType) {
    final allActions = getPendingActions();
    return allActions
        .where((action) => action['action'] == actionType)
        .toList();
  }

  /// Clear all pending actions
  Future<void> clearPendingActions() async {
    final queue = getPendingActions();
    for (int i = queue.length - 1; i >= 0; i--) {
      await _localStorage.removeFromSyncQueue(i);
    }
  }

  /// Remove specific pending action
  Future<void> removePendingAction(int index) async {
    await _localStorage.removeFromSyncQueue(index);
  }
}
