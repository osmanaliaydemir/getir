using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class RateLimitLog
{
    public Guid Id { get; set; }
    public Guid? RateLimitRuleId { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? UserId { get; set; } // Can be string for flexibility
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public RateLimitType Type { get; set; }
    public RateLimitAction Action { get; set; }
    public int RequestCount { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public bool IsLimitExceeded { get; set; }
    public string? Reason { get; set; } // Reason for blocking/throttling
    public DateTime RequestTime { get; set; }
    public DateTime? BlockedUntil { get; set; } // When the block expires
    public string? RequestId { get; set; } // For correlation
    public string? SessionId { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }

    // Navigation properties
    public virtual RateLimitRule? RateLimitRule { get; set; }
}
