-- =============================================
-- Getir Clone - SQL Server Database Schema
-- DB-First yaklaşım için temel tablolar
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
-- USERS & AUTHENTICATION
-- =============================================

CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    LastLoginAt DATETIME2 NULL,
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%@%')
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(512) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2 NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_ExpiresAt ON RefreshTokens(ExpiresAt);

-- =============================================
-- CATEGORIES
-- =============================================

CREATE TABLE Categories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);
CREATE INDEX IX_Categories_DisplayOrder ON Categories(DisplayOrder);

-- =============================================
-- MERCHANTS (Market & Restoran)
-- =============================================

CREATE TABLE Merchants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    LogoUrl NVARCHAR(500) NULL,
    CoverImageUrl NVARCHAR(500) NULL,
    Address NVARCHAR(500) NOT NULL,
    Latitude DECIMAL(10, 8) NOT NULL,
    Longitude DECIMAL(11, 8) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(256) NULL,
    MinimumOrderAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    DeliveryFee DECIMAL(18, 2) NOT NULL DEFAULT 0,
    AverageDeliveryTime INT NOT NULL DEFAULT 30, -- dakika
    Rating DECIMAL(3, 2) NULL DEFAULT 0.00,
    TotalReviews INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsBusy BIT NOT NULL DEFAULT 0,
    IsOpen BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Merchants_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT CK_Merchants_Rating CHECK (Rating >= 0 AND Rating <= 5)
);

CREATE INDEX IX_Merchants_CategoryId ON Merchants(CategoryId);
CREATE INDEX IX_Merchants_IsActive ON Merchants(IsActive);
CREATE INDEX IX_Merchants_Location ON Merchants(Latitude, Longitude);
CREATE INDEX IX_Merchants_Rating ON Merchants(Rating DESC);

-- =============================================
-- PRODUCTS
-- =============================================

CREATE TABLE Products (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL,
    Price DECIMAL(18, 2) NOT NULL,
    DiscountedPrice DECIMAL(18, 2) NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    Unit NVARCHAR(50) NULL, -- adet, kg, lt, etc.
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Products_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
);

CREATE INDEX IX_Products_MerchantId ON Products(MerchantId);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_Price ON Products(Price);

-- =============================================
-- ORDERS
-- =============================================

CREATE TABLE Orders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    UserId UNIQUEIDENTIFIER NOT NULL,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', 
    -- Pending, Confirmed, Preparing, Ready, OnTheWay, Delivered, Cancelled
    SubTotal DECIMAL(18, 2) NOT NULL,
    DeliveryFee DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Discount DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Total DECIMAL(18, 2) NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL, -- CreditCard, Cash, Wallet
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, Failed, Refunded
    DeliveryAddress NVARCHAR(500) NOT NULL,
    DeliveryLatitude DECIMAL(10, 8) NOT NULL,
    DeliveryLongitude DECIMAL(11, 8) NOT NULL,
    EstimatedDeliveryTime DATETIME2 NULL,
    ActualDeliveryTime DATETIME2 NULL,
    Notes NVARCHAR(MAX) NULL,
    CancellationReason NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Orders_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT CK_Orders_Total CHECK (Total >= 0)
);

CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_MerchantId ON Orders(MerchantId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt DESC);

-- =============================================
-- ORDER LINES
-- =============================================

CREATE TABLE OrderLines (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ProductName NVARCHAR(200) NOT NULL, -- snapshot
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_OrderLines_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderLines_UnitPrice CHECK (UnitPrice >= 0)
);

CREATE INDEX IX_OrderLines_OrderId ON OrderLines(OrderId);
CREATE INDEX IX_OrderLines_ProductId ON OrderLines(ProductId);

GO

-- =============================================
-- SEED DATA (Örnek veriler)
-- =============================================

-- Örnek kategori
INSERT INTO Categories (Id, Name, Description, DisplayOrder, IsActive) 
VALUES 
    (NEWID(), 'Market', 'Günlük ihtiyaç ürünleri', 1, 1),
    (NEWID(), 'Restoran', 'Yemek siparişleri', 2, 1);

GO

PRINT 'Database schema created successfully!';
