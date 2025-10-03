-- Migration 006: ServiceCategory Type Column
-- Bu migration ServiceCategory tablosuna Type kolonu ekler ve enum değerlerini tanımlar

-- 1. Önce Type kolonunu ekle (NULL olarak)
ALTER TABLE ServiceCategories
ADD Type INT NULL;

-- 2. Mevcut verileri güncelle (Name'e göre)
UPDATE ServiceCategories 
SET Type = CASE 
    WHEN Name = 'Yemek' OR Name = 'Restoran' THEN 1
    WHEN Name = 'Market' THEN 2
    WHEN Name = 'Eczane' THEN 3
    WHEN Name = 'Su' THEN 4
    WHEN Name = 'Kafe' THEN 5
    WHEN Name = 'Fırın' OR Name = 'Pastane' THEN 6
    ELSE 1 -- Default to Restaurant
END;

-- 3. Type kolonunu NOT NULL yap
ALTER TABLE ServiceCategories
ALTER COLUMN Type INT NOT NULL;

-- 4. Index ekle (performans için)
CREATE INDEX IX_ServiceCategories_Type ON ServiceCategories (Type);

-- 5. Seed data - Varsayılan kategoriler
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Restoran')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Restoran', 'Yemek siparişi ve teslimatı', 1, '/images/categories/restaurant.jpg', '/icons/restaurant.svg', 1, 1, GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Market')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Market', 'Gıda ve temizlik ürünleri', 2, '/images/categories/market.jpg', '/icons/market.svg', 2, 1, GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Eczane')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Eczane', 'İlaç ve sağlık ürünleri', 3, '/images/categories/pharmacy.jpg', '/icons/pharmacy.svg', 3, 1, GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Su')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Su', 'Su teslimatı', 4, '/images/categories/water.jpg', '/icons/water.svg', 4, 1, GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Kafe')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Kafe', 'Kahve ve atıştırmalık', 5, '/images/categories/cafe.jpg', '/icons/cafe.svg', 5, 1, GETUTCDATE(), GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Pastane')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'Pastane', 'Tatlı ve hamur işi', 6, '/images/categories/bakery.jpg', '/icons/bakery.svg', 6, 1, GETUTCDATE(), GETUTCDATE());
END

-- 6. Check constraint ekle (enum değerlerini sınırla)
ALTER TABLE ServiceCategories
ADD CONSTRAINT CK_ServiceCategories_Type CHECK (Type IN (1, 2, 3, 4, 5, 6));

-- 7. Performance için composite index
CREATE INDEX IX_ServiceCategories_Type_IsActive ON ServiceCategories (Type, IsActive);

-- Migration tamamlandı
PRINT 'Migration 006: ServiceCategory Type column added successfully';