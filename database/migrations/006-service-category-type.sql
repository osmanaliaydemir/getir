-- Migration: 006-service-category-type.sql
-- Description: ServiceCategory tablosuna Type kolonu ekleme ve ServiceCategoryType enum desteği

-- 1. ServiceCategory tablosuna Type kolonu ekle
ALTER TABLE ServiceCategories 
ADD Type INT NOT NULL DEFAULT 1;

-- 2. Mevcut verileri güncelle (varsayılan olarak Restaurant)
UPDATE ServiceCategories 
SET Type = 1 
WHERE Type IS NULL OR Type = 0;

-- 3. Type kolonunu NOT NULL yap
ALTER TABLE ServiceCategories 
ALTER COLUMN Type INT NOT NULL;

-- 4. Index ekle (performans için)
CREATE INDEX IX_ServiceCategories_Type ON ServiceCategories (Type);

-- 5. Seed data - Varsayılan kategoriler
INSERT INTO ServiceCategories (Id, Name, Description, Type, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
VALUES 
    (NEWID(), 'Restoran', 'Yemek siparişi ve teslimatı', 1, '/images/categories/restaurant.jpg', '/icons/restaurant.svg', 1, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Market', 'Gıda ve temizlik ürünleri', 2, '/images/categories/market.jpg', '/icons/market.svg', 2, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Eczane', 'İlaç ve sağlık ürünleri', 3, '/images/categories/pharmacy.jpg', '/icons/pharmacy.svg', 3, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Su', 'Su teslimatı', 4, '/images/categories/water.jpg', '/icons/water.svg', 4, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Kafe', 'Kahve ve atıştırmalık', 5, '/images/categories/cafe.jpg', '/icons/cafe.svg', 5, 1, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), 'Pastane', 'Tatlı ve hamur işi', 6, '/images/categories/bakery.jpg', '/icons/bakery.svg', 6, 1, GETUTCDATE(), GETUTCDATE())
ON CONFLICT (Name) DO NOTHING;

-- 6. Merchant tablosundaki mevcut ServiceCategoryId'leri kontrol et ve güncelle
-- (Eğer mevcut merchant'lar varsa, onları uygun kategoriye atayın)

-- 7. Check constraint ekle (enum değerlerini sınırla)
ALTER TABLE ServiceCategories 
ADD CONSTRAINT CK_ServiceCategories_Type 
CHECK (Type IN (1, 2, 3, 4, 5, 6, 99));

-- 8. Foreign key constraint'leri kontrol et
-- (ServiceCategory tablosu referans eden tablolar varsa)

-- 9. Performance için composite index
CREATE INDEX IX_ServiceCategories_Type_IsActive ON ServiceCategories (Type, IsActive);

-- 10. View oluştur (kategori tiplerini daha kolay sorgulamak için)
CREATE OR ALTER VIEW vw_ServiceCategoriesWithType AS
SELECT 
    sc.Id,
    sc.Name,
    sc.Description,
    sc.Type,
    CASE sc.Type
        WHEN 1 THEN 'Restoran'
        WHEN 2 THEN 'Market'
        WHEN 3 THEN 'Eczane'
        WHEN 4 THEN 'Su'
        WHEN 5 THEN 'Kafe'
        WHEN 6 THEN 'Pastane'
        WHEN 99 THEN 'Diğer'
        ELSE 'Bilinmeyen'
    END AS TypeName,
    CASE sc.Type
        WHEN 1 THEN 'Yemek siparişi ve teslimatı'
        WHEN 2 THEN 'Gıda ve temizlik ürünleri'
        WHEN 3 THEN 'İlaç ve sağlık ürünleri'
        WHEN 4 THEN 'Su teslimatı'
        WHEN 5 THEN 'Kahve ve atıştırmalık'
        WHEN 6 THEN 'Tatlı ve hamur işi'
        WHEN 99 THEN 'Diğer hizmetler'
        ELSE 'Bilinmeyen'
    END AS TypeDescription,
    sc.ImageUrl,
    sc.IconUrl,
    sc.DisplayOrder,
    sc.IsActive,
    sc.CreatedAt,
    sc.UpdatedAt
FROM ServiceCategories sc;

-- 11. Stored procedure - Kategori tipine göre merchant'ları getir
CREATE OR ALTER PROCEDURE sp_GetMerchantsByCategoryType
    @CategoryType INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        m.Id,
        m.Name,
        m.Description,
        m.Address,
        m.PhoneNumber,
        m.Email,
        m.Rating,
        m.TotalReviews,
        m.IsActive,
        m.IsOpen,
        sc.Name AS CategoryName,
        sc.Type AS CategoryType
    FROM Merchants m
    INNER JOIN ServiceCategories sc ON m.ServiceCategoryId = sc.Id
    WHERE sc.Type = @CategoryType 
        AND m.IsActive = 1
        AND sc.IsActive = 1
    ORDER BY m.Rating DESC, m.Name;
END;

-- 12. Function - Kategori tipinin yemek ile ilgili olup olmadığını kontrol et
CREATE OR ALTER FUNCTION fn_IsFoodRelatedCategory(@CategoryType INT)
RETURNS BIT
AS
BEGIN
    DECLARE @IsFoodRelated BIT = 0;
    
    IF @CategoryType IN (1, 5, 6) -- Restaurant, Cafe, Bakery
        SET @IsFoodRelated = 1;
    
    RETURN @IsFoodRelated;
END;

-- 13. Function - Kategori tipinin ürün ile ilgili olup olmadığını kontrol et
CREATE OR ALTER FUNCTION fn_IsProductRelatedCategory(@CategoryType INT)
RETURNS BIT
AS
BEGIN
    DECLARE @IsProductRelated BIT = 0;
    
    IF @CategoryType IN (2, 3, 4) -- Market, Pharmacy, Water
        SET @IsProductRelated = 1;
    
    RETURN @IsProductRelated;
END;

-- Migration tamamlandı
PRINT 'Migration 006: ServiceCategory Type column added successfully';
