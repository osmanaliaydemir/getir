using WebApp.Models;

namespace WebApp.Services
{
    public class UserService
    {
        private readonly ApiClient _apiClient;

        public UserService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<UserProfileResponse?> GetUserProfileAsync()
        {
            var response = await _apiClient.GetAsync<UserProfileResponse>("api/v1/user/profile");
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<bool> UpdateUserProfileAsync(UpdateUserProfileRequest request)
        {
            var response = await _apiClient.PutAsync("api/v1/user/profile", request);
            return response.IsSuccess;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var response = await _apiClient.PostAsync("api/v1/user/change-password", request);
            return response.IsSuccess;
        }

        public async Task<bool> UpdateAddressAsync(Guid addressId, UpdateAddressRequest request)
        {
            var response = await _apiClient.PutAsync($"api/v1/user/addresses/{addressId}", request);
            return response.IsSuccess;
        }

        public async Task<List<AddressResponse>> GetUserAddressesAsync()
        {
            var response = await _apiClient.GetAsync<List<AddressResponse>>("api/v1/user/addresses");
            return response.IsSuccess ? response.Data ?? new List<AddressResponse>() : new List<AddressResponse>();
        }

        public async Task<bool> AddAddressAsync(AddAddressRequest request)
        {
            var response = await _apiClient.PostAsync("api/v1/user/addresses", request);
            return response.IsSuccess;
        }

        public async Task<bool> DeleteAddressAsync(Guid addressId)
        {
            var response = await _apiClient.DeleteAsync($"api/v1/user/addresses/{addressId}");
            return response.IsSuccess;
        }

        public async Task<bool> SetDefaultAddressAsync(Guid addressId)
        {
            var response = await _apiClient.PutAsync($"api/v1/user/addresses/{addressId}/set-default", null);
            return response.IsSuccess;
        }
    }
}
