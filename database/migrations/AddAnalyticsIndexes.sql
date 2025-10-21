-- =============================================
-- Migration: Add Performance Indexes for Analytics Queries
-- Author: System  
-- Date: 2025-10-21
-- Description: Adds critical indexes for Admin Analytics Dashboard performance
-- =============================================

USE [db29009]
GO

PRINT 'Starting Analytics Index Creation...'
GO

-- =============================================
-- 1. User Growth Analytics Indexes
-- =============================================

-- Index for User.CreatedAt (user growth queries)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_CreatedAt' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Creating index: IX_Users_CreatedAt'
    CREATE NONCLUSTERED INDEX [IX_Users_CreatedAt]
    ON [dbo].[Users] ([CreatedAt] ASC)
    INCLUDE ([Id], [FirstName], [LastName], [Email], [IsActive])
    WITH (FILLFACTOR = 90);
    PRINT 'Index IX_Users_CreatedAt created successfully.'
END
ELSE
    PRINT 'Index IX_Users_CreatedAt already exists.'
GO

-- Index for User.Role + CreatedAt (merchant growth queries)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Role_CreatedAt' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Creating index: IX_Users_Role_CreatedAt'
    CREATE NONCLUSTERED INDEX [IX_Users_Role_CreatedAt]
    ON [dbo].[Users] ([Role] ASC, [CreatedAt] ASC)
    INCLUDE ([Id], [FirstName], [LastName], [IsActive])
    WITH (FILLFACTOR = 90);
    PRINT 'Index IX_Users_Role_CreatedAt created successfully.'
END
ELSE
    PRINT 'Index IX_Users_Role_CreatedAt already exists.'
GO

-- =============================================
-- 2. Search Functionality Indexes
-- =============================================

-- Composite index for User search (FirstName, LastName, Email)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Search' AND object_id = OBJECT_ID('Users'))
BEGIN
    PRINT 'Creating index: IX_Users_Search'
    CREATE NONCLUSTERED INDEX [IX_Users_Search]
    ON [dbo].[Users] ([FirstName] ASC, [LastName] ASC, [Email] ASC)
    INCLUDE ([Id], [Role], [IsActive], [CreatedAt])
    WITH (FILLFACTOR = 85);
    PRINT 'Index IX_Users_Search created successfully.'
END
ELSE
    PRINT 'Index IX_Users_Search already exists.'
GO

-- =============================================
-- 3. Order Analytics Indexes
-- =============================================

-- Index for Order.CreatedAt (order trend queries)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CreatedAt' AND object_id = OBJECT_ID('Orders'))
BEGIN
    PRINT 'Creating index: IX_Orders_CreatedAt'
    CREATE NONCLUSTERED INDEX [IX_Orders_CreatedAt]
    ON [dbo].[Orders] ([CreatedAt] ASC)
    INCLUDE ([Id], [OrderNumber], [Status], [Total], [UserId])
    WITH (FILLFACTOR = 80);
    PRINT 'Index IX_Orders_CreatedAt created successfully.'
END
ELSE
    PRINT 'Index IX_Orders_CreatedAt already exists.'
GO

-- Index for Order.Status + CreatedAt (revenue analytics)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Status_CreatedAt' AND object_id = OBJECT_ID('Orders'))
BEGIN
    PRINT 'Creating index: IX_Orders_Status_CreatedAt'
    CREATE NONCLUSTERED INDEX [IX_Orders_Status_CreatedAt]
    ON [dbo].[Orders] ([Status] ASC, [CreatedAt] ASC)
    INCLUDE ([Id], [Total], [OrderNumber])
    WITH (FILLFACTOR = 80);
    PRINT 'Index IX_Orders_Status_CreatedAt created successfully.'
END
ELSE
    PRINT 'Index IX_Orders_Status_CreatedAt already exists.'
GO

-- Index for Order.OrderNumber (search functionality)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_OrderNumber' AND object_id = OBJECT_ID('Orders'))
BEGIN
    PRINT 'Creating index: IX_Orders_OrderNumber'
    CREATE NONCLUSTERED INDEX [IX_Orders_OrderNumber]
    ON [dbo].[Orders] ([OrderNumber] ASC)
    INCLUDE ([Id], [Status], [Total], [UserId], [CreatedAt])
    WITH (FILLFACTOR = 90);
    PRINT 'Index IX_Orders_OrderNumber created successfully.'
END
ELSE
    PRINT 'Index IX_Orders_OrderNumber already exists.'
GO

-- =============================================
-- 4. SystemNotifications Index
-- =============================================

-- Index for SystemNotifications.IsActive + CreatedAt
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SystemNotifications_IsActive_CreatedAt' AND object_id = OBJECT_ID('SystemNotifications'))
BEGIN
    PRINT 'Creating index: IX_SystemNotifications_IsActive_CreatedAt'
    CREATE NONCLUSTERED INDEX [IX_SystemNotifications_IsActive_CreatedAt]
    ON [dbo].[SystemNotifications] ([IsActive] ASC, [CreatedAt] DESC)
    INCLUDE ([Id], [Title], [Message], [Type], [Priority])
    WITH (FILLFACTOR = 90);
    PRINT 'Index IX_SystemNotifications_IsActive_CreatedAt created successfully.'
END
ELSE
    PRINT 'Index IX_SystemNotifications_IsActive_CreatedAt already exists.'
GO

-- =============================================
-- 5. Index Statistics Update
-- =============================================

PRINT 'Updating statistics for better query optimization...'
GO

UPDATE STATISTICS [dbo].[Users] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[Orders] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[SystemNotifications] WITH FULLSCAN;
GO

-- =============================================
-- 6. Verification
-- =============================================

PRINT ''
PRINT '========================================='
PRINT 'Analytics Indexes Verification'
PRINT '========================================='

SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.fill_factor AS FillFactor
FROM sys.indexes i
WHERE i.object_id IN (
    OBJECT_ID('Users'),
    OBJECT_ID('Orders'),
    OBJECT_ID('SystemNotifications')
)
AND i.name LIKE 'IX[_]%'
ORDER BY TableName, IndexName;

PRINT ''
PRINT '========================================='
PRINT 'Migration completed successfully!'
PRINT 'Total Indexes Created: 7'
PRINT '========================================='
GO

