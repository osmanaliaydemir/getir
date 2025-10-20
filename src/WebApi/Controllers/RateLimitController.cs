using Microsoft.AspNetCore.Mvc;
using Getir.Application.DTO;
using Getir.Application.Services.RateLimiting;
using Getir.Domain.Enums;

namespace Getir.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Oran sınırlama (rate limit) kuralları, loglar, ihlaller, istatistikler ve yapılandırmalar için API
/// </summary>
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

    /// <summary>
    /// Tüm aktif oran sınırlama kurallarını getirir
    /// </summary>
    /// <returns>Kurallar listesi</returns>
    [HttpGet("rules")]
    public async Task<ActionResult<List<RateLimitRuleDto>>> GetAllRules()
    {
        var rules = await _rateLimitService.GetActiveRulesAsync();
        return Ok(rules);
    }

    /// <summary>
    /// ID'ye göre oran sınırlama kuralını getirir
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <returns>Kural</returns>
    [HttpGet("rules/{id:guid}")]
    public async Task<ActionResult<RateLimitRuleDto>> GetRuleById(Guid id)
    {
        var rule = await _rateLimitService.GetRuleByIdAsync(id);
        if (rule == null)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return Ok(rule);
    }

    /// <summary>
    /// Yeni oran sınırlama kuralı oluşturur
    /// </summary>
    /// <param name="request">Kural oluşturma isteği</param>
    /// <returns>Oluşturulan kural</returns>
    [HttpPost("rules")]
    public async Task<ActionResult<RateLimitRuleDto>> CreateRule([FromBody] CreateRateLimitRuleRequest request)
    {
        var rule = await _rateLimitService.CreateRuleAsync(request);
        return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
    }

    /// <summary>
    /// Kuralı günceller
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <param name="request">Kural güncelleme isteği</param>
    /// <returns>Güncellenen kural</returns>
    [HttpPut("rules/{id:guid}")]
    public async Task<ActionResult<RateLimitRuleDto>> UpdateRule(Guid id, [FromBody] UpdateRateLimitRuleRequest request)
    {
        var rule = await _rateLimitService.UpdateRuleAsync(id, request);
        return Ok(rule);
    }

    /// <summary>
    /// Kuralı siler
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("rules/{id:guid}")]
    public async Task<ActionResult> DeleteRule(Guid id)
    {
        var result = await _rateLimitService.DeleteRuleAsync(id);
        if (!result)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return NoContent();
    }

    /// <summary>
    /// Kuralı etkinleştirir
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("rules/{id:guid}/enable")]
    public async Task<ActionResult> EnableRule(Guid id)
    {
        var result = await _rateLimitService.EnableRuleAsync(id);
        if (!result)
            return NotFound(new { Error = "Rate limit rule not found", ErrorCode = "RULE_NOT_FOUND" });

        return Ok(new { Message = "Rule enabled successfully" });
    }

    /// <summary>
    /// Kuralı devre dışı bırakır
    /// </summary>
    /// <param name="id">Kural ID</param>
    /// <returns>Başarı yanıtı</returns>
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

    /// <summary>
    /// Oran sınırlama kontrolü yapar
    /// </summary>
    /// <param name="request">Kontrol isteği</param>
    /// <returns>Kontrol sonucu</returns>
    [HttpPost("check")]
    public async Task<ActionResult<RateLimitCheckResponse>> CheckRateLimit([FromBody] RateLimitCheckRequest request)
    {
        var response = await _rateLimitService.CheckRateLimitAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Bir endpoint için mevcut oran sınırlama durumunu getirir
    /// </summary>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="httpMethod">HTTP metodu</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="ipAddress">IP adresi</param>
    /// <returns>Oran sınırlama durumu</returns>
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

    /// <summary>
    /// Oran sınırlama önbelleğini temizler
    /// </summary>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="ipAddress">IP adresi</param>
    /// <returns>Başarı yanıtı</returns>
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

    /// <summary>
    /// Oran sınırlama loglarını arar
    /// </summary>
    /// <param name="request">Arama isteği</param>
    /// <returns>Arama sonucu</returns>
    [HttpPost("logs/search")]
    public async Task<ActionResult<RateLimitSearchResponse>> SearchLogs([FromBody] RateLimitSearchRequest request)
    {
        var response = await _rateLimitService.SearchLogsAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Son log kayıtlarını getirir
    /// </summary>
    /// <param name="count">Adet</param>
    /// <returns>Log listesi</returns>
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

    /// <summary>
    /// Oran sınırlama ihlallerini arar
    /// </summary>
    /// <param name="request">Arama isteği</param>
    /// <returns>Arama sonucu</returns>
    [HttpPost("violations/search")]
    public async Task<ActionResult<RateLimitViolationSearchResponse>> SearchViolations([FromBody] RateLimitViolationSearchRequest request)
    {
        var response = await _rateLimitService.SearchViolationsAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Son ihlalleri getirir
    /// </summary>
    /// <param name="count">Adet</param>
    /// <returns>İhlal listesi</returns>
    [HttpGet("violations/recent")]
    public async Task<ActionResult<List<RateLimitViolationDto>>> GetRecentViolations([FromQuery] int count = 10)
    {
        var violations = await _monitoringService.GetRecentViolationsAsync(count);
        return Ok(violations);
    }

    /// <summary>
    /// Çözülmemiş ihlalleri getirir
    /// </summary>
    /// <returns>İhlal listesi</returns>
    [HttpGet("violations/unresolved")]
    public async Task<ActionResult<List<RateLimitViolationDto>>> GetUnresolvedViolations()
    {
        var violations = await _monitoringService.GetUnresolvedViolationsAsync();
        return Ok(violations);
    }

    /// <summary>
    /// Yüksek şiddetli ihlalleri getirir
    /// </summary>
    /// <returns>İhlal listesi</returns>
    [HttpGet("violations/high-severity")]
    public async Task<ActionResult<List<RateLimitViolationDto>>> GetHighSeverityViolations()
    {
        var violations = await _monitoringService.GetHighSeverityViolationsAsync();
        return Ok(violations);
    }

    /// <summary>
    /// İhlali çözer/sonlandırır
    /// </summary>
    /// <param name="id">İhlal ID</param>
    /// <param name="request">Çözüm isteği</param>
    /// <returns>Başarı yanıtı</returns>
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

    /// <summary>
    /// İstatistikleri getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="type">Tür filtresi</param>
    /// <returns>İstatistikler</returns>
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

    /// <summary>
    /// Belirli bir endpoint için istatistikleri getirir
    /// </summary>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>İstatistik</returns>
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

    /// <summary>
    /// Belirli bir kullanıcı için istatistikleri getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>İstatistik</returns>
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

    /// <summary>
    /// Belirli bir IP adresi için istatistikleri getirir
    /// </summary>
    /// <param name="ipAddress">IP adresi</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <returns>İstatistik</returns>
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

    /// <summary>
    /// En çok ihlal eden endpointleri getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="count">Adet</param>
    /// <returns>Endpoint-ihlal sayısı sözlüğü</returns>
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

    /// <summary>
    /// En çok ihlal eden kullanıcıları getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="count">Adet</param>
    /// <returns>Kullanıcı-ihlal sayısı sözlüğü</returns>
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

    /// <summary>
    /// En çok ihlal eden IP adreslerini getirir
    /// </summary>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="count">Adet</param>
    /// <returns>IP-ihlal sayısı sözlüğü</returns>
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

    /// <summary>
    /// Gerçek zamanlı logları getirir
    /// </summary>
    /// <param name="count">Adet</param>
    /// <returns>Log listesi</returns>
    [HttpGet("logs/realtime")]
    public async Task<ActionResult<List<RateLimitLogDto>>> GetRealTimeLogs([FromQuery] int count = 100)
    {
        var logs = await _monitoringService.GetRealTimeLogsAsync(count);
        return Ok(logs);
    }

    /// <summary>
    /// Dashboard verilerini getirir
    /// </summary>
    /// <returns>Dashboard verileri</returns>
    [HttpGet("dashboard")]
    public async Task<ActionResult<Dictionary<string, object>>> GetDashboardData()
    {
        var dashboardData = await _monitoringService.GetDashboardDataAsync();
        return Ok(dashboardData);
    }

    /// <summary>
    /// Uyarı (alert) gönderir
    /// </summary>
    /// <param name="request">Uyarı gönderme isteği</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("alerts/send")]
    public async Task<ActionResult> SendAlert([FromBody] SendAlertRequest request)
    {
        var result = await _monitoringService.SendAlertAsync(request.Message, request.Recipients);
        return Ok(new { Message = "Alert sent successfully" });
    }

    /// <summary>
    /// Uyarı eşiklerini kontrol eder
    /// </summary>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("alerts/check-thresholds")]
    public async Task<ActionResult> CheckAlertThresholds()
    {
        var result = await _monitoringService.CheckAlertThresholdsAsync();
        return Ok(new { Message = "Alert thresholds checked successfully" });
    }

    #endregion

    #region Rate Limit Configurations

    /// <summary>
    /// Tüm oran sınırlama yapılandırmalarını getirir
    /// </summary>
    /// <returns>Yapılandırmalar listesi</returns>
    [HttpGet("configurations")]
    public async Task<ActionResult<List<RateLimitConfigurationDto>>> GetAllConfigurations()
    {
        var configurations = await _configurationService.GetAllConfigurationsAsync();
        return Ok(configurations);
    }

    /// <summary>
    /// ID'ye göre yapılandırmayı getirir
    /// </summary>
    /// <param name="id">Yapılandırma ID</param>
    /// <returns>Yapılandırma</returns>
    [HttpGet("configurations/{id:guid}")]
    public async Task<ActionResult<RateLimitConfigurationDto>> GetConfigurationById(Guid id)
    {
        var configuration = await _configurationService.GetConfigurationByIdAsync(id);
        if (configuration == null)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(configuration);
    }

    /// <summary>
    /// Yeni yapılandırma oluşturur
    /// </summary>
    /// <param name="request">Yapılandırma oluşturma isteği</param>
    /// <returns>Oluşturulan yapılandırma</returns>
    [HttpPost("configurations")]
    public async Task<ActionResult<RateLimitConfigurationDto>> CreateConfiguration([FromBody] CreateRateLimitConfigurationRequest request)
    {
        var configuration = await _configurationService.CreateConfigurationAsync(request);
        return CreatedAtAction(nameof(GetConfigurationById), new { id = configuration.Id }, configuration);
    }

    /// <summary>
    /// Yapılandırmayı günceller
    /// </summary>
    /// <param name="id">Yapılandırma ID</param>
    /// <param name="request">Yapılandırma güncelleme isteği</param>
    /// <returns>Güncellenen yapılandırma</returns>
    [HttpPut("configurations/{id:guid}")]
    public async Task<ActionResult<RateLimitConfigurationDto>> UpdateConfiguration(Guid id, [FromBody] UpdateRateLimitConfigurationRequest request)
    {
        var configuration = await _configurationService.UpdateConfigurationAsync(id, request);
        return Ok(configuration);
    }

    /// <summary>
    /// Yapılandırmayı siler
    /// </summary>
    /// <param name="id">Yapılandırma ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("configurations/{id:guid}")]
    public async Task<ActionResult> DeleteConfiguration(Guid id)
    {
        var result = await _configurationService.DeleteConfigurationAsync(id);
        if (!result)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return NoContent();
    }

    /// <summary>
    /// Yapılandırmayı etkinleştirir
    /// </summary>
    /// <param name="id">Yapılandırma ID</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("configurations/{id:guid}/enable")]
    public async Task<ActionResult> EnableConfiguration(Guid id)
    {
        var result = await _configurationService.EnableConfigurationAsync(id);
        if (!result)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(new { Message = "Configuration enabled successfully" });
    }

    /// <summary>
    /// Yapılandırmayı devre dışı bırakır
    /// </summary>
    /// <param name="id">Yapılandırma ID</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("configurations/{id:guid}/disable")]
    public async Task<ActionResult> DisableConfiguration(Guid id)
    {
        var result = await _configurationService.DisableConfigurationAsync(id);
        if (!result)
            return NotFound(new { Error = "Configuration not found", ErrorCode = "CONFIGURATION_NOT_FOUND" });

        return Ok(new { Message = "Configuration disabled successfully" });
    }

    /// <summary>
    /// Endpoint'e göre yapılandırmayı getirir
    /// </summary>
    /// <param name="endpoint">Endpoint</param>
    /// <param name="httpMethod">HTTP metodu</param>
    /// <returns>Yapılandırma</returns>
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

    /// <summary>
    /// Yapılandırmayı test eder
    /// </summary>
    /// <param name="id">Yapılandırma ID</param>
    /// <param name="testRequest">Test isteği</param>
    /// <returns>Test sonucu</returns>
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
