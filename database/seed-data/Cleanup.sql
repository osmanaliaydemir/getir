-- =============================================
-- Cleanup Script: Truncate tables in correct FK order
-- WARNING: This deletes ALL data. Use only in dev/testing.
-- =============================================

PRINT '⚠️  Starting database cleanup...';

-- Disable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Delete in reverse dependency order (handle FKs)
-- Reviews system
IF OBJECT_ID('ProductReviewHelpfuls', 'U') IS NOT NULL DELETE FROM ProductReviewHelpfuls;
IF OBJECT_ID('ProductReviews', 'U') IS NOT NULL DELETE FROM ProductReviews;

-- Order related
IF OBJECT_ID('OrderLineOptions', 'U') IS NOT NULL DELETE FROM OrderLineOptions;
IF OBJECT_ID('OrderLines', 'U') IS NOT NULL DELETE FROM OrderLines;
IF OBJECT_ID('OrderStatusTransitionLogs', 'U') IS NOT NULL DELETE FROM OrderStatusTransitionLogs;
IF OBJECT_ID('Payments', 'U') IS NOT NULL DELETE FROM Payments;
IF OBJECT_ID('Orders', 'U') IS NOT NULL DELETE FROM Orders;

-- Favorites, Cart
IF OBJECT_ID('FavoriteProducts', 'U') IS NOT NULL DELETE FROM FavoriteProducts;
IF OBJECT_ID('CartItems', 'U') IS NOT NULL DELETE FROM CartItems;

-- Stock & inventory
IF OBJECT_ID('StockSyncDetails', 'U') IS NOT NULL DELETE FROM StockSyncDetails;
IF OBJECT_ID('StockSyncSessions', 'U') IS NOT NULL DELETE FROM StockSyncSessions;
IF OBJECT_ID('InventoryCountItems', 'U') IS NOT NULL DELETE FROM InventoryCountItems;
IF OBJECT_ID('InventoryDiscrepancies', 'U') IS NOT NULL DELETE FROM InventoryDiscrepancies;
IF OBJECT_ID('InventoryCountSessions', 'U') IS NOT NULL DELETE FROM InventoryCountSessions;
IF OBJECT_ID('StockHistories', 'U') IS NOT NULL DELETE FROM StockHistories;
IF OBJECT_ID('StockAlerts', 'U') IS NOT NULL DELETE FROM StockAlerts;

-- Ratings
IF OBJECT_ID('Ratings', 'U') IS NOT NULL DELETE FROM Ratings;
IF OBJECT_ID('RatingHistories', 'U') IS NOT NULL DELETE FROM RatingHistories;

-- Notifications
IF OBJECT_ID('NotificationHistories', 'U') IS NOT NULL DELETE FROM NotificationHistories;
IF OBJECT_ID('NotificationLogs', 'U') IS NOT NULL DELETE FROM NotificationLogs;
IF OBJECT_ID('NotificationTemplates', 'U') IS NOT NULL DELETE FROM NotificationTemplates;
IF OBJECT_ID('Notifications', 'U') IS NOT NULL DELETE FROM Notifications;
IF OBJECT_ID('SystemNotifications', 'U') IS NOT NULL DELETE FROM SystemNotifications;

-- Realtime tracking
IF OBJECT_ID('TrackingNotifications', 'U') IS NOT NULL DELETE FROM TrackingNotifications;
IF OBJECT_ID('LocationHistories', 'U') IS NOT NULL DELETE FROM LocationHistories;
IF OBJECT_ID('OrderTrackings', 'U') IS NOT NULL DELETE FROM OrderTrackings;
IF OBJECT_ID('ETAEstimations', 'U') IS NOT NULL DELETE FROM ETAEstimations;

-- Documents
IF OBJECT_ID('MerchantDocuments', 'U') IS NOT NULL DELETE FROM MerchantDocuments;

-- Devices
IF OBJECT_ID('DeviceTokens', 'U') IS NOT NULL DELETE FROM DeviceTokens;

-- Loyalty
IF OBJECT_ID('LoyaltyPointTransactions', 'U') IS NOT NULL DELETE FROM LoyaltyPointTransactions;
IF OBJECT_ID('UserLoyaltyPoints', 'U') IS NOT NULL DELETE FROM UserLoyaltyPoints;

-- Coupons & campaigns
IF OBJECT_ID('CouponUsages', 'U') IS NOT NULL DELETE FROM CouponUsages;
IF OBJECT_ID('Coupons', 'U') IS NOT NULL DELETE FROM Coupons;
IF OBJECT_ID('Campaigns', 'U') IS NOT NULL DELETE FROM Campaigns;

-- Market/Restaurant options
IF OBJECT_ID('RestaurantProductOptions', 'U') IS NOT NULL DELETE FROM RestaurantProductOptions;
IF OBJECT_ID('RestaurantProductOptionGroups', 'U') IS NOT NULL DELETE FROM RestaurantProductOptionGroups;
IF OBJECT_ID('RestaurantProducts', 'U') IS NOT NULL DELETE FROM RestaurantProducts;
IF OBJECT_ID('RestaurantMenuCategories', 'U') IS NOT NULL DELETE FROM RestaurantMenuCategories;
IF OBJECT_ID('MarketProductVariants', 'U') IS NOT NULL DELETE FROM MarketProductVariants;
IF OBJECT_ID('MarketProducts', 'U') IS NOT NULL DELETE FROM MarketProducts;
IF OBJECT_ID('MarketCategories', 'U') IS NOT NULL DELETE FROM MarketCategories;

-- Products & categories
IF OBJECT_ID('ProductOptions', 'U') IS NOT NULL DELETE FROM ProductOptions;
IF OBJECT_ID('ProductOptionGroups', 'U') IS NOT NULL DELETE FROM ProductOptionGroups;
IF OBJECT_ID('Products', 'U') IS NOT NULL DELETE FROM Products;
IF OBJECT_ID('ProductCategories', 'U') IS NOT NULL DELETE FROM ProductCategories;

-- Merchants & related
IF OBJECT_ID('WorkingHours', 'U') IS NOT NULL DELETE FROM WorkingHours;
IF OBJECT_ID('DeliveryZonePoints', 'U') IS NOT NULL DELETE FROM DeliveryZonePoints;
IF OBJECT_ID('DeliveryZones', 'U') IS NOT NULL DELETE FROM DeliveryZones;
IF OBJECT_ID('StockSettings', 'U') IS NOT NULL DELETE FROM StockSettings;
IF OBJECT_ID('Merchants', 'U') IS NOT NULL DELETE FROM Merchants;

-- I18n
IF OBJECT_ID('Translations', 'U') IS NOT NULL DELETE FROM Translations;
IF OBJECT_ID('Languages', 'U') IS NOT NULL DELETE FROM Languages;

-- Users & addresses & tokens
IF OBJECT_ID('UserNotificationPreferences', 'U') IS NOT NULL DELETE FROM UserNotificationPreferences;
IF OBJECT_ID('UserLanguagePreferences', 'U') IS NOT NULL DELETE FROM UserLanguagePreferences;
IF OBJECT_ID('UserAddresses', 'U') IS NOT NULL DELETE FROM UserAddresses;
IF OBJECT_ID('RefreshTokens', 'U') IS NOT NULL DELETE FROM RefreshTokens;

-- Couriers
IF OBJECT_ID('CourierLocations', 'U') IS NOT NULL DELETE FROM CourierLocations;
IF OBJECT_ID('Couriers', 'U') IS NOT NULL DELETE FROM Couriers;

-- Merchants support tables
IF OBJECT_ID('ServiceCategories', 'U') IS NOT NULL DELETE FROM ServiceCategories;

-- Audit & logs
IF OBJECT_ID('AuditLogs', 'U') IS NOT NULL DELETE FROM AuditLogs;
IF OBJECT_ID('UserActivityLogs', 'U') IS NOT NULL DELETE FROM UserActivityLogs;
IF OBJECT_ID('SystemChangeLogs', 'U') IS NOT NULL DELETE FROM SystemChangeLogs;
IF OBJECT_ID('SecurityEventLogs', 'U') IS NOT NULL DELETE FROM SecurityEventLogs;
IF OBJECT_ID('LogAnalysisReports', 'U') IS NOT NULL DELETE FROM LogAnalysisReports;

-- Rate limiting
IF OBJECT_ID('RateLimitViolations', 'U') IS NOT NULL DELETE FROM RateLimitViolations;
IF OBJECT_ID('RateLimitLogs', 'U') IS NOT NULL DELETE FROM RateLimitLogs;
IF OBJECT_ID('RateLimitRules', 'U') IS NOT NULL DELETE FROM RateLimitRules;
IF OBJECT_ID('RateLimitConfigurations', 'U') IS NOT NULL DELETE FROM RateLimitConfigurations;

-- Users last (keep admins if needed; here we purge all)
IF OBJECT_ID('Users', 'U') IS NOT NULL DELETE FROM Users;

-- Re-enable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

PRINT '✅ Database cleanup completed.';


