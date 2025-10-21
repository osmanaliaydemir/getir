-- =============================================
-- SQL Optimizations: Stored Procedures for Analytics
-- Author: System
-- Date: 2025-10-21
-- Description: High-performance stored procedures using SQL GROUP BY
-- NOTE: These are OPTIONAL alternatives to C# LINQ queries
-- =============================================

USE [db29009]
GO

-- =============================================
-- 1. User Growth Data (SQL-optimized version)
-- =============================================
IF OBJECT_ID('sp_GetUserGrowthData', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetUserGrowthData;
GO

CREATE PROCEDURE sp_GetUserGrowthData
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate parameters
    IF @FromDate > @ToDate
    BEGIN
        RAISERROR('FromDate cannot be greater than ToDate', 16, 1);
        RETURN;
    END

    -- Get cumulative total before start date
    DECLARE @BaseCumulativeCount INT;
    SELECT @BaseCumulativeCount = COUNT(*)
    FROM Users
    WHERE CreatedAt < @FromDate;

    -- Generate daily aggregates with cumulative totals
    WITH DailyUsers AS (
        SELECT 
            CAST(CreatedAt AS DATE) AS Date,
            COUNT(*) AS NewUsers
        FROM Users
        WHERE CreatedAt >= @FromDate 
          AND CreatedAt <= DATEADD(DAY, 1, @ToDate)
        GROUP BY CAST(CreatedAt AS DATE)
    ),
    DateRange AS (
        SELECT CAST(@FromDate AS DATE) AS Date
        UNION ALL
        SELECT DATEADD(DAY, 1, Date)
        FROM DateRange
        WHERE Date < CAST(@ToDate AS DATE)
    ),
    DailyGrowth AS (
        SELECT 
            dr.Date,
            ISNULL(du.NewUsers, 0) AS NewUsers,
            ROW_NUMBER() OVER (ORDER BY dr.Date) AS RowNum
        FROM DateRange dr
        LEFT JOIN DailyUsers du ON dr.Date = du.Date
    )
    SELECT 
        Date,
        NewUsers,
        @BaseCumulativeCount + (
            SELECT SUM(NewUsers) 
            FROM DailyGrowth dg2 
            WHERE dg2.RowNum <= dg1.RowNum
        ) AS TotalUsers
    FROM DailyGrowth dg1
    ORDER BY Date
    OPTION (MAXRECURSION 0);
END
GO

-- =============================================
-- 2. Order Trend Data (SQL-optimized version)
-- =============================================
IF OBJECT_ID('sp_GetOrderTrendData', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetOrderTrendData;
GO

CREATE PROCEDURE sp_GetOrderTrendData
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate parameters
    IF @FromDate > @ToDate
    BEGIN
        RAISERROR('FromDate cannot be greater than ToDate', 16, 1);
        RETURN;
    END;

    -- Generate daily aggregates
    WITH DailyOrders AS (
        SELECT 
            CAST(CreatedAt AS DATE) AS Date,
            COUNT(*) AS OrderCount,
            SUM(Total) AS TotalValue
        FROM Orders
        WHERE CreatedAt >= @FromDate 
          AND CreatedAt <= DATEADD(DAY, 1, @ToDate)
        GROUP BY CAST(CreatedAt AS DATE)
    ),
    DateRange AS (
        SELECT CAST(@FromDate AS DATE) AS Date
        UNION ALL
        SELECT DATEADD(DAY, 1, Date)
        FROM DateRange
        WHERE Date < CAST(@ToDate AS DATE)
    )
    SELECT 
        dr.Date,
        ISNULL(do.OrderCount, 0) AS OrderCount,
        ISNULL(do.TotalValue, 0) AS TotalValue
    FROM DateRange dr
    LEFT JOIN DailyOrders do ON dr.Date = do.Date
    ORDER BY dr.Date
    OPTION (MAXRECURSION 0);
END
GO

-- =============================================
-- 3. Revenue Trend Data (SQL-optimized version)
-- =============================================
IF OBJECT_ID('sp_GetRevenueTrendData', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetRevenueTrendData;
GO

CREATE PROCEDURE sp_GetRevenueTrendData
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate parameters
    IF @FromDate > @ToDate
    BEGIN
        RAISERROR('FromDate cannot be greater than ToDate', 16, 1);
        RETURN;
    END;

    -- Only count delivered orders (actual revenue)
    WITH DailyRevenue AS (
        SELECT 
            CAST(CreatedAt AS DATE) AS Date,
            COUNT(*) AS OrderCount,
            SUM(Total) AS Revenue
        FROM Orders
        WHERE Status = 6 -- OrderStatus.Delivered
          AND CreatedAt >= @FromDate 
          AND CreatedAt <= DATEADD(DAY, 1, @ToDate)
        GROUP BY CAST(CreatedAt AS DATE)
    ),
    DateRange AS (
        SELECT CAST(@FromDate AS DATE) AS Date
        UNION ALL
        SELECT DATEADD(DAY, 1, Date)
        FROM DateRange
        WHERE Date < CAST(@ToDate AS DATE)
    )
    SELECT 
        dr.Date,
        ISNULL(drev.Revenue, 0) AS Revenue,
        ISNULL(drev.OrderCount, 0) AS OrderCount
    FROM DateRange dr
    LEFT JOIN DailyRevenue drev ON dr.Date = drev.Date
    ORDER BY dr.Date
    OPTION (MAXRECURSION 0);
END
GO

-- =============================================
-- 4. Grant Permissions (adjust as needed)
-- =============================================
-- GRANT EXECUTE ON sp_GetUserGrowthData TO [YourAppRole];
-- GRANT EXECUTE ON sp_GetOrderTrendData TO [YourAppRole];
-- GRANT EXECUTE ON sp_GetRevenueTrendData TO [YourAppRole];

PRINT 'Analytics Stored Procedures created successfully!'
PRINT ''
PRINT 'Usage Examples:'
PRINT '  EXEC sp_GetUserGrowthData @FromDate = ''2025-01-01'', @ToDate = ''2025-01-31'''
PRINT '  EXEC sp_GetOrderTrendData @FromDate = ''2025-01-01'', @ToDate = ''2025-01-31'''
PRINT '  EXEC sp_GetRevenueTrendData @FromDate = ''2025-01-01'', @ToDate = ''2025-01-31'''
GO

