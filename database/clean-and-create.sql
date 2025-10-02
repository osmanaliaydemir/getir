-- =============================================
-- Clean and Create Database
-- Mevcut database'i sil ve yeniden oluştur
-- =============================================

USE master;
GO

-- Mevcut database'i sil
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'GetirDb')
BEGIN
    ALTER DATABASE GetirDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GetirDb;
END
GO

-- Yeni database oluştur
CREATE DATABASE GetirDb;
GO

USE GetirDb;
GO

PRINT 'Database cleaned and created successfully!';
GO
