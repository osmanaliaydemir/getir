-- =============================================
-- Migration: 022 - OrderLines Variant Support (Remote)
-- Description: Add ProductVariantId and VariantName columns to OrderLines table
-- Date: 2025-10-14
-- =============================================

-- =============================================
-- 1. Add New Columns to OrderLines
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderLines') AND name = 'ProductVariantId')
BEGIN
    ALTER TABLE OrderLines ADD ProductVariantId UNIQUEIDENTIFIER NULL;
    PRINT 'ProductVariantId column added.';
END
ELSE
BEGIN
    PRINT 'ProductVariantId column already exists.';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('OrderLines') AND name = 'VariantName')
BEGIN
    ALTER TABLE OrderLines ADD VariantName NVARCHAR(200) NULL;
    PRINT 'VariantName column added.';
END
ELSE
BEGIN
    PRINT 'VariantName column already exists.';
END

-- =============================================
-- 2. Create Index for Performance
-- =============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderLines_ProductVariantId' AND object_id = OBJECT_ID('OrderLines'))
BEGIN
    CREATE INDEX IX_OrderLines_ProductVariantId ON OrderLines (ProductVariantId)
    WHERE ProductVariantId IS NOT NULL;
    PRINT 'Index created.';
END
ELSE
BEGIN
    PRINT 'Index IX_OrderLines_ProductVariantId already exists.';
END

-- =============================================
-- 3. Verify Changes
-- =============================================

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

PRINT 'Migration 022 completed successfully!';

