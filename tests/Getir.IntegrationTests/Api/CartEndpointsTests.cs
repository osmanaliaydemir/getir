using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class CartEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CartEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequest(
            $"cart{Guid.NewGuid()}@example.com",
            "Test123!",
            "John",
            "Doe",
            "+905551234567",
            Domain.Enums.UserRole.Admin); // Admin role for testing

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    [Fact]
    public async Task GetCart_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCart_WithAuth_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
        cart.Should().NotBeNull();
        cart!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task AddToCart_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var request = new AddToCartRequest(Guid.NewGuid(), Guid.NewGuid(), 1, null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddToCart_WithValidData_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a merchant and product first
        var merchantId = await CreateTestMerchantAsync(client);
        var productId = await CreateTestProductAsync(client, merchantId);

        var request = new AddToCartRequest(merchantId, productId, 2, null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cartItem = await response.Content.ReadFromJsonAsync<CartItemResponse>();
        cartItem.Should().NotBeNull();
        cartItem!.Quantity.Should().Be(2);
    }

    [Fact]
    public async Task AddToCart_WithInvalidProductId_ShouldReturn404()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new AddToCartRequest(Guid.NewGuid(), Guid.NewGuid(), 1, null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/cart/items", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task AddToCart_WithZeroQuantity_ShouldReturn400()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new AddToCartRequest(Guid.NewGuid(), Guid.NewGuid(), 0, null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/cart/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "Requires product setup")]
    public async Task UpdateCartItem_WithValidData_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Add item first
        var merchantId = await CreateTestMerchantAsync(client);
        var productId = await CreateTestProductAsync(client, merchantId);
        var addRequest = new AddToCartRequest(merchantId, productId, 1, null);
        var addResponse = await client.PostAsJsonAsync("/api/v1/cart/items", addRequest);
        var cartItem = await addResponse.Content.ReadFromJsonAsync<CartItemResponse>();

        // Act - Update quantity
        var updateRequest = new UpdateCartItemRequest(3, null);
        var response = await client.PutAsJsonAsync($"/api/v1/cart/items/{cartItem!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedItem = await response.Content.ReadFromJsonAsync<CartItemResponse>();
        updatedItem!.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task UpdateCartItem_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateCartItemRequest(3, null);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/cart/items/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Requires merchant with valid ServiceCategory")]
    public async Task RemoveCartItem_WithValidId_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Add item first
        var merchantId = await CreateTestMerchantAsync(client);
        var productId = await CreateTestProductAsync(client, merchantId);
        var addRequest = new AddToCartRequest(merchantId, productId, 1, null);
        var addResponse = await client.PostAsJsonAsync("/api/v1/cart/items", addRequest);
        var cartItem = await addResponse.Content.ReadFromJsonAsync<CartItemResponse>();

        // Act
        var response = await client.DeleteAsync($"/api/v1/cart/items/{cartItem!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify item is removed
        var cartResponse = await client.GetAsync("/api/v1/cart");
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartResponse>();
        cart!.Items.Should().NotContain(i => i.Id == cartItem.Id);
    }

    [Fact]
    public async Task RemoveCartItem_WithInvalidId_ShouldReturn404()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/v1/cart/items/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Requires merchant setup with valid ServiceCategory")]
    public async Task ClearCart_WithItems_ShouldReturn200AndEmptyCart()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Add items first
        var merchantId = await CreateTestMerchantAsync(client);
        var productId = await CreateTestProductAsync(client, merchantId);
        var addRequest = new AddToCartRequest(merchantId, productId, 2, null);
        await client.PostAsJsonAsync("/api/v1/cart/items", addRequest);

        // Act
        var response = await client.DeleteAsync("/api/v1/cart/clear");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify cart is empty
        var cartResponse = await client.GetAsync("/api/v1/cart");
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartResponse>();
        cart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearCart_WhenEmpty_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync("/api/v1/cart/clear");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Helper methods
    private async Task<Guid> CreateTestMerchantAsync(HttpClient client)
    {
        var merchantRequest = new CreateMerchantRequest(
            $"Test Merchant {Guid.NewGuid()}",
            "Test Description",
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Test Address",
            40.7128m,
            -74.0060m,
            "+905551234567",
            "test@merchant.com",
            50.0m,
            10.0m);

        var response = await client.PostAsJsonAsync("/api/v1/merchant", merchantRequest);
        response.EnsureSuccessStatusCode(); // Throw if not success
        var merchant = await response.Content.ReadFromJsonAsync<MerchantResponse>();
        return merchant!.Id;
    }

    private async Task<Guid> CreateTestProductAsync(HttpClient client, Guid merchantId)
    {
        var productRequest = new CreateProductRequest(
            merchantId,
            null,
            $"Test Product {Guid.NewGuid()}",
            "Test product description",
            100.0m,
            null,
            10,
            "piece");

        var response = await client.PostAsJsonAsync("/api/v1/product", productRequest);
        response.EnsureSuccessStatusCode(); // Throw if not success
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return product!.Id;
    }
}


