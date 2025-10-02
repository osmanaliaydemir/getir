-- =============================================
-- Getir Clone - Seed Data
-- Test ve demo verileri
-- =============================================

USE GetirDb;
GO

-- =============================================
-- SERVICE CATEGORIES
-- =============================================

INSERT INTO ServiceCategories (Id, Name, Description, ImageUrl, IconUrl, DisplayOrder, IsActive, CreatedAt) VALUES
(NEWID(), 'Market', 'Günlük alışveriş ihtiyaçlarınız için', 'https://example.com/market.jpg', 'https://example.com/market-icon.png', 1, 1, GETUTCDATE()),
(NEWID(), 'Yemek', 'Lezzetli yemekler ve fast food', 'https://example.com/food.jpg', 'https://example.com/food-icon.png', 2, 1, GETUTCDATE()),
(NEWID(), 'Su & İçecek', 'Su, meşrubat ve içecekler', 'https://example.com/drinks.jpg', 'https://example.com/drinks-icon.png', 3, 1, GETUTCDATE()),
(NEWID(), 'Eczane', 'İlaç ve sağlık ürünleri', 'https://example.com/pharmacy.jpg', 'https://example.com/pharmacy-icon.png', 4, 1, GETUTCDATE()),
(NEWID(), 'Pet Shop', 'Evcil hayvan ürünleri', 'https://example.com/petshop.jpg', 'https://example.com/petshop-icon.png', 5, 1, GETUTCDATE());

-- =============================================
-- USERS (Test Users)
-- =============================================

-- Admin User
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt) VALUES
('11111111-1111-1111-1111-111111111111', 'admin@getir.com', '$2a$11$example.hash.for.admin', 'Admin', 'User', '+905551234567', 4, 1, 1, GETUTCDATE());

-- Customer Users
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt) VALUES
('22222222-2222-2222-2222-222222222222', 'customer1@example.com', '$2a$11$example.hash.for.customer1', 'Ahmet', 'Yılmaz', '+905551234568', 1, 1, 1, GETUTCDATE()),
('33333333-3333-3333-3333-333333333333', 'customer2@example.com', '$2a$11$example.hash.for.customer2', 'Ayşe', 'Demir', '+905551234569', 1, 1, 1, GETUTCDATE()),
('44444444-4444-4444-4444-444444444444', 'customer3@example.com', '$2a$11$example.hash.for.customer3', 'Mehmet', 'Kaya', '+905551234570', 1, 1, 1, GETUTCDATE());

-- Merchant Owner
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt) VALUES
('55555555-5555-5555-5555-555555555555', 'merchant@example.com', '$2a$11$example.hash.for.merchant', 'Ali', 'Veli', '+905551234571', 2, 1, 1, GETUTCDATE());

-- Courier
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role, IsEmailVerified, IsActive, CreatedAt) VALUES
('66666666-6666-6666-6666-666666666666', 'courier@example.com', '$2a$11$example.hash.for.courier', 'Kurye', 'Ali', '+905551234572', 3, 1, 1, GETUTCDATE());

-- =============================================
-- MERCHANTS
-- =============================================

DECLARE @MarketCategoryId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ServiceCategories WHERE Name = 'Market');
DECLARE @FoodCategoryId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ServiceCategories WHERE Name = 'Yemek');

INSERT INTO Merchants (Id, OwnerId, Name, Description, ServiceCategoryId, LogoUrl, CoverImageUrl, Address, Latitude, Longitude, PhoneNumber, Email, MinimumOrderAmount, DeliveryFee, AverageDeliveryTime, Rating, TotalReviews, IsActive, IsBusy, IsOpen, CreatedAt) VALUES
('77777777-7777-7777-7777-777777777777', '55555555-5555-5555-5555-555555555555', 'Migros Express', 'Günlük alışveriş ihtiyaçlarınız için taze ürünler', @MarketCategoryId, 'https://example.com/migros-logo.jpg', 'https://example.com/migros-cover.jpg', 'Kadıköy, İstanbul', 40.9923, 29.0244, '+905551234573', 'migros@example.com', 50.00, 5.00, 30, 4.5, 150, 1, 0, 1, GETUTCDATE()),
('88888888-8888-8888-8888-888888888888', '55555555-5555-5555-5555-555555555555', 'Burger King', 'Lezzetli hamburgerler ve fast food', @FoodCategoryId, 'https://example.com/burgerking-logo.jpg', 'https://example.com/burgerking-cover.jpg', 'Beşiktaş, İstanbul', 41.0428, 29.0082, '+905551234574', 'burgerking@example.com', 30.00, 8.00, 25, 4.2, 89, 1, 0, 1, GETUTCDATE());

-- =============================================
-- PRODUCT CATEGORIES
-- =============================================

DECLARE @MigrosId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777';
DECLARE @BurgerKingId UNIQUEIDENTIFIER = '88888888-8888-8888-8888-888888888888';

-- Migros Categories
INSERT INTO ProductCategories (Id, MerchantId, Name, Description, ImageUrl, DisplayOrder, IsActive, CreatedAt) VALUES
(NEWID(), @MigrosId, 'Meyve & Sebze', 'Taze meyve ve sebzeler', 'https://example.com/fruits-vegetables.jpg', 1, 1, GETUTCDATE()),
(NEWID(), @MigrosId, 'Et & Tavuk', 'Taze et ve tavuk ürünleri', 'https://example.com/meat-chicken.jpg', 2, 1, GETUTCDATE()),
(NEWID(), @MigrosId, 'Süt & Kahvaltılık', 'Süt ürünleri ve kahvaltılık', 'https://example.com/dairy.jpg', 3, 1, GETUTCDATE()),
(NEWID(), @MigrosId, 'Temizlik', 'Temizlik ürünleri', 'https://example.com/cleaning.jpg', 4, 1, GETUTCDATE());

-- Burger King Categories
INSERT INTO ProductCategories (Id, MerchantId, Name, Description, ImageUrl, DisplayOrder, IsActive, CreatedAt) VALUES
(NEWID(), @BurgerKingId, 'Hamburgerler', 'Lezzetli hamburgerler', 'https://example.com/burgers.jpg', 1, 1, GETUTCDATE()),
(NEWID(), @BurgerKingId, 'Tavuk Ürünleri', 'Tavuk burger ve nugget', 'https://example.com/chicken.jpg', 2, 1, GETUTCDATE()),
(NEWID(), @BurgerKingId, 'İçecekler', 'Soğuk içecekler', 'https://example.com/drinks.jpg', 3, 1, GETUTCDATE()),
(NEWID(), @BurgerKingId, 'Tatlılar', 'Tatlı ve atıştırmalıklar', 'https://example.com/desserts.jpg', 4, 1, GETUTCDATE());

-- =============================================
-- PRODUCTS
-- =============================================

DECLARE @MeyveSebzeId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Meyve & Sebze' AND MerchantId = @MigrosId);
DECLARE @HamburgerlerId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Hamburgerler' AND MerchantId = @BurgerKingId);

-- Migros Products
INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, CreatedAt) VALUES
(NEWID(), @MigrosId, @MeyveSebzeId, 'Elma (1 kg)', 'Taze kırmızı elma', 'https://example.com/apple.jpg', 15.50, 12.99, 100, 'kg', 1, 1, 1, GETUTCDATE()),
(NEWID(), @MigrosId, @MeyveSebzeId, 'Muz (1 kg)', 'Taze muz', 'https://example.com/banana.jpg', 18.90, NULL, 50, 'kg', 1, 1, 2, GETUTCDATE()),
(NEWID(), @MigrosId, @MeyveSebzeId, 'Domates (1 kg)', 'Taze domates', 'https://example.com/tomato.jpg', 12.00, 9.99, 75, 'kg', 1, 1, 3, GETUTCDATE()),
(NEWID(), @MigrosId, @MeyveSebzeId, 'Salatalık (1 kg)', 'Taze salatalık', 'https://example.com/cucumber.jpg', 8.50, NULL, 60, 'kg', 1, 1, 4, GETUTCDATE());

-- Burger King Products
INSERT INTO Products (Id, MerchantId, ProductCategoryId, Name, Description, ImageUrl, Price, DiscountedPrice, StockQuantity, Unit, IsAvailable, IsActive, DisplayOrder, CreatedAt) VALUES
(NEWID(), @BurgerKingId, @HamburgerlerId, 'Whopper', 'Klasik Whopper hamburger', 'https://example.com/whopper.jpg', 45.90, 39.90, 50, 'adet', 1, 1, 1, GETUTCDATE()),
(NEWID(), @BurgerKingId, @HamburgerlerId, 'Chicken Royale', 'Tavuklu hamburger', 'https://example.com/chicken-royale.jpg', 42.90, NULL, 30, 'adet', 1, 1, 2, GETUTCDATE()),
(NEWID(), @BurgerKingId, @HamburgerlerId, 'Cheeseburger', 'Peynirli hamburger', 'https://example.com/cheeseburger.jpg', 28.90, 24.90, 40, 'adet', 1, 1, 3, GETUTCDATE());

-- =============================================
-- COUPONS
-- =============================================

INSERT INTO Coupons (Id, Code, Title, Description, DiscountType, DiscountValue, MinimumOrderAmount, MaximumDiscountAmount, StartDate, EndDate, UsageLimit, UsageCount, IsActive, CreatedAt) VALUES
(NEWID(), 'WELCOME10', 'Hoş Geldin İndirimi', 'İlk siparişinizde %10 indirim', 'Percentage', 10.00, 50.00, 20.00, GETUTCDATE(), DATEADD(month, 1, GETUTCDATE()), 1000, 0, 1, GETUTCDATE()),
(NEWID(), 'SAVE20', '20 TL İndirim', '50 TL ve üzeri siparişlerde 20 TL indirim', 'FixedAmount', 20.00, 50.00, NULL, GETUTCDATE(), DATEADD(month, 2, GETUTCDATE()), 500, 0, 1, GETUTCDATE()),
(NEWID(), 'FIRSTORDER', 'İlk Sipariş', 'İlk siparişinizde 15 TL indirim', 'FixedAmount', 15.00, 30.00, NULL, GETUTCDATE(), DATEADD(month, 3, GETUTCDATE()), 2000, 0, 1, GETUTCDATE());

-- =============================================
-- COURIERS
-- =============================================

INSERT INTO Couriers (Id, UserId, VehicleType, LicensePlate, IsAvailable, IsActive, CurrentLatitude, CurrentLongitude, LastLocationUpdate, TotalDeliveries, Rating, CreatedAt) VALUES
(NEWID(), '66666666-6666-6666-6666-666666666666', 'Motorcycle', '34ABC123', 1, 1, 41.0082, 28.9784, GETUTCDATE(), 0, NULL, GETUTCDATE());

-- =============================================
-- WORKING HOURS
-- =============================================

-- Migros Working Hours (7/24)
INSERT INTO WorkingHours (Id, MerchantId, DayOfWeek, OpenTime, CloseTime, IsClosed, CreatedAt) VALUES
(NEWID(), @MigrosId, 0, '00:00', '23:59', 0, GETUTCDATE()), -- Sunday
(NEWID(), @MigrosId, 1, '00:00', '23:59', 0, GETUTCDATE()), -- Monday
(NEWID(), @MigrosId, 2, '00:00', '23:59', 0, GETUTCDATE()), -- Tuesday
(NEWID(), @MigrosId, 3, '00:00', '23:59', 0, GETUTCDATE()), -- Wednesday
(NEWID(), @MigrosId, 4, '00:00', '23:59', 0, GETUTCDATE()), -- Thursday
(NEWID(), @MigrosId, 5, '00:00', '23:59', 0, GETUTCDATE()), -- Friday
(NEWID(), @MigrosId, 6, '00:00', '23:59', 0, GETUTCDATE()); -- Saturday

-- Burger King Working Hours (10:00-02:00)
INSERT INTO WorkingHours (Id, MerchantId, DayOfWeek, OpenTime, CloseTime, IsClosed, CreatedAt) VALUES
(NEWID(), @BurgerKingId, 0, '10:00', '02:00', 0, GETUTCDATE()), -- Sunday
(NEWID(), @BurgerKingId, 1, '10:00', '02:00', 0, GETUTCDATE()), -- Monday
(NEWID(), @BurgerKingId, 2, '10:00', '02:00', 0, GETUTCDATE()), -- Tuesday
(NEWID(), @BurgerKingId, 3, '10:00', '02:00', 0, GETUTCDATE()), -- Wednesday
(NEWID(), @BurgerKingId, 4, '10:00', '02:00', 0, GETUTCDATE()), -- Thursday
(NEWID(), @BurgerKingId, 5, '10:00', '02:00', 0, GETUTCDATE()), -- Friday
(NEWID(), @BurgerKingId, 6, '10:00', '02:00', 0, GETUTCDATE()); -- Saturday

-- =============================================
-- DELIVERY ZONES
-- =============================================

-- Migros Delivery Zone
INSERT INTO DeliveryZones (Id, MerchantId, Name, Description, DeliveryFee, EstimatedDeliveryTime, IsActive, CreatedAt) VALUES
(NEWID(), @MigrosId, 'Kadıköy Merkez', 'Kadıköy merkez bölgesi', 5.00, 30, 1, GETUTCDATE());

-- Burger King Delivery Zone
INSERT INTO DeliveryZones (Id, MerchantId, Name, Description, DeliveryFee, EstimatedDeliveryTime, IsActive, CreatedAt) VALUES
(NEWID(), @BurgerKingId, 'Beşiktaş Merkez', 'Beşiktaş merkez bölgesi', 8.00, 25, 1, GETUTCDATE());

-- =============================================
-- USER ADDRESSES
-- =============================================

INSERT INTO UserAddresses (Id, UserId, Title, FullAddress, City, District, Latitude, Longitude, IsDefault, IsActive, CreatedAt) VALUES
(NEWID(), '22222222-2222-2222-2222-222222222222', 'Ev', 'Kadıköy, İstanbul', 'İstanbul', 'Kadıköy', 40.9923, 29.0244, 1, 1, GETUTCDATE()),
(NEWID(), '33333333-3333-3333-3333-333333333333', 'İş', 'Beşiktaş, İstanbul', 'İstanbul', 'Beşiktaş', 41.0428, 29.0082, 1, 1, GETUTCDATE());

-- =============================================
-- MERCHANT ONBOARDING
-- =============================================

INSERT INTO MerchantOnboardings (Id, MerchantId, OwnerId, BasicInfoCompleted, BusinessInfoCompleted, WorkingHoursCompleted, DeliveryZonesCompleted, ProductsAdded, DocumentsUploaded, IsVerified, IsApproved, CompletedSteps, TotalSteps, ProgressPercentage, CreatedAt) VALUES
(NEWID(), @MigrosId, '55555555-5555-5555-5555-555555555555', 1, 1, 1, 1, 1, 1, 1, 1, 6, 6, 100.00, GETUTCDATE()),
(NEWID(), @BurgerKingId, '55555555-5555-5555-5555-555555555555', 1, 1, 1, 1, 1, 1, 1, 1, 6, 6, 100.00, GETUTCDATE());

-- =============================================
-- USER LOYALTY POINTS
-- =============================================

INSERT INTO UserLoyaltyPoints (Id, UserId, Points, TotalEarned, TotalSpent, UpdatedAt) VALUES
(NEWID(), '22222222-2222-2222-2222-222222222222', 150, 200, 50, GETUTCDATE()),
(NEWID(), '33333333-3333-3333-3333-333333333333', 75, 100, 25, GETUTCDATE()),
(NEWID(), '44444444-4444-4444-4444-444444444444', 0, 0, 0, GETUTCDATE());

-- =============================================
-- NOTIFICATIONS
-- =============================================

INSERT INTO Notifications (Id, UserId, Title, Message, Type, IsRead, CreatedAt) VALUES
(NEWID(), '22222222-2222-2222-2222-222222222222', 'Hoş Geldiniz!', 'Getir uygulamasına hoş geldiniz. İlk siparişinizde %10 indirim kazanın!', 'Promotion', 0, GETUTCDATE()),
(NEWID(), '33333333-3333-3333-3333-333333333333', 'Yeni Ürünler', 'Favori restoranınızda yeni ürünler var!', 'Promotion', 0, GETUTCDATE());

-- =============================================
-- SYSTEM NOTIFICATIONS
-- =============================================

INSERT INTO SystemNotifications (Id, Title, Message, Type, TargetRoles, CreatedBy, IsActive, Priority, CreatedAt) VALUES
(NEWID(), 'Sistem Bakımı', 'Sistem bakımı nedeniyle 2 saat süreyle hizmet veremeyeceğiz.', 'WARNING', 'Customer,MerchantOwner,Courier', '11111111-1111-1111-1111-111111111111', 1, 3, GETUTCDATE()),
(NEWID(), 'Yeni Özellik', 'Artık favori ürünlerinizi kaydedebilirsiniz!', 'INFO', 'Customer', '11111111-1111-1111-1111-111111111111', 1, 1, GETUTCDATE());

PRINT 'Seed data inserted successfully!';
GO
