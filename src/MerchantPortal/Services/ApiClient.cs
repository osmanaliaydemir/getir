using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Getir.MerchantPortal.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    private string? _authToken;
    
    // JSON serialization settings for consistent camelCase handling
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuthToken()
    {
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
    
    /// <summary>
    /// Deserialize JSON content with camelCase handling
    /// </summary>
    private T? DeserializeResponse<T>(string content)
    {
        return JsonConvert.DeserializeObject<T>(content, JsonSettings);
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("GET request to {Endpoint} failed with status {StatusCode}. Response: {Response}", 
                    endpoint, response.StatusCode, errorContent);
                return default;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GET request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null, CancellationToken ct = default)
    {
        try
        {
            HttpContent? content = null;
            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PostAsync(endpoint, content, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("POST request to {Endpoint} failed with status {StatusCode}. Error: {Error}", 
                    endpoint, response.StatusCode, errorContent);
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during POST request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data, CancellationToken ct = default)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PUT request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PUT request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> DeleteAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DELETE request to {Endpoint} failed with status {StatusCode}", endpoint, response.StatusCode);
                return default;
            }

            var content = await response.Content.ReadAsStringAsync(ct);
            return DeserializeResponse<T>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DELETE request to {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during DELETE request to {Endpoint}", endpoint);
            return false;
        }
    }
}

