-- =============================================
-- Merchants for Service Categories
-- Her kategoriye merchant ekleme
-- =============================================

PRINT 'Adding merchants for categories...';

-- ServiceCategory ID'lerini bul
DECLARE @MarketCategoryId UNIQUEIDENTIFIER;
DECLARE @RestaurantCategoryId UNIQUEIDENTIFIER;
DECLARE @PharmacyCategoryId UNIQUEIDENTIFIER;
DECLARE @CafeCategoryId UNIQUEIDENTIFIER;

SELECT @MarketCategoryId = Id FROM ServiceCategories WHERE Type = 2; -- Market
SELECT @RestaurantCategoryId = Id FROM ServiceCategories WHERE Type = 1; -- Restaurant
SELECT @PharmacyCategoryId = Id FROM ServiceCategories WHERE Type = 3; -- Pharmacy
SELECT @CafeCategoryId = Id FROM ServiceCategories WHERE Type = 5; -- Cafe

PRINT 'Category IDs:';
PRINT '  Market: ' + CAST(@MarketCategoryId AS NVARCHAR(50));
PRINT '  Restaurant: ' + CAST(@RestaurantCategoryId AS NVARCHAR(50));

-- Test koordinatları (Kadıköy)
DECLARE @TestLat DECIMAL(10, 7) = 40.9817599;
DECLARE @TestLon DECIMAL(10, 7) = 29.1512717;

-- =============================================
-- MARKET KATEGORİSİ
-- =============================================

-- 1. Migros
IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'Migros Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        '77777777-7777-7777-7777-777777777777',
        '33333333-3333-3333-3333-333333333333', -- Test user as owner
        @MarketCategoryId,
        'Migros Kadıköy',
        'Günlük taze gıda ve temel ihtiyaçlar',
        'Kadıköy, İstanbul',
        @TestLat + 0.001,
        @TestLon + 0.001,
        '+905551234567',
        'migros@example.com',
        1,
        50.00,
        9.99,
        25,
        4.5,
        1234,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ Migros eklendi';
END

-- 2. A101
IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'A101 Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        NEWID(),
        '33333333-3333-3333-3333-333333333333',
        @MarketCategoryId,
        'A101 Kadıköy',
        'Uygun fiyatlı market alışverişi',
        'Kadıköy, İstanbul',
        @TestLat + 0.002,
        @TestLon + 0.002,
        '+905551234568',
        'a101@example.com',
        1,
        30.00,
        7.99,
        20,
        4.3,
        856,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ A101 eklendi';
END

-- 3. Şok Market
IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'Şok Market Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        NEWID(),
        '33333333-3333-3333-3333-333333333333',
        @MarketCategoryId,
        'Şok Market Kadıköy',
        'Kaliteli ürünler uygun fiyatlarla',
        'Kadıköy, İstanbul',
        @TestLat + 0.0015,
        @TestLon + 0.0015,
        '+905551234569',
        'sok@example.com',
        1,
        35.00,
        8.99,
        22,
        4.4,
        678,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ Şok Market eklendi';
END

-- =============================================
-- RESTORAN KATEGORİSİ
-- =============================================

-- 1. Burger King
IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'Burger King Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        '88888888-8888-8888-8888-888888888888',
        '33333333-3333-3333-3333-333333333333',
        @RestaurantCategoryId,
        'Burger King Kadıköy',
        'Flame-grilled burgers',
        'Kadıköy, İstanbul',
        @TestLat + 0.003,
        @TestLon + 0.003,
        '+905552345678',
        'bk@example.com',
        1,
        40.00,
        12.99,
        35,
        4.6,
        2345,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ Burger King eklendi';
END

-- 2. Pizza Hut
IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'Pizza Hut Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        NEWID(),
        '33333333-3333-3333-3333-333333333333',
        @RestaurantCategoryId,
        'Pizza Hut Kadıköy',
        'Lezzetli pizzalar',
        'Kadıköy, İstanbul',
        @TestLat + 0.0025,
        @TestLon + 0.0025,
        '+905552345679',
        'pizzahut@example.com',
        1,
        50.00,
        14.99,
        40,
        4.5,
        1876,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ Pizza Hut eklendi';
END

-- =============================================
-- ECZANE KATEGORİSİ
-- =============================================

IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'Eczane Plus Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        NEWID(),
        '33333333-3333-3333-3333-333333333333',
        @PharmacyCategoryId,
        'Eczane Plus Kadıköy',
        '7/24 ilaç ve sağlık ürünleri',
        'Kadıköy, İstanbul',
        @TestLat + 0.0018,
        @TestLon + 0.0018,
        '+905553456789',
        'eczaneplus@example.com',
        1,
        20.00,
        5.99,
        15,
        4.8,
        543,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ Eczane Plus eklendi';
END

-- =============================================
-- KAFE KATEGORİSİ
-- =============================================

IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Name = 'Starbucks Kadıköy')
BEGIN
    INSERT INTO Merchants (
        Id, OwnerId, ServiceCategoryId, Name, Description, Address, Latitude, Longitude,
        PhoneNumber, Email, IsActive, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime,
        Rating, TotalReviews, IsOpen, IsBusy, CreatedAt
    ) VALUES (
        NEWID(),
        '33333333-3333-3333-3333-333333333333',
        @CafeCategoryId,
        'Starbucks Kadıköy',
        'Premium kahve ve içecekler',
        'Kadıköy, İstanbul',
        @TestLat + 0.0012,
        @TestLon + 0.0012,
        '+905554567890',
        'starbucks@example.com',
        1,
        30.00,
        8.99,
        20,
        4.7,
        3456,
        1,
        0,
        GETUTCDATE()
    );
    PRINT '✅ Starbucks eklendi';
END

-- Sonuçları göster
PRINT '';
PRINT '=== Merchants by Category ===';
SELECT 
    sc.Name AS [Kategori],
    COUNT(m.Id) AS [Merchant Sayısı],
    STRING_AGG(m.Name, ', ') AS [Merchantlar]
FROM ServiceCategories sc
LEFT JOIN Merchants m ON m.ServiceCategoryId = sc.Id
GROUP BY sc.Name, sc.DisplayOrder
ORDER BY sc.DisplayOrder;

PRINT '';
PRINT '✅ Merchants seed data tamamlandı!';
PRINT '';
PRINT '📊 Test:';
PRINT 'GET /api/v1/geo/merchants/nearby?latitude=40.9817599&longitude=29.1512717&categoryType=2&radius=10';

