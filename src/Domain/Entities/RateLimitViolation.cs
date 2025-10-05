using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class RateLimitViolation
{
    public Guid Id { get; set; }
    public Guid? RateLimitRuleId { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public RateLimitType Type { get; set; }
    public RateLimitAction Action { get; set; }
    public int RequestCount { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public string? ViolationReason { get; set; }
    public DateTime ViolationTime { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? RequestId { get; set; }
    public string? SessionId { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public int Severity { get; set; } = 1; // 1=Low, 2=Medium, 3=High, 4=Critical
    public bool RequiresInvestigation { get; set; } = false;
    public string? InvestigationNotes { get; set; }

    // Navigation properties
    public virtual RateLimitRule? RateLimitRule { get; set; }
}
