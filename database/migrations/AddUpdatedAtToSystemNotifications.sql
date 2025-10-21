-- =============================================
-- Migration: Add UpdatedAt column to SystemNotifications
-- Author: System
-- Date: 2025-10-21
-- Description: Adds UpdatedAt column for tracking notification updates
-- =============================================

USE [db29009]
GO

-- Check if column exists before adding
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SystemNotifications' 
    AND COLUMN_NAME = 'UpdatedAt'
)
BEGIN
    PRINT 'Adding UpdatedAt column to SystemNotifications table...'
    
    ALTER TABLE [dbo].[SystemNotifications]
    ADD [UpdatedAt] DATETIME2(7) NULL;
    
    PRINT 'UpdatedAt column added successfully.'
    
    -- Set UpdatedAt to CreatedAt for existing records
    UPDATE [dbo].[SystemNotifications]
    SET [UpdatedAt] = [CreatedAt]
    WHERE [UpdatedAt] IS NULL;
    
    PRINT 'Existing records updated with CreatedAt value.'
END
ELSE
BEGIN
    PRINT 'UpdatedAt column already exists in SystemNotifications table.'
END
GO

-- Verify the change
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SystemNotifications'
AND COLUMN_NAME = 'UpdatedAt';
GO

PRINT 'Migration completed successfully!'
GO

