-- =============================================
-- Users and Roles Seed
-- Creates Admin, MerchantOwner, and Customers with PBKDF2 SHA256 hashes
-- Password for all demo users: Passw0rd!
-- =============================================

PRINT 'Inserting users and roles...';

-- Helper: Insert user if not exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@getir.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES ('11111111-1111-1111-1111-111111111111', 'admin@getir.com', 'AQAAAADemoHashPlaceholderDoNotUse==', 'Sistem', 'Admin', '05550000000', 4, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = '33333333-3333-3333-3333-333333333333')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES ('33333333-3333-3333-3333-333333333333', 'owner@getir.com', 'AQAAAADemoHashPlaceholderDoNotUse==', 'Restoran', 'Sahibi', '05553333333', 2, 1, GETUTCDATE());
END

-- Customers
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'musteri1@getir.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES ('22222222-2222-2222-2222-222222222221', 'musteri1@getir.com', 'AQAAAADemoHashPlaceholderDoNotUse==', 'Ahmet', 'Yılmaz', '05551111111', 1, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'musteri2@getir.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES ('22222222-2222-2222-2222-222222222222', 'musteri2@getir.com', 'AQAAAADemoHashPlaceholderDoNotUse==', 'Ayşe', 'Demir', '05552222222', 1, 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'musteri3@getir.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsActive, CreatedAt)
    VALUES ('22222222-2222-2222-2222-222222222223', 'musteri3@getir.com', 'AQAAAADemoHashPlaceholderDoNotUse==', 'Mehmet', 'Kaya', '05553333334', 1, 1, GETUTCDATE());
END

PRINT '⚠️  NOTE: Replace demo hash with real PBKDF2 hash if login required.';
PRINT '✅ Users inserted.';


