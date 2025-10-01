-- =============================================
-- Sprint 2: Category Hierarchy - Database Migration
-- ServiceCategory (Market, Yemek) + ProductCategory (Hiyerarşik)
-- =============================================

USE GetirDb;
GO

-- =============================================
-- 1. RENAME Categories to ServiceCategories
-- =============================================

-- ServiceCategories tablosu oluştur
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ServiceCategories')
BEGIN
    CREATE TABLE ServiceCategories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        IconUrl NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    
    CREATE INDEX IX_ServiceCategories_IsActive ON ServiceCategories(IsActive);
    CREATE INDEX IX_ServiceCategories_DisplayOrder ON ServiceCategories(DisplayOrder);
    
    PRINT 'ServiceCategories table created';
END
GO

-- Eski Categories verisini ServiceCategories'e taşı
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, ImageUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    SELECT Id, Name, Description, ImageUrl, DisplayOrder, IsActive, CreatedAt, UpdatedAt
    FROM Categories;
    
    PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' rows migrated from Categories to ServiceCategories';
END
GO

-- =============================================
-- 2. MERCHANTS: CategoryId → ServiceCategoryId
-- =============================================

-- Önce foreign key'i kaldır
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Merchants_Categories')
BEGIN
    ALTER TABLE Merchants DROP CONSTRAINT FK_Merchants_Categories;
    PRINT 'Dropped FK_Merchants_Categories';
END
GO

-- Column'ı rename et (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Merchants') AND name = 'ServiceCategoryId')
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Merchants') AND name = 'CategoryId')
    BEGIN
        EXEC sp_rename 'Merchants.CategoryId', 'ServiceCategoryId', 'COLUMN';
        PRINT 'Renamed Merchants.CategoryId to ServiceCategoryId';
    END
    ELSE
    BEGIN
        ALTER TABLE Merchants ADD ServiceCategoryId UNIQUEIDENTIFIER NOT NULL;
        PRINT 'Added ServiceCategoryId to Merchants';
    END
END
GO

-- Yeni foreign key ekle
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Merchants_ServiceCategories')
BEGIN
    ALTER TABLE Merchants ADD CONSTRAINT FK_Merchants_ServiceCategories
        FOREIGN KEY (ServiceCategoryId) REFERENCES ServiceCategories(Id);
    PRINT 'Added FK_Merchants_ServiceCategories';
END
GO

-- Index ekle
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Merchants_ServiceCategoryId')
BEGIN
    CREATE INDEX IX_Merchants_ServiceCategoryId ON Merchants(ServiceCategoryId);
    PRINT 'Added index IX_Merchants_ServiceCategoryId';
END
GO

-- =============================================
-- 3. PRODUCT CATEGORIES (Hiyerarşik, Merchant-specific)
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductCategories')
BEGIN
    CREATE TABLE ProductCategories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MerchantId UNIQUEIDENTIFIER NOT NULL,
        ParentCategoryId UNIQUEIDENTIFIER NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_ProductCategories_Merchants 
            FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
        
        CONSTRAINT FK_ProductCategories_ParentCategories 
            FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_ProductCategories_MerchantId ON ProductCategories(MerchantId);
    CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId);
    CREATE INDEX IX_ProductCategories_IsActive ON ProductCategories(IsActive);
    
    PRINT 'ProductCategories table created';
END
GO

-- =============================================
-- 4. PRODUCTS: CategoryId → ProductCategoryId
-- =============================================

-- Önce foreign key'i kaldır
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Products_Categories')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_Categories;
    PRINT 'Dropped FK_Products_Categories';
END
GO

-- Column'ı rename et (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ProductCategoryId')
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CategoryId')
    BEGIN
        EXEC sp_rename 'Products.CategoryId', 'ProductCategoryId', 'COLUMN';
        PRINT 'Renamed Products.CategoryId to ProductCategoryId';
    END
    ELSE
    BEGIN
        ALTER TABLE Products ADD ProductCategoryId UNIQUEIDENTIFIER NULL;
        PRINT 'Added ProductCategoryId to Products';
    END
END
GO

-- Yeni foreign key ekle (cascade conflict önlemek için NO ACTION)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Products_ProductCategories')
BEGIN
    ALTER TABLE Products ADD CONSTRAINT FK_Products_ProductCategories
        FOREIGN KEY (ProductCategoryId) REFERENCES ProductCategories(Id) ON DELETE NO ACTION;
    PRINT 'Added FK_Products_ProductCategories';
END
GO

-- Index ekle
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ProductCategoryId')
BEGIN
    CREATE INDEX IX_Products_ProductCategoryId ON Products(ProductCategoryId);
    PRINT 'Added index IX_Products_ProductCategoryId';
END
GO

-- =============================================
-- 5. DROP OLD CATEGORIES TABLE (Optional - Yedek alındıktan sonra)
-- =============================================

-- UYARI: Bu adımı production'da dikkatli kullanın!
-- IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
-- BEGIN
--     DROP TABLE Categories;
--     PRINT 'Dropped old Categories table';
-- END
-- GO

-- =============================================
-- 6. SEED DATA - ServiceCategories
-- =============================================

-- Getir'in ana hizmet kategorilerini ekle
IF NOT EXISTS (SELECT * FROM ServiceCategories WHERE Name = 'Market')
BEGIN
    INSERT INTO ServiceCategories (Id, Name, Description, IconUrl, DisplayOrder, IsActive)
    VALUES 
        (NEWID(), 'Market', 'Günlük ihtiyaç ürünleri ve market alışverişi', '🛒', 1, 1),
        (NEWID(), 'Yemek', 'Restoranlar ve yemek siparişi', '🍔', 2, 1),
        (NEWID(), 'Su', 'Su ve içecek siparişi', '💧', 3, 1),
        (NEWID(), 'Eczane', 'İlaç ve sağlık ürünleri', '💊', 4, 1);
    
    PRINT 'ServiceCategories seed data inserted';
END
GO

-- =============================================
-- 7. VERIFICATION QUERIES
-- =============================================

PRINT '================================';
PRINT 'Migration completed successfully!';
PRINT '================================';
PRINT '';

PRINT 'ServiceCategories:';
SELECT Id, Name, DisplayOrder, IsActive FROM ServiceCategories ORDER BY DisplayOrder;

PRINT '';
PRINT 'ProductCategories Count:';
SELECT COUNT(*) AS TotalProductCategories FROM ProductCategories;

PRINT '';
PRINT 'Merchants with ServiceCategoryId:';
SELECT TOP 5 Id, Name, ServiceCategoryId FROM Merchants;

PRINT '';
PRINT 'Products with ProductCategoryId:';
SELECT TOP 5 Id, Name, ProductCategoryId FROM Products;

GO

