using WebApp.Models;

namespace WebApp.Services;

public interface ICartService
{
    Task<CartResponse?> GetCartAsync();
    Task<bool> AddToCartAsync(AddToCartRequest request);
    Task<bool> AddToCartAsync(Guid productId, int quantity);
    Task<List<CartItemResponse>> GetCartItemsAsync();
    Task<bool> UpdateCartItemAsync(Guid itemId, int quantity);
    Task<bool> RemoveFromCartAsync(Guid itemId);
    Task<bool> ClearCartAsync();
    Task<OrderResponse?> CreateOrderAsync(CreateOrderRequest request);
}

public class CartService : ICartService
{
    private readonly ApiClient _apiClient;
    private readonly AuthService _authService;

    public CartService(ApiClient apiClient, AuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    public async Task<CartResponse?> GetCartAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return null;

        var response = await _apiClient.GetAsync<CartResponse>("api/v1/cart", token);
        return response.IsSuccess ? response.Data : null;
    }

    public async Task<bool> AddToCartAsync(AddToCartRequest request)
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var response = await _apiClient.PostAsync<object>("api/v1/cart/items", request, token);
        return response.IsSuccess;
    }

    public async Task<bool> AddToCartAsync(Guid productId, int quantity)
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var request = new AddToCartRequest
        {
            ProductId = productId,
            Quantity = quantity,
            MerchantId = Guid.Empty // Bu değer API'den alınmalı
        };

        var response = await _apiClient.PostAsync<object>("api/v1/cart/items", request, token);
        return response.IsSuccess;
    }

    public async Task<List<CartItemResponse>> GetCartItemsAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new List<CartItemResponse>();

        var response = await _apiClient.GetAsync<List<CartItemResponse>>("api/v1/cart/items", token);
        return response.IsSuccess ? response.Data ?? new List<CartItemResponse>() : new List<CartItemResponse>();
    }

    public async Task<bool> UpdateCartItemAsync(Guid itemId, int quantity)
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var request = new { Quantity = quantity };
        var response = await _apiClient.PutAsync($"api/v1/cart/items/{itemId}", request, token);
        return response.IsSuccess;
    }

    public async Task<bool> RemoveFromCartAsync(Guid itemId)
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var response = await _apiClient.DeleteAsync($"api/v1/cart/items/{itemId}", token);
        return response.IsSuccess;
    }

    public async Task<bool> ClearCartAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        var response = await _apiClient.DeleteAsync("api/v1/cart/clear", token);
        return response.IsSuccess;
    }

    public async Task<OrderResponse?> CreateOrderAsync(CreateOrderRequest request)
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return null;

        var response = await _apiClient.PostAsync<OrderResponse>("api/v1/order", request, token);
        return response.IsSuccess ? response.Data : null;
    }
}
