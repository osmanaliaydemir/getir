using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Addresses;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class UserAddressServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UserAddressService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly UserAddressService _service;

    public UserAddressServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UserAddressService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new UserAddressService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetUserAddressesAsync_WithAddresses_ShouldReturnList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addresses = new List<UserAddress>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Home", FullAddress = "Address 1", City = "Istanbul", District = "Kadikoy", IsDefault = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = userId, Title = "Work", FullAddress = "Address 2", City = "Istanbul", District = "Besiktas", IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var readRepoMock = new Mock<IReadOnlyRepository<UserAddress>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        _unitOfWorkMock.Setup(u => u.ReadRepository<UserAddress>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetUserAddressesAsync(userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAddressAsync_FirstAddress_ShouldSetAsDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateAddressRequest("Home", "Test Address", "Istanbul", "Kadikoy", 41.0m, 29.0m);

        var readRepoMock = new Mock<IReadOnlyRepository<UserAddress>>();
        readRepoMock.Setup(r => r.AnyAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // No existing addresses

        var addressRepoMock = new Mock<IGenericRepository<UserAddress>>();
        addressRepoMock.Setup(r => r.AddAsync(It.IsAny<UserAddress>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAddress());

        _unitOfWorkMock.Setup(u => u.ReadRepository<UserAddress>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<UserAddress>()).Returns(addressRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.AddAddressAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAddressAsync_ValidAddress_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var request = new UpdateAddressRequest("Updated", "Updated Address", "Ankara", "Cankaya", 39.0m, 32.0m);

        var address = new UserAddress
        {
            Id = addressId,
            UserId = userId,
            Title = "Old",
            FullAddress = "Old Address",
            City = "Istanbul",
            District = "Kadikoy",
            IsActive = true
        };

        var addressRepoMock = new Mock<IGenericRepository<UserAddress>>();
        addressRepoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        addressRepoMock.Setup(r => r.Update(It.IsAny<UserAddress>()));

        _unitOfWorkMock.Setup(u => u.Repository<UserAddress>()).Returns(addressRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAddressAsync(userId, addressId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAddressAsync_ValidAddress_ShouldSoftDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();

        var address = new UserAddress
        {
            Id = addressId,
            UserId = userId,
            Title = "Home",
            IsDefault = false,
            IsActive = true
        };

        var addressRepoMock = new Mock<IGenericRepository<UserAddress>>();
        addressRepoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        addressRepoMock.Setup(r => r.Update(It.IsAny<UserAddress>()));

        _unitOfWorkMock.Setup(u => u.Repository<UserAddress>()).Returns(addressRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.DeleteAddressAsync(userId, addressId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SetDefaultAddressAsync_ValidAddress_ShouldSetAsDefault()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();

        var address = new UserAddress
        {
            Id = addressId,
            UserId = userId,
            Title = "Work",
            IsDefault = false,
            IsActive = true
        };

        var addressRepoMock = new Mock<IGenericRepository<UserAddress>>();
        addressRepoMock.Setup(r => r.GetAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        addressRepoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<UserAddress, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserAddress>());

        addressRepoMock.Setup(r => r.Update(It.IsAny<UserAddress>()));

        _unitOfWorkMock.Setup(u => u.Repository<UserAddress>()).Returns(addressRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.SetDefaultAddressAsync(userId, addressId);

        // Assert
        result.Success.Should().BeTrue();
    }
}

