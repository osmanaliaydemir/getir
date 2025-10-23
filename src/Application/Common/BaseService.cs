using Microsoft.Extensions.Logging;
using Getir.Application.Abstractions;

namespace Getir.Application.Common;

/// <summary>
/// Tüm servisler için base class
/// </summary>
public abstract class BaseService
{
    /// <summary>
    /// Unit of work instance
    /// </summary>
    protected readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// Logger instance
    /// </summary>
    protected readonly ILogger _logger;
    /// <summary>
    /// Logging service instance
    /// </summary>
    protected readonly ILoggingService _loggingService;
    /// <summary>
    /// Cache service instance
    /// </summary>
    protected readonly ICacheService _cacheService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="unitOfWork">Unit of work instance</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="loggingService">Logging service instance</param>
    /// <param name="cacheService">Cache service instance</param>
    protected BaseService(
        IUnitOfWork unitOfWork,
        ILogger logger,
        ILoggingService loggingService,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _loggingService = loggingService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Performance tracking ile operasyon çalıştırır
    /// </summary>
    /// <typeparam name="T">Result tipi</typeparam>
    /// <param name="operation">Operasyon</param>
    /// <param name="operationName">Operasyon adı</param>
    /// <param name="metadata">Metadata</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result</returns>
    protected async Task<Result<T>> ExecuteWithPerformanceTracking<T>(
        Func<Task<Result<T>>> operation,
        string operationName,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _loggingService.LogBusinessEvent($"Starting {operationName}", metadata);
            
            var result = await operation();
            
            stopwatch.Stop();
            _loggingService.LogPerformance(operationName, stopwatch.Elapsed, metadata);
            
            if (result.Success)
            {
                _loggingService.LogBusinessEvent($"Completed {operationName}", metadata);
            }
            else
            {
                _loggingService.LogBusinessEvent($"Failed {operationName}", new { Error = result.Error }, LogLevel.Warning);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _loggingService.LogPerformance(operationName, stopwatch.Elapsed, metadata);
            _loggingService.LogError($"Exception in {operationName}", ex, metadata);
            
            return ServiceResult.HandleException<T>(ex, _logger, operationName);
        }
    }

    /// <summary>
    /// Performance tracking ile operasyon çalıştırır (void)
    /// </summary>
    /// <param name="operation">Operasyon</param>
    /// <param name="operationName">Operasyon adı</param>
    /// <param name="metadata">Metadata</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result</returns>
    protected async Task<Result> ExecuteWithPerformanceTracking(
        Func<Task<Result>> operation,
        string operationName,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _loggingService.LogBusinessEvent($"Starting {operationName}", metadata);
            
            var result = await operation();
            
            stopwatch.Stop();
            _loggingService.LogPerformance(operationName, stopwatch.Elapsed, metadata);
            
            if (result.Success)
            {
                _loggingService.LogBusinessEvent($"Completed {operationName}", metadata);
            }
            else
            {
                _loggingService.LogBusinessEvent($"Failed {operationName}", new { Error = result.Error }, LogLevel.Warning);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _loggingService.LogPerformance(operationName, stopwatch.Elapsed, metadata);
            _loggingService.LogError($"Exception in {operationName}", ex, metadata);
            
            return ServiceResult.HandleException(ex, _logger, operationName);
        }
    }

    /// <summary>
    /// Cache'den veri getirir veya cache miss durumunda factory'den getirir
    /// </summary>
    /// <typeparam name="T">Result tipi</typeparam>
    /// <param name="cacheKey">Cache anahtarı</param>
    /// <param name="factory">Factory</param>
    /// <param name="expiration">Expiration</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Result</returns>
    protected async Task<Result<T>> GetOrSetCacheAsync<T>(
        string cacheKey,
        Func<Task<Result<T>>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            // Cache'den kontrol et
            var cachedValue = await _cacheService.GetAsync<T>(cacheKey, cancellationToken);
            if (cachedValue != null)
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                return ServiceResult.Success(cachedValue);
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
            
            // Factory'den veri getir
            var result = await factory();
            if (result.Success && result.Value != null)
            {
                // Cache'e kaydet
                await _cacheService.SetAsync(cacheKey, result.Value, expiration, cancellationToken);
                _logger.LogDebug("Value cached for key: {CacheKey}", cacheKey);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in cache operation for key: {CacheKey}", cacheKey);
            return ServiceResult.HandleException<T>(ex, _logger, $"Cache operation for {cacheKey}");
        }
    }

    /// <summary>
    /// Cache'i temizler
    /// </summary>
    /// <param name="pattern">Pattern</param>
    /// <param name="cancellationToken">CancellationToken</param>
    protected async Task InvalidateCacheAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
            _logger.LogDebug("Cache invalidated for pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for pattern: {Pattern}", pattern);
        }
    }
}
