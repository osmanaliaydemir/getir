-- =============================================
-- Payment System Migration - v1.0
-- Date: 2024-12-19
-- Description: Payment entities ve ilişkileri için migration
-- =============================================

USE GetirDb;
GO

-- =============================================
-- ENUM DEĞERLERİ (Yorum olarak)
-- =============================================

/*
PaymentMethod enum değerleri:
1=Cash, 2=CreditCard, 3=VodafonePay, 4=BankTransfer, 5=BkmExpress, 6=Papara, 7=QrCode

PaymentStatus enum değerleri:
0=Pending, 1=Completed, 2=Failed, 3=Cancelled, 4=Refunded, 5=Processing, 6=Expired
*/

-- =============================================
-- PAYMENTS TABLE
-- =============================================

CREATE TABLE Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    
    -- Payment Method & Status
    PaymentMethod INT NOT NULL DEFAULT 1, -- PaymentMethod enum (default: Cash)
    Status INT NOT NULL DEFAULT 0, -- PaymentStatus enum (default: Pending)
    
    -- Amount Information
    Amount DECIMAL(18, 2) NOT NULL,
    ChangeAmount DECIMAL(18, 2) NULL, -- Para üstü (sadece Cash için)
    
    -- Timestamps
    ProcessedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    CollectedAt DATETIME2 NULL, -- Kurye tarafından para toplanma tarihi (Cash için)
    SettledAt DATETIME2 NULL, -- Merchant'a ödeme aktarım tarihi
    
    -- Courier Information (Cash için)
    CollectedByCourierId UNIQUEIDENTIFIER NULL,
    
    -- Notes & Failure Information
    Notes NVARCHAR(500) NULL,
    FailureReason NVARCHAR(500) NULL,
    
    -- External Payment Provider Information (CreditCard, VodafonePay vb. için)
    ExternalTransactionId NVARCHAR(200) NULL,
    ExternalResponse NVARCHAR(MAX) NULL, -- JSON format
    
    -- Refund Information
    RefundAmount DECIMAL(18, 2) NULL,
    RefundedAt DATETIME2 NULL,
    RefundReason NVARCHAR(500) NULL,
    
    -- Audit Fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT CK_Payments_PaymentMethod CHECK (PaymentMethod BETWEEN 1 AND 7),
    CONSTRAINT CK_Payments_Status CHECK (Status BETWEEN 0 AND 6),
    CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Payments_ChangeAmount CHECK (ChangeAmount >= 0),
    CONSTRAINT CK_Payments_RefundAmount CHECK (RefundAmount >= 0)
);

-- =============================================
-- COURIER CASH COLLECTIONS TABLE
-- =============================================

CREATE TABLE CourierCashCollections (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PaymentId UNIQUEIDENTIFIER NOT NULL,
    CourierId UNIQUEIDENTIFIER NOT NULL,
    
    -- Collection Information
    CollectedAmount DECIMAL(18, 2) NOT NULL,
    CollectedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Notes NVARCHAR(500) NULL,
    
    -- Status Tracking
    Status NVARCHAR(20) NOT NULL DEFAULT 'Collected', -- Collected, HandedToMerchant, HandedToCompany
    
    -- Handover Tracking
    HandedToMerchantAt DATETIME2 NULL,
    HandedToCompanyAt DATETIME2 NULL,
    
    -- Audit Fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT CK_CourierCashCollections_CollectedAmount CHECK (CollectedAmount > 0),
    CONSTRAINT CK_CourierCashCollections_Status CHECK (Status IN ('Collected', 'HandedToMerchant', 'HandedToCompany'))
);

-- =============================================
-- CASH SETTLEMENTS TABLE
-- =============================================

CREATE TABLE CashSettlements (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    
    -- Settlement Information
    TotalAmount DECIMAL(18, 2) NOT NULL,
    Commission DECIMAL(18, 2) NOT NULL,
    NetAmount DECIMAL(18, 2) NOT NULL,
    
    -- Settlement Details
    SettlementDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Failed
    Notes NVARCHAR(500) NULL,
    
    -- Admin Processing
    ProcessedByAdminId UNIQUEIDENTIFIER NULL,
    CompletedAt DATETIME2 NULL,
    
    -- Bank Transfer Information
    BankTransferReference NVARCHAR(100) NULL,
    
    -- Audit Fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    -- Constraints
    CONSTRAINT CK_CashSettlements_TotalAmount CHECK (TotalAmount > 0),
    CONSTRAINT CK_CashSettlements_Commission CHECK (Commission >= 0),
    CONSTRAINT CK_CashSettlements_NetAmount CHECK (NetAmount >= 0),
    CONSTRAINT CK_CashSettlements_Status CHECK (Status IN ('Pending', 'Completed', 'Failed'))
);

-- =============================================
-- FOREIGN KEY CONSTRAINTS
-- =============================================

-- Payments Foreign Keys
ALTER TABLE Payments ADD CONSTRAINT FK_Payments_Orders 
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE;

ALTER TABLE Payments ADD CONSTRAINT FK_Payments_Couriers 
    FOREIGN KEY (CollectedByCourierId) REFERENCES Couriers(Id) ON DELETE NO ACTION;

-- CourierCashCollections Foreign Keys
ALTER TABLE CourierCashCollections ADD CONSTRAINT FK_CourierCashCollections_Payments 
    FOREIGN KEY (PaymentId) REFERENCES Payments(Id) ON DELETE CASCADE;

ALTER TABLE CourierCashCollections ADD CONSTRAINT FK_CourierCashCollections_Couriers 
    FOREIGN KEY (CourierId) REFERENCES Couriers(Id) ON DELETE NO ACTION;

-- CashSettlements Foreign Keys
ALTER TABLE CashSettlements ADD CONSTRAINT FK_CashSettlements_Merchants 
    FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE;

ALTER TABLE CashSettlements ADD CONSTRAINT FK_CashSettlements_Admins 
    FOREIGN KEY (ProcessedByAdminId) REFERENCES Users(Id) ON DELETE NO ACTION;

-- =============================================
-- PERFORMANCE INDEXES
-- =============================================

-- Payments Indexes
CREATE INDEX IX_Payments_OrderId ON Payments(OrderId);
CREATE INDEX IX_Payments_PaymentMethod ON Payments(PaymentMethod);
CREATE INDEX IX_Payments_Status ON Payments(Status);
CREATE INDEX IX_Payments_CollectedByCourierId ON Payments(CollectedByCourierId);
CREATE INDEX IX_Payments_CreatedAt ON Payments(CreatedAt);
CREATE INDEX IX_Payments_CompletedAt ON Payments(CompletedAt);
CREATE INDEX IX_Payments_CollectedAt ON Payments(CollectedAt);
CREATE INDEX IX_Payments_SettledAt ON Payments(SettledAt);
CREATE INDEX IX_Payments_ExternalTransactionId ON Payments(ExternalTransactionId);

-- Composite Indexes for Performance
CREATE INDEX IX_Payments_PaymentMethod_Status ON Payments(PaymentMethod, Status);
CREATE INDEX IX_Payments_OrderId_Status ON Payments(OrderId, Status);
CREATE INDEX IX_Payments_CourierId_Status ON Payments(CollectedByCourierId, Status);

-- CourierCashCollections Indexes
CREATE INDEX IX_CourierCashCollections_PaymentId ON CourierCashCollections(PaymentId);
CREATE INDEX IX_CourierCashCollections_CourierId ON CourierCashCollections(CourierId);
CREATE INDEX IX_CourierCashCollections_CollectedAt ON CourierCashCollections(CollectedAt);
CREATE INDEX IX_CourierCashCollections_Status ON CourierCashCollections(Status);

-- Composite Index for Courier Daily Summary
CREATE INDEX IX_CourierCashCollections_CourierId_CollectedAt ON CourierCashCollections(CourierId, CollectedAt);

-- CashSettlements Indexes
CREATE INDEX IX_CashSettlements_MerchantId ON CashSettlements(MerchantId);
CREATE INDEX IX_CashSettlements_Status ON CashSettlements(Status);
CREATE INDEX IX_CashSettlements_SettlementDate ON CashSettlements(SettlementDate);
CREATE INDEX IX_CashSettlements_ProcessedByAdminId ON CashSettlements(ProcessedByAdminId);

-- Composite Index for Merchant Settlement History
CREATE INDEX IX_CashSettlements_MerchantId_SettlementDate ON CashSettlements(MerchantId, SettlementDate);

-- =============================================
-- ORDERS TABLE UPDATES
-- =============================================

-- Orders tablosunda PaymentMethod ve PaymentStatus alanlarını güncelle
-- (Eğer zaten varsa, değişiklik yapmaz)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PaymentMethod')
BEGIN
    ALTER TABLE Orders ADD PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'Cash';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PaymentStatus')
BEGIN
    ALTER TABLE Orders ADD PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending';
END
GO

-- Orders tablosu için payment index'leri
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Orders') AND name = 'IX_Orders_PaymentMethod')
BEGIN
    CREATE INDEX IX_Orders_PaymentMethod ON Orders(PaymentMethod);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('Orders') AND name = 'IX_Orders_PaymentStatus')
BEGIN
    CREATE INDEX IX_Orders_PaymentStatus ON Orders(PaymentStatus);
END
GO

-- =============================================
-- SAMPLE DATA (OPTIONAL)
-- =============================================

-- Test için örnek payment data'sı (isteğe bağlı)
/*
-- Örnek payment record
INSERT INTO Payments (Id, OrderId, PaymentMethod, Status, Amount, ChangeAmount, Notes, CreatedAt)
VALUES (
    NEWID(),
    (SELECT TOP 1 Id FROM Orders ORDER BY CreatedAt DESC), -- Son oluşturulan order
    1, -- Cash
    0, -- Pending
    85.50,
    4.50,
    'Para üstü: 4.50 TL',
    GETUTCDATE()
);
*/

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Tabloların oluşturulduğunu doğrula
SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Payments', 'CourierCashCollections', 'CashSettlements')
ORDER BY TABLE_NAME;

-- Index'lerin oluşturulduğunu doğrula
SELECT 
    i.name AS IndexName,
    t.name AS TableName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Payments', 'CourierCashCollections', 'CashSettlements')
AND i.name LIKE 'IX_%'
ORDER BY t.name, i.name;

-- Foreign key'lerin oluşturulduğunu doğrula
SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name IN ('Payments', 'CourierCashCollections', 'CashSettlements')
ORDER BY tp.name, fk.name;

PRINT 'Payment System Migration completed successfully!';
PRINT 'Tables created: Payments, CourierCashCollections, CashSettlements';
PRINT 'Indexes and foreign keys created successfully.';
PRINT 'Orders table updated with PaymentMethod and PaymentStatus columns.';

GO
