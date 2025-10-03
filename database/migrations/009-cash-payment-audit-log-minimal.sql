-- Migration 009: Nakit ödeme audit log tablosu (Minimal versiyon)

-- 1. CashPaymentAuditLogs Tablosu (Foreign key'ler olmadan)
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
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
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

-- Migration tamamlandı
PRINT 'Migration 009: Cash payment audit log table created successfully';
