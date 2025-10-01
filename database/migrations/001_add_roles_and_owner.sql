-- =============================================
-- Sprint 1: Role & Auth - Database Migration
-- Adds UserRole and Merchant OwnerId
-- =============================================

USE GetirDb;
GO

-- =============================================
-- 1. USERS: Add Role Column
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
BEGIN
    ALTER TABLE Users ADD Role INT NOT NULL DEFAULT 1; -- 1 = Customer (default)
    PRINT 'Added Role column to Users table';
END
ELSE
BEGIN
    PRINT 'Role column already exists in Users table';
END
GO

-- Role değerleri için check constraint ekle
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Users_Role')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT CK_Users_Role 
        CHECK (Role IN (1, 2, 3, 4)); -- Customer=1, MerchantOwner=2, Courier=3, Admin=4
    PRINT 'Added check constraint CK_Users_Role';
END
GO

-- Role column için index ekle (sık sık sorgulanacak)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Role')
BEGIN
    CREATE INDEX IX_Users_Role ON Users(Role);
    PRINT 'Added index IX_Users_Role';
END
GO

-- =============================================
-- 2. MERCHANTS: Add OwnerId Column
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Merchants') AND name = 'OwnerId')
BEGIN
    -- Geçici olarak NULL kabul ediyoruz (mevcut veriler için)
    ALTER TABLE Merchants ADD OwnerId UNIQUEIDENTIFIER NULL;
    PRINT 'Added OwnerId column to Merchants table';
    
    -- Foreign key constraint ekle
    ALTER TABLE Merchants ADD CONSTRAINT FK_Merchants_Owners 
        FOREIGN KEY (OwnerId) REFERENCES Users(Id);
    PRINT 'Added foreign key FK_Merchants_Owners';
    
    -- Index ekle
    CREATE INDEX IX_Merchants_OwnerId ON Merchants(OwnerId);
    PRINT 'Added index IX_Merchants_OwnerId';
END
ELSE
BEGIN
    PRINT 'OwnerId column already exists in Merchants table';
END
GO

-- =============================================
-- 3. DATA MIGRATION (Optional)
-- =============================================

-- Örnek: İlk Admin kullanıcı oluştur (password: Admin123!)
-- Password hash: PBKDF2 ile oluşturulmuş (gerçek uygulamada API üzerinden register edilmeli)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@getir.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt)
    VALUES (
        NEWID(),
        'admin@getir.com',
        'AQAAAAIAAYagAAAAEJ8xGPWVKxnH5ZL9vdRQPqC5MdU3xZq6YM5nW8rP7sT2VqU3hK9jL4mN1oP0qR3s', -- Admin123! (örnek hash)
        'Admin',
        'User',
        '+905551234567',
        4, -- Admin
        1, -- EmailVerified
        1, -- Active
        GETUTCDATE()
    );
    PRINT 'Created default admin user (email: admin@getir.com)';
END
GO

-- =============================================
-- 4. VERIFICATION QUERIES
-- =============================================

PRINT '================================';
PRINT 'Migration completed successfully!';
PRINT '================================';
PRINT '';
PRINT 'Verification:';
PRINT '1. Users with Role column:';
SELECT TOP 5 Id, Email, FirstName, LastName, Role, CreatedAt 
FROM Users 
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '2. Merchants with OwnerId column:';
SELECT TOP 5 Id, Name, OwnerId, IsActive 
FROM Merchants 
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '3. User Roles Distribution:';
SELECT 
    CASE Role
        WHEN 1 THEN 'Customer'
        WHEN 2 THEN 'MerchantOwner'
        WHEN 3 THEN 'Courier'
        WHEN 4 THEN 'Admin'
        ELSE 'Unknown'
    END AS RoleName,
    COUNT(*) AS UserCount
FROM Users
GROUP BY Role
ORDER BY Role;

GO

