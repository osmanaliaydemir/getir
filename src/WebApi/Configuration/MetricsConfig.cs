using Prometheus;

namespace Getir.WebApi.Configuration;

public static class MetricsConfig
{
    public static IServiceCollection AddMetricsConfiguration(this IServiceCollection services)
    {
        // Prometheus metrics are automatically registered
        return services;
    }

    public static WebApplication UseMetricsConfiguration(this WebApplication app)
    {
        // Enable Prometheus metrics
        app.UseHttpMetrics();
        app.UseMetricServer();
        
        return app;
    }
}
