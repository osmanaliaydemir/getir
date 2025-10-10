using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.IntegrationTests.Setup;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class CampaignEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CampaignEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActiveCampaigns_WithoutAuth_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/campaign?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<CampaignResponse>>();
        pagedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveCampaigns_WithPagination_ShouldRespectPageSize()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/campaign?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResult = await response.Content.ReadFromJsonAsync<PagedResult<CampaignResponse>>();
        pagedResult.Should().NotBeNull();
        pagedResult!.PageSize.Should().Be(5);
    }
}


