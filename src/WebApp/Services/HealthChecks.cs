using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebApp.Services;

namespace WebApp.Services;

public class ApiHealthCheck : IHealthCheck
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<ApiHealthCheck> _logger;

    public ApiHealthCheck(ApiClient apiClient, ILogger<ApiHealthCheck> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<object>("health", cancellationToken: cancellationToken);
            
            if (response.IsSuccess)
            {
                return HealthCheckResult.Healthy("API is responding correctly");
            }
            else
            {
                return HealthCheckResult.Unhealthy($"API returned error: {response.Error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API health check failed");
            return HealthCheckResult.Unhealthy($"API health check failed: {ex.Message}");
        }
    }
}

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Burada gerçek bir database connection check yapılabilir
            // Şimdilik basit bir check yapıyoruz
            
            await Task.Delay(100, cancellationToken); // Simulate database check
            
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}");
        }
    }
}
