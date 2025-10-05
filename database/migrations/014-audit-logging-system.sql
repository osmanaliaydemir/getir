-- =============================================
-- Audit Logging System Migration
-- Migration: 014-audit-logging-system.sql
-- Description: Adds comprehensive audit logging tables for user activity, system changes, security events, and log analysis reports
-- Created: 2025-01-03
-- =============================================

-- Set required options for SQL Server 2014 compatibility
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET ANSI_NULL_DFLT_ON ON;

-- Drop tables if they exist to allow re-running the script during development
IF OBJECT_ID('LogAnalysisReports', 'U') IS NOT NULL DROP TABLE LogAnalysisReports;
IF OBJECT_ID('SecurityEventLogs', 'U') IS NOT NULL DROP TABLE SecurityEventLogs;
IF OBJECT_ID('SystemChangeLogs', 'U') IS NOT NULL DROP TABLE SystemChangeLogs;
IF OBJECT_ID('UserActivityLogs', 'U') IS NOT NULL DROP TABLE UserActivityLogs;

-- USER ACTIVITY LOGS
CREATE TABLE UserActivityLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    ActivityType NVARCHAR(100) NOT NULL,
    ActivityDescription NVARCHAR(200) NOT NULL,
    EntityType NVARCHAR(100) NULL,
    EntityId NVARCHAR(50) NULL,
    ActivityData NVARCHAR(2000) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    SessionId NVARCHAR(100) NULL,
    RequestId NVARCHAR(50) NULL,
    DeviceType NVARCHAR(100) NULL,
    Browser NVARCHAR(100) NULL,
    OperatingSystem NVARCHAR(100) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    Location NVARCHAR(200) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Duration INT NOT NULL DEFAULT 0,
    IsSuccess BIT NOT NULL DEFAULT 1,
    ErrorMessage NVARCHAR(500) NULL,
    
    -- Foreign Key Constraints
    CONSTRAINT FK_UserActivityLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    
    -- Check Constraints
    CONSTRAINT CK_UserActivityLogs_Duration CHECK (Duration >= 0)
);

-- SYSTEM CHANGE LOGS
CREATE TABLE SystemChangeLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ChangeType NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(50) NOT NULL,
    EntityName NVARCHAR(200) NULL,
    OldValues NVARCHAR(2000) NULL,
    NewValues NVARCHAR(2000) NULL,
    ChangedFields NVARCHAR(2000) NULL,
    ChangeReason NVARCHAR(1000) NULL,
    ChangeSource NVARCHAR(100) NULL,
    ChangedByUserId UNIQUEIDENTIFIER NULL,
    ChangedByUserName NVARCHAR(100) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    SessionId NVARCHAR(100) NULL,
    RequestId NVARCHAR(50) NULL,
    CorrelationId NVARCHAR(100) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsSuccess BIT NOT NULL DEFAULT 1,
    ErrorMessage NVARCHAR(500) NULL,
    Severity NVARCHAR(50) NULL DEFAULT 'INFO',
    
    -- Foreign Key Constraints
    CONSTRAINT FK_SystemChangeLogs_Users FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- SECURITY EVENT LOGS
CREATE TABLE SecurityEventLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EventType NVARCHAR(100) NOT NULL,
    EventTitle NVARCHAR(200) NOT NULL,
    EventDescription NVARCHAR(2000) NOT NULL,
    Severity NVARCHAR(50) NULL DEFAULT 'MEDIUM',
    RiskLevel NVARCHAR(50) NULL DEFAULT 'MEDIUM',
    UserId UNIQUEIDENTIFIER NULL,
    UserName NVARCHAR(100) NULL,
    UserRole NVARCHAR(100) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    DeviceFingerprint NVARCHAR(100) NULL,
    SessionId NVARCHAR(100) NULL,
    RequestId NVARCHAR(50) NULL,
    CorrelationId NVARCHAR(100) NULL,
    EventData NVARCHAR(2000) NULL,
    ThreatIndicators NVARCHAR(2000) NULL,
    MitigationActions NVARCHAR(1000) NULL,
    Source NVARCHAR(100) NULL,
    Category NVARCHAR(100) NULL,
    Latitude FLOAT NULL,
    Longitude FLOAT NULL,
    Location NVARCHAR(200) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsResolved BIT NOT NULL DEFAULT 0,
    ResolvedAt DATETIME2 NULL,
    ResolvedBy NVARCHAR(100) NULL,
    ResolutionNotes NVARCHAR(1000) NULL,
    RequiresInvestigation BIT NOT NULL DEFAULT 0,
    IsFalsePositive BIT NOT NULL DEFAULT 0,
    
    -- Foreign Key Constraints
    CONSTRAINT FK_SecurityEventLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- LOG ANALYSIS REPORTS
CREATE TABLE LogAnalysisReports (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ReportType NVARCHAR(100) NOT NULL,
    ReportTitle NVARCHAR(200) NOT NULL,
    ReportDescription NVARCHAR(2000) NULL,
    ReportStartDate DATETIME2 NOT NULL,
    ReportEndDate DATETIME2 NOT NULL,
    TimeZone NVARCHAR(50) NULL DEFAULT 'UTC',
    ReportData NVARCHAR(2000) NULL,
    Summary NVARCHAR(2000) NULL,
    Insights NVARCHAR(2000) NULL,
    Alerts NVARCHAR(2000) NULL,
    Charts NVARCHAR(2000) NULL,
    Status NVARCHAR(100) NULL DEFAULT 'GENERATED',
    Format NVARCHAR(50) NULL DEFAULT 'JSON',
    FilePath NVARCHAR(500) NULL,
    FileName NVARCHAR(100) NULL,
    FileSizeBytes BIGINT NULL,
    GeneratedByUserId UNIQUEIDENTIFIER NULL,
    GeneratedByUserName NVARCHAR(100) NULL,
    GeneratedByRole NVARCHAR(100) NULL,
    GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    Recipients NVARCHAR(1000) NULL,
    IsScheduled BIT NOT NULL DEFAULT 0,
    SchedulePattern NVARCHAR(100) NULL,
    NextScheduledRun DATETIME2 NULL,
    GenerationTimeMs INT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(500) NULL,
    
    -- Foreign Key Constraints
    CONSTRAINT FK_LogAnalysisReports_Users FOREIGN KEY (GeneratedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    
    -- Check Constraints
    CONSTRAINT CK_LogAnalysisReports_GenerationTimeMs CHECK (GenerationTimeMs >= 0),
    CONSTRAINT CK_LogAnalysisReports_FileSizeBytes CHECK (FileSizeBytes >= 0)
);

-- =============================================
-- INDEXES FOR PERFORMANCE OPTIMIZATION
-- =============================================

-- UserActivityLogs Indexes
CREATE INDEX IX_UserActivityLogs_UserId_Timestamp ON UserActivityLogs (UserId, Timestamp);
CREATE INDEX IX_UserActivityLogs_ActivityType_Timestamp ON UserActivityLogs (ActivityType, Timestamp);
CREATE INDEX IX_UserActivityLogs_EntityType_EntityId ON UserActivityLogs (EntityType, EntityId);
CREATE INDEX IX_UserActivityLogs_Timestamp ON UserActivityLogs (Timestamp);
CREATE INDEX IX_UserActivityLogs_IsSuccess ON UserActivityLogs (IsSuccess);

-- SystemChangeLogs Indexes
CREATE INDEX IX_SystemChangeLogs_EntityType_EntityId_Timestamp ON SystemChangeLogs (EntityType, EntityId, Timestamp);
CREATE INDEX IX_SystemChangeLogs_ChangedByUserId_Timestamp ON SystemChangeLogs (ChangedByUserId, Timestamp);
CREATE INDEX IX_SystemChangeLogs_ChangeType_Timestamp ON SystemChangeLogs (ChangeType, Timestamp);
CREATE INDEX IX_SystemChangeLogs_Timestamp ON SystemChangeLogs (Timestamp);
CREATE INDEX IX_SystemChangeLogs_Severity ON SystemChangeLogs (Severity);

-- SecurityEventLogs Indexes
CREATE INDEX IX_SecurityEventLogs_EventType_Timestamp ON SecurityEventLogs (EventType, Timestamp);
CREATE INDEX IX_SecurityEventLogs_Severity_RiskLevel_Timestamp ON SecurityEventLogs (Severity, RiskLevel, Timestamp);
CREATE INDEX IX_SecurityEventLogs_UserId_Timestamp ON SecurityEventLogs (UserId, Timestamp);
CREATE INDEX IX_SecurityEventLogs_IpAddress_Timestamp ON SecurityEventLogs (IpAddress, Timestamp);
CREATE INDEX IX_SecurityEventLogs_IsResolved_RequiresInvestigation ON SecurityEventLogs (IsResolved, RequiresInvestigation);
CREATE INDEX IX_SecurityEventLogs_Timestamp ON SecurityEventLogs (Timestamp);
CREATE INDEX IX_SecurityEventLogs_Severity ON SecurityEventLogs (Severity);
CREATE INDEX IX_SecurityEventLogs_RiskLevel ON SecurityEventLogs (RiskLevel);

-- LogAnalysisReports Indexes
CREATE INDEX IX_LogAnalysisReports_ReportType_GeneratedAt ON LogAnalysisReports (ReportType, GeneratedAt);
CREATE INDEX IX_LogAnalysisReports_Status_GeneratedAt ON LogAnalysisReports (Status, GeneratedAt);
CREATE INDEX IX_LogAnalysisReports_GeneratedByUserId_GeneratedAt ON LogAnalysisReports (GeneratedByUserId, GeneratedAt);
CREATE INDEX IX_LogAnalysisReports_IsScheduled_NextScheduledRun ON LogAnalysisReports (IsScheduled, NextScheduledRun);
CREATE INDEX IX_LogAnalysisReports_GeneratedAt ON LogAnalysisReports (GeneratedAt);
CREATE INDEX IX_LogAnalysisReports_ExpiresAt ON LogAnalysisReports (ExpiresAt);

-- =============================================
-- SAMPLE DATA FOR TESTING (OPTIONAL)
-- =============================================

-- Insert sample user activity log
INSERT INTO UserActivityLogs (UserId, UserName, ActivityType, ActivityDescription, EntityType, EntityId, IpAddress, UserAgent, DeviceType, Browser, OperatingSystem, IsSuccess)
SELECT TOP 1 
    Id, 
    COALESCE(FirstName + ' ' + LastName, Email), 
    'LOGIN', 
    'User logged in successfully', 
    'User', 
    CAST(Id AS NVARCHAR(50)), 
    '192.168.1.1', 
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', 
    'Desktop', 
    'Chrome', 
    'Windows 10', 
    1
FROM Users 
WHERE FirstName IS NOT NULL OR Email IS NOT NULL;

-- Insert sample system change log
INSERT INTO SystemChangeLogs (ChangeType, EntityType, EntityId, EntityName, ChangeReason, ChangeSource, ChangedByUserId, ChangedByUserName, IpAddress, IsSuccess, Severity)
SELECT TOP 1 
    'UPDATE', 
    'User', 
    CAST(Id AS NVARCHAR(50)), 
    COALESCE(FirstName + ' ' + LastName, Email), 
    'Profile information updated', 
    'API', 
    Id, 
    COALESCE(FirstName + ' ' + LastName, Email), 
    '192.168.1.1', 
    1, 
    'INFO'
FROM Users 
WHERE FirstName IS NOT NULL OR Email IS NOT NULL;

-- Insert sample security event log
INSERT INTO SecurityEventLogs (EventType, EventTitle, EventDescription, Severity, RiskLevel, Source, Category, IsResolved, RequiresInvestigation)
VALUES (
    'LOGIN_ATTEMPT', 
    'Multiple failed login attempts detected', 
    'User attempted to login 5 times with incorrect password', 
    'HIGH', 
    'MEDIUM', 
    'Authentication System', 
    'Authentication', 
    0, 
    1
);

-- Insert sample log analysis report
INSERT INTO LogAnalysisReports (ReportType, ReportTitle, ReportDescription, ReportStartDate, ReportEndDate, Status, Format, IsPublic, GenerationTimeMs)
VALUES (
    'DAILY', 
    'Daily Security Report', 
    'Daily security events and user activity summary', 
    DATEADD(DAY, -1, GETUTCDATE()), 
    GETUTCDATE(), 
    'GENERATED', 
    'JSON', 
    0, 
    1500
);

PRINT 'Audit Logging System migration completed successfully!';
PRINT 'Created tables: UserActivityLogs, SystemChangeLogs, SecurityEventLogs, LogAnalysisReports';
PRINT 'Created indexes for performance optimization';
PRINT 'Inserted sample data for testing';
