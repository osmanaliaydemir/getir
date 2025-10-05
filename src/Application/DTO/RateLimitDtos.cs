using Getir.Domain.Enums;

namespace Getir.Application.DTO;

public class RateLimitRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public RateLimitType Type { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
}

public class CreateRateLimitRuleRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public RateLimitType Type { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public int Priority { get; set; } = 0;
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
}

public class UpdateRateLimitRuleRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
}

public class RateLimitLogDto
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
    public bool IsLimitExceeded { get; set; }
    public string? Reason { get; set; }
    public DateTime RequestTime { get; set; }
    public DateTime? BlockedUntil { get; set; }
    public string? RequestId { get; set; }
    public string? SessionId { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
}

public class RateLimitViolationDto
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
    public bool IsResolved { get; set; }
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
    public int Severity { get; set; }
    public bool RequiresInvestigation { get; set; }
    public string? InvestigationNotes { get; set; }
}

public class RateLimitConfigurationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public RateLimitType Type { get; set; }
    public string? EndpointPattern { get; set; }
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
    public string? IpWhitelist { get; set; }
    public string? IpBlacklist { get; set; }
    public string? UserWhitelist { get; set; }
    public string? UserBlacklist { get; set; }
    public bool EnableLogging { get; set; }
    public bool EnableAlerting { get; set; }
    public int? AlertThreshold { get; set; }
    public string? AlertRecipients { get; set; }
}

public class CreateRateLimitConfigurationRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public RateLimitType Type { get; set; }
    public string? EndpointPattern { get; set; }
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public int Priority { get; set; } = 0;
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
    public string? IpWhitelist { get; set; }
    public string? IpBlacklist { get; set; }
    public string? UserWhitelist { get; set; }
    public string? UserBlacklist { get; set; }
    public bool EnableLogging { get; set; } = true;
    public bool EnableAlerting { get; set; } = false;
    public int? AlertThreshold { get; set; }
    public string? AlertRecipients { get; set; }
}

public class UpdateRateLimitConfigurationRequest
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? EndpointPattern { get; set; }
    public string? HttpMethod { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
    public string? IpWhitelist { get; set; }
    public string? IpBlacklist { get; set; }
    public string? UserWhitelist { get; set; }
    public string? UserBlacklist { get; set; }
    public bool EnableLogging { get; set; }
    public bool EnableAlerting { get; set; }
    public int? AlertThreshold { get; set; }
    public string? AlertRecipients { get; set; }
}

public class RateLimitCheckRequest
{
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? UserId { get; set; }
    public string? UserRole { get; set; }
    public string? UserTier { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestId { get; set; }
    public string? SessionId { get; set; }
}

public class RateLimitCheckResponse
{
    public bool IsAllowed { get; set; }
    public bool IsLimitExceeded { get; set; }
    public int RequestCount { get; set; }
    public int RequestLimit { get; set; }
    public RateLimitPeriod Period { get; set; }
    public RateLimitAction Action { get; set; }
    public string? Reason { get; set; }
    public DateTime? RetryAfter { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public string? BlockedUntil { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class RateLimitStatisticsDto
{
    public RateLimitType Type { get; set; }
    public string TypeName { get; set; } = default!;
    public int TotalRequests { get; set; }
    public int BlockedRequests { get; set; }
    public int ThrottledRequests { get; set; }
    public int Violations { get; set; }
    public double BlockRate { get; set; }
    public double ThrottleRate { get; set; }
    public double ViolationRate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class RateLimitSearchRequest
{
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? UserId { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public RateLimitType? Type { get; set; }
    public RateLimitAction? Action { get; set; }
    public bool? IsLimitExceeded { get; set; }
    public bool? IsResolved { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RateLimitSearchResponse
{
    public List<RateLimitLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class RateLimitViolationSearchRequest
{
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? UserId { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public RateLimitType? Type { get; set; }
    public RateLimitAction? Action { get; set; }
    public bool? IsResolved { get; set; }
    public bool? RequiresInvestigation { get; set; }
    public int? MinSeverity { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RateLimitViolationSearchResponse
{
    public List<RateLimitViolationDto> Violations { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ResolveViolationRequest
{
    public string ResolutionNotes { get; set; } = default!;
    public bool RequiresInvestigation { get; set; } = false;
    public string? InvestigationNotes { get; set; }
}
