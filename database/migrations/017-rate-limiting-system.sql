-- =============================================
-- Rate Limiting System Migration
-- Migration: 017-rate-limiting-system.sql
-- Description: Creates tables for API rate limiting and throttling
-- =============================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET ANSI_NULL_DFLT_ON ON;

-- Drop tables if they exist (for re-runnability)
IF OBJECT_ID('RateLimitViolations', 'U') IS NOT NULL DROP TABLE RateLimitViolations;
IF OBJECT_ID('RateLimitLogs', 'U') IS NOT NULL DROP TABLE RateLimitLogs;
IF OBJECT_ID('RateLimitConfigurations', 'U') IS NOT NULL DROP TABLE RateLimitConfigurations;
IF OBJECT_ID('RateLimitRules', 'U') IS NOT NULL DROP TABLE RateLimitRules;

-- =============================================
-- RateLimitRules Table
-- =============================================
CREATE TABLE RateLimitRules (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Type INT NOT NULL, -- RateLimitType enum value
    Endpoint NVARCHAR(500) NULL,
    HttpMethod NVARCHAR(10) NULL,
    RequestLimit INT NOT NULL,
    Period INT NOT NULL, -- RateLimitPeriod enum value
    Action INT NOT NULL, -- RateLimitAction enum value
    ThrottleDelayMs INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 0,
    UserRole NVARCHAR(100) NULL,
    UserTier NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL
);

-- Indexes for RateLimitRules
CREATE INDEX IX_RateLimitRules_Type ON RateLimitRules (Type);
CREATE INDEX IX_RateLimitRules_Endpoint ON RateLimitRules (Endpoint);
CREATE INDEX IX_RateLimitRules_IsActive ON RateLimitRules (IsActive);
CREATE INDEX IX_RateLimitRules_Priority ON RateLimitRules (Priority);
CREATE INDEX IX_RateLimitRules_UserRole ON RateLimitRules (UserRole);
CREATE INDEX IX_RateLimitRules_UserTier ON RateLimitRules (UserTier);

-- =============================================
-- RateLimitLogs Table
-- =============================================
CREATE TABLE RateLimitLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RateLimitRuleId UNIQUEIDENTIFIER NULL,
    Endpoint NVARCHAR(500) NULL,
    HttpMethod NVARCHAR(10) NULL,
    UserId NVARCHAR(50) NULL,
    UserName NVARCHAR(100) NULL,
    UserRole NVARCHAR(100) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    Type INT NOT NULL, -- RateLimitType enum value
    Action INT NOT NULL, -- RateLimitAction enum value
    RequestCount INT NOT NULL,
    RequestLimit INT NOT NULL,
    Period INT NOT NULL, -- RateLimitPeriod enum value
    IsLimitExceeded BIT NOT NULL DEFAULT 0,
    Reason NVARCHAR(500) NULL,
    RequestTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    BlockedUntil DATETIME2 NULL,
    RequestId NVARCHAR(50) NULL,
    SessionId NVARCHAR(100) NULL,
    Country NVARCHAR(100) NULL,
    City NVARCHAR(100) NULL,
    DeviceType NVARCHAR(100) NULL,
    Browser NVARCHAR(100) NULL,
    OperatingSystem NVARCHAR(100) NULL
);

-- Indexes for RateLimitLogs
CREATE INDEX IX_RateLimitLogs_RateLimitRuleId ON RateLimitLogs (RateLimitRuleId);
CREATE INDEX IX_RateLimitLogs_Endpoint ON RateLimitLogs (Endpoint);
CREATE INDEX IX_RateLimitLogs_UserId ON RateLimitLogs (UserId);
CREATE INDEX IX_RateLimitLogs_IpAddress ON RateLimitLogs (IpAddress);
CREATE INDEX IX_RateLimitLogs_Type ON RateLimitLogs (Type);
CREATE INDEX IX_RateLimitLogs_Action ON RateLimitLogs (Action);
CREATE INDEX IX_RateLimitLogs_IsLimitExceeded ON RateLimitLogs (IsLimitExceeded);
CREATE INDEX IX_RateLimitLogs_RequestTime ON RateLimitLogs (RequestTime);

-- =============================================
-- RateLimitViolations Table
-- =============================================
CREATE TABLE RateLimitViolations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RateLimitRuleId UNIQUEIDENTIFIER NULL,
    Endpoint NVARCHAR(500) NULL,
    HttpMethod NVARCHAR(10) NULL,
    UserId NVARCHAR(50) NULL,
    UserName NVARCHAR(100) NULL,
    UserRole NVARCHAR(100) NULL,
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    Type INT NOT NULL, -- RateLimitType enum value
    Action INT NOT NULL, -- RateLimitAction enum value
    RequestCount INT NOT NULL,
    RequestLimit INT NOT NULL,
    Period INT NOT NULL, -- RateLimitPeriod enum value
    ViolationReason NVARCHAR(500) NULL,
    ViolationTime DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    BlockedUntil DATETIME2 NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    ResolvedAt DATETIME2 NULL,
    ResolvedBy NVARCHAR(100) NULL,
    ResolutionNotes NVARCHAR(1000) NULL,
    RequestId NVARCHAR(50) NULL,
    SessionId NVARCHAR(100) NULL,
    Country NVARCHAR(100) NULL,
    City NVARCHAR(100) NULL,
    DeviceType NVARCHAR(100) NULL,
    Browser NVARCHAR(100) NULL,
    OperatingSystem NVARCHAR(100) NULL,
    Severity INT NOT NULL DEFAULT 1, -- 1=Low, 2=Medium, 3=High, 4=Critical
    RequiresInvestigation BIT NOT NULL DEFAULT 0,
    InvestigationNotes NVARCHAR(1000) NULL
);

-- Indexes for RateLimitViolations
CREATE INDEX IX_RateLimitViolations_RateLimitRuleId ON RateLimitViolations (RateLimitRuleId);
CREATE INDEX IX_RateLimitViolations_Endpoint ON RateLimitViolations (Endpoint);
CREATE INDEX IX_RateLimitViolations_UserId ON RateLimitViolations (UserId);
CREATE INDEX IX_RateLimitViolations_IpAddress ON RateLimitViolations (IpAddress);
CREATE INDEX IX_RateLimitViolations_Type ON RateLimitViolations (Type);
CREATE INDEX IX_RateLimitViolations_Action ON RateLimitViolations (Action);
CREATE INDEX IX_RateLimitViolations_IsResolved ON RateLimitViolations (IsResolved);
CREATE INDEX IX_RateLimitViolations_RequiresInvestigation ON RateLimitViolations (RequiresInvestigation);
CREATE INDEX IX_RateLimitViolations_Severity ON RateLimitViolations (Severity);
CREATE INDEX IX_RateLimitViolations_ViolationTime ON RateLimitViolations (ViolationTime);

-- =============================================
-- RateLimitConfigurations Table
-- =============================================
CREATE TABLE RateLimitConfigurations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Type INT NOT NULL, -- RateLimitType enum value
    EndpointPattern NVARCHAR(500) NULL,
    HttpMethod NVARCHAR(10) NULL,
    RequestLimit INT NOT NULL,
    Period INT NOT NULL, -- RateLimitPeriod enum value
    Action INT NOT NULL, -- RateLimitAction enum value
    ThrottleDelayMs INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 0,
    UserRole NVARCHAR(100) NULL,
    UserTier NVARCHAR(100) NULL,
    IpWhitelist NVARCHAR(2000) NULL,
    IpBlacklist NVARCHAR(2000) NULL,
    UserWhitelist NVARCHAR(2000) NULL,
    UserBlacklist NVARCHAR(2000) NULL,
    EnableLogging BIT NOT NULL DEFAULT 1,
    EnableAlerting BIT NOT NULL DEFAULT 0,
    AlertThreshold INT NULL,
    AlertRecipients NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL
);

-- Indexes for RateLimitConfigurations
CREATE INDEX IX_RateLimitConfigurations_Type ON RateLimitConfigurations (Type);
CREATE INDEX IX_RateLimitConfigurations_EndpointPattern ON RateLimitConfigurations (EndpointPattern);
CREATE INDEX IX_RateLimitConfigurations_IsActive ON RateLimitConfigurations (IsActive);
CREATE INDEX IX_RateLimitConfigurations_Priority ON RateLimitConfigurations (Priority);
CREATE INDEX IX_RateLimitConfigurations_UserRole ON RateLimitConfigurations (UserRole);
CREATE INDEX IX_RateLimitConfigurations_UserTier ON RateLimitConfigurations (UserTier);

-- =============================================
-- Foreign Key Constraints
-- =============================================

-- RateLimitRules foreign keys
ALTER TABLE RateLimitRules
ADD CONSTRAINT FK_RateLimitRules_CreatedBy_Users
FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

ALTER TABLE RateLimitRules
ADD CONSTRAINT FK_RateLimitRules_UpdatedBy_Users
FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

-- RateLimitLogs foreign keys
ALTER TABLE RateLimitLogs
ADD CONSTRAINT FK_RateLimitLogs_RateLimitRuleId_RateLimitRules
FOREIGN KEY (RateLimitRuleId) REFERENCES RateLimitRules(Id) ON DELETE SET NULL;

-- RateLimitViolations foreign keys
ALTER TABLE RateLimitViolations
ADD CONSTRAINT FK_RateLimitViolations_RateLimitRuleId_RateLimitRules
FOREIGN KEY (RateLimitRuleId) REFERENCES RateLimitRules(Id) ON DELETE SET NULL;

-- RateLimitConfigurations foreign keys
ALTER TABLE RateLimitConfigurations
ADD CONSTRAINT FK_RateLimitConfigurations_CreatedBy_Users
FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

ALTER TABLE RateLimitConfigurations
ADD CONSTRAINT FK_RateLimitConfigurations_UpdatedBy_Users
FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

-- =============================================
-- Sample Data
-- =============================================

-- Insert default rate limit rules
INSERT INTO RateLimitRules (Name, Description, Type, RequestLimit, Period, Action, ThrottleDelayMs, IsActive, Priority)
VALUES 
    ('Global API Limit', 'Global rate limit for all API endpoints', 4, 1000, 2, 3, 100, 1, 1),
    ('Login Endpoint Limit', 'Rate limit for authentication endpoints', 1, 5, 2, 2, NULL, 1, 10),
    ('User API Limit', 'Per user API rate limit', 2, 100, 2, 3, 50, 1, 5),
    ('IP Rate Limit', 'Per IP rate limit', 3, 200, 2, 3, 25, 1, 3);

-- Insert default rate limit configurations
INSERT INTO RateLimitConfigurations (Name, Description, Type, RequestLimit, Period, Action, ThrottleDelayMs, IsActive, Priority, EnableLogging, EnableAlerting, AlertThreshold, AlertRecipients)
VALUES 
    ('Default API Configuration', 'Default rate limiting configuration for all API endpoints', 4, 1000, 2, 3, 100, 1, 1, 1, 0, NULL, NULL),
    ('Authentication Endpoints', 'Rate limiting for authentication endpoints', 1, 10, 2, 2, NULL, 1, 10, 1, 1, 5, 'admin@getir.com'),
    ('Premium User Limits', 'Higher limits for premium users', 2, 500, 2, 3, 50, 1, 5, 1, 0, NULL, NULL),
    ('Free User Limits', 'Standard limits for free users', 2, 100, 2, 3, 100, 1, 3, 1, 0, NULL, NULL),
    ('Admin User Limits', 'Higher limits for admin users', 2, 2000, 2, 3, 25, 1, 8, 1, 0, NULL, NULL);

-- Insert sample rate limit logs
INSERT INTO RateLimitLogs (Endpoint, HttpMethod, UserId, UserName, IpAddress, Type, Action, RequestCount, RequestLimit, Period, IsLimitExceeded, Reason, RequestTime)
VALUES 
    ('/api/auth/login', 'POST', 'user123', 'testuser', '192.168.1.1', 1, 2, 6, 5, 2, 1, 'Rate limit exceeded for login endpoint', GETUTCDATE() - 0.01),
    ('/api/orders', 'GET', 'user456', 'anotheruser', '192.168.1.2', 2, 3, 101, 100, 2, 1, 'User rate limit exceeded', GETUTCDATE() - 0.02),
    ('/api/products', 'GET', NULL, NULL, '10.0.0.1', 3, 3, 201, 200, 2, 1, 'IP rate limit exceeded', GETUTCDATE() - 0.03),
    ('/api/users', 'GET', 'user789', 'premiumuser', '192.168.1.3', 2, 1, 50, 500, 2, 0, 'Request allowed', GETUTCDATE() - 0.04);

-- Insert sample rate limit violations
INSERT INTO RateLimitViolations (Endpoint, HttpMethod, UserId, UserName, IpAddress, Type, Action, RequestCount, RequestLimit, Period, ViolationReason, ViolationTime, IsResolved, Severity, RequiresInvestigation)
VALUES 
    ('/api/auth/login', 'POST', 'user123', 'testuser', '192.168.1.1', 1, 2, 6, 5, 2, 'Multiple failed login attempts', GETUTCDATE() - 0.01, 0, 3, 1),
    ('/api/orders', 'GET', 'user456', 'anotheruser', '192.168.1.2', 2, 3, 101, 100, 2, 'User exceeded rate limit', GETUTCDATE() - 0.02, 0, 2, 0),
    ('/api/products', 'GET', NULL, NULL, '10.0.0.1', 3, 3, 201, 200, 2, 'IP exceeded rate limit', GETUTCDATE() - 0.03, 1, 2, 0),
    ('/api/admin/users', 'DELETE', 'admin123', 'adminuser', '192.168.1.4', 2, 2, 11, 10, 2, 'Admin user exceeded rate limit', GETUTCDATE() - 0.05, 0, 4, 1);

PRINT 'Rate limiting system migration completed successfully!';
PRINT 'Created tables: RateLimitRules, RateLimitLogs, RateLimitViolations, RateLimitConfigurations';
PRINT 'Inserted sample data for rate limiting rules and configurations';
PRINT 'Total rate limit rules inserted: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));
