-- =============================================
-- Core Products Seed Data
-- Referenced by PopularProductsWithReviews.sql (fixed GUIDs)
-- =============================================

PRINT 'Inserting core product categories...';

-- Ensure some ProductCategories exist (MerchantId is required)
DECLARE @MigrosId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777';
DECLARE @BurgerKingId UNIQUEIDENTIFIER = '88888888-8888-8888-8888-888888888888';

IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Meyve & Sebze' AND MerchantId = @MigrosId)
BEGIN
    INSERT INTO ProductCategories (Id, MerchantId, Name, Description, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), @MigrosId, 'Meyve & Sebze', 'Taze meyve ve sebzeler', 1, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Burger' AND MerchantId = @BurgerKingId)
BEGIN
    INSERT INTO ProductCategories (Id, MerchantId, Name, Description, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), @BurgerKingId, 'Burger', 'Burger çeşitleri', 2, 1, GETUTCDATE());
END

DECLARE @FruitsCategoryId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Meyve & Sebze');
DECLARE @BurgerCategoryId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Burger');

PRINT 'Inserting core products...';

-- Market: Migros (must exist with fixed Id '77777777-7777-7777-7777-777777777777')
-- Restaurant: Burger King (must exist with fixed Id '88888888-8888-8888-8888-888888888888')

-- ELMA (1 kg)
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = '3856B465-23CC-4640-BCDA-BA0EE1292F8D')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('3856B465-23CC-4640-BCDA-BA0EE1292F8D', '77777777-7777-7777-7777-777777777777', @FruitsCategoryId, 'Elma (1 kg)', 'Taze elma', NULL, 15.50, NULL, 1000, 'kg', 1, 1, 1, NULL, GETUTCDATE());
END

-- DOMATES (1 kg)
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'AF09ADEB-D99F-4AF5-B01C-6AC27853DC57')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('AF09ADEB-D99F-4AF5-B01C-6AC27853DC57', '77777777-7777-7777-7777-777777777777', @FruitsCategoryId, 'Domates (1 kg)', 'Taze domates', NULL, 12.00, NULL, 1000, 'kg', 1, 1, 2, NULL, GETUTCDATE());
END

-- SALATALIK (1 kg)
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'EF45B4D3-432E-4653-B985-6F072D95B5E2')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('EF45B4D3-432E-4653-B985-6F072D95B5E2', '77777777-7777-7777-7777-777777777777', @FruitsCategoryId, 'Salatalık (1 kg)', 'Taze salatalık', NULL, 10.00, NULL, 1000, 'kg', 1, 1, 3, NULL, GETUTCDATE());
END

-- MUZ (1 kg)
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = 'D318CBBE-E874-4872-B2F8-116D78E47A85')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('D318CBBE-E874-4872-B2F8-116D78E47A85', '77777777-7777-7777-7777-777777777777', @FruitsCategoryId, 'Muz (1 kg)', 'Taze muz', NULL, 18.90, NULL, 1000, 'kg', 1, 1, 4, NULL, GETUTCDATE());
END

-- Burger King Ürünleri
-- WHOPPER
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = '984F8EA8-62D0-45D8-93F7-7715149E2300')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('984F8EA8-62D0-45D8-93F7-7715149E2300', '88888888-8888-8888-8888-888888888888', @BurgerCategoryId, 'Whopper', 'Alevde ızgara', NULL, 45.90, NULL, 1000, 'adet', 1, 1, 1, NULL, GETUTCDATE());
END

-- CHICKEN ROYALE
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = '9E8B0C09-934C-4644-A02A-E22625AAFDBE')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('9E8B0C09-934C-4644-A02A-E22625AAFDBE', '88888888-8888-8888-8888-888888888888', @BurgerCategoryId, 'Chicken Royale', 'Pane tavuklu burger', NULL, 42.90, NULL, 1000, 'adet', 1, 1, 2, NULL, GETUTCDATE());
END

-- Ek örnek ürün (gerekirse)
IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = '50DC84B8-DC10-4C96-A783-BAB7EBD8FFBC')
BEGIN
    INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, ExternalId, CreatedAt)
    VALUES ('50DC84B8-DC10-4C96-A783-BAB7EBD8FFBC', '77777777-7777-7777-7777-777777777777', @FruitsCategoryId, 'Portakal (1 kg)', 'Taze portakal', NULL, 19.90, NULL, 1000, 'kg', 1, 1, 5, NULL, GETUTCDATE());
END

PRINT '✅ Core products inserted.';

-- Show summary
PRINT '';
PRINT '=== Core Products Summary ===';
SELECT TOP 50 p.Id, p.Name, m.Name AS Merchant, p.Price, p.StockQuantity
FROM Products p
JOIN Merchants m ON m.Id = p.MerchantId
ORDER BY p.DisplayOrder, p.CreatedAt DESC;

PRINT '';
PRINT '✅ CoreProducts.sql completed.';


