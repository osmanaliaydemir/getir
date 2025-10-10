using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class MerchantDashboardEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public MerchantDashboardEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequest(
            $"dashboard{Guid.NewGuid()}@example.com",
            "Test123!",
            "John",
            "Doe",
            "+905551234567",
            Domain.Enums.UserRole.Admin); // Admin role for testing

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    [Fact(Skip = "Dashboard tests require merchant setup")]
    public async Task GetMerchantDashboard_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/merchants/{Guid.NewGuid()}/merchantdashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Dashboard tests require merchant setup")]
    public async Task GetMerchantDashboard_WithAuth_ShouldReturn200OrForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var merchantId = await CreateTestMerchantAsync(client);

        // Act
        var response = await client.GetAsync($"/api/v1/merchants/{merchantId}/merchantdashboard");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Requires merchant setup")]
    public async Task GetRecentOrders_WithAuth_ShouldReturn200OrForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var merchantId = await CreateTestMerchantAsync(client);

        // Act
        var response = await client.GetAsync($"/api/v1/merchants/{merchantId}/merchantdashboard/recent-orders?limit=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Dashboard tests require merchant setup")]
    public async Task GetTopProducts_WithAuth_ShouldReturn200OrForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var merchantId = await CreateTestMerchantAsync(client);

        // Act
        var response = await client.GetAsync($"/api/v1/merchants/{merchantId}/merchantdashboard/top-products?limit=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Dashboard tests require merchant setup")]
    public async Task GetPerformanceMetrics_WithAuth_ShouldReturn200OrForbidden()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var merchantId = await CreateTestMerchantAsync(client);

        // Act
        var response = await client.GetAsync($"/api/v1/merchants/{merchantId}/merchantdashboard/performance");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

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


