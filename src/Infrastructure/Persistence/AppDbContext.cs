using Getir.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Getir.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Courier> Couriers { get; set; }
    public DbSet<UserLoyaltyPoint> UserLoyaltyPoints { get; set; }
    public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<DeliveryZone> DeliveryZones { get; set; }
    public DbSet<ProductOptionGroup> ProductOptionGroups { get; set; }
    public DbSet<ProductOption> ProductOptions { get; set; }
    public DbSet<OrderLineOption> OrderLineOptions { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewTag> ReviewTags { get; set; }
    public DbSet<ReviewHelpful> ReviewHelpfuls { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<RatingHistory> RatingHistories { get; set; }
    public DbSet<DeliveryZonePoint> DeliveryZonePoints { get; set; }
    public DbSet<MerchantOnboarding> MerchantOnboardings { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<SystemNotification> SystemNotifications { get; set; }

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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

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
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
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
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
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
        });
    }
}
