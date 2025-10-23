using WebApp.Models;

namespace WebApp.Services
{
    public class OrderService
    {
        private readonly ApiClient _apiClient;

        public OrderService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<OrderResponse>> GetUserOrdersAsync(int page = 1, int pageSize = 20)
        {
            var response = await _apiClient.GetAsync<List<OrderResponse>>($"api/v1/order?page={page}&pageSize={pageSize}");
            return response.IsSuccess ? response.Data ?? new List<OrderResponse>() : new List<OrderResponse>();
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid orderId)
        {
            var response = await _apiClient.GetAsync<OrderResponse>($"api/v1/order/{orderId}");
            return response.IsSuccess ? response.Data : null;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var response = await _apiClient.PostAsync($"api/v1/order/{orderId}/cancel", null);
            return response.IsSuccess;
        }

        public async Task<bool> RepeatOrderAsync(Guid orderId)
        {
            var response = await _apiClient.PostAsync($"api/v1/order/{orderId}/repeat", null);
            return response.IsSuccess;
        }
    }
}
