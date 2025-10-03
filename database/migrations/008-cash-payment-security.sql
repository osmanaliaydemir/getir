-- Migration 008: Nakit ödeme güvenlik sistemi tabloları

-- 1. CashPaymentEvidence Tablosu
CREATE TABLE CashPaymentEvidence (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PaymentId UNIQUEIDENTIFIER NOT NULL,
    CourierId UNIQUEIDENTIFIER NOT NULL,
    EvidenceType INT NOT NULL, -- 1: CashCollectionPhoto, 2: CustomerSignature, 3: ChangePhoto, 4: DeliveryPhoto, 5: Video, 6: Audio
    FileUrl NVARCHAR(MAX) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100) NOT NULL,
    FileHash NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX),
    Latitude FLOAT,
    Longitude FLOAT,
    DeviceInfo NVARCHAR(MAX),
    Status INT NOT NULL DEFAULT 0, -- 0: Pending, 1: Verified, 2: Rejected, 3: RequiresReview
    VerificationNotes NVARCHAR(MAX),
    VerifiedByAdminId UNIQUEIDENTIFIER,
    VerifiedAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (PaymentId) REFERENCES Payments(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourierId) REFERENCES Couriers(Id),
    FOREIGN KEY (VerifiedByAdminId) REFERENCES Users(Id)
);
GO

-- 2. CashPaymentSecurity Tablosu
CREATE TABLE CashPaymentSecurity (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PaymentId UNIQUEIDENTIFIER NOT NULL,
    ChangeCalculationVerified BIT NOT NULL DEFAULT 0,
    CalculatedChange DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GivenChange DECIMAL(18, 2) NOT NULL DEFAULT 0,
    ChangeDifference DECIMAL(18, 2) NOT NULL DEFAULT 0,
    FakeMoneyCheckPerformed BIT NOT NULL DEFAULT 0,
    FakeMoneyDetected BIT NOT NULL DEFAULT 0,
    FakeMoneyNotes NVARCHAR(MAX),
    CustomerIdentityVerified BIT NOT NULL DEFAULT 0,
    IdentityVerificationType NVARCHAR(50),
    IdentityNumberHash NVARCHAR(500),
    RiskLevel INT NOT NULL DEFAULT 1, -- 1: Low, 2: Medium, 3: High, 4: Critical
    RiskFactors NVARCHAR(MAX),
    SecurityNotes NVARCHAR(MAX),
    RequiresManualApproval BIT NOT NULL DEFAULT 0,
    ApprovedByAdminId UNIQUEIDENTIFIER,
    ApprovedAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (PaymentId) REFERENCES Payments(Id) ON DELETE CASCADE,
    FOREIGN KEY (ApprovedByAdminId) REFERENCES Users(Id)
);
GO

-- 3. Index'ler (performans için)
CREATE INDEX IX_CashPaymentEvidence_PaymentId ON CashPaymentEvidence (PaymentId);
CREATE INDEX IX_CashPaymentEvidence_CourierId ON CashPaymentEvidence (CourierId);
CREATE INDEX IX_CashPaymentEvidence_Status ON CashPaymentEvidence (Status);
CREATE INDEX IX_CashPaymentEvidence_EvidenceType ON CashPaymentEvidence (EvidenceType);
CREATE INDEX IX_CashPaymentEvidence_CreatedAt ON CashPaymentEvidence (CreatedAt);

CREATE INDEX IX_CashPaymentSecurity_PaymentId ON CashPaymentSecurity (PaymentId);
CREATE INDEX IX_CashPaymentSecurity_RiskLevel ON CashPaymentSecurity (RiskLevel);
CREATE INDEX IX_CashPaymentSecurity_RequiresManualApproval ON CashPaymentSecurity (RequiresManualApproval);
CREATE INDEX IX_CashPaymentSecurity_CreatedAt ON CashPaymentSecurity (CreatedAt);

-- 4. Check Constraints
ALTER TABLE CashPaymentEvidence 
ADD CONSTRAINT CK_CashPaymentEvidence_EvidenceType 
CHECK (EvidenceType IN (1, 2, 3, 4, 5, 6));

ALTER TABLE CashPaymentEvidence 
ADD CONSTRAINT CK_CashPaymentEvidence_Status 
CHECK (Status IN (0, 1, 2, 3));

ALTER TABLE CashPaymentSecurity 
ADD CONSTRAINT CK_CashPaymentSecurity_RiskLevel 
CHECK (RiskLevel IN (1, 2, 3, 4));

ALTER TABLE CashPaymentSecurity 
ADD CONSTRAINT CK_CashPaymentSecurity_CalculatedChange 
CHECK (CalculatedChange >= 0);

ALTER TABLE CashPaymentSecurity 
ADD CONSTRAINT CK_CashPaymentSecurity_GivenChange 
CHECK (GivenChange >= 0);

-- 5. Unique Constraints
ALTER TABLE CashPaymentSecurity 
ADD CONSTRAINT UQ_CashPaymentSecurity_PaymentId 
UNIQUE (PaymentId);

-- Migration tamamlandı
PRINT 'Migration 008: Cash payment security tables created successfully';
