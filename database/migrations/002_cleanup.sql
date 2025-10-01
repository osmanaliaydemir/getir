-- =============================================
-- Sprint 2: Cleanup Script (Tekrar çalıştırmadan önce)
-- =============================================

USE GetirDb;
GO

-- Foreign key'leri kaldır
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Products_ProductCategories')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_ProductCategories;
    PRINT 'Dropped FK_Products_ProductCategories';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Merchants_ServiceCategories')
BEGIN
    ALTER TABLE Merchants DROP CONSTRAINT FK_Merchants_ServiceCategories;
    PRINT 'Dropped FK_Merchants_ServiceCategories';
END
GO

-- Index'leri kaldır
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ProductCategoryId' AND object_id = OBJECT_ID('Products'))
BEGIN
    DROP INDEX IX_Products_ProductCategoryId ON Products;
    PRINT 'Dropped IX_Products_ProductCategoryId';
END
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Merchants_ServiceCategoryId' AND object_id = OBJECT_ID('Merchants'))
BEGIN
    DROP INDEX IX_Merchants_ServiceCategoryId ON Merchants;
    PRINT 'Dropped IX_Merchants_ServiceCategoryId';
END
GO

-- ProductCategories tablosunu drop et
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductCategories')
BEGIN
    DROP TABLE ProductCategories;
    PRINT 'Dropped ProductCategories table';
END
GO

-- ServiceCategories tablosunu drop et
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceCategories')
BEGIN
    DROP TABLE ServiceCategories;
    PRINT 'Dropped ServiceCategories table';
END
GO

-- Merchants.ServiceCategoryId column'unu eski haline döndür (CategoryId)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Merchants') AND name = 'ServiceCategoryId')
BEGIN
    EXEC sp_rename 'Merchants.ServiceCategoryId', 'CategoryId', 'COLUMN';
    PRINT 'Renamed Merchants.ServiceCategoryId back to CategoryId';
END
GO

-- Products.ProductCategoryId column'unu eski haline döndür (CategoryId)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ProductCategoryId')
BEGIN
    EXEC sp_rename 'Products.ProductCategoryId', 'CategoryId', 'COLUMN';
    PRINT 'Renamed Products.ProductCategoryId back to CategoryId';
END
GO

-- Foreign key'leri geri ekle
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Merchants_Categories')
BEGIN
    ALTER TABLE Merchants ADD CONSTRAINT FK_Merchants_Categories
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id);
    PRINT 'Re-added FK_Merchants_Categories';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Products_Categories')
BEGIN
    ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id);
    PRINT 'Re-added FK_Products_Categories';
END
GO

PRINT '================================';
PRINT 'Cleanup completed successfully!';
PRINT 'You can now run 002_category_hierarchy.sql again';
PRINT '================================';
GO

