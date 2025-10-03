-- Migration 009: CashPaymentAuditLog tablosunun oluşturulması

-- 1. CashPaymentAuditLogs Tablosu
CREATE TABLE CashPaymentAuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PaymentId UNIQUEIDENTIFIER,
    CourierId UNIQUEIDENTIFIER,
    CustomerId UNIQUEIDENTIFIER,
    AdminId UNIQUEIDENTIFIER,
    EventType INT NOT NULL, -- AuditEventType enum
    SeverityLevel INT NOT NULL, -- AuditSeverityLevel enum
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Details NVARCHAR(MAX), -- JSON format
    RiskLevel INT, -- SecurityRiskLevel enum (nullable)
    IpAddress NVARCHAR(45), -- IPv6 support
    UserAgent NVARCHAR(500),
    DeviceInfo NVARCHAR(500),
    Latitude FLOAT,
    Longitude FLOAT,
    SessionId NVARCHAR(100),
    RequestId NVARCHAR(100),
    CorrelationId NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    DeletedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    -- Foreign Key Constraints
    FOREIGN KEY (PaymentId) REFERENCES Payments(Id) ON DELETE SET NULL,
    FOREIGN KEY (CourierId) REFERENCES Couriers(Id) ON DELETE SET NULL,
    FOREIGN KEY (CustomerId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (AdminId) REFERENCES Users(Id) ON DELETE SET NULL
);
GO

-- 2. Indexler
CREATE INDEX IX_CashPaymentAuditLogs_PaymentId ON CashPaymentAuditLogs (PaymentId);
CREATE INDEX IX_CashPaymentAuditLogs_CourierId ON CashPaymentAuditLogs (CourierId);
CREATE INDEX IX_CashPaymentAuditLogs_CustomerId ON CashPaymentAuditLogs (CustomerId);
CREATE INDEX IX_CashPaymentAuditLogs_AdminId ON CashPaymentAuditLogs (AdminId);
CREATE INDEX IX_CashPaymentAuditLogs_EventType ON CashPaymentAuditLogs (EventType);
CREATE INDEX IX_CashPaymentAuditLogs_SeverityLevel ON CashPaymentAuditLogs (SeverityLevel);
CREATE INDEX IX_CashPaymentAuditLogs_RiskLevel ON CashPaymentAuditLogs (RiskLevel);
CREATE INDEX IX_CashPaymentAuditLogs_CreatedAt ON CashPaymentAuditLogs (CreatedAt);
CREATE INDEX IX_CashPaymentAuditLogs_IsDeleted ON CashPaymentAuditLogs (IsDeleted);
CREATE INDEX IX_CashPaymentAuditLogs_CorrelationId ON CashPaymentAuditLogs (CorrelationId);
CREATE INDEX IX_CashPaymentAuditLogs_SessionId ON CashPaymentAuditLogs (SessionId);

-- 3. Composite Indexler (performans için)
CREATE INDEX IX_CashPaymentAuditLogs_PaymentId_CreatedAt ON CashPaymentAuditLogs (PaymentId, CreatedAt DESC);
CREATE INDEX IX_CashPaymentAuditLogs_CourierId_CreatedAt ON CashPaymentAuditLogs (CourierId, CreatedAt DESC);
CREATE INDEX IX_CashPaymentAuditLogs_EventType_CreatedAt ON CashPaymentAuditLogs (EventType, CreatedAt DESC);
CREATE INDEX IX_CashPaymentAuditLogs_SeverityLevel_CreatedAt ON CashPaymentAuditLogs (SeverityLevel, CreatedAt DESC);
CREATE INDEX IX_CashPaymentAuditLogs_RiskLevel_CreatedAt ON CashPaymentAuditLogs (RiskLevel, CreatedAt DESC);
CREATE INDEX IX_CashPaymentAuditLogs_IsDeleted_CreatedAt ON CashPaymentAuditLogs (IsDeleted, CreatedAt DESC);

-- 4. Check Constraints (Enum değerlerini sınırla)
ALTER TABLE CashPaymentAuditLogs
ADD CONSTRAINT CK_CashPaymentAuditLogs_EventType CHECK (EventType IN (1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20));

ALTER TABLE CashPaymentAuditLogs
ADD CONSTRAINT CK_CashPaymentAuditLogs_SeverityLevel CHECK (SeverityLevel IN (1,2,3,4,5));

ALTER TABLE CashPaymentAuditLogs
ADD CONSTRAINT CK_CashPaymentAuditLogs_RiskLevel CHECK (RiskLevel IS NULL OR RiskLevel IN (1,2,3,4));

-- 5. Partitioning için hazırlık (gelecekte büyük veri için)
-- Bu tablo çok büyüyebileceği için partitioning düşünülebilir
-- CREATE PARTITION FUNCTION PF_AuditLogs_Date (DATETIME2)
-- AS RANGE RIGHT FOR VALUES ('2024-01-01', '2024-07-01', '2025-01-01');

-- 6. Audit log cleanup stored procedure (opsiyonel)
CREATE PROCEDURE sp_CleanupOldAuditLogs
    @CutoffDate DATETIME2,
    @BatchSize INT = 1000
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT = 0;
    DECLARE @BatchDeleted INT;
    
    WHILE 1 = 1
    BEGIN
        UPDATE TOP (@BatchSize) CashPaymentAuditLogs
        SET IsDeleted = 1,
            DeletedAt = GETUTCDATE(),
            UpdatedAt = GETUTCDATE()
        WHERE CreatedAt < @CutoffDate 
          AND IsDeleted = 0;
        
        SET @BatchDeleted = @@ROWCOUNT;
        SET @DeletedCount = @DeletedCount + @BatchDeleted;
        
        IF @BatchDeleted = 0
            BREAK;
            
        -- Batch'ler arasında kısa bekleme
        WAITFOR DELAY '00:00:01';
    END
    
    SELECT @DeletedCount AS DeletedCount;
END
GO

-- 7. Audit log statistics view (performans için)
CREATE VIEW vw_AuditLogStatistics
AS
SELECT 
    EventType,
    SeverityLevel,
    RiskLevel,
    COUNT(*) AS EventCount,
    MIN(CreatedAt) AS FirstEvent,
    MAX(CreatedAt) AS LastEvent,
    COUNT(DISTINCT PaymentId) AS UniquePayments,
    COUNT(DISTINCT CourierId) AS UniqueCouriers,
    COUNT(DISTINCT CustomerId) AS UniqueCustomers
FROM CashPaymentAuditLogs
WHERE IsDeleted = 0
GROUP BY EventType, SeverityLevel, RiskLevel;
GO

-- 8. Security incidents view
CREATE VIEW vw_SecurityIncidents
AS
SELECT 
    Id,
    PaymentId,
    CourierId,
    CustomerId,
    EventType,
    SeverityLevel,
    RiskLevel,
    Title,
    Description,
    CreatedAt,
    IpAddress,
    DeviceInfo
FROM CashPaymentAuditLogs
WHERE IsDeleted = 0
  AND (SeverityLevel = 5 -- Security
       OR RiskLevel IN (3, 4) -- High, Critical
       OR EventType IN (5, 6, 9, 10, 11, 12, 13, 15, 19)) -- Security related events
ORDER BY CreatedAt DESC;
GO

-- 9. Sample data (test için)
INSERT INTO CashPaymentAuditLogs (
    PaymentId, CourierId, EventType, SeverityLevel, Title, Description, 
    RiskLevel, CreatedAt
) VALUES 
(
    NULL, NULL, 1, 1, 'System Started', 'Cash payment audit system initialized',
    NULL, GETUTCDATE()
),
(
    NULL, NULL, 16, 1, 'Migration Completed', 'Audit log table created successfully',
    NULL, GETUTCDATE()
);

PRINT 'Migration 009: CashPaymentAuditLog table created successfully';
PRINT 'Audit log system is ready for use';
