using System.Text;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🌐 ApiClient.GetAsync çağrıldı - Endpoint: {endpoint}");
        Console.WriteLine($"🌐 Base URL: {_httpClient.BaseAddress}");
        Console.WriteLine($"🌐 Full URL: {_httpClient.BaseAddress}{endpoint}");
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"🔐 Authorization header eklendi");
            }

            Console.WriteLine($"📤 HTTP Request gönderiliyor...");
            var response = await _httpClient.SendAsync(request, cancellationToken);
            Console.WriteLine($"📥 HTTP Response alındı - Status: {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📄 Response Content Length: {content.Length}");
            Console.WriteLine($"📄 Response Content Preview: {content.Substring(0, Math.Min(200, content.Length))}...");

            if (response.IsSuccessStatusCode)
            {
                // API'den gelen response'u ApiResponse<T> olarak deserialize et
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                if (apiResponse != null)
                {
                    Console.WriteLine($"✅ API Response deserialize başarılı - IsSuccess: {apiResponse.IsSuccess}");
                    return apiResponse;
                }
                else
                {
                    Console.WriteLine($"❌ API Response deserialize başarısız - null döndü");
                    return new ApiResponse<T> { IsSuccess = false, Error = "API response deserialize failed" };
                }
            }
            else
            {
                Console.WriteLine($"❌ HTTP Error: {response.StatusCode} - {content}");
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {content}" };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ApiClient Exception: {ex.Message}");
            Console.WriteLine($"💥 Stack Trace: {ex.StackTrace}");
            return new ApiResponse<T> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null;
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // API response'unu ApiResponse<T> olarak parse et
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, _jsonOptions);

                    if (apiResponse != null && apiResponse.IsSuccess)
                    {
                        return apiResponse;
                    }
                    else
                    {
                        return new ApiResponse<T> { IsSuccess = false, Error = apiResponse?.Error ?? "Unknown error" };
                    }
                }
                catch (JsonException ex)
                {
                    return new ApiResponse<T> { IsSuccess = false, Error = $"JSON Deserialization Error: {ex.Message}" };
                }
            }
            else
            {
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {responseContent}" };
            }
        }
        catch (Exception ex)
        {
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
        try
        {
            var json = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null;
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                return new ApiResponse<T> { IsSuccess = true, Data = result };
            }
            else
            {
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {responseContent}" };
            }
        }
        catch (Exception ex)
        {
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
        try
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.DeleteAsync(endpoint);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                return new ApiResponse<T> { IsSuccess = true, Data = result };
            }
            else
            {
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {responseContent}" };
            }
        }
        catch (Exception ex)
        {
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
