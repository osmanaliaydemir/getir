using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Getir.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core context sınıfı
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// AppDbContext constructor
    /// </summary>
    /// <param name="options">Entity Framework Core options</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users tablosu
    /// </summary>
    public DbSet<User> Users { get; set; }
    /// <summary>
    /// RefreshTokens tablosu
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    /// <summary>
    /// ServiceCategories tablosu
    /// </summary>
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    /// <summary>
    /// ProductCategories tablosu
    /// </summary>
    public DbSet<ProductCategory> ProductCategories { get; set; }
    /// <summary>
    /// Merchants tablosu
    /// </summary>
    public DbSet<Merchant> Merchants { get; set; }
    /// <summary>
    /// Products tablosu
    /// </summary>
    public DbSet<Product> Products { get; set; }
    /// <summary>
    /// Orders tablosu
    /// </summary>
    public DbSet<Order> Orders { get; set; }
    /// <summary>
    /// OrderLines tablosu
    /// </summary>
    public DbSet<OrderLine> OrderLines { get; set; }
    /// <summary>
    /// OrderStatusTransitionLogs tablosu
    /// </summary>
    public DbSet<OrderStatusTransitionLog> OrderStatusTransitionLogs { get; set; }
    /// <summary>
    /// StockHistories tablosu
    /// </summary>
    public DbSet<StockHistory> StockHistories { get; set; }
    /// <summary>
    /// StockAlerts tablosu
    /// </summary>
    public DbSet<StockAlert> StockAlerts { get; set; }
    /// <summary>
    /// StockSettings tablosu
    /// </summary>
    public DbSet<StockSettings> StockSettings { get; set; }
    /// <summary>
    /// UserAddresses tablosu
    /// </summary>
    public DbSet<UserAddress> UserAddresses { get; set; }
    /// <summary>
    /// CartItems tablosu
    /// </summary>
    public DbSet<CartItem> CartItems { get; set; }
    /// <summary>
    /// Coupons tablosu
    /// </summary>
    public DbSet<Coupon> Coupons { get; set; }
    /// <summary>
    /// CouponUsages tablosu
    /// </summary>
    public DbSet<CouponUsage> CouponUsages { get; set; }
    /// <summary>
    /// Campaigns tablosu
    /// </summary>
    public DbSet<Campaign> Campaigns { get; set; }
    /// <summary>
    /// Notifications tablosu
    /// </summary>
    public DbSet<Notification> Notifications { get; set; }
    /// <summary>
    /// Couriers tablosu
    /// </summary>
    public DbSet<Courier> Couriers { get; set; }
    /// <summary>
    /// UserLoyaltyPoints tablosu
    /// </summary>
    public DbSet<UserLoyaltyPoint> UserLoyaltyPoints { get; set; }
    /// <summary>
    /// LoyaltyPointTransactions tablosu
    /// </summary>
    public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; }
    /// <summary>
    /// WorkingHours tablosu
    /// </summary>
    public DbSet<WorkingHours> WorkingHours { get; set; }
    /// <summary>
    /// SpecialHolidays tablosu
    /// </summary>
    public DbSet<SpecialHoliday> SpecialHolidays { get; set; }
    /// <summary>
    /// DeliveryZones tablosu
    /// </summary>
    public DbSet<DeliveryZone> DeliveryZones { get; set; }
    /// <summary>
    /// ProductOptionGroups tablosu
    /// </summary>
    public DbSet<ProductOptionGroup> ProductOptionGroups { get; set; }
    /// <summary>
    /// ProductOptions tablosu
    /// </summary>
    public DbSet<ProductOption> ProductOptions { get; set; }
    /// <summary>
    /// OrderLineOptions tablosu
    /// </summary>
    public DbSet<OrderLineOption> OrderLineOptions { get; set; }
    /// <summary>
    /// Reviews tablosu
    /// </summary>
    public DbSet<Review> Reviews { get; set; }
    /// <summary>
    /// ReviewTags tablosu
    /// </summary>
    public DbSet<ReviewTag> ReviewTags { get; set; }
    /// <summary>
    /// ReviewHelpfuls tablosu
    /// </summary>
    public DbSet<ReviewHelpful> ReviewHelpfuls { get; set; }
    /// <summary>
    /// ReviewModerationLogs tablosu
    /// </summary>
    public DbSet<ReviewModerationLog> ReviewModerationLogs { get; set; }
    /// <summary>
    /// ReviewReports tablosu
    /// </summary>
    public DbSet<ReviewReport> ReviewReports { get; set; }
    /// <summary>
    /// ReviewLikes tablosu
    /// </summary>
    public DbSet<ReviewLike> ReviewLikes { get; set; }
    /// <summary>
    /// ReviewBookmarks tablosu
    /// </summary>
    public DbSet<ReviewBookmark> ReviewBookmarks { get; set; }
    /// <summary>
    /// Ratings tablosu
    /// </summary>
    public DbSet<Rating> Ratings { get; set; }
    /// <summary>
    /// RatingHistories tablosu
    /// </summary>
    public DbSet<RatingHistory> RatingHistories { get; set; }
    /// <summary>
    /// ProductReviews tablosu
    /// </summary>
    public DbSet<ProductReview> ProductReviews { get; set; }
    /// <summary>
    /// ProductReviewHelpfuls tablosu
    /// </summary>
    public DbSet<ProductReviewHelpful> ProductReviewHelpfuls { get; set; }
    /// <summary>
    /// DeliveryZonePoints tablosu
    /// </summary>
    public DbSet<DeliveryZonePoint> DeliveryZonePoints { get; set; }
    /// <summary>
    /// MerchantOnboardings tablosu
    /// </summary>
    public DbSet<MerchantOnboarding> MerchantOnboardings { get; set; }
    /// <summary>
    /// AuditLogs tablosu
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; }
    /// <summary>
    /// SystemNotifications tablosu
    /// </summary>
    public DbSet<SystemNotification> SystemNotifications { get; set; }
    
    // Realtime Tracking entities
    /// <summary>
    /// OrderTrackings tablosu
    /// </summary>
    public DbSet<OrderTracking> OrderTrackings { get; set; }
    /// <summary>
    /// LocationHistories tablosu
    /// </summary>
    public DbSet<LocationHistory> LocationHistories { get; set; }
    /// <summary>
    /// TrackingNotifications tablosu
    /// </summary>
    public DbSet<TrackingNotification> TrackingNotifications { get; set; }
    /// <summary>
    /// ETAEstimations tablosu
    /// </summary>
    public DbSet<ETAEstimation> ETAEstimations { get; set; }
    /// <summary>
    /// TrackingSettings tablosu
    /// </summary>
    public DbSet<TrackingSettings> TrackingSettings { get; set; }
    
    // Audit Logging entities
    /// <summary>
    /// UserActivityLogs tablosu
    /// </summary>
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    /// <summary>
    /// SystemChangeLogs tablosu
    /// </summary>
    public DbSet<SystemChangeLog> SystemChangeLogs { get; set; }
    /// <summary>
    /// SecurityEventLogs tablosu
    /// </summary>
    public DbSet<SecurityEventLog> SecurityEventLogs { get; set; }
    /// <summary>
    /// LogAnalysisReports tablosu
    /// </summary>
    public DbSet<LogAnalysisReport> LogAnalysisReports { get; set; }

    // Internationalization entities
    /// <summary>
    /// Languages tablosu
    /// </summary>
    public DbSet<Language> Languages { get; set; }
    /// <summary>
    /// Translations tablosu
    /// </summary>
    public DbSet<Translation> Translations { get; set; }
    public DbSet<UserLanguagePreference> UserLanguagePreferences { get; set; }
    /// <summary>
    /// UserNotificationPreferences tablosu
    /// </summary>
    public DbSet<UserNotificationPreferences> UserNotificationPreferences { get; set; }
    
    // Rate Limiting entities
    /// <summary>
    /// RateLimitRules tablosu
    /// </summary>
    public DbSet<RateLimitRule> RateLimitRules { get; set; }
    /// <summary>
    /// RateLimitLogs tablosu
    /// </summary>
    public DbSet<RateLimitLog> RateLimitLogs { get; set; }
    /// <summary>
    /// RateLimitViolations tablosu
    /// </summary>
    public DbSet<RateLimitViolation> RateLimitViolations { get; set; }
    /// <summary>
    /// RateLimitConfigurations tablosu
    /// </summary>
    public DbSet<RateLimitConfiguration> RateLimitConfigurations { get; set; }
    
    // Payment entities
    /// <summary>
    /// Payments tablosu
    /// </summary>
    public DbSet<Payment> Payments { get; set; }
    /// <summary>
    /// CourierCashCollections tablosu
    /// </summary>
    public DbSet<CourierCashCollection> CourierCashCollections { get; set; }
    /// <summary>
    /// CashSettlements tablosu
    /// </summary>
    public DbSet<CashSettlement> CashSettlements { get; set; }
    /// <summary>
    /// CashPaymentSecurity tablosu
    /// </summary>
    public DbSet<CashPaymentSecurity> CashPaymentSecurities { get; set; }
    /// <summary>
    /// CashPaymentEvidences tablosu
    /// </summary>
    public DbSet<CashPaymentEvidence> CashPaymentEvidences { get; set; }
    /// <summary>
    /// CashPaymentAuditLogs tablosu
    /// </summary>
    public DbSet<CashPaymentAuditLog> CashPaymentAuditLogs { get; set; }
    
    // Delivery optimization entities
    /// <summary>
    /// DeliveryCapacity tablosu
    /// </summary>
    public DbSet<DeliveryCapacity> DeliveryCapacities { get; set; }
    /// <summary>
    /// DeliveryRoute tablosu
    /// </summary>
    public DbSet<DeliveryRoute> DeliveryRoutes { get; set; }
    /// <summary>
    /// CourierLocation tablosu
    /// </summary>
    public DbSet<CourierLocation> CourierLocations { get; set; }
    
    // Favorites
    /// <summary>
    /// FavoriteProducts tablosu
    /// </summary>
    public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
    
    // Device & Notification entities
    /// <summary>
    /// DeviceTokens tablosu
    /// </summary>
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    /// <summary>
    /// NotificationLogs tablosu
    /// </summary>
    public DbSet<NotificationLog> NotificationLogs { get; set; }
    /// <summary>
    /// NotificationHistories tablosu
    /// </summary>
    public DbSet<NotificationHistory> NotificationHistories { get; set; }
    /// <summary>
    /// NotificationTemplates tablosu
    /// </summary>
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    
    // Merchant documents
    /// <summary>
    /// MerchantDocuments tablosu
    /// </summary>
    public DbSet<MerchantDocument> MerchantDocuments { get; set; }
    
    // Market entities
    /// <summary>
    /// Markets tablosu
    /// </summary>
    public DbSet<Market> Markets { get; set; }
    /// <summary>
    /// MarketCategories tablosu
    /// </summary>
    public DbSet<MarketCategory> MarketCategories { get; set; }
    /// <summary>
    /// MarketProducts tablosu
    /// </summary>
    public DbSet<MarketProduct> MarketProducts { get; set; }
    /// <summary>
    /// MarketProductVariants tablosu
    /// </summary>
    public DbSet<MarketProductVariant> MarketProductVariants { get; set; }
    
    // Restaurant entities
    /// <summary>
    /// Restaurants tablosu
    /// </summary>
    public DbSet<Restaurant> Restaurants { get; set; }
    /// <summary>
    /// RestaurantMenuCategories tablosu
    /// </summary>
    public DbSet<RestaurantMenuCategory> RestaurantMenuCategories { get; set; }
    /// <summary>
    /// RestaurantProducts tablosu
    /// </summary>
    public DbSet<RestaurantProduct> RestaurantProducts { get; set; }
    /// <summary>
    /// RestaurantProductOptions tablosu
    /// </summary>
    public DbSet<RestaurantProductOption> RestaurantProductOptions { get; set; }
    /// <summary>
    /// RestaurantProductOptionGroups tablosu
    /// </summary>
    public DbSet<RestaurantProductOptionGroup> RestaurantProductOptionGroups { get; set; }
    
    // Inventory entities
    /// <summary>
    /// InventoryCountSessions tablosu
    /// </summary>
    public DbSet<InventoryCountSession> InventoryCountSessions { get; set; }
    /// <summary>
    /// InventoryCountItems tablosu
    /// </summary>
    public DbSet<InventoryCountItem> InventoryCountItems { get; set; }
    /// <summary>
    /// InventoryDiscrepancies tablosu
    /// </summary>
    public DbSet<InventoryDiscrepancy> InventoryDiscrepancies { get; set; }
    
    // Stock sync entities
    /// <summary>
    /// StockSyncSessions tablosu
    /// </summary>
    public DbSet<StockSyncSession> StockSyncSessions { get; set; }
    /// <summary>
    /// StockSyncDetails tablosu
    /// </summary>
    public DbSet<StockSyncDetail> StockSyncDetails { get; set; }

    /// <summary>
    /// OnModelCreating method
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).IsRequired().HasConversion<int>().HasDefaultValue(Domain.Enums.UserRole.Customer);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Configure inverse navigation for UserNotificationPreferences
            entity.HasOne(u => u.NotificationPreferences)
                .WithOne(p => p.User)
                .HasForeignKey<UserNotificationPreferences>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(512);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ServiceCategory configuration
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ProductCategory configuration
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Merchant)
                .WithMany(m => m.ProductCategories)
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceCategory)
                .WithMany()
                .HasForeignKey(e => e.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Merchant configuration
        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);
            entity.Property(e => e.MinimumOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.Rating).HasPrecision(3, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedMerchants)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ServiceCategory)
                .WithMany(c => c.Merchants)
                .HasForeignKey(e => e.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DiscountedPrice).HasPrecision(18, 2);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Rating).HasPrecision(3, 2); // 0.00 - 5.00
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.Rating);
            entity.HasIndex(e => e.ReviewCount);

            entity.HasOne(e => e.Merchant)
                .WithMany(m => m.Products)
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProductCategory)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.ProductCategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.DeliveryAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DeliveryLatitude).HasPrecision(10, 8);
            entity.Property(e => e.DeliveryLongitude).HasPrecision(11, 8);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Merchant)
                .WithMany(m => m.Orders)
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderLine configuration
        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.VariantName).HasMaxLength(200);
            entity.Property(e => e.ProductVariantId);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Index for variant lookups
            entity.HasIndex(e => e.ProductVariantId);
        });

        // WorkingHours configuration
        modelBuilder.Entity<WorkingHours>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DayOfWeek).IsRequired();
            entity.Property(e => e.OpenTime).HasConversion<string>();
            entity.Property(e => e.CloseTime).HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.MerchantId, e.DayOfWeek }).IsUnique();
            
            entity.HasOne(e => e.Merchant)
                .WithMany(m => m.WorkingHours)
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SpecialHoliday configuration
        modelBuilder.Entity<SpecialHoliday>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.IsClosed).IsRequired().HasDefaultValue(true);
            
            // Nullable TimeSpan - EF Core 9 native support
            entity.Property(e => e.SpecialOpenTime)
                .HasColumnType("TIME")
                .IsRequired(false);
            
            entity.Property(e => e.SpecialCloseTime)
                .HasColumnType("TIME")
                .IsRequired(false);
            
            entity.Property(e => e.IsRecurring).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.MerchantId);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
            entity.HasIndex(e => e.IsActive);
            
            entity.HasOne(e => e.Merchant)
                .WithMany()
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeliveryZone configuration
        modelBuilder.Entity<DeliveryZone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DeliveryFee).HasPrecision(18, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.Merchant)
                .WithMany(m => m.DeliveryZones)
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeliveryZonePoint configuration
        modelBuilder.Entity<DeliveryZonePoint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.DeliveryZone)
                .WithMany(dz => dz.Points)
                .HasForeignKey(e => e.DeliveryZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MerchantOnboarding configuration
        modelBuilder.Entity<MerchantOnboarding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProgressPercentage).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.MerchantId).IsUnique();
            
            entity.HasOne(e => e.Merchant)
                .WithOne(m => m.Onboarding)
                .HasForeignKey<MerchantOnboarding>(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductOptionGroup configuration
        modelBuilder.Entity<ProductOptionGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.ProductId, e.DisplayOrder });
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OptionGroups)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductOption configuration
        modelBuilder.Entity<ProductOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExtraPrice).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.ProductOptionGroupId, e.DisplayOrder });
            
            entity.HasOne(e => e.ProductOptionGroup)
                .WithMany(pog => pog.Options)
                .HasForeignKey(e => e.ProductOptionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderLineOption configuration
        modelBuilder.Entity<OrderLineOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OptionName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExtraPrice).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.OrderLine)
                .WithMany(ol => ol.Options)
                .HasForeignKey(e => e.OrderLineId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ProductOption)
                .WithMany()
                .HasForeignKey(e => e.ProductOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RevieweeType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Comment).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ModerationNotes).HasMaxLength(500);
            entity.Property(e => e.ModeratorNotes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.RevieweeId, e.RevieweeType });
            entity.HasIndex(e => new { e.ReviewerId, e.OrderId });
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Reviewee)
                .WithMany()
                .HasForeignKey(e => e.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Moderator)
                .WithMany()
                .HasForeignKey(e => e.ModeratedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReviewTag configuration
        modelBuilder.Entity<ReviewTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tag).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.Tag);
            
            entity.HasOne(e => e.Review)
                .WithMany(r => r.ReviewTags)
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ReviewHelpful configuration
        modelBuilder.Entity<ReviewHelpful>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.VotedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
            
            entity.HasOne(e => e.Review)
                .WithMany(r => r.ReviewHelpfuls)
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReviewModerationLog configuration
        modelBuilder.Entity<ReviewModerationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ModeratedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.ReviewId);
            entity.HasIndex(e => e.ModeratorId);
            entity.HasIndex(e => e.ModeratedAt);
            
            entity.HasOne(e => e.Review)
                .WithMany()
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Moderator)
                .WithMany()
                .HasForeignKey(e => e.ModeratorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReviewReport configuration
        modelBuilder.Entity<ReviewReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Details).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.ReviewNotes).HasMaxLength(500);
            entity.Property(e => e.ReportedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.ReviewId);
            entity.HasIndex(e => e.ReporterId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ReportedAt);
            
            entity.HasOne(e => e.Review)
                .WithMany()
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Reporter)
                .WithMany()
                .HasForeignKey(e => e.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Reviewer)
                .WithMany()
                .HasForeignKey(e => e.ReviewedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReviewLike configuration
        modelBuilder.Entity<ReviewLike>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LikedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.LikedAt);
            
            entity.HasOne(e => e.Review)
                .WithMany()
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReviewBookmark configuration
        modelBuilder.Entity<ReviewBookmark>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookmarkedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.BookmarkedAt);
            
            entity.HasOne(e => e.Review)
                .WithMany()
                .HasForeignKey(e => e.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductReview configuration
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.ImageUrls).HasMaxLength(2000); // JSON array
            entity.Property(e => e.ModerationNotes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes for performance
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => new { e.ProductId, e.Rating });
            entity.HasIndex(e => new { e.ProductId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique(); // Bir kullanıcı bir ürüne sadece bir review yapabilir
            entity.HasIndex(e => e.IsApproved);
            entity.HasIndex(e => e.IsDeleted);
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Moderator)
                .WithMany()
                .HasForeignKey(e => e.ModeratedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductReviewHelpful configuration
        modelBuilder.Entity<ProductReviewHelpful>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.ProductReviewId, e.UserId }).IsUnique(); // Bir kullanıcı bir review'a sadece bir kez oy verebilir
            entity.HasIndex(e => e.ProductReviewId);
            entity.HasIndex(e => e.IsHelpful);
            
            entity.HasOne(e => e.ProductReview)
                .WithMany(r => r.ProductReviewHelpfuls)
                .HasForeignKey(e => e.ProductReviewId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Rating configuration
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.RecentAverageRating).HasPrecision(3, 2);
            entity.Property(e => e.ResponseRate).HasPrecision(5, 2);
            entity.Property(e => e.PositiveReviewRate).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.EntityId, e.EntityType }).IsUnique();
        });

        // RatingHistory configuration
        modelBuilder.Entity<RatingHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.FoodQualityRating).HasPrecision(3, 2);
            entity.Property(e => e.DeliverySpeedRating).HasPrecision(3, 2);
            entity.Property(e => e.ServiceRating).HasPrecision(3, 2);
            entity.Property(e => e.SnapshotDate).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.EntityId, e.EntityType, e.SnapshotDate });
        });

        // AuditLog Configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.Action, e.Timestamp });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Timestamp);

            // Relationship with User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SystemNotification Configuration
        modelBuilder.Entity<SystemNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TargetRoles).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.IsActive, e.Priority, e.CreatedAt });
            entity.HasIndex(e => e.TargetRoles);
            entity.HasIndex(e => e.Type);

            // Relationship with User (Creator)
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship with User (ReadByUser)
            entity.HasOne(e => e.ReadByUser)
                .WithMany()
                .HasForeignKey(e => e.ReadBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PaymentMethod).IsRequired().HasConversion<int>();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ChangeAmount).HasPrecision(18, 2);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.ExternalTransactionId).HasMaxLength(100);
            entity.Property(e => e.ExternalResponse).HasMaxLength(2000);
            entity.Property(e => e.RefundReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.CollectedByCourierId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.CollectedByCourier)
                .WithMany()
                .HasForeignKey(e => e.CollectedByCourierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CourierCashCollection configuration
        modelBuilder.Entity<CourierCashCollection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CollectedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Collected");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.CollectedAt);
            
            entity.HasOne(e => e.Payment)
                .WithMany(p => p.CourierCashCollections)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Courier)
                .WithMany()
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CashSettlement configuration
        modelBuilder.Entity<CashSettlement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Commission).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.BankTransferReference).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.MerchantId);
            entity.HasIndex(e => e.SettlementDate);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Merchant)
                .WithMany()
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ProcessedByAdmin)
                .WithMany()
                .HasForeignKey(e => e.ProcessedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CashPaymentSecurity configuration
        modelBuilder.Entity<CashPaymentSecurity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CalculatedChange).HasPrecision(18, 2);
            entity.Property(e => e.GivenChange).HasPrecision(18, 2);
            entity.Property(e => e.ChangeDifference).HasPrecision(18, 2);
            entity.Property(e => e.FakeMoneyNotes).HasMaxLength(500);
            entity.Property(e => e.IdentityVerificationType).HasMaxLength(50);
            entity.Property(e => e.IdentityNumberHash).HasMaxLength(256);
            entity.Property(e => e.RiskFactors).HasMaxLength(1000);
            entity.Property(e => e.SecurityNotes).HasMaxLength(1000);
            entity.Property(e => e.RiskLevel).IsRequired().HasConversion<int>().HasDefaultValue(Domain.Enums.SecurityRiskLevel.Low);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.PaymentId).IsUnique();
            
            entity.HasOne(e => e.Payment)
                .WithOne()
                .HasForeignKey<CashPaymentSecurity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ApprovedByAdmin)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CashPaymentEvidence configuration
        modelBuilder.Entity<CashPaymentEvidence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EvidenceType).IsRequired().HasConversion<int>();
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.VerificationNotes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.EvidenceType);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Payment)
                .WithMany()
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Courier)
                .WithMany()
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.VerifiedByAdmin)
                .WithMany()
                .HasForeignKey(e => e.VerifiedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CashPaymentAuditLog configuration
        modelBuilder.Entity<CashPaymentAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasConversion<int>();
            entity.Property(e => e.SeverityLevel).IsRequired().HasConversion<int>();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.RiskLevel).HasConversion<int>();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.SeverityLevel);
            entity.HasIndex(e => e.CreatedAt);
        });

        // OrderStatusTransitionLog configuration
        modelBuilder.Entity<OrderStatusTransitionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromStatus).IsRequired().HasConversion<int>();
            entity.Property(e => e.ToStatus).IsRequired().HasConversion<int>();
            entity.Property(e => e.ChangedByRole).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.IsRollback).HasDefaultValue(false);
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ChangedBy);
            entity.HasIndex(e => e.ChangedAt);
            entity.HasIndex(e => e.FromStatus);
            entity.HasIndex(e => e.ToStatus);
            entity.HasIndex(e => e.IsRollback);
            entity.HasIndex(e => e.ChangedByRole);
            
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ChangedByUser)
                .WithMany()
                .HasForeignKey(e => e.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.RollbackFromLog)
                .WithMany()
                .HasForeignKey(e => e.RollbackFromLogId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StockHistory configuration
        modelBuilder.Entity<StockHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeType).IsRequired().HasConversion<int>();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ProductVariantId);
            entity.HasIndex(e => e.ChangedAt);
            entity.HasIndex(e => e.ChangeType);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ChangedBy);
            
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ProductVariant)
                .WithMany()
                .HasForeignKey(e => e.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ChangedByUser)
                .WithMany()
                .HasForeignKey(e => e.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StockAlert configuration
        modelBuilder.Entity<StockAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).IsRequired().HasConversion<int>();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(2000);
            entity.Property(e => e.IsResolved).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.ProductVariantId);
            entity.HasIndex(e => e.MerchantId);
            entity.HasIndex(e => e.AlertType);
            entity.HasIndex(e => e.IsResolved);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ProductVariant)
                .WithMany()
                .HasForeignKey(e => e.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Merchant)
                .WithMany()
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ResolvedByUser)
                .WithMany()
                .HasForeignKey(e => e.ResolvedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StockSettings configuration
        modelBuilder.Entity<StockSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AutoStockReduction).HasDefaultValue(true);
            entity.Property(e => e.LowStockAlerts).HasDefaultValue(true);
            entity.Property(e => e.OverstockAlerts).HasDefaultValue(false);
            entity.Property(e => e.DefaultMinimumStock).HasDefaultValue(10);
            entity.Property(e => e.DefaultMaximumStock).HasDefaultValue(1000);
            entity.Property(e => e.EnableStockSync).HasDefaultValue(false);
            entity.Property(e => e.ExternalSystemId).HasMaxLength(100);
            entity.Property(e => e.SyncApiKey).HasMaxLength(500);
            entity.Property(e => e.SyncApiUrl).HasMaxLength(500);
            entity.Property(e => e.SyncIntervalMinutes).HasDefaultValue(60);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.MerchantId).IsUnique();
            entity.HasIndex(e => e.IsActive);
            
            entity.HasOne(e => e.Merchant)
                .WithMany()
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserActivityLog Configuration
        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ActivityDescription).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.ActivityData).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.DeviceType).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.ActivityType, e.Timestamp });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.Timestamp);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SystemChangeLog Configuration
        modelBuilder.Entity<SystemChangeLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(200);
            entity.Property(e => e.OldValues).HasMaxLength(2000);
            entity.Property(e => e.NewValues).HasMaxLength(2000);
            entity.Property(e => e.ChangedFields).HasMaxLength(2000);
            entity.Property(e => e.ChangeReason).HasMaxLength(1000);
            entity.Property(e => e.ChangeSource).HasMaxLength(100);
            entity.Property(e => e.ChangedByUserName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
            entity.HasIndex(e => new { e.ChangedByUserId, e.Timestamp });
            entity.HasIndex(e => new { e.ChangeType, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);

            entity.HasOne(e => e.ChangedByUser)
                .WithMany()
                .HasForeignKey(e => e.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SecurityEventLog Configuration
        modelBuilder.Entity<SecurityEventLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventTitle).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EventDescription).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Severity).HasMaxLength(50);
            entity.Property(e => e.RiskLevel).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserRole).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceFingerprint).HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.Property(e => e.EventData).HasMaxLength(2000);
            entity.Property(e => e.ThreatIndicators).HasMaxLength(2000);
            entity.Property(e => e.MitigationActions).HasMaxLength(1000);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.EventType, e.Timestamp });
            entity.HasIndex(e => new { e.Severity, e.RiskLevel, e.Timestamp });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.IpAddress, e.Timestamp });
            entity.HasIndex(e => new { e.IsResolved, e.RequiresInvestigation });
            entity.HasIndex(e => e.Timestamp);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LogAnalysisReport Configuration
        modelBuilder.Entity<LogAnalysisReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReportTitle).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ReportDescription).HasMaxLength(2000);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.ReportData).HasMaxLength(2000);
            entity.Property(e => e.Summary).HasMaxLength(2000);
            entity.Property(e => e.Insights).HasMaxLength(2000);
            entity.Property(e => e.Alerts).HasMaxLength(2000);
            entity.Property(e => e.Charts).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.Format).HasMaxLength(50);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(100);
            entity.Property(e => e.GeneratedByUserName).HasMaxLength(100);
            entity.Property(e => e.GeneratedByRole).HasMaxLength(100);
            entity.Property(e => e.Recipients).HasMaxLength(1000);
            entity.Property(e => e.SchedulePattern).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.ReportType, e.GeneratedAt });
            entity.HasIndex(e => new { e.Status, e.GeneratedAt });
            entity.HasIndex(e => new { e.GeneratedByUserId, e.GeneratedAt });
            entity.HasIndex(e => new { e.IsScheduled, e.NextScheduledRun });
            entity.HasIndex(e => e.GeneratedAt);

            entity.HasOne(e => e.GeneratedByUser)
                .WithMany()
                .HasForeignKey(e => e.GeneratedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Language Configuration
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NativeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CultureCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.FlagIcon).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.CultureCode).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDefault);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Translation Configuration
        modelBuilder.Entity<Translation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.LanguageCode).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Context).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Key);
            entity.HasIndex(e => e.LanguageCode);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.Key, e.LanguageCode }).IsUnique();
            entity.HasIndex(e => new { e.Category, e.LanguageCode });

            entity.HasOne(e => e.Language)
                .WithMany(l => l.Translations)
                .HasForeignKey(e => e.LanguageCode)
                .HasPrincipalKey(l => l.Code)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserLanguagePreference Configuration
        modelBuilder.Entity<UserLanguagePreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LanguageId);
            entity.HasIndex(e => e.IsPrimary);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.UserId, e.IsPrimary });
            entity.HasIndex(e => new { e.UserId, e.LanguageId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Language)
                .WithMany(l => l.UserLanguagePreferences)
                .HasForeignKey(e => e.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserNotificationPreferences Configuration
        modelBuilder.Entity<UserNotificationPreferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // String properties
            entity.Property(e => e.NotificationSound)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("default");
            
            entity.Property(e => e.Language)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("tr-TR");
            
            // TimeSpan properties - EF Core 9 native support
            entity.Property(e => e.QuietStartTime)
                .HasColumnType("TIME")
                .IsRequired(false);
            
            entity.Property(e => e.QuietEndTime)
                .HasColumnType("TIME")
                .IsRequired(false);
            
            // Timestamps
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Indexes
            entity.HasIndex(e => e.UserId).IsUnique(); // One preference per user
            
            // Note: Foreign key relationship is configured in User entity
        });

        // RateLimitRule Configuration
        modelBuilder.Entity<RateLimitRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.UserRole).HasMaxLength(100);
            entity.Property(e => e.UserTier).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Endpoint);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.UserRole);
            entity.HasIndex(e => e.UserTier);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RateLimitLog Configuration
        modelBuilder.Entity<RateLimitLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserRole).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.DeviceType).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.RequestTime).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.RateLimitRuleId);
            entity.HasIndex(e => e.Endpoint);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.IsLimitExceeded);
            entity.HasIndex(e => e.RequestTime);

            entity.HasOne(e => e.RateLimitRule)
                .WithMany(r => r.RateLimitLogs)
                .HasForeignKey(e => e.RateLimitRuleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RateLimitViolation Configuration
        modelBuilder.Entity<RateLimitViolation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserRole).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ViolationReason).HasMaxLength(500);
            entity.Property(e => e.RequestId).HasMaxLength(50);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.DeviceType).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.ResolvedBy).HasMaxLength(100);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
            entity.Property(e => e.InvestigationNotes).HasMaxLength(1000);
            entity.Property(e => e.ViolationTime).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.RateLimitRuleId);
            entity.HasIndex(e => e.Endpoint);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.IsResolved);
            entity.HasIndex(e => e.RequiresInvestigation);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.ViolationTime);

            entity.HasOne(e => e.RateLimitRule)
                .WithMany()
                .HasForeignKey(e => e.RateLimitRuleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // RateLimitConfiguration Configuration
        modelBuilder.Entity<RateLimitConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.EndpointPattern).HasMaxLength(500);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.UserRole).HasMaxLength(100);
            entity.Property(e => e.UserTier).HasMaxLength(100);
            entity.Property(e => e.IpWhitelist).HasMaxLength(2000);
            entity.Property(e => e.IpBlacklist).HasMaxLength(2000);
            entity.Property(e => e.UserWhitelist).HasMaxLength(2000);
            entity.Property(e => e.UserBlacklist).HasMaxLength(2000);
            entity.Property(e => e.AlertRecipients).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.EndpointPattern);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.UserRole);
            entity.HasIndex(e => e.UserTier);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderTracking Configuration
        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StatusMessage).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.LastUpdatedAt);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Courier)
                .WithMany()
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LocationHistory Configuration
        modelBuilder.Entity<LocationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.DeviceInfo).HasMaxLength(200);
            entity.Property(e => e.AppVersion).HasMaxLength(50);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.OrderTrackingId);
            entity.HasIndex(e => e.RecordedAt);
            entity.HasIndex(e => e.UpdateType);

            entity.HasOne(e => e.OrderTracking)
                .WithMany(t => t.LocationHistory)
                .HasForeignKey(e => e.OrderTrackingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TrackingNotification Configuration
        modelBuilder.Entity<TrackingNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Data).HasMaxLength(2000);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(50);
            entity.Property(e => e.DeliveryStatus).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.OrderTrackingId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsSent);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.OrderTracking)
                .WithMany(t => t.Notifications)
                .HasForeignKey(e => e.OrderTrackingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ETAEstimation Configuration
        modelBuilder.Entity<ETAEstimation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CalculationMethod).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.OrderTrackingId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.OrderTracking)
                .WithMany()
                .HasForeignKey(e => e.OrderTrackingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TrackingSettings Configuration
        modelBuilder.Entity<TrackingSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.MerchantId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Merchant)
                .WithMany()
                .HasForeignKey(e => e.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // FavoriteProduct configuration
        modelBuilder.Entity<FavoriteProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.AddedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.AddedAt);
            entity.HasIndex(e => new { e.UserId, e.AddedAt }); // Pagination optimize
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade cycle
        });
        
        // CourierLocation configuration
        modelBuilder.Entity<CourierLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.CourierId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.CourierId, e.Timestamp });
            
            entity.HasOne(e => e.Courier)
                .WithMany()
                .HasForeignKey(e => e.CourierId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // LoyaltyPointTransaction configuration
        modelBuilder.Entity<LoyaltyPointTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
