-- =============================================
-- Delivery Optimization System Migration
-- Migration: 013-delivery-optimization.sql
-- Description: Adds delivery capacity management and route optimization tables
-- Created: 2025-01-03
-- =============================================

-- Set required options
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET ANSI_NULL_DFLT_ON ON;

-- DELIVERY CAPACITY MANAGEMENT
CREATE TABLE DeliveryCapacities (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    MerchantId UNIQUEIDENTIFIER NOT NULL,
    DeliveryZoneId UNIQUEIDENTIFIER NULL,
    
    -- Capacity Settings
    MaxConcurrentDeliveries INT NOT NULL DEFAULT 10,
    MaxDailyDeliveries INT NOT NULL DEFAULT 100,
    MaxWeeklyDeliveries INT NOT NULL DEFAULT 500,
    
    -- Time Restrictions
    PeakStartTime TIME NULL,
    PeakEndTime TIME NULL,
    PeakHourCapacityReduction INT NOT NULL DEFAULT 0,
    
    -- Distance Restrictions
    MaxDeliveryDistanceKm DECIMAL(18, 2) NULL,
    DistanceBasedFeeMultiplier DECIMAL(18, 4) NOT NULL DEFAULT 1.0000,
    
    -- Dynamic Capacity Settings
    IsDynamicCapacityEnabled BIT NOT NULL DEFAULT 1,
    CurrentActiveDeliveries INT NOT NULL DEFAULT 0,
    TodayDeliveryCount INT NOT NULL DEFAULT 0,
    ThisWeekDeliveryCount INT NOT NULL DEFAULT 0,
    
    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    LastResetDate DATETIME2 NULL,
    
    CONSTRAINT FK_DeliveryCapacities_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DeliveryCapacities_DeliveryZones FOREIGN KEY (DeliveryZoneId) REFERENCES DeliveryZones(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_DeliveryCapacities_MaxConcurrentDeliveries CHECK (MaxConcurrentDeliveries > 0),
    CONSTRAINT CK_DeliveryCapacities_MaxDailyDeliveries CHECK (MaxDailyDeliveries > 0),
    CONSTRAINT CK_DeliveryCapacities_MaxWeeklyDeliveries CHECK (MaxWeeklyDeliveries > 0),
    CONSTRAINT CK_DeliveryCapacities_PeakHourCapacityReduction CHECK (PeakHourCapacityReduction >= 0 AND PeakHourCapacityReduction <= 100),
    CONSTRAINT CK_DeliveryCapacities_MaxDeliveryDistanceKm CHECK (MaxDeliveryDistanceKm IS NULL OR MaxDeliveryDistanceKm > 0),
    CONSTRAINT CK_DeliveryCapacities_DistanceBasedFeeMultiplier CHECK (DistanceBasedFeeMultiplier > 0),
    CONSTRAINT CK_DeliveryCapacities_CurrentActiveDeliveries CHECK (CurrentActiveDeliveries >= 0),
    CONSTRAINT CK_DeliveryCapacities_TodayDeliveryCount CHECK (TodayDeliveryCount >= 0),
    CONSTRAINT CK_DeliveryCapacities_ThisWeekDeliveryCount CHECK (ThisWeekDeliveryCount >= 0)
);

-- DELIVERY ROUTES
CREATE TABLE DeliveryRoutes (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    CourierId UNIQUEIDENTIFIER NULL,
    
    -- Route Information
    RouteName NVARCHAR(200) NOT NULL,
    RouteType NVARCHAR(50) NOT NULL,
    
    -- Coordinates (stored as JSON)
    Waypoints NVARCHAR(MAX) NOT NULL, -- JSON array of coordinates
    Polyline NVARCHAR(MAX) NOT NULL, -- Google Maps polyline string
    
    -- Route Metrics
    TotalDistanceKm DECIMAL(18, 2) NOT NULL,
    EstimatedDurationMinutes INT NOT NULL,
    EstimatedTrafficDelayMinutes INT NOT NULL DEFAULT 0,
    EstimatedFuelCost DECIMAL(18, 2) NOT NULL DEFAULT 0,
    
    -- Route Quality
    RouteScore DECIMAL(5, 2) NOT NULL DEFAULT 0, -- 0-100 route quality score
    HasTollRoads BIT NOT NULL DEFAULT 0,
    HasHighTrafficAreas BIT NOT NULL DEFAULT 0,
    IsHighwayPreferred BIT NOT NULL DEFAULT 0,
    
    -- Status
    IsSelected BIT NOT NULL DEFAULT 0,
    IsCompleted BIT NOT NULL DEFAULT 0,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    
    -- Metadata
    Notes NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    
    CONSTRAINT FK_DeliveryRoutes_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DeliveryRoutes_Couriers FOREIGN KEY (CourierId) REFERENCES Couriers(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_DeliveryRoutes_TotalDistanceKm CHECK (TotalDistanceKm >= 0),
    CONSTRAINT CK_DeliveryRoutes_EstimatedDurationMinutes CHECK (EstimatedDurationMinutes > 0),
    CONSTRAINT CK_DeliveryRoutes_EstimatedTrafficDelayMinutes CHECK (EstimatedTrafficDelayMinutes >= 0),
    CONSTRAINT CK_DeliveryRoutes_EstimatedFuelCost CHECK (EstimatedFuelCost >= 0),
    CONSTRAINT CK_DeliveryRoutes_RouteScore CHECK (RouteScore >= 0 AND RouteScore <= 100)
);

-- INDEXES FOR PERFORMANCE
CREATE INDEX IX_DeliveryCapacities_MerchantId ON DeliveryCapacities(MerchantId);
CREATE INDEX IX_DeliveryCapacities_DeliveryZoneId ON DeliveryCapacities(DeliveryZoneId);
CREATE INDEX IX_DeliveryCapacities_IsActive ON DeliveryCapacities(IsActive);
CREATE INDEX IX_DeliveryCapacities_LastResetDate ON DeliveryCapacities(LastResetDate);

CREATE INDEX IX_DeliveryRoutes_OrderId ON DeliveryRoutes(OrderId);
CREATE INDEX IX_DeliveryRoutes_CourierId ON DeliveryRoutes(CourierId);
CREATE INDEX IX_DeliveryRoutes_IsSelected ON DeliveryRoutes(IsSelected);
CREATE INDEX IX_DeliveryRoutes_IsCompleted ON DeliveryRoutes(IsCompleted);
CREATE INDEX IX_DeliveryRoutes_CreatedAt ON DeliveryRoutes(CreatedAt);

-- UNIQUE CONSTRAINTS
CREATE UNIQUE INDEX UQ_DeliveryCapacities_Merchant_DeliveryZone 
ON DeliveryCapacities(MerchantId, DeliveryZoneId) 
WHERE IsActive = 1;

-- SAMPLE DATA FOR TESTING
INSERT INTO DeliveryCapacities (
    MerchantId, 
    MaxConcurrentDeliveries, 
    MaxDailyDeliveries, 
    MaxWeeklyDeliveries,
    PeakStartTime,
    PeakEndTime,
    PeakHourCapacityReduction,
    MaxDeliveryDistanceKm,
    DistanceBasedFeeMultiplier,
    IsDynamicCapacityEnabled,
    LastResetDate
)
SELECT 
    m.Id,
    15, -- Max concurrent deliveries
    150, -- Max daily deliveries
    750, -- Max weekly deliveries
    '18:00:00', -- Peak start time
    '22:00:00', -- Peak end time
    20, -- 20% capacity reduction during peak hours
    15.0, -- Max delivery distance 15km
    1.2, -- Distance-based fee multiplier
    1, -- Dynamic capacity enabled
    GETUTCDATE() -- Last reset date
FROM Merchants m
WHERE m.IsActive = 1
AND NOT EXISTS (
    SELECT 1 FROM DeliveryCapacities dc 
    WHERE dc.MerchantId = m.Id
);

-- COMMENTS
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Delivery capacity management for merchants and delivery zones', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'DeliveryCapacities';

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Delivery routes and route optimization data', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'DeliveryRoutes';

-- SUCCESS MESSAGE
PRINT 'Delivery Optimization System migration completed successfully!';
PRINT 'Added tables: DeliveryCapacities, DeliveryRoutes';
PRINT 'Added indexes and constraints for performance optimization';
PRINT 'Added sample data for testing';
