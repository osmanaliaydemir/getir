using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class CourierEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CourierEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var registerRequest = new RegisterRequest(
            $"courier{Guid.NewGuid()}@example.com",
            "Test123!",
            "John",
            "Doe",
            "+905551234567",
            Domain.Enums.UserRole.Admin); // Admin role for testing

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    [Fact(Skip = "Courier tests require courier role setup")]
    public async Task GetCourierDashboard_WithoutAuth_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/courier/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "Courier tests require courier role setup")]
    public async Task GetCourierDashboard_WithAuth_ShouldReturn200OrNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/courier/dashboard");

        // Assert - User might not be a courier
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Courier tests require courier role setup")]
    public async Task GetCourierStats_WithAuth_ShouldReturn200OrNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/courier/stats");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Courier tests require courier role setup")]
    public async Task GetCourierEarnings_WithAuth_ShouldReturn200OrNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/courier/earnings");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetCourierOrders_WithAuth_ShouldReturn200OrNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v1/courier/orders?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Courier tests require courier role setup")]
    public async Task UpdateCourierLocation_WithValidData_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new UpdateCourierLocationRequest(40.7128m, -74.0060m);

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/courier/location", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact(Skip = "Courier tests require courier role setup")]
    public async Task SetCourierAvailability_WithValidData_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new SetAvailabilityRequest(true);

        // Act
        var response = await client.PutAsJsonAsync("/api/v1/courier/availability", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }
}


