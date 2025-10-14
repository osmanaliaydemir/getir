# 🔧 Backend - Kategori Filtresi Ekleme

## 📊 Durum
Backend'de kategori bazlı merchant listeleme var AMA geo-location filtreleme ile birleşik değil.

---

## 🎯 Hedef
Yakındaki merchantları kategori tipine göre filtreleyebilme.

---

## 🔴 Yapılacaklar (Öncelik Sırası)

### 1. ✅ Interface Güncellemesi
**Dosya**: `src/Application/Services/GeoLocation/IGeoLocationService.cs`

```csharp
// EKLE:
Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsByCategoryAsync(
    double latitude, 
    double longitude, 
    ServiceCategoryType categoryType,
    double radius = 5.0,
    CancellationToken ct = default
);
```

**Tahmini Süre**: 5 dakika

---

### 2. ✅ Service Implementation
**Dosya**: `src/Application/Services/GeoLocation/GeoLocationService.cs`

```csharp
public async Task<Result<IEnumerable<NearbyMerchantResponse>>> GetNearbyMerchantsByCategoryAsync(
    double latitude,
    double longitude,
    ServiceCategoryType categoryType,
    double radius,
    CancellationToken ct)
{
    try
    {
        // Validate input
        if (latitude < -90 || latitude > 90)
            return Result<IEnumerable<NearbyMerchantResponse>>.Failure("Invalid latitude");
        
        if (longitude < -180 || longitude > 180)
            return Result<IEnumerable<NearbyMerchantResponse>>.Failure("Invalid longitude");
        
        if (radius <= 0 || radius > 50)
            return Result<IEnumerable<NearbyMerchantResponse>>.Failure("Radius must be between 0 and 50 km");
        
        // Get nearby merchants with category filter
        var query = _readOnlyRepository.GetQueryable<Merchant>()
            .Where(m => m.IsActive && m.ServiceCategoryType == categoryType)
            .AsNoTracking();
        
        var merchants = await query.ToListAsync(ct);
        
        // Calculate distances
        var nearbyMerchants = merchants
            .Select(m => new
            {
                Merchant = m,
                Distance = CalculateDistance(
                    latitude, longitude, 
                    m.Latitude, m.Longitude
                )
            })
            .Where(x => x.Distance <= radius)
            .OrderBy(x => x.Distance)
            .Select(x => new NearbyMerchantResponse
            {
                Id = x.Merchant.Id,
                Name = x.Merchant.Name,
                LogoUrl = x.Merchant.LogoUrl,
                CategoryType = x.Merchant.ServiceCategoryType,
                IsOpen = x.Merchant.IsOpen,
                Rating = x.Merchant.AverageRating,
                EstimatedDeliveryTime = x.Merchant.EstimatedDeliveryTimeMinutes,
                Distance = x.Distance,
                Latitude = x.Merchant.Latitude,
                Longitude = x.Merchant.Longitude,
                MinimumOrderAmount = x.Merchant.MinimumOrderAmount,
                DeliveryFee = x.Merchant.DeliveryFee
            })
            .ToList();
        
        return Result<IEnumerable<NearbyMerchantResponse>>.Success(nearbyMerchants);
    }
    catch (Exception ex)
    {
        _loggingService.LogError(ex, "Error getting nearby merchants by category");
        return Result<IEnumerable<NearbyMerchantResponse>>.Failure("An error occurred while getting nearby merchants");
    }
}
```

**Tahmini Süre**: 20-30 dakika

---

### 3. ✅ Controller Endpoint Ekleme
**Dosya**: `src/WebApi/Controllers/GeoLocationController.cs`

**Seçenek A - Yeni Endpoint**:
```csharp
/// <summary>
/// Get nearby merchants by category type within specified radius
/// </summary>
/// <param name="latitude">Latitude</param>
/// <param name="longitude">Longitude</param>
/// <param name="categoryType">Service category type</param>
/// <param name="radius">Radius in kilometers (default: 5)</param>
/// <param name="ct">Cancellation token</param>
/// <returns>Nearby merchants of specified category</returns>
[HttpGet("merchants/nearby/by-category/{categoryType}")]
[ProducesResponseType(typeof(IEnumerable<NearbyMerchantResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> GetNearbyMerchantsByCategory(
    [FromRoute] ServiceCategoryType categoryType,
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] double radius = 5.0,
    CancellationToken ct = default)
{
    var result = await _geoLocationService.GetNearbyMerchantsByCategoryAsync(
        latitude, longitude, categoryType, radius, ct);
    return ToActionResult(result);
}
```

**VEYA Seçenek B - Mevcut Endpoint'i Güncelle**:
```csharp
/// <summary>
/// Get nearby merchants within specified radius, optionally filtered by category
/// </summary>
[HttpGet("merchants/nearby")]
[ProducesResponseType(typeof(IEnumerable<NearbyMerchantResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> GetNearbyMerchants(
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] ServiceCategoryType? categoryType = null,  // EKLE
    [FromQuery] double radius = 5.0,
    CancellationToken ct = default)
{
    Result<IEnumerable<NearbyMerchantResponse>> result;
    
    if (categoryType.HasValue)
    {
        result = await _geoLocationService.GetNearbyMerchantsByCategoryAsync(
            latitude, longitude, categoryType.Value, radius, ct);
    }
    else
    {
        result = await _geoLocationService.GetNearbyMerchantsAsync(
            latitude, longitude, radius, ct);
    }
    
    return ToActionResult(result);
}
```

**ÖNERİ**: Seçenek B (mevcut endpoint'i güncelle) - Backward compatible

**Tahmini Süre**: 10-15 dakika

---

### 4. ✅ Response DTO Kontrolü
**Dosya**: `src/Application/DTO/NearbyMerchantResponse.cs`

Kontrol Et:
```csharp
public class NearbyMerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? LogoUrl { get; set; }
    public ServiceCategoryType CategoryType { get; set; }  // VARSA OK
    public bool IsOpen { get; set; }
    public double? Rating { get; set; }
    public int? EstimatedDeliveryTime { get; set; }
    public double Distance { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? DeliveryFee { get; set; }
}
```

**Eğer CategoryType yoksa EKLE!**

**Tahmini Süre**: 5 dakika

---

### 5. ✅ Test

**Postman/Swagger Test**:
```
GET /api/v1/geo/merchants/nearby?latitude=41.0082&longitude=28.9784&categoryType=2&radius=5

Beklenen Sonuç:
- Sadece Market (categoryType=2) olan merchantlar dönmeli
- Distance hesaplanmış olmalı
- IsOpen durumu doğru olmalı
```

**Test Senaryoları**:
- ✅ Market kategorisi
- ✅ Restaurant kategorisi
- ✅ Pharmacy kategorisi
- ✅ CategoryType null (tüm merchantlar)
- ✅ Geçersiz koordinatlar (error handling)
- ✅ Radius 0 veya negatif (error handling)

**Tahmini Süre**: 15-20 dakika

---

## 📊 TOPLAM TAHMINI SÜRE

**1-1.5 saat** (backend değişikliği + test)

---

## 🔄 İLERİ SEVİYE İYİLEŞTİRMELER (Opsiyonel)

### 1. Pagination Desteği
```csharp
[HttpGet("merchants/nearby")]
public async Task<IActionResult> GetNearbyMerchants(
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] ServiceCategoryType? categoryType = null,
    [FromQuery] double radius = 5.0,
    [FromQuery] PaginationQuery? pagination = null,  // EKLE
    CancellationToken ct = default)
```

### 2. Ek Filtreler
```csharp
[FromQuery] bool? isOpen = null,          // Sadece açık olanlar
[FromQuery] decimal? minRating = null,    // Minimum rating
[FromQuery] decimal? maxDeliveryFee = null, // Maksimum teslimat ücreti
```

### 3. Sıralama Seçenekleri
```csharp
[FromQuery] string? sortBy = "distance",  // distance, rating, deliveryFee
[FromQuery] string? sortOrder = "asc",    // asc, desc
```

### 4. Caching
```csharp
// Service method'una cache ekle
[ResponseCache(Duration = 60)] // 60 saniye cache
```

---

## 📝 NOTLAR

1. **Database Index**: 
   - `Merchant` tablosunda `ServiceCategoryType` ve `(Latitude, Longitude)` index'leri var mı kontrol et
   - Performans için gerekli

2. **Distance Calculation**:
   - Haversine formula kullanılıyor mu? Kontrol et
   - Alternatif: PostgreSQL PostGIS extension

3. **Error Handling**:
   - Invalid coordinates
   - Invalid radius
   - Empty results

4. **Logging**:
   - Her API call log'lanmalı
   - Performance metrics (response time)

---

## ✅ Checklist

- [x] Interface güncellendi
- [x] Service implementation tamamlandı
- [x] Controller endpoint eklendi
- [x] Response DTO kontrol edildi (CategoryType field eklendi)
- [x] Using statements eklendi (Getir.Domain.Enums)
- [x] Merchant.ServiceCategory.Type kullanımı düzeltildi
- [x] ServiceCategory include eklendi (eager loading)
- [x] Build başarılı (64 warning, 0 error)
- [ ] Unit testler yazıldı
- [ ] Integration testler yazıldı
- [x] API documentation (Swagger) güncellendi
- [x] Postman collection hazırlandı (BACKEND_CATEGORY_TEST.md)
- [ ] Performance test yapıldı
- [ ] API çalışıyor ve manuel test tamamlandı (bkz: BACKEND_CATEGORY_TEST.md)
- [ ] Code review tamamlandı
- [ ] Git commit yapıldı
- [ ] Merge edildi

---

**Son Güncelleme**: 7 Ocak 2025  
**Geliştirici**: Osman Ali Aydemir

