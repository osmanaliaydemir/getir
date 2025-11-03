using WebApp.Models;

namespace WebApp.Services;

public interface IContactService
{
    Task<bool> SubmitContactFormAsync(ContactFormRequest request);
}

public class ContactService : IContactService
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public ContactService(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<bool> SubmitContactFormAsync(ContactFormRequest request)
    {
        try
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PostAsync("api/v1/contact/submit", request, token);
            return response.IsSuccess;
        }
        catch
        {
            // Backend hazır değilse false döndür
            return false;
        }
    }
}

