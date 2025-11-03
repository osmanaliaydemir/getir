# 🛒 Cart & Checkout Flow

**Tarih:** 2 Kasım 2025  
**Konu:** Sepet ve Ödeme Akışı

---

## 📋 İçindekiler

- [Cart Flow](#cart-flow)
- [Checkout Flow](#checkout-flow)
- [Multi-Merchant Support](#multi-merchant-support)
- [Coupon System](#coupon-system)

---

## 🛒 Cart Flow

### Add to Cart

```
ProductCard → AddToCart button
    ↓
CartBloc.add(AddToCart(
  merchantId, productId, quantity, variantId, options
))
    ↓
CartService.addToCart()
    ↓
CartRepository.addToCart()
    ↓
Save to local storage + API sync
    ↓
CartBloc.emit(CartItemAdded(item))
    ↓
Update UI badge + notification
```

### Remove from Cart

```
CartItem → Remove button
    ↓
CartBloc.add(RemoveFromCart(itemId))
    ↓
CartService.removeFromCart()
    ↓
Delete from storage + API
    ↓
CartBloc.emit(CartItemRemoved(itemId))
```

### Update Quantity

```
CartItem → +/- buttons
    ↓
CartBloc.add(UpdateCartItem(itemId, newQuantity))
    ↓
If quantity == 0 → Remove
Else → Update
    ↓
CartBloc.emit(CartItemUpdated(item))
```

---

## 💳 Checkout Flow

### 1. Validate Cart

```dart
// Minimum order amount
if (cart.total < merchant.minimumOrder) {
  showError('Minimum order: ${merchant.minimumOrder}');
  return;
}

// Check merchant is open
if (!merchant.isOpen) {
  showError('Merchant is closed');
  return;
}
```

### 2. Select Address

```dart
// User selects delivery address
final selectedAddress = await showAddressPicker();

// Calculate delivery fee
final deliveryFee = calculateFee(address, merchant);
```

### 3. Apply Coupon

```dart
CartBloc.add(ApplyCoupon(code));

// Validate coupon
if (coupon.isValid) {
  cart.applyCoupon(coupon);
  showSuccess('Coupon applied!');
}
```

### 4. Place Order

```dart
// Create order
OrderBloc.add(PlaceOrder(
  cart: cart,
  address: address,
  paymentMethod: paymentMethod,
));

// Process payment
await processPayment();

// Confirm order
OrderBloc.emit(OrderPlaced(order));
```

---

## 🏪 Multi-Merchant Support

### Strategy

- Her merchant için ayrı cart segment
- Checkout'ta sadece 1 merchant
- Split order to multiple merchants

### Implementation

```dart
class Cart {
  final List<CartItem> items;
  final String? activeMerchantId;
  
  bool isMultiMerchant() {
    return items.map((e) => e.merchantId).toSet().length > 1;
  }
}
```

---

## 🎫 Coupon System

### Apply Coupon

```dart
CartService.applyCoupon(code) {
  // Validate
  if (!isValid(code)) throw InvalidCouponException();
  if (isExpired(code)) throw ExpiredCouponException();
  if (cart.total < coupon.minOrder) {
    throw MinimumOrderException();
  }
  
  // Apply discount
  cart.discount = coupon.amount;
  cart.total -= cart.discount;
}
```

### Remove Coupon

```dart
CartBloc.add(RemoveCoupon());

cart.discount = 0;
cart.total = cart.subtotal + cart.deliveryFee;
```

---

**Hazırlayan:** Product Team  
**Tarih:** 2 Kasım 2025

