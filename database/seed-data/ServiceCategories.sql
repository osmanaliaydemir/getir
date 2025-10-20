-- =============================================
-- Service Categories Seed Data
-- Ana sayfa kategorileri (Market, Restoran, vs.)
-- =============================================

PRINT 'Inserting Service Categories...';

-- ServiceCategory verilerini ekle
-- Type değerleri: Restaurant=1, Market=2, Pharmacy=3, Water=4, Cafe=5, Bakery=6, Other=99

-- 1. Market (En popüler)
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 2)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Market', 'Gıda ve temel ihtiyaç ürünleri', 2, 1, 1, GETUTCDATE());
    PRINT '✅ Market kategori eklendi';
END

-- 2. Restoran
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 1)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Restoran', 'Yemek siparişi ve teslimatı', 1, 2, 1, GETUTCDATE());
    PRINT '✅ Restoran kategori eklendi';
END

-- 3. Eczane
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 3)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Eczane', 'İlaç ve sağlık ürünleri', 3, 3, 1, GETUTCDATE());
    PRINT '✅ Eczane kategori eklendi';
END

-- 4. Kafe
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 5)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Kafe', 'Kahve ve atıştırmalıklar', 5, 4, 1, GETUTCDATE());
    PRINT '✅ Kafe kategori eklendi';
END

-- 5. Su
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 4)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Su', 'Su teslimatı', 4, 5, 1, GETUTCDATE());
    PRINT '✅ Su kategori eklendi';
END

-- 6. Pastane
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 6)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Pastane', 'Tatlı ve hamur işleri', 6, 6, 1, GETUTCDATE());
    PRINT '✅ Pastane kategori eklendi';
END

-- 7. Diğer
IF NOT EXISTS (SELECT 1 FROM ServiceCategories WHERE Type = 99)
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, Type, DisplayOrder, IsActive, CreatedAt)
    VALUES (NEWID(), 'Diğer', 'Diğer hizmetler', 99, 7, 1, GETUTCDATE());
    PRINT '✅ Diğer kategori eklendi';
END

-- Sonuçları göster
PRINT '';
PRINT '=== Service Categories ===';
SELECT 
    Name AS [Kategori],
    Type AS [Tip],
    DisplayOrder AS [Sıra],
    IsActive AS [Aktif],
    (SELECT COUNT(*) FROM Merchants WHERE ServiceCategoryId = ServiceCategories.Id) AS [Mağaza Sayısı]
FROM ServiceCategories
ORDER BY DisplayOrder;

PRINT '';
PRINT '✅ Service Categories seed data tamamlandı!';
PRINT '';
PRINT '📊 Test Endpoint:';
PRINT 'GET /api/v1/ServiceCategory?page=1&pageSize=100';

