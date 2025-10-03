-- Migration 010: Merchant document upload system

-- 1. MerchantDocuments Tablosu
CREATE TABLE MerchantDocuments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    UploadedBy UNIQUEIDENTIFIER NOT NULL,
    DocumentType NVARCHAR(50) NOT NULL, -- DocumentType enum as string
    DocumentName NVARCHAR(200) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileUrl NVARCHAR(MAX) NOT NULL,
    MimeType NVARCHAR(100) NOT NULL,
    FileSize BIGINT NOT NULL,
    FileHash NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ExpiryDate DATETIME2 NOT NULL,
    IsRequired BIT NOT NULL DEFAULT 1,
    IsVerified BIT NOT NULL DEFAULT 0,
    IsApproved BIT NOT NULL DEFAULT 0,
    VerificationNotes NVARCHAR(MAX) NULL,
    VerifiedBy UNIQUEIDENTIFIER NULL,
    VerifiedAt DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 0, -- DocumentStatus enum
    RejectionReason NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_MerchantDocuments_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MerchantDocuments_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES Users(Id),
    CONSTRAINT FK_MerchantDocuments_VerifiedBy FOREIGN KEY (VerifiedBy) REFERENCES Users(Id)
);
GO

-- 2. Index'ler (performans için)
CREATE INDEX IX_MerchantDocuments_MerchantId ON MerchantDocuments (MerchantId);
CREATE INDEX IX_MerchantDocuments_UploadedBy ON MerchantDocuments (UploadedBy);
CREATE INDEX IX_MerchantDocuments_VerifiedBy ON MerchantDocuments (VerifiedBy);
CREATE INDEX IX_MerchantDocuments_DocumentType ON MerchantDocuments (DocumentType);
CREATE INDEX IX_MerchantDocuments_Status ON MerchantDocuments (Status);
CREATE INDEX IX_MerchantDocuments_ExpiryDate ON MerchantDocuments (ExpiryDate);
CREATE INDEX IX_MerchantDocuments_CreatedAt ON MerchantDocuments (CreatedAt);
CREATE INDEX IX_MerchantDocuments_IsRequired ON MerchantDocuments (IsRequired);
CREATE INDEX IX_MerchantDocuments_IsApproved ON MerchantDocuments (IsApproved);

-- 3. Check Constraints
ALTER TABLE MerchantDocuments 
ADD CONSTRAINT CK_MerchantDocuments_Status 
CHECK (Status BETWEEN 0 AND 5);

ALTER TABLE MerchantDocuments 
ADD CONSTRAINT CK_MerchantDocuments_FileSize 
CHECK (FileSize > 0);

ALTER TABLE MerchantDocuments 
ADD CONSTRAINT CK_MerchantDocuments_ExpiryDate 
CHECK (ExpiryDate > CreatedAt);

-- 4. Unique constraint - bir merchant için aynı document type'tan sadece bir tane olabilir (rejected olanlar hariç)
CREATE UNIQUE INDEX IX_MerchantDocuments_MerchantId_DocumentType_Unique 
ON MerchantDocuments (MerchantId, DocumentType) 
WHERE Status != 3; -- Rejected status = 3

-- 5. View for document statistics
CREATE VIEW vw_MerchantDocumentStats AS
SELECT 
    m.Id as MerchantId,
    m.Name as MerchantName,
    COUNT(md.Id) as TotalDocuments,
    COUNT(CASE WHEN md.IsApproved = 1 THEN 1 END) as ApprovedDocuments,
    COUNT(CASE WHEN md.Status = 0 THEN 1 END) as PendingDocuments,
    COUNT(CASE WHEN md.Status = 1 THEN 1 END) as UnderReviewDocuments,
    COUNT(CASE WHEN md.Status = 2 THEN 1 END) as ApprovedDocumentsCount,
    COUNT(CASE WHEN md.Status = 3 THEN 1 END) as RejectedDocuments,
    COUNT(CASE WHEN md.ExpiryDate < GETUTCDATE() THEN 1 END) as ExpiredDocuments,
    COUNT(CASE WHEN md.ExpiryDate BETWEEN GETUTCDATE() AND DATEADD(DAY, 30, GETUTCDATE()) THEN 1 END) as ExpiringSoonDocuments,
    MAX(md.CreatedAt) as LastDocumentUpload,
    MAX(md.VerifiedAt) as LastVerification
FROM Merchants m
LEFT JOIN MerchantDocuments md ON m.Id = md.MerchantId
GROUP BY m.Id, m.Name;
GO

-- 6. Stored procedure for document cleanup
CREATE PROCEDURE sp_CleanupExpiredDocuments
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update status of expired documents
    UPDATE MerchantDocuments 
    SET Status = 4, -- Expired
        UpdatedAt = GETUTCDATE()
    WHERE ExpiryDate < GETUTCDATE() 
    AND Status NOT IN (3, 4, 5); -- Not already rejected, expired, or invalid
    
    -- Log cleanup action
    INSERT INTO AuditLogs (UserId, UserName, Action, EntityType, EntityId, Details, Timestamp)
    VALUES ('SYSTEM', 'SYSTEM', 'DocumentCleanup', 'MerchantDocument', 'BATCH', 
            'Expired documents status updated', GETUTCDATE());
END;
GO

-- 7. Function to check if merchant has all required documents
CREATE FUNCTION fn_MerchantHasAllRequiredDocuments(@MerchantId UNIQUEIDENTIFIER)
RETURNS BIT
AS
BEGIN
    DECLARE @RequiredCount INT = 3; -- TaxCertificate, BusinessLicense, IdentityCard
    DECLARE @ApprovedCount INT;
    
    SELECT @ApprovedCount = COUNT(*)
    FROM MerchantDocuments 
    WHERE MerchantId = @MerchantId 
    AND IsApproved = 1 
    AND DocumentType IN ('TaxCertificate', 'BusinessLicense', 'IdentityCard');
    
    RETURN CASE WHEN @ApprovedCount >= @RequiredCount THEN 1 ELSE 0 END;
END;
GO

-- Migration tamamlandı
PRINT 'Migration 010: Merchant document upload system created successfully';
