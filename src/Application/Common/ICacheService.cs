using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Common;

/// <summary>
/// Caching işlemleri için interface
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den değer getirir
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Cache'e değer set eder
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Cache'den değer siler
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Pattern'e göre cache'den değerleri siler
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'de değer var mı kontrol eder
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'i temizler
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Memory cache implementasyonu (geliştirme için)
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _memoryCache.TryGetValue(key, out T? value);
            _logger.LogDebug("Cache {Action}: {Key} - Found: {Found}", "GET", key, value != null);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            _memoryCache.Set(key, value, options);
            _logger.LogDebug("Cache {Action}: {Key} - Expiration: {Expiration}", "SET", key, expiration);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Cache {Action}: {Key}", "REMOVE", key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Memory cache'de pattern removal için reflection kullanılabilir
        // Production'da Redis kullanılmalı
        _logger.LogWarning("Pattern removal not fully supported in memory cache. Pattern: {Pattern}", pattern);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            _logger.LogDebug("Cache {Action}: {Key} - Exists: {Exists}", "EXISTS", key, exists);
            return Task.FromResult(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_memoryCache is MemoryCache mc)
            {
                mc.Compact(1.0); // Clear all entries
            }
            _logger.LogInformation("Cache cleared");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return Task.CompletedTask;
        }
    }
}
