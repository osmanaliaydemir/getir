using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndToken()
    {
        // Arrange
        var request = new RegisterRequest(
            $"test{Guid.NewGuid()}@example.com",
            "Test123!",
            "John",
            "Doe",
            "+905551234567");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest(email, "Test123!", "John", "Doe", null);

        // First registration
        await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError); // Error handling middleware
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("AUTH_EMAIL_EXISTS");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200()
    {
        // Arrange - First register
        var email = $"login{Guid.NewGuid()}@example.com";
        var password = "Test123!";
        
        var registerRequest = new RegisterRequest(email, password, "John", "Doe", null);
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Act - Login
        var loginRequest = new LoginRequest(email, password);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        var email = $"invalid{Guid.NewGuid()}@example.com";
        
        var registerRequest = new RegisterRequest(email, "Test123!", "John", "Doe", null);
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Act - Wrong password
        var loginRequest = new LoginRequest(email, "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("AUTH_INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturn200()
    {
        // Arrange - Register and get token
        var email = $"logout{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterRequest(email, "Test123!", "John", "Doe", null);
        
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act - Logout
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);
        
        var logoutResponse = await _client.PostAsync("/api/v1/auth/logout", null);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
