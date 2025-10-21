-- =============================================
-- Rollback Migration: Remove UpdatedAt column from SystemNotifications
-- Author: System
-- Date: 2025-10-21
-- Description: Rollback script to remove UpdatedAt column if needed
-- WARNING: This will remove the UpdatedAt column and all its data!
-- =============================================

USE [Getir]
GO

-- Check if column exists before removing
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SystemNotifications' 
    AND COLUMN_NAME = 'UpdatedAt'
)
BEGIN
    PRINT 'Removing UpdatedAt column from SystemNotifications table...'
    
    ALTER TABLE [dbo].[SystemNotifications]
    DROP COLUMN [UpdatedAt];
    
    PRINT 'UpdatedAt column removed successfully.'
END
ELSE
BEGIN
    PRINT 'UpdatedAt column does not exist in SystemNotifications table.'
END
GO

-- Verify the change
SELECT COUNT(*) AS ColumnCount
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SystemNotifications'
AND COLUMN_NAME = 'UpdatedAt';
GO

PRINT 'Rollback completed successfully!'
GO

