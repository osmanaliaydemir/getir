using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Audit log entity for tracking system activities
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, etc.
    
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty; // User, Merchant, Order, etc.
    
    [Required]
    [MaxLength(50)]
    public string EntityId { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Details { get; set; } // JSON or description of changes
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Additional context
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    [MaxLength(50)]
    public string? RequestId { get; set; }
    
    public bool IsSuccess { get; set; } = true;
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual User? User { get; set; }
}

/// <summary>
/// System notification entity for admin notifications
/// </summary>
public class SystemNotification
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // INFO, WARNING, ERROR, SUCCESS
    
    [Required]
    [MaxLength(100)]
    public string TargetRoles { get; set; } = string.Empty; // Comma-separated roles
    
    public Guid? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High, 4=Critical
    
    // Navigation properties
    public virtual User? Creator { get; set; }
}
