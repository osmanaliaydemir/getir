# Getir Clone API - Complete Documentation

## 🚀 **API Overview**

The Getir Clone API is a comprehensive food delivery platform API built with .NET 9, following Clean Architecture principles. It provides endpoints for users, merchants, couriers, and administrators to manage the entire food delivery ecosystem.

### **Base URL**
```
Production: https://api.getir-clone.com
Development: https://localhost:7001
```

### **API Versioning**
All endpoints use URL-based versioning:
```
/api/v1/{endpoint}
```

## 🔐 **Authentication**

### **JWT Token Authentication**
The API uses JWT (JSON Web Token) for authentication. Include the token in the Authorization header:

```http
Authorization: Bearer {your-jwt-token}
```

### **Token Types**
- **Access Token**: Short-lived (15 minutes), used for API requests
- **Refresh Token**: Long-lived (7 days), used to refresh access tokens

### **Authentication Endpoints**

#### **Register User**
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+905551234567"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "User registered successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Customer"
  }
}
```

#### **Login**
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Customer"
    }
  }
}
```

## 👤 **User Management**

### **Get User Profile**
```http
GET /api/v1/users/profile
Authorization: Bearer {token}
```

### **Update User Profile**
```http
PUT /api/v1/users/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Updated Name",
  "lastName": "Updated Surname",
  "phoneNumber": "+905551234568"
}
```

### **User Addresses**

#### **Get User Addresses**
```http
GET /api/v1/users/addresses
Authorization: Bearer {token}
```

#### **Add User Address**
```http
POST /api/v1/users/addresses
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Home",
  "fullAddress": "Kadıköy, İstanbul",
  "city": "İstanbul",
  "district": "Kadıköy",
  "latitude": 40.9923,
  "longitude": 29.0244,
  "isDefault": true
}
```

## 🏪 **Merchants**

### **Get All Merchants**
```http
GET /api/v1/merchants?page=1&pageSize=20&latitude=40.9923&longitude=29.0244
```

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)
- `latitude` (optional): User latitude for distance calculation
- `longitude` (optional): User longitude for distance calculation

### **Get Merchant by ID**
```http
GET /api/v1/merchants/{merchantId}
```

### **Search Merchants**
```http
GET /api/v1/merchants/search?query=Migros&latitude=40.9923&longitude=29.0244
```

### **Create Merchant**
```http
POST /api/v1/merchants
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "My Restaurant",
  "description": "Delicious food and fast delivery",
  "serviceCategoryId": "123e4567-e89b-12d3-a456-426614174000",
  "address": "Kadıköy, İstanbul",
  "latitude": 40.9923,
  "longitude": 29.0244,
  "phoneNumber": "+905551234567",
  "email": "restaurant@example.com",
  "minimumOrderAmount": 50.00,
  "deliveryFee": 5.00,
  "averageDeliveryTime": 30
}
```

## 🛍️ **Products**

### **Get Products by Merchant**
```http
GET /api/v1/products/merchant/{merchantId}?page=1&pageSize=20
```

### **Get Product by ID**
```http
GET /api/v1/products/{productId}
```

### **Search Products**
```http
GET /api/v1/products/search?query=elma&merchantId={merchantId}&categoryId={categoryId}
```

### **Create Product**
```http
POST /api/v1/products
Authorization: Bearer {merchant-token}
Content-Type: application/json

{
  "merchantId": "123e4567-e89b-12d3-a456-426614174000",
  "productCategoryId": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Fresh Apples",
  "description": "Crisp and delicious apples",
  "price": 25.50,
  "stockQuantity": 100,
  "unit": "kg",
  "isActive": true
}
```

## 🛒 **Cart Management**

### **Get Cart Items**
```http
GET /api/v1/cart
Authorization: Bearer {token}
```

### **Add Item to Cart**
```http
POST /api/v1/cart/items
Authorization: Bearer {token}
Content-Type: application/json

{
  "merchantId": "123e4567-e89b-12d3-a456-426614174000",
  "productId": "123e4567-e89b-12d3-a456-426614174000",
  "quantity": 2,
  "notes": "Extra fresh please"
}
```

### **Update Cart Item**
```http
PUT /api/v1/cart/items/{cartItemId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "quantity": 3,
  "notes": "Updated notes"
}
```

### **Remove Cart Item**
```http
DELETE /api/v1/cart/items/{cartItemId}
Authorization: Bearer {token}
```

### **Clear Cart**
```http
DELETE /api/v1/cart
Authorization: Bearer {token}
```

## 📦 **Orders**

### **Create Order**
```http
POST /api/v1/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "merchantId": "123e4567-e89b-12d3-a456-426614174000",
  "deliveryAddress": "Kadıköy, İstanbul",
  "deliveryLatitude": 40.9923,
  "deliveryLongitude": 29.0244,
  "paymentMethod": "CreditCard",
  "notes": "Please ring the doorbell",
  "couponCode": "WELCOME10"
}
```

### **Get User Orders**
```http
GET /api/v1/orders?page=1&pageSize=20&status=Pending
Authorization: Bearer {token}
```

**Query Parameters:**
- `page` (optional): Page number
- `pageSize` (optional): Items per page
- `status` (optional): Filter by order status
- `merchantId` (optional): Filter by merchant

### **Get Order by ID**
```http
GET /api/v1/orders/{orderId}
Authorization: Bearer {token}
```

### **Cancel Order**
```http
POST /api/v1/orders/{orderId}/cancel
Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "Changed my mind"
}
```

## 🎫 **Coupons**

### **Get Available Coupons**
```http
GET /api/v1/coupons?page=1&pageSize=20&isActive=true
```

### **Validate Coupon**
```http
POST /api/v1/coupons/validate
Content-Type: application/json

{
  "code": "WELCOME10",
  "orderAmount": 100.00
}
```

### **Get User Coupons**
```http
GET /api/v1/coupons/user
Authorization: Bearer {token}
```

## ⭐ **Reviews**

### **Create Review**
```http
POST /api/v1/reviews
Authorization: Bearer {token}
Content-Type: application/json

{
  "revieweeId": "123e4567-e89b-12d3-a456-426614174000",
  "revieweeType": "Merchant",
  "orderId": "123e4567-e89b-12d3-a456-426614174000",
  "rating": 5,
  "comment": "Great service and fast delivery!"
}
```

### **Get Reviews by Merchant**
```http
GET /api/v1/reviews/merchant/{merchantId}?page=1&pageSize=20
```

### **Get User Reviews**
```http
GET /api/v1/reviews/user?page=1&pageSize=20
Authorization: Bearer {token}
```

## 🚚 **Courier Management**

### **Get Available Couriers**
```http
GET /api/v1/couriers/available?latitude=40.9923&longitude=29.0244
```

### **Update Courier Location**
```http
PUT /api/v1/couriers/location
Authorization: Bearer {courier-token}
Content-Type: application/json

{
  "latitude": 40.9923,
  "longitude": 29.0244
}
```

### **Get Courier Orders**
```http
GET /api/v1/couriers/orders?page=1&pageSize=20&status=Assigned
Authorization: Bearer {courier-token}
```

## 🔔 **Notifications**

### **Get User Notifications**
```http
GET /api/v1/notifications?page=1&pageSize=20&isRead=false
Authorization: Bearer {token}
```

### **Mark Notification as Read**
```http
PUT /api/v1/notifications/{notificationId}/read
Authorization: Bearer {token}
```

## 🏢 **Merchant Dashboard**

### **Get Dashboard Stats**
```http
GET /api/v1/merchant/dashboard/stats
Authorization: Bearer {merchant-token}
```

### **Get Recent Orders**
```http
GET /api/v1/merchant/dashboard/recent-orders?page=1&pageSize=10
Authorization: Bearer {merchant-token}
```

### **Get Top Products**
```http
GET /api/v1/merchant/dashboard/top-products?limit=10&period=30days
Authorization: Bearer {merchant-token}
```

## 👑 **Admin Panel**

### **Get All Users**
```http
GET /api/v1/admin/users?page=1&pageSize=20&role=Customer
Authorization: Bearer {admin-token}
```

### **Get All Merchants**
```http
GET /api/v1/admin/merchants?page=1&pageSize=20&status=Pending
Authorization: Bearer {admin-token}
```

### **Approve Merchant**
```http
POST /api/v1/admin/merchants/approve
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "merchantId": "123e4567-e89b-12d3-a456-426614174000",
  "isApproved": true,
  "notes": "All documents verified"
}
```

### **Get System Statistics**
```http
GET /api/v1/admin/statistics?period=30days
Authorization: Bearer {admin-token}
```

## 🔧 **System & Health**

### **Health Check**
```http
GET /health
```

### **Metrics**
```http
GET /metrics
```

### **Database Test**
```http
GET /api/v1/database-test/connection
Authorization: Bearer {admin-token}
```

## 📊 **Response Format**

### **Success Response**
```json
{
  "isSuccess": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### **Error Response**
```json
{
  "isSuccess": false,
  "message": "Error description",
  "errorCode": "VALIDATION_ERROR",
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### **Paged Response**
```json
{
  "isSuccess": true,
  "message": "Data retrieved successfully",
  "data": {
    "items": [
      // Array of items
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 100,
      "totalPages": 5,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## 🚨 **Error Codes**

| Code | Description |
|------|-------------|
| `VALIDATION_ERROR` | Input validation failed |
| `AUTHENTICATION_ERROR` | Authentication failed |
| `AUTHORIZATION_ERROR` | Insufficient permissions |
| `NOT_FOUND` | Resource not found |
| `DUPLICATE_ERROR` | Resource already exists |
| `BUSINESS_LOGIC_ERROR` | Business rule violation |
| `EXTERNAL_SERVICE_ERROR` | External service failure |
| `INTERNAL_ERROR` | Internal server error |

## 🔒 **Security**

### **Rate Limiting**
- 100 requests per minute per IP
- 1000 requests per hour per user
- Burst limit: 20 requests per second

### **HTTPS Only**
All API endpoints require HTTPS in production.

### **CORS**
Configured for specific origins in production.

## 📈 **Performance**

### **Response Times**
- Simple queries: < 100ms
- Complex queries: < 500ms
- File uploads: < 2s
- Database operations: < 1s

### **Caching**
- GET requests are cached for 5 minutes
- User-specific data is cached for 1 minute
- Static data is cached for 1 hour

## 🧪 **Testing**

### **Test Environment**
```
Base URL: https://test-api.getir-clone.com
```

### **Test Data**
- Test users are available for all roles
- Test merchants and products are seeded
- Test orders can be created and managed

## 📚 **SDKs and Libraries**

### **Official SDKs**
- .NET SDK (NuGet package)
- JavaScript SDK (NPM package)
- Python SDK (PyPI package)

### **Community Libraries**
- Postman Collection
- OpenAPI Specification
- GraphQL Schema

## 🔄 **Changelog**

### **Version 1.0.0** (2024-01-01)
- Initial release
- Core functionality implemented
- Authentication and authorization
- User, merchant, and courier management
- Order processing
- Review and rating system

---

**📞 Support**: For API support, contact api-support@getir-clone.com
**📖 Documentation**: https://docs.getir-clone.com
**🐛 Bug Reports**: https://github.com/getir-clone/api/issues
