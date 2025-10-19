using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.ProductOptions;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class ProductOptionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProductOptionService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly ProductOptionService _service;

    public ProductOptionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductOptionService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();
        
        _service = new ProductOptionService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    [Fact]
    public async Task GetProductOptionsAsync_ValidGroup_ShouldReturnOptions()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var query = new PaginationQuery { Page = 1, PageSize = 10 };
        var options = new List<ProductOption>
        {
            new() { Id = Guid.NewGuid(), ProductOptionGroupId = groupId, Name = "Option 1", ExtraPrice = 5m, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        var repoMock = new Mock<IGenericRepository<ProductOption>>();
        repoMock.Setup(r => r.GetPagedAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOption, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOption, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(options);

        var readRepoMock = new Mock<IReadOnlyRepository<ProductOption>>();
        readRepoMock.Setup(r => r.CountAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOption, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(u => u.Repository<ProductOption>()).Returns(repoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<ProductOption>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetProductOptionsAsync(groupId, query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProductOptionAsync_ValidId_ShouldReturnOption()
    {
        // Arrange
        var optionId = Guid.NewGuid();
        var option = new ProductOption { Id = optionId, Name = "Test Option", ExtraPrice = 10m, CreatedAt = DateTime.UtcNow };

        var readRepoMock = new Mock<IReadOnlyRepository<ProductOption>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOption, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(option);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ProductOption>()).Returns(readRepoMock.Object);

        // Act
        var result = await _service.GetProductOptionAsync(optionId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Name.Should().Be("Test Option");
    }

    [Fact]
    public async Task CreateProductOptionAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateProductOptionRequest(groupId, "New Option", "Description", 5m, false, 1);

        var groupReadRepoMock = new Mock<IReadOnlyRepository<ProductOptionGroup>>();
        groupReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOptionGroup, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductOptionGroup 
            { 
                Id = groupId, 
                Product = new Product 
                { 
                    Merchant = new Merchant { OwnerId = userId } 
                } 
            });

        var optionRepoMock = new Mock<IGenericRepository<ProductOption>>();
        optionRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOption, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOption, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductOption>());

        optionRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductOption>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductOption());

        _unitOfWorkMock.Setup(u => u.ReadRepository<ProductOptionGroup>()).Returns(groupReadRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<ProductOption>()).Returns(optionRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateProductOptionAsync(request, userId);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProductOptionAsync_GroupNotFound_ShouldFail()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateProductOptionRequest(groupId, "New Option", "Description", 5m, false, 1);

        var groupReadRepoMock = new Mock<IReadOnlyRepository<ProductOptionGroup>>();
        groupReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductOptionGroup, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductOptionGroup?)null);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ProductOptionGroup>()).Returns(groupReadRepoMock.Object);

        // Act
        var result = await _service.CreateProductOptionAsync(request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND_OPTION_GROUP");
    }
}

