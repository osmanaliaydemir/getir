using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Infrastructure.Configuration;
using Getir.Infrastructure.Persistence;
using Getir.Infrastructure.Persistence.Repositories;
using Getir.Infrastructure.Security;
using Getir.Infrastructure.Services.Caching;
using Getir.Infrastructure.Services.Notifications;
using StackExchange.Redis;

namespace Getir.WebApi.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services
/// This includes repositories, security services, and external integrations
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers all Infrastructure layer services with dependency injection
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repository Pattern
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IReadOnlyRepository<>), typeof(ReadOnlyRepository<>));
        
        // Security Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        
        // Framework Adapters (Clean Architecture - keeps Application layer framework-agnostic)
        services.AddScoped<IFileUploadAdapter, Getir.Infrastructure.Adapters.AspNetCoreFileUploadAdapter>();
        
        // Common Infrastructure Services
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
        services.AddScoped<IEmailService, EmailService>();
        
        // File Storage Services
        services.AddScoped<IFileStorageService, Getir.Infrastructure.Services.FileStorage.SimpleFileStorageService>();
        services.AddScoped<ICdnService, Getir.Infrastructure.Services.Cdn.SimpleCdnService>();
        
        // ============= CACHING SERVICES (Redis + Memory Fallback) =============
        services.AddCachingServices(configuration);
        
        return services;
    }

    /// <summary>
    /// Registers caching services with Redis primary and MemoryCache fallback
    /// Production-ready with circuit breaker and graceful degradation
    /// </summary>
    private static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Redis settings
        var redisSettings = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>() ?? new RedisSettings();
        services.Configure<RedisSettings>(configuration.GetSection(RedisSettings.SectionName));

        // Always add MemoryCache (used as fallback)
        services.AddMemoryCache();
        
        // Register MemoryCacheService for fallback
        services.AddSingleton<MemoryCacheService>();

        // Register Redis connection (singleton for connection pooling)
        if (redisSettings.Enabled)
        {
            try
            {
                var redisConnectionString = redisSettings.GetConnectionString();
                
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                    
                    try
                    {
                        var options = ConfigurationOptions.Parse(redisConnectionString);
                        options.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
                        options.ConnectRetry = redisSettings.ConnectRetry;
                        options.ConnectTimeout = redisSettings.ConnectTimeout;
                        options.SyncTimeout = redisSettings.SyncTimeout;
                        options.KeepAlive = redisSettings.KeepAlive;
                        options.AllowAdmin = redisSettings.AllowAdmin;

                        var multiplexer = ConnectionMultiplexer.Connect(options);
                        
                        // Event handlers for monitoring
                        multiplexer.ConnectionFailed += (object? sender, ConnectionFailedEventArgs args) =>
                        {
                            logger.LogError("Redis connection failed: {EndPoint} - {FailureType}", 
                                args.EndPoint, args.FailureType);
                        };

                        multiplexer.ConnectionRestored += (object? sender, ConnectionFailedEventArgs args) =>
                        {
                            logger.LogInformation("Redis connection restored: {EndPoint}", args.EndPoint);
                        };

                        multiplexer.ErrorMessage += (object? sender, RedisErrorEventArgs args) =>
                        {
                            logger.LogError("Redis error: {Message}", args.Message);
                        };

                        logger.LogInformation("Redis connection established successfully: {Configuration}", 
                            redisSettings.Configuration);
                        
                        return multiplexer;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to connect to Redis. Falling back to MemoryCache. Connection: {Connection}", 
                            redisSettings.Configuration);
                        
                        // Return null to trigger fallback to MemoryCache
                        return null!;
                    }
                });

                // Register RedisCacheService as primary cache
                services.AddScoped<ICacheService>(sp =>
                {
                    var redis = sp.GetService<IConnectionMultiplexer>();
                    var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
                    var fallbackCache = sp.GetRequiredService<MemoryCacheService>();

                    return new RedisCacheService(redis, logger, fallbackCache);
                });

                // Add distributed cache (for ASP.NET Core's IDistributedCache interface compatibility)
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = redisSettings.InstanceName;
                });
            }
            catch (Exception)
            {
                // If Redis configuration fails, fallback to MemoryCache
                // Note: Can't log here without building service provider (anti-pattern)
                // Error will be logged when RedisCacheService tries to connect
                services.AddScoped<ICacheService, MemoryCacheService>();
            }
        }
        else
        {
            // Redis disabled in configuration, use MemoryCache
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

        return services;
    }
}

