using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Getir.Application.Common;

/// <summary>
/// Entity Framework sorguları için performans optimizasyonu uzantıları
/// </summary>
public static class PerformanceExtensions
{
    /// <summary>
    /// Performans takibi ile sorgu çalıştır
    /// </summary>
    /// <typeparam name="T">Dönüş tipi</typeparam>
    /// <param name="task">Çalıştırılacak task</param>
    /// <param name="operationName">İşlem adı</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>İşlem sonucu</returns>
    public static async Task<T> ExecuteWithPerformanceTrackingAsync<T>(
        this Task<T> task, 
        string operationName, 
        ILogger logger)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await task;
            stopwatch.Stop();
            
            logger.LogInformation("Performance: {OperationName} completed in {ElapsedMs}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Performance: {OperationName} failed after {ElapsedMs}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Performans takibi ile sorgu çalıştır ve sayı döndür
    /// </summary>
    /// <param name="task">Çalıştırılacak task</param>
    /// <param name="operationName">İşlem adı</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>İşlem sonucu (sayı)</returns>
    public static async Task<int> ExecuteWithPerformanceTrackingAsync(
        this Task<int> task, 
        string operationName, 
        ILogger logger)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await task;
            stopwatch.Stop();
            
            logger.LogInformation("Performance: {OperationName} completed in {ElapsedMs}ms, Count: {Count}", 
                operationName, stopwatch.ElapsedMilliseconds, result);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Performance: {OperationName} failed after {ElapsedMs}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Performans takibi ile sorgu çalıştır ve boolean döndür
    /// </summary>
    /// <param name="task">Çalıştırılacak task</param>
    /// <param name="operationName">İşlem adı</param>
    /// <param name="logger">Logger instance</param>
    /// <returns>İşlem sonucu (boolean)</returns>
    public static async Task<bool> ExecuteWithPerformanceTrackingAsync(
        this Task<bool> task, 
        string operationName, 
        ILogger logger)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await task;
            stopwatch.Stop();
            
            logger.LogInformation("Performance: {OperationName} completed in {ElapsedMs}ms, Result: {Result}", 
                operationName, stopwatch.ElapsedMilliseconds, result);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Performance: {OperationName} failed after {ElapsedMs}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Sorguyu read-only operasyonlar için optimize et
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="query">Optimize edilecek sorgu</param>
    /// <returns>Optimize edilmiş sorgu</returns>
    public static IQueryable<T> AsOptimizedReadOnly<T>(this IQueryable<T> query) where T : class
    {
        return query.AsNoTracking();
    }

    /// <summary>
    /// Sorguyu karmaşık include'lar için optimize et (N+1 önleme)
    /// </summary>
    /// <typeparam name="T">Entity tipi</typeparam>
    /// <param name="query">Optimize edilecek sorgu</param>
    /// <returns>Optimize edilmiş sorgu</returns>
    public static IQueryable<T> AsOptimizedWithIncludes<T>(this IQueryable<T> query) where T : class
    {
        return query.AsNoTracking();
    }

    /// <summary>
    /// Timeout ile sorgu çalıştır
    /// </summary>
    /// <typeparam name="T">Dönüş tipi</typeparam>
    /// <param name="task">Çalıştırılacak task</param>
    /// <param name="timeout">Timeout süresi</param>
    /// <returns>İşlem sonucu</returns>
    public static async Task<T> WithTimeoutAsync<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            return await task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Retry policy ile sorgu çalıştır
    /// </summary>
    /// <typeparam name="T">Dönüş tipi</typeparam>
    /// <param name="operation">Çalıştırılacak işlem</param>
    /// <param name="maxRetries">Maksimum deneme sayısı</param>
    /// <param name="delay">Denemeler arası bekleme süresi</param>
    /// <returns>İşlem sonucu</returns>
    public static async Task<T> WithRetryAsync<T>(
        this Func<Task<T>> operation, 
        int maxRetries = 3, 
        TimeSpan delay = default)
    {
        if (delay == default)
            delay = TimeSpan.FromSeconds(1);

        var exceptions = new List<Exception>();
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                exceptions.Add(ex);
                await Task.Delay(delay * (i + 1)); // Exponential backoff
            }
        }

        throw new AggregateException("Operation failed after all retries", exceptions);
    }
}
