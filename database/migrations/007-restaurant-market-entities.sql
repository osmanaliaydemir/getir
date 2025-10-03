-- Migration 007: Restaurant and Market Entities
-- Bu migration Restaurant ve Market için özel tablolar oluşturur

-- 1. Restaurants Tablosu
CREATE TABLE Restaurants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    CuisineType NVARCHAR(100) NOT NULL,
    AveragePreparationTimeMinutes INT NOT NULL,
    HasVegetarianOptions BIT NOT NULL DEFAULT 0,
    HasVeganOptions BIT NOT NULL DEFAULT 0,
    HasGlutenFreeOptions BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id)
);

-- 2. Markets Tablosu
CREATE TABLE Markets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    MarketType NVARCHAR(100) NOT NULL,
    OffersDelivery BIT NOT NULL DEFAULT 1,
    OffersPickup BIT NOT NULL DEFAULT 1,
    HasOrganicProducts BIT NOT NULL DEFAULT 0,
    HasLocalProducts BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id)
);

-- 3. RestaurantMenuCategories Tablosu
CREATE TABLE RestaurantMenuCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RestaurantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (RestaurantId) REFERENCES Restaurants(Id)
);

-- 4. MarketCategories Tablosu
CREATE TABLE MarketCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MarketId UNIQUEIDENTIFIER NOT NULL,
    ParentCategoryId UNIQUEIDENTIFIER,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (MarketId) REFERENCES Markets(Id),
    FOREIGN KEY (ParentCategoryId) REFERENCES MarketCategories(Id)
);

-- 5. RestaurantProducts Tablosu
CREATE TABLE RestaurantProducts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    RestaurantMenuCategoryId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ImageUrl NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    DiscountedPrice DECIMAL(18, 2),
    StockQuantity INT NOT NULL DEFAULT 0,
    Unit NVARCHAR(50),
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    PreparationTimeMinutes INT NOT NULL DEFAULT 15,
    PortionSize NVARCHAR(50) NOT NULL DEFAULT 'Orta',
    IsSpicy BIT NOT NULL DEFAULT 0,
    Allergens NVARCHAR(MAX),
    Calories DECIMAL(18, 2),
    RowVersion VARBINARY(8) NOT NULL DEFAULT 0x0000000000000000,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    FOREIGN KEY (RestaurantMenuCategoryId) REFERENCES RestaurantMenuCategories(Id)
);

-- 6. MarketProducts Tablosu
CREATE TABLE MarketProducts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    MarketCategoryId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    ImageUrl NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    DiscountedPrice DECIMAL(18, 2),
    StockQuantity INT NOT NULL DEFAULT 0,
    Unit NVARCHAR(50),
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    Brand NVARCHAR(100) NOT NULL,
    Barcode NVARCHAR(100) NOT NULL,
    Weight DECIMAL(18, 2) NOT NULL DEFAULT 0,
    UnitOfMeasure NVARCHAR(50),
    ExpiryDate DATETIME2,
    StorageConditions NVARCHAR(MAX),
    RowVersion VARBINARY(8) NOT NULL DEFAULT 0x0000000000000000,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    FOREIGN KEY (MarketCategoryId) REFERENCES MarketCategories(Id)
);

-- 7. RestaurantProductOptionGroups Tablosu
CREATE TABLE RestaurantProductOptionGroups (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RestaurantProductId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    MinSelections INT NOT NULL DEFAULT 0,
    MaxSelections INT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (RestaurantProductId) REFERENCES RestaurantProducts(Id)
);

-- 8. RestaurantProductOptions Tablosu
CREATE TABLE RestaurantProductOptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OptionGroupId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    PriceAdjustment DECIMAL(18, 2) NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (OptionGroupId) REFERENCES RestaurantProductOptionGroups(Id)
);

-- 9. MarketProductVariants Tablosu
CREATE TABLE MarketProductVariants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MarketProductId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Sku NVARCHAR(100),
    PriceAdjustment DECIMAL(18, 2) NOT NULL DEFAULT 0,
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(MAX),
    IsAvailable BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (MarketProductId) REFERENCES MarketProducts(Id)
);

-- 10. Index'ler
CREATE INDEX IX_Restaurants_MerchantId ON Restaurants (MerchantId);
CREATE INDEX IX_Markets_MerchantId ON Markets (MerchantId);
CREATE INDEX IX_RestaurantMenuCategories_RestaurantId ON RestaurantMenuCategories (RestaurantId);
CREATE INDEX IX_MarketCategories_MarketId ON MarketCategories (MarketId);
CREATE INDEX IX_MarketCategories_ParentCategoryId ON MarketCategories (ParentCategoryId);
CREATE INDEX IX_RestaurantProducts_MerchantId ON RestaurantProducts (MerchantId);
CREATE INDEX IX_RestaurantProducts_RestaurantMenuCategoryId ON RestaurantProducts (RestaurantMenuCategoryId);
CREATE INDEX IX_MarketProducts_MerchantId ON MarketProducts (MerchantId);
CREATE INDEX IX_MarketProducts_MarketCategoryId ON MarketProducts (MarketCategoryId);
CREATE INDEX IX_RestaurantProductOptionGroups_RestaurantProductId ON RestaurantProductOptionGroups (RestaurantProductId);
CREATE INDEX IX_RestaurantProductOptions_OptionGroupId ON RestaurantProductOptions (OptionGroupId);
CREATE INDEX IX_MarketProductVariants_MarketProductId ON MarketProductVariants (MarketProductId);

-- 11. Check Constraint'ler
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_Price CHECK (Price >= 0);
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0);
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_StockQuantity CHECK (StockQuantity >= 0);
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_PreparationTime CHECK (PreparationTimeMinutes > 0);
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_Calories CHECK (Calories IS NULL OR Calories >= 0);

ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_Price CHECK (Price >= 0);
ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0);
ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_StockQuantity CHECK (StockQuantity >= 0);
ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_Weight CHECK (Weight >= 0);

ALTER TABLE RestaurantProductOptions ADD CONSTRAINT CK_RestaurantProductOptions_Price CHECK (PriceAdjustment >= -999999.99);
ALTER TABLE MarketProductVariants ADD CONSTRAINT CK_MarketProductVariants_Price CHECK (PriceAdjustment >= -999999.99);
ALTER TABLE MarketProductVariants ADD CONSTRAINT CK_MarketProductVariants_StockQuantity CHECK (StockQuantity >= 0);

-- Migration tamamlandı
PRINT 'Migration 007: Restaurant and Market entities created successfully';