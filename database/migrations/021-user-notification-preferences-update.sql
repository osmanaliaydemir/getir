-- =====================================================
-- USER NOTIFICATION PREFERENCES UPDATE
-- Sprint 14: Merchant Portal - Sound & Desktop Notifications
-- =====================================================

-- Check if columns exist before adding
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserNotificationPreferences' 
               AND COLUMN_NAME = 'SoundEnabled')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [SoundEnabled] BIT NOT NULL DEFAULT 1;
    PRINT 'Added SoundEnabled column';
END
ELSE
BEGIN
    PRINT 'SoundEnabled column already exists';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserNotificationPreferences' 
               AND COLUMN_NAME = 'DesktopNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [DesktopNotifications] BIT NOT NULL DEFAULT 1;
    PRINT 'Added DesktopNotifications column';
END
ELSE
BEGIN
    PRINT 'DesktopNotifications column already exists';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserNotificationPreferences' 
               AND COLUMN_NAME = 'NotificationSound')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [NotificationSound] NVARCHAR(50) NOT NULL DEFAULT 'default';
    PRINT 'Added NotificationSound column';
END
ELSE
BEGIN
    PRINT 'NotificationSound column already exists';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserNotificationPreferences' 
               AND COLUMN_NAME = 'NewOrderNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [NewOrderNotifications] BIT NOT NULL DEFAULT 1;
    PRINT 'Added NewOrderNotifications column';
END
ELSE
BEGIN
    PRINT 'NewOrderNotifications column already exists';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserNotificationPreferences' 
               AND COLUMN_NAME = 'StatusChangeNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [StatusChangeNotifications] BIT NOT NULL DEFAULT 1;
    PRINT 'Added StatusChangeNotifications column';
END
ELSE
BEGIN
    PRINT 'StatusChangeNotifications column already exists';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserNotificationPreferences' 
               AND COLUMN_NAME = 'CancellationNotifications')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    ADD [CancellationNotifications] BIT NOT NULL DEFAULT 1;
    PRINT 'Added CancellationNotifications column';
END
ELSE
BEGIN
    PRINT 'CancellationNotifications column already exists';
END

-- Drop and recreate the UserId1 column if it exists (EF Core bug)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'UserNotificationPreferences' 
           AND COLUMN_NAME = 'UserId1')
BEGIN
    ALTER TABLE [dbo].[UserNotificationPreferences]
    DROP COLUMN [UserId1];
    PRINT 'Dropped incorrect UserId1 column';
END

PRINT 'User notification preferences update migration completed successfully!';

