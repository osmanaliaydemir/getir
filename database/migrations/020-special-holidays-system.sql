-- =============================================
-- SPECIAL HOLIDAYS SYSTEM
-- Özel tatil günleri ve geçici kapanış/açılış yönetimi
-- =============================================

-- SpecialHolidays Tablosu
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpecialHolidays]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SpecialHolidays] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [MerchantId] UNIQUEIDENTIFIER NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [StartDate] DATETIME2 NOT NULL,
        [EndDate] DATETIME2 NOT NULL,
        [IsClosed] BIT NOT NULL DEFAULT 1,
        [SpecialOpenTime] TIME NULL,
        [SpecialCloseTime] TIME NULL,
        [IsRecurring] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        
        CONSTRAINT [FK_SpecialHolidays_Merchants] 
            FOREIGN KEY ([MerchantId]) 
            REFERENCES [dbo].[Merchants]([Id]) 
            ON DELETE CASCADE,
        
        CONSTRAINT [CK_SpecialHolidays_DateRange] 
            CHECK ([EndDate] >= [StartDate]),
        
        CONSTRAINT [CK_SpecialHolidays_SpecialTimes] 
            CHECK (
                ([IsClosed] = 1 AND [SpecialOpenTime] IS NULL AND [SpecialCloseTime] IS NULL)
                OR
                ([IsClosed] = 0 AND [SpecialOpenTime] IS NOT NULL AND [SpecialCloseTime] IS NOT NULL)
            )
    );

    PRINT 'SpecialHolidays tablosu oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'SpecialHolidays tablosu zaten mevcut.';
END
GO

-- İndeksler
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SpecialHolidays_MerchantId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SpecialHolidays_MerchantId]
        ON [dbo].[SpecialHolidays]([MerchantId])
        INCLUDE ([StartDate], [EndDate], [IsActive]);
    
    PRINT 'IX_SpecialHolidays_MerchantId indexi oluşturuldu.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SpecialHolidays_DateRange')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SpecialHolidays_DateRange]
        ON [dbo].[SpecialHolidays]([StartDate], [EndDate])
        INCLUDE ([MerchantId], [IsActive], [IsClosed]);
    
    PRINT 'IX_SpecialHolidays_DateRange indexi oluşturuldu.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SpecialHolidays_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_SpecialHolidays_IsActive]
        ON [dbo].[SpecialHolidays]([IsActive])
        INCLUDE ([MerchantId], [StartDate], [EndDate]);
    
    PRINT 'IX_SpecialHolidays_IsActive indexi oluşturuldu.';
END
GO

-- =============================================
-- SEED DATA - Örnek Özel Tatiller
-- =============================================

-- Test için örnek veriler (Migros için)
DECLARE @MigrosId UNIQUEIDENTIFIER;
SELECT TOP 1 @MigrosId = Id FROM Merchants WHERE Name = 'Migros' OR Name LIKE '%Migros%';

IF @MigrosId IS NOT NULL
BEGIN
    -- Yılbaşı tatili (her yıl tekrar eden)
    IF NOT EXISTS (SELECT 1 FROM SpecialHolidays WHERE MerchantId = @MigrosId AND Title = 'Yılbaşı Tatili')
    BEGIN
        INSERT INTO SpecialHolidays (Id, MerchantId, Title, Description, StartDate, EndDate, IsClosed, SpecialOpenTime, SpecialCloseTime, IsRecurring, IsActive, CreatedAt)
        VALUES (
            NEWID(),
            @MigrosId,
            'Yılbaşı Tatili',
            'Yılbaşı günü kapalı',
            DATEFROMPARTS(YEAR(GETUTCDATE()), 1, 1),
            DATEFROMPARTS(YEAR(GETUTCDATE()), 1, 1),
            1, -- Kapalı
            NULL,
            NULL,
            1, -- Her yıl tekrar ediyor
            1,
            GETUTCDATE()
        );
        PRINT 'Yılbaşı tatili eklendi.';
    END

    -- Ramazan Bayramı (örnek - 2025 tarihleri)
    IF NOT EXISTS (SELECT 1 FROM SpecialHolidays WHERE MerchantId = @MigrosId AND Title = 'Ramazan Bayramı 2025')
    BEGIN
        INSERT INTO SpecialHolidays (Id, MerchantId, Title, Description, StartDate, EndDate, IsClosed, SpecialOpenTime, SpecialCloseTime, IsRecurring, IsActive, CreatedAt)
        VALUES (
            NEWID(),
            @MigrosId,
            'Ramazan Bayramı 2025',
            'Ramazan Bayramı tatili - İlk gün kapalı',
            '2025-03-30',
            '2025-03-30',
            1, -- Kapalı
            NULL,
            NULL,
            0,
            1,
            GETUTCDATE()
        );
        PRINT 'Ramazan Bayramı 2025 eklendi.';
    END

    -- Özel çalışma saatleri örneği (Cumhuriyet Bayramı)
    IF NOT EXISTS (SELECT 1 FROM SpecialHolidays WHERE MerchantId = @MigrosId AND Title = 'Cumhuriyet Bayramı')
    BEGIN
        INSERT INTO SpecialHolidays (Id, MerchantId, Title, Description, StartDate, EndDate, IsClosed, SpecialOpenTime, SpecialCloseTime, IsRecurring, IsActive, CreatedAt)
        VALUES (
            NEWID(),
            @MigrosId,
            'Cumhuriyet Bayramı',
            '29 Ekim - Kısıtlı çalışma saatleri',
            DATEFROMPARTS(YEAR(GETUTCDATE()), 10, 29),
            DATEFROMPARTS(YEAR(GETUTCDATE()), 10, 29),
            0, -- Açık ama özel saatler
            '10:00', -- Özel açılış
            '18:00', -- Özel kapanış
            1, -- Her yıl tekrar ediyor
            1,
            GETUTCDATE()
        );
        PRINT 'Cumhuriyet Bayramı özel çalışma saatleri eklendi.';
    END
END
GO

-- =============================================
-- STORED PROCEDURE: Belirli tarihte merchant'ın durumunu kontrol et
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CheckMerchantAvailability]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[sp_CheckMerchantAvailability];
END
GO

CREATE PROCEDURE [dbo].[sp_CheckMerchantAvailability]
    @MerchantId UNIQUEIDENTIFIER,
    @CheckDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    -- Özel tatil kontrolü
    SELECT TOP 1
        sh.Id,
        sh.Title,
        sh.Description,
        sh.IsClosed,
        sh.SpecialOpenTime,
        sh.SpecialCloseTime,
        CASE 
            WHEN sh.IsClosed = 1 THEN 'Tatilde Kapalı'
            ELSE 'Özel Çalışma Saatleri'
        END AS [Status],
        CASE 
            WHEN sh.IsClosed = 1 THEN sh.Title + ' nedeniyle kapalıdır'
            ELSE sh.Title + ' - Özel çalışma saatleri: ' + 
                 CONVERT(VARCHAR(5), sh.SpecialOpenTime, 108) + ' - ' + 
                 CONVERT(VARCHAR(5), sh.SpecialCloseTime, 108)
        END AS [Message]
    FROM SpecialHolidays sh
    WHERE sh.MerchantId = @MerchantId
        AND sh.IsActive = 1
        AND sh.StartDate <= @CheckDate
        AND sh.EndDate >= @CheckDate
    ORDER BY sh.CreatedAt DESC;

    -- Eğer özel tatil yoksa normal çalışma saatlerini kontrol et
    IF @@ROWCOUNT = 0
    BEGIN
        DECLARE @DayOfWeek INT = DATEPART(WEEKDAY, @CheckDate) - 1; -- 0=Sunday, 6=Saturday
        
        SELECT TOP 1
            wh.OpenTime,
            wh.CloseTime,
            wh.IsClosed,
            CASE 
                WHEN wh.IsClosed = 1 THEN 'Kapalı'
                ELSE 'Açık'
            END AS [Status],
            CASE 
                WHEN wh.IsClosed = 1 THEN 'Bu gün kapalı'
                ELSE 'Normal çalışma saatleri: ' + 
                     CONVERT(VARCHAR(5), wh.OpenTime, 108) + ' - ' + 
                     CONVERT(VARCHAR(5), wh.CloseTime, 108)
            END AS [Message]
        FROM WorkingHours wh
        WHERE wh.MerchantId = @MerchantId
            AND wh.DayOfWeek = @DayOfWeek;
    END
END
GO

PRINT 'sp_CheckMerchantAvailability stored procedure oluşturuldu.';
GO

-- =============================================
-- ÖRNEK KULLANIM
-- =============================================

-- DECLARE @TestMerchantId UNIQUEIDENTIFIER;
-- SELECT TOP 1 @TestMerchantId = Id FROM Merchants;
-- 
-- EXEC sp_CheckMerchantAvailability 
--     @MerchantId = @TestMerchantId,
--     @CheckDate = '2025-01-01'; -- Yılbaşı kontrolü

PRINT 'Special Holidays System migration tamamlandı!';
GO

