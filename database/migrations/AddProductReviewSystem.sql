-- =============================================
-- Product Review System Migration
-- Adds Rating, ReviewCount to Products table
-- Creates ProductReviews and ProductReviewHelpfuls tables
-- =============================================

-- 1. Add Rating and ReviewCount columns to Products table
ALTER TABLE Products
ADD Rating DECIMAL(3, 2) NULL,
    ReviewCount INT NULL;

-- Create indexes for performance
CREATE INDEX IX_Products_Rating ON Products(Rating);
CREATE INDEX IX_Products_ReviewCount ON Products(ReviewCount);

-- 2. Create ProductReviews table
CREATE TABLE ProductReviews (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrderId UNIQUEIDENTIFIER NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(1000) NULL,
    ImageUrls NVARCHAR(2000) NULL, -- JSON array
    IsVerifiedPurchase BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Moderation
    IsApproved BIT NOT NULL DEFAULT 1,
    IsModerated BIT NOT NULL DEFAULT 0,
    ModerationNotes NVARCHAR(500) NULL,
    ModeratedBy UNIQUEIDENTIFIER NULL,
    ModeratedAt DATETIME2 NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    -- Helpful counts
    HelpfulCount INT NOT NULL DEFAULT 0,
    NotHelpfulCount INT NOT NULL DEFAULT 0,
    
    -- Foreign keys
    CONSTRAINT FK_ProductReviews_Products FOREIGN KEY (ProductId) 
        REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductReviews_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id),
    CONSTRAINT FK_ProductReviews_Orders FOREIGN KEY (OrderId) 
        REFERENCES Orders(Id),
    CONSTRAINT FK_ProductReviews_Moderator FOREIGN KEY (ModeratedBy) 
        REFERENCES Users(Id)
);

-- ProductReviews indexes
CREATE INDEX IX_ProductReviews_ProductId ON ProductReviews(ProductId);
CREATE INDEX IX_ProductReviews_UserId ON ProductReviews(UserId);
CREATE INDEX IX_ProductReviews_OrderId ON ProductReviews(OrderId);
CREATE INDEX IX_ProductReviews_ProductId_Rating ON ProductReviews(ProductId, Rating);
CREATE INDEX IX_ProductReviews_ProductId_CreatedAt ON ProductReviews(ProductId, CreatedAt);
CREATE INDEX IX_ProductReviews_IsApproved ON ProductReviews(IsApproved);
CREATE INDEX IX_ProductReviews_IsDeleted ON ProductReviews(IsDeleted);

-- Unique constraint: Bir kullanıcı bir ürüne sadece bir review yapabilir
CREATE UNIQUE INDEX IX_ProductReviews_UserId_ProductId 
    ON ProductReviews(UserId, ProductId) 
    WHERE IsDeleted = 0;

-- 3. Create ProductReviewHelpfuls table
CREATE TABLE ProductReviewHelpfuls (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductReviewId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    IsHelpful BIT NOT NULL, -- true = helpful, false = not helpful
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Foreign keys
    CONSTRAINT FK_ProductReviewHelpfuls_ProductReviews FOREIGN KEY (ProductReviewId) 
        REFERENCES ProductReviews(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductReviewHelpfuls_Users FOREIGN KEY (UserId) 
        REFERENCES Users(Id)
);

-- ProductReviewHelpfuls indexes
CREATE INDEX IX_ProductReviewHelpfuls_ProductReviewId ON ProductReviewHelpfuls(ProductReviewId);
CREATE INDEX IX_ProductReviewHelpfuls_IsHelpful ON ProductReviewHelpfuls(IsHelpful);

-- Unique constraint: Bir kullanıcı bir review'a sadece bir kez oy verebilir
CREATE UNIQUE INDEX IX_ProductReviewHelpfuls_ProductReviewId_UserId 
    ON ProductReviewHelpfuls(ProductReviewId, UserId);

GO

-- =============================================
-- Product Rating Hesaplama Stored Procedure
-- =============================================
CREATE OR ALTER PROCEDURE sp_RecalculateProductRating
    @ProductId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AverageRating DECIMAL(3, 2);
    DECLARE @ReviewCount INT;
    
    -- Onaylı ve silinmemiş review'ları kullanarak hesapla
    SELECT 
        @AverageRating = ROUND(AVG(CAST(Rating AS DECIMAL(3, 2))), 2),
        @ReviewCount = COUNT(*)
    FROM ProductReviews
    WHERE ProductId = @ProductId
        AND IsDeleted = 0
        AND IsApproved = 1;
    
    -- Product tablosunu güncelle
    UPDATE Products
    SET 
        Rating = @AverageRating,
        ReviewCount = @ReviewCount,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @ProductId;
    
    -- Sonuçları döndür
    SELECT 
        @ProductId AS ProductId,
        @AverageRating AS Rating,
        @ReviewCount AS ReviewCount;
END;
GO

-- =============================================
-- ProductReview Trigger: Review eklenince/güncellenince rating hesapla
-- =============================================
CREATE OR ALTER TRIGGER trg_ProductReview_AfterInsertUpdate
ON ProductReviews
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Sadece Rating, IsApproved veya IsDeleted değiştiğinde çalış
    IF UPDATE(Rating) OR UPDATE(IsApproved) OR UPDATE(IsDeleted)
    BEGIN
        DECLARE @ProductId UNIQUEIDENTIFIER;
        
        -- Etkilenen ProductId'leri al
        SELECT DISTINCT @ProductId = ProductId
        FROM inserted;
        
        -- Rating'i yeniden hesapla
        EXEC sp_RecalculateProductRating @ProductId;
    END
END;
GO

-- =============================================
-- Örnek test data (opsiyonel - silebilirsin)
-- =============================================
-- İlk product'a örnek review'lar
/*
DECLARE @FirstProductId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE IsActive = 1);
DECLARE @FirstUserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Role = 0); -- Customer
DECLARE @FirstOrderId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Orders WHERE Status = 4); -- Delivered

IF @FirstProductId IS NOT NULL AND @FirstUserId IS NOT NULL AND @FirstOrderId IS NOT NULL
BEGIN
    INSERT INTO ProductReviews (Id, ProductId, UserId, OrderId, Rating, Comment, IsVerifiedPurchase)
    VALUES 
        (NEWID(), @FirstProductId, @FirstUserId, @FirstOrderId, 5, 'Harika ürün! Çok memnun kaldım.', 1),
        (NEWID(), @FirstProductId, @FirstUserId, @FirstOrderId, 4, 'İyi ama biraz pahalı.', 1);
END;
*/

PRINT 'Product Review System migration completed successfully!';

