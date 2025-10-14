# 📋 Getir Clone API - Postman Collections Summary

## 🚀 **Oluşturulan Postman Collection'ları**

### **1. Getir-API-Complete.postman_collection.json**
- **📊 Kapsam**: 100+ endpoint
- **🎯 Hedef**: Kapsamlı API testi
- **✨ Özellikler**:
  - Tüm endpoint'ler detaylı olarak
  - Otomatik token yönetimi
  - Test script'leri
  - Environment variables
  - Response validation

### **2. Getir-API-Extended.postman_collection.json**
- **📊 Kapsam**: 50+ endpoint
- **🎯 Hedef**: Temel + gelişmiş özellikler
- **✨ Özellikler**:
  - Role-based authentication
  - Merchant & Admin paneli
  - System monitoring
  - Performance testing

### **3. Getir-API-Full.postman_collection.json**
- **📊 Kapsam**: 150+ endpoint
- **🎯 Hedef**: Tam kapsamlı test
- **✨ Özellikler**:
  - Tüm endpoint'ler
  - Multiple user roles
  - Advanced testing
  - Comprehensive validation

## 📚 **Collection Detayları**

### **🔐 Authentication Endpoints**
- `POST /api/v1/auth/register` - Kullanıcı kaydı
- `POST /api/v1/auth/login` - Giriş yapma (Customer, Admin, Merchant, Courier)
- `POST /api/v1/auth/refresh` - Token yenileme
- `POST /api/v1/auth/logout` - Çıkış yapma

### **👤 User Management Endpoints**
- `GET /api/v1/users/profile` - Profil bilgileri
- `PUT /api/v1/users/profile` - Profil güncelleme
- `GET /api/v1/users/addresses` - Adres listesi
- `POST /api/v1/users/addresses` - Adres ekleme
- `PUT /api/v1/users/addresses/{id}` - Adres güncelleme
- `DELETE /api/v1/users/addresses/{id}` - Adres silme

### **🏪 Merchant Endpoints**
- `GET /api/v1/merchants` - Merchant listesi
- `GET /api/v1/merchants/{id}` - Merchant detayı
- `GET /api/v1/merchants/search` - Merchant arama
- `POST /api/v1/merchants` - Merchant oluşturma
- `PUT /api/v1/merchants/{id}` - Merchant güncelleme

### **🛍️ Product Endpoints**
- `GET /api/v1/products/merchant/{merchantId}` - Merchant ürünleri
- `GET /api/v1/products/{id}` - Ürün detayı
- `GET /api/v1/products/search` - Ürün arama
- `POST /api/v1/products` - Ürün oluşturma
- `PUT /api/v1/products/{id}` - Ürün güncelleme
- `DELETE /api/v1/products/{id}` - Ürün silme

### **🛒 Cart Endpoints**
- `GET /api/v1/cart` - Sepet içeriği
- `POST /api/v1/cart/items` - Sepete ürün ekleme
- `PUT /api/v1/cart/items/{id}` - Sepet ürünü güncelleme
- `DELETE /api/v1/cart/items/{id}` - Sepetten ürün çıkarma
- `DELETE /api/v1/cart` - Sepeti temizleme

### **📦 Order Endpoints**
- `POST /api/v1/orders` - Sipariş oluşturma
- `GET /api/v1/orders` - Kullanıcı siparişleri
- `GET /api/v1/orders/{id}` - Sipariş detayı
- `POST /api/v1/orders/{id}/cancel` - Sipariş iptal etme

### **🎫 Coupon Endpoints**
- `GET /api/v1/coupons` - Kupon listesi
- `POST /api/v1/coupons/validate` - Kupon doğrulama
- `GET /api/v1/coupons/user` - Kullanıcı kuponları

### **⭐ Review Endpoints**
- `POST /api/v1/reviews` - Yorum oluşturma
- `GET /api/v1/reviews/merchant/{id}` - Merchant yorumları
- `GET /api/v1/reviews/user` - Kullanıcı yorumları

### **🚚 Courier Endpoints**
- `GET /api/v1/couriers/available` - Müsait kuryeler
- `PUT /api/v1/couriers/location` - Kurye konum güncelleme
- `GET /api/v1/couriers/orders` - Kurye siparişleri

### **🔔 Notification Endpoints**
- `GET /api/v1/notifications` - Bildirimler
- `PUT /api/v1/notifications/{id}/read` - Bildirim okundu işaretleme

### **🏢 Merchant Dashboard Endpoints**
- `GET /api/v1/merchant/dashboard/stats` - Dashboard istatistikleri
- `GET /api/v1/merchant/dashboard/recent-orders` - Son siparişler
- `GET /api/v1/merchant/dashboard/top-products` - En çok satan ürünler

### **👑 Admin Panel Endpoints**
- `GET /api/v1/admin/users` - Tüm kullanıcılar
- `GET /api/v1/admin/merchants` - Tüm merchantlar
- `POST /api/v1/admin/merchants/approve` - Merchant onaylama
- `GET /api/v1/admin/statistics` - Sistem istatistikleri

### **🔧 System & Health Endpoints**
- `GET /health` - Sistem sağlık durumu
- `GET /metrics` - Sistem metrikleri
- `GET /api/v1/database-test/connection` - Database bağlantı testi
- `GET /api/v1/database-test/indexes` - Index kontrolü
- `GET /api/v1/database-test/performance-test` - Performans testi
- `GET /api/v1/database-test/schema-validation` - Schema doğrulama

## 🎯 **Kullanım Senaryoları**

### **1. Temel API Testi**
```bash
1. Getir-API-Complete.postman_collection.json kullan
2. Authentication > Login Customer
3. Sırayla tüm endpoint'leri test et
4. Response'ları kontrol et
```

### **2. Role-Based Testing**
```bash
1. Getir-API-Extended.postman_collection.json kullan
2. Farklı rollerle login ol (Customer, Admin, Merchant, Courier)
3. Her rolün yetkili olduğu endpoint'leri test et
4. Yetkisiz erişimleri kontrol et
```

### **3. Comprehensive Testing**
```bash
1. Getir-API-Full.postman_collection.json kullan
2. Tüm endpoint'leri test et
3. Performance testleri çalıştır
4. Database testleri yap
```

## 🔧 **Environment Variables**

### **Otomatik Set Edilenler**
- `accessToken` - Login sonrası
- `refreshToken` - Login sonrası
- `userId` - Login sonrası
- `orderId` - Sipariş oluşturma sonrası

### **Manuel Set Edilmesi Gerekenler**
- `merchantId` - Test için merchant ID
- `productId` - Test için ürün ID
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
- İstediğiniz collection'ı seçin
- Postman'e import edin

### **3. Environment Variables Ayarla**
- `baseUrl`: `https://localhost:7001`
- Diğer değişkenler otomatik set edilecek

### **4. Test Et**
- Authentication ile başla
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

## 🔍 **Troubleshooting**

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

- [Complete Postman Guide](POSTMAN-COMPLETE-GUIDE.md)
- [API Documentation](API-DOCUMENTATION.md)
- [Architecture Guide](ARCHITECTURE.md)
- [Testing Guide](TESTING-GUIDE.md)
- [Docker Guide](DOCKER-GUIDE.md)

---

**🎯 Bu collection'lar ile tüm API endpoint'lerini kolayca test edebilirsiniz!**

## 📋 **Collection Seçim Rehberi**

| Collection | Kullanım Amacı | Endpoint Sayısı | Özellikler |
|------------|----------------|-----------------|------------|
| **Complete** | Genel API testi | 100+ | Detaylı, kapsamlı |
| **Extended** | Role-based test | 50+ | Merchant, Admin paneli |
| **Full** | Tam test | 150+ | Tüm özellikler |

**💡 Öneri**: İlk test için **Complete** collection'ını kullanın, sonra ihtiyacınıza göre diğerlerini deneyin.
