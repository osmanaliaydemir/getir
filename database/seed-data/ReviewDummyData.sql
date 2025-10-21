USE [db29009]
GO

PRINT 'Starting Review Dummy Data Insertion...'
GO

-- First, let's get some existing merchants and users for reviews
DECLARE @MerchantId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Merchants WHERE IsActive = 1)
DECLARE @MerchantId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Merchants WHERE IsActive = 1 AND Id != @MerchantId1)
DECLARE @UserId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE IsActive = 1)
DECLARE @UserId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE IsActive = 1 AND Id != @UserId1)
DECLARE @UserId3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE IsActive = 1 AND Id NOT IN (@UserId1, @UserId2))
DECLARE @CourierId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Couriers WHERE IsActive = 1)
DECLARE @CourierId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Couriers WHERE IsActive = 1 AND Id != @CourierId1)
DECLARE @OrderId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Orders WHERE Status = 'Delivered')
DECLARE @OrderId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Orders WHERE Status = 'Delivered' AND Id != @OrderId1)
DECLARE @OrderId3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Orders WHERE Status = 'Delivered' AND Id NOT IN (@OrderId1, @OrderId2))

-- Insert Merchant Reviews
PRINT 'Inserting Merchant Reviews...'

INSERT INTO Reviews (Id, ReviewerId, RevieweeId, RevieweeType, OrderId, Rating, Comment, Tags, LikeCount, ReportCount, IsActive, CreatedAt, UpdatedAt)
VALUES 
-- 5-star reviews
(NEWID(), @UserId1, @MerchantId1, 'Merchant', @OrderId1, 5, 'Çok hızlı teslimat! Ürünler taze ve kaliteli. Kesinlikle tekrar sipariş veririm.', 'Hızlı Teslimat,Taze Ürün,Kaliteli', 3, 0, 1, GETDATE(), GETDATE()),
(NEWID(), @UserId2, @MerchantId1, 'Merchant', @OrderId2, 5, 'Mükemmel hizmet! Çok memnun kaldım. Personel çok ilgili ve yardımcı.', 'Mükemmel Hizmet,İlgili Personel', 5, 0, 1, GETDATE() - 1, GETDATE() - 1),
(NEWID(), @UserId3, @MerchantId1, 'Merchant', @OrderId3, 5, 'Harika! Çok hızlı ve güvenilir. Ürünler beklediğimden daha iyi.', 'Hızlı,Güvenilir,Kaliteli', 2, 0, 1, GETDATE() - 2, GETDATE() - 2),

-- 4-star reviews
(NEWID(), @UserId1, @MerchantId2, 'Merchant', @OrderId1, 4, 'Genel olarak iyi hizmet. Biraz geç teslimat oldu ama ürünler kaliteli.', 'Geç Teslimat,Kaliteli', 1, 0, 1, GETDATE() - 3, GETDATE() - 3),
(NEWID(), @UserId2, @MerchantId2, 'Merchant', @OrderId2, 4, 'İyi hizmet. Fiyatlar biraz yüksek ama kalite fena değil.', 'İyi Hizmet,Yüksek Fiyat', 0, 0, 1, GETDATE() - 4, GETDATE() - 4),

-- 3-star reviews
(NEWID(), @UserId3, @MerchantId1, 'Merchant', @OrderId3, 3, 'Orta seviye hizmet. Bazı ürünler taze değildi ama genel olarak idare eder.', 'Orta Seviye,Taze Değil', 0, 1, 1, GETDATE() - 5, GETDATE() - 5),

-- 2-star reviews
(NEWID(), @UserId1, @MerchantId2, 'Merchant', @OrderId1, 2, 'Çok geç teslimat. Ürünler de beklediğim gibi değildi. Memnun kalmadım.', 'Geç Teslimat,Kalitesiz', 0, 2, 1, GETDATE() - 6, GETDATE() - 6),

-- 1-star reviews
(NEWID(), @UserId2, @MerchantId2, 'Merchant', @OrderId2, 1, 'Çok kötü hizmet! Ürünler bozuk geldi. Hiç memnun kalmadım.', 'Kötü Hizmet,Bozuk Ürün', 0, 3, 1, GETDATE() - 7, GETDATE() - 7)

PRINT 'Merchant Reviews inserted successfully.'
GO

-- Insert Courier Reviews
PRINT 'Inserting Courier Reviews...'

INSERT INTO Reviews (Id, ReviewerId, RevieweeId, RevieweeType, OrderId, Rating, Comment, Tags, LikeCount, ReportCount, IsActive, CreatedAt, UpdatedAt)
VALUES 
-- 5-star courier reviews
(NEWID(), @UserId1, @CourierId1, 'Courier', @OrderId1, 5, 'Çok nazik ve hızlı kurye! Paketi özenle teslim etti.', 'Nazik,Hızlı,Özenli', 4, 0, 1, GETDATE(), GETDATE()),
(NEWID(), @UserId2, @CourierId1, 'Courier', @OrderId2, 5, 'Mükemmel kurye! Çok profesyonel ve güler yüzlü.', 'Profesyonel,Güler Yüzlü', 3, 0, 1, GETDATE() - 1, GETDATE() - 1),
(NEWID(), @UserId3, @CourierId1, 'Courier', @OrderId3, 5, 'Harika hizmet! Çok hızlı ve güvenilir.', 'Hızlı,Güvenilir', 2, 0, 1, GETDATE() - 2, GETDATE() - 2),

-- 4-star courier reviews
(NEWID(), @UserId1, @CourierId2, 'Courier', @OrderId1, 4, 'İyi kurye. Biraz geç geldi ama nazikti.', 'İyi Kurye,Geç,Nazik', 1, 0, 1, GETDATE() - 3, GETDATE() - 3),
(NEWID(), @UserId2, @CourierId2, 'Courier', @OrderId2, 4, 'Genel olarak iyi. Biraz daha hızlı olabilirdi.', 'İyi,Hızlı Değil', 0, 0, 1, GETDATE() - 4, GETDATE() - 4),

-- 3-star courier reviews
(NEWID(), @UserId3, @CourierId1, 'Courier', @OrderId3, 3, 'Orta seviye. Ne iyi ne kötü.', 'Orta Seviye', 0, 0, 1, GETDATE() - 5, GETDATE() - 5),

-- 2-star courier reviews
(NEWID(), @UserId1, @CourierId2, 'Courier', @OrderId1, 2, 'Geç teslimat. Kurye de çok ilgisizdi.', 'Geç Teslimat,İlgisiz', 0, 1, 1, GETDATE() - 6, GETDATE() - 6),

-- 1-star courier reviews
(NEWID(), @UserId2, @CourierId2, 'Courier', @OrderId2, 1, 'Çok kötü! Kurye kaba ve geç teslimat.', 'Kötü,Kaba,Geç', 0, 2, 1, GETDATE() - 7, GETDATE() - 7)

PRINT 'Courier Reviews inserted successfully.'
GO

-- Insert Review Helpful Votes
PRINT 'Inserting Review Helpful Votes...'

-- Get some review IDs for helpful votes
DECLARE @ReviewId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 5 AND RevieweeType = 'Merchant')
DECLARE @ReviewId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 5 AND RevieweeType = 'Merchant' AND Id != @ReviewId1)
DECLARE @ReviewId3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 5 AND RevieweeType = 'Courier')

INSERT INTO ReviewHelpfuls (Id, ReviewId, UserId, IsHelpful, CreatedAt)
VALUES 
(NEWID(), @ReviewId1, @UserId2, 1, GETDATE()),
(NEWID(), @ReviewId1, @UserId3, 1, GETDATE()),
(NEWID(), @ReviewId2, @UserId1, 1, GETDATE()),
(NEWID(), @ReviewId2, @UserId3, 1, GETDATE()),
(NEWID(), @ReviewId3, @UserId1, 1, GETDATE()),
(NEWID(), @ReviewId3, @UserId2, 1, GETDATE())

PRINT 'Review Helpful Votes inserted successfully.'
GO

-- Insert Review Reports
PRINT 'Inserting Review Reports...'

-- Get some review IDs for reports
DECLARE @ReportReviewId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 1)
DECLARE @ReportReviewId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 2)

INSERT INTO ReviewReports (Id, ReviewId, ReporterId, Reason, Status, CreatedAt)
VALUES 
(NEWID(), @ReportReviewId1, @UserId1, 'Uygunsuz içerik', 'Pending', GETDATE()),
(NEWID(), @ReportReviewId2, @UserId2, 'Sahte değerlendirme', 'Pending', GETDATE())

PRINT 'Review Reports inserted successfully.'
GO

-- Insert Review Likes
PRINT 'Inserting Review Likes...'

-- Get some review IDs for likes
DECLARE @LikeReviewId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 5 AND RevieweeType = 'Merchant')
DECLARE @LikeReviewId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 5 AND RevieweeType = 'Courier')

INSERT INTO ReviewLikes (Id, ReviewId, UserId, CreatedAt)
VALUES 
(NEWID(), @LikeReviewId1, @UserId2, GETDATE()),
(NEWID(), @LikeReviewId1, @UserId3, GETDATE()),
(NEWID(), @LikeReviewId2, @UserId1, GETDATE()),
(NEWID(), @LikeReviewId2, @UserId2, GETDATE())

PRINT 'Review Likes inserted successfully.'
GO

-- Update Review counts based on inserted data
PRINT 'Updating Review counts...'

UPDATE Reviews 
SET LikeCount = (SELECT COUNT(*) FROM ReviewLikes WHERE ReviewId = Reviews.Id),
    ReportCount = (SELECT COUNT(*) FROM ReviewReports WHERE ReviewId = Reviews.Id)

PRINT 'Review counts updated successfully.'
GO

-- Insert some Review Responses (Merchant responses to reviews)
PRINT 'Inserting Review Responses...'

DECLARE @ResponseReviewId1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 5 AND RevieweeType = 'Merchant')
DECLARE @ResponseReviewId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Reviews WHERE Rating = 4 AND RevieweeType = 'Merchant')

UPDATE Reviews 
SET Response = 'Teşekkür ederiz! Müşteri memnuniyeti bizim için çok önemli.',
    RespondedAt = GETDATE()
WHERE Id = @ResponseReviewId1

UPDATE Reviews 
SET Response = 'Geri bildiriminiz için teşekkürler. Teslimat sürelerimizi iyileştirmeye çalışıyoruz.',
    RespondedAt = GETDATE() - 1
WHERE Id = @ResponseReviewId2

PRINT 'Review Responses inserted successfully.'
GO

PRINT ''
PRINT 'Review Dummy Data insertion completed successfully!'
PRINT 'Total Reviews: ' + CAST((SELECT COUNT(*) FROM Reviews) AS VARCHAR(10))
PRINT 'Merchant Reviews: ' + CAST((SELECT COUNT(*) FROM Reviews WHERE RevieweeType = 'Merchant') AS VARCHAR(10))
PRINT 'Courier Reviews: ' + CAST((SELECT COUNT(*) FROM Reviews WHERE RevieweeType = 'Courier') AS VARCHAR(10))
PRINT 'Review Helpful Votes: ' + CAST((SELECT COUNT(*) FROM ReviewHelpfuls) AS VARCHAR(10))
PRINT 'Review Reports: ' + CAST((SELECT COUNT(*) FROM ReviewReports) AS VARCHAR(10))
PRINT 'Review Likes: ' + CAST((SELECT COUNT(*) FROM ReviewLikes) AS VARCHAR(10))
GO
