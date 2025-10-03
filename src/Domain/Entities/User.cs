using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer; // Default role
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Merchant> OwnedMerchants { get; set; } = new List<Merchant>();
    public virtual UserNotificationPreferences? NotificationPreferences { get; set; }
    public virtual ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    public virtual ICollection<ReviewHelpful> ReviewHelpfuls { get; set; } = new List<ReviewHelpful>();
    public virtual ICollection<SystemNotification> CreatedSystemNotifications { get; set; } = new List<SystemNotification>();
    public virtual ICollection<SystemNotification> ReadSystemNotifications { get; set; } = new List<SystemNotification>();
}
