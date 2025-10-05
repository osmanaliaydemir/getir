-- Migration 012: Stock Management System

-- 1. StockHistory Tablosu
CREATE TABLE StockHistories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ProductVariantId UNIQUEIDENTIFIER NULL,
    PreviousQuantity INT NOT NULL,
    NewQuantity INT NOT NULL,
    ChangeAmount INT NOT NULL,
    ChangeType INT NOT NULL, -- StockChangeType enum
    Reason NVARCHAR(500) NULL,
    Notes NVARCHAR(MAX) NULL,
    ChangedBy UNIQUEIDENTIFIER NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    OrderId UNIQUEIDENTIFIER NULL,
    ReferenceNumber NVARCHAR(100) NULL,
    UnitPrice DECIMAL(18, 2) NULL,
    TotalValue DECIMAL(18, 2) NULL,
    
    CONSTRAINT FK_StockHistories_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    -- CONSTRAINT FK_StockHistories_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES MarketProductVariants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockHistories_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(Id),
    CONSTRAINT FK_StockHistories_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE SET NULL
);
GO

-- 2. StockAlerts Tablosu
CREATE TABLE StockAlerts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ProductVariantId UNIQUEIDENTIFIER NULL,
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    CurrentStock INT NOT NULL,
    MinimumStock INT NOT NULL,
    MaximumStock INT NOT NULL,
    AlertType INT NOT NULL, -- StockAlertType enum
    Message NVARCHAR(500) NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ResolvedAt DATETIME2 NULL,
    ResolvedBy UNIQUEIDENTIFIER NULL,
    ResolutionNotes NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_StockAlerts_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    -- CONSTRAINT FK_StockAlerts_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES MarketProductVariants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockAlerts_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_StockAlerts_ResolvedBy FOREIGN KEY (ResolvedBy) REFERENCES Users(Id)
);
GO

-- 3. StockSettings Tablosu
CREATE TABLE StockSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    AutoStockReduction BIT NOT NULL DEFAULT 1,
    LowStockAlerts BIT NOT NULL DEFAULT 1,
    OverstockAlerts BIT NOT NULL DEFAULT 0,
    DefaultMinimumStock INT NOT NULL DEFAULT 10,
    DefaultMaximumStock INT NOT NULL DEFAULT 1000,
    EnableStockSync BIT NOT NULL DEFAULT 0,
    ExternalSystemId NVARCHAR(100) NULL,
    SyncApiKey NVARCHAR(500) NULL,
    SyncApiUrl NVARCHAR(500) NULL,
    LastSyncAt DATETIME2 NULL,
    SyncIntervalMinutes INT NOT NULL DEFAULT 60,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_StockSettings_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_StockSettings_MerchantId UNIQUE (MerchantId)
);
GO

-- 4. Index'ler (performans için)
CREATE INDEX IX_StockHistories_ProductId ON StockHistories (ProductId);
CREATE INDEX IX_StockHistories_ProductVariantId ON StockHistories (ProductVariantId);
CREATE INDEX IX_StockHistories_ChangedAt ON StockHistories (ChangedAt);
CREATE INDEX IX_StockHistories_ChangeType ON StockHistories (ChangeType);
CREATE INDEX IX_StockHistories_OrderId ON StockHistories (OrderId);
CREATE INDEX IX_StockHistories_ChangedBy ON StockHistories (ChangedBy);
GO

CREATE INDEX IX_StockAlerts_ProductId ON StockAlerts (ProductId);
CREATE INDEX IX_StockAlerts_ProductVariantId ON StockAlerts (ProductVariantId);
CREATE INDEX IX_StockAlerts_MerchantId ON StockAlerts (MerchantId);
CREATE INDEX IX_StockAlerts_AlertType ON StockAlerts (AlertType);
CREATE INDEX IX_StockAlerts_IsResolved ON StockAlerts (IsResolved);
CREATE INDEX IX_StockAlerts_IsActive ON StockAlerts (IsActive);
CREATE INDEX IX_StockAlerts_CreatedAt ON StockAlerts (CreatedAt);
GO

CREATE INDEX IX_StockSettings_MerchantId ON StockSettings (MerchantId);
CREATE INDEX IX_StockSettings_IsActive ON StockSettings (IsActive);
GO

-- 5. Check constraints
ALTER TABLE StockHistories 
ADD CONSTRAINT CK_StockHistories_ChangeAmount 
CHECK (ChangeAmount = NewQuantity - PreviousQuantity);
GO

ALTER TABLE StockHistories 
ADD CONSTRAINT CK_StockHistories_ValidChangeType 
CHECK (ChangeType >= 0 AND ChangeType <= 9);
GO

ALTER TABLE StockAlerts 
ADD CONSTRAINT CK_StockAlerts_ValidAlertType 
CHECK (AlertType >= 0 AND AlertType <= 5);
GO

ALTER TABLE StockAlerts 
ADD CONSTRAINT CK_StockAlerts_ValidStock 
CHECK (CurrentStock >= 0 AND MinimumStock >= 0 AND MaximumStock >= 0);
GO

ALTER TABLE StockSettings 
ADD CONSTRAINT CK_StockSettings_ValidStock 
CHECK (DefaultMinimumStock >= 0 AND DefaultMaximumStock >= 0);
GO

-- 6. View for stock summary
CREATE VIEW vw_StockSummary AS
SELECT 
    p.Id AS ProductId,
    p.MerchantId,
    p.Name AS ProductName,
    p.StockQuantity AS CurrentStock,
    p.Price AS UnitPrice,
    p.StockQuantity * p.Price AS TotalValue,
    CASE 
        WHEN p.StockQuantity = 0 THEN 'OutOfStock'
        WHEN p.StockQuantity <= 10 THEN 'LowStock'
        WHEN p.StockQuantity > 100 THEN 'Overstock'
        ELSE 'InStock'
    END AS StockStatus,
    p.UpdatedAt AS LastUpdated,
    ISNULL(ss.DefaultMinimumStock, 10) AS MinimumStock,
    ISNULL(ss.DefaultMaximumStock, 1000) AS MaximumStock
FROM Products p
    LEFT JOIN StockSettings ss ON p.MerchantId = ss.MerchantId AND ss.IsActive = 1
WHERE p.IsActive = 1;
GO

-- 7. View for stock alerts summary
CREATE VIEW vw_StockAlertsSummary AS
SELECT 
    sa.Id,
    sa.ProductId,
    sa.ProductVariantId,
    sa.MerchantId,
    p.Name AS ProductName,
        NULL AS VariantName,
    sa.CurrentStock,
    sa.MinimumStock,
    sa.MaximumStock,
    sa.AlertType,
    sa.Message,
    sa.CreatedAt,
    sa.IsResolved,
    sa.IsActive
FROM StockAlerts sa
INNER JOIN Products p ON sa.ProductId = p.Id
    -- LEFT JOIN MarketProductVariants mpv ON sa.ProductVariantId = mpv.Id
WHERE sa.IsActive = 1;
GO

-- 8. Stored procedure for stock movement report
CREATE PROCEDURE sp_GetStockMovementReport
    @MerchantId UNIQUEIDENTIFIER,
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @ChangeType INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sh.Id,
        sh.ProductId,
        sh.ProductVariantId,
        p.Name AS ProductName,
        NULL AS VariantName,
        sh.PreviousQuantity,
        sh.NewQuantity,
        sh.ChangeAmount,
        sh.ChangeType,
        sh.Reason,
        sh.ChangedAt,
        sh.OrderId,
        sh.ReferenceNumber,
        u.FirstName + ' ' + u.LastName AS ChangedByName
    FROM StockHistories sh
    INNER JOIN Products p ON sh.ProductId = p.Id
    -- LEFT JOIN MarketProductVariants mpv ON sh.ProductVariantId = mpv.Id
    LEFT JOIN Users u ON sh.ChangedBy = u.Id
    WHERE p.MerchantId = @MerchantId
        AND (@FromDate IS NULL OR sh.ChangedAt >= @FromDate)
        AND (@ToDate IS NULL OR sh.ChangedAt <= @ToDate)
        AND (@ChangeType IS NULL OR sh.ChangeType = @ChangeType)
    ORDER BY sh.ChangedAt DESC;
END
GO

-- 9. Stored procedure for low stock check
CREATE PROCEDURE sp_CheckLowStock
    @MerchantId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DefaultMinimumStock INT = 10;
    
    -- Get minimum stock from settings
    SELECT @DefaultMinimumStock = DefaultMinimumStock
    FROM StockSettings
    WHERE MerchantId = @MerchantId AND IsActive = 1;
    
    -- Find products with low stock
    SELECT 
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.StockQuantity AS CurrentStock,
        @DefaultMinimumStock AS MinimumStock,
        CASE 
            WHEN p.StockQuantity = 0 THEN 0 -- OutOfStock
            ELSE 1 -- LowStock
        END AS AlertType
    FROM Products p
    WHERE p.MerchantId = @MerchantId
        AND p.IsActive = 1
        AND p.StockQuantity <= @DefaultMinimumStock
        AND NOT EXISTS (
            SELECT 1 FROM StockAlerts sa 
            WHERE sa.ProductId = p.Id 
                AND sa.IsResolved = 0 
                AND sa.IsActive = 1
        );
END
GO

-- 10. Trigger for automatic stock history logging
CREATE TRIGGER TR_Products_StockChange
ON Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only log if stock quantity actually changed
    IF UPDATE(StockQuantity) AND EXISTS (
        SELECT 1 FROM inserted i 
        INNER JOIN deleted d ON i.Id = d.Id 
        WHERE i.StockQuantity != d.StockQuantity
    )
    BEGIN
        INSERT INTO StockHistories (
            ProductId, ProductVariantId, PreviousQuantity, NewQuantity, 
            ChangeAmount, ChangeType, Reason, ChangedAt, ReferenceNumber
        )
        SELECT 
            i.Id,
            NULL, -- Product variant ID
            d.StockQuantity,
            i.StockQuantity,
            i.StockQuantity - d.StockQuantity,
            2, -- ManualAdjustment
            'Automatic stock change detected',
            GETUTCDATE(),
            'SYSTEM'
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.StockQuantity != d.StockQuantity;
    END
END
GO

-- 11. Trigger for MarketProductVariants stock change (commented out - table doesn't exist)
-- CREATE TRIGGER TR_MarketProductVariants_StockChange
-- ON MarketProductVariants
-- AFTER UPDATE
-- AS
-- BEGIN
--     SET NOCOUNT ON;
--     
--     -- Only log if stock quantity actually changed
--     IF UPDATE(StockQuantity) AND EXISTS (
--         SELECT 1 FROM inserted i 
--         INNER JOIN deleted d ON i.Id = d.Id 
--         WHERE i.StockQuantity != d.StockQuantity
--     )
--     BEGIN
--         INSERT INTO StockHistories (
--             ProductId, ProductVariantId, PreviousQuantity, NewQuantity, 
--             ChangeAmount, ChangeType, Reason, ChangedAt, ReferenceNumber
--         )
--         SELECT 
--             i.ProductId,
--             i.Id,
--             d.StockQuantity,
--             i.StockQuantity,
--             i.StockQuantity - d.StockQuantity,
--             2, -- ManualAdjustment
--             'Automatic variant stock change detected',
--             GETUTCDATE(),
--             'SYSTEM'
--         FROM inserted i
--         INNER JOIN deleted d ON i.Id = d.Id
--         WHERE i.StockQuantity != d.StockQuantity;
--     END
-- END
-- GO

-- 12. Insert default stock settings for existing merchants
INSERT INTO StockSettings (MerchantId, AutoStockReduction, LowStockAlerts, OverstockAlerts, DefaultMinimumStock, DefaultMaximumStock)
SELECT 
    Id,
    1, -- AutoStockReduction
    1, -- LowStockAlerts
    0, -- OverstockAlerts
    10, -- DefaultMinimumStock
    1000 -- DefaultMaximumStock
FROM Merchants
WHERE NOT EXISTS (
    SELECT 1 FROM StockSettings ss WHERE ss.MerchantId = Merchants.Id
);
GO

PRINT 'Migration 012 completed successfully - Stock Management System created';
