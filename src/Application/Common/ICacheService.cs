namespace Getir.Application.Common;

/// <summary>
/// Caching işlemleri için interface
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Cache'den değer getirir
    /// </summary>
    /// <typeparam name="T">Cache value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cache value</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Cache'e değer set eder
    /// </summary>
    /// <typeparam name="T">Cache value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Cache value</param>
    /// <param name="expiration">Expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Cache'den değer siler
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Pattern'e göre cache'den değerleri siler
    /// </summary>
    /// <param name="pattern">Cache pattern</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'de değer var mı kontrol eder
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cache value exists</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache'i temizler
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
