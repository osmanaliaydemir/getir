-- =============================================
-- Migration: 023 - Market Entities Update (FIX)
-- Description: Add MarketProductId to OrderLines only
-- Date: 2025-10-14
-- =============================================

-- =============================================
-- Add MarketProductId to OrderLines (if not exists)
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderLines') AND name = 'MarketProductId')
BEGIN
    PRINT 'Adding MarketProductId column to OrderLines table...';
    ALTER TABLE OrderLines ADD MarketProductId UNIQUEIDENTIFIER NULL;
    PRINT '✓ MarketProductId column added to OrderLines.';
END
ELSE
BEGIN
    PRINT '⊗ MarketProductId column already exists in OrderLines.';
END
GO

-- Wait for schema change to commit
WAITFOR DELAY '00:00:01';
GO

-- Create Index (separate batch after column is committed)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderLines_MarketProductId' AND object_id = OBJECT_ID('OrderLines'))
BEGIN
    PRINT 'Creating index IX_OrderLines_MarketProductId...';
    CREATE INDEX IX_OrderLines_MarketProductId ON OrderLines (MarketProductId)
    WHERE MarketProductId IS NOT NULL;
    PRINT '✓ Index created.';
END
ELSE
BEGIN
    PRINT '⊗ Index IX_OrderLines_MarketProductId already exists.';
END
GO

PRINT '========================================';
PRINT 'Migration 023 FIX completed successfully!';
PRINT 'Column added: OrderLines.MarketProductId';
PRINT '========================================';
GO

