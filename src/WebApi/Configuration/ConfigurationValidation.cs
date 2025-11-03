using Microsoft.Extensions.Options;

namespace Getir.WebApi.Configuration;

public static class ConfigurationValidation
{
    public static IServiceCollection AddConfigurationValidation(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddHostedService(provider => new ConfigurationValidationHostedService(configuration, env, provider.GetRequiredService<ILogger<ConfigurationValidationHostedService>>()));
        return services;
    }
}

public class ConfigurationValidationHostedService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ConfigurationValidationHostedService> _logger;

    public ConfigurationValidationHostedService(IConfiguration configuration, IHostEnvironment env, ILogger<ConfigurationValidationHostedService> logger)
    {
        _configuration = configuration;
        _env = env;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // ConnectionStrings
        var defaultConn = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(defaultConn)) errors.Add("ConnectionStrings:DefaultConnection missing");

        // JWT
        var jwtSection = _configuration.GetSection("Jwt");
        var jwtSecret = jwtSection["Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
        {
            errors.Add("Jwt:Secret must be at least 32 characters");
        }
        if (!int.TryParse(jwtSection["AccessTokenMinutes"], out _)) errors.Add("Jwt:AccessTokenMinutes invalid");
        if (!int.TryParse(jwtSection["RefreshTokenMinutes"], out _)) errors.Add("Jwt:RefreshTokenMinutes invalid");

        // Redis (if enabled)
        var redisEnabled = _configuration.GetValue<bool>("Redis:Enabled");
        if (redisEnabled)
        {
            var redisCfg = _configuration["Redis:Configuration"];
            if (string.IsNullOrWhiteSpace(redisCfg)) errors.Add("Redis:Configuration missing while Redis.Enabled=true");
        }

        // CORS
        var corsOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (!_env.IsDevelopment() && (corsOrigins == null || corsOrigins.Length == 0))
        {
            errors.Add("Cors:AllowedOrigins must be set in non-development environments");
        }

        if (errors.Count > 0)
        {
            var message = "Critical configuration errors:\n - " + string.Join("\n - ", errors);
            _logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        _logger.LogInformation("Configuration validation passed");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
