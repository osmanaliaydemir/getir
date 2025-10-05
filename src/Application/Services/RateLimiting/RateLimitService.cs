using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RateLimiting;

public class RateLimitService : IRateLimitService
{
    private readonly List<RateLimitRuleDto> _mockRules;
    private readonly Dictionary<string, int> _requestCounts;
    private readonly Dictionary<string, DateTime> _lastRequestTimes;

    public RateLimitService()
    {
        _requestCounts = new Dictionary<string, int>();
        _lastRequestTimes = new Dictionary<string, DateTime>();
        
        // Mock rate limit rules
        _mockRules = new List<RateLimitRuleDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "API General Limit",
                Description = "General API rate limit",
                Type = RateLimitType.Global,
                RequestLimit = 1000,
                Period = RateLimitPeriod.PerHour,
                Action = RateLimitAction.Throttle,
                ThrottleDelayMs = 100,
                IsActive = true,
                Priority = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Login Endpoint Limit",
                Description = "Rate limit for login endpoint",
                Type = RateLimitType.Endpoint,
                Endpoint = "/api/auth/login",
                HttpMethod = "POST",
                RequestLimit = 5,
                Period = RateLimitPeriod.PerMinute,
                Action = RateLimitAction.Block,
                IsActive = true,
                Priority = 10
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "User API Limit",
                Description = "Per user API rate limit",
                Type = RateLimitType.User,
                RequestLimit = 100,
                Period = RateLimitPeriod.PerMinute,
                Action = RateLimitAction.Throttle,
                ThrottleDelayMs = 50,
                IsActive = true,
                Priority = 5
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "IP Rate Limit",
                Description = "Per IP rate limit",
                Type = RateLimitType.IP,
                RequestLimit = 200,
                Period = RateLimitPeriod.PerMinute,
                Action = RateLimitAction.Throttle,
                ThrottleDelayMs = 25,
                IsActive = true,
                Priority = 3
            }
        };
    }

    public Task<RateLimitCheckResponse> CheckRateLimitAsync(RateLimitCheckRequest request)
    {
        var response = new RateLimitCheckResponse
        {
            IsAllowed = true,
            IsLimitExceeded = false,
            RequestCount = 1,
            RequestLimit = 1000,
            Period = RateLimitPeriod.PerHour,
            Action = RateLimitAction.Allow,
            Headers = new Dictionary<string, string>()
        };

        // Find applicable rules
        var applicableRules = _mockRules
            .Where(r => r.IsActive && IsRuleApplicable(r, request))
            .OrderByDescending(r => r.Priority)
            .ToList();

        if (!applicableRules.Any())
        {
            return Task.FromResult(response);
        }

        var rule = applicableRules.First();
        var key = GenerateKey(rule, request);
        
        // Check current count
        var currentCount = GetCurrentCount(key, rule.Period);
        response.RequestCount = currentCount;
        response.RequestLimit = rule.RequestLimit;
        response.Period = rule.Period;
        response.Action = rule.Action;

        if (currentCount >= rule.RequestLimit)
        {
            response.IsLimitExceeded = true;
            response.IsAllowed = rule.Action != RateLimitAction.Block;
            response.Reason = $"Rate limit exceeded for {rule.Type.GetDisplayName()}";
            
            if (rule.Action == RateLimitAction.Throttle && rule.ThrottleDelayMs.HasValue)
            {
                response.ThrottleDelayMs = rule.ThrottleDelayMs.Value;
            }
            
            if (rule.Action == RateLimitAction.Block)
            {
                response.RetryAfter = DateTime.UtcNow.Add(rule.Period.GetTimeSpan());
                response.BlockedUntil = response.RetryAfter?.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
        }

        // Update count
        IncrementCount(key);

        // Add rate limit headers
        response.Headers["X-RateLimit-Limit"] = rule.RequestLimit.ToString();
        response.Headers["X-RateLimit-Remaining"] = Math.Max(0, rule.RequestLimit - currentCount - 1).ToString();
        response.Headers["X-RateLimit-Reset"] = DateTime.UtcNow.Add(rule.Period.GetTimeSpan()).ToString("yyyy-MM-ddTHH:mm:ssZ");

        return Task.FromResult(response);
    }

    public Task<bool> IsAllowedAsync(string endpoint, string httpMethod, string? userId = null, string? ipAddress = null)
    {
        var request = new RateLimitCheckRequest
        {
            Endpoint = endpoint,
            HttpMethod = httpMethod,
            UserId = userId,
            IpAddress = ipAddress
        };

        var response = CheckRateLimitAsync(request).Result;
        return Task.FromResult(response.IsAllowed);
    }

    public Task<RateLimitCheckResponse> GetRateLimitStatusAsync(string endpoint, string httpMethod, string? userId = null, string? ipAddress = null)
    {
        var request = new RateLimitCheckRequest
        {
            Endpoint = endpoint,
            HttpMethod = httpMethod,
            UserId = userId,
            IpAddress = ipAddress
        };

        return CheckRateLimitAsync(request);
    }

    public Task LogRequestAsync(RateLimitCheckRequest request, RateLimitCheckResponse response)
    {
        // Mock logging - in real implementation, this would save to database
        return Task.CompletedTask;
    }

    public Task<List<RateLimitRuleDto>> GetActiveRulesAsync()
    {
        return Task.FromResult(_mockRules.Where(r => r.IsActive).ToList());
    }

    public Task<RateLimitRuleDto?> GetRuleByIdAsync(Guid id)
    {
        return Task.FromResult(_mockRules.FirstOrDefault(r => r.Id == id));
    }

    public Task<RateLimitRuleDto> CreateRuleAsync(CreateRateLimitRuleRequest request)
    {
        var rule = new RateLimitRuleDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Endpoint = request.Endpoint,
            HttpMethod = request.HttpMethod,
            RequestLimit = request.RequestLimit,
            Period = request.Period,
            Action = request.Action,
            ThrottleDelayMs = request.ThrottleDelayMs,
            IsActive = true,
            Priority = request.Priority,
            UserRole = request.UserRole,
            UserTier = request.UserTier
        };

        _mockRules.Add(rule);
        return Task.FromResult(rule);
    }

    public Task<RateLimitRuleDto> UpdateRuleAsync(Guid id, UpdateRateLimitRuleRequest request)
    {
        var rule = _mockRules.FirstOrDefault(r => r.Id == id);
        if (rule == null)
        {
            throw new ArgumentException("Rule not found");
        }

        rule.Name = request.Name;
        rule.Description = request.Description;
        rule.Endpoint = request.Endpoint;
        rule.HttpMethod = request.HttpMethod;
        rule.RequestLimit = request.RequestLimit;
        rule.Period = request.Period;
        rule.Action = request.Action;
        rule.ThrottleDelayMs = request.ThrottleDelayMs;
        rule.IsActive = request.IsActive;
        rule.Priority = request.Priority;
        rule.UserRole = request.UserRole;
        rule.UserTier = request.UserTier;

        return Task.FromResult(rule);
    }

    public Task<bool> DeleteRuleAsync(Guid id)
    {
        var rule = _mockRules.FirstOrDefault(r => r.Id == id);
        if (rule != null)
        {
            _mockRules.Remove(rule);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> EnableRuleAsync(Guid id)
    {
        var rule = _mockRules.FirstOrDefault(r => r.Id == id);
        if (rule != null)
        {
            rule.IsActive = true;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DisableRuleAsync(Guid id)
    {
        var rule = _mockRules.FirstOrDefault(r => r.Id == id);
        if (rule != null)
        {
            rule.IsActive = false;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<RateLimitStatisticsDto>> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var statistics = new List<RateLimitStatisticsDto>
        {
            new()
            {
                Type = RateLimitType.Global,
                TypeName = "Global",
                TotalRequests = 15000,
                BlockedRequests = 150,
                ThrottledRequests = 300,
                Violations = 450,
                BlockRate = 1.0,
                ThrottleRate = 2.0,
                ViolationRate = 3.0,
                PeriodStart = startDate,
                PeriodEnd = endDate
            },
            new()
            {
                Type = RateLimitType.Endpoint,
                TypeName = "Endpoint",
                TotalRequests = 8000,
                BlockedRequests = 80,
                ThrottledRequests = 160,
                Violations = 240,
                BlockRate = 1.0,
                ThrottleRate = 2.0,
                ViolationRate = 3.0,
                PeriodStart = startDate,
                PeriodEnd = endDate
            },
            new()
            {
                Type = RateLimitType.User,
                TypeName = "User",
                TotalRequests = 5000,
                BlockedRequests = 50,
                ThrottledRequests = 100,
                Violations = 150,
                BlockRate = 1.0,
                ThrottleRate = 2.0,
                ViolationRate = 3.0,
                PeriodStart = startDate,
                PeriodEnd = endDate
            },
            new()
            {
                Type = RateLimitType.IP,
                TypeName = "IP Address",
                TotalRequests = 3000,
                BlockedRequests = 30,
                ThrottledRequests = 60,
                Violations = 90,
                BlockRate = 1.0,
                ThrottleRate = 2.0,
                ViolationRate = 3.0,
                PeriodStart = startDate,
                PeriodEnd = endDate
            }
        };

        return Task.FromResult(statistics);
    }

    public Task<RateLimitSearchResponse> SearchLogsAsync(RateLimitSearchRequest request)
    {
        // Mock search results
        var logs = new List<RateLimitLogDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Endpoint = "/api/auth/login",
                HttpMethod = "POST",
                UserId = "user123",
                UserName = "testuser",
                IpAddress = "192.168.1.1",
                Type = RateLimitType.Endpoint,
                Action = RateLimitAction.Block,
                RequestCount = 6,
                RequestLimit = 5,
                Period = RateLimitPeriod.PerMinute,
                IsLimitExceeded = true,
                Reason = "Rate limit exceeded",
                RequestTime = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        var response = new RateLimitSearchResponse
        {
            Logs = logs,
            TotalCount = logs.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = 1
        };

        return Task.FromResult(response);
    }

    public Task<RateLimitViolationSearchResponse> SearchViolationsAsync(RateLimitViolationSearchRequest request)
    {
        // Mock violation results
        var violations = new List<RateLimitViolationDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Endpoint = "/api/auth/login",
                HttpMethod = "POST",
                UserId = "user123",
                UserName = "testuser",
                IpAddress = "192.168.1.1",
                Type = RateLimitType.Endpoint,
                Action = RateLimitAction.Block,
                RequestCount = 6,
                RequestLimit = 5,
                Period = RateLimitPeriod.PerMinute,
                ViolationReason = "Rate limit exceeded",
                ViolationTime = DateTime.UtcNow.AddMinutes(-5),
                IsResolved = false,
                Severity = 3,
                RequiresInvestigation = true
            }
        };

        var response = new RateLimitViolationSearchResponse
        {
            Violations = violations,
            TotalCount = violations.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = 1
        };

        return Task.FromResult(response);
    }

    public Task<bool> ResolveViolationAsync(Guid violationId, ResolveViolationRequest request)
    {
        // Mock resolution
        return Task.FromResult(true);
    }

    public Task<bool> ClearRateLimitCacheAsync(string? endpoint = null, string? userId = null, string? ipAddress = null)
    {
        // Mock cache clearing
        return Task.FromResult(true);
    }

    private bool IsRuleApplicable(RateLimitRuleDto rule, RateLimitCheckRequest request)
    {
        return rule.Type switch
        {
            RateLimitType.Global => true,
            RateLimitType.Endpoint => rule.Endpoint == request.Endpoint && 
                                    (string.IsNullOrEmpty(rule.HttpMethod) || rule.HttpMethod == request.HttpMethod),
            RateLimitType.User => !string.IsNullOrEmpty(request.UserId) && 
                                (string.IsNullOrEmpty(rule.UserRole) || rule.UserRole == request.UserRole),
            RateLimitType.IP => !string.IsNullOrEmpty(request.IpAddress),
            _ => false
        };
    }

    private string GenerateKey(RateLimitRuleDto rule, RateLimitCheckRequest request)
    {
        return rule.Type switch
        {
            RateLimitType.Global => "global",
            RateLimitType.Endpoint => $"endpoint:{request.Endpoint}:{request.HttpMethod}",
            RateLimitType.User => $"user:{request.UserId}",
            RateLimitType.IP => $"ip:{request.IpAddress}",
            _ => "unknown"
        };
    }

    private int GetCurrentCount(string key, RateLimitPeriod period)
    {
        var now = DateTime.UtcNow;
        var periodStart = now.Subtract(period.GetTimeSpan());
        
        if (_lastRequestTimes.TryGetValue(key, out var lastRequest) && lastRequest < periodStart)
        {
            _requestCounts[key] = 0;
        }

        return _requestCounts.GetValueOrDefault(key, 0);
    }

    private void IncrementCount(string key)
    {
        _requestCounts[key] = _requestCounts.GetValueOrDefault(key, 0) + 1;
        _lastRequestTimes[key] = DateTime.UtcNow;
    }
}
