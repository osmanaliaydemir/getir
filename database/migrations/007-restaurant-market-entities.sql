-- Migration: 007-restaurant-market-entities.sql
-- Description: Restaurant ve Market entity'leri için tablolar oluşturma

-- 1. Restaurant tablosu
CREATE TABLE Restaurants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    CuisineType NVARCHAR(100) NOT NULL,
    AveragePreparationTime INT NOT NULL DEFAULT 30,
    IsHalal BIT NOT NULL DEFAULT 0,
    IsVegetarianFriendly BIT NOT NULL DEFAULT 0,
    IsVeganFriendly BIT NOT NULL DEFAULT 0,
    IsGlutenFree BIT NOT NULL DEFAULT 0,
    IsLactoseFree BIT NOT NULL DEFAULT 0,
    OpeningTime TIME NOT NULL DEFAULT '09:00:00',
    ClosingTime TIME NOT NULL DEFAULT '22:00:00',
    IsOpen24Hours BIT NOT NULL DEFAULT 0,
    MinimumOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryRadius INT NOT NULL DEFAULT 5,
    AverageDeliveryTime INT NOT NULL DEFAULT 45,
    HasOutdoorSeating BIT NOT NULL DEFAULT 0,
    HasIndoorSeating BIT NOT NULL DEFAULT 0,
    HasTakeaway BIT NOT NULL DEFAULT 1,
    HasDelivery BIT NOT NULL DEFAULT 1,
    HasDriveThrough BIT NOT NULL DEFAULT 0,
    Certifications NVARCHAR(MAX) NULL,
    Awards NVARCHAR(MAX) NULL,
    InstagramUrl NVARCHAR(500) NULL,
    FacebookUrl NVARCHAR(500) NULL,
    WebsiteUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Restaurants_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE
);

-- 2. Market tablosu
CREATE TABLE Markets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    MarketType NVARCHAR(100) NOT NULL,
    IsOrganic BIT NOT NULL DEFAULT 0,
    IsLocal BIT NOT NULL DEFAULT 0,
    IsInternational BIT NOT NULL DEFAULT 0,
    OpeningTime TIME NOT NULL DEFAULT '08:00:00',
    ClosingTime TIME NOT NULL DEFAULT '23:00:00',
    IsOpen24Hours BIT NOT NULL DEFAULT 0,
    MinimumOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryRadius INT NOT NULL DEFAULT 5,
    AverageDeliveryTime INT NOT NULL DEFAULT 30,
    HasFreshProducts BIT NOT NULL DEFAULT 1,
    HasFrozenProducts BIT NOT NULL DEFAULT 1,
    HasPharmacy BIT NOT NULL DEFAULT 0,
    HasBakery BIT NOT NULL DEFAULT 0,
    HasButcher BIT NOT NULL DEFAULT 0,
    HasDeli BIT NOT NULL DEFAULT 0,
    HasOnlineShopping BIT NOT NULL DEFAULT 1,
    HasClickAndCollect BIT NOT NULL DEFAULT 0,
    HasHomeDelivery BIT NOT NULL DEFAULT 1,
    HasExpressDelivery BIT NOT NULL DEFAULT 0,
    Certifications NVARCHAR(MAX) NULL,
    QualityStandards NVARCHAR(MAX) NULL,
    InstagramUrl NVARCHAR(500) NULL,
    FacebookUrl NVARCHAR(500) NULL,
    WebsiteUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_Markets_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE
);

-- 3. RestaurantMenuCategory tablosu
CREATE TABLE RestaurantMenuCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RestaurantId UNIQUEIDENTIFIER NOT NULL,
    ParentCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ImageUrl NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_RestaurantMenuCategories_Restaurants FOREIGN KEY (RestaurantId) REFERENCES Restaurants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RestaurantMenuCategories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES RestaurantMenuCategories(Id) ON DELETE NO ACTION
);

-- 4. MarketCategory tablosu
CREATE TABLE MarketCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MarketId UNIQUEIDENTIFIER NOT NULL,
    ParentCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ImageUrl NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_MarketCategories_Markets FOREIGN KEY (MarketId) REFERENCES Markets(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MarketCategories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES MarketCategories(Id) ON DELETE NO ACTION
);

-- 5. RestaurantProduct tablosu
CREATE TABLE RestaurantProducts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RestaurantId UNIQUEIDENTIFIER NOT NULL,
    MenuCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountedPrice DECIMAL(18,2) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    PreparationTimeMinutes INT NOT NULL DEFAULT 15,
    PortionSize NVARCHAR(100) NULL,
    SpiceLevel NVARCHAR(50) NULL,
    IsSpicy BIT NOT NULL DEFAULT 0,
    IsVegetarian BIT NOT NULL DEFAULT 0,
    IsVegan BIT NOT NULL DEFAULT 0,
    IsGlutenFree BIT NOT NULL DEFAULT 0,
    IsLactoseFree BIT NOT NULL DEFAULT 0,
    IsHalal BIT NOT NULL DEFAULT 0,
    Allergens NVARCHAR(MAX) NULL,
    AllergenWarnings NVARCHAR(500) NULL,
    NutritionInfo NVARCHAR(MAX) NULL,
    Calories INT NULL,
    Protein DECIMAL(8,2) NULL,
    Carbs DECIMAL(8,2) NULL,
    Fat DECIMAL(8,2) NULL,
    Fiber DECIMAL(8,2) NULL,
    Sugar DECIMAL(8,2) NULL,
    Sodium DECIMAL(8,2) NULL,
    IsPopular BIT NOT NULL DEFAULT 0,
    IsNew BIT NOT NULL DEFAULT 0,
    IsChefSpecial BIT NOT NULL DEFAULT 0,
    IsSeasonal BIT NOT NULL DEFAULT 0,
    StockQuantity INT NOT NULL DEFAULT 0,
    IsUnlimitedStock BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_RestaurantProducts_Restaurants FOREIGN KEY (RestaurantId) REFERENCES Restaurants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RestaurantProducts_MenuCategories FOREIGN KEY (MenuCategoryId) REFERENCES RestaurantMenuCategories(Id) ON DELETE SET NULL
);

-- 6. MarketProduct tablosu
CREATE TABLE MarketProducts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MarketId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountedPrice DECIMAL(18,2) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    Brand NVARCHAR(100) NULL,
    Barcode NVARCHAR(50) NULL,
    SKU NVARCHAR(100) NULL,
    Unit NVARCHAR(50) NULL,
    Weight DECIMAL(10,3) NULL,
    Volume DECIMAL(10,3) NULL,
    Size NVARCHAR(50) NULL,
    Color NVARCHAR(50) NULL,
    ExpiryDate DATETIME2 NULL,
    ProductionDate DATETIME2 NULL,
    BestBeforeDate DATETIME2 NULL,
    Origin NVARCHAR(100) NULL,
    OriginCity NVARCHAR(100) NULL,
    IsLocal BIT NOT NULL DEFAULT 0,
    IsOrganic BIT NOT NULL DEFAULT 0,
    IsFairTrade BIT NOT NULL DEFAULT 0,
    Allergens NVARCHAR(MAX) NULL,
    AllergenWarnings NVARCHAR(500) NULL,
    NutritionInfo NVARCHAR(MAX) NULL,
    Calories INT NULL,
    Protein DECIMAL(8,2) NULL,
    Carbs DECIMAL(8,2) NULL,
    Fat DECIMAL(8,2) NULL,
    Fiber DECIMAL(8,2) NULL,
    Sugar DECIMAL(8,2) NULL,
    Sodium DECIMAL(8,2) NULL,
    IsPopular BIT NOT NULL DEFAULT 0,
    IsNew BIT NOT NULL DEFAULT 0,
    IsOnSale BIT NOT NULL DEFAULT 0,
    IsSeasonal BIT NOT NULL DEFAULT 0,
    IsLimitedEdition BIT NOT NULL DEFAULT 0,
    StockQuantity INT NOT NULL DEFAULT 0,
    MinOrderQuantity INT NULL,
    MaxOrderQuantity INT NULL,
    IsUnlimitedStock BIT NOT NULL DEFAULT 0,
    StorageConditions NVARCHAR(200) NULL,
    Temperature NVARCHAR(100) NULL,
    RequiresRefrigeration BIT NOT NULL DEFAULT 0,
    RequiresFreezing BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_MarketProducts_Markets FOREIGN KEY (MarketId) REFERENCES Markets(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MarketProducts_Categories FOREIGN KEY (CategoryId) REFERENCES MarketCategories(Id) ON DELETE SET NULL
);

-- 7. RestaurantProductOptionGroup tablosu
CREATE TABLE RestaurantProductOptionGroups (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsRequired BIT NOT NULL DEFAULT 0,
    IsMultipleSelection BIT NOT NULL DEFAULT 0,
    MaxSelections INT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_RestaurantProductOptionGroups_Products FOREIGN KEY (ProductId) REFERENCES RestaurantProducts(Id) ON DELETE CASCADE
);

-- 8. RestaurantProductOption tablosu
CREATE TABLE RestaurantProductOptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    OptionGroupId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsRequired BIT NOT NULL DEFAULT 0,
    IsAvailable BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_RestaurantProductOptions_Products FOREIGN KEY (ProductId) REFERENCES RestaurantProducts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RestaurantProductOptions_OptionGroups FOREIGN KEY (OptionGroupId) REFERENCES RestaurantProductOptionGroups(Id) ON DELETE SET NULL
);

-- 9. MarketProductVariant tablosu
CREATE TABLE MarketProductVariants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    SKU NVARCHAR(100) NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountedPrice DECIMAL(18,2) NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    IsAvailable BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    Size NVARCHAR(50) NULL,
    Color NVARCHAR(50) NULL,
    Flavor NVARCHAR(100) NULL,
    Material NVARCHAR(100) NULL,
    Weight NVARCHAR(50) NULL,
    Volume NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_MarketProductVariants_Products FOREIGN KEY (ProductId) REFERENCES MarketProducts(Id) ON DELETE CASCADE
);

-- 10. Index'ler oluştur
CREATE INDEX IX_Restaurants_MerchantId ON Restaurants (MerchantId);
CREATE INDEX IX_Markets_MerchantId ON Markets (MerchantId);
CREATE INDEX IX_RestaurantMenuCategories_RestaurantId ON RestaurantMenuCategories (RestaurantId);
CREATE INDEX IX_RestaurantMenuCategories_ParentCategoryId ON RestaurantMenuCategories (ParentCategoryId);
CREATE INDEX IX_MarketCategories_MarketId ON MarketCategories (MarketId);
CREATE INDEX IX_MarketCategories_ParentCategoryId ON MarketCategories (ParentCategoryId);
CREATE INDEX IX_RestaurantProducts_RestaurantId ON RestaurantProducts (RestaurantId);
CREATE INDEX IX_RestaurantProducts_MenuCategoryId ON RestaurantProducts (MenuCategoryId);
CREATE INDEX IX_MarketProducts_MarketId ON MarketProducts (MarketId);
CREATE INDEX IX_MarketProducts_CategoryId ON MarketProducts (CategoryId);
CREATE INDEX IX_RestaurantProductOptionGroups_ProductId ON RestaurantProductOptionGroups (ProductId);
CREATE INDEX IX_RestaurantProductOptions_ProductId ON RestaurantProductOptions (ProductId);
CREATE INDEX IX_RestaurantProductOptions_OptionGroupId ON RestaurantProductOptions (OptionGroupId);
CREATE INDEX IX_MarketProductVariants_ProductId ON MarketProductVariants (ProductId);

-- 11. Unique constraint'ler
CREATE UNIQUE INDEX UQ_Restaurants_MerchantId ON Restaurants (MerchantId);
CREATE UNIQUE INDEX UQ_Markets_MerchantId ON Markets (MerchantId);
CREATE UNIQUE INDEX UQ_MarketProducts_SKU ON MarketProducts (SKU) WHERE SKU IS NOT NULL;
CREATE UNIQUE INDEX UQ_MarketProductVariants_SKU ON MarketProductVariants (SKU) WHERE SKU IS NOT NULL;

-- 12. Check constraint'ler
ALTER TABLE Restaurants ADD CONSTRAINT CK_Restaurants_AveragePreparationTime CHECK (AveragePreparationTime > 0);
ALTER TABLE Restaurants ADD CONSTRAINT CK_Restaurants_DeliveryRadius CHECK (DeliveryRadius > 0);
ALTER TABLE Restaurants ADD CONSTRAINT CK_Restaurants_AverageDeliveryTime CHECK (AverageDeliveryTime > 0);
ALTER TABLE Restaurants ADD CONSTRAINT CK_Restaurants_OpeningClosingTime CHECK (OpeningTime < ClosingTime OR IsOpen24Hours = 1);

ALTER TABLE Markets ADD CONSTRAINT CK_Markets_DeliveryRadius CHECK (DeliveryRadius > 0);
ALTER TABLE Markets ADD CONSTRAINT CK_Markets_AverageDeliveryTime CHECK (AverageDeliveryTime > 0);
ALTER TABLE Markets ADD CONSTRAINT CK_Markets_OpeningClosingTime CHECK (OpeningTime < ClosingTime OR IsOpen24Hours = 1);

ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_Price CHECK (Price >= 0);
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0);
ALTER TABLE RestaurantProducts ADD CONSTRAINT CK_RestaurantProducts_PreparationTime CHECK (PreparationTimeMinutes > 0);

ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_Price CHECK (Price >= 0);
ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0);
ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_Weight CHECK (Weight IS NULL OR Weight > 0);
ALTER TABLE MarketProducts ADD CONSTRAINT CK_MarketProducts_Volume CHECK (Volume IS NULL OR Volume > 0);

ALTER TABLE RestaurantProductOptions ADD CONSTRAINT CK_RestaurantProductOptions_Price CHECK (Price >= 0);
ALTER TABLE MarketProductVariants ADD CONSTRAINT CK_MarketProductVariants_Price CHECK (Price >= 0);
ALTER TABLE MarketProductVariants ADD CONSTRAINT CK_MarketProductVariants_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0);

-- 13. View'lar oluştur
CREATE VIEW vw_RestaurantDetails AS
SELECT 
    r.Id,
    r.MerchantId,
    m.Name AS MerchantName,
    m.Address AS MerchantAddress,
    m.PhoneNumber AS MerchantPhone,
    m.Email AS MerchantEmail,
    r.CuisineType,
    r.AveragePreparationTime,
    r.IsHalal,
    r.IsVegetarianFriendly,
    r.IsVeganFriendly,
    r.IsGlutenFree,
    r.IsLactoseFree,
    r.OpeningTime,
    r.ClosingTime,
    r.IsOpen24Hours,
    r.MinimumOrderAmount,
    r.DeliveryFee,
    r.DeliveryRadius,
    r.AverageDeliveryTime,
    r.HasOutdoorSeating,
    r.HasIndoorSeating,
    r.HasTakeaway,
    r.HasDelivery,
    r.HasDriveThrough,
    r.CreatedAt,
    r.UpdatedAt
FROM Restaurants r
INNER JOIN Merchants m ON r.MerchantId = m.Id;

CREATE VIEW vw_MarketDetails AS
SELECT 
    mk.Id,
    mk.MerchantId,
    m.Name AS MerchantName,
    m.Address AS MerchantAddress,
    m.PhoneNumber AS MerchantPhone,
    m.Email AS MerchantEmail,
    mk.MarketType,
    mk.IsOrganic,
    mk.IsLocal,
    mk.IsInternational,
    mk.OpeningTime,
    mk.ClosingTime,
    mk.IsOpen24Hours,
    mk.MinimumOrderAmount,
    mk.DeliveryFee,
    mk.DeliveryRadius,
    mk.AverageDeliveryTime,
    mk.HasFreshProducts,
    mk.HasFrozenProducts,
    mk.HasPharmacy,
    mk.HasBakery,
    mk.HasButcher,
    mk.HasDeli,
    mk.HasOnlineShopping,
    mk.HasClickAndCollect,
    mk.HasHomeDelivery,
    mk.HasExpressDelivery,
    mk.CreatedAt,
    mk.UpdatedAt
FROM Markets mk
INNER JOIN Merchants m ON mk.MerchantId = m.Id;

-- 14. Stored procedure'lar
CREATE PROCEDURE sp_GetRestaurantsByCuisineType
    @CuisineType NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.Id,
        r.MerchantId,
        m.Name AS RestaurantName,
        m.Address,
        m.PhoneNumber,
        m.Rating,
        m.TotalReviews,
        r.CuisineType,
        r.AveragePreparationTime,
        r.IsHalal,
        r.IsVegetarianFriendly,
        r.IsVeganFriendly,
        r.DeliveryFee,
        r.AverageDeliveryTime
    FROM Restaurants r
    INNER JOIN Merchants m ON r.MerchantId = m.Id
    WHERE r.CuisineType = @CuisineType 
        AND m.IsActive = 1
        AND m.IsOpen = 1
    ORDER BY m.Rating DESC, m.Name;
END;

CREATE PROCEDURE sp_GetMarketsByType
    @MarketType NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        mk.Id,
        mk.MerchantId,
        m.Name AS MarketName,
        m.Address,
        m.PhoneNumber,
        m.Rating,
        m.TotalReviews,
        mk.MarketType,
        mk.IsOrganic,
        mk.IsLocal,
        mk.DeliveryFee,
        mk.AverageDeliveryTime,
        mk.HasExpressDelivery
    FROM Markets mk
    INNER JOIN Merchants m ON mk.MerchantId = m.Id
    WHERE mk.MarketType = @MarketType 
        AND m.IsActive = 1
        AND m.IsOpen = 1
    ORDER BY m.Rating DESC, m.Name;
END;

-- Migration tamamlandı
PRINT 'Migration 007: Restaurant and Market entities created successfully';
