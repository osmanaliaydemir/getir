namespace Getir.Infrastructure.Configuration;

/// <summary>
/// Redis configuration settings
/// Strongly-typed configuration for dependency injection
/// </summary>
public class RedisSettings
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Enable or disable Redis caching (fallback to MemoryCache if disabled)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Redis connection string
    /// Format: "host:port,option1=value1,option2=value2"
    /// Example: "localhost:6379,abortConnect=false"
    /// </summary>
    public string Configuration { get; set; } = "localhost:6379";

    /// <summary>
    /// Optional password for Redis authentication
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Instance name prefix for all cache keys
    /// Useful for multi-tenant or versioned caching
    /// </summary>
    public string InstanceName { get; set; } = "Getir:";

    /// <summary>
    /// Abort connection if Redis is not available on startup
    /// Recommended: false (graceful fallback to MemoryCache)
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Number of retry attempts when connecting to Redis
    /// </summary>
    public int ConnectRetry { get; set; } = 3;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Synchronous operation timeout in milliseconds
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Keep-alive interval in seconds
    /// </summary>
    public int KeepAlive { get; set; } = 60;

    /// <summary>
    /// Allow admin operations (FLUSHDB, CONFIG, etc.)
    /// Recommended: false in production
    /// </summary>
    public bool AllowAdmin { get; set; } = false;

    /// <summary>
    /// Use SSL/TLS for Redis connection
    /// Required for Azure Redis Cache and AWS ElastiCache
    /// </summary>
    public bool Ssl { get; set; } = false;

    /// <summary>
    /// Default database index (0-15 for standard Redis)
    /// </summary>
    public int DefaultDatabase { get; set; } = 0;

    /// <summary>
    /// Redis bağlantı string'ini oluştur
    /// </summary>
    /// <returns>Tam Redis bağlantı string'i</returns>
    public string GetConnectionString()
    {
        var parts = new List<string> { Configuration };

        if (!string.IsNullOrWhiteSpace(Password))
            parts.Add($"password={Password}");

        if (AbortOnConnectFail)
            parts.Add("abortConnect=true");
        else
            parts.Add("abortConnect=false");

        parts.Add($"connectRetry={ConnectRetry}");
        parts.Add($"connectTimeout={ConnectTimeout}");
        parts.Add($"syncTimeout={SyncTimeout}");
        parts.Add($"keepAlive={KeepAlive}");

        if (Ssl)
            parts.Add("ssl=true");

        if (AllowAdmin)
            parts.Add("allowAdmin=true");

        parts.Add($"defaultDatabase={DefaultDatabase}");

        return string.Join(",", parts);
    }
}

