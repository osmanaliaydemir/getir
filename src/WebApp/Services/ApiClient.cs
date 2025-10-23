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

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return new ApiResponse<T> { IsSuccess = true, Data = data };
            }
            else
            {
                return new ApiResponse<T> { IsSuccess = false, Error = $"HTTP {response.StatusCode}: {content}" };
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { IsSuccess = false, Error = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, string? token = null)
    {
        try
        {
            var json = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null;
            var content = new StringContent(json ?? string.Empty, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.PostAsync(endpoint, content);
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
