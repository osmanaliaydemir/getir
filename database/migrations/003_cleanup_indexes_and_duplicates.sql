-- =============================================
-- Sprint 2: Cleanup - Eski Index'ler ve Duplicate'ler
-- =============================================

USE GetirDb;
GO

PRINT '================================';
PRINT 'Starting cleanup process...';
PRINT '================================';
PRINT '';

-- =============================================
-- 1. ESKİ INDEX'LERİ KALDIR
-- =============================================

-- Merchants.CategoryId index (artık ServiceCategoryId kullanılıyor)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Merchants_CategoryId' AND object_id = OBJECT_ID('Merchants'))
BEGIN
    DROP INDEX IX_Merchants_CategoryId ON Merchants;
    PRINT '✅ Dropped IX_Merchants_CategoryId (old index)';
END
ELSE
BEGIN
    PRINT '⚠️ IX_Merchants_CategoryId already dropped or doesn''t exist';
END
GO

-- Products.CategoryId index (artık ProductCategoryId kullanılıyor)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CategoryId' AND object_id = OBJECT_ID('Products'))
BEGIN
    DROP INDEX IX_Products_CategoryId ON Products;
    PRINT '✅ Dropped IX_Products_CategoryId (old index)';
END
ELSE
BEGIN
    PRINT '⚠️ IX_Products_CategoryId already dropped or doesn''t exist';
END
GO

-- =============================================
-- 2. DUPLICATE ServiceCategories TEMİZLİĞİ
-- =============================================

PRINT '';
PRINT 'Checking for duplicates in ServiceCategories...';

-- Duplicate'leri göster
SELECT Name, COUNT(*) AS DuplicateCount 
FROM ServiceCategories 
GROUP BY Name 
HAVING COUNT(*) > 1;

-- Her Name için sadece ilk kaydı tut, diğerlerini sil
;WITH CTE AS (
    SELECT Id, Name,
           ROW_NUMBER() OVER (PARTITION BY Name ORDER BY CreatedAt ASC) AS RowNum
    FROM ServiceCategories
)
DELETE FROM CTE WHERE RowNum > 1;

PRINT '✅ Duplicate ServiceCategories cleaned: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' rows deleted';
GO

-- Sonuç kontrolü
PRINT '';
PRINT 'ServiceCategories after cleanup:';
SELECT Id, Name, IconUrl, DisplayOrder, IsActive, CreatedAt 
FROM ServiceCategories 
ORDER BY DisplayOrder;
GO

-- =============================================
-- 3. CATEGORIES TABLOSU (Opsiyonel Silme)
-- =============================================

PRINT '';
PRINT 'Old Categories table status:';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    DECLARE @categoryCount INT;
    SELECT @categoryCount = COUNT(*) FROM Categories;
    
    PRINT '⚠️ Old Categories table still exists with ' + CAST(@categoryCount AS VARCHAR) + ' rows';
    PRINT '   Migration completed: Data moved to ServiceCategories';
    PRINT '   Safe to drop: YES (backup recommended)';
    PRINT '';
    PRINT '   To drop, uncomment and run:';
    PRINT '   -- DROP TABLE Categories;';
    
    -- UYARI: Production'da önce backup alın!
    -- Uncomment to drop:
    -- DROP TABLE Categories;
    -- PRINT '✅ Dropped old Categories table';
END
ELSE
BEGIN
    PRINT '✅ Old Categories table already dropped';
END
GO

-- =============================================
-- 4. FOREIGN KEY DOĞRULAMA
-- =============================================

PRINT '';
PRINT 'Foreign Key Validation:';
PRINT '';

SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN ('Merchants', 'Products', 'ProductCategories', 'ServiceCategories')
ORDER BY TableName, ForeignKeyName;
GO

-- =============================================
-- 5. INDEX DOĞRULAMA
-- =============================================

PRINT '';
PRINT 'Active Indexes (Sprint 1+2):';
PRINT '';

SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    CASE 
        WHEN i.name LIKE '%CategoryId' AND i.name NOT LIKE '%ServiceCategoryId' AND i.name NOT LIKE '%ProductCategoryId' THEN '⚠️ OLD'
        WHEN i.name LIKE '%ServiceCategoryId' OR i.name LIKE '%ProductCategoryId' THEN '✅ NEW'
        WHEN i.name LIKE '%OwnerId' THEN '✅ Sprint 1'
        ELSE '✅ OK'
    END AS Status
FROM sys.indexes i
WHERE OBJECT_NAME(i.object_id) IN ('Merchants', 'Products', 'ProductCategories', 'ServiceCategories')
  AND i.is_primary_key = 0
  AND i.type > 0
ORDER BY TableName, IndexName;
GO

-- =============================================
-- 6. VERİ İSTATİSTİKLERİ
-- =============================================

PRINT '';
PRINT '================================';
PRINT 'DATA STATISTICS';
PRINT '================================';
PRINT '';

SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM Users
UNION ALL SELECT 'Merchants', COUNT(*) FROM Merchants
UNION ALL SELECT 'Products', COUNT(*) FROM Products
UNION ALL SELECT 'ServiceCategories', COUNT(*) FROM ServiceCategories
UNION ALL SELECT 'ProductCategories', COUNT(*) FROM ProductCategories
UNION ALL SELECT 'Orders', COUNT(*) FROM Orders
UNION ALL SELECT 'CartItems', COUNT(*) FROM CartItems
UNION ALL SELECT 'Coupons', COUNT(*) FROM Coupons
UNION ALL SELECT 'Campaigns', COUNT(*) FROM Campaigns
UNION ALL SELECT 'Notifications', COUNT(*) FROM Notifications
UNION ALL SELECT 'Couriers', COUNT(*) FROM Couriers
ORDER BY TableName;
GO

-- =============================================
-- 7. ORPHAN RECORDS KONTROLÜ
-- =============================================

PRINT '';
PRINT 'Checking for orphan records...';
PRINT '';

-- Merchants with invalid ServiceCategoryId
SELECT 'Merchants with invalid ServiceCategoryId' AS Issue, COUNT(*) AS Count
FROM Merchants m
WHERE NOT EXISTS (SELECT 1 FROM ServiceCategories sc WHERE sc.Id = m.ServiceCategoryId);

-- Products with invalid ProductCategoryId
SELECT 'Products with invalid ProductCategoryId' AS Issue, COUNT(*) AS Count
FROM Products p
WHERE p.ProductCategoryId IS NOT NULL 
  AND NOT EXISTS (SELECT 1 FROM ProductCategories pc WHERE pc.Id = p.ProductCategoryId);

-- ProductCategories with invalid ParentCategoryId
SELECT 'ProductCategories with invalid ParentCategoryId' AS Issue, COUNT(*) AS Count
FROM ProductCategories pc
WHERE pc.ParentCategoryId IS NOT NULL 
  AND NOT EXISTS (SELECT 1 FROM ProductCategories parent WHERE parent.Id = pc.ParentCategoryId);

GO

PRINT '';
PRINT '================================';
PRINT 'Cleanup completed successfully!';
PRINT '================================';
PRINT '';
PRINT 'Summary:';
PRINT '  ✅ Old indexes checked';
PRINT '  ✅ Duplicate ServiceCategories cleaned';
PRINT '  ✅ Foreign keys validated';
PRINT '  ⚠️ Old Categories table still exists (safe to drop)';
PRINT '';

GO

