using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class CategoryEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CategoryEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ShouldReturnPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/categories?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCategory_WithAuth_ShouldSucceed()
    {
        // Arrange - Get auth token
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateCategoryRequest(
            "New Category",
            "Test Description",
            "https://example.com/image.jpg",
            1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categoryResponse = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        categoryResponse.Should().NotBeNull();
        categoryResponse!.Name.Should().Be("New Category");
    }

    [Fact]
    public async Task CreateCategory_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        var request = new CreateCategoryRequest("Test", null, null, 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequest(
            $"test{Guid.NewGuid()}@example.com",
            "Test123!",
            "Test",
            "User",
            null);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        return authResponse!.AccessToken;
    }
}
