-- =============================================
-- Migration: Add Merchant Portal Fields to UserNotificationPreferences
-- Date: 2025-10-13
-- Description: Adds Sound, Desktop, and Event-specific notification fields
-- =============================================

-- Check if table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserNotificationPreferences](
        [Id] [uniqueidentifier] NOT NULL DEFAULT NEWID(),
        [UserId] [uniqueidentifier] NOT NULL,
        
        -- Email preferences
        [EmailEnabled] [bit] NOT NULL DEFAULT 1,
        [EmailOrderUpdates] [bit] NOT NULL DEFAULT 1,
        [EmailPromotions] [bit] NOT NULL DEFAULT 1,
        [EmailNewsletter] [bit] NOT NULL DEFAULT 1,
        [EmailSecurityAlerts] [bit] NOT NULL DEFAULT 1,
        
        -- SMS preferences
        [SmsEnabled] [bit] NOT NULL DEFAULT 1,
        [SmsOrderUpdates] [bit] NOT NULL DEFAULT 1,
        [SmsPromotions] [bit] NOT NULL DEFAULT 0,
        [SmsSecurityAlerts] [bit] NOT NULL DEFAULT 1,
        
        -- Push notification preferences
        [PushEnabled] [bit] NOT NULL DEFAULT 1,
        [PushOrderUpdates] [bit] NOT NULL DEFAULT 1,
        [PushPromotions] [bit] NOT NULL DEFAULT 1,
        [PushMerchantUpdates] [bit] NOT NULL DEFAULT 1,
        [PushSecurityAlerts] [bit] NOT NULL DEFAULT 1,
        
        -- Time preferences (Do Not Disturb)
        [QuietStartTime] [time](7) NULL,
        [QuietEndTime] [time](7) NULL,
        [RespectQuietHours] [bit] NOT NULL DEFAULT 1,
        
        -- Language preference
        [Language] [nvarchar](10) NOT NULL DEFAULT 'tr-TR',
        
        -- Timestamps
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_UserNotificationPreferences] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_UserNotificationPreferences_Users_UserId] FOREIGN KEY([UserId])
            REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_UserNotificationPreferences_UserId] ON [dbo].[UserNotificationPreferences]
    (
        [UserId] ASC
    );
    
    PRINT 'Table UserNotificationPreferences created successfully.';
END
ELSE
BEGIN
    PRINT 'Table UserNotificationPreferences already exists.';
END
GO

-- Add new columns if they don't exist

-- Merchant Portal - Sound & Desktop Notifications
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND name = 'SoundEnabled')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [SoundEnabled] [bit] NOT NULL DEFAULT 1;
    PRINT 'Column SoundEnabled added.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND name = 'DesktopNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [DesktopNotifications] [bit] NOT NULL DEFAULT 1;
    PRINT 'Column DesktopNotifications added.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND name = 'NotificationSound')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [NotificationSound] [nvarchar](50) NOT NULL DEFAULT 'default';
    PRINT 'Column NotificationSound added.';
END

-- Merchant Portal - Event-specific Notifications
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND name = 'NewOrderNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [NewOrderNotifications] [bit] NOT NULL DEFAULT 1;
    PRINT 'Column NewOrderNotifications added.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND name = 'StatusChangeNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [StatusChangeNotifications] [bit] NOT NULL DEFAULT 1;
    PRINT 'Column StatusChangeNotifications added.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserNotificationPreferences]') AND name = 'CancellationNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [CancellationNotifications] [bit] NOT NULL DEFAULT 1;
    PRINT 'Column CancellationNotifications added.';
END

GO

PRINT 'Migration completed successfully!';
GO

-- Verify the changes
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserNotificationPreferences'
    AND COLUMN_NAME IN (
        'SoundEnabled', 
        'DesktopNotifications', 
        'NotificationSound',
        'NewOrderNotifications',
        'StatusChangeNotifications',
        'CancellationNotifications'
    )
ORDER BY ORDINAL_POSITION;
GO

