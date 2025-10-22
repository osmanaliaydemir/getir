# Veritabanı Temizleme ve Seed Dokümantasyonu

Bu doküman, geliştirme/test ortamında veritabanını güvenli şekilde sıfırlayıp tüm senaryoları test edilebilir hale getiren kapsamlı seed işlemlerini açıklar.

## Bağımlılıklar
- SQL Server (database: `db29009`)
- Erişim bilgileri `src/WebApi/appsettings.json` içindeki `DefaultConnection` ile uyumlu.
- PowerShell 5+ ve `sqlcmd`.

## İçerik
- `database/seed-data/Cleanup.sql`: Tüm tabloları FK sırasına uygun biçimde temizler.
- `database/seed-data/ServiceCategories.sql`: Market, Restoran vb. kategorileri ekler.
- `database/seed-data/UsersAndRoles.sql`: Admin, MerchantOwner ve 3 müşteri oluşturur.
- `database/seed-data/MerchantsForCategories.sql`: Kategorilere uygun merchant’ları (Migros, Burger King vb.) ekler.
- `database/seed-data/CoreProducts.sql`: Sabit GUID’lerle ürünleri ekler (Elma, Domates, Whopper, Chicken Royale, vb.).
- `database/migrations/AddProductReviewSystem.sql`: Ürün değerlendirme şeması + `sp_RecalculateProductRating` ve trigger.
- `database/seed-data/PopularProductsWithReviews.sql`: Teslim edilmiş siparişler, order lines ve ürün yorumları.
- `database/optimizations/AnalyticsStoredProcedures.sql`: Raporlar için optimize SP’ler (opsiyonel).
- `database/seed-data/Seed-Database.ps1`: Tüm adımların orkestrasyonu.

## Çalıştırma
PowerShell’de proje kökünden:

```bash
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File database/seed-data/Seed-Database.ps1
```

Parametreler gerekirse override edilebilir:

```bash
powershell -File database/seed-data/Seed-Database.ps1 -Server "your.server" -Database "yourdb" -User "user" -Password "pass"
```

## Oluşturulan Veriler ve Roller
- Kullanıcılar:
  - Admin: `admin@getir.com` (demo hash; login için gerçek hash üretin)
  - MerchantOwner: `owner@getir.com`
  - Müşteriler: `musteri1@getir.com`, `musteri2@getir.com`, `musteri3@getir.com`
- Roller (`UserRole`):
  - Customer=1, MerchantOwner=2, Courier=3, Admin=4
- Merchant’lar (örnek):
  - Market: `Migros Kadıköy` (Id: `7777...7777`)
  - Restoran: `Burger King Kadıköy` (Id: `8888...8888`)
- Ürünler (sabit GUID’ler):
  - Elma (1 kg): `3856B465-23CC-4640-BCDA-BA0EE1292F8D`
  - Domates (1 kg): `AF09ADEB-D99F-4AF5-B01C-6AC27853DC57`
  - Salatalık (1 kg): `EF45B4D3-432E-4653-B985-6F072D95B5E2`
  - Muz (1 kg): `D318CBBE-E874-4872-B2F8-116D78E47A85`
  - Whopper: `984F8EA8-62D0-45D8-93F7-7715149E2300`
  - Chicken Royale: `9E8B0C09-934C-4644-A02A-E22625AAFDBE`

## Test Edilebilirlik
- Siparişler Delivered durumda oluşturulur; ödeme metodu `Cash`, `PaymentStatus=Completed`.
- `ProductReviews` eklenir; `sp_RecalculateProductRating` ile `Products.Rating` ve `ReviewCount` güncellenir.
- Popüler ürünler ve rapor ekranları (MerchantPortal `Reports`) gerçekçi verilerle dolar.
- Analytics SP’leri ile WebApi raporları performanslı sorgulanır.

## Güvenlik Notu (Şifre Hash’i)
Uygulama PBKDF2-SHA256 (100k iterasyon) kullanır (`PasswordHasherService`). Seed edilen `PasswordHash` alanları demo placeholder’dır. Gerçek login testleri için şu adımlardan biri uygulanmalıdır:
- WebApi `AdminController` üzerinden kullanıcı oluşturma endpoint’i ile şifre hash’lenerek yazılsın.
- Veya bir kez çalıştırmak üzere basit bir SQL/console tool ile PBKDF2 uyumlu hash üretilip `Users.PasswordHash` güncellensin.

## Sık Kullanılan Kontroller
- DB bağlantısı: `GET /api/v1/DatabaseTest/connection`
- Popüler ürünler (örnek): `GET /api/v1/Product/popular?limit=5`
- Merchant yakınları: `GET /api/v1/geo/merchants/nearby?latitude=40.9817599&longitude=29.1512717&categoryType=2&radius=10`

## Yeniden Çalıştırma
Temiz şekilde başlamak için `Seed-Database.ps1` script’i tüm veriyi siler ve baştan yükler. Kendi ortam bilgilerinize göre parametreleri güncelleyerek tekrar çalıştırabilirsiniz.
