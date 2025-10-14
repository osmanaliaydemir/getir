-- =============================================
-- Migration: 022 - OrderLines Variant Support
-- Description: Add ProductVariantId and VariantName columns to OrderLines table
-- Date: 2025-10-14
-- =============================================

USE GetirDB;
GO

PRINT 'Starting Migration 022: OrderLines Variant Support';
GO

-- =============================================
-- 1. Add New Columns to OrderLines
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderLines') AND name = 'ProductVariantId')
BEGIN
    PRINT 'Adding ProductVariantId column to OrderLines table...';
    ALTER TABLE OrderLines
    ADD ProductVariantId UNIQUEIDENTIFIER NULL;
    PRINT '✓ ProductVariantId column added.';
END
ELSE
BEGIN
    PRINT '⊗ ProductVariantId column already exists.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderLines') AND name = 'VariantName')
BEGIN
    PRINT 'Adding VariantName column to OrderLines table...';
    ALTER TABLE OrderLines
    ADD VariantName NVARCHAR(200) NULL;
    PRINT '✓ VariantName column added.';
END
ELSE
BEGIN
    PRINT '⊗ VariantName column already exists.';
END
GO

-- =============================================
-- 2. Add Foreign Key Constraint (Optional)
-- =============================================

-- Note: FK to MarketProductVariants is commented out as it may not exist in all deployments
-- Uncomment if MarketProductVariants table exists and you want to enforce referential integrity

/*
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrderLines_MarketProductVariants')
BEGIN
    PRINT 'Adding foreign key constraint FK_OrderLines_MarketProductVariants...';
    ALTER TABLE OrderLines
    ADD CONSTRAINT FK_OrderLines_MarketProductVariants 
    FOREIGN KEY (ProductVariantId) REFERENCES MarketProductVariants(Id);
    PRINT '✓ Foreign key constraint added.';
END
ELSE
BEGIN
    PRINT '⊗ FK_OrderLines_MarketProductVariants already exists.';
END
GO
*/

-- =============================================
-- 3. Create Index for Performance
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderLines_ProductVariantId' AND object_id = OBJECT_ID('OrderLines'))
BEGIN
    PRINT 'Creating index IX_OrderLines_ProductVariantId...';
    CREATE INDEX IX_OrderLines_ProductVariantId ON OrderLines (ProductVariantId)
    WHERE ProductVariantId IS NOT NULL;
    PRINT '✓ Index created.';
END
ELSE
BEGIN
    PRINT '⊗ Index IX_OrderLines_ProductVariantId already exists.';
END
GO

-- =============================================
-- 4. Verify Changes
-- =============================================

PRINT 'Verifying schema changes...';
GO

SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('OrderLines')
AND c.name IN ('ProductVariantId', 'VariantName')
ORDER BY c.column_id;
GO

PRINT '✓ Migration 022 completed successfully!';
PRINT 'OrderLines table now supports product variants.';
GO

