namespace Getir.MerchantPortal.Services;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default);
    Task<T?> PostAsync<T>(string endpoint, object? data = null, CancellationToken ct = default);
    Task<T?> PutAsync<T>(string endpoint, object data, CancellationToken ct = default);
    Task<T?> DeleteAsync<T>(string endpoint, CancellationToken ct = default);
    Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default);
    void SetAuthToken(string token);
    void ClearAuthToken();
}

