# 📡 Getir Mobile - API Documentation

**Version:** 1.0.0  
**Base URL:** `https://api.getir.com/v1`  
**Last Updated:** 8 Ekim 2025

---

## 📋 Table of Contents

- [Authentication](#authentication)
- [Merchants](#merchants)
- [Products](#products)
- [Cart](#cart)
- [Orders](#orders)
- [User Profile](#user-profile)
- [Addresses](#addresses)
- [Notifications](#notifications)

---

## 🔐 Authentication

All authenticated endpoints require Bearer token:

```
Authorization: Bearer {access_token}
```

### POST /auth/login

**Description:** User login with email and password

**Request:**
```json
{
  "email": "user@getir.com",
  "password": "SecurePassword123"
}
```

**Response:** `200 OK`
```json
{
  "user": {
    "id": "user-uuid",
    "email": "user@getir.com",
    "firstName": "Osman",
    "lastName": "Aydemir",
    "phoneNumber": "+905551234567",
    "role": "customer",
    "isEmailVerified": true,
    "createdAt": "2025-10-01T10:00:00Z"
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 900
}
```

**Errors:**
- `400` - Invalid credentials
- `401` - Authentication failed
- `429` - Too many requests

---

### POST /auth/register

**Description:** Register new user

**Request:**
```json
{
  "email": "newuser@getir.com",
  "password": "SecurePassword123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+905551234567"
}
```

**Response:** `201 Created`
```json
{
  "user": { /* User object */ },
  "accessToken": "...",
  "refreshToken": "...",
  "expiresIn": 900
}
```

**Errors:**
- `400` - Validation error
- `409` - Email already exists

---

### POST /auth/refresh

**Description:** Refresh access token

**Request:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "new_access_token",
  "refreshToken": "new_refresh_token",
  "expiresIn": 900
}
```

---

### POST /auth/logout

**Description:** Logout user

**Headers:** `Authorization: Bearer {token}`

**Response:** `204 No Content`

---

### POST /auth/forgot-password

**Description:** Request password reset

**Request:**
```json
{
  "email": "user@getir.com"
}
```

**Response:** `200 OK`
```json
{
  "message": "Password reset email sent"
}
```

---

### POST /auth/reset-password

**Description:** Reset password with token

**Request:**
```json
{
  "token": "reset_token_from_email",
  "newPassword": "NewSecurePassword123"
}
```

**Response:** `200 OK`

---

## 🏪 Merchants

### GET /merchants

**Description:** Get list of merchants with filters

**Query Parameters:**
- `latitude` (required): User latitude
- `longitude` (required): User longitude
- `serviceType` (optional): "restaurant" | "market" | "pharmacy"
- `isOpen` (optional): boolean
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20)

**Example:**
```
GET /merchants?latitude=41.0082&longitude=28.9784&serviceType=market&page=1
```

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "merchant-uuid",
      "name": "Migros",
      "description": "En taze ürünler",
      "logoUrl": "https://cdn.getir.com/merchants/migros-logo.png",
      "coverImageUrl": "https://cdn.getir.com/merchants/migros-cover.jpg",
      "rating": 4.5,
      "reviewCount": 1250,
      "deliveryFee": 9.99,
      "estimatedDeliveryTime": 15,
      "distance": 2.5,
      "isOpen": true,
      "address": "Kadıköy, Istanbul",
      "phoneNumber": "+902121234567",
      "categories": ["Market", "Bakkal"],
      "workingHours": {
        "monday": "08:00-23:00",
        "tuesday": "08:00-23:00"
      },
      "minimumOrderAmount": 50.0
    }
  ],
  "pagination": {
    "currentPage": 1,
    "totalPages": 5,
    "totalItems": 100,
    "pageSize": 20
  }
}
```

---

### GET /merchants/{id}

**Description:** Get merchant details

**Response:** `200 OK`
```json
{
  "id": "merchant-uuid",
  "name": "Migros",
  // ... full merchant object
  "products": [
    {
      "id": "product-uuid",
      "name": "Süt",
      "price": 29.99,
      // ... product details
    }
  ]
}
```

---

## 📦 Products

### GET /products

**Description:** Get products list

**Query Parameters:**
- `merchantId` (optional): Filter by merchant
- `category` (optional): Filter by category
- `search` (optional): Search query
- `page`, `pageSize`: Pagination

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "product-uuid",
      "merchantId": "merchant-uuid",
      "merchantName": "Migros",
      "name": "Süt 1L",
      "description": "Tam yağlı süt",
      "price": 29.99,
      "imageUrl": "https://cdn.getir.com/products/milk.jpg",
      "category": "Süt Ürünleri",
      "isAvailable": true,
      "stockQuantity": 100,
      "unit": "adet",
      "rating": 4.7,
      "reviewCount": 450,
      "tags": ["organic", "local"],
      "nutritionalInfo": {
        "calories": 150,
        "protein": "8g"
      },
      "brand": "Pınar",
      "barcode": "1234567890123",
      "variants": [],
      "options": []
    }
  ],
  "pagination": { /* ... */ }
}
```

---

### GET /products/{id}

**Description:** Get product details

**Response:** `200 OK`
```json
{
  "id": "product-uuid",
  // ... full product object
  "variants": [
    {
      "id": "variant-uuid",
      "name": "500ml",
      "price": 19.99,
      "isAvailable": true,
      "stockQuantity": 50
    }
  ],
  "options": [
    {
      "id": "option-uuid",
      "name": "Soğuk/Sıcak",
      "type": "single",
      "isRequired": true,
      "values": [
        {"id": "val1", "name": "Soğuk", "price": 0},
        {"id": "val2", "name": "Sıcak", "price": 2}
      ]
    }
  ]
}
```

---

## 🛒 Cart

### GET /cart

**Description:** Get user's current cart

**Headers:** `Authorization: Bearer {token}`

**Response:** `200 OK`
```json
{
  "id": "cart-uuid",
  "userId": "user-uuid",
  "items": [
    {
      "id": "item-uuid",
      "productId": "product-uuid",
      "productName": "Süt",
      "productImageUrl": "...",
      "unitPrice": 29.99,
      "quantity": 2,
      "totalPrice": 59.98,
      "merchantId": "merchant-uuid",
      "merchantName": "Migros",
      "addedAt": "2025-10-08T10:00:00Z"
    }
  ],
  "subtotal": 59.98,
  "deliveryFee": 9.99,
  "discountAmount": 0,
  "total": 69.97,
  "couponCode": null
}
```

---

### POST /cart/items

**Description:** Add product to cart

**Request:**
```json
{
  "productId": "product-uuid",
  "quantity": 1,
  "variantId": "variant-uuid",
  "optionIds": ["option-val-uuid"]
}
```

**Response:** `201 Created`
```json
{
  "id": "cart-item-uuid",
  "productId": "product-uuid",
  // ... cart item details
}
```

---

### PUT /cart/items/{id}

**Description:** Update cart item quantity

**Request:**
```json
{
  "itemId": "cart-item-uuid",
  "quantity": 3
}
```

**Response:** `200 OK`

---

### DELETE /cart/items/{id}

**Description:** Remove item from cart

**Response:** `204 No Content`

---

### DELETE /cart

**Description:** Clear entire cart

**Response:** `204 No Content`

---

### POST /cart/coupon

**Description:** Apply coupon code

**Request:**
```json
{
  "code": "GETIR50"
}
```

**Response:** `200 OK`
```json
{
  "cart": { /* updated cart with discount */ },
  "discount": {
    "code": "GETIR50",
    "type": "percentage",
    "value": 50,
    "amount": 29.99
  }
}
```

---

## 📋 Orders

### POST /orders

**Description:** Create new order

**Request:**
```json
{
  "cartId": "cart-uuid",
  "addressId": "address-uuid",
  "paymentMethod": "credit_card",
  "paymentDetails": {
    "cardToken": "card_token_here"
  },
  "notes": "Ring the doorbell twice"
}
```

**Response:** `201 Created`
```json
{
  "id": "order-uuid",
  "orderNumber": "ORD-2025-001234",
  "status": "pending",
  "totalAmount": 69.97,
  "estimatedDeliveryTime": "2025-10-08T11:30:00Z",
  "createdAt": "2025-10-08T10:00:00Z"
}
```

---

### GET /orders

**Description:** Get user's orders

**Query Parameters:**
- `status` (optional): "pending" | "confirmed" | "delivered" | "cancelled"
- `page`, `pageSize`: Pagination

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "order-uuid",
      "orderNumber": "ORD-2025-001234",
      "status": "on_the_way",
      "merchant": { /* merchant object */ },
      "items": [ /* order items */ ],
      "totalAmount": 69.97,
      "address": { /* delivery address */ },
      "courier": {
        "name": "Ahmet",
        "phone": "+905559876543",
        "currentLocation": {
          "latitude": 41.0082,
          "longitude": 28.9784
        }
      },
      "createdAt": "2025-10-08T10:00:00Z",
      "estimatedDeliveryTime": "2025-10-08T11:30:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

---

### GET /orders/{id}

**Description:** Get order details

**Response:** `200 OK`
```json
{
  "id": "order-uuid",
  "orderNumber": "ORD-2025-001234",
  "status": "on_the_way",
  "timeline": [
    {"status": "pending", "timestamp": "2025-10-08T10:00:00Z"},
    {"status": "confirmed", "timestamp": "2025-10-08T10:05:00Z"},
    {"status": "preparing", "timestamp": "2025-10-08T10:10:00Z"},
    {"status": "on_the_way", "timestamp": "2025-10-08T10:20:00Z"}
  ],
  // ... full order details
}
```

---

### PUT /orders/{id}/cancel

**Description:** Cancel order

**Request:**
```json
{
  "reason": "Changed my mind"
}
```

**Response:** `200 OK`

---

### POST /orders/{id}/rate

**Description:** Rate order

**Request:**
```json
{
  "rating": 5,
  "comment": "Harika hizmet!",
  "merchantRating": 5,
  "courierRating": 5
}
```

**Response:** `201 Created`

---

## 👤 User Profile

### GET /users/me

**Description:** Get current user profile

**Response:** `200 OK`
```json
{
  "id": "user-uuid",
  "email": "user@getir.com",
  "firstName": "Osman",
  "lastName": "Aydemir",
  "phoneNumber": "+905551234567",
  "role": "customer",
  "isEmailVerified": true,
  "createdAt": "2025-10-01T10:00:00Z",
  "lastLoginAt": "2025-10-08T09:00:00Z"
}
```

---

### PUT /users/me

**Description:** Update user profile

**Request:**
```json
{
  "firstName": "Osman Ali",
  "lastName": "Aydemir",
  "phoneNumber": "+905551234567"
}
```

**Response:** `200 OK`

---

## 📍 Addresses

### GET /addresses

**Description:** Get user's addresses

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "address-uuid",
      "title": "Ev",
      "fullAddress": "Kadıköy Mahallesi, İstanbul",
      "buildingNumber": "123",
      "floor": "3",
      "apartment": "5",
      "latitude": 40.9923,
      "longitude": 29.0287,
      "type": "home",
      "isDefault": true
    }
  ]
}
```

---

### POST /addresses

**Description:** Add new address

**Request:**
```json
{
  "title": "İş",
  "fullAddress": "Levent, İstanbul",
  "buildingNumber": "456",
  "floor": "10",
  "apartment": "15",
  "latitude": 41.0764,
  "longitude": 29.0108,
  "type": "work"
}
```

**Response:** `201 Created`

---

### DELETE /addresses/{id}

**Description:** Delete address

**Response:** `204 No Content`

---

## 🔔 Notifications

### GET /notifications

**Description:** Get user notifications

**Query Parameters:**
- `unreadOnly` (optional): boolean
- `page`, `pageSize`: Pagination

**Response:** `200 OK`
```json
{
  "data": [
    {
      "id": "notification-uuid",
      "type": "order_update",
      "title": "Siparişiniz yolda!",
      "message": "Kuryeniz 5 dakika içinde kapınızda olacak.",
      "data": {
        "orderId": "order-uuid",
        "orderStatus": "on_the_way"
      },
      "isRead": false,
      "createdAt": "2025-10-08T10:20:00Z"
    }
  ],
  "pagination": { /* ... */ }
}
```

---

### PUT /notifications/{id}/read

**Description:** Mark notification as read

**Response:** `200 OK`

---

## 🔄 Real-time (SignalR)

### OrderHub

**URL:** `https://api.getir.com/hubs/order`

**Events (Server → Client):**

```javascript
// Order status changed
connection.on("OrderStatusChanged", (orderId, newStatus) => {
  // orderId: string
  // newStatus: "pending" | "confirmed" | "preparing" | "on_the_way" | "delivered"
});

// Order updated
connection.on("OrderUpdated", (order) => {
  // order: full order object
});
```

**Methods (Client → Server):**

```javascript
// Subscribe to order updates
await connection.invoke("SubscribeToOrder", orderId);

// Unsubscribe
await connection.invoke("UnsubscribeFromOrder", orderId);
```

---

### TrackingHub

**URL:** `https://api.getir.com/hubs/tracking`

**Events:**

```javascript
// Courier location updated
connection.on("CourierLocationUpdated", (orderId, location) => {
  // location: { latitude, longitude, heading, speed }
});

// ETA updated
connection.on("ETAUpdated", (orderId, eta) => {
  // eta: ISO timestamp
});
```

---

### NotificationHub

**URL:** `https://api.getir.com/hubs/notification`

**Events:**

```javascript
// New notification
connection.on("NewNotification", (notification) => {
  // notification: full notification object
});
```

---

## 📊 Response Format

### Success Response

```json
{
  "data": { /* payload */ },
  "message": "Success",
  "timestamp": "2025-10-08T10:00:00Z"
}
```

### Error Response

```json
{
  "error": {
    "code": "AUTH_001",
    "message": "Invalid credentials",
    "details": "Email or password is incorrect"
  },
  "timestamp": "2025-10-08T10:00:00Z"
}
```

---

## 🚦 HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET/PUT request |
| 201 | Created | Successful POST request |
| 204 | No Content | Successful DELETE request |
| 400 | Bad Request | Validation error |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Permission denied |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource already exists |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Maintenance mode |

---

## 🔒 Security

### Authentication

All protected endpoints require:

```
Authorization: Bearer {access_token}
```

### Rate Limiting

- **Standard:** 100 requests/minute
- **Auth endpoints:** 10 requests/minute
- **Search:** 30 requests/minute

### CORS

Allowed origins:
- `https://getir.com`
- `https://app.getir.com`
- `http://localhost:*` (development)

---

## 📚 Additional Resources

- **Postman Collection:** `docs/Getir-API-Complete.postman_collection.json`
- **Environment Variables:** `docs/Getir-API-Environment.postman_environment.json`
- **API Guide:** `docs/API-COMPLETE-DOCUMENTATION.md`

---

**Maintained by:** Backend Team  
**Contact:** api-support@getir.com  
**Last Updated:** 8 Ekim 2025
