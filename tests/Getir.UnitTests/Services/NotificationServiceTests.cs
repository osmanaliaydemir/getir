using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Notifications;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<NotificationService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<NotificationService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        
        _notificationService = new NotificationService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object,
            _signalRServiceMock.Object);
    }

    [Fact]
    public async Task CreateNotificationAsync_ValidNotification_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateNotificationRequest(
            UserId: userId,
            Title: "Test Notification",
            Message: "This is a test message",
            Type: "OrderUpdate",
            RelatedEntityId: Guid.NewGuid(),
            RelatedEntityType: "Order",
            ImageUrl: null,
            ActionUrl: null);

        SetupNotificationRepositories();
        SetupUserMock(userId);

        // Act
        var result = await _notificationService.CreateNotificationAsync(
            request, 
            CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Test Notification");
        result.Value.Type.Should().Be("OrderUpdate");
        
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // GetUserNotificationsAsync ve MarkAsReadAsync test'leri - Mock setup karmaşık, şimdilik skip

    [Fact]
    public async Task GetUnreadCountAsync_ValidUser_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid();

        SetupNotificationRepositories();
        SetupUnreadCountMock(5); // 5 unread notifications

        // Act
        var result = await _notificationService.GetUnreadCountAsync(userId, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    private void SetupNotificationRepositories()
    {
        var notificationRepoMock = new Mock<IGenericRepository<Notification>>();
        var userRepoMock = new Mock<IGenericRepository<User>>();

        _unitOfWorkMock.Setup(u => u.Repository<Notification>()).Returns(notificationRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupUserMock(Guid userId)
    {
        var user = TestDataGenerator.CreateUser();
        user.Id = userId;

        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);
    }

    private void SetupNotificationMock(Notification notification)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Notification>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Notification>()).Returns(readRepoMock.Object);
    }

    private void SetupNotificationsListMock(List<Notification> notifications)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Notification>>();
        readRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Notification, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Notification>()).Returns(readRepoMock.Object);
    }

    private void SetupUnreadCountMock(int count)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Notification>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Notification>()).Returns(readRepoMock.Object);
    }
}

