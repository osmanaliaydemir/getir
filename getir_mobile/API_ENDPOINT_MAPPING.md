# 🔗 API Endpoint Mapping - WebApi ↔ Mobile

## 📋 **Endpoint Analizi ve Uyumluluk Raporu**

### ✅ **1. Authentication Endpoints**

| Endpoint | Method | WebApi | Mobile | Status |
|----------|--------|---------|---------|--------|
| `/api/v1/auth/register` | POST | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/auth/login` | POST | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/auth/refresh` | POST | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/auth/logout` | POST | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/auth/forgot-password` | POST | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/auth/reset-password` | POST | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/auth/change-password` | POST | ✅ | ❌ | ⚠️ Mobilde Eksik! |

**Eksik:** `change-password` endpoint'i mobil uygulamaya eklenecek

---

### ⚠️ **2. Product Endpoints**

| Endpoint | Method | WebApi | Mobile | Status |
|----------|--------|---------|---------|--------|
| `/api/v1/product?page=1&limit=20` | GET | ⚠️ | ✅ | ⚠️ Pagination format farklı |
| `/api/v1/product/{id}` | GET | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/product/merchant/{merchantId}` | GET | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/product` | POST | ✅ | ❌ | ⚠️ Create (Merchant için) |
| `/api/v1/product/{id}` | PUT | ✅ | ❌ | ⚠️ Update (Merchant için) |
| `/api/v1/product/{id}` | DELETE | ✅ | ❌ | ⚠️ Delete (Merchant için) |
| `/api/v1/product/categories` | GET | ❌ | ✅ | ❌ WebApi'de yok! |

**Sorunlar:**
1. Pagination format: WebApi `PaginationQuery` (page/pageSize), Mobile `page/limit`
2. Categories endpoint WebApi'de yok - `ProductCategoryController` kullanılmalı
3. CRUD operations mobilde yok (Merchant app için gerekli değil)

---

### ⚠️ **3. Merchant Endpoints**

| Endpoint | Method | WebApi | Mobile | Status |
|----------|--------|---------|---------|--------|
| `/api/v1/merchant/nearby` | GET | ✅ | ✅ | ⚠️ Params kontrol edilmeli |
| `/api/v1/merchant/{id}` | GET | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/merchant/service-category/{categoryId}` | GET | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/merchant/search` | GET | ✅ | ✅ | ✅ Uyumlu |

---

### ⚠️ **4. Order Endpoints**

| Endpoint | Method | WebApi | Mobile | Status |
|----------|--------|---------|---------|--------|
| `/api/v1/order` | POST | ✅ | ✅ | ⚠️ DTO kontrol edilmeli |
| `/api/v1/order/{id}` | GET | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/order/user/{userId}` | GET | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/order/{id}/status` | PUT | ✅ | ❌ | ⚠️ Courier için |
| `/api/v1/order/{id}/cancel` | POST | ✅ | ⚠️ | ⚠️ Kontrol edilmeli |

---

### ⚠️ **5. Cart Endpoints**

| Endpoint | Method | WebApi | Mobile | Status |
|----------|--------|---------|---------|--------|
| `/api/v1/cart` | GET | ✅ | ✅ | ⚠️ DTO kontrol edilmeli |
| `/api/v1/cart/add` | POST | ✅ | ✅ | ⚠️ DTO kontrol edilmeli |
| `/api/v1/cart/update` | PUT | ✅ | ✅ | ⚠️ DTO kontrol edilmeli |
| `/api/v1/cart/remove/{itemId}` | DELETE | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/cart/clear` | DELETE | ✅ | ✅ | ✅ Uyumlu |
| `/api/v1/cart/merge` | POST | ✅ | ✅ | ✅ Uyumlu |

---

### ⚠️ **6. Address Endpoints**

| Endpoint | Method | WebApi | Mobile | Status |
|----------|--------|---------|---------|--------|
| `/api/v1/user/addresses` | GET | ⚠️ | ✅ | ⚠️ Route kontrol edilmeli |
| `/api/v1/user/addresses` | POST | ⚠️ | ✅ | ⚠️ Route kontrol edilmeli |
| `/api/v1/user/addresses/{id}` | PUT | ⚠️ | ✅ | ⚠️ Route kontrol edilmeli |
| `/api/v1/user/addresses/{id}` | DELETE | ⚠️ | ✅ | ⚠️ Route kontrol edilmeli |

---

## 🔴 **KRİTİK SORUNLAR**

### 1. **Pagination Format Uyumsuzluğu**
```csharp
// WebApi: PaginationQuery
public class PaginationQuery {
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20;
}

// Mobile: page & limit
queryParams['page'] = page;
queryParams['limit'] = limit;  // ❌ Farklı parametre adı!
```

**Çözüm:** Mobile tarafı `pageSize` kullanmalı

### 2. **Response Format Uyumsuzluğu**
```csharp
// WebApi: BaseController.ToActionResult
return Ok(new { 
  success = true,
  value = result.Value,
  error = (string?)null
});

// Mobile: Bazen 'data' wrap'i, bazen direkt
final data = response.data['data'] ?? response.data;
```

**Çözüm:** WebApi tutarlı ApiResponse döner, mobil bunu handle ediyor ✅

### 3. **Endpoint Route Farklılıkları**
```
WebApi Route Pattern: [Route("api/v1/[controller]")]
Mobile Usage: '/api/v1/Auth', '/api/v1/Product'

❌ Mobile case-sensitive: '/api/v1/Auth' (Capital A)
✅ WebApi: api/v1/auth (lowercase)
```

**Çözüm:** Mobile endpoint'leri lowercase'e çevir

---

## 📝 **YAPILACAKLAR LİSTESİ**

### 🔴 **Kritik (Hemen)**
1. [ ] Mobile endpoint'lerini lowercase yap ('/api/v1/Auth' → '/api/v1/auth')
2. [ ] Pagination parametrelerini standardize et ('limit' → 'pageSize')
3. [ ] Change password endpoint'i mobil uygulamaya ekle
4. [ ] Product categories endpoint'i düzelt (ProductCategoryController kullan)
5. [ ] DTO field mapping'ini kontrol et (camelCase vs PascalCase)

### 🟡 **Yüksek Öncelik**
6. [ ] Address endpoint route'unu kontrol et (UserController?)
7. [ ] Order DTO'larını karşılaştır
8. [ ] Cart DTO'larını karşılaştır
9. [ ] Merchant DTO'larını karşılaştır
10. [ ] Review endpoint'lerini kontrol et

### 🟢 **Orta Öncelik**
11. [ ] API endpoint integration test'leri yaz
12. [ ] Postman collection oluştur
13. [ ] API documentation güncelle

---

## 🎯 **SONRAKİ ADIMLAR**

1. **Endpoint Route'ları Düzelt** (Case-sensitivity)
2. **Change Password Ekle**
3. **Pagination Standardize Et**
4. **DTO Mapping Kontrol Et**
5. **Integration Test**


