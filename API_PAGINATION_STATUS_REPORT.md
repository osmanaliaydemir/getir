# API Pagination Durumu Raporu

## ✅ Pagination MEVCUT - Mobil Destekleniyor

### 1. Search Endpoints
| Endpoint | Query Model | Pagination | Status |
|----------|------------|------------|--------|
| `GET /api/v1/search/products` | `SearchProductsQuery` | `Page`, `PageSize` | ✅ |
| `GET /api/v1/search/merchants` | `SearchMerchantsQuery` | `Page`, `PageSize` | ✅ |

**Mobil Mapping:**
```dart
// Mobile gönderir:
queryParameters: {'query': query, 'pageNumber': page, 'pageSize': limit}

// API alır (SearchProductsQuery):
Page = pageNumber (case-insensitive)
PageSize = limit (case-insensitive)
```

### 2. Product Endpoints
| Endpoint | Query Model | Pagination | Status |
|----------|------------|------------|--------|
| `GET /api/v1/product` | `PaginationQuery` | `Page`, `PageSize` | ✅ |
| `GET /api/v1/product/merchant/{merchantId}` | `PaginationQuery` | `Page`, `PageSize` | ✅ |

**Mobil Mapping:**
```dart
// Mobile gönderir:
queryParameters: {'page': page, 'limit': limit}

// API alır (PaginationQuery):
Page = page
PageSize = limit
```

### 3. Merchant Endpoints
| Endpoint | Query Model | Pagination | Status |
|----------|------------|------------|--------|
| `GET /api/v1/merchant` | `PaginationQuery` | `Page`, `PageSize` | ✅ |
| `GET /api/v1/merchant/by-category-type/{categoryType}` | `PaginationQuery` | `Page`, `PageSize` | ✅ |

**Mobil Mapping:**
```dart
// Mobile gönderir:
queryParameters: {'page': page, 'limit': limit}

// API alır (PaginationQuery):
Page = page
PageSize = limit
```

### 4. Notification Endpoints
| Endpoint | Query Model | Pagination | Status |
|----------|------------|------------|--------|
| `GET /api/v1/notification` | `PaginationQuery` | `Page`, `PageSize` | ✅ |

**Mobil Mapping:**
```dart
// Mobile gönderir:
queryParameters: {'page': page, 'pageSize': pageSize}

// API alır (PaginationQuery):
Page = page
PageSize = pageSize
```

---

## ❌ Pagination YOK - Implement Edilmeli

### 1. User Orders Endpoint
```csharp
// src/WebApi/Controllers/UserController.cs:254
[HttpGet("orders")]
public IActionResult GetUserOrders(CancellationToken ct = default)
{
    // TODO: Implement orders service to get user orders
    return Ok(new List<object>());
}
```

**Problem:**
- Endpoint implement edilmemiş (TODO)
- Mobil taraf pagination ile çağırıyor: `queryParameters: {'page': page, 'limit': limit}`
- API boş liste döndürüyor

**Çözüm:**
```csharp
[HttpGet("orders")]
public async Task<IActionResult> GetUserOrders(
    [FromQuery] PaginationQuery query,
    CancellationToken ct = default)
{
    var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
    if (unauthorizedResult != null) return unauthorizedResult;

    var result = await _orderService.GetUserOrdersAsync(userId, query, ct);
    return ToActionResult<PagedResult<OrderResponse>>(result);
}
```

### 2. User Favorites Endpoint
```csharp
// src/WebApi/Controllers/UserController.cs:183
[HttpGet("favorites")]
public IActionResult GetFavorites(CancellationToken ct = default)
{
    // TODO: Implement favorites service
    return Ok(new List<object>());
}
```

**Problem:**
- Endpoint implement edilmemiş (TODO)
- Mobil taraf pagination ile çağırıyor: `queryParameters: {'page': page, 'limit': limit}`
- API boş liste döndürüyor

**Çözüm:**
```csharp
[HttpGet("favorites")]
public async Task<IActionResult> GetFavorites(
    [FromQuery] PaginationQuery query,
    CancellationToken ct = default)
{
    var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
    if (unauthorizedResult != null) return unauthorizedResult;

    var result = await _favoritesService.GetUserFavoritesAsync(userId, query, ct);
    return ToActionResult<PagedResult<FavoriteProductResponse>>(result);
}
```

### 3. Diğer TODO Endpoint'ler (Pagination Olmayan)
```csharp
// src/WebApi/Controllers/UserController.cs
[HttpGet("orders/{orderId}")] // GetOrderDetails - TODO
[HttpPost("orders/{orderId}/cancel")] // CancelOrder - TODO
[HttpPost("orders/{orderId}/reorder")] // Reorder - TODO
[HttpPost("favorites")] // AddToFavorites - TODO
[HttpDelete("favorites/{productId}")] // RemoveFromFavorites - TODO
[HttpGet("favorites/{productId}/status")] // IsFavorite - TODO
```

---

## 📝 API Pagination Model

### PaginationQuery
```csharp
public class PaginationQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "desc";

    public bool IsAscending => SortDir?.ToLower() == "asc";
}
```

### SearchProductsQuery
```csharp
public record SearchProductsQuery(
    string Query,
    Guid? MerchantId,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20);
```

### SearchMerchantsQuery
```csharp
public record SearchMerchantsQuery(
    string Query,
    Guid? CategoryId,
    decimal? Latitude,
    decimal? Longitude,
    int? MaxDistance, // km
    int Page = 1,
    int PageSize = 20);
```

---

## ⚠️ Mobil-API Parametre Uyumsuzluğu

### Durum:
Mobil tarafta bazı endpoint'ler için farklı parametre isimleri kullanılıyor:

**Search endpoint'leri:**
```dart
// Mobile
queryParameters: {'query': query, 'pageNumber': page, 'pageSize': limit}

// API beklediği
Page = pageNumber  // ✅ ASP.NET Core case-insensitive, çalışır
PageSize = limit   // ❌ UYUMSUZ! API 'PageSize' bekliyor
```

**Diğer endpoint'ler:**
```dart
// Mobile
queryParameters: {'page': page, 'limit': limit}

// API beklediği
Page = page        // ✅ Uyumlu
PageSize = limit   // ❌ UYUMSUZ! API 'PageSize' bekliyor
```

### Öneri:
Mobil tarafta **tutarlılık** için parametreleri düzelt:

```dart
// ✅ DOĞRU
queryParameters: {'page': page, 'pageSize': limit}

// ❌ YANLIŞ
queryParameters: {'page': page, 'limit': limit}
queryParameters: {'query': query, 'pageNumber': page, 'pageSize': limit}
```

**VEYA** API tarafında custom model binding ekle (önerilmez).

---

## 🎯 Öncelikli Aksiyonlar

### 1. API Tarafı (YÜKSEK ÖNCELİK ❗)
- [ ] `UserController.GetUserOrders()` - Order service implement et
- [ ] `UserController.GetFavorites()` - Favorites service implement et
- [ ] Diğer TODO endpoint'leri implement et

### 2. Mobil Tarafı (ORTA ÖNCELİK ⚠️)
- [ ] Search endpoint'lerinde parametre isimlerini düzelt: `pageNumber` → `page`
- [ ] Tüm endpoint'lerde `limit` → `pageSize` yap (tutarlılık için)

### 3. Test (DÜŞÜK ÖNCELİK 📝)
- [ ] Integration testleri yaz (pagination edge case'ler)
- [ ] Mobil tarafta pagination test senaryoları ekle

---

## ✅ Sonuç

**Genel Durum:** 7/9 endpoint pagination destekliyor (78% tamamlanmış)

**Kritik Sorunlar:**
1. ❌ User Orders endpoint implement edilmemiş
2. ❌ User Favorites endpoint implement edilmemiş
3. ⚠️ Mobil-API parametre isimleri tutarsız

**Pozitif Yönler:**
1. ✅ Core endpoint'lerde (Search, Product, Merchant) pagination var
2. ✅ API model yapısı tutarlı (PaginationQuery)
3. ✅ ASP.NET Core case-insensitive, çoğu çalışır

**Tavsiye:** API'deki TODO endpoint'leri implement et, sonra mobil parametre tutarlılığını düzelt.

