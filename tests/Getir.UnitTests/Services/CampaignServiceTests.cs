using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Campaigns;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class CampaignServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CampaignService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly CampaignService _service;

    public CampaignServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CampaignService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new CampaignService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetActiveCampaignsAsync_WithActiveCampaigns_ShouldReturnList()
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = 10 };
        var now = DateTime.UtcNow;
        var campaigns = new List<Campaign>
        {
            new() { Id = Guid.NewGuid(), Title = "Campaign 1", IsActive = true, StartDate = now.AddDays(-1), EndDate = now.AddDays(7), Merchant = new Merchant { Name = "Test" } },
            new() { Id = Guid.NewGuid(), Title = "Campaign 2", IsActive = true, StartDate = now.AddDays(-2), EndDate = now.AddDays(5), Merchant = new Merchant { Name = "Test" } }
        };

        var campaignRepoMock = new Mock<IGenericRepository<Campaign>>();
        campaignRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Campaign, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Campaign, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var readRepoMock = new Mock<IReadOnlyRepository<Campaign>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Campaign, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _unitOfWorkMock.Setup(u => u.Repository<Campaign>()).Returns(campaignRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Campaign>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetActiveCampaignsAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetActiveCampaignsAsync_NoActiveCampaigns_ShouldReturnEmpty()
    {
        // Arrange
        var query = new PaginationQuery { Page = 1, PageSize = 10 };

        var campaignRepoMock = new Mock<IGenericRepository<Campaign>>();
        campaignRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Campaign, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Campaign, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Campaign>());

        var readRepoMock = new Mock<IReadOnlyRepository<Campaign>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Campaign, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _unitOfWorkMock.Setup(u => u.Repository<Campaign>()).Returns(campaignRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Campaign>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetActiveCampaignsAsync(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }
}

