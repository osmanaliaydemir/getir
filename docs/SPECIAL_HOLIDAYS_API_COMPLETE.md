# ✅ Özel Tatil Günleri API - Tam Dokümantasyon

## 📊 Özet

**Tarih:** 8 Ekim 2025  
**Durum:** ✅ **TAMAMLANDI**  
**Süre:** ~1 saat  
**Dosya Sayısı:** 8 yeni dosya + 1 güncelleme

---

## 🎯 Tamamlanan Özellikler

### ✅ 1. Domain Katmanı
- **Entity:** `SpecialHoliday.cs`
  - Özel tatil günleri yönetimi
  - Resmi tatiller (Yılbaşı, Ramazan Bayramı, vb.)
  - Geçici kapanışlar (Tadilat, özel etkinlik, vb.)
  - Özel çalışma saatleri (Bayram günlerinde kısıtlı açılış)
  - Her yıl tekrar eden tatiller desteği

**Özellikler:**
```csharp
- Id: Guid
- MerchantId: Guid
- Title: string (örn: "Ramazan Bayramı")
- Description: string? (opsiyonel açıklama)
- StartDate: DateTime
- EndDate: DateTime
- IsClosed: bool (Kapalı mı yoksa özel saatler mi?)
- SpecialOpenTime: TimeSpan? (Özel açılış saati)
- SpecialCloseTime: TimeSpan? (Özel kapanış saati)
- IsRecurring: bool (Her yıl tekrar ediyor mu?)
- IsActive: bool
- CreatedAt: DateTime
- UpdatedAt: DateTime?
```

---

### ✅ 2. Application Katmanı

#### DTO'lar
**Dosya:** `src/Application/DTO/SpecialHolidayDto.cs`

**DTO'lar:**
- `SpecialHolidayResponse` - Özel tatil yanıt modeli
- `CreateSpecialHolidayRequest` - Yeni özel tatil oluşturma
- `UpdateSpecialHolidayRequest` - Mevcut özel tatili güncelleme
- `MerchantAvailabilityResponse` - Merchant'ın belirli tarihteki durumu

#### Validators
**Dosya:** `src/Application/Validators/SpecialHolidayValidator.cs`

**Validation Kuralları:**
- Başlık zorunlu (max 200 karakter)
- Açıklama opsiyonel (max 1000 karakter)
- Bitiş tarihi başlangıç tarihinden sonra olmalı
- Maksimum tatil süresi 365 gün
- Kapalı değilse özel saatler zorunlu
- Kapalıysa özel saatler null olmalı
- Özel kapanış saati açılış saatinden sonra olmalı

#### Service Layer
**Interface:** `src/Application/Services/SpecialHolidays/ISpecialHolidayService.cs`  
**Implementation:** `src/Application/Services/SpecialHolidays/SpecialHolidayService.cs`

**Metodlar:**
```csharp
// CRUD İşlemleri
+ CreateSpecialHolidayAsync()      // Yeni özel tatil oluştur
+ UpdateSpecialHolidayAsync()      // Mevcut tatili güncelle
+ DeleteSpecialHolidayAsync()      // Tatili sil
+ ToggleSpecialHolidayStatusAsync() // Aktif/pasif yap

// Sorgulama İşlemleri
+ GetAllSpecialHolidaysAsync()              // Tüm aktif tatilleri listele
+ GetSpecialHolidaysByMerchantAsync()       // Merchant'a ait tatiller
+ GetSpecialHolidaysByDateRangeAsync()      // Tarih aralığındaki tatiller
+ GetSpecialHolidayByIdAsync()              // ID'ye göre getir
+ GetUpcomingSpecialHolidaysAsync()         // Gelecek tatilleri getir

// İş Mantığı
+ CheckMerchantAvailabilityAsync()          // Belirli tarihte merchant durumu
```

**Özel Özellikler:**
- Merchant ownership kontrolü (sadece kendi merchant'ını yönetebilir)
- Çakışan tatil kontrolü (aynı tarih aralığında başka tatil olmamalı)
- Özel tatil varsa normal çalışma saatlerini override eder
- Recurring tatiller için yıllık tekrar desteği

---

### ✅ 3. WebApi Katmanı

**Controller:** `src/WebApi/Controllers/SpecialHolidayController.cs`

#### 📡 API Endpoints

| Method | Endpoint | Açıklama | Auth |
|--------|----------|----------|------|
| GET | `/api/v1/SpecialHoliday` | Tüm aktif özel tatilleri listele | ❌ |
| GET | `/api/v1/SpecialHoliday/{id}` | ID'ye göre özel tatil getir | ❌ |
| GET | `/api/v1/SpecialHoliday/merchant/{merchantId}` | Merchant'a ait tatiller | ❌ |
| GET | `/api/v1/SpecialHoliday/merchant/{merchantId}/date-range` | Tarih aralığındaki tatiller | ❌ |
| GET | `/api/v1/SpecialHoliday/merchant/{merchantId}/upcoming` | Gelecek tatiller | ❌ |
| GET | `/api/v1/SpecialHoliday/merchant/{merchantId}/availability` | Belirli tarihteki durum | ❌ |
| POST | `/api/v1/SpecialHoliday` | Yeni özel tatil oluştur | ✅ Admin, MerchantOwner |
| PUT | `/api/v1/SpecialHoliday/{id}` | Özel tatil güncelle | ✅ Admin, MerchantOwner |
| DELETE | `/api/v1/SpecialHoliday/{id}` | Özel tatil sil | ✅ Admin, MerchantOwner |
| PATCH | `/api/v1/SpecialHoliday/{id}/toggle-status` | Aktif/pasif yap | ✅ Admin, MerchantOwner |

---

### ✅ 4. Database Katmanı

**Migration:** `database/migrations/020-special-holidays-system.sql`

#### Tablo Yapısı
```sql
CREATE TABLE SpecialHolidays (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsClosed BIT NOT NULL DEFAULT 1,
    SpecialOpenTime TIME NULL,
    SpecialCloseTime TIME NULL,
    IsRecurring BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE
);
```

#### İndeksler
```sql
IX_SpecialHolidays_MerchantId      -- Merchant bazlı sorgular için
IX_SpecialHolidays_DateRange       -- Tarih aralığı sorguları için
IX_SpecialHolidays_IsActive        -- Aktif tatil filtreleme için
```

#### Constraints
- `CK_SpecialHolidays_DateRange`: Bitiş tarihi >= Başlangıç tarihi
- `CK_SpecialHolidays_SpecialTimes`: Kapalıysa özel saatler null, değilse dolu olmalı

#### Stored Procedure
```sql
sp_CheckMerchantAvailability(@MerchantId, @CheckDate)
```
Belirli bir tarihte merchant'ın durumunu kontrol eder (özel tatil veya normal çalışma saatleri)

#### Seed Data
- Yılbaşı tatili (her yıl tekrar eden)
- Ramazan Bayramı 2025 (tek seferlik)
- Cumhuriyet Bayramı (özel çalışma saatleri ile)

---

### ✅ 5. Dependency Injection

**Dosya:** `src/WebApi/Program.cs`

**Eklenen Kayıtlar:**
```csharp
// Using
using Getir.Application.Services.SpecialHolidays;

// Service Registration
builder.Services.AddScoped<ISpecialHolidayService, SpecialHolidayService>();
builder.Services.AddScoped<IEmailService, Getir.Infrastructure.Services.Notifications.EmailService>();

// Validator Registration
builder.Services.AddValidatorsFromAssemblyContaining<CreateSpecialHolidayRequestValidator>();
```

---

## 📋 Kullanım Örnekleri

### 1. Yeni Özel Tatil Oluşturma (Ramazan Bayramı - Kapalı)

**Request:**
```http
POST /api/v1/SpecialHoliday
Authorization: Bearer {token}
Content-Type: application/json

{
  "merchantId": "123e4567-e89b-12d3-a456-426614174000",
  "title": "Ramazan Bayramı 2025",
  "description": "İlk 2 gün kapalı",
  "startDate": "2025-03-30T00:00:00Z",
  "endDate": "2025-03-31T23:59:59Z",
  "isClosed": true,
  "specialOpenTime": null,
  "specialCloseTime": null,
  "isRecurring": false
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "456e7890-e89b-12d3-a456-426614174001",
    "merchantId": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Ramazan Bayramı 2025",
    "description": "İlk 2 gün kapalı",
    "startDate": "2025-03-30T00:00:00Z",
    "endDate": "2025-03-31T23:59:59Z",
    "isClosed": true,
    "specialOpenTime": null,
    "specialCloseTime": null,
    "isRecurring": false,
    "isActive": true,
    "createdAt": "2025-10-08T10:30:00Z",
    "updatedAt": null
  }
}
```

---

### 2. Özel Çalışma Saatleri (Cumhuriyet Bayramı)

**Request:**
```http
POST /api/v1/SpecialHoliday
Authorization: Bearer {token}
Content-Type: application/json

{
  "merchantId": "123e4567-e89b-12d3-a456-426614174000",
  "title": "Cumhuriyet Bayramı",
  "description": "29 Ekim - Kısıtlı çalışma saatleri",
  "startDate": "2025-10-29T00:00:00Z",
  "endDate": "2025-10-29T23:59:59Z",
  "isClosed": false,
  "specialOpenTime": "10:00:00",
  "specialCloseTime": "18:00:00",
  "isRecurring": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "789e0123-e89b-12d3-a456-426614174002",
    "merchantId": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Cumhuriyet Bayramı",
    "description": "29 Ekim - Kısıtlı çalışma saatleri",
    "startDate": "2025-10-29T00:00:00Z",
    "endDate": "2025-10-29T23:59:59Z",
    "isClosed": false,
    "specialOpenTime": "10:00:00",
    "specialCloseTime": "18:00:00",
    "isRecurring": true,
    "isActive": true,
    "createdAt": "2025-10-08T10:35:00Z",
    "updatedAt": null
  }
}
```

---

### 3. Merchant Durumu Kontrolü

**Request:**
```http
GET /api/v1/SpecialHoliday/merchant/{merchantId}/availability?checkDate=2025-03-30
```

**Response (Tatilde Kapalı):**
```json
{
  "success": true,
  "data": {
    "isOpen": false,
    "status": "Tatilde Kapalı",
    "specialHoliday": {
      "id": "456e7890-e89b-12d3-a456-426614174001",
      "title": "Ramazan Bayramı 2025",
      "startDate": "2025-03-30T00:00:00Z",
      "endDate": "2025-03-31T23:59:59Z",
      "isClosed": true
    },
    "message": "Ramazan Bayramı 2025 nedeniyle kapalıdır"
  }
}
```

**Response (Özel Çalışma Saatleri):**
```json
{
  "success": true,
  "data": {
    "isOpen": true,
    "status": "Özel Çalışma Saatleri",
    "specialHoliday": {
      "id": "789e0123-e89b-12d3-a456-426614174002",
      "title": "Cumhuriyet Bayramı",
      "startDate": "2025-10-29T00:00:00Z",
      "endDate": "2025-10-29T23:59:59Z",
      "isClosed": false,
      "specialOpenTime": "10:00:00",
      "specialCloseTime": "18:00:00"
    },
    "message": "Cumhuriyet Bayramı - Özel çalışma saatleri: 10:00 - 18:00"
  }
}
```

**Response (Normal Gün):**
```json
{
  "success": true,
  "data": {
    "isOpen": true,
    "status": "Açık",
    "specialHoliday": null,
    "message": "Normal çalışma saatleri: 09:00 - 22:00"
  }
}
```

---

### 4. Gelecek Tatilleri Listele

**Request:**
```http
GET /api/v1/SpecialHoliday/merchant/{merchantId}/upcoming
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "789e0123-e89b-12d3-a456-426614174002",
      "title": "Cumhuriyet Bayramı",
      "startDate": "2025-10-29T00:00:00Z",
      "endDate": "2025-10-29T23:59:59Z",
      "isClosed": false,
      "specialOpenTime": "10:00:00",
      "specialCloseTime": "18:00:00",
      "isRecurring": true
    },
    {
      "id": "abc12345-e89b-12d3-a456-426614174003",
      "title": "Yılbaşı Tatili",
      "startDate": "2026-01-01T00:00:00Z",
      "endDate": "2026-01-01T23:59:59Z",
      "isClosed": true,
      "specialOpenTime": null,
      "specialCloseTime": null,
      "isRecurring": true
    }
  ]
}
```

---

### 5. Tarih Aralığında Tatilleri Sorgula

**Request:**
```http
GET /api/v1/SpecialHoliday/merchant/{merchantId}/date-range?startDate=2025-03-01&endDate=2025-04-30
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "456e7890-e89b-12d3-a456-426614174001",
      "title": "Ramazan Bayramı 2025",
      "startDate": "2025-03-30T00:00:00Z",
      "endDate": "2025-03-31T23:59:59Z",
      "isClosed": true,
      "isRecurring": false
    }
  ]
}
```

---

### 6. Özel Tatili Güncelle

**Request:**
```http
PUT /api/v1/SpecialHoliday/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Ramazan Bayramı 2025",
  "description": "İlk 3 gün kapalı (güncellenmiş)",
  "startDate": "2025-03-30T00:00:00Z",
  "endDate": "2025-04-01T23:59:59Z",
  "isClosed": true,
  "specialOpenTime": null,
  "specialCloseTime": null,
  "isRecurring": false,
  "isActive": true
}
```

---

### 7. Özel Tatili Aktif/Pasif Yap

**Request:**
```http
PATCH /api/v1/SpecialHoliday/{id}/toggle-status
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Özel tatil durumu başarıyla değiştirildi"
}
```

---

### 8. Özel Tatili Sil

**Request:**
```http
DELETE /api/v1/SpecialHoliday/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Özel tatil başarıyla silindi"
}
```

---

## 🔒 Güvenlik Özellikleri

1. **Authorization:** Admin ve MerchantOwner rolleri için yetkilendirilmiş
2. **Merchant Ownership:** Sadece kendi merchant'ını yönetebilir
3. **Validation:** FluentValidation ile kapsamlı veri doğrulama
4. **Çakışma Kontrolü:** Aynı tarih aralığında birden fazla tatil engellenmiş
5. **Audit Logging:** Tüm CRUD işlemleri loglanıyor

---

## 📊 Veritabanı İlişkileri

```
Merchants (1) -----> (N) SpecialHolidays
              -----> (N) WorkingHours
```

**Cascade Delete:** Merchant silindiğinde özel tatiller de silinir

---

## 🎯 İş Mantığı

### Merchant Durumu Kontrolü Sırası:
1. **Özel Tatil Kontrolü:** Belirtilen tarihte aktif bir özel tatil var mı?
   - Varsa ve `IsClosed = true` → **Kapalı**
   - Varsa ve `IsClosed = false` → **Özel Çalışma Saatleri**
2. **Normal Çalışma Saatleri:** Özel tatil yoksa WorkingHours'a bak
   - `IsClosed = true` → **Kapalı**
   - `IsClosed = false` → **Normal Açılış Saatleri**

### Recurring Tatiller:
- `IsRecurring = true` olan tatiller her yıl otomatik tekrar eder
- Yeni yıl geldiğinde frontend/backend tarafından yıl güncellenir

---

## ✅ Tamamlanan Dosyalar

| # | Dosya | Durum |
|---|-------|-------|
| 1 | `src/Domain/Entities/SpecialHoliday.cs` | ✅ |
| 2 | `src/Application/DTO/SpecialHolidayDto.cs` | ✅ |
| 3 | `src/Application/Validators/SpecialHolidayValidator.cs` | ✅ |
| 4 | `src/Application/Services/SpecialHolidays/ISpecialHolidayService.cs` | ✅ |
| 5 | `src/Application/Services/SpecialHolidays/SpecialHolidayService.cs` | ✅ |
| 6 | `src/WebApi/Controllers/SpecialHolidayController.cs` | ✅ |
| 7 | `database/migrations/020-special-holidays-system.sql` | ✅ |
| 8 | `src/WebApi/Program.cs` (güncelleme) | ✅ |

---

## 🚀 Sonraki Adımlar (Opsiyonel)

### Mobile App (Flutter)
- [ ] SpecialHoliday entity oluştur
- [ ] SpecialHoliday data source ve repository
- [ ] SpecialHoliday service ve BLoC
- [ ] Merchant Owner Dashboard - Tatil yönetimi ekranı
- [ ] Kullanıcılar için tatil bildirim widget'ı

### Backend İyileştirmeler
- [ ] Email bildirimleri (Tatil yaklaşırken merchant'a hatırlatma)
- [ ] Push notification (Müşterilere tatil bildirimi)
- [ ] Bulk import (CSV'den tatil yükleme)
- [ ] Recurring tatillerin otomatik yıl güncelleme job'u

---

## 📝 Notlar

- ✅ Tüm endpoint'ler RESTful standartlara uygun
- ✅ SOLID prensipleri uygulandı
- ✅ Clean Architecture yapısına uygun
- ✅ Comprehensive validation
- ✅ Audit logging
- ✅ Performance optimizasyonları (indexing)
- ✅ Linter hataları yok

---

**Geliştiren:** AI Assistant  
**Tarih:** 8 Ekim 2025  
**Durum:** Production Ready ✅

