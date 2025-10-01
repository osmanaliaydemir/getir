# 📋 Endpoint Summary - Quick Reference

## Total: 44 Endpoints

### 🔐 Authentication (4)
```
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
POST   /api/v1/auth/logout              [Auth Required]
```

### 📂 Categories (5)
```
GET    /api/v1/categories
GET    /api/v1/categories/{id}
POST   /api/v1/categories               [Auth Required]
PUT    /api/v1/categories/{id}          [Auth Required]
DELETE /api/v1/categories/{id}          [Auth Required]
```

### 🏪 Merchants (5)
```
GET    /api/v1/merchants
GET    /api/v1/merchants/{id}
POST   /api/v1/merchants                [Auth Required]
PUT    /api/v1/merchants/{id}           [Auth Required]
DELETE /api/v1/merchants/{id}           [Auth Required]
```

### 🍔 Products (5)
```
GET    /api/v1/products/merchant/{merchantId}
GET    /api/v1/products/{id}
POST   /api/v1/products                 [Auth Required]
PUT    /api/v1/products/{id}            [Auth Required]
DELETE /api/v1/products/{id}            [Auth Required]
```

### 📦 Orders (3)
```
POST   /api/v1/orders                   [Auth Required]
GET    /api/v1/orders/{id}              [Auth Required]
GET    /api/v1/orders                   [Auth Required]
```

### 👤 User Addresses (5)
```
GET    /api/v1/users/addresses          [Auth Required]
POST   /api/v1/users/addresses          [Auth Required]
PUT    /api/v1/users/addresses/{id}     [Auth Required]
PUT    /api/v1/users/addresses/{id}/set-default  [Auth Required]
DELETE /api/v1/users/addresses/{id}     [Auth Required]
```

### 🛒 Shopping Cart (5)
```
GET    /api/v1/cart                     [Auth Required]
POST   /api/v1/cart/items               [Auth Required]
PUT    /api/v1/cart/items/{id}          [Auth Required]
DELETE /api/v1/cart/items/{id}          [Auth Required]
DELETE /api/v1/cart/clear               [Auth Required]
```

### 🎁 Coupons (3)
```
POST   /api/v1/coupons/validate         [Auth Required]
POST   /api/v1/coupons                  [Auth Required]
GET    /api/v1/coupons
```

### 🎯 Campaigns (1)
```
GET    /api/v1/campaigns
```

### 🔔 Notifications (2)
```
GET    /api/v1/notifications            [Auth Required]
POST   /api/v1/notifications/mark-as-read  [Auth Required]
```

### 🚴 Courier (3)
```
GET    /api/v1/courier/orders           [Auth Required]
POST   /api/v1/courier/location/update  [Auth Required]
POST   /api/v1/courier/availability/set [Auth Required]
```

### 🔎 Search (2)
```
GET    /api/v1/search/products
GET    /api/v1/search/merchants
```

### ❤️ Health (1)
```
GET    /health
```

---

## 📊 Statistics

| Category | Count |
|----------|-------|
| **Public Endpoints** | 12 |
| **Protected Endpoints** | 32 |
| **CRUD Modules** | 5 |
| **Search Endpoints** | 2 |
| **Business Logic** | 8 services |

---

## 🔑 Quick Test Sequence

```bash
# 1. Register
POST /api/v1/auth/register

# 2. Create Category
POST /api/v1/categories

# 3. Create Merchant
POST /api/v1/merchants

# 4. Create Product
POST /api/v1/products

# 5. Add to Cart
POST /api/v1/cart/items

# 6. Validate Coupon
POST /api/v1/coupons/validate

# 7. Create Order
POST /api/v1/orders

# 8. Track Order
GET /api/v1/orders/{id}
```

---

**Full Documentation:** [API-DOCUMENTATION.md](API-DOCUMENTATION.md)  
**Postman Collection:** [Getir-API.postman_collection.json](Getir-API.postman_collection.json)
