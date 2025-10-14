-- =============================================
-- Migration: 023 - Market Entities Update
-- Description: Create/Update Market and MarketProduct tables with all required columns
-- Date: 2025-10-14
-- =============================================

-- =============================================
-- 1. Markets Table
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Markets')
BEGIN
    PRINT 'Creating Markets table...';
    
    CREATE TABLE Markets (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MerchantId UNIQUEIDENTIFIER NOT NULL,
        
        -- Market özel bilgileri
        MarketType NVARCHAR(100) NOT NULL,
        IsOrganic BIT NOT NULL DEFAULT 0,
        IsLocal BIT NOT NULL DEFAULT 0,
        IsInternational BIT NOT NULL DEFAULT 0,
        
        -- Çalışma saatleri
        OpeningTime TIME NOT NULL,
        ClosingTime TIME NOT NULL,
        IsOpen24Hours BIT NOT NULL DEFAULT 0,
        
        -- Teslimat bilgileri
        MinimumOrderAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
        DeliveryFee DECIMAL(18, 2) NOT NULL DEFAULT 0,
        DeliveryRadius INT NOT NULL DEFAULT 5,
        AverageDeliveryTime INT NOT NULL DEFAULT 30,
        
        -- Market özellikleri
        HasFreshProducts BIT NOT NULL DEFAULT 0,
        HasFrozenProducts BIT NOT NULL DEFAULT 0,
        HasPharmacy BIT NOT NULL DEFAULT 0,
        HasBakery BIT NOT NULL DEFAULT 0,
        HasButcher BIT NOT NULL DEFAULT 0,
        HasDeli BIT NOT NULL DEFAULT 0,
        
        -- Özel hizmetler
        HasOnlineShopping BIT NOT NULL DEFAULT 1,
        HasClickAndCollect BIT NOT NULL DEFAULT 0,
        HasHomeDelivery BIT NOT NULL DEFAULT 1,
        HasExpressDelivery BIT NOT NULL DEFAULT 0,
        
        -- Sertifikalar ve Sosyal Medya
        Certifications NVARCHAR(MAX) NULL,
        QualityStandards NVARCHAR(MAX) NULL,
        InstagramUrl NVARCHAR(500) NULL,
        FacebookUrl NVARCHAR(500) NULL,
        WebsiteUrl NVARCHAR(500) NULL,
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_Markets_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_Markets_MerchantId ON Markets (MerchantId);
    
    PRINT '✓ Markets table created.';
END
ELSE
BEGIN
    PRINT '⊗ Markets table already exists.';
END
GO

-- =============================================
-- 2. MarketCategories Table
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MarketCategories')
BEGIN
    PRINT 'Creating MarketCategories table...';
    
    CREATE TABLE MarketCategories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MarketId UNIQUEIDENTIFIER NOT NULL,
        ParentCategoryId UNIQUEIDENTIFIER NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_MarketCategories_Markets FOREIGN KEY (MarketId) REFERENCES Markets(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MarketCategories_ParentCategory FOREIGN KEY (ParentCategoryId) REFERENCES MarketCategories(Id)
    );
    
    CREATE INDEX IX_MarketCategories_MarketId ON MarketCategories (MarketId);
    CREATE INDEX IX_MarketCategories_ParentCategoryId ON MarketCategories (ParentCategoryId);
    
    PRINT '✓ MarketCategories table created.';
END
ELSE
BEGIN
    PRINT '⊗ MarketCategories table already exists.';
END
GO

-- =============================================
-- 3. MarketProducts Table
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MarketProducts')
BEGIN
    PRINT 'Creating MarketProducts table...';
    
    CREATE TABLE MarketProducts (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MarketId UNIQUEIDENTIFIER NOT NULL,
        CategoryId UNIQUEIDENTIFIER NULL,
        
        -- Temel ürün bilgileri
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        ImageUrl NVARCHAR(500) NULL,
        Price DECIMAL(18, 2) NOT NULL,
        DiscountedPrice DECIMAL(18, 2) NULL,
        IsAvailable BIT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        
        -- Market özel özellikleri
        Brand NVARCHAR(200) NULL,
        Barcode NVARCHAR(100) NULL,
        SKU NVARCHAR(100) NULL,
        Unit NVARCHAR(50) NULL,
        Weight DECIMAL(18, 2) NULL,
        Volume DECIMAL(18, 2) NULL,
        Size NVARCHAR(50) NULL,
        Color NVARCHAR(50) NULL,
        
        -- Tarih bilgileri
        ExpiryDate DATETIME2 NULL,
        ProductionDate DATETIME2 NULL,
        BestBeforeDate DATETIME2 NULL,
        
        -- Menşei bilgileri
        Origin NVARCHAR(100) NULL,
        OriginCity NVARCHAR(100) NULL,
        IsLocal BIT NOT NULL DEFAULT 0,
        IsOrganic BIT NOT NULL DEFAULT 0,
        IsFairTrade BIT NOT NULL DEFAULT 0,
        
        -- Alerjen bilgileri
        Allergens NVARCHAR(MAX) NULL,
        AllergenWarnings NVARCHAR(MAX) NULL,
        
        -- Besin değerleri
        NutritionInfo NVARCHAR(MAX) NULL,
        Calories INT NULL,
        Protein DECIMAL(18, 2) NULL,
        Carbs DECIMAL(18, 2) NULL,
        Fat DECIMAL(18, 2) NULL,
        Fiber DECIMAL(18, 2) NULL,
        Sugar DECIMAL(18, 2) NULL,
        Sodium DECIMAL(18, 2) NULL,
        
        -- Ürün özellikleri
        IsPopular BIT NOT NULL DEFAULT 0,
        IsNew BIT NOT NULL DEFAULT 0,
        IsOnSale BIT NOT NULL DEFAULT 0,
        IsSeasonal BIT NOT NULL DEFAULT 0,
        IsLimitedEdition BIT NOT NULL DEFAULT 0,
        
        -- Stok bilgileri
        StockQuantity INT NOT NULL DEFAULT 0,
        MinOrderQuantity INT NULL,
        MaxOrderQuantity INT NULL,
        IsUnlimitedStock BIT NOT NULL DEFAULT 0,
        
        -- Depolama bilgileri
        StorageConditions NVARCHAR(MAX) NULL,
        Temperature NVARCHAR(100) NULL,
        RequiresRefrigeration BIT NOT NULL DEFAULT 0,
        RequiresFreezing BIT NOT NULL DEFAULT 0,
        
        -- Timestamps
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_MarketProducts_Markets FOREIGN KEY (MarketId) REFERENCES Markets(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MarketProducts_Categories FOREIGN KEY (CategoryId) REFERENCES MarketCategories(Id),
        CONSTRAINT CK_MarketProducts_Price CHECK (Price >= 0),
        CONSTRAINT CK_MarketProducts_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0),
        CONSTRAINT CK_MarketProducts_StockQuantity CHECK (StockQuantity >= 0)
    );
    
    CREATE INDEX IX_MarketProducts_MarketId ON MarketProducts (MarketId);
    CREATE INDEX IX_MarketProducts_CategoryId ON MarketProducts (CategoryId);
    CREATE INDEX IX_MarketProducts_IsAvailable ON MarketProducts (IsAvailable);
    CREATE INDEX IX_MarketProducts_IsActive ON MarketProducts (IsActive);
    
    PRINT '✓ MarketProducts table created.';
END
ELSE
BEGIN
    PRINT '⊗ MarketProducts table already exists.';
END
GO

-- =============================================
-- 4. MarketProductVariants Table
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MarketProductVariants')
BEGIN
    PRINT 'Creating MarketProductVariants table...';
    
    CREATE TABLE MarketProductVariants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        MarketProductId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Sku NVARCHAR(100) NULL,
        PriceAdjustment DECIMAL(18, 2) NOT NULL DEFAULT 0,
        StockQuantity INT NOT NULL DEFAULT 0,
        Weight DECIMAL(18, 2) NULL,
        Volume DECIMAL(18, 2) NULL,
        Size NVARCHAR(50) NULL,
        Color NVARCHAR(50) NULL,
        ImageUrl NVARCHAR(500) NULL,
        Barcode NVARCHAR(100) NULL,
        IsAvailable BIT NOT NULL DEFAULT 1,
        IsDefault BIT NOT NULL DEFAULT 0,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_MarketProductVariants_MarketProducts FOREIGN KEY (MarketProductId) REFERENCES MarketProducts(Id) ON DELETE CASCADE,
        CONSTRAINT CK_MarketProductVariants_PriceAdjustment CHECK (PriceAdjustment >= -999999.99),
        CONSTRAINT CK_MarketProductVariants_StockQuantity CHECK (StockQuantity >= 0)
    );
    
    CREATE INDEX IX_MarketProductVariants_MarketProductId ON MarketProductVariants (MarketProductId);
    CREATE INDEX IX_MarketProductVariants_IsAvailable ON MarketProductVariants (IsAvailable);
    
    PRINT '✓ MarketProductVariants table created.';
END
ELSE
BEGIN
    PRINT '⊗ MarketProductVariants table already exists.';
END
GO

-- =============================================
-- 5. Add MarketProductId to OrderLines (if not exists)
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderLines') AND name = 'MarketProductId')
BEGIN
    PRINT 'Adding MarketProductId column to OrderLines table...';
    ALTER TABLE OrderLines ADD MarketProductId UNIQUEIDENTIFIER NULL;
    
    CREATE INDEX IX_OrderLines_MarketProductId ON OrderLines (MarketProductId)
    WHERE MarketProductId IS NOT NULL;
    
    PRINT '✓ MarketProductId column added to OrderLines.';
END
ELSE
BEGIN
    PRINT '⊗ MarketProductId column already exists in OrderLines.';
END
GO

-- =============================================
-- Verification
-- =============================================

PRINT '========================================';
PRINT 'Migration 023 completed successfully!';
PRINT '========================================';
PRINT 'Tables created/verified:';
PRINT '  - Markets';
PRINT '  - MarketCategories';
PRINT '  - MarketProducts';
PRINT '  - MarketProductVariants';
PRINT 'Columns added:';
PRINT '  - OrderLines.MarketProductId';
PRINT '========================================';
GO

