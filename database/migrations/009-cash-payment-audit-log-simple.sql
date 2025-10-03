-- Migration 009: Nakit ödeme audit log tablosu (Basit versiyon)

-- 1. CashPaymentAuditLogs Tablosu
CREATE TABLE CashPaymentAuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PaymentId UNIQUEIDENTIFIER NOT NULL,
    CourierId UNIQUEIDENTIFIER NULL,
    CustomerId UNIQUEIDENTIFIER NULL,
    AdminId UNIQUEIDENTIFIER NULL,
    EventType INT NOT NULL, -- AuditEventType enum
    SeverityLevel INT NOT NULL, -- AuditSeverityLevel enum
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Details NVARCHAR(MAX) NULL,
    RiskLevel INT NULL, -- SecurityRiskLevel enum
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    DeviceInfo NVARCHAR(MAX) NULL,
    Latitude DECIMAL(18, 15) NULL,
    Longitude DECIMAL(18, 15) NULL,
    SessionId NVARCHAR(100) NULL,
    RequestId NVARCHAR(50) NULL,
    CorrelationId NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_CashPaymentAuditLogs_Payments FOREIGN KEY (PaymentId) REFERENCES Payments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CashPaymentAuditLogs_Couriers FOREIGN KEY (CourierId) REFERENCES Couriers(Id),
    CONSTRAINT FK_CashPaymentAuditLogs_Customers FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    CONSTRAINT FK_CashPaymentAuditLogs_Admins FOREIGN KEY (AdminId) REFERENCES Users(Id)
);
GO

-- 2. Index'ler (performans için)
CREATE INDEX IX_CashPaymentAuditLogs_PaymentId ON CashPaymentAuditLogs (PaymentId);
CREATE INDEX IX_CashPaymentAuditLogs_CourierId ON CashPaymentAuditLogs (CourierId);
CREATE INDEX IX_CashPaymentAuditLogs_CustomerId ON CashPaymentAuditLogs (CustomerId);
CREATE INDEX IX_CashPaymentAuditLogs_AdminId ON CashPaymentAuditLogs (AdminId);
CREATE INDEX IX_CashPaymentAuditLogs_EventType ON CashPaymentAuditLogs (EventType);
CREATE INDEX IX_CashPaymentAuditLogs_SeverityLevel ON CashPaymentAuditLogs (SeverityLevel);
CREATE INDEX IX_CashPaymentAuditLogs_RiskLevel ON CashPaymentAuditLogs (RiskLevel);
CREATE INDEX IX_CashPaymentAuditLogs_CreatedAt ON CashPaymentAuditLogs (CreatedAt);
CREATE INDEX IX_CashPaymentAuditLogs_SessionId ON CashPaymentAuditLogs (SessionId);
CREATE INDEX IX_CashPaymentAuditLogs_RequestId ON CashPaymentAuditLogs (RequestId);
CREATE INDEX IX_CashPaymentAuditLogs_CorrelationId ON CashPaymentAuditLogs (CorrelationId);

-- 3. Check Constraints
ALTER TABLE CashPaymentAuditLogs 
ADD CONSTRAINT CK_CashPaymentAuditLogs_EventType 
CHECK (EventType BETWEEN 0 AND 20);

ALTER TABLE CashPaymentAuditLogs 
ADD CONSTRAINT CK_CashPaymentAuditLogs_SeverityLevel 
CHECK (SeverityLevel BETWEEN 0 AND 4);

ALTER TABLE CashPaymentAuditLogs 
ADD CONSTRAINT CK_CashPaymentAuditLogs_RiskLevel 
CHECK (RiskLevel IS NULL OR RiskLevel BETWEEN 0 AND 3);

ALTER TABLE CashPaymentAuditLogs 
ADD CONSTRAINT CK_CashPaymentAuditLogs_Latitude 
CHECK (Latitude IS NULL OR (Latitude >= -90 AND Latitude <= 90));

ALTER TABLE CashPaymentAuditLogs 
ADD CONSTRAINT CK_CashPaymentAuditLogs_Longitude 
CHECK (Longitude IS NULL OR (Longitude >= -180 AND Longitude <= 180));

-- Migration tamamlandı
PRINT 'Migration 009: Cash payment audit log table created successfully';
