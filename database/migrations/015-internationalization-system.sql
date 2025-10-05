-- =============================================
-- Internationalization System Migration
-- Migration: 015-internationalization-system.sql
-- Description: Adds comprehensive internationalization tables
-- Created: 2025-01-04
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

-- Drop tables if they exist to allow re-running the script during development
IF OBJECT_ID('UserLanguagePreferences', 'U') IS NOT NULL DROP TABLE UserLanguagePreferences;
IF OBJECT_ID('Translations', 'U') IS NOT NULL DROP TABLE Translations;
IF OBJECT_ID('Languages', 'U') IS NOT NULL DROP TABLE Languages;

-- LANGUAGES TABLE
CREATE TABLE Languages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code INT NOT NULL, -- 1=Turkish, 2=English, 3=Arabic
    Name NVARCHAR(100) NOT NULL,
    NativeName NVARCHAR(100) NOT NULL,
    CultureCode NVARCHAR(10) NOT NULL, -- tr-TR, en-US, ar-SA
    IsRtl BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDefault BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    FlagIcon NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Languages_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Languages_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- TRANSLATIONS TABLE
CREATE TABLE Translations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(200) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    LanguageCode INT NOT NULL, -- 1=Turkish, 2=English, 3=Arabic
    Category NVARCHAR(100) NULL, -- UI, API, Email, SMS, etc.
    Context NVARCHAR(200) NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Translations_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Translations_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION
);

-- USER LANGUAGE PREFERENCES TABLE
CREATE TABLE UserLanguagePreferences (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    LanguageId UNIQUEIDENTIFIER NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_UserLanguagePreferences_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserLanguagePreferences_Languages FOREIGN KEY (LanguageId) REFERENCES Languages(Id) ON DELETE CASCADE
);

-- INDEXES FOR PERFORMANCE
CREATE INDEX IX_Languages_Code ON Languages (Code);
CREATE INDEX IX_Languages_IsActive ON Languages (IsActive);
CREATE INDEX IX_Languages_IsDefault ON Languages (IsDefault);
CREATE INDEX IX_Languages_SortOrder ON Languages (SortOrder);

CREATE INDEX IX_Translations_Key ON Translations ([Key]);
CREATE INDEX IX_Translations_LanguageCode ON Translations (LanguageCode);
CREATE INDEX IX_Translations_Category ON Translations (Category);
CREATE INDEX IX_Translations_IsActive ON Translations (IsActive);
CREATE INDEX IX_Translations_Key_LanguageCode ON Translations ([Key], LanguageCode);
CREATE INDEX IX_Translations_Category_LanguageCode ON Translations (Category, LanguageCode);

CREATE INDEX IX_UserLanguagePreferences_UserId ON UserLanguagePreferences (UserId);
CREATE INDEX IX_UserLanguagePreferences_LanguageId ON UserLanguagePreferences (LanguageId);
CREATE INDEX IX_UserLanguagePreferences_IsPrimary ON UserLanguagePreferences (IsPrimary);
CREATE INDEX IX_UserLanguagePreferences_IsActive ON UserLanguagePreferences (IsActive);
CREATE INDEX IX_UserLanguagePreferences_UserId_IsPrimary ON UserLanguagePreferences (UserId, IsPrimary);

-- UNIQUE CONSTRAINTS
ALTER TABLE Languages ADD CONSTRAINT UQ_Languages_Code UNIQUE (Code);
ALTER TABLE Languages ADD CONSTRAINT UQ_Languages_CultureCode UNIQUE (CultureCode);
ALTER TABLE Translations ADD CONSTRAINT UQ_Translations_Key_LanguageCode UNIQUE ([Key], LanguageCode);
ALTER TABLE UserLanguagePreferences ADD CONSTRAINT UQ_UserLanguagePreferences_UserId_LanguageId UNIQUE (UserId, LanguageId);

-- CHECK CONSTRAINTS
ALTER TABLE Languages ADD CONSTRAINT CK_Languages_Code CHECK (Code IN (1, 2, 3)); -- Turkish, English, Arabic
ALTER TABLE Translations ADD CONSTRAINT CK_Translations_LanguageCode CHECK (LanguageCode IN (1, 2, 3)); -- Turkish, English, Arabic

PRINT 'Internationalization System migration completed successfully!';
PRINT 'Created tables: Languages, Translations, UserLanguagePreferences';
PRINT 'Created indexes for performance optimization';
PRINT 'Inserted sample data for testing';

-- =============================================
-- SAMPLE DATA FOR TESTING
-- =============================================

-- Insert sample languages
INSERT INTO Languages (Code, Name, NativeName, CultureCode, IsRtl, IsActive, IsDefault, SortOrder, FlagIcon) VALUES
(1, 'Turkish', 'Türkçe', 'tr-TR', 0, 1, 1, 1, '🇹🇷'),
(2, 'English', 'English', 'en-US', 0, 1, 0, 2, '🇺🇸'),
(3, 'Arabic', 'العربية', 'ar-SA', 1, 1, 0, 3, '🇸🇦');

-- Insert sample translations for Turkish
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description) VALUES
-- UI Translations
('welcome', 'Hoş Geldiniz', 1, 'UI', 'HomePage', 'Welcome message on homepage'),
('login', 'Giriş Yap', 1, 'UI', 'Auth', 'Login button text'),
('register', 'Kayıt Ol', 1, 'UI', 'Auth', 'Register button text'),
('logout', 'Çıkış Yap', 1, 'UI', 'Auth', 'Logout button text'),
('home', 'Ana Sayfa', 1, 'UI', 'Navigation', 'Home navigation item'),
('profile', 'Profil', 1, 'UI', 'Navigation', 'Profile navigation item'),
('settings', 'Ayarlar', 1, 'UI', 'Navigation', 'Settings navigation item'),
('search', 'Ara', 1, 'UI', 'Search', 'Search button text'),
('cancel', 'İptal', 1, 'UI', 'Common', 'Cancel button text'),
('save', 'Kaydet', 1, 'UI', 'Common', 'Save button text'),
('delete', 'Sil', 1, 'UI', 'Common', 'Delete button text'),
('edit', 'Düzenle', 1, 'UI', 'Common', 'Edit button text'),
('add', 'Ekle', 1, 'UI', 'Common', 'Add button text'),
('loading', 'Yükleniyor...', 1, 'UI', 'Common', 'Loading indicator text'),
('error', 'Hata', 1, 'UI', 'Common', 'Error message title'),

-- API Translations
('api.success', 'İşlem başarılı', 1, 'API', 'Response', 'Success response message'),
('api.error', 'İşlem başarısız', 1, 'API', 'Response', 'Error response message'),
('api.not_found', 'Kayıt bulunamadı', 1, 'API', 'Response', 'Not found response message'),
('api.unauthorized', 'Yetkisiz erişim', 1, 'API', 'Response', 'Unauthorized response message'),
('api.forbidden', 'Erişim reddedildi', 1, 'API', 'Response', 'Forbidden response message'),
('api.validation_error', 'Doğrulama hatası', 1, 'API', 'Response', 'Validation error response message'),

-- Email Translations
('email.welcome_subject', 'Getir''e Hoş Geldiniz', 1, 'Email', 'Welcome', 'Welcome email subject'),
('email.welcome_body', 'Getir ailesine hoş geldiniz!', 1, 'Email', 'Welcome', 'Welcome email body'),
('email.order_confirmation_subject', 'Sipariş Onayı', 1, 'Email', 'Order', 'Order confirmation email subject'),
('email.order_confirmation_body', 'Siparişiniz onaylandı.', 1, 'Email', 'Order', 'Order confirmation email body'),

-- SMS Translations
('sms.otp', 'Getir doğrulama kodunuz: {0}', 1, 'SMS', 'OTP', 'OTP SMS message'),
('sms.order_ready', 'Siparişiniz hazır!', 1, 'SMS', 'Order', 'Order ready SMS message'),

-- Notification Translations
('notification.new_order', 'Yeni sipariş', 1, 'Notification', 'Order', 'New order notification title'),
('notification.order_delivered', 'Sipariş teslim edildi', 1, 'Notification', 'Order', 'Order delivered notification title');

-- Insert sample translations for English
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description) VALUES
-- UI Translations
('welcome', 'Welcome', 2, 'UI', 'HomePage', 'Welcome message on homepage'),
('login', 'Login', 2, 'UI', 'Auth', 'Login button text'),
('register', 'Register', 2, 'UI', 'Auth', 'Register button text'),
('logout', 'Logout', 2, 'UI', 'Auth', 'Logout button text'),
('home', 'Home', 2, 'UI', 'Navigation', 'Home navigation item'),
('profile', 'Profile', 2, 'UI', 'Navigation', 'Profile navigation item'),
('settings', 'Settings', 2, 'UI', 'Navigation', 'Settings navigation item'),
('search', 'Search', 2, 'UI', 'Search', 'Search button text'),
('cancel', 'Cancel', 2, 'UI', 'Common', 'Cancel button text'),
('save', 'Save', 2, 'UI', 'Common', 'Save button text'),
('delete', 'Delete', 2, 'UI', 'Common', 'Delete button text'),
('edit', 'Edit', 2, 'UI', 'Common', 'Edit button text'),
('add', 'Add', 2, 'UI', 'Common', 'Add button text'),
('loading', 'Loading...', 2, 'UI', 'Common', 'Loading indicator text'),
('error', 'Error', 2, 'UI', 'Common', 'Error message title'),

-- API Translations
('api.success', 'Operation successful', 2, 'API', 'Response', 'Success response message'),
('api.error', 'Operation failed', 2, 'API', 'Response', 'Error response message'),
('api.not_found', 'Record not found', 2, 'API', 'Response', 'Not found response message'),
('api.unauthorized', 'Unauthorized access', 2, 'API', 'Response', 'Unauthorized response message'),
('api.forbidden', 'Access denied', 2, 'API', 'Response', 'Forbidden response message'),
('api.validation_error', 'Validation error', 2, 'API', 'Response', 'Validation error response message'),

-- Email Translations
('email.welcome_subject', 'Welcome to Getir', 2, 'Email', 'Welcome', 'Welcome email subject'),
('email.welcome_body', 'Welcome to the Getir family!', 2, 'Email', 'Welcome', 'Welcome email body'),
('email.order_confirmation_subject', 'Order Confirmation', 2, 'Email', 'Order', 'Order confirmation email subject'),
('email.order_confirmation_body', 'Your order has been confirmed.', 2, 'Email', 'Order', 'Order confirmation email body'),

-- SMS Translations
('sms.otp', 'Your Getir verification code: {0}', 2, 'SMS', 'OTP', 'OTP SMS message'),
('sms.order_ready', 'Your order is ready!', 2, 'SMS', 'Order', 'Order ready SMS message'),

-- Notification Translations
('notification.new_order', 'New order', 2, 'Notification', 'Order', 'New order notification title'),
('notification.order_delivered', 'Order delivered', 2, 'Notification', 'Order', 'Order delivered notification title');

-- Insert sample translations for Arabic
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description) VALUES
-- UI Translations
('welcome', 'أهلاً وسهلاً', 3, 'UI', 'HomePage', 'Welcome message on homepage'),
('login', 'تسجيل الدخول', 3, 'UI', 'Auth', 'Login button text'),
('register', 'إنشاء حساب', 3, 'UI', 'Auth', 'Register button text'),
('logout', 'تسجيل الخروج', 3, 'UI', 'Auth', 'Logout button text'),
('home', 'الرئيسية', 3, 'UI', 'Navigation', 'Home navigation item'),
('profile', 'الملف الشخصي', 3, 'UI', 'Navigation', 'Profile navigation item'),
('settings', 'الإعدادات', 3, 'UI', 'Navigation', 'Settings navigation item'),
('search', 'بحث', 3, 'UI', 'Search', 'Search button text'),
('cancel', 'إلغاء', 3, 'UI', 'Common', 'Cancel button text'),
('save', 'حفظ', 3, 'UI', 'Common', 'Save button text'),
('delete', 'حذف', 3, 'UI', 'Common', 'Delete button text'),
('edit', 'تعديل', 3, 'UI', 'Common', 'Edit button text'),
('add', 'إضافة', 3, 'UI', 'Common', 'Add button text'),
('loading', 'جاري التحميل...', 3, 'UI', 'Common', 'Loading indicator text'),
('error', 'خطأ', 3, 'UI', 'Common', 'Error message title'),

-- API Translations
('api.success', 'تمت العملية بنجاح', 3, 'API', 'Response', 'Success response message'),
('api.error', 'فشلت العملية', 3, 'API', 'Response', 'Error response message'),
('api.not_found', 'لم يتم العثور على السجل', 3, 'API', 'Response', 'Not found response message'),
('api.unauthorized', 'وصول غير مصرح به', 3, 'API', 'Response', 'Unauthorized response message'),
('api.forbidden', 'تم رفض الوصول', 3, 'API', 'Response', 'Forbidden response message'),
('api.validation_error', 'خطأ في التحقق', 3, 'API', 'Response', 'Validation error response message'),

-- Email Translations
('email.welcome_subject', 'مرحباً بك في جتير', 3, 'Email', 'Welcome', 'Welcome email subject'),
('email.welcome_body', 'مرحباً بك في عائلة جتير!', 3, 'Email', 'Welcome', 'Welcome email body'),
('email.order_confirmation_subject', 'تأكيد الطلب', 3, 'Email', 'Order', 'Order confirmation email subject'),
('email.order_confirmation_body', 'تم تأكيد طلبك.', 3, 'Email', 'Order', 'Order confirmation email body'),

-- SMS Translations
('sms.otp', 'رمز التحقق الخاص بك من جتير: {0}', 3, 'SMS', 'OTP', 'OTP SMS message'),
('sms.order_ready', 'طلبك جاهز!', 3, 'SMS', 'Order', 'Order ready SMS message'),

-- Notification Translations
('notification.new_order', 'طلب جديد', 3, 'Notification', 'Order', 'New order notification title'),
('notification.order_delivered', 'تم تسليم الطلب', 3, 'Notification', 'Order', 'Order delivered notification title');

-- Insert sample user language preferences
INSERT INTO UserLanguagePreferences (UserId, LanguageId, IsPrimary, IsActive)
SELECT TOP 1 
    Id, 
    (SELECT Id FROM Languages WHERE Code = 1), -- Turkish
    1, 
    1
FROM Users 
WHERE FirstName IS NOT NULL OR Email IS NOT NULL;

PRINT 'Sample data inserted successfully!';
PRINT 'Languages: Turkish (default), English, Arabic';
PRINT 'Translations: UI, API, Email, SMS, Notification categories';
PRINT 'User language preferences: Sample user set to Turkish';
