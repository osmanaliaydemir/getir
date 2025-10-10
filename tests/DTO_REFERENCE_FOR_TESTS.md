# DTO Reference Guide for Integration Tests

Bu dosya test yazarken kullanılacak DTO'ların doğru parametre sıralarını içerir.

## Cart DTOs

### AddToCartRequest (4 parametre)
```csharp
new AddToCartRequest(
    Guid merchantId,        // 1. Merchant ID (YENİ!)
    Guid productId,         // 2. Product ID
    int quantity,           // 3. Quantity
    string? notes)          // 4. Notes (optional)
```

### UpdateCartItemRequest (2 parametre)
```csharp
new UpdateCartItemRequest(
    int quantity,           // 1. Quantity
    string? notes)          // 2. Notes (optional)
```

## Order DTOs

### CreateOrderRequest (7 parametre)
```csharp
new CreateOrderRequest(
    Guid merchantId,                    // 1. Merchant ID
    List<OrderLineRequest> items,       // 2. Order items list (YENİ!)
    string deliveryAddress,             // 3. Delivery address
    decimal deliveryLatitude,           // 4. Latitude (decimal!)
    decimal deliveryLongitude,          // 5. Longitude (decimal!)
    string paymentMethod,               // 6. Payment method
    string? notes)                      // 7. Notes (optional)
```

### OrderLineRequest
```csharp
new OrderLineRequest(
    Guid productId,
    int quantity,
    decimal price)
```

## Product DTOs

### CreateProductRequest (8 parametre)
```csharp
new CreateProductRequest(
    Guid merchantId,                // 1. Merchant ID
    Guid? productCategoryId,        // 2. Category ID (optional, YENİ!)
    string name,                    // 3. Name
    string? description,            // 4. Description (optional)
    decimal price,                  // 5. Price
    decimal? discountedPrice,       // 6. Discounted price (optional)
    int stockQuantity,              // 7. Stock quantity (YENİ!)
    string? unit)                   // 8. Unit (optional, YENİ!)
```

### UpdateProductRequest
```csharp
new UpdateProductRequest(
    string name,
    string? description,
    decimal? discountedPrice,
    int stockQuantity,
    string? imageUrl,
    bool isActive)
```

## Address DTOs

### CreateAddressRequest (6 parametre)
```csharp
new CreateAddressRequest(
    string title,           // 1. Title (e.g., "Home", "Work")
    string fullAddress,     // 2. Full address (YENİ: tek field!)
    string city,            // 3. City
    string district,        // 4. District
    decimal latitude,       // 5. Latitude (decimal!)
    decimal longitude)      // 6. Longitude (decimal!)
```

### UpdateAddressRequest (6 parametre - aynı)
```csharp
new UpdateAddressRequest(
    string title,
    string fullAddress,
    string city,
    string district,
    decimal latitude,
    decimal longitude)
```

## Coupon DTOs

### CreateCouponRequest (10 parametre)
```csharp
new CreateCouponRequest(
    string code,                        // 1. Coupon code
    string title,                       // 2. Title (YENİ!)
    string? description,                // 3. Description (optional)
    string discountType,                // 4. "Percentage" or "FixedAmount"
    decimal discountValue,              // 5. Discount value
    decimal minimumOrderAmount,         // 6. Minimum order amount
    decimal? maximumDiscountAmount,     // 7. Max discount (optional)
    DateTime startDate,                 // 8. Start date (YENİ!)
    DateTime endDate,                   // 9. End date
    int? usageLimit)                    // 10. Usage limit (optional)
```

## Review DTOs

### CreateReviewRequest (6 parametre)
```csharp
new CreateReviewRequest(
    Guid revieweeId,            // 1. Reviewee ID (merchant/courier ID)
    string revieweeType,        // 2. "Merchant" or "Courier"
    Guid orderId,               // 3. Order ID (YENİ!)
    int rating,                 // 4. Rating (1-5)
    string comment,             // 5. Comment
    List<string>? tags)         // 6. Tags (optional)
```

### UpdateReviewRequest
```csharp
new UpdateReviewRequest(
    int rating,
    string comment,
    List<string>? tags)
```

## Courier DTOs

### UpdateCourierLocationRequest (2 parametre)
```csharp
new UpdateCourierLocationRequest(
    decimal latitude,       // 1. Latitude (decimal, not double!)
    decimal longitude)      // 2. Longitude (decimal, not double!)
```

## Product Category DTOs

### CreateProductCategoryRequest (5 parametre)
```csharp
new CreateProductCategoryRequest(
    string name,                    // 1. Name
    string? description,            // 2. Description (optional)
    Guid? parentCategoryId,         // 3. Parent category ID (optional)
    string? imageUrl,               // 4. Image URL (optional)
    int displayOrder)               // 5. Display order
```

## Önemli Notlar

1. **decimal vs double**: Tüm koordinatlar ve fiyatlar `decimal` olmalı (`40.7128m`, not `40.7128`)
2. **Yeni eklenen parametreler**: 
   - AddToCartRequest: merchantId (1. parametre)
   - CreateOrderRequest: items (2. parametre, List<OrderLineRequest>)
   - CreateProductRequest: productCategoryId, stockQuantity, unit
   - CreateCouponRequest: title, startDate
   - CreateReviewRequest: orderId (3. parametre)
3. **Parametre sırası çok önemli!** Primary constructor kullanıldığı için sıra değiştirilemiyor.

---

**Oluşturulma Tarihi**: 2025-10-10
**Amaç**: Test yazarken compile hatalarını önlemek

