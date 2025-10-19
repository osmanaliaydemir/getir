namespace Getir.Application.Common;

/// <summary>
/// Caching işlemleri için interface
/// Clean Architecture: Implementation details are in Infrastructure layer
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
