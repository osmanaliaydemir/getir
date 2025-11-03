using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using WebApp.Models;

namespace WebApp.Services;

public partial class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAuthService? _authService;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger, IAuthService? authService = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Configure Polly retry policy
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => 
                !r.IsSuccessStatusCode && 
                (r.StatusCode == HttpStatusCode.RequestTimeout ||
                 r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                 r.StatusCode == HttpStatusCode.GatewayTimeout ||
                 r.StatusCode >= HttpStatusCode.InternalServerError))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>() // Timeout exceptions
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2s, 4s, 8s
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        _logger.LogWarning(
                            outcome.Exception,
                            "Retry {RetryCount} after {DelaySeconds}s - Exception: {ExceptionType}",
                            retryCount,
                            timespan.TotalSeconds,
                            outcome.Exception.GetType().Name);
                    }
                    else if (outcome.Result != null)
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} after {DelaySeconds}s - HTTP Status: {StatusCode}",
                            retryCount,
                            timespan.TotalSeconds,
                            outcome.Result.StatusCode);
                    }
                });
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("ApiClient.GetAsync called - Endpoint: {Endpoint}, BaseUrl: {BaseUrl}", endpoint, _httpClient.BaseAddress);
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            if (string.IsNullOrEmpty(token) && _authService != null)
            {
                token = await _authService.GetTokenAsync();
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Authorization header added to request");
            }

            // Execute request with retry policy
            var response = await SendWithAuthRetry(async () =>
                await _httpClient.SendAsync(request, cancellationToken));

            _logger.LogDebug("HTTP Response received - Status: {StatusCode}", response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                // API'den gelen response'u ApiResponse<T> olarak deserialize et
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                if (apiResponse != null)
                {
                    _logger.LogDebug("API Response deserialized successfully - IsSuccess: {IsSuccess}", apiResponse.IsSuccess);
                    return apiResponse;
                }
                else
                {
                    _logger.LogWarning("API Response deserialization failed - null returned for endpoint: {Endpoint}", endpoint);
                    return new ApiResponse<T> { IsSuccess = false, Error = "API response deserialize failed" };
                }
            }
            else
            {
                _logger.LogWarning("HTTP Error for endpoint {Endpoint}: {StatusCode} - {Content}", endpoint, response.StatusCode, content.Substring(0, Math.Min(200, content.Length)));
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {content}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApiClient exception for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            return new ApiResponse<T> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, string? token = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("ApiClient.PostAsync called - Endpoint: {Endpoint}", endpoint);
        
        try
        {
            var json = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null;
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");

            if (string.IsNullOrEmpty(token) && _authService != null)
            {
                token = await _authService.GetTokenAsync();
            }

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Authorization header added to POST request");
            }

            // CSRF header if available
            if (_authService != null)
            {
                var csrf = await _authService.GetCsrfTokenAsync();
                if (!string.IsNullOrEmpty(csrf))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
                    _httpClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf);
                }
            }

            var response = await SendWithAuthRetry(async () =>
                await _httpClient.PostAsync(endpoint, content, cancellationToken));
            
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // API response'unu ApiResponse<T> olarak parse et
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        _logger.LogDebug("POST request successful for endpoint: {Endpoint}", endpoint);
                        return apiResponse;
                    }
                    else
                    {
                        _logger.LogWarning("POST request returned error for endpoint {Endpoint}: {Error}", endpoint, apiResponse?.Error ?? "Unknown error");
                        return new ApiResponse<T> { IsSuccess = false, Error = apiResponse?.Error ?? "Unknown error" };
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON deserialization error for endpoint {Endpoint}", endpoint);
                    return new ApiResponse<T> { IsSuccess = false, Error = $"JSON Deserialization Error: {ex.Message}" };
                }
            }
            else
            {
                _logger.LogWarning("HTTP error for POST endpoint {Endpoint}: {StatusCode} - {Content}", endpoint, response.StatusCode, responseContent.Substring(0, Math.Min(200, responseContent.Length)));
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {responseContent}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in POST request for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            return new ApiResponse<T> { IsSuccess = false, Error = ex.Message };
        }
        finally
        {
            // Clear authorization header after request
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null, string? token = null)
    {
        _logger.LogDebug("ApiClient.PutAsync called - Endpoint: {Endpoint}", endpoint);
        
        try
        {
            var json = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null;
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");

            if (string.IsNullOrEmpty(token) && _authService != null)
            {
                token = await _authService.GetTokenAsync();
            }

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Authorization header added to PUT request");
            }

            if (_authService != null)
            {
                var csrf = await _authService.GetCsrfTokenAsync();
                if (!string.IsNullOrEmpty(csrf))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
                    _httpClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf);
                }
            }

            var response = await SendWithAuthRetry(async () =>
                await _httpClient.PutAsync(endpoint, content));
            
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("PUT request successful for endpoint: {Endpoint}", endpoint);
                var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                return new ApiResponse<T> { IsSuccess = true, Data = result };
            }
            else
            {
                _logger.LogWarning("HTTP error for PUT endpoint {Endpoint}: {StatusCode} - {Content}", endpoint, response.StatusCode, responseContent.Substring(0, Math.Min(200, responseContent.Length)));
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {responseContent}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in PUT request for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            return new ApiResponse<T> { IsSuccess = false, Error = ex.Message };
        }
        finally
        {
            // Clear authorization header after request
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, string? token = null)
    {
        _logger.LogDebug("ApiClient.DeleteAsync called - Endpoint: {Endpoint}", endpoint);
        
        try
        {
            if (string.IsNullOrEmpty(token) && _authService != null)
            {
                token = await _authService.GetTokenAsync();
            }

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Authorization header added to DELETE request");
            }

            if (_authService != null)
            {
                var csrf = await _authService.GetCsrfTokenAsync();
                if (!string.IsNullOrEmpty(csrf))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
                    _httpClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf);
                }
            }

            var response = await SendWithAuthRetry(async () =>
                await _httpClient.DeleteAsync(endpoint));
            
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("DELETE request successful for endpoint: {Endpoint}", endpoint);
                var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                return new ApiResponse<T> { IsSuccess = true, Data = result };
            }
            else
            {
                _logger.LogWarning("HTTP error for DELETE endpoint {Endpoint}: {StatusCode} - {Content}", endpoint, response.StatusCode, responseContent.Substring(0, Math.Min(200, responseContent.Length)));
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {responseContent}" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in DELETE request for endpoint {Endpoint}: {Message}", endpoint, ex.Message);
            return new ApiResponse<T> { IsSuccess = false, Error = ex.Message };
        }
        finally
        {
            // Clear authorization header after request
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    // Non-generic methods for cases where we don't need to deserialize response
    public async Task<ApiResponse<string>> PostAsync(string endpoint, object? data = null, string? token = null)
    {
        return await PostAsync<string>(endpoint, data, token);
    }

    public async Task<ApiResponse<string>> PutAsync(string endpoint, object? data = null, string? token = null)
    {
        return await PutAsync<string>(endpoint, data, token);
    }

    public async Task<ApiResponse<string>> DeleteAsync(string endpoint, string? token = null)
    {
        return await DeleteAsync<string>(endpoint, token);
    }
}

// Helpers
partial class ApiClient
{
    private async Task<HttpResponseMessage> SendWithAuthRetry(Func<Task<HttpResponseMessage>> sender)
    {
        var response = await _retryPolicy.ExecuteAsync(sender);
        if (response.StatusCode == HttpStatusCode.Unauthorized && _authService != null)
        {
            var refreshed = await _authService.RefreshTokenAsync();
            if (refreshed)
            {
                response = await _retryPolicy.ExecuteAsync(sender);
            }
        }
        return response;
    }
}
