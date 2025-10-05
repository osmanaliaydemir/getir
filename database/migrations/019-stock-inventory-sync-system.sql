-- Migration 019: Stock Inventory and Sync System Extensions

-- 1. Inventory Count Session Table
CREATE TABLE InventoryCountSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    CountDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CountType INT NOT NULL, -- InventoryCountType enum
    Status INT NOT NULL DEFAULT 0, -- InventoryCountStatus enum
    DiscrepancyCount INT NOT NULL DEFAULT 0,
    Notes NVARCHAR(MAX) NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_InventoryCountSessions_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InventoryCountSessions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT CK_InventoryCountSessions_ValidCountType CHECK (CountType >= 0 AND CountType <= 4),
    CONSTRAINT CK_InventoryCountSessions_ValidStatus CHECK (Status >= 0 AND Status <= 4)
);
GO

-- 2. Inventory Count Item Table
CREATE TABLE InventoryCountItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CountSessionId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ProductVariantId UNIQUEIDENTIFIER NULL,
    ExpectedQuantity INT NOT NULL,
    CountedQuantity INT NOT NULL,
    Variance INT NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_InventoryCountItems_CountSession FOREIGN KEY (CountSessionId) REFERENCES InventoryCountSessions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InventoryCountItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_InventoryCountItems_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES MarketProductVariants(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_InventoryCountItems_ValidQuantities CHECK (ExpectedQuantity >= 0 AND CountedQuantity >= 0)
);
GO

-- 3. Inventory Discrepancy Table
CREATE TABLE InventoryDiscrepancies (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CountSessionId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ProductVariantId UNIQUEIDENTIFIER NULL,
    ExpectedQuantity INT NOT NULL,
    ActualQuantity INT NOT NULL,
    Variance INT NOT NULL,
    VariancePercentage DECIMAL(5, 2) NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- InventoryDiscrepancyStatus enum
    ResolutionNotes NVARCHAR(MAX) NULL,
    ResolvedBy UNIQUEIDENTIFIER NULL,
    ResolvedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_InventoryDiscrepancies_CountSession FOREIGN KEY (CountSessionId) REFERENCES InventoryCountSessions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InventoryDiscrepancies_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_InventoryDiscrepancies_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES MarketProductVariants(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_InventoryDiscrepancies_ResolvedBy FOREIGN KEY (ResolvedBy) REFERENCES Users(Id),
    CONSTRAINT CK_InventoryDiscrepancies_ValidStatus CHECK (Status >= 0 AND Status <= 4),
    CONSTRAINT CK_InventoryDiscrepancies_ValidQuantities CHECK (ExpectedQuantity >= 0 AND ActualQuantity >= 0)
);
GO

-- 4. Stock Sync Session Table
CREATE TABLE StockSyncSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    ExternalSystemId NVARCHAR(100) NOT NULL,
    SyncType INT NOT NULL, -- StockSyncType enum
    Status INT NOT NULL DEFAULT 0, -- StockSyncStatus enum
    StartedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    SyncedItemsCount INT NOT NULL DEFAULT 0,
    FailedItemsCount INT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_StockSyncSessions_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT CK_StockSyncSessions_ValidSyncType CHECK (SyncType >= 0 AND SyncType <= 3),
    CONSTRAINT CK_StockSyncSessions_ValidStatus CHECK (Status >= 0 AND Status <= 4)
);
GO

-- 5. Stock Sync Detail Table
CREATE TABLE StockSyncDetails (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SyncSessionId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    ExternalProductId NVARCHAR(100) NOT NULL,
    ExternalVariantId NVARCHAR(100) NULL,
    PreviousQuantity INT NOT NULL,
    NewQuantity INT NOT NULL,
    QuantityDifference INT NOT NULL,
    SyncStatus INT NOT NULL DEFAULT 0, -- StockSyncDetailStatus enum
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_StockSyncDetails_SyncSession FOREIGN KEY (SyncSessionId) REFERENCES StockSyncSessions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockSyncDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_StockSyncDetails_ValidSyncStatus CHECK (SyncStatus >= 0 AND SyncStatus <= 3),
    CONSTRAINT CK_StockSyncDetails_ValidQuantities CHECK (PreviousQuantity >= 0 AND NewQuantity >= 0)
);
GO

-- 6. Add UpdatedAt column to StockAlerts table
ALTER TABLE StockAlerts ADD UpdatedAt DATETIME2 NULL;
GO

-- 7. Add ExternalId column to Products table
ALTER TABLE Products ADD ExternalId NVARCHAR(100) NULL;
GO

-- 8. Update StockChangeType constraint to include new Sync type
ALTER TABLE StockHistories 
DROP CONSTRAINT CK_StockHistories_ValidChangeType;
GO

ALTER TABLE StockHistories 
ADD CONSTRAINT CK_StockHistories_ValidChangeType 
CHECK (ChangeType >= 0 AND ChangeType <= 10);
GO

-- 9. Indexes for performance
CREATE INDEX IX_InventoryCountSessions_MerchantId ON InventoryCountSessions (MerchantId);
CREATE INDEX IX_InventoryCountSessions_CountDate ON InventoryCountSessions (CountDate);
CREATE INDEX IX_InventoryCountSessions_Status ON InventoryCountSessions (Status);
CREATE INDEX IX_InventoryCountSessions_CreatedBy ON InventoryCountSessions (CreatedBy);
GO

CREATE INDEX IX_InventoryCountItems_CountSessionId ON InventoryCountItems (CountSessionId);
CREATE INDEX IX_InventoryCountItems_ProductId ON InventoryCountItems (ProductId);
CREATE INDEX IX_InventoryCountItems_ProductVariantId ON InventoryCountItems (ProductVariantId);
GO

CREATE INDEX IX_InventoryDiscrepancies_CountSessionId ON InventoryDiscrepancies (CountSessionId);
CREATE INDEX IX_InventoryDiscrepancies_ProductId ON InventoryDiscrepancies (ProductId);
CREATE INDEX IX_InventoryDiscrepancies_ProductVariantId ON InventoryDiscrepancies (ProductVariantId);
CREATE INDEX IX_InventoryDiscrepancies_Status ON InventoryDiscrepancies (Status);
CREATE INDEX IX_InventoryDiscrepancies_CreatedAt ON InventoryDiscrepancies (CreatedAt);
GO

CREATE INDEX IX_StockSyncSessions_MerchantId ON StockSyncSessions (MerchantId);
CREATE INDEX IX_StockSyncSessions_ExternalSystemId ON StockSyncSessions (ExternalSystemId);
CREATE INDEX IX_StockSyncSessions_StartedAt ON StockSyncSessions (StartedAt);
CREATE INDEX IX_StockSyncSessions_Status ON StockSyncSessions (Status);
GO

CREATE INDEX IX_StockSyncDetails_SyncSessionId ON StockSyncDetails (SyncSessionId);
CREATE INDEX IX_StockSyncDetails_ProductId ON StockSyncDetails (ProductId);
CREATE INDEX IX_StockSyncDetails_ExternalProductId ON StockSyncDetails (ExternalProductId);
CREATE INDEX IX_StockSyncDetails_SyncStatus ON StockSyncDetails (SyncStatus);
GO

-- 10. Views for inventory management
CREATE VIEW vw_InventoryCountSummary AS
SELECT 
    ics.Id,
    ics.MerchantId,
    m.Name AS MerchantName,
    ics.CountDate,
    ics.CountType,
    ics.Status,
    ics.DiscrepancyCount,
    ics.CreatedAt,
    ics.CompletedAt,
    u.FirstName + ' ' + u.LastName AS CreatedByName
FROM InventoryCountSessions ics
INNER JOIN Merchants m ON ics.MerchantId = m.Id
INNER JOIN Users u ON ics.CreatedBy = u.Id;
GO

CREATE VIEW vw_InventoryDiscrepanciesSummary AS
SELECT 
    id.Id,
    id.CountSessionId,
    id.ProductId,
    id.ProductVariantId,
    p.Name AS ProductName,
    mpv.Name AS VariantName,
    id.ExpectedQuantity,
    id.ActualQuantity,
    id.Variance,
    id.VariancePercentage,
    id.Status,
    id.CreatedAt,
    id.ResolvedAt
FROM InventoryDiscrepancies id
INNER JOIN Products p ON id.ProductId = p.Id
LEFT JOIN MarketProductVariants mpv ON id.ProductVariantId = mpv.Id;
GO

CREATE VIEW vw_StockSyncSummary AS
SELECT 
    sss.Id,
    sss.MerchantId,
    m.Name AS MerchantName,
    sss.ExternalSystemId,
    sss.SyncType,
    sss.Status,
    sss.StartedAt,
    sss.CompletedAt,
    sss.SyncedItemsCount,
    sss.FailedItemsCount,
    sss.ErrorMessage
FROM StockSyncSessions sss
INNER JOIN Merchants m ON sss.MerchantId = m.Id;
GO

-- 11. Stored procedures for inventory management
CREATE PROCEDURE sp_GetInventoryTurnoverReport
    @MerchantId UNIQUEIDENTIFIER,
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.StockQuantity AS CurrentStock,
        p.Price AS UnitPrice,
        p.StockQuantity * p.Price AS StockValue,
        ISNULL(SUM(CASE WHEN sh.ChangeAmount < 0 THEN ABS(sh.ChangeAmount) ELSE 0 END), 0) AS StockOutQuantity,
        CASE 
            WHEN p.StockQuantity > 0 THEN 
                CAST(ISNULL(SUM(CASE WHEN sh.ChangeAmount < 0 THEN ABS(sh.ChangeAmount) ELSE 0 END), 0) AS DECIMAL(18,2)) / p.StockQuantity
            ELSE 999
        END AS TurnoverRate,
        CASE 
            WHEN p.StockQuantity > 0 AND ISNULL(SUM(CASE WHEN sh.ChangeAmount < 0 THEN ABS(sh.ChangeAmount) ELSE 0 END), 0) > 0 THEN 
                365.0 / (CAST(ISNULL(SUM(CASE WHEN sh.ChangeAmount < 0 THEN ABS(sh.ChangeAmount) ELSE 0 END), 0) AS DECIMAL(18,2)) / p.StockQuantity)
            ELSE 999
        END AS DaysToTurnover
    FROM Products p
    LEFT JOIN StockHistories sh ON p.Id = sh.ProductId 
        AND sh.ChangedAt >= @FromDate 
        AND sh.ChangedAt <= @ToDate
    WHERE p.MerchantId = @MerchantId 
        AND p.IsActive = 1
    GROUP BY p.Id, p.Name, p.StockQuantity, p.Price
    ORDER BY DaysToTurnover;
END
GO

CREATE PROCEDURE sp_GetSlowMovingInventory
    @MerchantId UNIQUEIDENTIFIER,
    @DaysThreshold INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME2 = DATEADD(DAY, -@DaysThreshold, GETUTCDATE());
    
    SELECT 
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.StockQuantity AS CurrentStock,
        p.Price AS UnitPrice,
        p.StockQuantity * p.Price AS TotalValue,
        MAX(sh.ChangedAt) AS LastMovementDate,
        CASE 
            WHEN MAX(sh.ChangedAt) IS NULL THEN DATEDIFF(DAY, p.CreatedAt, GETUTCDATE())
            ELSE DATEDIFF(DAY, MAX(sh.ChangedAt), GETUTCDATE())
        END AS DaysSinceLastMovement
    FROM Products p
    LEFT JOIN StockHistories sh ON p.Id = sh.ProductId
    WHERE p.MerchantId = @MerchantId 
        AND p.IsActive = 1
    GROUP BY p.Id, p.Name, p.StockQuantity, p.Price, p.CreatedAt
    HAVING MAX(sh.ChangedAt) IS NULL OR MAX(sh.ChangedAt) < @CutoffDate
    ORDER BY DaysSinceLastMovement DESC;
END
GO

CREATE PROCEDURE sp_GetInventoryValuation
    @MerchantId UNIQUEIDENTIFIER,
    @Method INT = 0 -- ValuationMethod enum (0 = FIFO)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.Id AS ProductId,
        p.Name AS ProductName,
        p.StockQuantity AS Quantity,
        p.Price AS UnitValue,
        p.StockQuantity * p.Price AS TotalValue
    FROM Products p
    WHERE p.MerchantId = @MerchantId 
        AND p.IsActive = 1
    ORDER BY p.StockQuantity * p.Price DESC;
END
GO

-- 12. Trigger for inventory count completion
CREATE TRIGGER TR_InventoryCountSessions_Completion
ON InventoryCountSessions
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update CompletedAt when status changes to Completed
    IF UPDATE(Status) AND EXISTS (
        SELECT 1 FROM inserted i 
        INNER JOIN deleted d ON i.Id = d.Id 
        WHERE i.Status = 1 AND d.Status != 1 -- Completed
    )
    BEGIN
        UPDATE InventoryCountSessions 
        SET CompletedAt = GETUTCDATE(),
            UpdatedAt = GETUTCDATE()
        WHERE Id IN (
            SELECT i.Id FROM inserted i 
            INNER JOIN deleted d ON i.Id = d.Id 
            WHERE i.Status = 1 AND d.Status != 1
        );
    END
END
GO

-- 13. Trigger for stock sync completion
CREATE TRIGGER TR_StockSyncSessions_Completion
ON StockSyncSessions
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update CompletedAt when status changes to Success, PartialSuccess, or Failed
    IF UPDATE(Status) AND EXISTS (
        SELECT 1 FROM inserted i 
        INNER JOIN deleted d ON i.Id = d.Id 
        WHERE i.Status IN (1, 2, 3) AND d.Status = 0 -- Success, PartialSuccess, Failed from InProgress
    )
    BEGIN
        UPDATE StockSyncSessions 
        SET CompletedAt = GETUTCDATE(),
            UpdatedAt = GETUTCDATE()
        WHERE Id IN (
            SELECT i.Id FROM inserted i 
            INNER JOIN deleted d ON i.Id = d.Id 
            WHERE i.Status IN (1, 2, 3) AND d.Status = 0
        );
    END
END
GO

-- 14. Sample data for testing
-- Insert sample inventory count session
INSERT INTO InventoryCountSessions (MerchantId, CountType, Status, CreatedBy)
SELECT TOP 1 
    Id, -- MerchantId
    0, -- Full count
    1, -- Completed
    (SELECT TOP 1 Id FROM Users ORDER BY CreatedAt DESC) -- CreatedBy
FROM Merchants
WHERE NOT EXISTS (SELECT 1 FROM InventoryCountSessions);
GO

-- Insert sample stock sync session
INSERT INTO StockSyncSessions (MerchantId, ExternalSystemId, SyncType, Status, SyncedItemsCount)
SELECT TOP 1 
    Id, -- MerchantId
    'ERP_SYSTEM_001', -- ExternalSystemId
    0, -- Manual sync
    1, -- Success
    5 -- SyncedItemsCount
FROM Merchants
WHERE NOT EXISTS (SELECT 1 FROM StockSyncSessions);
GO

PRINT 'Migration 019 completed successfully - Stock Inventory and Sync System Extensions created';
