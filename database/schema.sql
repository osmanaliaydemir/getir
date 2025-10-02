-- =============================================
-- Getir Clone - Complete Database Schema
-- Entity Framework Code-First yaklaşımı için optimize edilmiş
-- Performance index'leri dahil
-- =============================================

USE master;
GO

-- Database oluştur (yoksa)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'GetirDb')
BEGIN
    CREATE DATABASE GetirDb;
END
GO

USE GetirDb;
GO

-- =============================================
-- ENUMS & TYPES
-- =============================================

-- UserRole enum değerleri
-- 1=Customer, 2=MerchantOwner, 3=Courier, 4=Admin

-- OrderStatus enum değerleri  
-- 0=Pending, 1=Confirmed, 2=Preparing, 3=Ready, 4=OnTheWay, 5=Delivered, 6=Cancelled

-- MerchantOnboardingStatus enum değerleri
-- 0=NotStarted, 1=BasicInfoCompleted, 2=DocumentsUploaded, 3=PaymentInfoCompleted, 
-- 4=BusinessInfoCompleted, 5=PendingApproval, 6=Approved, 7=Rejected, 8=Suspended

-- =============================================
-- CORE TABLES
-- =============================================

-- USERS
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Role INT NOT NULL DEFAULT 1, -- UserRole enum
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    LastLoginAt DATETIME2 NULL,
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%'),
    CONSTRAINT CK_Users_Role CHECK (Role BETWEEN 1 AND 4)
);

-- REFRESH TOKENS
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(450) NOT NULL UNIQUE, -- Reduced for index compatibility, UNIQUE constraint included
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2 NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- SERVICE CATEGORIES (Market, Yemek, Su, Eczane, vb.)
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

-- MERCHANTS
CREATE TABLE Merchants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ServiceCategoryId UNIQUEIDENTIFIER NOT NULL,
    LogoUrl NVARCHAR(500) NULL,
    CoverImageUrl NVARCHAR(500) NULL,
    Address NVARCHAR(500) NOT NULL,
    Latitude DECIMAL(18, 15) NOT NULL,
    Longitude DECIMAL(18, 15) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(256) NULL,
    MinimumOrderAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    DeliveryFee DECIMAL(18, 2) NOT NULL DEFAULT 0,
    AverageDeliveryTime INT NOT NULL DEFAULT 30, -- Dakika
    Rating DECIMAL(3, 2) NULL,
    TotalReviews INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsBusy BIT NOT NULL DEFAULT 0,
    IsOpen BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Merchants_Users FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Merchants_ServiceCategories FOREIGN KEY (ServiceCategoryId) REFERENCES ServiceCategories(Id),
    CONSTRAINT CK_Merchants_Rating CHECK (Rating IS NULL OR (Rating >= 0 AND Rating <= 5)),
    CONSTRAINT CK_Merchants_Latitude CHECK (Latitude >= -90 AND Latitude <= 90),
    CONSTRAINT CK_Merchants_Longitude CHECK (Longitude >= -180 AND Longitude <= 180)
);

-- PRODUCT CATEGORIES (Merchant-specific, hiyerarşik)
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
    CONSTRAINT FK_ProductCategories_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductCategories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id)
);

-- PRODUCTS
CREATE TABLE Products (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    ProductCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Price DECIMAL(18, 2) NOT NULL,
    DiscountedPrice DECIMAL(18, 2) NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    Unit NVARCHAR(50) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL, -- Optimistic locking
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Products_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Products_ProductCategories FOREIGN KEY (ProductCategoryId) REFERENCES ProductCategories(Id),
    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_DiscountedPrice CHECK (DiscountedPrice IS NULL OR DiscountedPrice >= 0),
    CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
);

-- PRODUCT OPTION GROUPS
CREATE TABLE ProductOptionGroups (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsRequired BIT NOT NULL DEFAULT 0,
    MinSelection INT NOT NULL DEFAULT 0,
    MaxSelection INT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_ProductOptionGroups_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- PRODUCT OPTIONS
CREATE TABLE ProductOptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductOptionGroupId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    ExtraPrice DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_ProductOptions_ProductOptionGroups FOREIGN KEY (ProductOptionGroupId) REFERENCES ProductOptionGroups(Id) ON DELETE CASCADE,
    CONSTRAINT CK_ProductOptions_ExtraPrice CHECK (ExtraPrice >= 0)
);

-- COUPONS (moved before ORDERS for foreign key reference)
CREATE TABLE Coupons (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    DiscountType NVARCHAR(20) NOT NULL, -- Percentage, FixedAmount
    DiscountValue DECIMAL(18, 2) NOT NULL,
    MinimumOrderAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    MaximumDiscountAmount DECIMAL(18, 2) NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    UsageLimit INT NULL,
    UsageCount INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Coupons_DiscountType CHECK (DiscountType IN ('Percentage', 'FixedAmount')),
    CONSTRAINT CK_Coupons_DiscountValue CHECK (DiscountValue > 0),
    CONSTRAINT CK_Coupons_MinimumOrderAmount CHECK (MinimumOrderAmount >= 0),
    CONSTRAINT CK_Coupons_MaximumDiscountAmount CHECK (MaximumDiscountAmount IS NULL OR MaximumDiscountAmount > 0),
    CONSTRAINT CK_Coupons_UsageLimit CHECK (UsageLimit IS NULL OR UsageLimit > 0),
    CONSTRAINT CK_Coupons_UsageCount CHECK (UsageCount >= 0),
    CONSTRAINT CK_Coupons_DateRange CHECK (EndDate > StartDate)
);

-- COURIERS (moved before ORDERS for foreign key reference)
CREATE TABLE Couriers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    VehicleType NVARCHAR(50) NOT NULL,
    LicensePlate NVARCHAR(20) NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CurrentLatitude DECIMAL(18, 15) NULL,
    CurrentLongitude DECIMAL(18, 15) NULL,
    LastLocationUpdate DATETIME2 NULL,
    TotalDeliveries INT NOT NULL DEFAULT 0,
    Rating DECIMAL(3, 2) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Couriers_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Couriers_Latitude CHECK (CurrentLatitude IS NULL OR (CurrentLatitude >= -90 AND CurrentLatitude <= 90)),
    CONSTRAINT CK_Couriers_Longitude CHECK (CurrentLongitude IS NULL OR (CurrentLongitude >= -180 AND CurrentLongitude <= 180)),
    CONSTRAINT CK_Couriers_Rating CHECK (Rating IS NULL OR (Rating >= 0 AND Rating <= 5))
);

-- ORDERS
CREATE TABLE Orders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    UserId UNIQUEIDENTIFIER NOT NULL,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    CourierId UNIQUEIDENTIFIER NULL,
    CouponId UNIQUEIDENTIFIER NULL,
    CouponCode NVARCHAR(50) NULL,
    Status INT NOT NULL DEFAULT 0, -- OrderStatus enum
    SubTotal DECIMAL(18, 2) NOT NULL,
    DeliveryFee DECIMAL(18, 2) NOT NULL,
    Discount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Total DECIMAL(18, 2) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    PaymentStatus NVARCHAR(50) NOT NULL,
    DeliveryAddress NVARCHAR(500) NOT NULL,
    DeliveryLatitude DECIMAL(18, 15) NOT NULL,
    DeliveryLongitude DECIMAL(18, 15) NOT NULL,
    EstimatedDeliveryTime DATETIME2 NULL,
    ActualDeliveryTime DATETIME2 NULL,
    Notes NVARCHAR(500) NULL,
    CancellationReason NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Orders_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_Orders_Couriers FOREIGN KEY (CourierId) REFERENCES Couriers(Id),
    CONSTRAINT FK_Orders_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(Id),
    CONSTRAINT CK_Orders_Status CHECK (Status BETWEEN 0 AND 6),
    CONSTRAINT CK_Orders_SubTotal CHECK (SubTotal >= 0),
    CONSTRAINT CK_Orders_DeliveryFee CHECK (DeliveryFee >= 0),
    CONSTRAINT CK_Orders_Discount CHECK (Discount >= 0),
    CONSTRAINT CK_Orders_Total CHECK (Total >= 0),
    CONSTRAINT CK_Orders_DeliveryLatitude CHECK (DeliveryLatitude >= -90 AND DeliveryLatitude <= 90),
    CONSTRAINT CK_Orders_DeliveryLongitude CHECK (DeliveryLongitude >= -180 AND DeliveryLongitude <= 180)
);

-- ORDER LINES
CREATE TABLE OrderLines (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Notes NVARCHAR(500) NULL,
    CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_OrderLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderLines_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_OrderLines_TotalPrice CHECK (TotalPrice >= 0)
);

-- ORDER LINE OPTIONS
CREATE TABLE OrderLineOptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderLineId UNIQUEIDENTIFIER NOT NULL,
    ProductOptionId UNIQUEIDENTIFIER NOT NULL,
    OptionName NVARCHAR(100) NOT NULL,
    ExtraPrice DECIMAL(18, 2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_OrderLineOptions_OrderLines FOREIGN KEY (OrderLineId) REFERENCES OrderLines(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderLineOptions_ProductOptions FOREIGN KEY (ProductOptionId) REFERENCES ProductOptions(Id),
    CONSTRAINT CK_OrderLineOptions_ExtraPrice CHECK (ExtraPrice >= 0)
);

-- CART ITEMS
CREATE TABLE CartItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL,
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_CartItems_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_CartItems_Quantity CHECK (Quantity > 0)
);

-- COUPON USAGE
CREATE TABLE CouponUsages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CouponId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    DiscountAmount DECIMAL(18, 2) NOT NULL,
    UsedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CouponUsages_Coupons FOREIGN KEY (CouponId) REFERENCES Coupons(Id),
    CONSTRAINT FK_CouponUsages_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_CouponUsages_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT CK_CouponUsages_DiscountAmount CHECK (DiscountAmount >= 0)
);

-- REVIEWS
CREATE TABLE Reviews (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReviewerId UNIQUEIDENTIFIER NOT NULL,
    RevieweeId UNIQUEIDENTIFIER NOT NULL,
    RevieweeType NVARCHAR(20) NOT NULL, -- Merchant, Courier
    OrderId UNIQUEIDENTIFIER NOT NULL,
    Rating INT NOT NULL,
    Comment NVARCHAR(500) NOT NULL,
    IsApproved BIT NOT NULL DEFAULT 1,
    IsModerated BIT NOT NULL DEFAULT 0,
    ModerationNotes NVARCHAR(500) NULL,
    ModeratedBy UNIQUEIDENTIFIER NULL,
    ModeratedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Reviews_Reviewer FOREIGN KEY (ReviewerId) REFERENCES Users(Id),
    CONSTRAINT FK_Reviews_Reviewee FOREIGN KEY (RevieweeId) REFERENCES Users(Id),
    CONSTRAINT FK_Reviews_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_Reviews_Moderator FOREIGN KEY (ModeratedBy) REFERENCES Users(Id),
    CONSTRAINT CK_Reviews_Rating CHECK (Rating >= 1 AND Rating <= 5),
    CONSTRAINT CK_Reviews_RevieweeType CHECK (RevieweeType IN ('Merchant', 'Courier'))
);

-- REVIEW TAGS
CREATE TABLE ReviewTags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReviewId UNIQUEIDENTIFIER NOT NULL,
    Tag NVARCHAR(50) NOT NULL,
    IsPositive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ReviewTags_Reviews FOREIGN KEY (ReviewId) REFERENCES Reviews(Id) ON DELETE CASCADE
);

-- REVIEW HELPFUL
CREATE TABLE ReviewHelpfuls (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReviewId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    IsHelpful BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ReviewHelpfuls_Reviews FOREIGN KEY (ReviewId) REFERENCES Reviews(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReviewHelpfuls_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- RATINGS (Aggregated)
CREATE TABLE Ratings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntityId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(20) NOT NULL,
    AverageRating DECIMAL(3, 2) NOT NULL DEFAULT 0,
    TotalReviews INT NOT NULL DEFAULT 0,
    FiveStarCount INT NOT NULL DEFAULT 0,
    FourStarCount INT NOT NULL DEFAULT 0,
    ThreeStarCount INT NOT NULL DEFAULT 0,
    TwoStarCount INT NOT NULL DEFAULT 0,
    OneStarCount INT NOT NULL DEFAULT 0,
    RecentAverageRating DECIMAL(3, 2) NOT NULL DEFAULT 0,
    RecentReviewCount INT NOT NULL DEFAULT 0,
    ResponseRate DECIMAL(5, 2) NOT NULL DEFAULT 0,
    PositiveReviewRate DECIMAL(5, 2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastReviewDate DATETIME2 NULL,
    CONSTRAINT CK_Ratings_EntityType CHECK (EntityType IN ('Merchant', 'Courier')),
    CONSTRAINT CK_Ratings_AverageRating CHECK (AverageRating >= 0 AND AverageRating <= 5),
    CONSTRAINT CK_Ratings_TotalReviews CHECK (TotalReviews >= 0),
    CONSTRAINT CK_Ratings_StarCounts CHECK (FiveStarCount >= 0 AND FourStarCount >= 0 AND ThreeStarCount >= 0 AND TwoStarCount >= 0 AND OneStarCount >= 0)
);

-- RATING HISTORY
CREATE TABLE RatingHistories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntityId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(20) NOT NULL,
    AverageRating DECIMAL(3, 2) NOT NULL,
    TotalReviews INT NOT NULL,
    NewReviews INT NOT NULL,
    SnapshotDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FoodQualityRating DECIMAL(3, 2) NULL,
    DeliverySpeedRating DECIMAL(3, 2) NULL,
    ServiceRating DECIMAL(3, 2) NULL,
    CONSTRAINT CK_RatingHistories_EntityType CHECK (EntityType IN ('Merchant', 'Courier'))
);

-- NOTIFICATIONS
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- Order, Promotion, System, Payment
    RelatedEntityId UNIQUEIDENTIFIER NULL,
    RelatedEntityType NVARCHAR(50) NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(500) NULL,
    ActionUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReadAt DATETIME2 NULL,
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- AUDIT LOGS
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(50) NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(50) NOT NULL,
    Details NVARCHAR(2000) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    SessionId NVARCHAR(100) NULL,
    RequestId NVARCHAR(50) NULL,
    IsSuccess BIT NOT NULL DEFAULT 1,
    ErrorMessage NVARCHAR(500) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- SYSTEM NOTIFICATIONS
CREATE TABLE SystemNotifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    TargetRoles NVARCHAR(100) NOT NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_SystemNotifications_Users FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT CK_SystemNotifications_Priority CHECK (Priority BETWEEN 1 AND 4)
);

-- WORKING HOURS
CREATE TABLE WorkingHours (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek INT NOT NULL, -- 0=Sunday, 1=Monday, ..., 6=Saturday
    OpenTime TIME NULL,
    CloseTime TIME NULL,
    IsClosed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_WorkingHours_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT CK_WorkingHours_DayOfWeek CHECK (DayOfWeek BETWEEN 0 AND 6)
);

-- DELIVERY ZONES
CREATE TABLE DeliveryZones (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    DeliveryFee DECIMAL(18, 2) NOT NULL,
    EstimatedDeliveryTime INT NOT NULL, -- Dakika
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_DeliveryZones_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT CK_DeliveryZones_DeliveryFee CHECK (DeliveryFee >= 0),
    CONSTRAINT CK_DeliveryZones_EstimatedDeliveryTime CHECK (EstimatedDeliveryTime > 0)
);

-- DELIVERY ZONE POINTS
CREATE TABLE DeliveryZonePoints (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DeliveryZoneId UNIQUEIDENTIFIER NOT NULL,
    Latitude DECIMAL(18, 15) NOT NULL,
    Longitude DECIMAL(18, 15) NOT NULL,
    [Order] INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_DeliveryZonePoints_DeliveryZones FOREIGN KEY (DeliveryZoneId) REFERENCES DeliveryZones(Id) ON DELETE CASCADE,
    CONSTRAINT CK_DeliveryZonePoints_Latitude CHECK (Latitude >= -90 AND Latitude <= 90),
    CONSTRAINT CK_DeliveryZonePoints_Longitude CHECK (Longitude >= -180 AND Longitude <= 180)
);

-- USER ADDRESSES
CREATE TABLE UserAddresses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    FullAddress NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    District NVARCHAR(100) NOT NULL,
    Latitude DECIMAL(18, 15) NOT NULL,
    Longitude DECIMAL(18, 15) NOT NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_UserAddresses_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_UserAddresses_Latitude CHECK (Latitude >= -90 AND Latitude <= 90),
    CONSTRAINT CK_UserAddresses_Longitude CHECK (Longitude >= -180 AND Longitude <= 180)
);

-- CAMPAIGNS
CREATE TABLE Campaigns (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(500) NULL,
    MerchantId UNIQUEIDENTIFIER NULL,
    DiscountType NVARCHAR(20) NOT NULL,
    DiscountValue DECIMAL(18, 2) NOT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Campaigns_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT CK_Campaigns_DiscountType CHECK (DiscountType IN ('Percentage', 'FixedAmount')),
    CONSTRAINT CK_Campaigns_DiscountValue CHECK (DiscountValue > 0),
    CONSTRAINT CK_Campaigns_DateRange CHECK (EndDate > StartDate)
);

-- MERCHANT ONBOARDING
CREATE TABLE MerchantOnboardings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    BasicInfoCompleted BIT NOT NULL DEFAULT 0,
    BusinessInfoCompleted BIT NOT NULL DEFAULT 0,
    WorkingHoursCompleted BIT NOT NULL DEFAULT 0,
    DeliveryZonesCompleted BIT NOT NULL DEFAULT 0,
    ProductsAdded BIT NOT NULL DEFAULT 0,
    DocumentsUploaded BIT NOT NULL DEFAULT 0,
    IsVerified BIT NOT NULL DEFAULT 0,
    IsApproved BIT NOT NULL DEFAULT 0,
    RejectionReason NVARCHAR(500) NULL,
    VerifiedAt DATETIME2 NULL,
    ApprovedAt DATETIME2 NULL,
    CompletedSteps INT NOT NULL DEFAULT 0,
    TotalSteps INT NOT NULL DEFAULT 6,
    ProgressPercentage DECIMAL(5, 2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_MerchantOnboardings_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MerchantOnboardings_Users FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT CK_MerchantOnboardings_CompletedSteps CHECK (CompletedSteps >= 0 AND CompletedSteps <= TotalSteps),
    CONSTRAINT CK_MerchantOnboardings_TotalSteps CHECK (TotalSteps > 0),
    CONSTRAINT CK_MerchantOnboardings_ProgressPercentage CHECK (ProgressPercentage >= 0 AND ProgressPercentage <= 100)
);

-- USER LOYALTY POINTS
CREATE TABLE UserLoyaltyPoints (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Points INT NOT NULL DEFAULT 0,
    TotalEarned INT NOT NULL DEFAULT 0,
    TotalSpent INT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_UserLoyaltyPoints_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT CK_UserLoyaltyPoints_Points CHECK (Points >= 0),
    CONSTRAINT CK_UserLoyaltyPoints_TotalEarned CHECK (TotalEarned >= 0),
    CONSTRAINT CK_UserLoyaltyPoints_TotalSpent CHECK (TotalSpent >= 0)
);

-- LOYALTY POINT TRANSACTIONS
CREATE TABLE LoyaltyPointTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrderId UNIQUEIDENTIFIER NULL,
    Points INT NOT NULL,
    Type NVARCHAR(20) NOT NULL, -- Earned, Spent, Expired
    Description NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_LoyaltyPointTransactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LoyaltyPointTransactions_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT CK_LoyaltyPointTransactions_Type CHECK (Type IN ('Earned', 'Spent', 'Expired'))
);

-- =============================================
-- PERFORMANCE INDEXES
-- =============================================

-- USERS INDEXES
-- IX_Users_Email already exists as UNIQUE constraint
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);

-- REFRESH TOKENS INDEXES
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
-- IX_RefreshTokens_Token already exists as UNIQUE constraint
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);

-- MERCHANTS INDEXES
CREATE INDEX IX_Merchants_OwnerId ON Merchants(OwnerId);
CREATE INDEX IX_Merchants_ServiceCategoryId ON Merchants(ServiceCategoryId);
CREATE INDEX IX_Merchants_IsActive ON Merchants(IsActive);
CREATE INDEX IX_Merchants_Name ON Merchants(Name);
CREATE INDEX IX_Merchants_Location ON Merchants(Latitude, Longitude);
CREATE INDEX IX_Merchants_Rating ON Merchants(Rating DESC);

-- PRODUCT CATEGORIES INDEXES
CREATE INDEX IX_ProductCategories_MerchantId ON ProductCategories(MerchantId);
CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId);
CREATE INDEX IX_ProductCategories_IsActive ON ProductCategories(IsActive);

-- PRODUCTS INDEXES
CREATE INDEX IX_Products_MerchantId ON Products(MerchantId);
CREATE INDEX IX_Products_ProductCategoryId ON Products(ProductCategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Products_Price ON Products(Price);
CREATE INDEX IX_Products_StockQuantity ON Products(StockQuantity);

-- PRODUCT OPTION GROUPS INDEXES
CREATE INDEX IX_ProductOptionGroups_ProductId ON ProductOptionGroups(ProductId);
CREATE INDEX IX_ProductOptionGroups_IsActive ON ProductOptionGroups(IsActive);

-- PRODUCT OPTIONS INDEXES
CREATE INDEX IX_ProductOptions_ProductOptionGroupId ON ProductOptions(ProductOptionGroupId);
CREATE INDEX IX_ProductOptions_IsActive ON ProductOptions(IsActive);

-- ORDERS INDEXES
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_MerchantId ON Orders(MerchantId);
CREATE INDEX IX_Orders_CourierId ON Orders(CourierId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);
-- IX_Orders_OrderNumber already exists as UNIQUE constraint
CREATE INDEX IX_Orders_Merchant_Status_Date ON Orders(MerchantId, Status, CreatedAt);

-- ORDER LINES INDEXES
CREATE INDEX IX_OrderLines_OrderId ON OrderLines(OrderId);
CREATE INDEX IX_OrderLines_ProductId ON OrderLines(ProductId);

-- ORDER LINE OPTIONS INDEXES
CREATE INDEX IX_OrderLineOptions_OrderLineId ON OrderLineOptions(OrderLineId);
CREATE INDEX IX_OrderLineOptions_ProductOptionId ON OrderLineOptions(ProductOptionId);

-- CART ITEMS INDEXES
CREATE INDEX IX_CartItems_UserId ON CartItems(UserId);
CREATE INDEX IX_CartItems_MerchantId ON CartItems(MerchantId);
CREATE INDEX IX_CartItems_ProductId ON CartItems(ProductId);

-- COUPONS INDEXES
-- IX_Coupons_Code already exists as UNIQUE constraint
CREATE INDEX IX_Coupons_IsActive ON Coupons(IsActive);
CREATE INDEX IX_Coupons_ValidFrom_ValidTo ON Coupons(StartDate, EndDate);

-- COUPON USAGES INDEXES
CREATE INDEX IX_CouponUsages_CouponId ON CouponUsages(CouponId);
CREATE INDEX IX_CouponUsages_UserId ON CouponUsages(UserId);
CREATE INDEX IX_CouponUsages_OrderId ON CouponUsages(OrderId);

-- REVIEWS INDEXES
CREATE INDEX IX_Reviews_ReviewerId ON Reviews(ReviewerId);
CREATE INDEX IX_Reviews_RevieweeId_RevieweeType ON Reviews(RevieweeId, RevieweeType);
CREATE INDEX IX_Reviews_OrderId ON Reviews(OrderId);
CREATE INDEX IX_Reviews_Rating ON Reviews(Rating);
CREATE INDEX IX_Reviews_IsApproved ON Reviews(IsApproved);

-- REVIEW TAGS INDEXES
CREATE INDEX IX_ReviewTags_ReviewId ON ReviewTags(ReviewId);
CREATE INDEX IX_ReviewTags_Tag ON ReviewTags(Tag);

-- REVIEW HELPFULS INDEXES
CREATE INDEX IX_ReviewHelpfuls_ReviewId ON ReviewHelpfuls(ReviewId);
CREATE INDEX IX_ReviewHelpfuls_UserId ON ReviewHelpfuls(UserId);

-- RATINGS INDEXES
CREATE INDEX IX_Ratings_EntityId_EntityType ON Ratings(EntityId, EntityType);
CREATE INDEX IX_Ratings_AverageRating ON Ratings(AverageRating DESC);

-- RATING HISTORIES INDEXES
CREATE INDEX IX_RatingHistories_EntityId_EntityType ON RatingHistories(EntityId, EntityType);
CREATE INDEX IX_RatingHistories_SnapshotDate ON RatingHistories(SnapshotDate);

-- NOTIFICATIONS INDEXES
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_Type ON Notifications(Type);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);

-- AUDIT LOGS INDEXES
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);

-- SYSTEM NOTIFICATIONS INDEXES
CREATE INDEX IX_SystemNotifications_IsActive ON SystemNotifications(IsActive);
CREATE INDEX IX_SystemNotifications_Priority ON SystemNotifications(Priority);
CREATE INDEX IX_SystemNotifications_CreatedAt ON SystemNotifications(CreatedAt);

-- COURIERS INDEXES
CREATE INDEX IX_Couriers_UserId ON Couriers(UserId);
CREATE INDEX IX_Couriers_IsActive ON Couriers(IsActive);
CREATE INDEX IX_Couriers_IsAvailable ON Couriers(IsAvailable);
CREATE INDEX IX_Couriers_Location ON Couriers(CurrentLatitude, CurrentLongitude);

-- WORKING HOURS INDEXES
CREATE INDEX IX_WorkingHours_MerchantId ON WorkingHours(MerchantId);
CREATE INDEX IX_WorkingHours_DayOfWeek ON WorkingHours(DayOfWeek);

-- DELIVERY ZONES INDEXES
CREATE INDEX IX_DeliveryZones_MerchantId ON DeliveryZones(MerchantId);
CREATE INDEX IX_DeliveryZones_IsActive ON DeliveryZones(IsActive);

-- DELIVERY ZONE POINTS INDEXES
CREATE INDEX IX_DeliveryZonePoints_DeliveryZoneId ON DeliveryZonePoints(DeliveryZoneId);
CREATE INDEX IX_DeliveryZonePoints_Order ON DeliveryZonePoints([Order]);

-- USER ADDRESSES INDEXES
CREATE INDEX IX_UserAddresses_UserId ON UserAddresses(UserId);
CREATE INDEX IX_UserAddresses_IsDefault ON UserAddresses(IsDefault);
CREATE INDEX IX_UserAddresses_IsActive ON UserAddresses(IsActive);

-- CAMPAIGNS INDEXES
CREATE INDEX IX_Campaigns_MerchantId ON Campaigns(MerchantId);
CREATE INDEX IX_Campaigns_IsActive ON Campaigns(IsActive);
CREATE INDEX IX_Campaigns_StartDate_EndDate ON Campaigns(StartDate, EndDate);

-- MERCHANT ONBOARDINGS INDEXES
CREATE INDEX IX_MerchantOnboardings_MerchantId ON MerchantOnboardings(MerchantId);
CREATE INDEX IX_MerchantOnboardings_OwnerId ON MerchantOnboardings(OwnerId);
CREATE INDEX IX_MerchantOnboardings_IsApproved ON MerchantOnboardings(IsApproved);

-- USER LOYALTY POINTS INDEXES
CREATE INDEX IX_UserLoyaltyPoints_UserId ON UserLoyaltyPoints(UserId);
CREATE INDEX IX_UserLoyaltyPoints_Points ON UserLoyaltyPoints(Points DESC);

-- LOYALTY POINT TRANSACTIONS INDEXES
CREATE INDEX IX_LoyaltyPointTransactions_UserId ON LoyaltyPointTransactions(UserId);
CREATE INDEX IX_LoyaltyPointTransactions_OrderId ON LoyaltyPointTransactions(OrderId);
CREATE INDEX IX_LoyaltyPointTransactions_Type ON LoyaltyPointTransactions(Type);
CREATE INDEX IX_LoyaltyPointTransactions_CreatedAt ON LoyaltyPointTransactions(CreatedAt);

-- SERVICE CATEGORIES INDEXES
CREATE INDEX IX_ServiceCategories_IsActive ON ServiceCategories(IsActive);
CREATE INDEX IX_ServiceCategories_DisplayOrder ON ServiceCategories(DisplayOrder);

PRINT 'Getir Database Schema created successfully with all performance indexes!';
GO
