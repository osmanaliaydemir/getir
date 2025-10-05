-- =============================================
-- Realtime Tracking System Migration
-- Migration: 018-realtime-tracking-system.sql
-- Description: Creates tables for realtime order tracking system
-- Created: 2024-12-19
-- =============================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET NOCOUNT ON;

-- Drop tables if they exist (for re-runnability)
IF OBJECT_ID('TrackingSettings', 'U') IS NOT NULL DROP TABLE TrackingSettings;
IF OBJECT_ID('ETAEstimations', 'U') IS NOT NULL DROP TABLE ETAEstimations;
IF OBJECT_ID('TrackingNotifications', 'U') IS NOT NULL DROP TABLE TrackingNotifications;
IF OBJECT_ID('LocationHistories', 'U') IS NOT NULL DROP TABLE LocationHistories;
IF OBJECT_ID('OrderTrackings', 'U') IS NOT NULL DROP TABLE OrderTrackings;

-- =============================================
-- OrderTrackings Table
-- =============================================
CREATE TABLE OrderTrackings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    CourierId UNIQUEIDENTIFIER NULL,
    Status INT NOT NULL, -- TrackingStatus enum
    StatusMessage NVARCHAR(500) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    Address NVARCHAR(500) NULL,
    City NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    LocationUpdateType INT NOT NULL DEFAULT 1, -- LocationUpdateType enum
    Accuracy FLOAT NULL,
    EstimatedArrivalTime DATETIME2 NULL,
    ActualArrivalTime DATETIME2 NULL,
    EstimatedMinutesRemaining INT NULL,
    DistanceFromDestination FLOAT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastUpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_OrderTrackings_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_OrderTrackings_Couriers FOREIGN KEY (CourierId) REFERENCES Couriers(Id) ON DELETE SET NULL,
    CONSTRAINT FK_OrderTrackings_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- Indexes for OrderTrackings
CREATE INDEX IX_OrderTrackings_OrderId ON OrderTrackings(OrderId);
CREATE INDEX IX_OrderTrackings_CourierId ON OrderTrackings(CourierId);
CREATE INDEX IX_OrderTrackings_Status ON OrderTrackings(Status);
CREATE INDEX IX_OrderTrackings_IsActive ON OrderTrackings(IsActive);
CREATE INDEX IX_OrderTrackings_LastUpdatedAt ON OrderTrackings(LastUpdatedAt);

-- =============================================
-- LocationHistories Table
-- =============================================
CREATE TABLE LocationHistories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderTrackingId UNIQUEIDENTIFIER NOT NULL,
    Latitude FLOAT NOT NULL,
    Longitude FLOAT NOT NULL,
    Address NVARCHAR(500) NULL,
    City NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    UpdateType INT NOT NULL, -- LocationUpdateType enum
    Accuracy FLOAT NULL,
    Speed FLOAT NULL,
    Bearing FLOAT NULL,
    Altitude FLOAT NULL,
    DeviceInfo NVARCHAR(200) NULL,
    AppVersion NVARCHAR(50) NULL,
    RecordedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_LocationHistories_OrderTrackings FOREIGN KEY (OrderTrackingId) REFERENCES OrderTrackings(Id) ON DELETE CASCADE
);

-- Indexes for LocationHistories
CREATE INDEX IX_LocationHistories_OrderTrackingId ON LocationHistories(OrderTrackingId);
CREATE INDEX IX_LocationHistories_RecordedAt ON LocationHistories(RecordedAt);
CREATE INDEX IX_LocationHistories_UpdateType ON LocationHistories(UpdateType);

-- =============================================
-- TrackingNotifications Table
-- =============================================
CREATE TABLE TrackingNotifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderTrackingId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NULL,
    Type INT NOT NULL, -- NotificationType enum
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Data NVARCHAR(2000) NULL,
    IsSent BIT NOT NULL DEFAULT 0,
    IsRead BIT NOT NULL DEFAULT 0,
    SentAt DATETIME2 NULL,
    ReadAt DATETIME2 NULL,
    DeliveryMethod NVARCHAR(50) NULL,
    DeliveryStatus NVARCHAR(50) NULL,
    ErrorMessage NVARCHAR(500) NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_TrackingNotifications_OrderTrackings FOREIGN KEY (OrderTrackingId) REFERENCES OrderTrackings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TrackingNotifications_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- Indexes for TrackingNotifications
CREATE INDEX IX_TrackingNotifications_OrderTrackingId ON TrackingNotifications(OrderTrackingId);
CREATE INDEX IX_TrackingNotifications_UserId ON TrackingNotifications(UserId);
CREATE INDEX IX_TrackingNotifications_Type ON TrackingNotifications(Type);
CREATE INDEX IX_TrackingNotifications_IsSent ON TrackingNotifications(IsSent);
CREATE INDEX IX_TrackingNotifications_IsRead ON TrackingNotifications(IsRead);
CREATE INDEX IX_TrackingNotifications_CreatedAt ON TrackingNotifications(CreatedAt);

-- =============================================
-- ETAEstimations Table
-- =============================================
CREATE TABLE ETAEstimations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderTrackingId UNIQUEIDENTIFIER NOT NULL,
    EstimatedArrivalTime DATETIME2 NOT NULL,
    EstimatedMinutesRemaining INT NOT NULL,
    DistanceRemaining FLOAT NULL,
    AverageSpeed FLOAT NULL,
    CalculationMethod NVARCHAR(100) NULL,
    Confidence FLOAT NULL,
    Notes NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_ETAEstimations_OrderTrackings FOREIGN KEY (OrderTrackingId) REFERENCES OrderTrackings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ETAEstimations_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- Indexes for ETAEstimations
CREATE INDEX IX_ETAEstimations_OrderTrackingId ON ETAEstimations(OrderTrackingId);
CREATE INDEX IX_ETAEstimations_IsActive ON ETAEstimations(IsActive);
CREATE INDEX IX_ETAEstimations_CreatedAt ON ETAEstimations(CreatedAt);

-- =============================================
-- TrackingSettings Table
-- =============================================
CREATE TABLE TrackingSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL,
    MerchantId UNIQUEIDENTIFIER NULL,
    EnableLocationTracking BIT NOT NULL DEFAULT 1,
    EnablePushNotifications BIT NOT NULL DEFAULT 1,
    EnableSMSNotifications BIT NOT NULL DEFAULT 1,
    EnableEmailNotifications BIT NOT NULL DEFAULT 1,
    LocationUpdateInterval INT NOT NULL DEFAULT 30,
    NotificationInterval INT NOT NULL DEFAULT 300,
    LocationAccuracyThreshold FLOAT NOT NULL DEFAULT 100,
    EnableETAUpdates BIT NOT NULL DEFAULT 1,
    ETAUpdateInterval INT NOT NULL DEFAULT 60,
    EnableDelayAlerts BIT NOT NULL DEFAULT 1,
    DelayThresholdMinutes INT NOT NULL DEFAULT 15,
    EnableNearbyAlerts BIT NOT NULL DEFAULT 1,
    NearbyDistanceMeters FLOAT NOT NULL DEFAULT 500,
    PreferredLanguage NVARCHAR(10) NULL,
    TimeZone NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_TrackingSettings_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TrackingSettings_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_TrackingSettings_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_TrackingSettings_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- Indexes for TrackingSettings
CREATE INDEX IX_TrackingSettings_UserId ON TrackingSettings(UserId);
CREATE INDEX IX_TrackingSettings_MerchantId ON TrackingSettings(MerchantId);

-- =============================================
-- Sample Data
-- =============================================

-- Sample OrderTrackings (using variables for SQL Server 2014 compatibility)
DECLARE @OrderId1 UNIQUEIDENTIFIER, @OrderId2 UNIQUEIDENTIFIER, @OrderId3 UNIQUEIDENTIFIER, @OrderId4 UNIQUEIDENTIFIER;
DECLARE @CourierId1 UNIQUEIDENTIFIER;

SELECT TOP 1 @OrderId1 = Id FROM Orders ORDER BY CreatedAt DESC;
SELECT TOP 1 @OrderId2 = Id FROM Orders WHERE Id != @OrderId1 ORDER BY CreatedAt DESC;
SELECT TOP 1 @OrderId3 = Id FROM Orders WHERE Id NOT IN (@OrderId1, @OrderId2) ORDER BY CreatedAt DESC;
SELECT TOP 1 @OrderId4 = Id FROM Orders WHERE Id NOT IN (@OrderId1, @OrderId2, @OrderId3) ORDER BY CreatedAt DESC;
SELECT TOP 1 @CourierId1 = Id FROM Couriers;

INSERT INTO OrderTrackings (Id, OrderId, CourierId, Status, StatusMessage, Latitude, Longitude, Address, City, District, LocationUpdateType, Accuracy, EstimatedArrivalTime, EstimatedMinutesRemaining, DistanceFromDestination, IsActive, LastUpdatedAt, CreatedAt)
VALUES 
(NEWID(), @OrderId1, @CourierId1, 5, 'Siparişiniz size doğru yolda', 41.0082, 28.9784, 'Beşiktaş, İstanbul', 'İstanbul', 'Beşiktaş', 2, 10.5, DATEADD(MINUTE, 15, GETUTCDATE()), 15, 2.5, 1, GETUTCDATE(), DATEADD(MINUTE, -30, GETUTCDATE())),
(NEWID(), @OrderId2, @CourierId1, 3, 'Siparişiniz hazırlanıyor', 41.0082, 28.9784, 'Kadıköy, İstanbul', 'İstanbul', 'Kadıköy', 1, 50.0, DATEADD(MINUTE, 45, GETUTCDATE()), 45, 8.2, 1, GETUTCDATE(), DATEADD(MINUTE, -20, GETUTCDATE())),
(NEWID(), @OrderId3, @CourierId1, 1, 'Siparişiniz alındı ve işleme konuldu', 41.0082, 28.9784, 'Şişli, İstanbul', 'İstanbul', 'Şişli', 1, 100.0, DATEADD(MINUTE, 60, GETUTCDATE()), 60, 12.0, 1, GETUTCDATE(), DATEADD(MINUTE, -10, GETUTCDATE())),
(NEWID(), @OrderId4, @CourierId1, 9, 'Siparişiniz başarıyla teslim edildi', 41.0082, 28.9784, 'Beyoğlu, İstanbul', 'İstanbul', 'Beyoğlu', 2, 5.0, GETUTCDATE(), 0, 0.0, 0, GETUTCDATE(), DATEADD(HOUR, -2, GETUTCDATE()));

-- Sample LocationHistories
INSERT INTO LocationHistories (Id, OrderTrackingId, Latitude, Longitude, Address, City, District, UpdateType, Accuracy, Speed, Bearing, Altitude, DeviceInfo, AppVersion, RecordedAt, CreatedAt)
SELECT 
    NEWID(),
    ot.Id,
    ot.Latitude,
    ot.Longitude,
    ot.Address,
    ot.City,
    ot.District,
    ot.LocationUpdateType,
    ot.Accuracy,
    25.5,
    45.0,
    100.0,
    'iPhone 13',
    '1.2.3',
    ot.LastUpdatedAt,
    ot.CreatedAt
FROM OrderTrackings ot
WHERE ot.IsActive = 1;

-- Sample TrackingNotifications
INSERT INTO TrackingNotifications (Id, OrderTrackingId, UserId, Type, Title, Message, Data, IsSent, IsRead, SentAt, DeliveryMethod, DeliveryStatus, RetryCount, CreatedAt)
SELECT 
    NEWID(),
    ot.Id,
    (SELECT TOP 1 Id FROM Users),
    CASE ot.Status
        WHEN 1 THEN 1 -- StatusUpdate
        WHEN 3 THEN 1 -- StatusUpdate
        WHEN 5 THEN 2 -- LocationUpdate
        WHEN 9 THEN 6 -- CompletionAlert
        ELSE 1
    END,
    CASE ot.Status
        WHEN 1 THEN 'Sipariş Durumu Güncellendi'
        WHEN 3 THEN 'Sipariş Durumu Güncellendi'
        WHEN 5 THEN 'Konum Güncellendi'
        WHEN 9 THEN 'Sipariş Teslim Edildi'
        ELSE 'Bildirim'
    END,
    CASE ot.Status
        WHEN 1 THEN 'Siparişiniz alındı ve işleme konuldu'
        WHEN 3 THEN 'Siparişiniz hazırlanıyor'
        WHEN 5 THEN 'Siparişiniz size doğru yolda'
        WHEN 9 THEN 'Siparişiniz başarıyla teslim edildi. Teşekkür ederiz!'
        ELSE 'Bildirim mesajı'
    END,
    '{"status":"' + CAST(ot.Status AS NVARCHAR(10)) + '","estimatedTime":"' + CAST(ot.EstimatedMinutesRemaining AS NVARCHAR(10)) + ' minutes"}',
    1,
    CASE WHEN ot.Status = 9 THEN 1 ELSE 0 END,
    ot.LastUpdatedAt,
    'push',
    'delivered',
    0,
    ot.CreatedAt
FROM OrderTrackings ot;

-- Sample ETAEstimations
INSERT INTO ETAEstimations (Id, OrderTrackingId, EstimatedArrivalTime, EstimatedMinutesRemaining, DistanceRemaining, AverageSpeed, CalculationMethod, Confidence, Notes, IsActive, CreatedAt, CreatedBy)
SELECT 
    NEWID(),
    ot.Id,
    ot.EstimatedArrivalTime,
    ot.EstimatedMinutesRemaining,
    ot.DistanceFromDestination,
    25.0,
    'algorithm',
    0.85,
    'Based on current traffic conditions',
    1,
    ot.CreatedAt,
    (SELECT TOP 1 Id FROM Users)
FROM OrderTrackings ot
WHERE ot.IsActive = 1;

-- Sample TrackingSettings (using variables for SQL Server 2014 compatibility)
DECLARE @UserId1 UNIQUEIDENTIFIER, @UserId2 UNIQUEIDENTIFIER, @MerchantId1 UNIQUEIDENTIFIER;

SELECT TOP 1 @UserId1 = Id FROM Users;
SELECT TOP 1 @UserId2 = Id FROM Users WHERE Id != @UserId1;
SELECT TOP 1 @MerchantId1 = Id FROM Merchants;

INSERT INTO TrackingSettings (Id, UserId, MerchantId, EnableLocationTracking, EnablePushNotifications, EnableSMSNotifications, EnableEmailNotifications, LocationUpdateInterval, NotificationInterval, LocationAccuracyThreshold, EnableETAUpdates, ETAUpdateInterval, EnableDelayAlerts, DelayThresholdMinutes, EnableNearbyAlerts, NearbyDistanceMeters, PreferredLanguage, TimeZone, CreatedAt, UpdatedAt, CreatedBy)
VALUES 
(NEWID(), @UserId1, NULL, 1, 1, 1, 0, 30, 300, 100, 1, 60, 1, 15, 1, 500, 'tr', 'Europe/Istanbul', DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE()), @UserId1),
(NEWID(), NULL, @MerchantId1, 1, 1, 0, 1, 60, 600, 200, 1, 120, 1, 30, 0, 1000, 'tr', 'Europe/Istanbul', DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()), @UserId1),
(NEWID(), @UserId2, NULL, 1, 1, 0, 1, 45, 450, 150, 1, 90, 1, 20, 1, 750, 'en', 'Europe/London', DATEADD(DAY, -15, GETUTCDATE()), NULL, @UserId1);

PRINT 'Realtime tracking system migration completed successfully!';
PRINT 'Created tables: OrderTrackings, LocationHistories, TrackingNotifications, ETAEstimations, TrackingSettings';
PRINT 'Inserted sample data for realtime tracking system';

-- Count records using variables
DECLARE @OrderTrackingCount INT, @LocationHistoryCount INT, @NotificationCount INT, @ETACount INT, @SettingsCount INT;
SELECT @OrderTrackingCount = COUNT(*) FROM OrderTrackings;
SELECT @LocationHistoryCount = COUNT(*) FROM LocationHistories;
SELECT @NotificationCount = COUNT(*) FROM TrackingNotifications;
SELECT @ETACount = COUNT(*) FROM ETAEstimations;
SELECT @SettingsCount = COUNT(*) FROM TrackingSettings;

PRINT 'Total order trackings inserted: ' + CAST(@OrderTrackingCount AS NVARCHAR(10));
PRINT 'Total location histories inserted: ' + CAST(@LocationHistoryCount AS NVARCHAR(10));
PRINT 'Total tracking notifications inserted: ' + CAST(@NotificationCount AS NVARCHAR(10));
PRINT 'Total ETA estimations inserted: ' + CAST(@ETACount AS NVARCHAR(10));
PRINT 'Total tracking settings inserted: ' + CAST(@SettingsCount AS NVARCHAR(10));
