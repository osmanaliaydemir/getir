using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Infrastructure.Persistence;
using Getir.Infrastructure.Persistence.Repositories;
using Getir.Infrastructure.Security;
using Getir.Infrastructure.Services.Notifications;

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
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
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
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<IBackgroundTaskService, BackgroundTaskService>();
        services.AddScoped<IEmailService, EmailService>();
        
        // File Storage Services
        services.AddScoped<IFileStorageService, Getir.Infrastructure.Services.FileStorage.SimpleFileStorageService>();
        services.AddScoped<ICdnService, Getir.Infrastructure.Services.Cdn.SimpleCdnService>();
        
        // Memory Cache
        services.AddMemoryCache();
        
        return services;
    }
}

