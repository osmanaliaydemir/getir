using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class RateLimitRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public RateLimitType Type { get; set; }
    public string? Endpoint { get; set; } // For endpoint-based limiting
    public string? HttpMethod { get; set; } // GET, POST, PUT, DELETE, etc.
    public int RequestLimit { get; set; } // Maximum requests allowed
    public RateLimitPeriod Period { get; set; } // Time period for the limit
    public RateLimitAction Action { get; set; } // What to do when limit exceeded
    public int? ThrottleDelayMs { get; set; } // Delay in milliseconds for throttling
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher priority rules are applied first
    public string? UserRole { get; set; } // For role-based limiting
    public string? UserTier { get; set; } // For tier-based limiting (free, premium, etc.)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual ICollection<RateLimitLog> RateLimitLogs { get; set; } = new List<RateLimitLog>();
}
