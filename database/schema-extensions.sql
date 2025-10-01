-- =============================================
-- Getir Clone - Schema Extensions
-- Yeni tablolar: Addresses, Cart, Coupons, Campaigns, Notifications
-- =============================================

USE GetirDb;
GO

-- =============================================
-- USER ADDRESSES
-- =============================================

CREATE TABLE UserAddresses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(100) NOT NULL, -- Ev, İş, vb.
    FullAddress NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    District NVARCHAR(100) NOT NULL,
    Latitude DECIMAL(10, 8) NOT NULL,
    Longitude DECIMAL(11, 8) NOT NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_UserAddresses_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_UserAddresses_UserId ON UserAddresses(UserId);
CREATE INDEX IX_UserAddresses_IsDefault ON UserAddresses(IsDefault);

-- =============================================
-- SHOPPING CART
-- =============================================

CREATE TABLE CartItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_CartItems_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_CartItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT UQ_CartItems_UserProductMerchant UNIQUE (UserId, ProductId, MerchantId)
);

CREATE INDEX IX_CartItems_UserId ON CartItems(UserId);
CREATE INDEX IX_CartItems_MerchantId ON CartItems(MerchantId);

-- =============================================
-- COUPONS
-- =============================================

CREATE TABLE Coupons (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DiscountType NVARCHAR(20) NOT NULL, -- Percentage, FixedAmount
    DiscountValue DECIMAL(18, 2) NOT NULL,
    MinimumOrderAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    MaximumDiscountAmount DECIMAL(18, 2) NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    UsageLimit INT NULL, -- Toplam kullanım limiti
    UsageCount INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Coupons_DiscountValue CHECK (DiscountValue > 0),
    CONSTRAINT CK_Coupons_Dates CHECK (EndDate > StartDate)
);

CREATE INDEX IX_Coupons_Code ON Coupons(Code);
CREATE INDEX IX_Coupons_IsActive ON Coupons(IsActive);
CREATE INDEX IX_Coupons_Dates ON Coupons(StartDate, EndDate);

-- =============================================
-- COUPON USAGE TRACKING
-- =============================================

CREATE TABLE CouponUsages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CouponId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    DiscountAmount DECIMAL(18, 2) NOT NULL,
    UsedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CouponUsages_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(Id),
    CONSTRAINT FK_CouponUsages_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_CouponUsages_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

CREATE INDEX IX_CouponUsages_UserId ON CouponUsages(UserId);
CREATE INDEX IX_CouponUsages_CouponId ON CouponUsages(CouponId);

-- =============================================
-- CAMPAIGNS
-- =============================================

CREATE TABLE Campaigns (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    MerchantId UNIQUEIDENTIFIER NULL, -- NULL ise tüm merchantlar için
    DiscountType NVARCHAR(20) NOT NULL, -- Percentage, FixedAmount, BuyXGetY
    DiscountValue DECIMAL(18, 2) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Campaigns_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id)
);

CREATE INDEX IX_Campaigns_MerchantId ON Campaigns(MerchantId);
CREATE INDEX IX_Campaigns_IsActive ON Campaigns(IsActive);
CREATE INDEX IX_Campaigns_Dates ON Campaigns(StartDate, EndDate);

-- =============================================
-- NOTIFICATIONS
-- =============================================

CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- Order, Promotion, System, Payment
    RelatedEntityId UNIQUEIDENTIFIER NULL, -- Order ID, Campaign ID, etc.
    RelatedEntityType NVARCHAR(50) NULL, -- Order, Campaign, etc.
    IsRead BIT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(500) NULL,
    ActionUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReadAt DATETIME2 NULL,
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt DESC);

-- =============================================
-- COURIER (Kurye) - Optional
-- =============================================

CREATE TABLE Couriers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    VehicleType NVARCHAR(50) NOT NULL, -- Motorcycle, Bicycle, Car
    LicensePlate NVARCHAR(20) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CurrentLatitude DECIMAL(10, 8) NULL,
    CurrentLongitude DECIMAL(11, 8) NULL,
    LastLocationUpdate DATETIME2 NULL,
    TotalDeliveries INT NOT NULL DEFAULT 0,
    Rating DECIMAL(3, 2) NULL DEFAULT 0.00,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Couriers_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_Couriers_Rating CHECK (Rating >= 0 AND Rating <= 5)
);

CREATE INDEX IX_Couriers_IsAvailable ON Couriers(IsAvailable);
CREATE INDEX IX_Couriers_Location ON Couriers(CurrentLatitude, CurrentLongitude);

-- =============================================
-- ORDER - COURIER ASSIGNMENT
-- =============================================

-- Orders tablosuna courier column ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CourierId')
BEGIN
    ALTER TABLE Orders ADD CourierId UNIQUEIDENTIFIER NULL;
    ALTER TABLE Orders ADD CONSTRAINT FK_Orders_Couriers FOREIGN KEY (CourierId) REFERENCES Couriers(Id);
    CREATE INDEX IX_Orders_CourierId ON Orders(CourierId);
END

-- Orders tablosuna coupon column ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'CouponId')
BEGIN
    ALTER TABLE Orders ADD CouponId UNIQUEIDENTIFIER NULL;
    ALTER TABLE Orders ADD CouponCode NVARCHAR(50) NULL;
    ALTER TABLE Orders ADD CONSTRAINT FK_Orders_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(Id);
END

-- =============================================
-- LOYALTY POINTS (Bonus)
-- =============================================

CREATE TABLE UserLoyaltyPoints (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Points INT NOT NULL DEFAULT 0,
    TotalEarned INT NOT NULL DEFAULT 0,
    TotalSpent INT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserLoyaltyPoints_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_UserLoyaltyPoints_UserId UNIQUE (UserId)
);

CREATE TABLE LoyaltyPointTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrderId UNIQUEIDENTIFIER NULL,
    Points INT NOT NULL,
    Type NVARCHAR(20) NOT NULL, -- Earned, Spent, Expired
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_LoyaltyPointTransactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_LoyaltyPointTransactions_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

CREATE INDEX IX_LoyaltyPointTransactions_UserId ON LoyaltyPointTransactions(UserId);

GO

PRINT 'Schema extensions created successfully!';
