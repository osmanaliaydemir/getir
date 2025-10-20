-- =============================================
-- Popular Products Mock Data with Reviews
-- Mevcut ürünlere review'lar ekleyip rating hesaplama
-- =============================================


PRINT 'Starting Popular Products seed data...';

-- 1. Mevcut kullanıcıları al (veritabanında zaten varsa kullan)
DECLARE @UserId1 UNIQUEIDENTIFIER;
DECLARE @UserId2 UNIQUEIDENTIFIER;
DECLARE @UserId3 UNIQUEIDENTIFIER;

-- Mevcut Customer rolündeki kullanıcıları al
SELECT TOP 1 @UserId1 = Id FROM Users WHERE Role = 1 AND IsActive = 1 ORDER BY CreatedAt;
SELECT TOP 1 @UserId2 = Id FROM Users WHERE Role = 1 AND IsActive = 1 AND Id != @UserId1 ORDER BY CreatedAt;
SELECT TOP 1 @UserId3 = Id FROM Users WHERE Role = 1 AND IsActive = 1 AND Id NOT IN (@UserId1, @UserId2) ORDER BY CreatedAt;

-- Eğer kullanıcı yoksa, basit test kullanıcıları oluştur
IF @UserId1 IS NULL
BEGIN
    SET @UserId1 = NEWID();
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES (@UserId1, 'testuser1@getir.com', 'AQAAAAIAAYagAAAAEPZXl8Qp1K3w==', 'Test', 'User1', '05551111111', 1, 1, GETUTCDATE());
END

IF @UserId2 IS NULL
BEGIN
    SET @UserId2 = NEWID();
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES (@UserId2, 'testuser2@getir.com', 'AQAAAAIAAYagAAAAEPZXl8Qp1K3w==', 'Test', 'User2', '05552222222', 1, 1, GETUTCDATE());
END

IF @UserId3 IS NULL
BEGIN
    SET @UserId3 = NEWID();
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES (@UserId3, 'testuser3@getir.com', 'AQAAAAIAAYagAAAAEPZXl8Qp1K3w==', 'Test', 'User3', '05553333333', 1, 1, GETUTCDATE());
END

PRINT 'Users ready: ' + CAST(@UserId1 AS NVARCHAR(50));

-- 2. Test siparişleri oluştur (review için gerekli - Delivered olmalı)
DECLARE @OrderId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderId4 UNIQUEIDENTIFIER = NEWID();
DECLARE @OrderId5 UNIQUEIDENTIFIER = NEWID();

-- Migros için siparişler (DeliveryLatitude/Longitude eklendi - Kadıköy koordinatları)
INSERT INTO Orders (Id, OrderNumber, UserId, MerchantId, Status, PaymentMethod, PaymentStatus, DeliveryAddress, DeliveryLatitude, DeliveryLongitude, SubTotal, DeliveryFee, Discount, Total, CreatedAt)
VALUES 
    (@OrderId1, 'TST-001', @UserId1, '77777777-7777-7777-7777-777777777777', 4, 'Cash', 'Completed', 'Test Adres, Kadıköy, İstanbul', 40.9817599, 29.1512717, 100.00, 10.00, 0, 110.00, DATEADD(DAY, -5, GETUTCDATE())),
    (@OrderId2, 'TST-002', @UserId2, '77777777-7777-7777-7777-777777777777', 4, 'Cash', 'Completed', 'Test Adres, Kadıköy, İstanbul', 40.9817599, 29.1512717, 120.00, 10.00, 0, 130.00, DATEADD(DAY, -10, GETUTCDATE())),
    (@OrderId3, 'TST-003', @UserId3, '77777777-7777-7777-7777-777777777777', 4, 'Cash', 'Completed', 'Test Adres, Kadıköy, İstanbul', 40.9817599, 29.1512717, 80.00, 10.00, 0, 90.00, DATEADD(DAY, -15, GETUTCDATE()));

PRINT 'Migros orders created';

-- Burger King için siparişler (DeliveryLatitude/Longitude eklendi - Kadıköy koordinatları)
INSERT INTO Orders (Id, OrderNumber, UserId, MerchantId, Status, PaymentMethod, PaymentStatus, DeliveryAddress, DeliveryLatitude, DeliveryLongitude, SubTotal, DeliveryFee, Discount, Total, CreatedAt)
VALUES 
    (@OrderId4, 'TST-004', @UserId1, '88888888-8888-8888-8888-888888888888', 4, 'Cash', 'Completed', 'Test Adres, Kadıköy, İstanbul', 40.9817599, 29.1512717, 150.00, 15.00, 0, 165.00, DATEADD(DAY, -3, GETUTCDATE())),
    (@OrderId5, 'TST-005', @UserId2, '88888888-8888-8888-8888-888888888888', 4, 'Cash', 'Completed', 'Test Adres, Kadıköy, İstanbul', 40.9817599, 29.1512717, 90.00, 15.00, 0, 105.00, DATEADD(DAY, -7, GETUTCDATE()));

PRINT 'Burger King orders created';

-- 3. OrderLine'lar ekle (popular products hesaplaması için)
-- Elma - En popüler (çok satılmış)
INSERT INTO OrderLines (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
VALUES 
    (NEWID(), @OrderId1, '3856B465-23CC-4640-BCDA-BA0EE1292F8D', 'Elma (1 kg)', 3, 15.50, 46.50),
    (NEWID(), @OrderId2, '3856B465-23CC-4640-BCDA-BA0EE1292F8D', 'Elma (1 kg)', 5, 15.50, 77.50),
    (NEWID(), @OrderId3, '3856B465-23CC-4640-BCDA-BA0EE1292F8D', 'Elma (1 kg)', 2, 15.50, 31.00);

PRINT 'Elma order lines created (10 total sales)';

-- Domates
INSERT INTO OrderLines (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
VALUES 
    (NEWID(), @OrderId1, 'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57', 'Domates (1 kg)', 2, 12.00, 24.00),
    (NEWID(), @OrderId2, 'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57', 'Domates (1 kg)', 4, 12.00, 48.00);

PRINT 'Domates order lines created (6 total sales)';

-- Whopper - En popüler hamburger
INSERT INTO OrderLines (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
VALUES 
    (NEWID(), @OrderId4, '984F8EA8-62D0-45D8-93F7-7715149E2300', 'Whopper', 2, 45.90, 91.80),
    (NEWID(), @OrderId5, '984F8EA8-62D0-45D8-93F7-7715149E2300', 'Whopper', 3, 45.90, 137.70);

PRINT 'Whopper order lines created (5 total sales)';

-- Chicken Royale
INSERT INTO OrderLines (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
VALUES 
    (NEWID(), @OrderId4, '9E8B0C09-934C-4644-A02A-E22625AAFDBE', 'Chicken Royale', 1, 42.90, 42.90);

-- Muz
INSERT INTO OrderLines (Id, OrderId, ProductId, ProductName, Quantity, UnitPrice, TotalPrice)
VALUES 
    (NEWID(), @OrderId1, 'D318CBBE-E874-4872-B2F8-116D78E47A85', 'Muz (1 kg)', 1, 18.90, 18.90);

PRINT 'Other order lines created';

-- 4. ProductReview'lar ekle (Rating hesaplaması için)

-- ELMA REVIEWS (⭐⭐⭐⭐⭐ 4.7 rating)
INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase, CreatedAt)
VALUES 
    (NEWID(), '3856B465-23CC-4640-BCDA-BA0EE1292F8D', @UserId1, @OrderId1, 5, 'Çok taze ve lezzetli! Elma bu kadar güzel olur mu 😍', 1, DATEADD(DAY, -4, GETUTCDATE())),
    (NEWID(), '3856B465-23CC-4640-BCDA-BA0EE1292F8D', @UserId2, @OrderId2, 5, 'Harika kalite, her zaman Migros''tan alıyorum', 1, DATEADD(DAY, -9, GETUTCDATE())),
    (NEWID(), '3856B465-23CC-4640-BCDA-BA0EE1292F8D', @UserId3, @OrderId3, 4, 'Güzel ama biraz küçük geldi', 1, DATEADD(DAY, -14, GETUTCDATE()));

PRINT 'Elma reviews created (3 reviews)';

-- WHOPPER REVIEWS (⭐⭐⭐⭐⭐ 4.5 rating)
INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase, CreatedAt)
VALUES 
    (NEWID(), '984F8EA8-62D0-45D8-93F7-7715149E2300', @UserId1, @OrderId4, 5, 'Burger King''in en iyisi! Doyurucu ve lezzetli 🍔', 1, DATEADD(DAY, -2, GETUTCDATE())),
    (NEWID(), '984F8EA8-62D0-45D8-93F7-7715149E2300', @UserId2, @OrderId5, 4, 'Fena değil ama biraz yağlı', 1, DATEADD(DAY, -6, GETUTCDATE()));

PRINT 'Whopper reviews created (2 reviews)';

-- DOMATES REVIEWS (⭐⭐⭐⭐ 4.0 rating)
INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase, CreatedAt)
VALUES 
    (NEWID(), 'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57', @UserId1, @OrderId1, 4, 'Taze ve güzel, salata yapmak için ideal', 1, DATEADD(DAY, -4, GETUTCDATE())),
    (NEWID(), 'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57', @UserId2, @OrderId2, 4, 'İyi kalite', 1, DATEADD(DAY, -9, GETUTCDATE()));

PRINT 'Domates reviews created (2 reviews)';

-- CHICKEN ROYALE REVIEW (⭐⭐⭐⭐⭐ 5.0 rating)
INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase, CreatedAt)
VALUES 
    (NEWID(), '9E8B0C09-934C-4644-A02A-E22625AAFDBE', @UserId1, @OrderId4, 5, 'Tavuklu burger sevenlere tavsiye ederim! 🍗', 1, DATEADD(DAY, -2, GETUTCDATE()));

PRINT 'Chicken Royale review created (1 review)';

-- MUZ REVIEW (⭐⭐⭐⭐ 4.0 rating)
INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase, CreatedAt)
VALUES 
    (NEWID(), 'D318CBBE-E874-4872-B2F8-116D78E47A85', @UserId1, @OrderId1, 4, 'Taze ve güzel', 1, DATEADD(DAY, -4, GETUTCDATE()));

PRINT 'Muz review created (1 review)';

-- SALATALIK REVIEW (⭐⭐⭐ 3.0 rating)
INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase, CreatedAt)
VALUES 
    (NEWID(), 'EF45B4D3-432E-4653-B985-6F072D95B5E2', @UserId3, @OrderId3, 3, 'Normal, beklediğim gibi', 1, DATEADD(DAY, -14, GETUTCDATE()));

PRINT 'Salatalık review created (1 review)';

-- 5. Rating'leri hesapla (stored procedure kullanarak)
PRINT '';
PRINT 'Calculating ratings...';

EXEC sp_RecalculateProductRating '3856B465-23CC-4640-BCDA-BA0EE1292F8D'; -- Elma: 4.67
EXEC sp_RecalculateProductRating '984F8EA8-62D0-45D8-93F7-7715149E2300'; -- Whopper: 4.50
EXEC sp_RecalculateProductRating 'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57'; -- Domates: 4.00
EXEC sp_RecalculateProductRating '9E8B0C09-934C-4644-A02A-E22625AAFDBE'; -- Chicken Royale: 5.00
EXEC sp_RecalculateProductRating 'D318CBBE-E874-4872-B2F8-116D78E47A85'; -- Muz: 4.00
EXEC sp_RecalculateProductRating 'EF45B4D3-432E-4653-B985-6F072D95B5E2'; -- Salatalık: 3.00

PRINT 'Ratings calculated successfully!';

-- 6. Sonuçları göster
PRINT '';
PRINT '=== Product Ratings Summary ===';
SELECT 
    p.Name AS [Ürün Adı],
    p.Rating AS [Rating],
    p.ReviewCount AS [Review Sayısı],
    p.Price AS [Fiyat],
    p.DiscountedPrice AS [İndirimli Fiyat]
FROM Products p
WHERE p.Id IN (
    '3856B465-23CC-4640-BCDA-BA0EE1292F8D',
    '984F8EA8-62D0-45D8-93F7-7715149E2300',
    'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57',
    '9E8B0C09-934C-4644-A02A-E22625AAFDBE',
    'D318CBBE-E874-4872-B2F8-116D78E47A85',
    'EF45B4D3-432E-4653-B985-6F072D95B5E2',
    '50DC84B8-DC10-4C96-A783-BAB7EBD8FFBC'
)
ORDER BY p.Rating DESC, p.ReviewCount DESC;

-- 7. Popular Products endpoint simulation
PRINT '';
PRINT '=== Popular Products Ranking (by sales + rating) ===';

SELECT TOP 10
    p.Name AS [Ürün Adı],
    p.Rating AS [Rating],
    p.ReviewCount AS [Reviews],
    ISNULL(SUM(ol.Quantity), 0) AS [Satış Adedi],
    p.Price AS [Fiyat],
    p.MerchantId
FROM Products p
LEFT JOIN OrderLines ol ON ol.ProductId = p.Id
LEFT JOIN Orders o ON o.Id = ol.OrderId 
    AND o.CreatedAt >= DATEADD(DAY, -30, GETUTCDATE()) 
    AND o.Status != 6 -- Not Cancelled
WHERE p.IsActive = 1 AND p.IsAvailable = 1
GROUP BY p.Id, p.Name, p.Rating, p.ReviewCount, p.Price, p.MerchantId
ORDER BY 
    ISNULL(SUM(ol.Quantity), 0) DESC, -- Satışa göre
    ISNULL(p.Rating, 0) DESC,         -- Rating'e göre
    ISNULL(p.ReviewCount, 0) DESC;    -- Review sayısına göre

PRINT '';
PRINT '✅ Seed data tamamlandı!';
PRINT '';
PRINT '📊 Test Endpoint:';
PRINT 'GET /api/v1/Product/popular?limit=5';
PRINT '';
PRINT '💡 Beklenen Sıralama:';
PRINT '1. Elma (10 satış, ⭐4.67)';
PRINT '2. Domates (6 satış, ⭐4.00)';
PRINT '3. Whopper (5 satış, ⭐4.50)';
PRINT '4. Chicken Royale (1 satış, ⭐5.00)';
PRINT '5. Muz (1 satış, ⭐4.00)';
