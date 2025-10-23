namespace Getir.Domain.Enums;

/// <summary>
/// Rate limit türleri
/// </summary>
public enum RateLimitType
{
    Endpoint = 1,
    User = 2,
    IP = 3,
    Global = 4
}

/// <summary>
/// Rate limit periyodu
/// </summary>
public enum RateLimitPeriod
{
    PerSecond = 1,
    PerMinute = 2,
    PerHour = 3,
    PerDay = 4
}

/// <summary>
/// Rate limit eylemi
/// </summary>
public enum RateLimitAction
{
    Allow = 1,
    Block = 2,
    Throttle = 3,
    Log = 4
}

/// <summary>
/// Rate limit türleri uzantıları
/// </summary>
public static class RateLimitTypeExtensions
{
    /// <summary>
    /// Rate limit türünün görünen adını döndürür
    /// </summary>
    /// <param name="type">Rate limit türü</param>
    /// <returns>Rate limit türünün görünen adı</returns>
    public static string GetDisplayName(this RateLimitType type)
    {
        return type switch
        {
            RateLimitType.Endpoint => "Endpoint",
            RateLimitType.User => "User",
            RateLimitType.IP => "IP Address",
            RateLimitType.Global => "Global",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Rate limit türünün açıklamasını döndürür
    /// </summary>
    /// <param name="type">Rate limit türü</param>
    /// <returns>Rate limit türünün açıklaması</returns>
    public static string GetDescription(this RateLimitType type)
    {
        return type switch
        {
            RateLimitType.Endpoint => "Rate limiting based on specific API endpoints",
            RateLimitType.User => "Rate limiting based on authenticated users",
            RateLimitType.IP => "Rate limiting based on IP addresses",
            RateLimitType.Global => "Global rate limiting across all requests",
            _ => "Unknown rate limit type"
        };
    }
}

/// <summary>
/// Rate limit periyodu uzantıları
/// </summary>
public static class RateLimitPeriodExtensions
{
    /// <summary>
    /// Rate limit periyodunun TimeSpan'ını döndürür
    /// </summary>
    /// <param name="period">Rate limit periyodu</param>
    /// <returns>Rate limit periyodunun TimeSpan'ı</returns>
    public static TimeSpan GetTimeSpan(this RateLimitPeriod period)
    {
        return period switch
        {
            RateLimitPeriod.PerSecond => TimeSpan.FromSeconds(1),
            RateLimitPeriod.PerMinute => TimeSpan.FromMinutes(1),
            RateLimitPeriod.PerHour => TimeSpan.FromHours(1),
            RateLimitPeriod.PerDay => TimeSpan.FromDays(1),
            _ => TimeSpan.FromMinutes(1)
        };
    }

    /// <summary>
    /// Rate limit periyodunun görünen adını döndürür
    /// </summary>
    /// <param name="period">Rate limit periyodu</param>
    /// <returns>Rate limit periyodunun görünen adı</returns>
    public static string GetDisplayName(this RateLimitPeriod period)
    {
        return period switch
        {
            RateLimitPeriod.PerSecond => "Per Second",
            RateLimitPeriod.PerMinute => "Per Minute",
            RateLimitPeriod.PerHour => "Per Hour",
            RateLimitPeriod.PerDay => "Per Day",
            _ => "Unknown"
        };
    }
}
