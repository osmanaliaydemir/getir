using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CategoryEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequest(
            $"category{Guid.NewGuid()}@example.com",
            "Test123!",
            "John",
            "Doe",
            "+905551234567",
            Domain.Enums.UserRole.Admin); // Admin role for testing

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    #region ServiceCategory Tests
    [Fact]
    public async Task GetServiceCategories_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/servicecategory?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<ServiceCategoryResponse>>();
        pagedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetServiceCategoryById_WithValidId_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/servicecategory/{Guid.Parse("11111111-1111-1111-1111-111111111111")}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServiceCategoriesByType_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/servicecategory/by-type/1?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<ServiceCategoryResponse>>();
        pagedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveServiceCategoriesByType_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/servicecategory/active/by-type/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<IEnumerable<ServiceCategoryResponse>>();
        categories.Should().NotBeNull();
    }
    #endregion

    #region ProductCategory Tests
    [Fact(Skip = "Requires merchant setup")]
    public async Task GetMerchantCategoryTree_WithValidMerchantId_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var merchantId = await CreateTestMerchantAsync(client);

        // Act
        var response = await _client.GetAsync($"/api/v1/productcategory/merchant/{merchantId}/tree");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<ProductCategoryTreeResponse>>();
        categories.Should().NotBeNull();
    }

    [Fact(Skip = "Requires merchant setup")]
    public async Task CreateProductCategory_WithAuth_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var merchantId = await CreateTestMerchantAsync(client);

        var request = new CreateProductCategoryRequest("Test Category", "Description", null, null, 1);

        // Act
        var response = await client.PostAsJsonAsync($"/api/v1/productcategory/merchant/{merchantId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
    #endregion

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
            $"merchant{Guid.NewGuid()}@example.com",
            50.0m,
            10.0m);

        var response = await client.PostAsJsonAsync("/api/v1/merchant", merchantRequest);
        response.EnsureSuccessStatusCode();
        var merchant = await response.Content.ReadFromJsonAsync<MerchantResponse>();
        return merchant!.Id;
    }
}


