# 🔌 API Connection Checker - Backend ↔ Flutter

**Date:** 8 Ekim 2025  
**Backend URL:** http://ajilgo.runasp.net  
**Status:** ⚠️ **CHECKING CONNECTION**

---

## 🎯 Backend API Endpoints (.NET)

### AuthController
```
Route: api/v1/Auth

✅ POST /api/v1/Auth/register
✅ POST /api/v1/Auth/login
✅ POST /api/v1/Auth/refresh
✅ POST /api/v1/Auth/logout
```

**Important:** Controller route = `/api/v1/[controller]` → `/api/v1/Auth` (büyük A)

---

## 📱 Flutter API Calls

### AuthDataSourceImpl
```dart
// Current calls:
POST /api/v1/auth/login      ← Küçük 'a'
POST /api/v1/auth/register   ← Küçük 'a'
POST /api/v1/auth/refresh    ← Küçük 'a'
POST /api/v1/auth/logout     ← Küçük 'a'
```

---

## 🚨 SORUN BULUNDU!

```
Backend:  /api/v1/Auth  (büyük A)
Flutter:  /api/v1/auth  (küçük a)

❌ EŞLEŞM

İYOR!
```

---

## 🔧 Çözüm

### Seçenek 1: Flutter'ı Düzelt (Önerilen)
```dart
// Tüm datasource'larda:
'/api/v1/auth/login'   → '/api/v1/Auth/login'
'/api/v1/auth/register' → '/api/v1/Auth/register'
```

### Seçenek 2: Backend'i Düzelt
```csharp
[Route("api/v1/[controller]")]  // AuthController → /api/v1/Auth
// Ya da:
[Route("api/v1/auth")]           // Küçük harf zorla
```

---

**Şimdi düzeltiyorum...**
