-- =============================================
-- Getir Clone - Seed Data for Testing/Demo
-- =============================================

USE GetirDb;
GO

-- =============================================
-- TEST USERS
-- =============================================

-- Password: Test123!
-- Hashed using PBKDF2
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, IsEmailVerified, IsActive, CreatedAt)
VALUES 
    (NEWID(), 'customer@getir.com', 'test_hash_here', 'Test', 'Customer', '+905551234567', 1, 1, GETUTCDATE()),
    (NEWID(), 'merchant@getir.com', 'test_hash_here', 'Merchant', 'Owner', '+905557654321', 1, 1, GETUTCDATE());

-- =============================================
-- SAMPLE CATEGORIES
-- =============================================

DECLARE @categoryMarket UNIQUEIDENTIFIER = NEWID();
DECLARE @categoryRestaurant UNIQUEIDENTIFIER = NEWID();

INSERT INTO Categories (Id, Name, Description, ImageUrl, DisplayOrder, IsActive, CreatedAt)
VALUES 
    (@categoryMarket, 'Market', 'Günlük ihtiyaç ürünleri', 'https://cdn.getir.com/cat/market.jpg', 1, 1, GETUTCDATE()),
    (@categoryRestaurant, 'Restoran', 'Yemek siparişleri', 'https://cdn.getir.com/cat/restaurant.jpg', 2, 1, GETUTCDATE()),
    (NEWID(), 'Su & İçecek', 'Su ve içecekler', 'https://cdn.getir.com/cat/drinks.jpg', 3, 1, GETUTCDATE()),
    (NEWID(), 'Meyve & Sebze', 'Taze meyve ve sebzeler', 'https://cdn.getir.com/cat/fresh.jpg', 4, 1, GETUTCDATE());

-- =============================================
-- SAMPLE MERCHANTS
-- =============================================

DECLARE @merchantMigros UNIQUEIDENTIFIER = NEWID();
DECLARE @merchantCarrefour UNIQUEIDENTIFIER = NEWID();
DECLARE @merchantBurgerKing UNIQUEIDENTIFIER = NEWID();

INSERT INTO Merchants (Id, Name, Description, CategoryId, LogoUrl, Address, Latitude, Longitude, PhoneNumber, Email, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime, Rating, IsActive, IsOpen, CreatedAt)
VALUES 
    (@merchantMigros, 'Migros Kadıköy', 'Taze ürünler ve market alışverişi', @categoryMarket, 'https://cdn.getir.com/merchants/migros.jpg', 'Kadıköy, İstanbul', 40.9897, 29.0257, '+902161234567', 'kadikoy@migros.com.tr', 50, 15, 30, 4.5, 1, 1, GETUTCDATE()),
    (@merchantCarrefour, 'CarrefourSA Bostancı', 'Kaliteli ürünler uygun fiyatlarla', @categoryMarket, 'https://cdn.getir.com/merchants/carrefour.jpg', 'Bostancı, İstanbul', 40.9645, 29.0854, '+902163456789', 'bostanci@carrefour.com.tr', 75, 20, 35, 4.3, 1, 1, GETUTCDATE()),
    (@merchantBurgerKing, 'Burger King Kadıköy', 'Lezzetli burgerler hızlı teslimat', @categoryRestaurant, 'https://cdn.getir.com/merchants/bk.jpg', 'Kadıköy Merkez, İstanbul', 40.9900, 29.0260, '+902169876543', 'kadikoy@burgerking.com.tr', 40, 10, 25, 4.7, 1, 1, GETUTCDATE());

-- =============================================
-- SAMPLE PRODUCTS
-- =============================================

-- Migros Products
INSERT INTO Products (Id, MerchantId, CategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, CreatedAt)
VALUES 
    (NEWID(), @merchantMigros, @categoryMarket, 'Süt 1L', 'Tam yağlı süt', 'https://cdn.getir.com/products/milk.jpg', 35.50, 29.90, 100, 'adet', 1, 1, 1, GETUTCDATE()),
    (NEWID(), @merchantMigros, @categoryMarket, 'Ekmek', 'Taze günlük ekmek', 'https://cdn.getir.com/products/bread.jpg', 15.00, NULL, 150, 'adet', 1, 1, 2, GETUTCDATE()),
    (NEWID(), @merchantMigros, @categoryMarket, 'Yumurta 10\'lu', 'Organik köy yumurtası', 'https://cdn.getir.com/products/eggs.jpg', 45.00, 39.90, 80, 'adet', 1, 1, 3, GETUTCDATE()),
    (NEWID(), @merchantMigros, @categoryMarket, 'Su 5L', 'Doğal kaynak suyu', 'https://cdn.getir.com/products/water.jpg', 12.50, NULL, 200, 'adet', 1, 1, 4, GETUTCDATE());

-- Carrefour Products
INSERT INTO Products (Id, MerchantId, CategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, CreatedAt)
VALUES 
    (NEWID(), @merchantCarrefour, @categoryMarket, 'Zeytinyağı 1L', 'Sızma zeytinyağı', 'https://cdn.getir.com/products/olive-oil.jpg', 180.00, 159.90, 50, 'adet', 1, 1, 1, GETUTCDATE()),
    (NEWID(), @merchantCarrefour, @categoryMarket, 'Peynir 500gr', 'Beyaz peynir tam yağlı', 'https://cdn.getir.com/products/cheese.jpg', 95.00, NULL, 60, 'adet', 1, 1, 2, GETUTCDATE());

-- Burger King Products
INSERT INTO Products (Id, MerchantId, CategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, CreatedAt)
VALUES 
    (NEWID(), @merchantBurgerKing, @categoryRestaurant, 'Whopper Menü', 'Whopper burger + patates + içecek', 'https://cdn.getir.com/products/whopper.jpg', 120.00, 99.90, 999, 'adet', 1, 1, 1, GETUTCDATE()),
    (NEWID(), @merchantBurgerKing, @categoryRestaurant, 'King Chicken Menü', 'Tavuk burger menü', 'https://cdn.getir.com/products/chicken.jpg', 95.00, NULL, 999, 'adet', 1, 1, 2, GETUTCDATE()),
    (NEWID(), @merchantBurgerKing, @categoryRestaurant, 'Patates (Orta)', 'Çıtır patates orta boy', 'https://cdn.getir.com/products/fries.jpg', 35.00, NULL, 999, 'adet', 1, 1, 3, GETUTCDATE());

-- =============================================
-- SAMPLE COUPONS
-- =============================================

INSERT INTO Coupons (Id, Code, Title, Description, DiscountType, DiscountValue, MinimumOrderAmount, MaximumDiscountAmount, StartDate, EndDate, UsageLimit, UsageCount, IsActive, CreatedAt)
VALUES 
    (NEWID(), 'WELCOME50', 'Hoş Geldin İndirimi', 'İlk siparişinize %50 indirim', 'Percentage', 50, 50, 50, GETUTCDATE(), DATEADD(MONTH, 3, GETUTCDATE()), 1000, 0, 1, GETUTCDATE()),
    (NEWID(), 'SAVE25', '25 TL İndirim', 'Tüm siparişlerde 25 TL indirim', 'FixedAmount', 25, 100, NULL, GETUTCDATE(), DATEADD(MONTH, 1, GETUTCDATE()), NULL, 0, 1, GETUTCDATE()),
    (NEWID(), 'SUMMER20', 'Yaz İndirimi', '%20 indirim', 'Percentage', 20, 75, 100, GETUTCDATE(), DATEADD(MONTH, 2, GETUTCDATE()), 500, 0, 1, GETUTCDATE());

-- =============================================
-- SAMPLE CAMPAIGNS
-- =============================================

INSERT INTO Campaigns (Id, Title, Description, ImageUrl, MerchantId, DiscountType, DiscountValue, StartDate, EndDate, IsActive, DisplayOrder, CreatedAt)
VALUES 
    (NEWID(), 'Migros''ta %30 İndirim', 'Migros ürünlerinde %30 indirim fırsatı', 'https://cdn.getir.com/campaigns/migros-campaign.jpg', @merchantMigros, 'Percentage', 30, GETUTCDATE(), DATEADD(DAY, 30, GETUTCDATE()), 1, 1, GETUTCDATE()),
    (NEWID(), 'Ücretsiz Teslimat Kampanyası', 'Tüm siparişlerde ücretsiz teslimat', 'https://cdn.getir.com/campaigns/free-delivery.jpg', NULL, 'FixedAmount', 0, GETUTCDATE(), DATEADD(DAY, 15, GETUTCDATE()), 1, 2, GETUTCDATE());

GO

PRINT 'Seed data inserted successfully!';
PRINT '';
PRINT '✅ Test Users created (customer@getir.com, merchant@getir.com)';
PRINT '✅ 4 Categories created';
PRINT '✅ 3 Merchants created (Migros, CarrefourSA, Burger King)';
PRINT '✅ 9 Products created';
PRINT '✅ 3 Coupons created (WELCOME50, SAVE25, SUMMER20)';
PRINT '✅ 2 Campaigns created';
PRINT '';
PRINT '🎉 Ready for testing with Postman!';
