# 🚨 Getir Mobile - Kritik Düzeltmeler TODO

## ✅ **TAMAMLANAN (Bu Session)**

### 1. **Backend Sync - merchantId Eklendi**
```dart
// ✅ auth_models.dart
class AuthResponse {
  final String? merchantId; // Backend ile sync
}
```

### 2. **Token Refresh Interceptor - Eklendi**
```dart
// ✅ token_refresh_interceptor.dart (YENİ)
// - 401'de otomatik token refresh
// - Request queue management
// - Seamless UX
```

### 3. **Güvenli Encryption Service - Oluşturuldu**
```dart
// ✅ secure_encryption_service.dart (YENİ)
// - AES-256-GCM encryption
// - Secure key storage
// - Key rotation support
// - Production-ready
```

### 4. **Pubspec.yaml - Encrypt Package Eklendi**
```yaml
// ✅ dependencies:
encrypt: ^5.0.3  # AES-256 encryption
```

### 5. **Localization - Generate Edildi**
```bash
# ✅ flutter gen-l10n çalıştırıldı
# app_localizations.g.dart oluşturuldu
```

### 6. **Analiz Raporu - Oluşturuldu**
```
# ✅ MOBILE_APP_ANALYSIS_REPORT.md
# - 25 bölüm kapsamlı analiz
# - Tüm katmanlar incelendi
# - Sorunlar öncelik sırasına göre listelendi
```

---

## 🔴 **YAPILMASI GEREKENLER (Sırayla)**

### **A. HEMEN (Bu Hafta - 1 Gün)**

#### **1. .env Dosyalarını Manuel Oluştur** ⏱️ 10 dakika
```bash
# getir_mobile/.env.dev
API_BASE_URL=http://ajilgo.runasp.net
API_TIMEOUT=30000
API_KEY=dev_api_key_12345
ENCRYPTION_KEY=dev_32_char_encryption_key_here!
ENABLE_SSL_PINNING=false
DEBUG_MODE=true
GOOGLE_MAPS_API_KEY=your_key_here

# getir_mobile/.env.staging (aynı format)
# getir_mobile/.env.prod (production values)
```

#### **2. Flutter Packages Yükle** ⏱️ 5 dakika
```bash
cd getir_mobile
flutter pub get
flutter pub upgrade
```

#### **3. Eski EncryptionService'i Değiştir** ⏱️ 30 dakika
```dart
// core/di/injection.dart
// Eski: getIt.registerLazySingleton(() => EncryptionService());
// Yeni: getIt.registerLazySingleton(() => SecureEncryptionService());

// Tüm import'ları güncelle:
// import 'encryption_service.dart' → 'secure_encryption_service.dart'
```

#### **4. Test Et** ⏱️ 1 saat
```bash
flutter test
flutter analyze
flutter run
```

---

### **B. YÜKSEK ÖNCELİK (Bu Hafta - 2-3 Gün)**

#### **5. Firebase Configuration** ⏱️ 2 saat
```
1. Firebase Console → Proje oluştur
2. google-services.json indir → android/app/
3. GoogleService-Info.plist indir → ios/Runner/
4. firebase_options.dart generate et:
   flutterfire configure
```

#### **6. SSL Pinning Implementation** ⏱️ 4 saat
```dart
// ssl_pinning_interceptor.dart güncelle
// Certificate fingerprints ekle
// Backend SSL certificate'i al
// SHA-256 fingerprint oluştur
```

#### **7. Push Notification Setup** ⏱️ 6 saat
```dart
// notification_service.dart genişlet:
// - FCM token registration
// - Local notification handling
// - Background message handler
// - Notification click action
```

#### **8. Backend API Test** ⏱️ 4 saat
```
- Tüm endpoint'leri Postman'den test et
- AuthResponse merchantId geldiğini doğrula
- SignalR hub'larına bağlan
- Error response format'ı kontrol et
```

---

### **C. ORTA ÖNCELİK (Sonraki Hafta - 1 Hafta)**

#### **9. Test Suite Update** ⏱️ 8 saat
```dart
// UseCase test'lerini sil (artık yok)
// Service test'lerini yaz/güncelle
// Test coverage raporu:
flutter test --coverage
genhtml coverage/lcov.info -o coverage/html
```

#### **10. Pagination Implementation** ⏱️ 6 saat
```dart
// PaginatedListView widget
// Infinite scroll için ScrollController
// Page loading state management
// "Load More" button
```

#### **11. Deep Link Configuration** ⏱️ 2 saat
```xml
<!-- android/app/src/main/AndroidManifest.xml -->
<intent-filter android:autoVerify="true">
  <action android:name="android.intent.action.VIEW" />
  <category android:name="android.intent.category.DEFAULT" />
  <category android:name="android.intent.category.BROWSABLE" />
  <data android:scheme="getir" android:host="app" />
</intent-filter>

<!-- ios/Runner/Info.plist -->
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLSchemes</key>
    <array><string>getir</string></array>
  </dict>
</array>
```

#### **12. Platform-Specific Configuration** ⏱️ 4 saat
```
Android:
  - ProGuard rules (proguard-rules.pro)
  - Release signing config (keystore)
  - Permissions manifest

iOS:
  - Info.plist permissions
  - App Store metadata
  - CocoaPods update
```

---

### **D. İYİLEŞTİRMELER (İsteğe Bağlı - 2 Hafta)**

#### **13. ApiClient Service Wrapper** ⏱️ 4 saat
```dart
// Dio'yu wrap eden centralized service
class ApiClient {
  Future<T> get<T>(String path);
  Future<T> post<T>(String path, dynamic body);
  Future<T> put<T>(String path, dynamic body);
  Future<T> delete<T>(String path);
}
```

#### **14. Offline Mode Enhancement** ⏱️ 8 saat
```dart
// - Offline queue
// - Conflict resolution
// - Background sync
// - Cache strategy
```

#### **15. Performance Profiling** ⏱️ 4 saat
```bash
# Flutter DevTools profiling
# Memory leak detection
# Rendering performance
# Build size optimization
```

#### **16. Code Generation Migration** ⏱️ 6 saat
```yaml
# injectable + build_runner
# Automatic DI registration
# JSON serialization (json_serializable)
# Freezed (immutable models)
```

---

## 📊 **İLERLEME TAKIP**

### Toplam Tahmini Süre:
```
🔴 Kritik:           6 saat   (Hemen)
🟡 Yüksek Öncelik:  16 saat   (Bu hafta)
🟢 Orta Öncelik:    20 saat   (Sonraki hafta)
💡 İyileştirmeler:  22 saat   (İsteğe bağlı)

TOPLAM: ~64 saat (~8 gün full-time)
```

### Checklist:
```
Kritik (1 gün):
  [✅] Localization generate
  [✅] Backend sync (merchantId)
  [✅] Token refresh interceptor
  [✅] Secure encryption service
  [✅] Analiz raporu
  [ ] .env dosyaları
  [ ] Flutter pub get
  [ ] Test run

Yüksek Öncelik (3 gün):
  [ ] Firebase config
  [ ] SSL Pinning
  [ ] Push notifications
  [ ] Backend API test

Orta Öncelik (5 gün):
  [ ] Test suite update
  [ ] Pagination
  [ ] Deep links
  [ ] Platform config
```

---

## 🎯 **SONRAKİ ADIM**

**Sıradaki işlemler:**

1. **flutter pub get** → encrypt package indir
2. **.env dosyaları oluştur** → Manuel olarak
3. **EncryptionService migration** → Eski → Yeni
4. **Test Et** → flutter run

**Hazır olduğunda devam et!** 🚀

