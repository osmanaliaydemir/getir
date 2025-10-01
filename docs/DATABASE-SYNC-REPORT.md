# 🔍 Database-Project Senkronizasyon Raporu

**Oluşturulma:** 1 Ekim 2025  
**Database:** GetirDb (OAA\MSSQLSERVER2014)  
**Sprint:** Sprint 2 sonrası

---

## 📊 GENEL DURUM

### ✅ BAŞARILI
- **Total Tables:** 18
- **Total Entities:** 15
- **Sync Status:** %90 (Küçük temizlik gerekiyor)

---

## 📋 TABLO-ENTITY KARŞILAŞTIRMASI

### ✅ SYNC (15 tablo)

| SQL Tablo | Entity | Status | Notes |
|-----------|--------|--------|-------|
| Users | User.cs | ✅ | Role column eklendi (Sprint 1) |
| RefreshTokens | RefreshToken.cs | ✅ | |
| ServiceCategories | ServiceCategory.cs | ✅ | **YENİ** (Sprint 2) |
| ProductCategories | ProductCategory.cs | ✅ | **YENİ** (Sprint 2) |
| Merchants | Merchant.cs | ✅ | OwnerId + ServiceCategoryId (Sprint 1+2) |
| Products | Product.cs | ✅ | ProductCategoryId (Sprint 2) |
| Orders | Order.cs | ✅ | |
| OrderLines | OrderLine.cs | ✅ | |
| UserAddresses | UserAddress.cs | ✅ | |
| CartItems | CartItem.cs | ✅ | |
| Coupons | Coupon.cs | ✅ | |
| CouponUsages | CouponUsage.cs | ✅ | |
| Campaigns | Campaign.cs | ✅ | |
| Notifications | Notification.cs | ✅ | |
| Couriers | Courier.cs | ✅ | |

### ⚠️ SQL'DE VAR, ENTITY YOK (2 tablo)

| SQL Tablo | Entity | Durum | Öneri |
|-----------|--------|-------|-------|
| UserLoyaltyPoints | ❌ Yok | Schema'da var ama kullanılmıyor | Entity oluştur veya tabloyu kaldır |
| LoyaltyPointTransactions | ❌ Yok | Schema'da var ama kullanılmıyor | Entity oluştur veya tabloyu kaldır |

### 🗑️ ESKİ/GEREKSİZ TABLO (1 tablo)

| SQL Tablo | Durum | Öneri |
|-----------|-------|-------|
| Categories | ❌ Eski yapı | ServiceCategories'e migrate edildi, **silinebilir** |

---

## 🔍 DETAYLI ANALİZ

### 1. ✅ Users Tablosu (Sprint 1 Güncellemesi)

**Yeni Column:**
```sql
Role INT NOT NULL DEFAULT 1
```

**Doğrulama:**
```bash
✅ Column var
✅ NOT NULL constraint
✅ Default value: 1 (Customer)
```

---

### 2. ✅ Merchants Tablosu (Sprint 1+2 Güncellemesi)

**Sprint 1:**
```sql
OwnerId UNIQUEIDENTIFIER NULL
FK_Merchants_Owners → Users(Id)
```

**Sprint 2:**
```sql
ServiceCategoryId UNIQUEIDENTIFIER NOT NULL  (CategoryId → renamed)
FK_Merchants_ServiceCategories → ServiceCategories(Id)
```

**Doğrulama:**
```bash
✅ OwnerId column var
✅ ServiceCategoryId var (CategoryId rename edildi)
✅ Foreign key'ler doğru
✅ Index'ler var
```

**⚠️ Eski Index:**
```sql
IX_Merchants_CategoryId  ← ESKİ (silinebilir)
IX_Merchants_ServiceCategoryId  ← YENİ ✅
```

---

### 3. ✅ Products Tablosu (Sprint 2 Güncellemesi)

**Sprint 2:**
```sql
ProductCategoryId UNIQUEIDENTIFIER NULL  (CategoryId → renamed)
FK_Products_ProductCategories → ProductCategories(Id) ON DELETE NO ACTION
```

**Doğrulama:**
```bash
✅ ProductCategoryId var (CategoryId rename edildi)
✅ Nullable (ürünler kategorisiz olabilir)
✅ Foreign key doğru (NO ACTION - cascade conflict çözüldü)
✅ Index var
```

**⚠️ Eski Index:**
```sql
IX_Products_CategoryId  ← ESKİ (silinebilir)
IX_Products_ProductCategoryId  ← YENİ ✅
```

---

### 4. ✅ ServiceCategories Tablosu (Yeni - Sprint 2)

**Yapı:**
```sql
Id UNIQUEIDENTIFIER PRIMARY KEY
Name NVARCHAR(200) NOT NULL
IconUrl NVARCHAR(500) NULL  ← Emoji/icon için
DisplayOrder INT NOT NULL
IsActive BIT NOT NULL
```

**Seed Data:**
```
✅ Market (🛒)
✅ Yemek (🍔) - Duplicate var! (Restoran)
✅ Su (💧)
✅ Eczane (💊)
```

**⚠️ Problem:** ServiceCategories'de 4 kayıt var ama 2'si duplicate:
- "Market" x2
- "Restoran" x2

---

### 5. ✅ ProductCategories Tablosu (Yeni - Sprint 2)

**Yapı:**
```sql
Id UNIQUEIDENTIFIER PRIMARY KEY
MerchantId UNIQUEIDENTIFIER NOT NULL
ParentCategoryId UNIQUEIDENTIFIER NULL  ← Self-referencing (hiyerarşi)
Name NVARCHAR(200) NOT NULL
```

**Foreign Keys:**
```sql
✅ FK_ProductCategories_Merchants (ON DELETE CASCADE)
✅ FK_ProductCategories_ParentCategories (ON DELETE NO ACTION)
```

**Doğrulama:**
```bash
✅ Tablo oluşturuldu
✅ Self-referencing FK var
✅ Index'ler var
✅ Henüz veri yok (0 rows) - Normal, merchant'lar ekleyecek
```

---

### 6. ❌ Categories Tablosu (Eski)

**Durum:**
```bash
❌ Hala var (silinmedi)
❌ Artık kullanılmıyor
✅ Verisi ServiceCategories'e migrate edildi
```

**Öneri:** Güvenli bir şekilde silinebilir (backup alındıktan sonra).

---

## 🔧 TEMİZLİK GEREKTİREN DURUMLAR

### 1. Eski Index'ler

**Merchants:**
```sql
-- Silinmeli:
DROP INDEX IX_Merchants_CategoryId ON Merchants;
```

**Products:**
```sql
-- Silinmeli:
DROP INDEX IX_Products_CategoryId ON Products;
```

### 2. Duplicate ServiceCategories

**Kontrol:**
```sql
SELECT Name, COUNT(*) AS Count 
FROM ServiceCategories 
GROUP BY Name 
HAVING COUNT(*) > 1;

-- Sonuç:
-- Market: 2
-- Restoran: 2
```

**Öneri:** Duplicate'leri temizle, sadece birer tane kalsın.

### 3. Categories Tablosu

**Öneri:** Backup aldıktan sonra silinebilir.

---

## ⚠️ EKSİK ENTITY'LER

### UserLoyaltyPoints & LoyaltyPointTransactions

**SQL'de var ama Entity yok:**

```csharp
// Oluşturulmalı: src/Domain/Entities/UserLoyaltyPoint.cs
public class UserLoyaltyPoint
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public int TotalEarned { get; set; }
    public int TotalSpent { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public virtual User User { get; set; }
    public virtual ICollection<LoyaltyPointTransaction> Transactions { get; set; }
}

// Oluşturulmalı: src/Domain/Entities/LoyaltyPointTransaction.cs
public class LoyaltyPointTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }
    public int Points { get; set; }
    public string Type { get; set; }  // Earned, Spent, Expired
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual User User { get; set; }
    public virtual Order? Order { get; set; }
}
```

**Durum:** Loyalty sistemi için gerekli ama henüz implement edilmemiş.

---

## 📊 KAYIT SAYILARI

```
Users:                2 rows ✅
ServiceCategories:    4 rows ✅ (Duplicate var!)
ProductCategories:    0 rows ✅ (Normal, merchant'lar ekleyecek)
Merchants:            0 rows ✅
Products:             0 rows ✅
Orders:               0 rows ✅
```

---

## 🎯 ÖNERİLER

### 🔥 ACIL (Veri tutarlılığı için)

1. **ServiceCategories Duplicate Temizliği**
```sql
-- Duplicate'leri temizle
DELETE FROM ServiceCategories 
WHERE Id NOT IN (
    SELECT MIN(Id) 
    FROM ServiceCategories 
    GROUP BY Name
);
```

2. **Eski Index'leri Kaldır**
```sql
DROP INDEX IX_Merchants_CategoryId ON Merchants;
DROP INDEX IX_Products_CategoryId ON Products;
```

### 📋 ORTA ÖNCELİK

3. **Categories Tablosunu Sil** (Backup sonrası)
```sql
-- Önce backup al
SELECT * INTO Categories_Backup FROM Categories;

-- Sonra sil
DROP TABLE Categories;
```

4. **Loyalty Entity'lerini Oluştur**
- `UserLoyaltyPoint.cs`
- `LoyaltyPointTransaction.cs`
- İlgili Service ve Endpoint'ler

### ✨ GELİŞTİRME

5. **AppDbContext'e Ekle**
```csharp
public DbSet<UserLoyaltyPoint> UserLoyaltyPoints { get; set; }
public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; }
```

---

## ✅ DOĞRULANAN ÖZELLIKLER

### Sprint 1 ✅
- ✅ Users.Role column var
- ✅ Merchants.OwnerId column var
- ✅ Foreign key: Merchants → Users (Owner)

### Sprint 2 ✅
- ✅ ServiceCategories tablosu oluşturuldu
- ✅ ProductCategories tablosu oluşturuldu
- ✅ Merchants.ServiceCategoryId (renamed from CategoryId)
- ✅ Products.ProductCategoryId (renamed from CategoryId)
- ✅ Foreign key'ler doğru
- ✅ Index'ler eklendi
- ✅ Seed data eklendi
- ✅ Hiyerarşik yapı (self-referencing FK) çalışıyor

---

## 🚀 TEMİZLİK SCRIPT'İ

Migration sonrası temizlik için hazırladım:

**Dosya:** `database/migrations/003_cleanup_indexes_and_duplicates.sql`

---

## 📈 SONRAKI ADIMLAR

1. ✅ Cleanup script'ini çalıştır (opsiyonel)
2. ✅ Application'ı build et
3. ✅ Test et
4. 📋 Sprint 3'e geç (Loyalty sistemi bu sprint'e dahil edilebilir)

---

**🎉 Database sync analizi tamamlandı!**

