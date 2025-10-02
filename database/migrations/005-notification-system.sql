-- =====================================================
-- NOTIFICATION SYSTEM MIGRATION
-- Sprint 13: Notification System Implementation
-- =====================================================

-- Create UserNotificationPreferences table
CREATE TABLE [dbo].[UserNotificationPreferences] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    
    -- Email preferences
    [EmailEnabled] BIT NOT NULL DEFAULT 1,
    [EmailOrderUpdates] BIT NOT NULL DEFAULT 1,
    [EmailPromotions] BIT NOT NULL DEFAULT 1,
    [EmailNewsletter] BIT NOT NULL DEFAULT 1,
    [EmailSecurityAlerts] BIT NOT NULL DEFAULT 1,
    
    -- SMS preferences
    [SmsEnabled] BIT NOT NULL DEFAULT 1,
    [SmsOrderUpdates] BIT NOT NULL DEFAULT 1,
    [SmsPromotions] BIT NOT NULL DEFAULT 0,
    [SmsSecurityAlerts] BIT NOT NULL DEFAULT 1,
    
    -- Push notification preferences
    [PushEnabled] BIT NOT NULL DEFAULT 1,
    [PushOrderUpdates] BIT NOT NULL DEFAULT 1,
    [PushPromotions] BIT NOT NULL DEFAULT 1,
    [PushMerchantUpdates] BIT NOT NULL DEFAULT 1,
    [PushSecurityAlerts] BIT NOT NULL DEFAULT 1,
    
    -- Time preferences
    [QuietStartTime] TIME NULL,
    [QuietEndTime] TIME NULL,
    [RespectQuietHours] BIT NOT NULL DEFAULT 1,
    
    -- Language preference
    [Language] NVARCHAR(10) NOT NULL DEFAULT 'tr-TR',
    
    -- Timestamps
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [PK_UserNotificationPreferences] PRIMARY KEY ([Id])
);

-- Create indexes for performance
CREATE INDEX [IX_UserNotificationPreferences_UserId] ON [dbo].[UserNotificationPreferences] ([UserId]);

-- Add foreign key constraint
ALTER TABLE [dbo].[UserNotificationPreferences]
ADD CONSTRAINT [FK_UserNotificationPreferences_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
ON DELETE CASCADE;

-- Create unique constraint to ensure one preference record per user
ALTER TABLE [dbo].[UserNotificationPreferences]
ADD CONSTRAINT [UQ_UserNotificationPreferences_UserId] UNIQUE ([UserId]);

-- Create NotificationTemplates table for reusable templates
CREATE TABLE [dbo].[NotificationTemplates] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL, -- Email, SMS, Push
    [Category] NVARCHAR(50) NOT NULL, -- OrderUpdate, Promotion, etc.
    [Subject] NVARCHAR(200) NULL, -- For email
    [Content] NVARCHAR(MAX) NOT NULL,
    [IsHtml] BIT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Language] NVARCHAR(10) NOT NULL DEFAULT 'tr-TR',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [PK_NotificationTemplates] PRIMARY KEY ([Id])
);

-- Create indexes for NotificationTemplates
CREATE INDEX [IX_NotificationTemplates_Type] ON [dbo].[NotificationTemplates] ([Type]);
CREATE INDEX [IX_NotificationTemplates_Category] ON [dbo].[NotificationTemplates] ([Category]);
CREATE INDEX [IX_NotificationTemplates_Language] ON [dbo].[NotificationTemplates] ([Language]);
CREATE INDEX [IX_NotificationTemplates_IsActive] ON [dbo].[NotificationTemplates] ([IsActive]);

-- Create unique constraint for template name per type and language
ALTER TABLE [dbo].[NotificationTemplates]
ADD CONSTRAINT [UQ_NotificationTemplates_Name_Type_Language] UNIQUE ([Name], [Type], [Language]);

-- Create NotificationLogs table for tracking sent notifications
CREATE TABLE [dbo].[NotificationLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Type] NVARCHAR(50) NOT NULL, -- Email, SMS, Push
    [Category] NVARCHAR(50) NOT NULL, -- OrderUpdate, Promotion, etc.
    [Recipient] NVARCHAR(255) NOT NULL, -- Email, Phone, DeviceToken
    [Subject] NVARCHAR(200) NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Sent, Delivered, Failed
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [SentAt] DATETIME2 NULL,
    [DeliveredAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [PK_NotificationLogs] PRIMARY KEY ([Id])
);

-- Create indexes for NotificationLogs
CREATE INDEX [IX_NotificationLogs_UserId] ON [dbo].[NotificationLogs] ([UserId]);
CREATE INDEX [IX_NotificationLogs_Type] ON [dbo].[NotificationLogs] ([Type]);
CREATE INDEX [IX_NotificationLogs_Status] ON [dbo].[NotificationLogs] ([Status]);
CREATE INDEX [IX_NotificationLogs_SentAt] ON [dbo].[NotificationLogs] ([SentAt]);
CREATE INDEX [IX_NotificationLogs_CreatedAt] ON [dbo].[NotificationLogs] ([CreatedAt]);

-- Add foreign key constraint for NotificationLogs
ALTER TABLE [dbo].[NotificationLogs]
ADD CONSTRAINT [FK_NotificationLogs_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
ON DELETE CASCADE;

-- Create DeviceTokens table for push notifications
CREATE TABLE [dbo].[DeviceTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [DeviceToken] NVARCHAR(500) NOT NULL,
    [Platform] NVARCHAR(20) NOT NULL, -- Android, iOS, Web
    [DeviceModel] NVARCHAR(100) NULL,
    [AppVersion] NVARCHAR(20) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [RegisteredAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastUsedAt] DATETIME2 NULL,
    
    CONSTRAINT [PK_DeviceTokens] PRIMARY KEY ([Id])
);

-- Create indexes for DeviceTokens
CREATE INDEX [IX_DeviceTokens_UserId] ON [dbo].[DeviceTokens] ([UserId]);
CREATE INDEX [IX_DeviceTokens_DeviceToken] ON [dbo].[DeviceTokens] ([DeviceToken]);
CREATE INDEX [IX_DeviceTokens_Platform] ON [dbo].[DeviceTokens] ([Platform]);
CREATE INDEX [IX_DeviceTokens_IsActive] ON [dbo].[DeviceTokens] ([IsActive]);

-- Add foreign key constraint for DeviceTokens
ALTER TABLE [dbo].[DeviceTokens]
ADD CONSTRAINT [FK_DeviceTokens_Users] 
FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) 
ON DELETE CASCADE;

-- Create unique constraint for device token per user
ALTER TABLE [dbo].[DeviceTokens]
ADD CONSTRAINT [UQ_DeviceTokens_UserId_DeviceToken] UNIQUE ([UserId], [DeviceToken]);

-- Insert default notification templates
INSERT INTO [dbo].[NotificationTemplates] ([Name], [Type], [Category], [Subject], [Content], [IsHtml], [Language])
VALUES 
    -- Order Update Templates
    ('OrderConfirmed', 'Email', 'OrderUpdate', 'Siparişiniz Onaylandı - #{OrderNumber}', 
     '<h2>Siparişiniz Onaylandı</h2><p>Merhaba {CustomerName},</p><p>{OrderNumber} numaralı siparişiniz onaylanmıştır.</p><p>Toplam: {TotalAmount} TL</p><p>Teslimat Süresi: {DeliveryTime} dakika</p>', 
     1, 'tr-TR'),
     
    ('OrderConfirmed', 'SMS', 'OrderUpdate', NULL, 
     'Siparişiniz onaylandı! #{OrderNumber} - {TotalAmount} TL - Teslimat: {DeliveryTime} dk', 
     0, 'tr-TR'),
     
    ('OrderConfirmed', 'Push', 'OrderUpdate', NULL, 
     'Siparişiniz onaylandı! #{OrderNumber}', 
     0, 'tr-TR'),
     
    ('OrderDelivered', 'Email', 'OrderUpdate', 'Siparişiniz Teslim Edildi - #{OrderNumber}', 
     '<h2>Siparişiniz Teslim Edildi</h2><p>Merhaba {CustomerName},</p><p>{OrderNumber} numaralı siparişiniz başarıyla teslim edilmiştir.</p><p>Teşekkür ederiz!</p>', 
     1, 'tr-TR'),
     
    ('OrderDelivered', 'SMS', 'OrderUpdate', NULL, 
     'Siparişiniz teslim edildi! #{OrderNumber} - Teşekkürler!', 
     0, 'tr-TR'),
     
    ('OrderDelivered', 'Push', 'OrderUpdate', NULL, 
     'Siparişiniz teslim edildi! #{OrderNumber}', 
     0, 'tr-TR'),
     
    -- Promotion Templates
    ('NewPromotion', 'Email', 'Promotion', 'Yeni Kampanya! {DiscountPercentage}% İndirim', 
     '<h2>Yeni Kampanya!</h2><p>Merhaba {CustomerName},</p><p>{MerchantName} size özel %{DiscountPercentage} indirim fırsatı!</p><p>Kampanya Kodu: {PromoCode}</p>', 
     1, 'tr-TR'),
     
    ('NewPromotion', 'SMS', 'Promotion', NULL, 
     '{MerchantName} - %{DiscountPercentage} indirim! Kod: {PromoCode}', 
     0, 'tr-TR'),
     
    ('NewPromotion', 'Push', 'Promotion', NULL, 
     'Yeni kampanya! %{DiscountPercentage} indirim - {MerchantName}', 
     0, 'tr-TR');

-- Create default notification preferences for existing users
INSERT INTO [dbo].[UserNotificationPreferences] ([UserId])
SELECT [Id] FROM [dbo].[Users] 
WHERE [Id] NOT IN (SELECT [UserId] FROM [dbo].[UserNotificationPreferences]);

PRINT 'Notification system migration completed successfully!';
PRINT 'Created tables: UserNotificationPreferences, NotificationTemplates, NotificationLogs, DeviceTokens';
PRINT 'Inserted default notification templates';
PRINT 'Created default preferences for existing users';
