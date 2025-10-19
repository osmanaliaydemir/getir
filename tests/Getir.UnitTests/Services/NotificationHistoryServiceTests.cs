using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainEntities = Getir.Domain.Entities;
using DomainEnums = Getir.Domain.Enums;
using DTO = Getir.Application.DTO;

namespace Getir.UnitTests.Services;

public class NotificationHistoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<NotificationHistoryService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly NotificationHistoryService _service;

    public NotificationHistoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<NotificationHistoryService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        _service = new NotificationHistoryService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task LogNotificationAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new LogNotificationRequest(
            Guid.NewGuid(),
            "Test",
            "Message",
            DomainEnums.NotificationType.OrderUpdate,
            DTO.NotificationChannel.Push,
            DTO.NotificationStatus.Sent);

        var repoMock = new Mock<IGenericRepository<NotificationHistory>>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<NotificationHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationHistory());

        _unitOfWorkMock.Setup(u => u.Repository<NotificationHistory>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.LogNotificationAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetNotificationHistoryAsync_ValidUser_ShouldReturnHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new NotificationHistoryQuery(null, null, null, null, null, 1, 10);
        
        var histories = new List<NotificationHistory>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Test", Message = "Msg", Type = Domain.Enums.NotificationType.OrderUpdate, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<NotificationHistory>>();
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<NotificationHistory, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotificationHistory, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<NotificationHistory, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.ReadRepository<NotificationHistory>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetNotificationHistoryAsync(userId, query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetNotificationStatisticsAsync_ValidUser_ShouldReturnStats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var histories = new List<NotificationHistory>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Status = DomainEntities.NotificationStatus.Sent, Channel = DomainEntities.NotificationChannel.Email, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = userId, Status = DomainEntities.NotificationStatus.Delivered, Channel = DomainEntities.NotificationChannel.Push, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<NotificationHistory>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<NotificationHistory, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotificationHistory, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        _unitOfWorkMock.Setup(u => u.ReadRepository<NotificationHistory>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetNotificationStatisticsAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.TotalNotifications.Should().Be(2);
    }
}

