using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Kullanıcı aktivite log'u için entity
/// </summary>
public class UserActivityLog
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ActivityType { get; set; } = string.Empty; // LOGIN, LOGOUT, VIEW, SEARCH, etc.
    
    [Required]
    [MaxLength(200)]
    public string ActivityDescription { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? EntityType { get; set; } // Order, Product, Merchant, etc.
    
    [MaxLength(50)]
    public string? EntityId { get; set; }
    
    [MaxLength(2000)]
    public string? ActivityData { get; set; } // JSON format
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    [MaxLength(50)]
    public string? RequestId { get; set; }
    
    [MaxLength(100)]
    public string? DeviceType { get; set; } // Mobile, Desktop, Tablet
    
    [MaxLength(100)]
    public string? Browser { get; set; }
    
    [MaxLength(100)]
    public string? OperatingSystem { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    [MaxLength(200)]
    public string? Location { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public int Duration { get; set; } // Activity duration in seconds
    
    public bool IsSuccess { get; set; } = true;
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = default!;
}
