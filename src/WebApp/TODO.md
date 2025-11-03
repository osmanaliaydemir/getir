# WebApp İş Listesi

Bu dokümantasyon, WebApp projesinde tamamlanması gereken işleri kategorize ederek listeler.

## 📋 İçindekiler
1. [API Entegrasyonları](#api-entegrasyonları)
2. [Servis Katmanı İyileştirmeleri](#servis-katmanı-iyileştirmeleri)
3. [UI/UX İyileştirmeleri](#uiux-iyileştirmeleri)
4. [Güvenlik](#güvenlik)
5. [Performans Optimizasyonları](#performans-optimizasyonları)
6. [Mimari İyileştirmeler](#mimari-iyileştirmeler)
7. [Test ve Kalite](#test-ve-kalite)
8. [Dokümantasyon](#dokümantasyon)

---

## 🔌 API Entegrasyonları

### Güvenlik Sayfası (Security.razor)
- [x] **API'den güvenlik bilgilerini çekme** (Satır 388)
  - Aktif oturumları API'den almak ✅
  - Güvenlik skorunu API'den hesaplamak ✅
  - İki faktörlü doğrulama durumunu API'den almak ✅
- [x] **E-posta değiştirme API entegrasyonu** (Satır 455) ✅
- [x] **Telefon değiştirme API entegrasyonu** (Satır 475) ✅
- [x] **2FA durumu değiştirme API entegrasyonu** (Satır 494) ✅
- [x] **Tek cihaz oturumu sonlandırma API entegrasyonu** (Satır 511) ✅
- [x] **Tüm cihazlardan çıkış yapma API entegrasyonu** (Satır 528) ✅

### Ayarlar Sayfası (Settings.razor)
- [x] **Kullanıcı ayarlarını API'den çekme** (Satır 317) ✅
- [x] **Kullanıcı istatistiklerini API'den çekme** (Satır 354) ✅
  - Toplam sipariş sayısı ✅
  - Toplam harcama ✅
  - Favori ürün sayısı ✅
  - Kayıtlı adres sayısı ✅
- [x] **Ayarları API'ye kaydetme** (Satır 388) ✅

### Bildirimler Sayfası (Notifications.razor)
- [x] **Bildirimleri API'den çekme** (Satır 177) ✅
- [x] **Bildirimi okundu olarak işaretleme API entegrasyonu** (Satır 232) ✅
- [x] **Tüm bildirimleri okundu olarak işaretleme API entegrasyonu** (Satır 253) ✅
- [x] **Bildirim silme API entegrasyonu** (Satır 279) ✅
- [x] **Tüm bildirimleri silme API entegrasyonu** (Satır 299) ✅
- [x] **NotificationService servisini oluşturma** ✅

### Favoriler Sayfası (Favorites.razor)
- [x] **Favori ürünleri API'den çekme** (Satır 188) ✅
- [x] **Favori ürünü çıkarma API entegrasyonu** (Satır 260) ✅

### Hesap Sayfası (Account.razor)
- [x] **Adres ekleme modal implementasyonu** (Satır 618) ✅
- [x] **Adres düzenleme modal implementasyonu** (Satır 624) ✅
- [x] **Varsayılan adres ayarlama implementasyonu** (Satır 630) ✅
  - Not: `SetDefaultAddressAsync` metodu `UserService`'de mevcut, UI'da kullanıldı ✅

### Checkout Sayfası (Checkout.razor)
- [x] **Adres ekleme modal implementasyonu** (Satır 396) ✅

### Ürün Detay Sayfası (ProductDetail.razor)
- [x] **Favorilere ekleme/çıkarma fonksiyonalitesi** (Satır 351)
- [x] **Paylaşma fonksiyonalitesi** (Satır 357)

### Yardım Sayfası (Help.razor)
- [x] **Yardım arama API entegrasyonu** (Satır 396)
- [x] **Makale değerlendirme API entegrasyonu** (Satır 414)

### İletişim Sayfası (Contact.razor)
- [x] **İletişim formu gönderme API entegrasyonu** (Satır 305)
- [x] **ContactService servisini oluşturma**

### İzinler Sayfası (Permissions.razor)
- [x] **İzin güncellemesi API entegrasyonu** (Satır 358)

---

## 🔧 Servis Katmanı İyileştirmeleri

### Eksik Servisler
- [x] **NotificationService oluşturulmalı** ✅
  - `GetUserNotificationsAsync()`
  - `MarkAsReadAsync(Guid notificationId)`
  - `MarkAllAsReadAsync()`
  - `DeleteNotificationAsync(Guid notificationId)`
  - `ClearAllNotificationsAsync()`
- [x] **ContactService oluşturulmalı** ✅
  - `SubmitContactFormAsync(ContactForm form)`
- [x] **SecurityService oluşturulmalı** ✅
  - `GetSecurityInfoAsync()`
  - `ChangeEmailAsync(ChangeEmailRequest request)`
  - `ChangePhoneAsync(ChangePhoneRequest request)`
  - `ToggleTwoFactorAsync(bool enabled)`
  - `LogoutDeviceAsync(Guid sessionId)`
  - `LogoutAllDevicesAsync()`
  - `GetActiveSessionsAsync()`

### Interface Eksiklikleri
- [x] **UserService için IUserService interface'i eklenmeli**
- [x] **ProductService için IProductService interface'i eklenmeli**
- [x] **CartService için ICartService interface'i eklenmeli**
- [x] **OrderService için IOrderService interface'i eklenmeli**
- [x] **MerchantService için IMerchantService interface'i eklenmeli**
- [x] **AuthService için IAuthService interface'i eklenmeli**
- [x] **LocalizationService için ILocalizationService interface'i eklenmeli**
- [x] **SeoService için ISeoService interface'i eklenmeli**

### UserService Genişletme
- [x] **GetUserSettingsAsync() metodu eklenmeli** ✅
- [x] **UpdateUserSettingsAsync(UserSettings settings) metodu eklenmeli** ✅
- [x] **GetUserStatisticsAsync() metodu eklenmeli** ✅

### AdvancedPwaService Tamamlama
- [x] **ProcessAddToCartAction() tam implementasyon** (Satır 275) ✅
- [x] **ProcessRemoveFromCartAction() tam implementasyon** (Satır 282) ✅
- [x] **ProcessUpdateProfileAction() tam implementasyon** (Satır 292) ✅
- [x] **ProcessPlaceOrderAction() tam implementasyon** (Satır 299) ✅
- [x] **Gerçek online durumu kontrolü** (Satır 197-207) ✅

### ApiClient İyileştirmeleri
- [x] **Console.WriteLine debug logları kaldırılmalı** (Production'da performans sorunu) ✅
  - Logger kullanılmalı (Serilog zaten mevcut) ✅
- [x] **Error handling iyileştirmeleri** ✅
- [x] **Retry policy eklenmeli** (Polly zaten ekli ama kullanılmıyor) ✅

---

## 🎨 UI/UX İyileştirmeleri

### Modal Bileşenleri
- [ ] **Adres Ekleme Modal Komponenti**
  - `AddAddressModal.razor` oluşturulmalı
  - Form validasyonu (FluentValidation)
  - Konum seçimi (harita entegrasyonu)
- [ ] **Adres Düzenleme Modal Komponenti**
  - `EditAddressModal.razor` oluşturulmalı
  - Mevcut adres bilgilerini yükleme
  - Form validasyonu

### Bileşen Eksiklikleri
- [x] **Paylaşma Bileşeni** (`ShareComponent.razor`)
  - Sosyal medya paylaşımları ✅
  - Link kopyalama ✅
  - WhatsApp/Telegram paylaşımı ✅
- [x] **Canlı Destek Bileşeni** (`LiveChatComponent.razor`)
  - SignalR ile gerçek zamanlı chat ✅
  - Mesajlaşma UI ✅

### Sayfa İyileştirmeleri
- [x] **Favoriler sayfasında loading state iyileştirmesi** ✅
- [x] **Bildirimler sayfasında real-time güncellemeler** (SignalR) ✅
- [x] **Sipariş takibi sayfasında gerçek zamanlı güncellemeler** ✅

---

## 🔒 Güvenlik

### Güvenlik İyileştirmeleri
- [x] **API token yönetimi iyileştirmesi** ✅
  - Refresh token mekanizması
  - Token otomatik yenileme
  - Token expiration handling
- [x] **CSRF token doğrulaması** ✅ (Antiforgery meta + header + client storage)
- [ ] **Input validation iyileştirmeleri**
  - XSS koruması
  - SQL Injection koruması (AdvancedSecurityService mevcut, kullanılmalı)
- [x] **Rate limiting endpoint'lerde uygulanmalı** ✅
  - `/api/v1/user/*` endpoint'leri için rate limiting (IP 1m/60, Client 1m/30)
  - `/api/v1/auth/*` endpoint'leri için rate limiting (IP 1m/20, Client 1m/10)
- [ ] **Sensitive data encryption**
  - Kredi kartı bilgileri
  - Kişisel bilgiler

### Logging ve Monitoring
- [x] **Güvenlik olayları için özel logging** ✅
  - Başarısız login denemeleri (FAILED_LOGIN)
  - Şüpheli aktiviteler / Token ihlalleri (401/403 + Bearer → TOKEN_VIOLATION)

---

## ⚡ Performans Optimizasyonları

### Caching
- [x] **Ürün listesi için cache stratejisi** ✅ (ProductService: popular/byId/similar)
- [x] **Merchant listesi için cache stratejisi** ✅ (MerchantService: list/byId/category/products)
- [ ] **Kullanıcı profil cache'i** (Redis ile)
- [ ] **Bildirimler için cache** (son 50 bildirim)

### Lazy Loading
- [ ] **Sayfa bazlı lazy loading**
- [ ] **Görsel lazy loading** (IntersectionObserver API)
- [ ] **Komponent lazy loading**

### Bundle Optimizasyonu
- [ ] **JavaScript bundle analizi**
- [ ] **CSS bundle optimizasyonu**
- [ ] **Tree shaking kontrolü**

### Database Query Optimizasyonu
- [ ] **N+1 query problemlerinin tespiti ve çözümü**
- [ ] **Bulk operation optimizasyonları**

---

## 🏗️ Mimari İyileştirmeler

### Dependency Injection
- [x] **Tüm servisler için interface kullanımı** ✅
- [x] **Service lifetime optimizasyonu** ✅
  - Scoped vs Singleton analizi
  - Memory leak kontrolü

### Error Handling
- [x] **GlobalErrorHandler iyileştirmesi** ✅
  - Daha detaylı error logging
  - User-friendly error messages
  - Error categorization
- [x] **API error response standardizasyonu** ✅
- [x] **Custom exception types** ✅

### Configuration Management
- [x] **Configuration validation** ✅
  - Startup'ta critical config kontrolü
  - Environment-specific config kontrolü


### Code Quality
- [x] **Code duplication analizi ve refactoring** ✅ (AdvancedSeoService, AdvancedPwaService)
 - [x] **Naming convention standartlaştırması** ✅
 - [x] **SOLID prensipleri review** ✅
 - [x] **Design pattern uygulamaları review** ✅

### Package Management
- [x] **Microsoft.AspNetCore.RateLimiting versiyonu güncellenmeli** ✅
  - Güncel: `9.0.0` (stable)

---

## 🧪 Test ve Kalite

### Unit Tests
- [ ] **UserService unit testleri**
- [ ] **AuthService unit testleri**
- [ ] **CartService unit testleri**
- [ ] **OrderService unit testleri**
- [ ] **ProductService unit testleri**
- [ ] **ApiClient unit testleri**
- [ ] **AdvancedCacheService unit testleri**
- [ ] **AdvancedSecurityService unit testleri**

### Integration Tests
- [ ] **API endpoint integration testleri**
- [ ] **Authentication flow integration testleri**
- [ ] **SignalR hub testleri**

### E2E Tests
- [ ] **Sipariş verme flow'u**
- [ ] **Kullanıcı kayıt/giriş flow'u**
- [ ] **Sepet yönetimi flow'u**

### Code Coverage
- [ ] **Code coverage hedefi: %80+**
- [ ] **Coverage report oluşturma**

---

## 📚 Dokümantasyon

### API Dokümantasyonu
- [ ] **API endpoint'leri için Swagger/OpenAPI dokümantasyonu**
- [ ] **Request/Response örnekleri**
- [ ] **Authentication dokümantasyonu**

### Code Dokümantasyonu
- [ ] **XML documentation comments eklenmeli**
- [ ] **Architecture decision records (ADR)**
- [ ] **Service katmanı dokümantasyonu**

### Kullanıcı Dokümantasyonu
- [ ] **Setup ve deployment guide**
- [ ] **Development environment setup**
- [ ] **Troubleshooting guide**

---

## 🐛 Kritik Hatalar ve İyileştirmeler

### ApiClient
- [x] **Console.WriteLine'lar kaldırılmalı** ✅
  - Serilog logger kullanılmalı
  - LogLevel kontrolü yapılmalı
- [x] **Authorization header yönetimi iyileştirilmeli** ✅
  - Her request'te token gönderilmeli
  - Token yönetimi merkezileştirilmeli

### Rate Limiting
- [x] **Rate limiting paket versiyonu güncellenmeli** ✅
  - RC versiyon yerine stable versiyon

### PWA Offline Support
- [ ] **Offline action queue persistence**
  - LocalStorage/IndexedDB kullanılmalı
  - Şu an sadece memory'de tutuluyor

### SignalR
- [ ] **SignalR reconnection stratejisi**
- [ ] **SignalR error handling**

---

## 📊 Öncelik Matrisi

### Yüksek Öncelik (Kritik)
1. ✅ API entegrasyonları (Security, Settings, Notifications)
2. ✅ Eksik servislerin oluşturulması
3. ✅ ApiClient debug loglarının kaldırılması
4. ✅ Rate limiting paket versiyonu güncellemesi

### Orta Öncelik (Önemli)
1. ⚠️ Interface'lerin eklenmesi
2. ⚠️ Modal bileşenlerinin oluşturulması
3. ⚠️ Test coverage
4. ⚠️ Error handling iyileştirmeleri

### Düşük Öncelik (İyileştirme)
1. 📝 Dokümantasyon
2. 📝 Code quality refactoring
3. 📝 Performance optimizasyonları

---

## 📝 Notlar

- Mock data kullanılan yerler gerçek API entegrasyonları ile değiştirilmeli
- Console.WriteLine yerine Serilog kullanılmalı
- Interface'ler SOLID prensipleri için kritik
- Test coverage artırılmalı, özellikle business logic için
- API token yönetimi merkezileştirilmeli
- Production'da debug logları kaldırılmalı veya kontrol edilmeli

---

**Son Güncelleme:** 2024-10-24
**Toplam İş Sayısı:** ~85 adet

