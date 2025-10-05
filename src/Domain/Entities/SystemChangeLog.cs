using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Sistem değişiklik log'u için entity
/// </summary>
public class SystemChangeLog
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ChangeType { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, CONFIG_CHANGE, etc.
    
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty; // User, Merchant, Order, SystemConfig, etc.
    
    [Required]
    [MaxLength(50)]
    public string EntityId { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? EntityName { get; set; } // Human readable name
    
    [MaxLength(2000)]
    public string? OldValues { get; set; } // JSON format - previous values
    
    [MaxLength(2000)]
    public string? NewValues { get; set; } // JSON format - new values
    
    [MaxLength(2000)]
    public string? ChangedFields { get; set; } // JSON array of changed field names
    
    [MaxLength(1000)]
    public string? ChangeReason { get; set; } // Reason for the change
    
    [MaxLength(100)]
    public string? ChangeSource { get; set; } // API, Admin Panel, System, etc.
    
    public Guid? ChangedByUserId { get; set; }
    
    [MaxLength(100)]
    public string? ChangedByUserName { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    [MaxLength(50)]
    public string? RequestId { get; set; }
    
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsSuccess { get; set; } = true;
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    [MaxLength(50)]
    public string? Severity { get; set; } = "INFO"; // INFO, WARNING, ERROR, CRITICAL
    
    // Navigation properties
    public virtual User? ChangedByUser { get; set; }
}
