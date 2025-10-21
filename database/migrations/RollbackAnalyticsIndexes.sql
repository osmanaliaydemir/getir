-- =============================================
-- Rollback: Remove Analytics Performance Indexes
-- Author: System
-- Date: 2025-10-21
-- Description: Removes analytics indexes if needed
-- WARNING: This will degrade analytics query performance!
-- =============================================

USE [Getir]
GO

PRINT 'Starting Analytics Index Removal...'
PRINT 'WARNING: Analytics queries will be slower after this rollback!'
GO

-- Drop User analytics indexes
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_CreatedAt' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Dropping index: IX_Users_CreatedAt'
    DROP INDEX [IX_Users_CreatedAt] ON [dbo].[Users];
    PRINT 'Index dropped.'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Role_CreatedAt' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Dropping index: IX_Users_Role_CreatedAt'
    DROP INDEX [IX_Users_Role_CreatedAt] ON [dbo].[Users];
    PRINT 'Index dropped.'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Search' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Dropping index: IX_Users_Search'
    DROP INDEX [IX_Users_Search] ON [dbo].[Users];
    PRINT 'Index dropped.'
END
GO

-- Drop Order analytics indexes
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CreatedAt' AND object_id = OBJECT_ID('Orders'))
BEGIN
    PRINT 'Dropping index: IX_Orders_CreatedAt'
    DROP INDEX [IX_Orders_CreatedAt] ON [dbo].[Orders];
    PRINT 'Index dropped.'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status_CreatedAt' AND object_id = OBJECT_ID('Orders'))
BEGIN
    PRINT 'Dropping index: IX_Orders_Status_CreatedAt'
    DROP INDEX [IX_Orders_Status_CreatedAt] ON [dbo].[Orders];
    PRINT 'Index dropped.'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_OrderNumber' AND object_id = OBJECT_ID('Orders'))
BEGIN
    PRINT 'Dropping index: IX_Orders_OrderNumber'
    DROP INDEX [IX_Orders_OrderNumber] ON [dbo].[Orders];
    PRINT 'Index dropped.'
END
GO

-- Drop SystemNotifications index
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SystemNotifications_IsActive_CreatedAt' AND object_id = OBJECT_ID('SystemNotifications'))
BEGIN
    PRINT 'Dropping index: IX_SystemNotifications_IsActive_CreatedAt'
    DROP INDEX [IX_SystemNotifications_IsActive_CreatedAt] ON [dbo].[SystemNotifications];
    PRINT 'Index dropped.'
END
GO

PRINT 'Rollback completed successfully.'
PRINT 'All analytics indexes have been removed.'
GO

