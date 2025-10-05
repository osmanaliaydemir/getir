using Microsoft.AspNetCore.Mvc;
using Getir.Application.DTO;
using Getir.Application.Services.RateLimiting;
using Getir.Domain.Enums;

namespace Getir.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RateLimitController : ControllerBase
{
    private readonly IRateLimitService _rateLimitService;
    private readonly IRateLimitConfigurationService _configurationService;
    private readonly IRateLimitMonitoringService _monitoringService;

    public RateLimitController(
        IRateLimitService rateLimitService,
        IRateLimitConfigurationService configurationService,
        IRateLimitMonitoringService monitoringService)
    {
        _rateLimitService = rateLimitService;
        _configurationService = configurationService;
        _monitoringService = monitoringService;
    }

    #region Rate Limit Rules

    [HttpGet("rules")]
    public async Task<ActionResult<List<RateLimitRuleDto>>> GetAllRules()
    {
        var rules = await _rateLimitService.GetActiveRulesAsync();
        return Ok(rules);
    }

    [HttpGet("rules/{id:guid}")]
    public async Task<ActionResult<RateLimitRuleDto>> GetRuleById(Guid id)
    {
        var rule = await _rateLimitService.GetRuleByIdAsync(id);
        if (rule == null)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return Ok(rule);
    }

    [HttpPost("rules")]
    public async Task<ActionResult<RateLimitRuleDto>> CreateRule([FromBody] CreateRateLimitRuleRequest request)
    {
        var rule = await _rateLimitService.CreateRuleAsync(request);
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    [HttpPut("rules/{id:guid}")]
    public async Task<ActionResult<RateLimitRuleDto>> UpdateRule(Guid id, [FromBody] UpdateRateLimitRuleRequest request)
    {
        var rule = await _rateLimitService.UpdateRuleAsync(id, request);
        return Ok(rule);
    }

    [HttpDelete("rules/{id:guid}")]
    public async Task<ActionResult> DeleteRule(Guid id)
    {
        var result = await _rateLimitService.DeleteRuleAsync(id);
        if (!result)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return NoContent();
    }

    [HttpPost("rules/{id:guid}/enable")]
    public async Task<ActionResult> EnableRule(Guid id)
    {
        var result = await _rateLimitService.EnableRuleAsync(id);
        if (!result)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return Ok(new { Message = "Rule enabled successfully" });
    }

    [HttpPost("rules/{id:guid}/disable")]
    public async Task<ActionResult> DisableRule(Guid id)
    {
        var result = await _rateLimitService.DisableRuleAsync(id);
        if (!result)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return Ok(new { Message = "Rule disabled successfully" });
    }

    #endregion

    #region Rate Limit Check

    [HttpPost("check")]
    public async Task<ActionResult<RateLimitCheckResponse>> CheckRateLimit([FromBody] RateLimitCheckRequest request)
    {
        var response = await _rateLimitService.CheckRateLimitAsync(request);
        return Ok(response);
    }

    [HttpGet("status")]
    public async Task<ActionResult<RateLimitCheckResponse>> GetRateLimitStatus(
        [FromQuery] string endpoint,
        [FromQuery] string httpMethod = "GET",
        [FromQuery] string? userId = null,
        [FromQuery] string? ipAddress = null)
    {
        var response = await _rateLimitService.GetRateLimitStatusAsync(endpoint, httpMethod, userId, ipAddress);
        return Ok(response);
    }

    [HttpPost("cache/clear")]
    public async Task<ActionResult> ClearRateLimitCache(
        [FromQuery] string? endpoint = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? ipAddress = null)
    {
        var result = await _rateLimitService.ClearRateLimitCacheAsync(endpoint, userId, ipAddress);
        return Ok(new { Message = "Rate limit cache cleared successfully" });
    }

    #endregion

    #region Rate Limit Logs

    [HttpPost("logs/search")]
    public async Task<ActionResult<RateLimitSearchResponse>> SearchLogs([FromBody] RateLimitSearchRequest request)
    {
        var response = await _rateLimitService.SearchLogsAsync(request);
        return Ok(response);
    }

    [HttpGet("logs/recent")]
    public async Task<ActionResult<List<RateLimitLogDto>>> GetRecentLogs([FromQuery] int count = 10)
    {
        var request = new RateLimitSearchRequest
        {
            Page = 1,
            PageSize = count
        };
        
        var response = await _rateLimitService.SearchLogsAsync(request);
        return Ok(response.Logs);
    }

    #endregion

    #region Rate Limit Violations

    [HttpPost("violations/search")]
    public async Task<ActionResult<RateLimitViolationSearchResponse>> SearchViolations([FromBody] RateLimitViolationSearchRequest request)
    {
        var response = await _rateLimitService.SearchViolationsAsync(request);
        return Ok(response);
    }

    [HttpGet("violations/recent")]
    public async Task<ActionResult<List<RateLimitViolationDto>>> GetRecentViolations([FromQuery] int count = 10)
    {
        var violations = await _monitoringService.GetRecentViolationsAsync(count);
        return Ok(violations);
    }

    [HttpGet("violations/unresolved")]
    public async Task<ActionResult<List<RateLimitViolationDto>>> GetUnresolvedViolations()
    {
        var violations = await _monitoringService.GetUnresolvedViolationsAsync();
        return Ok(violations);
    }

    [HttpGet("violations/high-severity")]
    public async Task<ActionResult<List<RateLimitViolationDto>>> GetHighSeverityViolations()
    {
        var violations = await _monitoringService.GetHighSeverityViolationsAsync();
        return Ok(violations);
    }

    [HttpPost("violations/{id:guid}/resolve")]
    public async Task<ActionResult> ResolveViolation(Guid id, [FromBody] ResolveViolationRequest request)
    {
        var result = await _rateLimitService.ResolveViolationAsync(id, request);
        if (!result)
            return NotFound(new { Error = "Violation not found", ErrorCode = "VIOLATION_NOT_FOUND" });

        return Ok(new { Message = "Violation resolved successfully" });
    }

    #endregion

    #region Rate Limit Statistics

    [HttpGet("statistics")]
    public async Task<ActionResult<List<RateLimitStatisticsDto>>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] RateLimitType? type = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var statistics = await _monitoringService.GetStatisticsAsync(start, end, type);
        return Ok(statistics);
    }

    [HttpGet("statistics/endpoint/{endpoint}")]
    public async Task<ActionResult<RateLimitStatisticsDto>> GetEndpointStatistics(
        string endpoint,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var statistics = await _monitoringService.GetEndpointStatisticsAsync(endpoint, start, end);
        return Ok(statistics);
    }

    [HttpGet("statistics/user/{userId}")]
    public async Task<ActionResult<RateLimitStatisticsDto>> GetUserStatistics(
        string userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var statistics = await _monitoringService.GetUserStatisticsAsync(userId, start, end);
        return Ok(statistics);
    }

    [HttpGet("statistics/ip/{ipAddress}")]
    public async Task<ActionResult<RateLimitStatisticsDto>> GetIpStatistics(
        string ipAddress,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var statistics = await _monitoringService.GetIpStatisticsAsync(ipAddress, start, end);
        return Ok(statistics);
    }

    #endregion

    #region Top Violators

    [HttpGet("top-violators/endpoints")]
    public async Task<ActionResult<Dictionary<string, int>>> GetTopViolatingEndpoints(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int count = 10)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var topEndpoints = await _monitoringService.GetTopViolatingEndpointsAsync(start, end, count);
        return Ok(topEndpoints);
    }

    [HttpGet("top-violators/users")]
    public async Task<ActionResult<Dictionary<string, int>>> GetTopViolatingUsers(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int count = 10)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var topUsers = await _monitoringService.GetTopViolatingUsersAsync(start, end, count);
        return Ok(topUsers);
    }

    [HttpGet("top-violators/ips")]
    public async Task<ActionResult<Dictionary<string, int>>> GetTopViolatingIps(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int count = 10)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        
        var topIps = await _monitoringService.GetTopViolatingIpsAsync(start, end, count);
        return Ok(topIps);
    }

    #endregion

    #region Real-time Monitoring

    [HttpGet("logs/realtime")]
    public async Task<ActionResult<List<RateLimitLogDto>>> GetRealTimeLogs([FromQuery] int count = 100)
    {
        var logs = await _monitoringService.GetRealTimeLogsAsync(count);
        return Ok(logs);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<Dictionary<string, object>>> GetDashboardData()
    {
        var dashboardData = await _monitoringService.GetDashboardDataAsync();
        return Ok(dashboardData);
    }

    [HttpPost("alerts/send")]
    public async Task<ActionResult> SendAlert([FromBody] SendAlertRequest request)
    {
        var result = await _monitoringService.SendAlertAsync(request.Message, request.Recipients);
        return Ok(new { Message = "Alert sent successfully" });
    }

    [HttpPost("alerts/check-thresholds")]
    public async Task<ActionResult> CheckAlertThresholds()
    {
        var result = await _monitoringService.CheckAlertThresholdsAsync();
        return Ok(new { Message = "Alert thresholds checked successfully" });
    }

    #endregion

    #region Rate Limit Configurations

    [HttpGet("configurations")]
    public async Task<ActionResult<List<RateLimitConfigurationDto>>> GetAllConfigurations()
    {
        var configurations = await _configurationService.GetAllConfigurationsAsync();
        return Ok(configurations);
    }

    [HttpGet("configurations/{id:guid}")]
    public async Task<ActionResult<RateLimitConfigurationDto>> GetConfigurationById(Guid id)
    {
        var configuration = await _configurationService.GetConfigurationByIdAsync(id);
        if (configuration == null)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(configuration);
    }

    [HttpPost("configurations")]
    public async Task<ActionResult<RateLimitConfigurationDto>> CreateConfiguration([FromBody] CreateRateLimitConfigurationRequest request)
    {
        var configuration = await _configurationService.CreateConfigurationAsync(request);
        return CreatedAtAction(nameof(GetConfigurationById), new { id = configuration.Id }, configuration);
    }

    [HttpPut("configurations/{id:guid}")]
    public async Task<ActionResult<RateLimitConfigurationDto>> UpdateConfiguration(Guid id, [FromBody] UpdateRateLimitConfigurationRequest request)
    {
        var configuration = await _configurationService.UpdateConfigurationAsync(id, request);
        return Ok(configuration);
    }

    [HttpDelete("configurations/{id:guid}")]
    public async Task<ActionResult> DeleteConfiguration(Guid id)
    {
        var result = await _configurationService.DeleteConfigurationAsync(id);
        if (!result)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return NoContent();
    }

    [HttpPost("configurations/{id:guid}/enable")]
    public async Task<ActionResult> EnableConfiguration(Guid id)
    {
        var result = await _configurationService.EnableConfigurationAsync(id);
        if (!result)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(new { Message = "Configuration enabled successfully" });
    }

    [HttpPost("configurations/{id:guid}/disable")]
    public async Task<ActionResult> DisableConfiguration(Guid id)
    {
        var result = await _configurationService.DisableConfigurationAsync(id);
        if (!result)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(new { Message = "Configuration disabled successfully" });
    }

    [HttpGet("configurations/endpoint/{endpoint}")]
    public async Task<ActionResult<RateLimitConfigurationDto>> GetConfigurationByEndpoint(
        string endpoint,
        [FromQuery] string httpMethod = "GET")
    {
        var configuration = await _configurationService.GetConfigurationByEndpointAsync(endpoint, httpMethod);
        if (configuration == null)
            return NotFound(new { Error = "Configuration not found for endpoint", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(configuration);
    }

    [HttpPost("configurations/{id:guid}/test")]
    public async Task<ActionResult> TestConfiguration(Guid id, [FromBody] RateLimitCheckRequest testRequest)
    {
        var result = await _configurationService.TestConfigurationAsync(id, testRequest);
        return Ok(new { Message = "Configuration test completed", Result = result });
    }

    #endregion
}

public class SendAlertRequest
{
    public string Message { get; set; } = default!;
    public string? Recipients { get; set; }
}
