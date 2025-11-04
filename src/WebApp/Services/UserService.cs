using WebApp.Models;

namespace WebApp.Services
{
    public interface IUserService
    {
        Task<UserProfileResponse?> GetUserProfileAsync();
        Task<bool> UpdateUserProfileAsync(UpdateUserProfileRequest request);
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
        Task<bool> UpdateAddressAsync(Guid addressId, UpdateAddressRequest request);
        Task<List<AddressResponse>> GetUserAddressesAsync();
        Task<bool> AddAddressAsync(AddAddressRequest request);
        Task<bool> DeleteAddressAsync(Guid addressId);
        Task<bool> SetDefaultAddressAsync(Guid addressId);
        Task<UserSettingsResponse?> GetUserSettingsAsync();
        Task<bool> UpdateUserSettingsAsync(UpdateUserSettingsRequest request);
        Task<UserStatisticsResponse?> GetUserStatisticsAsync();
        Task<UserPermissionsResponse?> GetUserPermissionsAsync();
        Task<bool> UpdateUserPermissionsAsync(UpdatePermissionsRequest request);
    }

    public class UserService : IUserService
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        public UserService(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        public async Task<UserProfileResponse?> GetUserProfileAsync()
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.GetAsync<UserProfileResponse>("api/v1/user/profile", token);
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<bool> UpdateUserProfileAsync(UpdateUserProfileRequest request)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PutAsync("api/v1/user/profile", request, token);
            return response.IsSuccess;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PostAsync("api/v1/user/change-password", request, token);
            return response.IsSuccess;
        }

        public async Task<bool> UpdateAddressAsync(Guid addressId, UpdateAddressRequest request)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PutAsync($"api/v1/user/addresses/{addressId}", request, token);
            return response.IsSuccess;
        }

        public async Task<List<AddressResponse>> GetUserAddressesAsync()
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.GetAsync<List<AddressResponse>>("api/v1/user/addresses", token);
            return response.IsSuccess ? response.Data ?? new List<AddressResponse>() : new List<AddressResponse>();
        }

        public async Task<bool> AddAddressAsync(AddAddressRequest request)
        {
            var token = await _authService.GetTokenAsync();

            // Map UI model to API contract: derive FullAddress and keep other fields
            var parts = new List<string>
            {
                request.AddressLine1,
                request.AddressLine2,
                request.District,
                request.City,
                request.PostalCode
            };
            var fullAddress = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p))).Trim();

            var payload = new
            {
                title = request.Title,
                fullAddress = fullAddress,
                city = request.City,
                district = request.District,
                latitude = 0m,
                longitude = 0m,
                postalCode = request.PostalCode,
                country = request.Country,
                instructions = request.Instructions,
                isDefault = request.IsDefault
                // Add optional ids if available in future: cityId, districtId, countryCode
            };

            var response = await _apiClient.PostAsync("api/v1/user/addresses", payload, token);
            return response.IsSuccess;
        }

        public async Task<bool> DeleteAddressAsync(Guid addressId)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.DeleteAsync($"api/v1/user/addresses/{addressId}", token);
            return response.IsSuccess;
        }

        public async Task<bool> SetDefaultAddressAsync(Guid addressId)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PutAsync($"api/v1/user/addresses/{addressId}/set-default", null, token);
            return response.IsSuccess;
        }

        public async Task<UserSettingsResponse?> GetUserSettingsAsync()
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.GetAsync<UserSettingsResponse>("api/v1/user/settings", token);
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<bool> UpdateUserSettingsAsync(UpdateUserSettingsRequest request)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PutAsync("api/v1/user/settings", request, token);
            return response.IsSuccess;
        }

        public async Task<UserStatisticsResponse?> GetUserStatisticsAsync()
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.GetAsync<UserStatisticsResponse>("api/v1/user/statistics", token);
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<UserPermissionsResponse?> GetUserPermissionsAsync()
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.GetAsync<UserPermissionsResponse>("api/v1/user/permissions", token);
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<bool> UpdateUserPermissionsAsync(UpdatePermissionsRequest request)
        {
            var token = await _authService.GetTokenAsync();
            var response = await _apiClient.PutAsync("api/v1/user/permissions", request, token);
            return response.IsSuccess;
        }
    }
}
