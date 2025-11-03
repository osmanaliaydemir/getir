# 🔧 Environment Setup Guide

**Tarih:** 2 Kasım 2025  
**Konu:** Getir Mobile - Environment Konfigürasyonu

---

## 📋 İçindekiler

- [Genel Bakış](#genel-bakış)
- [Environment Dosyaları](#environment-dosyaları)
- [Hızlı Başlangıç](#hızlı-başlangıç)
- [Detaylı Kurulum](#detaylı-kurulum)
- [Environment değiştirme](#environment-değiştirme)
- [Güvenlik Notları](#güvenlik-notları)

---

## 🎯 Genel Bakış

Getir Mobile **3 farklı environment** ile çalışır:

| Environment | Dosya | Açıklama |
|------------|-------|----------|
| **Development** | `.env.dev` | Geliştirme ortamı |
| **Staging** | `.env.staging` | Test ortamı |
| **Production** | `.env.prod` | Canlı ortam |

Her environment kendi konfigürasyonunu `.env` dosyasından okur.

---

## 📁 Environment Dosyaları

### Dosya Yapısı

```
getir_mobile/
├── .env.dev          # Development konfigürasyonu
├── .env.staging      # Staging konfigürasyonu
├── .env.prod         # Production konfigürasyonu
├── .gitignore        # .env dosyalarını ignore eder
└── lib/
    ├── main.dart     # Default (dev)
    ├── main_dev.dart
    ├── main_staging.dart
    └── main_prod.dart
```

**⚠️ ÖNEMLİ:** `.env` dosyaları `.gitignore`'da! Bunlar **asla** Git'e commit edilmemeli.

---

## 🚀 Hızlı Başlangıç

### 1. Environment Dosyası Oluştur

Kök dizinde `.env.dev` dosyası oluştur:

```bash
cd getir_mobile
touch .env.dev  # Linux/Mac
# veya
type nul > .env.dev  # Windows
```

### 2. Temel Konfigürasyon

`.env.dev` dosyasına şunları ekle:

```bash
# API Configuration
API_BASE_URL=https://ajilgo.runasp.net
SIGNALR_HUB_URL=https://ajilgo.runasp.net/hubs
API_TIMEOUT=30000
API_KEY=your_dev_api_key_here

# Security
ENCRYPTION_KEY=dev_encryption_key_32chars_getir
ENABLE_SSL_PINNING=false

# Features
DEBUG_MODE=true
ENABLE_LOGGING=true
ENVIRONMENT=development

# Google Maps
GOOGLE_MAPS_API_KEY=your_google_maps_key
```

### 3. Uygulamayı Çalıştır

```bash
# Development
flutter run

# veya açıkça belirt
flutter run -t lib/main_dev.dart
```

---

## 📝 Detaylı Kurulum

### Development Environment

#### `.env.dev` Örneği

```bash
# ========================================
# API Configuration
# ========================================
API_BASE_URL=https://ajilgo.runasp.net
SIGNALR_HUB_URL=https://ajilgo.runasp.net/hubs
API_TIMEOUT=30000
API_KEY=dev_api_key_getir_mobile_2025

# ========================================
# Security
# ========================================
# 32 karakter olmalı!
ENCRYPTION_KEY=dev_encryption_key_32chars_getir
ENABLE_SSL_PINNING=false

# ========================================
# Features
# ========================================
DEBUG_MODE=true
ENABLE_LOGGING=true
ENVIRONMENT=development

# ========================================
# Google Services
# ========================================
GOOGLE_MAPS_API_KEY=your_dev_google_maps_api_key

# ========================================
# Firebase (Optional)
# ========================================
FIREBASE_PROJECT_ID=your_firebase_project
ENABLE_FIREBASE_ANALYTICS=true
ENABLE_CRASHLYTICS=true
ENABLE_PERFORMANCE_MONITORING=true
```

### Staging Environment

#### `.env.staging` Örneği

```bash
# ========================================
# API Configuration
# ========================================
API_BASE_URL=https://ajilgo.runasp.net
SIGNALR_HUB_URL=https://ajilgo.runasp.net/hubs
API_TIMEOUT=30000
API_KEY=staging_api_key_getir_mobile_2025

# ========================================
# Security
# ========================================
ENCRYPTION_KEY=staging_encryption_key_32chars_g
ENABLE_SSL_PINNING=false  # Test için kapatılabilir

# ========================================
# Features
# ========================================
DEBUG_MODE=true
ENABLE_LOGGING=true
ENVIRONMENT=staging

# ========================================
# Google Services
# ========================================
GOOGLE_MAPS_API_KEY=your_staging_google_maps_api_key
```

### Production Environment

#### `.env.prod` Örneği

```bash
# ========================================
# API Configuration
# ========================================
API_BASE_URL=https://api.getir.com
SIGNALR_HUB_URL=https://api.getir.com/hubs
API_TIMEOUT=15000
API_KEY=prod_secure_api_key_REPLACE_WITH_REAL

# ========================================
# Security
# ========================================
ENCRYPTION_KEY=prod_encryption_key_32_REPLACE!!
ENABLE_SSL_PINNING=true  # MUTLAKA açık olmalı!

# ========================================
# Features
# ========================================
DEBUG_MODE=false
ENABLE_LOGGING=false
ENVIRONMENT=production

# ========================================
# Google Services
# ========================================
GOOGLE_MAPS_API_KEY=your_prod_google_maps_api_key

# ========================================
# Firebase
# ========================================
FIREBASE_PROJECT_ID=your_firebase_project
ENABLE_FIREBASE_ANALYTICS=true
ENABLE_CRASHLYTICS=true
ENABLE_PERFORMANCE_MONITORING=true
```

---

## 🔄 Environment Değiştirme

### Yöntem 1: Main Dosyaları ile

```bash
# Development
flutter run -t lib/main_dev.dart

# Staging
flutter run -t lib/main_staging.dart

# Production (Release mode)
flutter run -t lib/main_prod.dart --release
```

### Yöntem 2: Build Argument ile

Android flavor ile:

```bash
# Development
flutter build apk --debug --target lib/main_dev.dart

# Staging
flutter build apk --release --target lib/main_staging.dart

# Production
flutter build apk --release --target lib/main_prod.dart
```

### Yöntem 3: Kod İçinden

```dart
import 'core/config/environment_config.dart';

// Environment kontrolü
if (EnvironmentConfig.isDevelopment) {
  // Dev-only code
}

if (EnvironmentConfig.isProduction) {
  // Prod-only code
}

// Current environment
print(EnvironmentConfig.currentEnvironment); // "dev", "staging", "prod"
```

---

## 🔐 Güvenlik Notları

### ❌ YAPMA:

```
❌ .env dosyalarını Git'e commit etme
❌ API key'leri kod içine hard-code etme
❌ ENCRYPTION_KEY'i değiştirme (kullanıcı verileri kaybolur!)
❌ Production'da DEBUG_MODE=true yapma
❌ Production'da ENABLE_SSL_PINNING=false bırakma
```

### ✅ YAP:

```
✅ .env dosyalarını .gitignore'da tut
✅ Her environment için farklı key'ler kullan
✅ Production key'leri güvenli bir yerde sakla
✅ ENCRYPTION_KEY'i 32 karakter yap
✅ SSL Pinning'i production'da enable et
✅ Environment secrets'ları CI/CD'de manage et
✅ Key rotation policy belirle
```

---

## 🔑 API Key Alımı

### Google Maps API Key

1. Google Cloud Console'a git: https://console.cloud.google.com/
2. Proje oluştur veya seç
3. Maps API'yi enable et
4. Credentials → Create API Key
5. Key'i restrict et (Android/iOS apps)
6. Key'i `.env` dosyasına ekle

### Backend API Key

Backend ekibinden alınmalı:
- Development: Test API key
- Staging: Staging API key
- Production: Production API key

### Encryption Key

**⚠️ KRİTİK:** Bu key değiştirilirse tüm şifreli veriler kaybolur!

```bash
# 32 karakter random string oluştur
openssl rand -hex 16  # Linux/Mac
# veya online generator kullan
```

---

## 🧪 Test Etme

### Environment Kontrolü

```dart
// main.dart veya app başlangıcında
void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Environment'ı print et (debug mode'da)
  debugPrint('Current Environment: ${EnvironmentConfig.currentEnvironment}');
  debugPrint('API Base URL: ${EnvironmentConfig.apiBaseUrl}');
  debugPrint('SSL Pinning: ${EnvironmentConfig.enableSslPinning}');
  
  // ...
}
```

### Runtime Check

```dart
import 'package:flutter/foundation.dart';

if (kDebugMode) {
  // Dev-only code
}

if (kReleaseMode) {
  // Prod-only code
}
```

---

## 🐛 Troubleshooting

### Problem: `.env` dosyası bulunamıyor

**Çözüm:**
```bash
# Dosyanın varlığını kontrol et
ls -la .env.dev  # Linux/Mac
dir .env.dev     # Windows

# Flutter'ı temizle
flutter clean
flutter pub get
```

### Problem: Environment değişmiyor

**Çözüm:**
```bash
# Hot restart yap (hot reload değil!)
# VS Code: Ctrl+Shift+F5
# Terminal: 'R' tuşuna bas

# Veya uygulamayı kapatıp yeniden başlat
```

### Problem: API çağrıları başarısız

**Çözüm:**
```bash
# API key'i kontrol et
echo $API_KEY

# .env dosyasını yükle
flutter pub run dotenv:load

# Logs'u kontrol et
flutter logs
```

---

## 📊 Environment Farkları

| Özellik | Development | Staging | Production |
|---------|-------------|---------|------------|
| **Debug Mode** | ✅ True | ✅ True | ❌ False |
| **Logging** | ✅ Verbose | ✅ Moderate | ❌ Minimal |
| **SSL Pinning** | ❌ Disabled | ⚠️ Optional | ✅ Enabled |
| **API URL** | Dev Server | Test Server | Live Server |
| **Hot Reload** | ✅ Enabled | ✅ Enabled | ❌ Disabled |
| **Analytics** | ⚠️ Optional | ✅ Enabled | ✅ Enabled |
| **Crashlytics** | ❌ Disabled | ✅ Enabled | ✅ Enabled |

---

## 🎓 Best Practices

1. **Environment Separation**
   - Her environment için ayrı `.env` dosyası
   - Farklı API endpoint'leri
   - Farklı encryption key'leri

2. **Security First**
   - Production'da SSL pinning zorunlu
   - Debug mode production'da kapalı
   - Logging production'da minimal

3. **Developer Experience**
   - Dev: Maximum logging
   - Dev: SSL pinning kapalı (kolay test)
   - Dev: Mock data support

4. **CI/CD Integration**
   - Environment secrets CI/CD'de sakla
   - Automatic environment switch
   - Environment validation

---

## 📚 Referanslar

- [Flutter DotEnv Package](https://pub.dev/packages/flutter_dotenv)
- [Environment Variables Best Practices](https://12factor.net/config)
- [SSL Pinning Guide](../SECURITY_SETUP_GUIDE.md)
- [Firebase Setup Guide](./FIREBASE_SETUP.md)

---

**Hazırlayan:** Senior DevOps Engineer  
**Tarih:** 2 Kasım 2025  
**Versiyon:** 1.0

