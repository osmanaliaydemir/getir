using Microsoft.IO;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Getir.WebApi.Middleware;

/// <summary>
/// Middleware for logging HTTP request and response details
/// Features: Body logging, performance tracking, sensitive data masking
/// Global standards compliant with production-ready implementation
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly RequestResponseLoggingOptions _options;

    // Sensitive data patterns for masking
    private static readonly Regex[] SensitivePatterns = new[]
    {
        new Regex(@"""password""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""token""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""authorization""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""apikey""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""secret""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""creditcard""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""cvv""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"""ssn""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    };

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        _options = configuration.GetSection("RequestResponseLogging").Get<RequestResponseLoggingOptions>() 
                   ?? new RequestResponseLoggingOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for excluded paths
        if (ShouldSkipLogging(context))
        {
            await _next(context);
            return;
        }

        var requestId = context.TraceIdentifier;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Log request
            var requestLog = await LogRequestAsync(context, requestId);

            // Intercept response for logging
            var originalBodyStream = context.Response.Body;

            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            try
            {
                // Call the next middleware
                await _next(context);

                // Log response
                await LogResponseAsync(context, requestId, stopwatch.Elapsed, responseBody, originalBodyStream);
            }
            catch (Exception ex)
            {
                // Log error before re-throwing
                _logger.LogError(ex, "[{RequestId}] Unhandled exception occurred", requestId);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error in RequestResponseLoggingMiddleware", requestId);
            throw;
        }
    }

    private async Task<string?> LogRequestAsync(HttpContext context, string requestId)
    {
        try
        {
            context.Request.EnableBuffering();

            var request = context.Request;
            string? requestBody = null;

            // Read request body if enabled and content length is within limit
            if (_options.LogRequestBody && 
                request.ContentLength.HasValue && 
                request.ContentLength.Value > 0 &&
                request.ContentLength.Value <= _options.MaxBodySizeBytes)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);

                // Mask sensitive data
                if (!string.IsNullOrEmpty(requestBody))
                {
                    requestBody = MaskSensitiveData(requestBody);
                }
            }

            // Log request details
            var logData = new
            {
                RequestId = requestId,
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                Headers = GetSafeHeaders(request.Headers),
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                Body = _options.LogRequestBody && requestBody?.Length <= _options.MaxLoggedBodyLength 
                    ? requestBody 
                    : requestBody?.Length > _options.MaxLoggedBodyLength 
                        ? $"[Body too large: {requestBody.Length} bytes]" 
                        : null,
                Timestamp = DateTime.UtcNow,
                ClientIP = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = request.Headers["User-Agent"].ToString()
            };

            _logger.LogInformation(
                "[{RequestId}] HTTP Request: {Method} {Path}{QueryString} | ContentType: {ContentType} | ClientIP: {ClientIP}",
                requestId,
                request.Method,
                request.Path,
                request.QueryString,
                request.ContentType,
                context.Connection.RemoteIpAddress?.ToString());

            if (_options.LogRequestBody && !string.IsNullOrEmpty(requestBody))
            {
                _logger.LogDebug("[{RequestId}] Request Body: {Body}", requestId, requestBody);
            }

            return requestBody;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error reading request", requestId);
            return null;
        }
    }

    private async Task LogResponseAsync(
        HttpContext context,
        string requestId,
        TimeSpan duration,
        Stream responseBodyStream,
        Stream originalBodyStream)
    {
        try
        {
            var response = context.Response;
            string? responseBody = null;

            responseBodyStream.Seek(0, SeekOrigin.Begin);

            // Read response body if enabled and size is within limit
            if (_options.LogResponseBody && 
                responseBodyStream.Length > 0 &&
                responseBodyStream.Length <= _options.MaxBodySizeBytes)
            {
                using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
                responseBodyStream.Seek(0, SeekOrigin.Begin);

                // Mask sensitive data
                if (!string.IsNullOrEmpty(responseBody))
                {
                    responseBody = MaskSensitiveData(responseBody);
                }
            }

            // Copy response back to original stream
            await responseBodyStream.CopyToAsync(originalBodyStream);

            // Determine log level based on status code
            var logLevel = response.StatusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information
            };

            // Log response details
            _logger.Log(logLevel,
                "[{RequestId}] HTTP Response: {StatusCode} {ReasonPhrase} | Duration: {Duration}ms | ContentType: {ContentType} | ContentLength: {ContentLength}",
                requestId,
                response.StatusCode,
                GetReasonPhrase(response.StatusCode),
                duration.TotalMilliseconds,
                response.ContentType,
                responseBodyStream.Length);

            if (_options.LogResponseBody && !string.IsNullOrEmpty(responseBody))
            {
                _logger.LogDebug("[{RequestId}] Response Body: {Body}", requestId, responseBody);
            }

            // Log slow requests
            if (duration.TotalMilliseconds > _options.SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "[{RequestId}] ⚠️ SLOW REQUEST: {Method} {Path} took {Duration}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] Error reading response", requestId);
        }
    }

    private bool ShouldSkipLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (string.IsNullOrEmpty(path))
            return true;

        // Skip health checks, metrics, and static files
        var excludedPaths = new[]
        {
            "/health",
            "/metrics",
            "/swagger",
            "/favicon.ico",
            "/_framework",
            "/css",
            "/js",
            "/images"
        };

        foreach (var excludedPath in excludedPaths.Concat(_options.ExcludedPaths))
        {
            if (path.StartsWith(excludedPath.ToLowerInvariant()))
                return true;
        }

        return false;
    }

    private static string MaskSensitiveData(string content)
    {
        foreach (var pattern in SensitivePatterns)
        {
            content = pattern.Replace(content, match =>
            {
                var key = match.Value.Split(':')[0];
                return $"{key}: \"***MASKED***\"";
            });
        }

        return content;
    }

    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        var safeHeaders = new Dictionary<string, string>();
        var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key", "x-auth-token" };

        foreach (var header in headers)
        {
            if (sensitiveHeaders.Contains(header.Key.ToLowerInvariant()))
            {
                safeHeaders[header.Key] = "***MASKED***";
            }
            else
            {
                safeHeaders[header.Key] = header.Value.ToString();
            }
        }

        return safeHeaders;
    }

    private static string GetReasonPhrase(int statusCode) => statusCode switch
    {
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        502 => "Bad Gateway",
        503 => "Service Unavailable",
        _ => "Unknown"
    };
}

/// <summary>
/// Configuration options for Request/Response logging
/// </summary>
public class RequestResponseLoggingOptions
{
    /// <summary>
    /// Enable request body logging (default: false for production performance)
    /// </summary>
    public bool LogRequestBody { get; set; } = false;

    /// <summary>
    /// Enable response body logging (default: false for production performance)
    /// </summary>
    public bool LogResponseBody { get; set; } = false;

    /// <summary>
    /// Maximum body size to log (default: 10KB)
    /// </summary>
    public int MaxBodySizeBytes { get; set; } = 10 * 1024; // 10KB

    /// <summary>
    /// Maximum body length to include in logs (default: 5KB)
    /// </summary>
    public int MaxLoggedBodyLength { get; set; } = 5 * 1024; // 5KB

    /// <summary>
    /// Slow request threshold in milliseconds (default: 3000ms)
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 3000;

    /// <summary>
    /// Paths to exclude from logging
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new();
}

