# 🧪 Backend API Test Rehberi - Kategori Filtresi

## ✅ Tamamlanan Değişiklikler

### 1. DTO Güncellemeleri
**Dosya**: `src/Application/DTO/GeoLocationDtos.cs`
- ✅ `NearbyMerchantResponse` record'una `CategoryType` parametresi eklendi

### 2. Interface Güncellemeleri  
**Dosya**: `src/Application/Services/GeoLocation/IGeoLocationService.cs`
- ✅ `GetNearbyMerchantsByCategoryAsync` method tanımı eklendi

### 3. Service Implementation
**Dosya**: `src/Application/Services/GeoLocation/GeoLocationService.cs`
- ✅ `GetNearbyMerchantsByCategoryAsync` method implementation'ı eklendi
- ✅ Validasyon logic'i (latitude, longitude, radius kontrolü)
- ✅ Kategori bazlı filtreleme
- ✅ Delivery zone kontrolü
- ✅ Mesafe hesaplama ve sıralama
- ✅ Logging ve error handling

### 4. Controller Güncellemeleri
**Dosya**: `src/WebApi/Controllers/GeoLocationController.cs`
- ✅ Mevcut `GetNearbyMerchants` endpoint'ine opsiyonel `categoryType` parametresi eklendi
- ✅ Backward compatible (eski API çağrıları hala çalışır)

---

## 🚀 Test Adımları

### Ön Hazırlık
1. **Projeyi Build Et**:
   ```bash
   cd c:\Users\osmanali.aydemir\Desktop\projects\getir\src\WebApi
   dotnet build
   ```

2. **Projeyi Çalıştır**:
   ```bash
   dotnet run
   ```

3. **Swagger UI'ı Aç**:
   ```
   https://localhost:5001/swagger
   VEYA
   http://localhost:5000/swagger
   ```

---

## 📋 Test Senaryoları

### Test 1: Kategori Filtresi Olmadan (Backward Compatibility)
**Endpoint**: `GET /api/v1/geo/merchants/nearby`

**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&radius=5
```

**Beklenen Sonuç**:
- ✅ Status Code: 200 OK
- ✅ Tüm kategorilerdeki merchantlar dönmeli
- ✅ Response'da `categoryType` field'ı olmalı

**Swagger Test**:
1. "GeoLocation" controller'ı seç
2. "GET /api/v1/geo/merchants/nearby" endpoint'ini seç
3. Parametreleri gir:
   - latitude: `41.0082`
   - longitude: `28.9784`
   - radius: `5`
   - categoryType: **BOŞ BIRAK**
4. "Try it out" ve "Execute" butonlarına tıkla

---

### Test 2: Market Kategorisi (ServiceCategoryType = 2)
**Endpoint**: `GET /api/v1/geo/merchants/nearby`

**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=2&radius=5
```

**Beklenen Sonuç**:
- ✅ Status Code: 200 OK
- ✅ SADECE Market kategorisindeki merchantlar dönmeli
- ✅ Her merchant'ın `categoryType` field'ı `2` (Market) olmalı
- ✅ Distance hesaplanmış olmalı
- ✅ Merchantlar mesafeye göre sıralı olmalı

**Swagger Test**:
1. "GET /api/v1/geo/merchants/nearby" endpoint'ini seç
2. Parametreleri gir:
   - latitude: `41.0082`
   - longitude: `28.9784`
   - **categoryType: `2`** (Market)
   - radius: `5`
3. "Execute"

**Response Örneği**:
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Migros Express",
      "description": "Hızlı market teslimatı",
      "address": "Kadıköy, İstanbul",
      "distanceKm": 1.25,
      "deliveryFee": 9.99,
      "estimatedDeliveryTimeMinutes": 15,
      "rating": 4.5,
      "totalReviews": 250,
      "isOpen": true,
      "logoUrl": "https://example.com/logo.png",
      "categoryType": 2
    }
  ],
  "success": true,
  "message": null,
  "errorCode": null
}
```

---

### Test 3: Restaurant Kategorisi (ServiceCategoryType = 1)
**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=1&radius=5
```

**Beklenen Sonuç**:
- ✅ SADECE Restaurant kategorisindeki merchantlar
- ✅ categoryType = 1

---

### Test 4: Pharmacy Kategorisi (ServiceCategoryType = 3)
**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=3&radius=5
```

**Beklenen Sonuç**:
- ✅ SADECE Pharmacy kategorisindeki merchantlar
- ✅ categoryType = 3

---

### Test 5: Geçersiz Latitude (Error Handling)
**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=91&longitude=28.9784&categoryType=2&radius=5
```

**Beklenen Sonuç**:
- ✅ Status Code: 400 Bad Request
- ✅ Error message: "Invalid latitude. Must be between -90 and 90"
- ✅ errorCode: "INVALID_LATITUDE"

---

### Test 6: Geçersiz Radius (Error Handling)
**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=2&radius=100
```

**Beklenen Sonuç**:
- ✅ Status Code: 400 Bad Request
- ✅ Error message: "Radius must be between 0 and 50 km"
- ✅ errorCode: "INVALID_RADIUS"

---

### Test 7: Geçersiz Kategori (Error Handling)
**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=999&radius=5
```

**Beklenen Sonuç**:
- ✅ Status Code: 400 Bad Request
- ✅ Enum validation error

---

### Test 8: Merchant Bulunamadı (Empty Result)
**Request**:
```
GET /api/v1/geo/merchants/nearby?latitude=0&longitude=0&categoryType=2&radius=1
```

**Beklenen Sonuç**:
- ✅ Status Code: 200 OK
- ✅ data: [] (boş array)
- ✅ success: true

---

## 📊 ServiceCategoryType Enum Değerleri

| Kategori | Value | Açıklama |
|----------|-------|----------|
| Restaurant | 1 | Restoran - Yemek siparişi |
| Market | 2 | Market - Gıda ve temizlik |
| Pharmacy | 3 | Eczane - İlaç ve sağlık |
| Water | 4 | Su - Su teslimatı |
| Cafe | 5 | Kafe - Kahve ve atıştırmalık |
| Bakery | 6 | Pastane - Tatlı ve hamur işi |
| Other | 99 | Diğer - Diğer hizmetler |

---

## 🔍 Postman Test Collection

### Collection Import
1. Postman'ı aç
2. Import → Raw Text
3. Aşağıdaki JSON'u yapıştır:

```json
{
  "info": {
    "name": "Getir API - Category Filter Tests",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get Nearby Merchants - All Categories",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&radius=5",
          "host": ["{{baseUrl}}"],
          "path": ["api", "v1", "geo", "merchants", "nearby"],
          "query": [
            { "key": "latitude", "value": "41.0082" },
            { "key": "longitude", "value": "28.9784" },
            { "key": "radius", "value": "5" }
          ]
        }
      }
    },
    {
      "name": "Get Nearby Merchants - Market Only",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=2&radius=5",
          "host": ["{{baseUrl}}"],
          "path": ["api", "v1", "geo", "merchants", "nearby"],
          "query": [
            { "key": "latitude", "value": "41.0082" },
            { "key": "longitude", "value": "28.9784" },
            { "key": "categoryType", "value": "2" },
            { "key": "radius", "value": "5" }
          ]
        }
      }
    },
    {
      "name": "Get Nearby Merchants - Restaurant Only",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=1&radius=5",
          "host": ["{{baseUrl}}"],
          "path": ["api", "v1", "geo", "merchants", "nearby"],
          "query": [
            { "key": "latitude", "value": "41.0082" },
            { "key": "longitude", "value": "28.9784" },
            { "key": "categoryType", "value": "1" },
            { "key": "radius", "value": "5" }
          ]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://localhost:5001",
      "type": "string"
    }
  ]
}
```

---

## ✅ Checklist

### Build & Run
- [ ] Proje başarıyla build edildi
- [ ] API çalışıyor
- [ ] Swagger UI açılıyor

### Functionality Tests
- [ ] Test 1: Tüm kategoriler (backward compatibility)
- [ ] Test 2: Market kategorisi filtresi
- [ ] Test 3: Restaurant kategorisi filtresi
- [ ] Test 4: Pharmacy kategorisi filtresi
- [ ] Test 5: Geçersiz latitude error handling
- [ ] Test 6: Geçersiz radius error handling
- [ ] Test 7: Geçersiz kategori error handling
- [ ] Test 8: Boş sonuç senaryosu

### Response Validation
- [ ] CategoryType field'ı response'da mevcut
- [ ] Filtreleme doğru çalışıyor (sadece seçilen kategori dönüyor)
- [ ] Distance doğru hesaplanmış
- [ ] Merchantlar mesafeye göre sıralı
- [ ] Error mesajları doğru ve açıklayıcı

### Documentation
- [ ] Swagger documentation güncel
- [ ] API endpoint açıklamaları net
- [ ] Enum değerleri dokümante edilmiş

---

## 🐛 Olası Sorunlar ve Çözümler

### Sorun 1: Build Hatası
**Hata**: `ServiceCategoryType not found`
**Çözüm**: 
```bash
dotnet clean
dotnet restore
dotnet build
```

### Sorun 2: Merchant Bulunamıyor
**Nedeni**: Database'de test verisi yok
**Çözüm**: 
- Seed data'yı çalıştır
- SQL'de merchant tablosunu kontrol et
- ServiceCategoryType değerlerinin doğru atandığından emin ol

### Sorun 3: CategoryType null Dönüyor
**Nedeni**: Merchant entity'sinde ServiceCategoryType set edilmemiş
**Çözüm**: 
- Database'deki merchant kayıtlarını kontrol et
- ServiceCategoryType kolonunu güncelle

---

## 📝 Test Sonuçları (Manuel Doldur)

| Test No | Senaryo | Status | Not |
|---------|---------|--------|-----|
| 1 | Backward compatibility | ⬜ | |
| 2 | Market filtresi | ⬜ | |
| 3 | Restaurant filtresi | ⬜ | |
| 4 | Pharmacy filtresi | ⬜ | |
| 5 | Invalid latitude | ⬜ | |
| 6 | Invalid radius | ⬜ | |
| 7 | Invalid category | ⬜ | |
| 8 | Empty result | ⬜ | |

**Test Tarihi**: _________________  
**Test Eden**: Osman Ali Aydemir  
**Sonuç**: ✅ Başarılı / ❌ Başarısız / ⚠️ Kısmi

---

**Son Güncelleme**: 7 Ocak 2025

