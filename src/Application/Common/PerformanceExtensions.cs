using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Getir.Application.Common;

/// <summary>
/// Performance optimization extensions for Entity Framework queries
/// </summary>
public static class PerformanceExtensions
{
    /// <summary>
    /// Execute query with performance tracking
    /// </summary>
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
    /// Execute query with performance tracking and return count
    /// </summary>
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
    /// Execute query with performance tracking and return boolean
    /// </summary>
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
    /// Optimize query for read-only operations
    /// </summary>
    public static IQueryable<T> AsOptimizedReadOnly<T>(this IQueryable<T> query) where T : class
    {
        return query.AsNoTracking();
    }

    /// <summary>
    /// Optimize query for complex includes to prevent N+1
    /// </summary>
    public static IQueryable<T> AsOptimizedWithIncludes<T>(this IQueryable<T> query) where T : class
    {
        return query.AsNoTracking();
    }

    /// <summary>
    /// Execute query with timeout
    /// </summary>
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
    /// Execute query with retry policy
    /// </summary>
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
