import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/core/services/pending_actions_service.dart';
import 'package:getir_mobile/core/services/local_storage_service.dart';
import 'package:getir_mobile/core/services/network_service.dart';
import 'package:getir_mobile/core/services/logger_service.dart';
import 'package:get_it/get_it.dart';

@GenerateMocks([LocalStorageService, NetworkService, LoggerService])
import 'pending_actions_service_test.mocks.dart';

void main() {
  late PendingActionsService service;
  late MockLocalStorageService mockStorage;
  late MockNetworkService mockNetwork;
  late MockLoggerService mockLogger;
  final getIt = GetIt.instance;

  setUp(() {
    mockStorage = MockLocalStorageService();
    mockNetwork = MockNetworkService();
    mockLogger = MockLoggerService();

    // Register logger
    if (!getIt.isRegistered<LoggerService>()) {
      getIt.registerSingleton<LoggerService>(mockLogger);
    }

    when(mockNetwork.isOnline).thenReturn(true);
    when(mockStorage.addToSyncQueue(any, any)).thenAnswer((_) async => {});

    service = PendingActionsService(mockStorage, mockNetwork);
  });

  tearDown() {
    getIt.reset();
  }

  group('PendingActionsService - Queue Action Operations', () {
    test('should queue add to cart action', () async {
      // Act
      await service.queueAddToCart(productId: 'prod-123', quantity: 2);

      // Assert
      verify(
        mockStorage.addToSyncQueue(PendingActionsService.actionAddToCart, any),
      ).called(1);
    });

    test('should queue remove from cart action', () async {
      // Act
      await service.queueRemoveFromCart(itemId: 'item-123');

      // Assert
      verify(
        mockStorage.addToSyncQueue(
          PendingActionsService.actionRemoveFromCart,
          any,
        ),
      ).called(1);
    });

    test('should queue update cart item action', () async {
      // Act
      await service.queueUpdateCartItem(itemId: 'item-123', quantity: 3);

      // Assert
      verify(
        mockStorage.addToSyncQueue(
          PendingActionsService.actionUpdateCartItem,
          any,
        ),
      ).called(1);
    });

    test('should queue add to favorites action', () async {
      // Act
      await service.queueAddToFavorites(
        itemId: 'prod-123',
        itemType: 'product',
      );

      // Assert
      verify(
        mockStorage.addToSyncQueue(
          PendingActionsService.actionAddToFavorites,
          any,
        ),
      ).called(1);
    });

    test('should queue remove from favorites action', () async {
      // Act
      await service.queueRemoveFromFavorites(
        itemId: 'prod-123',
        itemType: 'product',
      );

      // Assert
      verify(
        mockStorage.addToSyncQueue(
          PendingActionsService.actionRemoveFromFavorites,
          any,
        ),
      ).called(1);
    });

    test('should queue submit review action', () async {
      // Act
      await service.queueSubmitReview(
        merchantId: 'merchant-123',
        rating: 5,
        comment: 'Great!',
      );

      // Assert
      verify(
        mockStorage.addToSyncQueue(
          PendingActionsService.actionSubmitReview,
          any,
        ),
      ).called(1);
    });

    test('should queue cancel order action', () async {
      // Act
      await service.queueCancelOrder(
        orderId: 'order-123',
        reason: 'Changed mind',
      );

      // Assert
      verify(
        mockStorage.addToSyncQueue(
          PendingActionsService.actionCancelOrder,
          any,
        ),
      ).called(1);
    });
  });

  group('PendingActionsService - Get Pending Actions', () {
    test('should get all pending actions', () {
      // Arrange
      final mockActions = [
        {
          'action': 'add_to_cart',
          'data': {'productId': '123'},
        },
        {
          'action': 'remove_from_cart',
          'data': {'itemId': '456'},
        },
      ];
      when(mockStorage.getSyncQueue()).thenReturn(mockActions);

      // Act
      final actions = service.getPendingActions();

      // Assert
      expect(actions, equals(mockActions));
      verify(mockStorage.getSyncQueue()).called(1);
    });

    test('should return empty list when no pending actions', () {
      // Arrange
      when(mockStorage.getSyncQueue()).thenReturn([]);

      // Act
      final actions = service.getPendingActions();

      // Assert
      expect(actions, isEmpty);
    });

    test('should get pending actions count', () {
      // Arrange
      when(mockStorage.getSyncQueueSize()).thenReturn(5);

      // Act
      final count = service.getPendingActionsCount();

      // Assert
      expect(count, equals(5));
    });

    test('should check if has pending actions', () {
      // Arrange
      when(mockStorage.getSyncQueueSize()).thenReturn(3);

      // Act
      final hasPending = service.hasPendingActions();

      // Assert
      expect(hasPending, isTrue);
    });

    test('should return false when no pending actions', () {
      // Arrange
      when(mockStorage.getSyncQueueSize()).thenReturn(0);

      // Act
      final hasPending = service.hasPendingActions();

      // Assert
      expect(hasPending, isFalse);
    });
  });

  group('PendingActionsService - Remove Pending Actions', () {
    test('should remove specific pending action by index', () async {
      // Arrange
      when(mockStorage.removeFromSyncQueue(any)).thenAnswer((_) async => {});

      // Act
      await service.removePendingAction(0);

      // Assert
      verify(mockStorage.removeFromSyncQueue(0)).called(1);
    });

    test('should clear all pending actions', () async {
      // Arrange
      final mockActions = [
        {
          'action': 'add_to_cart',
          'data': {'productId': '123'},
        },
        {
          'action': 'remove_from_cart',
          'data': {'itemId': '456'},
        },
      ];
      when(mockStorage.getSyncQueue()).thenReturn(mockActions);
      when(mockStorage.removeFromSyncQueue(any)).thenAnswer((_) async => {});

      // Act
      await service.clearPendingActions();

      // Assert
      verify(mockStorage.removeFromSyncQueue(1)).called(1);
      verify(mockStorage.removeFromSyncQueue(0)).called(1);
    });

    test('should handle clearing empty queue', () async {
      // Arrange
      when(mockStorage.getSyncQueue()).thenReturn([]);

      // Act & Assert
      expect(() => service.clearPendingActions(), returnsNormally);
    });

    test('should handle removing from empty queue', () async {
      // Arrange
      when(mockStorage.removeFromSyncQueue(any)).thenAnswer((_) async => {});

      // Act & Assert
      expect(() => service.removePendingAction(0), returnsNormally);
    });
  });

  group('PendingActionsService - Action Type Filtering', () {
    test('should filter pending actions by type', () {
      // Arrange
      final mockActions = [
        {
          'action': 'add_to_cart',
          'data': {'productId': '123'},
        },
        {
          'action': 'remove_from_cart',
          'data': {'itemId': '456'},
        },
        {
          'action': 'add_to_cart',
          'data': {'productId': '789'},
        },
      ];
      when(mockStorage.getSyncQueue()).thenReturn(mockActions);

      // Act
      final cartActions = service.getPendingActionsByType('add_to_cart');

      // Assert
      expect(cartActions.length, equals(2));
      expect(cartActions[0]['action'], equals('add_to_cart'));
    });

    test('should return empty list for non-existent action type', () {
      // Arrange
      final mockActions = [
        {
          'action': 'add_to_cart',
          'data': {'productId': '123'},
        },
      ];
      when(mockStorage.getSyncQueue()).thenReturn(mockActions);

      // Act
      final actions = service.getPendingActionsByType('non_existent_action');

      // Assert
      expect(actions, isEmpty);
    });

    test('should filter all action types correctly', () {
      // Arrange
      final mockActions = [
        {'action': 'add_to_cart', 'data': {}},
        {'action': 'remove_from_cart', 'data': {}},
        {'action': 'update_cart_item', 'data': {}},
        {'action': 'add_to_favorites', 'data': {}},
        {'action': 'remove_from_favorites', 'data': {}},
        {'action': 'submit_review', 'data': {}},
      ];
      when(mockStorage.getSyncQueue()).thenReturn(mockActions);

      // Act & Assert
      expect(service.getPendingActionsByType('add_to_cart').length, equals(1));
      expect(
        service.getPendingActionsByType('remove_from_cart').length,
        equals(1),
      );
      expect(
        service.getPendingActionsByType('update_cart_item').length,
        equals(1),
      );
    });
  });

  group('PendingActionsService - Pending Actions Count', () {
    test('should return zero for empty queue', () {
      // Arrange
      when(mockStorage.getSyncQueueSize()).thenReturn(0);

      // Act
      final count = service.getPendingActionsCount();

      // Assert
      expect(count, equals(0));
    });

    test('should return correct count for non-empty queue', () {
      // Arrange
      when(mockStorage.getSyncQueueSize()).thenReturn(10);

      // Act
      final count = service.getPendingActionsCount();

      // Assert
      expect(count, equals(10));
    });

    test('should update count after adding actions', () async {
      // Arrange
      var currentCount = 0;
      when(mockStorage.getSyncQueueSize()).thenAnswer((_) => currentCount);
      when(mockStorage.addToSyncQueue(any, any)).thenAnswer((_) async {
        currentCount++;
      });

      // Act
      await service.queueAddToCart(productId: 'prod-1', quantity: 1);
      await service.queueAddToCart(productId: 'prod-2', quantity: 2);
      final count = service.getPendingActionsCount();

      // Assert
      expect(count, equals(2));
    });
  });

  group('PendingActionsService - Edge Cases', () {
    test('should queue actions when offline', () async {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(false);

      // Act
      await service.queueAddToCart(productId: 'prod-123', quantity: 1);

      // Assert
      verify(mockStorage.addToSyncQueue(any, any)).called(1);
    });

    test('should queue actions when online', () async {
      // Arrange
      when(mockNetwork.isOnline).thenReturn(true);

      // Act
      await service.queueAddToCart(productId: 'prod-123', quantity: 1);

      // Assert
      verify(mockStorage.addToSyncQueue(any, any)).called(1);
    });

    test('should handle concurrent action queueing', () async {
      // Act
      final futures = [
        service.queueAddToCart(productId: 'prod-1', quantity: 1),
        service.queueRemoveFromCart(itemId: 'item-1'),
        service.queueAddToFavorites(itemId: 'prod-2', itemType: 'product'),
      ];

      // Assert
      await Future.wait(futures);
      verify(mockStorage.addToSyncQueue(any, any)).called(3);
    });
  });
}
