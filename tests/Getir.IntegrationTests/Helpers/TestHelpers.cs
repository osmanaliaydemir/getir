using System.Net.Http.Json;
using Getir.Application.DTO;

namespace Getir.IntegrationTests.Helpers;

public static class TestHelpers
{
    public static async Task<(string Token, Guid UserId)> GetAuthTokenAsync(HttpClient client, bool asAdmin = true)
    {
        var email = $"testuser{Guid.NewGuid()}@example.com";
        var role = asAdmin ? Domain.Enums.UserRole.Admin : Domain.Enums.UserRole.Customer;
        
        var registerRequest = new RegisterRequest(
            email,
            "Test123!",
            "Test",
            "User",
            "+905551234567",
            role);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get auth token. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        return (authResponse!.AccessToken, authResponse.UserId);
    }

    public static async Task<Guid> CreateTestMerchantAsync(HttpClient client)
    {
        var merchantRequest = new CreateMerchantRequest(
            $"Test Merchant {Guid.NewGuid()}",
            "Test Description",
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Test Address",
            40.7128m,
            -74.0060m,
            "+905551234567",
            $"merchant{Guid.NewGuid()}@example.com",
            50.0m,
            10.0m);

        var response = await client.PostAsJsonAsync("/api/v1/merchant", merchantRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create merchant. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var merchant = await response.Content.ReadFromJsonAsync<MerchantResponse>();
        return merchant!.Id;
    }

    public static async Task<Guid> CreateTestProductAsync(HttpClient client, Guid merchantId, string name = "Test Product", decimal price = 100.0m)
    {
        var productRequest = new CreateProductRequest(
            merchantId,
            null,
            name,
            "Test product description",
            price,
            null,
            10,
            "piece");

        var response = await client.PostAsJsonAsync("/api/v1/product", productRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create product. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return product!.Id;
    }

    public static async Task<Guid> CreateTestAddressAsync(HttpClient client)
    {
        var request = new CreateAddressRequest(
            $"Test Address {Guid.NewGuid().ToString().Substring(0, 8)}",
            "123 Test St",
            "Test City",
            "Test District",
            40.7128m,
            -74.0060m);

        var response = await client.PostAsJsonAsync("/api/v1/user/addresses", request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create address. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>();
        return address!.Id;
    }

    public static async Task<Guid> CreateTestOrderAsync(HttpClient client)
    {
        var merchantId = await CreateTestMerchantAsync(client);
        var productId = await CreateTestProductAsync(client, merchantId);

        // Clear cart and add product
        await client.DeleteAsync("/api/v1/cart/clear");
        var addToCartRequest = new AddToCartRequest(merchantId, productId, 2, null);
        await client.PostAsJsonAsync("/api/v1/cart/items", addToCartRequest);

        // Create order
        var items = new List<OrderLineRequest>
        {
            new OrderLineRequest(productId, null, 2, null, null)
        };

        var orderRequest = new CreateOrderRequest(
            merchantId,
            items,
            "Test Address",
            40.7128m,
            -74.0060m,
            "CREDIT_CARD",
            null);

        var response = await client.PostAsJsonAsync("/api/v1/order", orderRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create order. Status: {response.StatusCode}, Error: {errorContent}");
        }
        
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        return order!.Id;
    }
}

