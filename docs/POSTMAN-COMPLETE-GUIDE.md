# 📋 Getir Clone API - Complete Postman Guide

## 🚀 **Postman Collection'ları**

### **1. Getir-API-Complete.postman_collection.json**
- **Kapsam**: Tüm endpoint'ler detaylı olarak
- **Özellikler**: 
  - 100+ endpoint
  - Otomatik token yönetimi
  - Test script'leri
  - Environment variables

### **2. Getir-API-Extended.postman_collection.json**
- **Kapsam**: Temel endpoint'ler + gelişmiş özellikler
- **Özellikler**:
  - 50+ endpoint
  - Role-based authentication
  - Merchant & Admin paneli
  - System monitoring

## 🔧 **Kurulum ve Kullanım**

### **1. Postman'e Import Etme**
```bash
1. Postman'i açın
2. Import butonuna tıklayın
3. "Getir-API-Complete.postman_collection.json" dosyasını seçin
4. Import edin
```

### **2. Environment Variables Ayarlama**
```json
{
  "baseUrl": "https://localhost:7001",
  "accessToken": "",
  "adminToken": "",
  "merchantToken": "",
  "courierToken": "",
  "userId": "",
  "merchantId": "",
  "orderId": "",
  "productId": ""
}
```

### **3. Authentication Setup**
1. **Customer Login**: `customer1@example.com` / `Test123!`
2. **Admin Login**: `admin@getir.com` / `Admin123!`
3. **Merchant Login**: `merchant@example.com` / `Test123!`
4. **Courier Login**: `courier@example.com` / `Test123!`

## 📚 **Endpoint Kategorileri**

### **🔐 Authentication & Users**
- `POST /api/v1/auth/register` - Kullanıcı kaydı
- `POST /api/v1/auth/login` - Giriş yapma
- `POST /api/v1/auth/refresh` - Token yenileme
- `POST /api/v1/auth/logout` - Çıkış yapma
- `GET /api/v1/users/profile` - Profil bilgileri
- `PUT /api/v1/users/profile` - Profil güncelleme
- `GET /api/v1/users/addresses` - Adres listesi
- `POST /api/v1/users/addresses` - Adres ekleme

### **🏪 Merchants & Products**
- `GET /api/v1/merchants` - Merchant listesi
- `GET /api/v1/merchants/{id}` - Merchant detayı
- `GET /api/v1/merchants/search` - Merchant arama
- `POST /api/v1/merchants` - Merchant oluşturma
- `GET /api/v1/products/merchant/{merchantId}` - Merchant ürünleri
- `GET /api/v1/products/{id}` - Ürün detayı
- `GET /api/v1/products/search` - Ürün arama
- `POST /api/v1/products` - Ürün oluşturma

### **🛒 Cart & Orders**
- `GET /api/v1/cart` - Sepet içeriği
- `POST /api/v1/cart/items` - Sepete ürün ekleme
- `PUT /api/v1/cart/items/{id}` - Sepet ürünü güncelleme
- `DELETE /api/v1/cart/items/{id}` - Sepetten ürün çıkarma
- `DELETE /api/v1/cart` - Sepeti temizleme
- `POST /api/v1/orders` - Sipariş oluşturma
- `GET /api/v1/orders` - Kullanıcı siparişleri
- `GET /api/v1/orders/{id}` - Sipariş detayı
- `POST /api/v1/orders/{id}/cancel` - Sipariş iptal etme

### **🎫 Coupons & Reviews**
- `GET /api/v1/coupons` - Kupon listesi
- `POST /api/v1/coupons/validate` - Kupon doğrulama
- `GET /api/v1/coupons/user` - Kullanıcı kuponları
- `POST /api/v1/reviews` - Yorum oluşturma
- `GET /api/v1/reviews/merchant/{id}` - Merchant yorumları
- `GET /api/v1/reviews/user` - Kullanıcı yorumları

### **🚚 Courier & Notifications**
- `GET /api/v1/couriers/available` - Müsait kuryeler
- `PUT /api/v1/couriers/location` - Kurye konum güncelleme
- `GET /api/v1/couriers/orders` - Kurye siparişleri
- `GET /api/v1/notifications` - Bildirimler
- `PUT /api/v1/notifications/{id}/read` - Bildirim okundu işaretleme

### **🏢 Merchant Management**
- `GET /api/v1/merchant/dashboard/stats` - Dashboard istatistikleri
- `GET /api/v1/merchant/dashboard/recent-orders` - Son siparişler
- `GET /api/v1/merchant/dashboard/top-products` - En çok satan ürünler
- `GET /api/v1/merchant/orders` - Merchant siparişleri
- `POST /api/v1/merchant/products` - Ürün oluşturma
- `PUT /api/v1/merchant/products/{id}` - Ürün güncelleme
- `DELETE /api/v1/merchant/products/{id}` - Ürün silme

### **👑 Admin Panel**
- `GET /api/v1/admin/users` - Tüm kullanıcılar
- `GET /api/v1/admin/merchants` - Tüm merchantlar
- `POST /api/v1/admin/merchants/approve` - Merchant onaylama
- `GET /api/v1/admin/orders` - Tüm siparişler
- `GET /api/v1/admin/statistics` - Sistem istatistikleri

### **🔧 System & Health**
- `GET /health` - Sistem sağlık durumu
- `GET /metrics` - Sistem metrikleri
- `GET /api/v1/database-test/connection` - Database bağlantı testi
- `GET /api/v1/database-test/indexes` - Index kontrolü
- `GET /api/v1/database-test/performance-test` - Performans testi
- `GET /api/v1/database-test/schema-validation` - Schema doğrulama

## 🧪 **Test Senaryoları**

### **1. Temel Akış Testi**
```bash
1. Register User
2. Login
3. Get Merchants
4. Add to Cart
5. Create Order
6. Get User Orders
```

### **2. Merchant Akış Testi**
```bash
1. Login as Merchant
2. Get Dashboard Stats
3. Create Product
4. Get Merchant Orders
5. Update Product
```

### **3. Admin Akış Testi**
```bash
1. Login as Admin
2. Get All Users
3. Get All Merchants
4. Approve Merchant
5. Get System Statistics
```

### **4. Courier Akış Testi**
```bash
1. Login as Courier
2. Update Location
3. Get Available Orders
4. Accept Order
5. Update Order Status
```

## 🔍 **Environment Variables**

### **Otomatik Set Edilenler**
- `accessToken` - Login sonrası otomatik set edilir
- `refreshToken` - Login sonrası otomatik set edilir
- `userId` - Login sonrası otomatik set edilir
- `orderId` - Sipariş oluşturma sonrası otomatik set edilir

### **Manuel Set Edilmesi Gerekenler**
- `merchantId` - Test için kullanılacak merchant ID
- `productId` - Test için kullanılacak ürün ID
- `cartItemId` - Sepet testi için
- `notificationId` - Bildirim testi için

## 📊 **Test Script'leri**

### **Global Test Script**
```javascript
// Response time kontrolü
pm.test('Response time is less than 5000ms', function () {
    pm.expect(pm.response.responseTime).to.be.below(5000);
});

// Response structure kontrolü
pm.test('Response has proper structure', function () {
    const response = pm.response.json();
    pm.expect(response).to.have.property('isSuccess');
    pm.expect(response).to.have.property('message');
});
```

### **Login Test Script**
```javascript
// Token otomatik set etme
if (pm.response.code === 200) {
    const response = pm.response.json();
    if (response.data && response.data.accessToken) {
        pm.collectionVariables.set('accessToken', response.data.accessToken);
        pm.collectionVariables.set('userId', response.data.user.id);
    }
}
```

## 🚀 **Hızlı Başlangıç**

### **1. Projeyi Çalıştır**
```bash
dotnet run --project src/WebApi
```

### **2. Postman Collection'ı Import Et**
- `Getir-API-Complete.postman_collection.json` dosyasını import et

### **3. Environment Variables Ayarla**
- `baseUrl`: `https://localhost:7001`
- Diğer değişkenler otomatik set edilecek

### **4. Test Et**
- `Authentication > Login Customer` ile başla
- Diğer endpoint'leri sırayla test et

## 📈 **Performans Testi**

### **Database Test Endpoint'leri**
- `GET /api/v1/database-test/connection` - Bağlantı testi
- `GET /api/v1/database-test/indexes` - Index kontrolü
- `GET /api/v1/database-test/performance-test` - Performans testi
- `GET /api/v1/database-test/schema-validation` - Schema doğrulama

### **Health Check Endpoint'leri**
- `GET /health` - Sistem sağlığı
- `GET /metrics` - Sistem metrikleri

## 🔧 **Troubleshooting**

### **SSL Certificate Hatası**
- Postman Settings > General > SSL certificate verification: OFF

### **CORS Hatası**
- Browser'da test ediyorsanız CORS ayarlarını kontrol edin

### **Authentication Hatası**
- Token'ın süresi dolmuş olabilir, yeniden login yapın

### **Database Bağlantı Hatası**
- SQL Server'ın çalıştığından emin olun
- Connection string'i kontrol edin

## 📚 **Ek Kaynaklar**

- [API Documentation](API-DOCUMENTATION.md)
- [Architecture Guide](ARCHITECTURE.md)
- [Testing Guide](TESTING-GUIDE.md)
- [Docker Guide](DOCKER-GUIDE.md)

---

**🎯 Bu rehber ile tüm API endpoint'lerini kolayca test edebilirsiniz!**
