-- =============================================
-- Migration: Add Merchant Response fields to ProductReviews
-- Author: System
-- Date: 2025-10-21
-- Description: Adds MerchantResponse, MerchantRespondedAt, RejectionReason columns
-- =============================================

USE [db29009]
GO

PRINT 'Adding merchant response fields to ProductReviews table...'
GO

-- Add MerchantResponse column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductReviews' 
    AND COLUMN_NAME = 'MerchantResponse'
)
BEGIN
    PRINT 'Adding MerchantResponse column...'
    ALTER TABLE [dbo].[ProductReviews]
    ADD [MerchantResponse] NVARCHAR(2000) NULL;
    PRINT 'MerchantResponse column added.'
END
ELSE
    PRINT 'MerchantResponse column already exists.'
GO

-- Add MerchantRespondedAt column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductReviews' 
    AND COLUMN_NAME = 'MerchantRespondedAt'
)
BEGIN
    PRINT 'Adding MerchantRespondedAt column...'
    ALTER TABLE [dbo].[ProductReviews]
    ADD [MerchantRespondedAt] DATETIME2(7) NULL;
    PRINT 'MerchantRespondedAt column added.'
END
ELSE
    PRINT 'MerchantRespondedAt column already exists.'
GO

-- Add RejectionReason column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductReviews' 
    AND COLUMN_NAME = 'RejectionReason'
)
BEGIN
    PRINT 'Adding RejectionReason column...'
    ALTER TABLE [dbo].[ProductReviews]
    ADD [RejectionReason] NVARCHAR(500) NULL;
    PRINT 'RejectionReason column added.'
END
ELSE
    PRINT 'RejectionReason column already exists.'
GO

-- Create index for merchant response queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductReviews_MerchantResponse' AND object_id = OBJECT_ID('ProductReviews'))
BEGIN
    PRINT 'Creating index for merchant response queries...'
    CREATE NONCLUSTERED INDEX [IX_ProductReviews_MerchantResponse]
    ON [dbo].[ProductReviews] ([MerchantRespondedAt] DESC)
    WHERE [MerchantResponse] IS NOT NULL
    WITH (FILLFACTOR = 90);
    PRINT 'Index created.'
END
ELSE
    PRINT 'Index already exists.'
GO

-- Verification
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProductReviews'
AND COLUMN_NAME IN ('MerchantResponse', 'MerchantRespondedAt', 'RejectionReason')
ORDER BY COLUMN_NAME;
GO

PRINT ''
PRINT 'Migration completed successfully!'
PRINT 'ProductReviews table now supports merchant responses!'
GO

