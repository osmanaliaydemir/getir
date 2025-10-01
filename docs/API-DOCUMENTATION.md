# 📘 Getir Clone API - Complete Documentation

## 🎯 API Genel Bakış

**Base URL:** `https://localhost:7001`  
**API Version:** `v1`  
**Total Endpoints:** `44`  
**Authentication:** JWT Bearer Token

---

## 📋 Endpoint Listesi

### 🔐 Authentication (4 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| POST | `/api/v1/auth/register` | ❌ | Yeni kullanıcı kaydı |
| POST | `/api/v1/auth/login` | ❌ | Giriş yap |
| POST | `/api/v1/auth/refresh` | ❌ | Token yenile |
| POST | `/api/v1/auth/logout` | ✅ | Çıkış yap |

---

### 📂 Categories (5 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/categories` | ❌ | Kategori listesi (paginated) |
| GET | `/api/v1/categories/{id}` | ❌ | Kategori detayı |
| POST | `/api/v1/categories` | ✅ | Yeni kategori oluştur |
| PUT | `/api/v1/categories/{id}` | ✅ | Kategori güncelle |
| DELETE | `/api/v1/categories/{id}` | ✅ | Kategori sil (soft) |

**Query Parameters (List):**
- `page` (int): Sayfa numarası (default: 1)
- `pageSize` (int): Sayfa boyutu (default: 20)
- `sortDir` (string): asc/desc (default: desc)

---

### 🏪 Merchants (5 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/merchants` | ❌ | Merchant listesi (paginated) |
| GET | `/api/v1/merchants/{id}` | ❌ | Merchant detayı |
| POST | `/api/v1/merchants` | ✅ | Yeni merchant oluştur |
| PUT | `/api/v1/merchants/{id}` | ✅ | Merchant güncelle |
| DELETE | `/api/v1/merchants/{id}` | ✅ | Merchant sil (soft) |

**Business Rules:**
- Minimum sipariş tutarı kontrolü
- Teslimat ücreti hesaplama
- Rating ve review sistemi
- Açık/kapalı durumu

---

### 🍔 Products (5 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/products/merchant/{merchantId}` | ❌ | Merchant ürünleri (paginated) |
| GET | `/api/v1/products/{id}` | ❌ | Ürün detayı |
| POST | `/api/v1/products` | ✅ | Yeni ürün oluştur |
| PUT | `/api/v1/products/{id}` | ✅ | Ürün güncelle |
| DELETE | `/api/v1/products/{id}` | ✅ | Ürün sil (soft) |

**Business Rules:**
- Stok takibi
- İndirimli fiyat desteği
- Merchant bazlı listeleme
- Availability kontrolü

---

### 📦 Orders (3 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| POST | `/api/v1/orders` | ✅ | Sipariş oluştur (transaction) |
| GET | `/api/v1/orders/{id}` | ✅ | Sipariş detayı |
| GET | `/api/v1/orders` | ✅ | Kullanıcının siparişleri |

**Business Rules:**
- Transaction management (UoW)
- Stok kontrolü ve güncelleme
- Minimum sipariş tutarı kontrolü
- Teslimat ücreti hesaplama
- Kupon uygulaması
- Order number generation

**Order Status Flow:**
```
Pending → Confirmed → Preparing → Ready → OnTheWay → Delivered
                   ↘ Cancelled
```

---

### 👤 User Management (5 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/users/addresses` | ✅ | Kullanıcı adresleri |
| POST | `/api/v1/users/addresses` | ✅ | Adres ekle |
| PUT | `/api/v1/users/addresses/{id}` | ✅ | Adres güncelle |
| PUT | `/api/v1/users/addresses/{id}/set-default` | ✅ | Varsayılan adres seç |
| DELETE | `/api/v1/users/addresses/{id}` | ✅ | Adres sil |

**Business Rules:**
- İlk adres otomatik default
- Default adres değiştirildiğinde diğerleri false
- Soft delete
- Geo-location tracking

---

### 🛒 Shopping Cart (5 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/cart` | ✅ | Sepeti görüntüle |
| POST | `/api/v1/cart/items` | ✅ | Sepete ürün ekle |
| PUT | `/api/v1/cart/items/{id}` | ✅ | Sepet ürünü güncelle |
| DELETE | `/api/v1/cart/items/{id}` | ✅ | Sepetten ürün çıkar |
| DELETE | `/api/v1/cart/clear` | ✅ | Sepeti temizle |

**Business Rules:**
- ⚠️ **Tek Merchant Kuralı**: Sepette aynı anda sadece bir merchant'tan ürün olabilir
- Stok kontrolü
- Otomatik fiyat hesaplama
- İndirimli fiyat desteği
- Quantity update

---

### 🎁 Coupons & Campaigns (4 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| POST | `/api/v1/coupons/validate` | ✅ | Kupon doğrula |
| POST | `/api/v1/coupons` | ✅ | Kupon oluştur |
| GET | `/api/v1/coupons` | ❌ | Kupon listesi |
| GET | `/api/v1/campaigns` | ❌ | Aktif kampanyalar |

**Coupon Types:**
- `Percentage`: Yüzdesel indirim (ör: %20)
- `FixedAmount`: Sabit tutar (ör: 15 TL)

**Validation Rules:**
- Tarih aralığı kontrolü
- Kullanım limiti
- Minimum sipariş tutarı
- Maximum indirim tutarı
- Kullanım sayısı tracking

---

### 🔔 Notifications (2 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/notifications` | ✅ | Bildirimler (paginated) |
| POST | `/api/v1/notifications/mark-as-read` | ✅ | Okundu işaretle |

**Notification Types:**
- `Order`: Sipariş güncellemeleri
- `Promotion`: Kampanya bildirimleri
- `System`: Sistem mesajları
- `Payment`: Ödeme bilgileri

---

### 🚴 Courier (3 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/courier/orders` | ✅ | Atanan siparişler |
| POST | `/api/v1/courier/location/update` | ✅ | Konum güncelle |
| POST | `/api/v1/courier/availability/set` | ✅ | Müsaitlik durumu |

**Features:**
- Real-time location tracking
- Order assignment
- Availability status
- Delivery statistics

---

### 🔎 Search (2 endpoints)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/api/v1/search/products` | ❌ | Ürün ara |
| GET | `/api/v1/search/merchants` | ❌ | Merchant ara |

**Search Filters:**

**Products:**
- `query`: Ürün adı
- `merchantId`: Merchant filtresi
- `categoryId`: Kategori filtresi
- `minPrice`: Minimum fiyat
- `maxPrice`: Maximum fiyat

**Merchants:**
- `query`: Merchant adı
- `categoryId`: Kategori filtresi
- `latitude/longitude`: Konum bazlı arama
- `maxDistance`: Maximum mesafe (km)

---

### ❤️ Health Check (1 endpoint)

| Method | Endpoint | Auth | Açıklama |
|--------|----------|------|----------|
| GET | `/health` | ❌ | API ve DB durumu |

---

## 🔒 Authentication & Authorization

### Access Token
- **Type:** JWT
- **Lifetime:** 60 dakika
- **Location:** `Authorization: Bearer {token}`

### Refresh Token
- **Type:** Random Base64
- **Lifetime:** 7 gün
- **Storage:** Database (RefreshTokens table)

### Login Flow
```
1. POST /auth/register veya /auth/login
2. Receive: { accessToken, refreshToken, expiresAt }
3. Store tokens securely
4. Use accessToken in Authorization header
5. When expired → POST /auth/refresh
6. Receive new tokens
```

---

## 📄 Standard Response Formats

### Success Response
```json
{
  "id": "uuid",
  "name": "...",
  ...
}
```

### Error Response
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errorCode": "VALIDATION_ERROR",
  "errors": {
    "Email": ["Email is required"]
  },
  "traceId": "...",
  "requestId": "..."
}
```

### Pagination Response
```json
{
  "items": [...],
  "total": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## 🚨 Error Codes

| Error Code | HTTP Status | Açıklama |
|------------|-------------|----------|
| `AUTH_INVALID_CREDENTIALS` | 401 | Email veya şifre hatalı |
| `AUTH_EMAIL_EXISTS` | 409 | Email zaten kayıtlı |
| `AUTH_INVALID_REFRESH_TOKEN` | 401 | Geçersiz refresh token |
| `AUTH_REFRESH_TOKEN_EXPIRED` | 401 | Refresh token süresi dolmuş |
| `AUTH_ACCOUNT_DEACTIVATED` | 401 | Hesap deaktif |
| `NOT_FOUND_*` | 404 | İlgili kayıt bulunamadı |
| `VALIDATION_ERROR` | 400 | Request validation hatası |
| `INSUFFICIENT_STOCK` | 400 | Yetersiz stok |
| `BELOW_MINIMUM_ORDER` | 400 | Minimum sipariş tutarının altında |
| `CART_DIFFERENT_MERCHANT` | 400 | Farklı merchant'tan ürün ekleme |
| `CONFLICT_COUPON_CODE` | 409 | Kupon kodu zaten var |

---

## 🔄 Business Flows

### Complete E-Commerce Flow

```
1. User Registration
   POST /auth/register
   → Returns: { accessToken, refreshToken }

2. Browse Categories
   GET /categories
   → Returns: Paginated categories

3. Browse Merchants
   GET /merchants?categoryId={id}
   → Returns: Filtered merchants

4. View Products
   GET /products/merchant/{merchantId}
   → Returns: Merchant's products

5. Add to Cart
   POST /cart/items
   Body: { merchantId, productId, quantity }
   → Validates: stock, merchant constraint

6. View Cart
   GET /cart
   → Returns: Cart summary with totals

7. Add Address (if needed)
   POST /users/addresses
   → Auto sets as default if first

8. Validate Coupon (optional)
   POST /coupons/validate
   Body: { code, orderAmount }
   → Returns: discount amount

9. Create Order
   POST /orders
   Body: { merchantId, items, address, payment }
   → Transaction: creates order + updates stock

10. Track Order
    GET /orders/{id}
    → Returns: Order status, ETA, details
```

---

### Shopping Cart Constraint

⚠️ **Önemli Kural**: Sepette aynı anda sadece **bir merchant**'tan ürün olabilir.

**Senaryolar:**
```
✅ Sepet boş → Migros'tan ürün ekle → Başarılı
✅ Sepette Migros ürünü var → Migros'tan başka ürün ekle → Başarılı
❌ Sepette Migros ürünü var → CarrefourSA'dan ürün ekle → Hata
   Error: "CART_DIFFERENT_MERCHANT"
   
✓ Çözüm: Önce sepeti temizle (DELETE /cart/clear), sonra yeni merchant'tan ekle
```

---

### Coupon Validation Logic

```csharp
// Percentage Coupon
DiscountAmount = OrderAmount * (DiscountValue / 100)
if (DiscountAmount > MaximumDiscountAmount)
    DiscountAmount = MaximumDiscountAmount

// Fixed Amount Coupon
DiscountAmount = DiscountValue

// Final Price
FinalTotal = SubTotal - DiscountAmount + DeliveryFee
```

**Validation Checks:**
1. Coupon kodu geçerli mi?
2. Tarih aralığında mı?
3. Kullanım limiti dolmuş mu?
4. Minimum sipariş tutarı karşılanmış mı?

---

### Order Creation Flow (Transaction)

```
BEGIN TRANSACTION

1. Merchant kontrolü (active mi?)
2. Her ürün için:
   - Ürün var mı?
   - Stok yeterli mi?
   - Fiyat hesaplama
3. Minimum sipariş tutarı kontrolü
4. Order oluştur
5. OrderLines oluştur
6. Stokları güncelle
7. Cart temizle (opsiyonel)

COMMIT TRANSACTION

Hata durumunda: ROLLBACK
```

---

## 🎯 Pagination Standard

Tüm list endpoint'lerinde kullanılır:

**Request:**
```http
GET /api/v1/products/merchant/{id}?page=2&pageSize=10&sortDir=asc
```

**Response:**
```json
{
  "items": [...],
  "total": 156,
  "page": 2,
  "pageSize": 10,
  "totalPages": 16,
  "hasPreviousPage": true,
  "hasNextPage": true
}
```

---

## 🔑 Request Headers

### Required Headers
```http
Content-Type: application/json
```

### Optional Headers
```http
X-Request-Id: custom-request-id  # Yoksa otomatik üretilir
```

### Authentication
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📊 Rate Limiting (Future)

Currently: **Not implemented**

Planned:
- 100 requests/minute per user
- 1000 requests/hour per IP
- Configurable per endpoint

---

## 🧪 Testing with Postman

### Quick Start
```
1. Import: docs/Getir-API.postman_collection.json
2. Run: Register → Auto saves token
3. Run: Create Category → Auto saves categoryId
4. Run: Create Merchant → Auto saves merchantId
5. Test other endpoints!
```

### Auto-Managed Variables
- ✅ accessToken
- ✅ refreshToken
- ✅ merchantId
- ✅ productId
- ✅ orderId
- ✅ categoryId
- ✅ addressId
- ✅ cartItemId
- ✅ couponId

---

## 🐛 Common Issues

### 401 Unauthorized
**Causes:**
- Token expired
- Invalid token
- Missing Authorization header

**Solutions:**
- Login again
- Use refresh token
- Check header format

### 400 Bad Request
**Causes:**
- Validation errors
- Business rule violations

**Solutions:**
- Check request body
- Verify required fields
- Follow validation rules

### 404 Not Found
**Causes:**
- Invalid ID
- Resource deleted

**Solutions:**
- Verify ID exists
- Check isActive status

---

## 📈 Performance Tips

### Optimization
- Use pagination (default: 20 items)
- Include only needed navigation properties
- AsNoTracking for read-only queries

### Caching (Future)
- Redis for merchant/product lists
- Memory cache for categories
- Distributed cache for sessions

---

## 🔐 Security Best Practices

### Implemented
✅ JWT with secure secret  
✅ Password hashing (PBKDF2)  
✅ Refresh token rotation  
✅ HTTPS enforcement  
✅ Request ID correlation  
✅ Structured logging  

### Recommendations
- Use HTTPS in production
- Implement rate limiting
- Add CORS configuration
- Enable request throttling
- Use API keys for third-party

---

## 📝 Examples

### Register & Login
```bash
# Register
curl -X POST https://localhost:7001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Test123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# Login
curl -X POST https://localhost:7001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Test123!"
  }'
```

### Create Order (with Auth)
```bash
curl -X POST https://localhost:7001/api/v1/orders \
  -H "Authorization: Bearer {your-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "merchantId": "uuid",
    "items": [{
      "productId": "uuid",
      "quantity": 2
    }],
    "deliveryAddress": "Kadıköy, İstanbul",
    "deliveryLatitude": 40.9897,
    "deliveryLongitude": 29.0257,
    "paymentMethod": "CreditCard"
  }'
```

### Search Products
```bash
curl "https://localhost:7001/api/v1/search/products?query=süt&minPrice=10&maxPrice=50&page=1&pageSize=20"
```

---

## 🎉 Summary

**Total Coverage:**
- ✅ 44 Endpoints
- ✅ 8 Domain Modules
- ✅ 14 Database Tables
- ✅ Full CRUD Operations
- ✅ Transaction Support
- ✅ Validation & Error Handling
- ✅ Pagination & Filtering
- ✅ JWT Authentication
- ✅ Shopping Cart Logic
- ✅ Coupon System
- ✅ Search Functionality

---

**For detailed Postman usage:** See [POSTMAN-GUIDE.md](POSTMAN-GUIDE.md)  
**For connection strings:** See [CONNECTION-STRING-GUIDE.md](CONNECTION-STRING-GUIDE.md)
