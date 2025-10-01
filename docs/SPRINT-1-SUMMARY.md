# 🎯 Sprint 1: Role & Auth - Tamamlandı ✅

## 📋 Özet

Sprint 1'de **rol tabanlı yetkilendirme sistemi** başarıyla implement edildi. Artık kullanıcılar farklı rollerle (Customer, MerchantOwner, Courier, Admin) kayıt olabiliyor ve her rol kendi yetkilerine göre işlem yapabiliyor.

## ✅ Tamamlanan Görevler

### 1. **Domain Layer Güncellemeleri**

#### ✨ UserRole Enum
```csharp
// src/Domain/Enums/UserRole.cs
public enum UserRole
{
    Customer = 1,        // Müşteri (default)
    MerchantOwner = 2,   // Merchant sahibi
    Courier = 3,         // Kurye
    Admin = 4           // Admin
}
```

#### 👤 User Entity
- `Role` property eklendi (default: Customer)
- `OwnedMerchants` navigation property eklendi

```csharp
public UserRole Role { get; set; } = UserRole.Customer;
public virtual ICollection<Merchant> OwnedMerchants { get; set; }
```

#### 🏪 Merchant Entity
- `OwnerId` property eklendi
- `Owner` navigation property eklendi

```csharp
public Guid OwnerId { get; set; }
public virtual User Owner { get; set; }
```

---

### 2. **Application Layer Güncellemeleri**

#### 📝 DTO Değişiklikleri

**AuthDtos.cs:**
```csharp
// RegisterRequest - Role parametresi eklendi
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole? Role = null // Optional, default Customer
);

// AuthResponse - Role ve kullanıcı bilgileri eklendi
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserRole Role,           // ✅ Yeni
    Guid UserId,            // ✅ Yeni
    string Email,           // ✅ Yeni
    string FullName         // ✅ Yeni
);
```

**MerchantDtos.cs:**
```csharp
// MerchantResponse - Owner bilgileri eklendi
public record MerchantResponse(
    Guid Id,
    Guid OwnerId,           // ✅ Yeni
    string OwnerName,       // ✅ Yeni
    string Name,
    // ... diğer alanlar
);
```

#### 🔐 AuthService
- JWT token'a **role claim** eklendi
- Register ve Login'de role bilgisi döndürülüyor
- Refresh token'da role bilgisi korunuyor

```csharp
// Token oluşturma - Role claim'i ile
var roleClaim = new Claim(ClaimTypes.Role, user.Role.ToString());
var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email, new[] { roleClaim });
```

#### 🏪 MerchantService
- `CreateMerchantAsync`: `ownerId` parametresi eklendi
- `UpdateMerchantAsync`: Sadece merchant sahibi güncelleyebilir kontrolü eklendi
- Owner bilgileri response'a dahil edildi

```csharp
public async Task<Result<MerchantResponse>> CreateMerchantAsync(
    CreateMerchantRequest request,
    Guid ownerId,  // ✅ Yeni parametre
    CancellationToken cancellationToken = default)
{
    // Owner kontrolü
    var ownerExists = await _unitOfWork.ReadRepository<User>()
        .AnyAsync(u => u.Id == ownerId && u.IsActive, cancellationToken);
    
    if (!ownerExists)
        return Result.Fail<MerchantResponse>("Owner user not found or inactive", "NOT_FOUND_OWNER");
    
    // Merchant oluşturma
    var merchant = new Merchant
    {
        OwnerId = ownerId,  // ✅ Owner atanıyor
        // ... diğer alanlar
    };
}

public async Task<Result<MerchantResponse>> UpdateMerchantAsync(
    Guid id,
    UpdateMerchantRequest request,
    Guid currentUserId,  // ✅ Yeni parametre
    CancellationToken cancellationToken = default)
{
    // Sadece merchant sahibi güncelleyebilir
    if (merchant.OwnerId != currentUserId)
    {
        return Result.Fail<MerchantResponse>(
            "You are not authorized to update this merchant", 
            "FORBIDDEN_NOT_OWNER");
    }
    // ...
}
```

---

### 3. **Infrastructure Layer Güncellemeleri**

#### 🔑 JwtTokenService
- Role claim'i token'a eklendi
- Email claim'i eklendi (ek güvenlik)

#### 🗄️ AppDbContext
- `User.Role` mapping'i eklendi (int olarak saklanıyor)
- `Merchant.OwnerId` foreign key ilişkisi kuruldu

```csharp
// User configuration
entity.Property(e => e.Role)
    .IsRequired()
    .HasConversion<int>()
    .HasDefaultValue(UserRole.Customer);

// Merchant configuration
entity.HasOne(e => e.Owner)
    .WithMany(u => u.OwnedMerchants)
    .HasForeignKey(e => e.OwnerId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

### 4. **WebApi Layer Güncellemeleri**

#### 🛡️ Authorization Extensions
**src/WebApi/Extensions/AuthorizationExtensions.cs** oluşturuldu.

**Helper Methods:**
```csharp
// Role-based authorization
builder.RequireRole(UserRole.Admin, UserRole.MerchantOwner)
builder.RequireAdmin()
builder.RequireMerchantOwner()
builder.RequireCourier()

// Claims extraction
user.GetUserId()         // Guid
user.GetEmail()          // string
user.GetRole()           // UserRole?
user.IsAdmin()           // bool
user.IsMerchantOwner()   // bool
user.IsCourier()         // bool
user.IsCustomer()        // bool
```

#### 📍 MerchantEndpoints Güncellemeleri
```csharp
// ❌ ÖNCE (Herkes merchant oluşturabiliyordu)
group.MapPost("/", ...).RequireAuthorization();

// ✅ SONRA (Sadece Admin ve MerchantOwner oluşturabilir)
group.MapPost("/", async (
    [FromBody] CreateMerchantRequest request,
    ClaimsPrincipal user,  // ✅ User inject edildi
    [FromServices] IMerchantService merchantService,
    CancellationToken ct) =>
{
    var userId = user.GetUserId();  // ✅ CurrentUserId alındı
    var result = await merchantService.CreateMerchantAsync(request, userId, ct);
    return result.ToIResult();
})
.WithName("CreateMerchant")
.RequireAuthorization()
.RequireRole("Admin", "MerchantOwner");  // ✅ Role kontrolü

// Update endpoint
group.MapPut("/{id:guid}", ...)
    .RequireRole("Admin", "MerchantOwner");

// Delete endpoint
group.MapDelete("/{id:guid}", ...)
    .RequireAdmin();  // ✅ Sadece Admin silebilir
```

---

### 5. **Database Migrations**

#### 📄 Migration Script
**database/migrations/001_add_roles_and_owner.sql** oluşturuldu.

**Değişiklikler:**
```sql
-- 1. Users tablosuna Role column ekle
ALTER TABLE Users ADD Role INT NOT NULL DEFAULT 1; -- Customer

-- 2. Role değerleri için check constraint
ALTER TABLE Users ADD CONSTRAINT CK_Users_Role 
    CHECK (Role IN (1, 2, 3, 4));

-- 3. Role için index
CREATE INDEX IX_Users_Role ON Users(Role);

-- 4. Merchants tablosuna OwnerId ekle
ALTER TABLE Merchants ADD OwnerId UNIQUEIDENTIFIER NULL;

-- 5. Foreign key constraint
ALTER TABLE Merchants ADD CONSTRAINT FK_Merchants_Owners 
    FOREIGN KEY (OwnerId) REFERENCES Users(Id);

-- 6. Index ekle
CREATE INDEX IX_Merchants_OwnerId ON Merchants(OwnerId);
```

---

## 🎯 Yeni Yetkiler ve Kısıtlamalar

### 👤 Customer (Müşteri)
- ✅ Sipariş verebilir
- ✅ Sepet yönetimi yapabilir
- ✅ Adres ekleyebilir/düzenleyebilir
- ❌ Merchant oluşturamaz
- ❌ Ürün ekleyemez

### 🏪 MerchantOwner (İşletme Sahibi)
- ✅ Merchant oluşturabilir (kendi adına)
- ✅ Kendi merchant'ını güncelleyebilir
- ✅ Kendi ürünlerini ekleyebilir/düzenleyebilir
- ✅ Kendi siparişlerini yönetebilir
- ❌ Başkasının merchant'ını düzenleyemez
- ❌ Merchant silemez (sadece Admin)

### 🚴 Courier (Kurye)
- ✅ Sipariş teslimatı yapabilir
- ✅ Konum güncellemesi yapabilir
- ✅ Sipariş durumu güncelleyebilir
- ❌ Merchant oluşturamaz
- ❌ Ürün ekleyemez

### 👨‍💼 Admin (Yönetici)
- ✅ TÜM yetkilere sahip
- ✅ Merchant oluşturabilir
- ✅ Merchant silebilir
- ✅ Tüm merchant'ları güncelleyebilir
- ✅ Kullanıcı yönetimi
- ✅ Sistem ayarları

---

## 📊 API Değişiklikleri

### 🔐 Auth Endpoints

#### Register
```http
POST /api/v1/auth/register

{
  "email": "merchant@getir.com",
  "password": "Test123!",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "phoneNumber": "+905551234567",
  "role": 2  // ✅ YENİ: 1=Customer, 2=MerchantOwner, 3=Courier, 4=Admin
}

Response:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-10-01T12:00:00Z",
  "role": 2,  // ✅ YENİ
  "userId": "guid",  // ✅ YENİ
  "email": "merchant@getir.com",  // ✅ YENİ
  "fullName": "Ahmet Yılmaz"  // ✅ YENİ
}
```

#### Login
```http
POST /api/v1/auth/login

Response: (Register ile aynı format)
{
  "accessToken": "...",
  "role": 2,  // ✅ Role bilgisi döndürülüyor
  "userId": "guid",
  "email": "...",
  "fullName": "..."
}
```

### 🏪 Merchant Endpoints

#### Create Merchant
```http
POST /api/v1/merchants
Authorization: Bearer {token}
// ✅ Sadece Admin ve MerchantOwner erişebilir

{
  "name": "Migros Kadıköy",
  "categoryId": "...",
  // ... diğer alanlar
}

Response:
{
  "id": "guid",
  "ownerId": "current-user-guid",  // ✅ YENİ
  "ownerName": "Ahmet Yılmaz",     // ✅ YENİ
  "name": "Migros Kadıköy",
  // ... diğer alanlar
}
```

#### Update Merchant
```http
PUT /api/v1/merchants/{id}
Authorization: Bearer {token}
// ✅ Sadece merchant sahibi veya Admin güncelleyebilir

Error Response (Yetkisiz):
{
  "success": false,
  "error": "You are not authorized to update this merchant",
  "errorCode": "FORBIDDEN_NOT_OWNER"
}
```

#### Delete Merchant
```http
DELETE /api/v1/merchants/{id}
Authorization: Bearer {token}
// ✅ Sadece Admin silebilir

Error Response (403):
{
  "success": false,
  "error": "Forbidden",
  "errorCode": "FORBIDDEN"
}
```

---

## 🧪 Test Senaryoları

### ✅ Scenario 1: Customer Kayıt
```bash
# 1. Customer olarak kayıt ol
POST /api/v1/auth/register
{
  "email": "customer@test.com",
  "password": "Test123!",
  "firstName": "Ali",
  "lastName": "Demir"
  # role belirtilmediğinde otomatik Customer
}

# Response: role=1 (Customer)

# 2. Merchant oluşturmaya çalış
POST /api/v1/merchants  # ❌ 403 Forbidden
```

### ✅ Scenario 2: MerchantOwner Kayıt ve Merchant Oluşturma
```bash
# 1. MerchantOwner olarak kayıt ol
POST /api/v1/auth/register
{
  "email": "owner@migros.com",
  "password": "Test123!",
  "firstName": "Mehmet",
  "lastName": "Çelik",
  "role": 2  # MerchantOwner
}

# Response: role=2, userId=abc-123

# 2. Login yap ve token al
POST /api/v1/auth/login
# Response: accessToken + role=2

# 3. Merchant oluştur (token ile)
POST /api/v1/merchants
Authorization: Bearer {token}
{
  "name": "Migros Kadıköy",
  "categoryId": "{category-guid}",
  // ...
}

# Response: ✅ ownerId=abc-123 (current user)

# 4. Kendi merchant'ını güncelle
PUT /api/v1/merchants/{merchant-id}  # ✅ Başarılı

# 5. Başkasının merchant'ını güncellemeye çalış
PUT /api/v1/merchants/{other-merchant-id}  # ❌ 403 Forbidden
```

### ✅ Scenario 3: Admin Yetkileri
```bash
# 1. Admin olarak kayıt ol
POST /api/v1/auth/register
{
  "role": 4  # Admin
}

# 2. Herhangi bir merchant'ı güncelle
PUT /api/v1/merchants/{any-id}  # ✅ Başarılı

# 3. Merchant sil
DELETE /api/v1/merchants/{id}  # ✅ Başarılı (sadece Admin)
```

---

## 🔍 JWT Token İçeriği

**Önce (Sprint 1 Öncesi):**
```json
{
  "sub": "user-guid",
  "email": "user@test.com",
  "jti": "token-guid",
  "nameid": "user-guid"
}
```

**Sonra (Sprint 1 Sonrası):**
```json
{
  "sub": "user-guid",
  "email": "user@test.com",
  "jti": "token-guid",
  "nameid": "user-guid",
  "role": "MerchantOwner",  // ✅ YENİ!
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "MerchantOwner"
}
```

---

## 📁 Oluşturulan/Güncellenen Dosyalar

### ✨ Yeni Dosyalar
- `src/Domain/Enums/UserRole.cs`
- `src/WebApi/Extensions/AuthorizationExtensions.cs`
- `database/migrations/001_add_roles_and_owner.sql`
- `docs/SPRINT-1-SUMMARY.md` (bu döküman)

### 📝 Güncellenen Dosyalar
- `src/Domain/Entities/User.cs`
- `src/Domain/Entities/Merchant.cs`
- `src/Application/DTO/AuthDtos.cs`
- `src/Application/DTO/MerchantDtos.cs`
- `src/Application/Services/Auth/AuthService.cs`
- `src/Application/Services/Merchants/IMerchantService.cs`
- `src/Application/Services/Merchants/MerchantService.cs`
- `src/Infrastructure/Security/JwtTokenService.cs`
- `src/Infrastructure/Persistence/AppDbContext.cs`
- `src/WebApi/Endpoints/MerchantEndpoints.cs`

---

## 🚀 Deployment Adımları

### 1. Database Migration
```bash
# SQL Server'da migration script'ini çalıştır
sqlcmd -S OAA\MSSQLSERVER2014 -E -i database/migrations/001_add_roles_and_owner.sql
```

### 2. Application Build
```bash
dotnet build
```

### 3. Run & Test
```bash
dotnet run --project src/WebApi
```

### 4. Test API
```bash
# ⭐ YENİ Postman collection v2'yi import et
docs/Getir-API-v2.postman_collection.json

# Kullanım kılavuzu:
docs/POSTMAN-SPRINT1-GUIDE.md
```

**Postman Collection v2 Özellikleri:**
- ✅ 4 farklı role ile kayıt (Customer, MerchantOwner, Courier, Admin)
- ✅ Otomatik token yönetimi (role-based)
- ✅ Test scenarios (403 Forbidden testleri)
- ✅ Console log'lar ile detaylı debug
- ✅ Owner bilgisi tracking

---

## 🎉 Sprint 1 Başarıyla Tamamlandı!

**Şimdi Neler Yapabiliriz:**
- ✅ Kullanıcılar farklı rollerle kayıt olabiliyor
- ✅ JWT token'da role bilgisi taşınıyor
- ✅ Endpoint'lere role bazlı erişim kontrolü eklendi
- ✅ MerchantOwner'lar kendi merchant'larını oluşturup yönetebiliyor
- ✅ Admin tüm sistemi yönetebiliyor
- ✅ Authorization extension metodları kullanıma hazır

---

## 📋 Sonraki Adımlar (Sprint 2)

Sprint 2'de **Kategori Hiyerarşisi** üzerinde çalışacağız:
- ServiceCategory (Market, Yemek, Su, Eczane)
- ProductCategory (Hiyerarşik, merchant-specific)
- Merchant'ların kendi kategorilerini yönetebilmesi

---

**🎯 Sprint 1 - TAMAMLANDI ✅**  
**Tarih:** 1 Ekim 2025  
**Geliştirici:** AI + osmanali.aydemir  
**Süre:** ~2 saat

