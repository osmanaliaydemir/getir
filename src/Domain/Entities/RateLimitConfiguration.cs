using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class RateLimitConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public RateLimitType Type { get; set; }
    public string? EndpointPattern { get; set; } // Regex pattern for endpoints
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
    public string? IpWhitelist { get; set; } // Comma-separated IP addresses
    public string? IpBlacklist { get; set; } // Comma-separated IP addresses
    public string? UserWhitelist { get; set; } // Comma-separated user IDs
    public string? UserBlacklist { get; set; } // Comma-separated user IDs
    public bool EnableLogging { get; set; } = true;
    public bool EnableAlerting { get; set; } = false;
    public int? AlertThreshold { get; set; } // Alert when violations exceed this count
    public string? AlertRecipients { get; set; } // Comma-separated email addresses
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
}
