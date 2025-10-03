-- Migration 006: ServiceCategory Type Column
-- Bu migration ServiceCategory tablosuna Type kolonu ekler

-- 1. Type kolonunu ekle
ALTER TABLE ServiceCategories
ADD Type INT NULL;

-- 2. Mevcut verileri güncelle
UPDATE ServiceCategories 
SET Type = 1
WHERE Name = 'Yemek' OR Name = 'Restoran';

UPDATE ServiceCategories 
SET Type = 2
WHERE Name = 'Market';

UPDATE ServiceCategories 
SET Type = 3
WHERE Name = 'Eczane';

UPDATE ServiceCategories 
SET Type = 4
WHERE Name = 'Su';

UPDATE ServiceCategories 
SET Type = 5
WHERE Name = 'Kafe';

UPDATE ServiceCategories 
SET Type = 6
WHERE Name = 'Fırın' OR Name = 'Pastane';

-- 3. Kalan kayıtları default değerle güncelle
UPDATE ServiceCategories 
SET Type = 1
WHERE Type IS NULL;

-- 4. Type kolonunu NOT NULL yap
ALTER TABLE ServiceCategories
ALTER COLUMN Type INT NOT NULL;

-- 5. Index ekle
CREATE INDEX IX_ServiceCategories_Type ON ServiceCategories (Type);

-- 6. Check constraint ekle
ALTER TABLE ServiceCategories
ADD CONSTRAINT CK_ServiceCategories_Type CHECK (Type IN (1, 2, 3, 4, 5, 6));

-- 7. Yeni kategoriler ekle (eğer yoksa)
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Restoran')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Restoran', 'Yemek siparişi ve teslimatı', 1, 1, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Market')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Market', 'Gıda ve temizlik ürünleri', 2, 2, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Eczane')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Eczane', 'İlaç ve sağlık ürünleri', 3, 3, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Su')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Su', 'Su teslimatı', 4, 4, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Kafe')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Kafe', 'Kahve ve atıştırmalık', 5, 5, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Name = 'Pastane')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Pastane', 'Tatlı ve hamur işi', 6, 6, 1, GETUTCDATE());
END

-- Migration tamamlandı
PRINT 'Migration 006: ServiceCategory Type column added successfully';