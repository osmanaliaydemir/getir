# 📮 Postman Collection Kullanım Kılavuzu

## 🚀 Hızlı Başlangıç

### 1. Collection'ı İçe Aktarma

1. Postman'i açın
2. **Import** butonuna tıklayın
3. `docs/Getir-API.postman_collection.json` dosyasını seçin
4. **Import** yapın

### 2. Environment Variables (Otomatik)

Collection, aşağıdaki değişkenleri otomatik olarak yönetir:

| Değişken | Açıklama | Otomatik Güncellenir |
|----------|----------|----------------------|
| `baseUrl` | API base URL (default: https://localhost:7001) | ❌ Manuel |
| `accessToken` | JWT access token | ✅ Login/Register sonrası |
| `refreshToken` | JWT refresh token | ✅ Login/Register sonrası |
| `merchantId` | Son oluşturulan merchant ID | ✅ Create Merchant sonrası |
| `productId` | Son oluşturulan product ID | ✅ Create Product sonrası |
| `orderId` | Son oluşturulan order ID | ✅ Create Order sonrası |
| `categoryId` | Category ID | ✅ Auto (Create Category sonrası) |
| `addressId` | Address ID | ✅ Auto (Add Address sonrası) |
| `cartItemId` | Cart Item ID | ✅ Auto (Add to Cart sonrası) |
| `couponId` | Coupon ID | ✅ Auto (Create Coupon sonrası) |
| `notificationId` | Notification ID | ❌ Manuel |

### 3. İlk Kullanım Senaryosu

#### Adım 1: Health Check
```
GET /health
```
API'nin ayakta olduğunu ve database bağlantısının çalıştığını kontrol edin.

#### Adım 2: Register (Kayıt Ol)
```
POST /api/v1/auth/register
```
- Yeni kullanıcı oluşturur
- Otomatik olarak `accessToken` ve `refreshToken` değişkenlerini set eder
- Artık korumalı endpoint'lere erişebilirsiniz

#### Adım 3: Category ID Alma
Database'de oluşturduğumuz kategorilerden birinin ID'sini alın:
```sql
SELECT TOP 1 Id FROM Categories WHERE IsActive = 1
```
Bu ID'yi Postman'de `categoryId` değişkenine manuel olarak set edin.

#### Adım 4: Merchant Oluşturma
```
POST /api/v1/merchants (Create Merchant)
```
- Bearer token otomatik eklenir
- Response'tan `merchantId` otomatik kaydedilir

#### Adım 5: Product Oluşturma
```
POST /api/v1/products (Create Product)
```
- `{{merchantId}}` otomatik kullanılır
- Response'tan `productId` otomatik kaydedilir

#### Adım 6: Sipariş Oluşturma
```
POST /api/v1/orders (Create Order)
```
- Transaction örneği
- Stok kontrolü ve güncelleme
- Minimum sipariş tutarı kontrolü

---

## 🔐 Authentication Flow

### Tam Akış Örneği

1. **Register** → Token'lar otomatik kaydedilir
2. **Login** → Token'lar yenilenir
3. **Korumalı endpoint'lere erişim** → Bearer token otomatik eklenir
4. **Token süresi dolunca** → Refresh Token kullan
5. **Logout** → Tüm refresh token'lar iptal edilir

### Manuel Token Kullanımı

Eğer token'ı manuel eklemek isterseniz:

1. Authorization tab'ine gidin
2. Type: **Bearer Token** seçin
3. Token alanına `{{accessToken}}` yazın

---

## 📊 Test Scripts (Otomatik)

Her request'te otomatik çalışan test scriptleri:

### Register/Login
```javascript
// Token'ları otomatik kaydeder
pm.environment.set('accessToken', response.accessToken);
pm.environment.set('refreshToken', response.refreshToken);

// Başarı kontrolü
pm.test('Login successful', () => {
    pm.expect(response.accessToken).to.be.a('string');
});
```

### Create Merchant/Product/Order
```javascript
// ID'leri otomatik kaydeder
pm.environment.set('merchantId', response.id);
pm.environment.set('productId', response.id);
pm.environment.set('orderId', response.id);
```

---

## 🎯 Endpoint Kategorileri

### 1. 🔐 Authentication (4 endpoint)
- ✅ **Register** - Yeni kullanıcı kaydı
- ✅ **Login** - Email/password ile giriş
- ✅ **Refresh Token** - Access token yenileme
- ✅ **Logout** - Oturumu kapatma

### 2. 📂 Categories (5 endpoint)
- ✅ **List Categories** - Tüm kategoriler
- ✅ **Get Category By ID** - Kategori detayı
- ✅ **Create Category** - Yeni kategori (Auth)
- ✅ **Update Category** - Güncelleme (Auth)
- ✅ **Delete Category** - Soft delete (Auth)

### 3. 🏪 Merchants (5 endpoint)
- ✅ **List Merchants** - Pagination desteği
- ✅ **Get Merchant By ID** - Detay görüntüleme
- ✅ **Create Merchant** - Yeni merchant ekleme (Auth)
- ✅ **Update Merchant** - Güncelleme (Auth)
- ✅ **Delete Merchant** - Soft delete (Auth)

### 4. 🍔 Products (5 endpoint)
- ✅ **List Products by Merchant** - Merchant'a göre filtreleme
- ✅ **Get Product By ID** - Detay görüntüleme
- ✅ **Create Product** - Yeni ürün ekleme (Auth)
- ✅ **Update Product** - Güncelleme (Auth)
- ✅ **Delete Product** - Soft delete (Auth)

### 5. 📦 Orders (3 endpoint)
- ✅ **Create Order** - Transaction ile sipariş oluşturma
- ✅ **Get Order By ID** - Sipariş detayı
- ✅ **List User Orders** - Kullanıcının siparişleri

### 6. 👤 User Management (5 endpoint)
- ✅ **Get User Addresses** - Kullanıcı adresleri
- ✅ **Add Address** - Yeni adres (ilk adres otomatik default)
- ✅ **Update Address** - Adres güncelleme
- ✅ **Set Default Address** - Varsayılan adres seçimi
- ✅ **Delete Address** - Adres silme (soft delete)

### 7. 🛒 Shopping Cart (5 endpoint)
- ✅ **Get Cart** - Sepet görüntüleme
- ✅ **Add to Cart** - Ürün ekleme (merchant kontrolü)
- ✅ **Update Cart Item** - Miktar güncelleme
- ✅ **Remove Cart Item** - Ürün çıkarma
- ✅ **Clear Cart** - Sepeti temizleme

### 8. 🎁 Coupons & Campaigns (4 endpoint)
- ✅ **Validate Coupon** - Kupon doğrulama ve indirim hesaplama
- ✅ **Create Coupon** - Yeni kupon oluşturma (Auth)
- ✅ **Get Coupons** - Aktif kuponlar
- ✅ **Get Active Campaigns** - Devam eden kampanyalar

### 9. 🔔 Notifications (2 endpoint)
- ✅ **Get Notifications** - Kullanıcı bildirimleri
- ✅ **Mark as Read** - Bildirimleri okundu işaretle

### 10. 🚴 Courier (3 endpoint)
- ✅ **Get Assigned Orders** - Atanan siparişler
- ✅ **Update Location** - Konum güncelleme
- ✅ **Set Availability** - Müsaitlik durumu

### 11. 🔎 Search (2 endpoint)
- ✅ **Search Products** - Ürün arama (multi-criteria)
- ✅ **Search Merchants** - Market/Restoran arama

### 12. ❤️ Health Check (1 endpoint)
- ✅ **Health Check** - API ve DB durumu

### 📊 **TOPLAM: 44 Endpoint**

---

## 🧪 Test Senaryoları

### Senaryo 1: Tam E-Commerce Akışı
```
1. Register → Token al ✓
2. Create Category → categoryId al ✓
3. Add Address → addressId al ✓
4. Create Merchant → merchantId al ✓
5. Create Product → productId al ✓
6. Add to Cart → Sepete ekle ✓
7. Validate Coupon → Kupon kontrolü ✓
8. Create Order → orderId al ✓
9. Get Order By ID → Sipariş detayını gör ✓
```

### Senaryo 2: Pagination Testi
```
1. List Merchants (page=1, pageSize=5)
2. List Merchants (page=2, pageSize=5)
3. Sonuçları karşılaştır
```

### Senaryo 3: Error Handling
```
1. Login (yanlış şifre) → 401 AUTH_INVALID_CREDENTIALS
2. Create Order (yetersiz stok) → 400 INSUFFICIENT_STOCK
3. Get Merchant (olmayan ID) → 404 NOT_FOUND_MERCHANT
4. Add to Cart (farklı merchant) → 400 CART_DIFFERENT_MERCHANT
5. Validate Coupon (expired) → 200 isValid: false
```

### Senaryo 4: Shopping Cart Flow
```
1. Add to Cart → Ürün ekle
2. Add to Cart (aynı merchant) → Başarılı
3. Add to Cart (farklı merchant) → Hata (tek merchant kuralı)
4. Update Cart Item → Miktar değiştir
5. Get Cart → Sepet özeti
6. Create Order → Sepetten sipariş oluştur
7. Cart otomatik temizlenir
```

### Senaryo 5: Token Yenileme
```
1. Login → Token al
2. 60 dakika bekle (veya token'ı manuel sil)
3. Refresh Token → Yeni token al
4. Korumalı endpoint'e erişim → Başarılı olmalı
```

---

## 🔍 Pagination Query Parametreleri

Tüm list endpoint'lerinde kullanılabilir:

| Parametre | Tip | Default | Açıklama |
|-----------|-----|---------|----------|
| `page` | int | 1 | Sayfa numarası |
| `pageSize` | int | 20 | Sayfa başına kayıt |
| `sortBy` | string | - | Sıralama alanı (ör: "name") |
| `sortDir` | string | "desc" | Sıralama yönü ("asc" veya "desc") |

**Örnek:**
```
GET /api/v1/merchants?page=2&pageSize=10&sortBy=rating&sortDir=desc
```

---

## 🐛 Troubleshooting

### Problem: 401 Unauthorized
**Çözüm:** 
- Login veya Register yapın
- Token'ın expire olmadığını kontrol edin
- Refresh token ile yenileyin

### Problem: 404 Not Found
**Çözüm:**
- `merchantId`, `productId` vb. değişkenlerin set edildiğini kontrol edin
- ID'lerin database'de var olduğunu kontrol edin

### Problem: 400 Bad Request (Validation Error)
**Çözüm:**
- Request body'deki required alanları kontrol edin
- Email format, şifre uzunluğu vb. validasyon kurallarına uyun

### Problem: 500 Internal Server Error
**Çözüm:**
- API loglarını kontrol edin
- Database bağlantısını kontrol edin
- Connection string'i doğrulayın

---

## 📝 Önemli Notlar

1. **SSL Sertifikası:** Development ortamında self-signed sertifika hatası alırsanız:
   - Postman Settings → General → SSL certificate verification → **OFF**

2. **Base URL:** Production'a geçerken:
   - Collection variables → `baseUrl` → `https://api.yourdomain.com`

3. **Category ID:** İlk kullanımda manuel set edilmesi gereken tek değişken

4. **Token Expiry:** Access token 60 dakika, Refresh token 7 gün geçerli

5. **Request ID:** Her request otomatik `X-Request-Id` header'ı alır (log tracking için)

---

## 🎉 Bonus: Newman (CLI) ile Çalıştırma

Collection'ı komut satırından çalıştırmak için:

```bash
# Newman kur
npm install -g newman

# Collection'ı çalıştır
newman run docs/Getir-API.postman_collection.json

# Environment ile çalıştır
newman run docs/Getir-API.postman_collection.json \
  --env-var "baseUrl=https://localhost:7001"

# HTML report oluştur
newman run docs/Getir-API.postman_collection.json \
  --reporters cli,html \
  --reporter-html-export report.html
```

---

**Happy Testing! 🚀**
