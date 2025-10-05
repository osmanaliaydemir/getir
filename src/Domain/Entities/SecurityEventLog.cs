using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Güvenlik event log'u için entity
/// </summary>
public class SecurityEventLog
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty; // LOGIN_FAILED, SUSPICIOUS_ACTIVITY, DATA_BREACH, etc.
    
    [Required]
    [MaxLength(200)]
    public string EventTitle { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string EventDescription { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Severity { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
    
    [MaxLength(50)]
    public string? RiskLevel { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH, CRITICAL
    
    public Guid? UserId { get; set; }
    
    [MaxLength(100)]
    public string? UserName { get; set; }
    
    [MaxLength(100)]
    public string? UserRole { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    [MaxLength(100)]
    public string? DeviceFingerprint { get; set; }
    
    [MaxLength(100)]
    public string? SessionId { get; set; }
    
    [MaxLength(50)]
    public string? RequestId { get; set; }
    
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    [MaxLength(2000)]
    public string? EventData { get; set; } // JSON format - additional event data
    
    [MaxLength(2000)]
    public string? ThreatIndicators { get; set; } // JSON format - threat indicators
    
    [MaxLength(1000)]
    public string? MitigationActions { get; set; } // Actions taken to mitigate the threat
    
    [MaxLength(100)]
    public string? Source { get; set; } // API, Admin Panel, System, External, etc.
    
    [MaxLength(100)]
    public string? Category { get; set; } // Authentication, Authorization, Data Access, etc.
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    [MaxLength(200)]
    public string? Location { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsResolved { get; set; } = false;
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string? ResolvedBy { get; set; }
    
    [MaxLength(1000)]
    public string? ResolutionNotes { get; set; }
    
    public bool RequiresInvestigation { get; set; } = false;
    
    public bool IsFalsePositive { get; set; } = false;
    
    // Navigation properties
    public virtual User? User { get; set; }
}
