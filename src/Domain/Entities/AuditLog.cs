using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Audit log entity for tracking system activities
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
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

