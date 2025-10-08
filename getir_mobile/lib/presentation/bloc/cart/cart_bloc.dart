import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../domain/entities/cart.dart';
import '../../../domain/usecases/cart_usecases.dart';
import '../../../core/services/analytics_service.dart';

// Events
abstract class CartEvent extends Equatable {
  const CartEvent();

  @override
  List<Object?> get props => [];
}

class LoadCart extends CartEvent {}

class AddToCart extends CartEvent {
  final String productId;
  final int quantity;
  final String? variantId;
  final List<String>? optionIds;
  final String? productName;
  final double? price;
  final String? category;

  const AddToCart({
    required this.productId,
    required this.quantity,
    this.variantId,
    this.optionIds,
    this.productName,
    this.price,
    this.category,
  });

  @override
  List<Object?> get props => [
    productId,
    quantity,
    variantId,
    optionIds,
    productName,
    price,
    category,
  ];
}

class UpdateCartItem extends CartEvent {
  final String itemId;
  final int quantity;

  const UpdateCartItem({required this.itemId, required this.quantity});

  @override
  List<Object> get props => [itemId, quantity];
}

class RemoveFromCart extends CartEvent {
  final String itemId;

  const RemoveFromCart(this.itemId);

  @override
  List<Object> get props => [itemId];
}

class ClearCart extends CartEvent {}

class MergeLocalCartAfterLogin extends CartEvent {}

class ApplyCoupon extends CartEvent {
  final String couponCode;

  const ApplyCoupon(this.couponCode);

  @override
  List<Object> get props => [couponCode];
}

class RemoveCoupon extends CartEvent {}

// States
abstract class CartState extends Equatable {
  const CartState();

  @override
  List<Object?> get props => [];
}

class CartInitial extends CartState {}

class CartLoading extends CartState {}

class CartLoaded extends CartState {
  final Cart cart;

  const CartLoaded(this.cart);

  @override
  List<Object> get props => [cart];
}

class CartItemAdded extends CartState {
  final CartItem cartItem;

  const CartItemAdded(this.cartItem);

  @override
  List<Object> get props => [cartItem];
}

class CartItemUpdated extends CartState {
  final CartItem cartItem;

  const CartItemUpdated(this.cartItem);

  @override
  List<Object> get props => [cartItem];
}

class CartItemRemoved extends CartState {
  final String itemId;

  const CartItemRemoved(this.itemId);

  @override
  List<Object> get props => [itemId];
}

class CartCleared extends CartState {}

class CartError extends CartState {
  final String message;

  const CartError(this.message);

  @override
  List<Object> get props => [message];
}

// BLoC
class CartBloc extends Bloc<CartEvent, CartState> {
  final GetCartUseCase _getCartUseCase;
  final AddToCartUseCase _addToCartUseCase;
  final UpdateCartItemUseCase _updateCartItemUseCase;
  final RemoveFromCartUseCase _removeFromCartUseCase;
  final ClearCartUseCase _clearCartUseCase;
  final ApplyCouponUseCase _applyCouponUseCase;
  final RemoveCouponUseCase _removeCouponUseCase;
  final AnalyticsService _analytics;

  CartBloc({
    required GetCartUseCase getCartUseCase,
    required AddToCartUseCase addToCartUseCase,
    required UpdateCartItemUseCase updateCartItemUseCase,
    required RemoveFromCartUseCase removeFromCartUseCase,
    required ClearCartUseCase clearCartUseCase,
    required ApplyCouponUseCase applyCouponUseCase,
    required RemoveCouponUseCase removeCouponUseCase,
    required AnalyticsService analytics,
  }) : _getCartUseCase = getCartUseCase,
       _addToCartUseCase = addToCartUseCase,
       _updateCartItemUseCase = updateCartItemUseCase,
       _removeFromCartUseCase = removeFromCartUseCase,
       _clearCartUseCase = clearCartUseCase,
       _applyCouponUseCase = applyCouponUseCase,
       _removeCouponUseCase = removeCouponUseCase,
       _analytics = analytics,
       super(CartInitial()) {
    on<LoadCart>(_onLoadCart);
    on<AddToCart>(_onAddToCart);
    on<UpdateCartItem>(_onUpdateCartItem);
    on<RemoveFromCart>(_onRemoveFromCart);
    on<ClearCart>(_onClearCart);
    on<ApplyCoupon>(_onApplyCoupon);
    on<RemoveCoupon>(_onRemoveCoupon);
    on<MergeLocalCartAfterLogin>(_onMergeLocalCartAfterLogin);
  }

  Future<void> _onLoadCart(LoadCart event, Emitter<CartState> emit) async {
    emit(CartLoading());
    try {
      final cart = await _getCartUseCase();
      emit(CartLoaded(cart));
    } catch (e) {
      emit(CartError(e.toString()));
    }
  }

  Future<void> _onMergeLocalCartAfterLogin(
    MergeLocalCartAfterLogin event,
    Emitter<CartState> emit,
  ) async {
    try {
      // Strategy: backend is source of truth; trigger server to merge local items if any
      // Simplest approach: just reload server cart after login
      final cart = await _getCartUseCase();
      emit(CartLoaded(cart));
    } catch (e) {
      emit(CartError(e.toString()));
    }
  }

  Future<void> _onAddToCart(AddToCart event, Emitter<CartState> emit) async {
    try {
      final cartItem = await _addToCartUseCase(
        productId: event.productId,
        quantity: event.quantity,
        variantId: event.variantId,
        optionIds: event.optionIds,
      );

      // 📊 Analytics: Track add to cart
      if (event.productName != null && event.price != null) {
        await _analytics.logAddToCart(
          productId: event.productId,
          productName: event.productName!,
          price: event.price!,
          category: event.category,
          quantity: event.quantity,
        );
      }

      emit(CartItemAdded(cartItem));
      // Reload cart to get updated totals
      add(LoadCart());
    } catch (e) {
      emit(CartError(e.toString()));
      await _analytics.logError(error: e, reason: 'Add to cart failed');
    }
  }

  Future<void> _onUpdateCartItem(
    UpdateCartItem event,
    Emitter<CartState> emit,
  ) async {
    try {
      final cartItem = await _updateCartItemUseCase(
        itemId: event.itemId,
        quantity: event.quantity,
      );
      emit(CartItemUpdated(cartItem));
      // Reload cart to get updated totals
      add(LoadCart());
    } catch (e) {
      emit(CartError(e.toString()));
    }
  }

  Future<void> _onRemoveFromCart(
    RemoveFromCart event,
    Emitter<CartState> emit,
  ) async {
    try {
      await _removeFromCartUseCase(event.itemId);

      // 📊 Analytics: Track remove from cart
      await _analytics.logCustomEvent(
        eventName: 'remove_from_cart',
        parameters: {'item_id': event.itemId},
      );

      emit(CartItemRemoved(event.itemId));
      // Reload cart to get updated totals
      add(LoadCart());
    } catch (e) {
      emit(CartError(e.toString()));
      await _analytics.logError(error: e, reason: 'Remove from cart failed');
    }
  }

  Future<void> _onClearCart(ClearCart event, Emitter<CartState> emit) async {
    try {
      await _clearCartUseCase();
      emit(CartCleared());
      // Reload cart to get updated totals
      add(LoadCart());
    } catch (e) {
      emit(CartError(e.toString()));
    }
  }

  Future<void> _onApplyCoupon(
    ApplyCoupon event,
    Emitter<CartState> emit,
  ) async {
    try {
      final cart = await _applyCouponUseCase(event.couponCode);
      emit(CartLoaded(cart));
    } catch (e) {
      emit(CartError(e.toString()));
    }
  }

  Future<void> _onRemoveCoupon(
    RemoveCoupon event,
    Emitter<CartState> emit,
  ) async {
    try {
      final cart = await _removeCouponUseCase();
      emit(CartLoaded(cart));
    } catch (e) {
      emit(CartError(e.toString()));
    }
  }
}
