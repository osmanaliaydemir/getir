-- =============================================
-- Internationalization System Migration
-- Migration: 016-internationalization-system.sql
-- Description: Creates tables for multi-language support
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
IF OBJECT_ID('UserLanguagePreferences', 'U') IS NOT NULL DROP TABLE UserLanguagePreferences;
IF OBJECT_ID('Translations', 'U') IS NOT NULL DROP TABLE Translations;
IF OBJECT_ID('Languages', 'U') IS NOT NULL DROP TABLE Languages;

-- =============================================
-- Languages Table
-- =============================================
CREATE TABLE Languages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Code INT NOT NULL, -- LanguageCode enum value
    Name NVARCHAR(100) NOT NULL,
    NativeName NVARCHAR(100) NOT NULL,
    CultureCode NVARCHAR(10) NOT NULL,
    IsRtl BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDefault BIT NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    FlagIcon NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL
);

-- Indexes for Languages
CREATE UNIQUE INDEX IX_Languages_Code ON Languages (Code);
CREATE UNIQUE INDEX IX_Languages_CultureCode ON Languages (CultureCode);
CREATE INDEX IX_Languages_IsActive ON Languages (IsActive);
CREATE INDEX IX_Languages_IsDefault ON Languages (IsDefault);

-- =============================================
-- Translations Table
-- =============================================
CREATE TABLE Translations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(200) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    LanguageCode INT NOT NULL, -- LanguageCode enum value
    Category NVARCHAR(100) NULL,
    Context NVARCHAR(200) NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL
);

-- Indexes for Translations
CREATE INDEX IX_Translations_Key ON Translations ([Key]);
CREATE INDEX IX_Translations_LanguageCode ON Translations (LanguageCode);
CREATE INDEX IX_Translations_Category ON Translations (Category);
CREATE INDEX IX_Translations_IsActive ON Translations (IsActive);
CREATE UNIQUE INDEX IX_Translations_Key_LanguageCode ON Translations ([Key], LanguageCode);
CREATE INDEX IX_Translations_Category_LanguageCode ON Translations (Category, LanguageCode);

-- =============================================
-- UserLanguagePreferences Table
-- =============================================
CREATE TABLE UserLanguagePreferences (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    LanguageId UNIQUEIDENTIFIER NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

-- Indexes for UserLanguagePreferences
CREATE INDEX IX_UserLanguagePreferences_UserId ON UserLanguagePreferences (UserId);
CREATE INDEX IX_UserLanguagePreferences_LanguageId ON UserLanguagePreferences (LanguageId);
CREATE INDEX IX_UserLanguagePreferences_IsPrimary ON UserLanguagePreferences (IsPrimary);
CREATE INDEX IX_UserLanguagePreferences_IsActive ON UserLanguagePreferences (IsActive);
CREATE INDEX IX_UserLanguagePreferences_UserId_IsPrimary ON UserLanguagePreferences (UserId, IsPrimary);
CREATE UNIQUE INDEX IX_UserLanguagePreferences_UserId_LanguageId ON UserLanguagePreferences (UserId, LanguageId);

-- =============================================
-- Foreign Key Constraints
-- =============================================

-- Languages foreign keys
ALTER TABLE Languages
ADD CONSTRAINT FK_Languages_CreatedBy_Users
FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

ALTER TABLE Languages
ADD CONSTRAINT FK_Languages_UpdatedBy_Users
FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

-- Translations foreign keys
ALTER TABLE Translations
ADD CONSTRAINT FK_Translations_LanguageCode_Languages
FOREIGN KEY (LanguageCode) REFERENCES Languages(Code) ON DELETE CASCADE;

ALTER TABLE Translations
ADD CONSTRAINT FK_Translations_CreatedBy_Users
FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

ALTER TABLE Translations
ADD CONSTRAINT FK_Translations_UpdatedBy_Users
FOREIGN KEY (UpdatedBy) REFERENCES Users(Id) ON DELETE NO ACTION;

-- UserLanguagePreferences foreign keys
ALTER TABLE UserLanguagePreferences
ADD CONSTRAINT FK_UserLanguagePreferences_UserId_Users
FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE;

ALTER TABLE UserLanguagePreferences
ADD CONSTRAINT FK_UserLanguagePreferences_LanguageId_Languages
FOREIGN KEY (LanguageId) REFERENCES Languages(Id) ON DELETE CASCADE;

-- =============================================
-- Sample Data
-- =============================================

-- Insert default languages
INSERT INTO Languages (Code, Name, NativeName, CultureCode, IsRtl, IsActive, IsDefault, SortOrder, FlagIcon)
VALUES 
    (1, 'Turkish', 'Türkçe', 'tr-TR', 0, 1, 1, 1, '🇹🇷'),
    (2, 'English', 'English', 'en-US', 0, 1, 0, 2, '🇺🇸'),
    (3, 'Arabic', 'العربية', 'ar-SA', 1, 1, 0, 3, '🇸🇦');

-- Insert sample translations
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description, IsActive)
VALUES 
    -- Turkish translations
    ('welcome', 'Hoş Geldiniz', 1, 'UI', 'Login', 'Welcome message', 1),
    ('goodbye', 'Hoşça Kalın', 1, 'UI', 'Logout', 'Goodbye message', 1),
    ('login', 'Giriş Yap', 1, 'UI', 'Auth', 'Login button', 1),
    ('logout', 'Çıkış Yap', 1, 'UI', 'Auth', 'Logout button', 1),
    ('email', 'E-posta', 1, 'UI', 'Form', 'Email field label', 1),
    ('password', 'Şifre', 1, 'UI', 'Form', 'Password field label', 1),
    ('submit', 'Gönder', 1, 'UI', 'Form', 'Submit button', 1),
    ('cancel', 'İptal', 1, 'UI', 'Form', 'Cancel button', 1),
    ('save', 'Kaydet', 1, 'UI', 'Form', 'Save button', 1),
    ('delete', 'Sil', 1, 'UI', 'Action', 'Delete button', 1),
    ('edit', 'Düzenle', 1, 'UI', 'Action', 'Edit button', 1),
    ('view', 'Görüntüle', 1, 'UI', 'Action', 'View button', 1),
    ('search', 'Ara', 1, 'UI', 'Search', 'Search button', 1),
    ('filter', 'Filtrele', 1, 'UI', 'Filter', 'Filter button', 1),
    ('sort', 'Sırala', 1, 'UI', 'Sort', 'Sort button', 1),
    ('loading', 'Yükleniyor...', 1, 'UI', 'Status', 'Loading message', 1),
    ('error', 'Hata', 1, 'UI', 'Status', 'Error message', 1),
    ('success', 'Başarılı', 1, 'UI', 'Status', 'Success message', 1),
    ('warning', 'Uyarı', 1, 'UI', 'Status', 'Warning message', 1),
    ('info', 'Bilgi', 1, 'UI', 'Status', 'Info message', 1),
    
    -- English translations
    ('welcome', 'Welcome', 2, 'UI', 'Login', 'Welcome message', 1),
    ('goodbye', 'Goodbye', 2, 'UI', 'Logout', 'Goodbye message', 1),
    ('login', 'Login', 2, 'UI', 'Auth', 'Login button', 1),
    ('logout', 'Logout', 2, 'UI', 'Auth', 'Logout button', 1),
    ('email', 'Email', 2, 'UI', 'Form', 'Email field label', 1),
    ('password', 'Password', 2, 'UI', 'Form', 'Password field label', 1),
    ('submit', 'Submit', 2, 'UI', 'Form', 'Submit button', 1),
    ('cancel', 'Cancel', 2, 'UI', 'Form', 'Cancel button', 1),
    ('save', 'Save', 2, 'UI', 'Form', 'Save button', 1),
    ('delete', 'Delete', 2, 'UI', 'Action', 'Delete button', 1),
    ('edit', 'Edit', 2, 'UI', 'Action', 'Edit button', 1),
    ('view', 'View', 2, 'UI', 'Action', 'View button', 1),
    ('search', 'Search', 2, 'UI', 'Search', 'Search button', 1),
    ('filter', 'Filter', 2, 'UI', 'Filter', 'Filter button', 1),
    ('sort', 'Sort', 2, 'UI', 'Sort', 'Sort button', 1),
    ('loading', 'Loading...', 2, 'UI', 'Status', 'Loading message', 1),
    ('error', 'Error', 2, 'UI', 'Status', 'Error message', 1),
    ('success', 'Success', 2, 'UI', 'Status', 'Success message', 1),
    ('warning', 'Warning', 2, 'UI', 'Status', 'Warning message', 1),
    ('info', 'Info', 2, 'UI', 'Status', 'Info message', 1),
    
    -- Arabic translations
    ('welcome', 'أهلاً وسهلاً', 3, 'UI', 'Login', 'Welcome message', 1),
    ('goodbye', 'وداعاً', 3, 'UI', 'Logout', 'Goodbye message', 1),
    ('login', 'تسجيل الدخول', 3, 'UI', 'Auth', 'Login button', 1),
    ('logout', 'تسجيل الخروج', 3, 'UI', 'Auth', 'Logout button', 1),
    ('email', 'البريد الإلكتروني', 3, 'UI', 'Form', 'Email field label', 1),
    ('password', 'كلمة المرور', 3, 'UI', 'Form', 'Password field label', 1),
    ('submit', 'إرسال', 3, 'UI', 'Form', 'Submit button', 1),
    ('cancel', 'إلغاء', 3, 'UI', 'Form', 'Cancel button', 1),
    ('save', 'حفظ', 3, 'UI', 'Form', 'Save button', 1),
    ('delete', 'حذف', 3, 'UI', 'Action', 'Delete button', 1),
    ('edit', 'تعديل', 3, 'UI', 'Action', 'Edit button', 1),
    ('view', 'عرض', 3, 'UI', 'Action', 'View button', 1),
    ('search', 'بحث', 3, 'UI', 'Search', 'Search button', 1),
    ('filter', 'تصفية', 3, 'UI', 'Filter', 'Filter button', 1),
    ('sort', 'ترتيب', 3, 'UI', 'Sort', 'Sort button', 1),
    ('loading', 'جاري التحميل...', 3, 'UI', 'Status', 'Loading message', 1),
    ('error', 'خطأ', 3, 'UI', 'Status', 'Error message', 1),
    ('success', 'نجح', 3, 'UI', 'Status', 'Success message', 1),
    ('warning', 'تحذير', 3, 'UI', 'Status', 'Warning message', 1),
    ('info', 'معلومات', 3, 'UI', 'Status', 'Info message', 1);

-- Insert API translations
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description, IsActive)
VALUES 
    -- Turkish API translations
    ('api.user.not.found', 'Kullanıcı bulunamadı', 1, 'API', 'User', 'User not found error', 1),
    ('api.invalid.credentials', 'Geçersiz kimlik bilgileri', 1, 'API', 'Auth', 'Invalid credentials error', 1),
    ('api.access.denied', 'Erişim reddedildi', 1, 'API', 'Auth', 'Access denied error', 1),
    ('api.validation.error', 'Doğrulama hatası', 1, 'API', 'Validation', 'Validation error', 1),
    ('api.server.error', 'Sunucu hatası', 1, 'API', 'Error', 'Server error', 1),
    
    -- English API translations
    ('api.user.not.found', 'User not found', 2, 'API', 'User', 'User not found error', 1),
    ('api.invalid.credentials', 'Invalid credentials', 2, 'API', 'Auth', 'Invalid credentials error', 1),
    ('api.access.denied', 'Access denied', 2, 'API', 'Auth', 'Access denied error', 1),
    ('api.validation.error', 'Validation error', 2, 'API', 'Validation', 'Validation error', 1),
    ('api.server.error', 'Server error', 2, 'API', 'Error', 'Server error', 1),
    
    -- Arabic API translations
    ('api.user.not.found', 'المستخدم غير موجود', 3, 'API', 'User', 'User not found error', 1),
    ('api.invalid.credentials', 'بيانات اعتماد غير صحيحة', 3, 'API', 'Auth', 'Invalid credentials error', 1),
    ('api.access.denied', 'تم رفض الوصول', 3, 'API', 'Auth', 'Access denied error', 1),
    ('api.validation.error', 'خطأ في التحقق', 3, 'API', 'Validation', 'Validation error', 1),
    ('api.server.error', 'خطأ في الخادم', 3, 'API', 'Error', 'Server error', 1);

-- Insert Email translations
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description, IsActive)
VALUES 
    -- Turkish Email translations
    ('email.welcome.subject', 'Hoş Geldiniz!', 1, 'Email', 'Welcome', 'Welcome email subject', 1),
    ('email.welcome.body', 'Getir uygulamasına hoş geldiniz!', 1, 'Email', 'Welcome', 'Welcome email body', 1),
    ('email.password.reset.subject', 'Şifre Sıfırlama', 1, 'Email', 'PasswordReset', 'Password reset email subject', 1),
    ('email.password.reset.body', 'Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın.', 1, 'Email', 'PasswordReset', 'Password reset email body', 1),
    
    -- English Email translations
    ('email.welcome.subject', 'Welcome!', 2, 'Email', 'Welcome', 'Welcome email subject', 1),
    ('email.welcome.body', 'Welcome to Getir app!', 2, 'Email', 'Welcome', 'Welcome email body', 1),
    ('email.password.reset.subject', 'Password Reset', 2, 'Email', 'PasswordReset', 'Password reset email subject', 1),
    ('email.password.reset.body', 'Click the link below to reset your password.', 2, 'Email', 'PasswordReset', 'Password reset email body', 1),
    
    -- Arabic Email translations
    ('email.welcome.subject', 'أهلاً وسهلاً!', 3, 'Email', 'Welcome', 'Welcome email subject', 1),
    ('email.welcome.body', 'أهلاً وسهلاً في تطبيق جتير!', 3, 'Email', 'Welcome', 'Welcome email body', 1),
    ('email.password.reset.subject', 'إعادة تعيين كلمة المرور', 3, 'Email', 'PasswordReset', 'Password reset email subject', 1),
    ('email.password.reset.body', 'انقر على الرابط أدناه لإعادة تعيين كلمة المرور.', 3, 'Email', 'PasswordReset', 'Password reset email body', 1);

-- Insert SMS translations
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description, IsActive)
VALUES 
    -- Turkish SMS translations
    ('sms.verification.code', 'Doğrulama kodunuz: {0}', 1, 'SMS', 'Verification', 'SMS verification code', 1),
    ('sms.order.confirmation', 'Siparişiniz alındı. Sipariş numarası: {0}', 1, 'SMS', 'Order', 'Order confirmation SMS', 1),
    ('sms.delivery.update', 'Siparişiniz yolda. Tahmini teslimat: {0}', 1, 'SMS', 'Delivery', 'Delivery update SMS', 1),
    
    -- English SMS translations
    ('sms.verification.code', 'Your verification code: {0}', 2, 'SMS', 'Verification', 'SMS verification code', 1),
    ('sms.order.confirmation', 'Your order has been received. Order number: {0}', 2, 'SMS', 'Order', 'Order confirmation SMS', 1),
    ('sms.delivery.update', 'Your order is on the way. Estimated delivery: {0}', 2, 'SMS', 'Delivery', 'Delivery update SMS', 1),
    
    -- Arabic SMS translations
    ('sms.verification.code', 'رمز التحقق الخاص بك: {0}', 3, 'SMS', 'Verification', 'SMS verification code', 1),
    ('sms.order.confirmation', 'تم استلام طلبك. رقم الطلب: {0}', 3, 'SMS', 'Order', 'Order confirmation SMS', 1),
    ('sms.delivery.update', 'طلبك في الطريق. التوصيل المتوقع: {0}', 3, 'SMS', 'Delivery', 'Delivery update SMS', 1);

-- Insert Notification translations
INSERT INTO Translations ([Key], Value, LanguageCode, Category, Context, Description, IsActive)
VALUES 
    -- Turkish Notification translations
    ('notification.new.order', 'Yeni sipariş geldi!', 1, 'Notification', 'Order', 'New order notification', 1),
    ('notification.order.ready', 'Siparişiniz hazır!', 1, 'Notification', 'Order', 'Order ready notification', 1),
    ('notification.promotion', 'Yeni kampanya! {0} indirim', 1, 'Notification', 'Promotion', 'Promotion notification', 1),
    
    -- English Notification translations
    ('notification.new.order', 'New order received!', 2, 'Notification', 'Order', 'New order notification', 1),
    ('notification.order.ready', 'Your order is ready!', 2, 'Notification', 'Order', 'Order ready notification', 1),
    ('notification.promotion', 'New promotion! {0} discount', 2, 'Notification', 'Promotion', 'Promotion notification', 1),
    
    -- Arabic Notification translations
    ('notification.new.order', 'طلب جديد!', 3, 'Notification', 'Order', 'New order notification', 1),
    ('notification.order.ready', 'طلبك جاهز!', 3, 'Notification', 'Order', 'Order ready notification', 1),
    ('notification.promotion', 'عرض جديد! خصم {0}', 3, 'Notification', 'Promotion', 'Promotion notification', 1);

PRINT 'Internationalization system migration completed successfully!';
PRINT 'Created tables: Languages, Translations, UserLanguagePreferences';
PRINT 'Inserted sample data for Turkish, English, and Arabic languages';
PRINT 'Total translations inserted: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));
