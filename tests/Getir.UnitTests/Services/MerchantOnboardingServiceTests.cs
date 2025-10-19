using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Getir.UnitTests.Services;

public class MerchantOnboardingServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<MerchantOnboardingService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly MerchantOnboardingService _service;

    public MerchantOnboardingServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<MerchantOnboardingService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();

        _service = new MerchantOnboardingService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object);
    }

    #region CreateOnboardingAsync Tests

    [Fact]
    public async Task CreateOnboardingAsync_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new CreateMerchantOnboardingRequest(merchantId, ownerId);

        SetupMerchantExists(merchantId, ownerId);
        SetupNoExistingOnboarding(merchantId);
        SetupOnboardingRepository();

        // Act
        var result = await _service.CreateOnboardingAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MerchantId.Should().Be(merchantId);
        result.Value.OwnerId.Should().Be(ownerId);
        result.Value.CompletedSteps.Should().Be(0);
        result.Value.TotalSteps.Should().Be(6);
        result.Value.ProgressPercentage.Should().Be(0);
        result.Value.IsVerified.Should().BeFalse();
        result.Value.IsApproved.Should().BeFalse();
    }

    [Fact]
    public async Task CreateOnboardingAsync_MerchantNotFound_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new CreateMerchantOnboardingRequest(merchantId, ownerId);

        SetupMerchantNotExists();

        // Act
        var result = await _service.CreateOnboardingAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf("NOT_FOUND_MERCHANT", "MERCHANT_NOT_FOUND", "ACCESS_DENIED");
    }

    [Fact]
    public async Task CreateOnboardingAsync_OnboardingAlreadyExists_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var request = new CreateMerchantOnboardingRequest(merchantId, ownerId);

        SetupMerchantExists(merchantId, ownerId);
        SetupExistingOnboarding(merchantId);

        // Act
        var result = await _service.CreateOnboardingAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ONBOARDING_EXISTS");
    }

    #endregion

    #region GetOnboardingStatusAsync Tests

    [Fact]
    public async Task GetOnboardingStatusAsync_ValidMerchant_ShouldReturnStatus()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId);

        SetupOnboardingExists(onboarding);

        // Act
        var result = await _service.GetOnboardingStatusAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MerchantId.Should().Be(merchantId);
    }

    [Fact]
    public async Task GetOnboardingStatusAsync_OnboardingNotFound_ShouldFail()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupOnboardingNotExists();

        // Act
        var result = await _service.GetOnboardingStatusAsync(merchantId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().BeOneOf("NOT_FOUND_ONBOARDING", "ONBOARDING_NOT_FOUND");
    }

    #endregion

    #region UpdateOnboardingStepAsync Tests

    [Fact]
    public async Task UpdateOnboardingStepAsync_ValidStep_ShouldUpdateProgress()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        var request = new UpdateOnboardingStepRequest(onboardingId, "BasicInfo", true);

        SetupOnboardingByIdExists(onboardingId, onboarding);
        SetupOnboardingUpdateRepository();

        // Act
        var result = await _service.UpdateOnboardingStepAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.BasicInfoCompleted.Should().BeTrue();
        result.Value.CompletedSteps.Should().BeGreaterThan(0);
        result.Value.ProgressPercentage.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateOnboardingStepAsync_AllStepsCompleted_ShouldAutoVerify()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        onboarding.BasicInfoCompleted = true;
        onboarding.BusinessInfoCompleted = true;
        onboarding.WorkingHoursCompleted = true;
        onboarding.DeliveryZonesCompleted = true;
        onboarding.ProductsAdded = true;
        // Only Documents left
        var request = new UpdateOnboardingStepRequest(onboardingId, "Documents", true);

        SetupOnboardingByIdExists(onboardingId, onboarding);
        SetupOnboardingUpdateRepository();

        // Act
        var result = await _service.UpdateOnboardingStepAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.IsVerified.Should().BeTrue();
        result.Value.VerifiedAt.Should().NotBeNull();
        result.Value.CompletedSteps.Should().Be(6);
        result.Value.ProgressPercentage.Should().Be(100);
    }

    [Fact]
    public async Task UpdateOnboardingStepAsync_InvalidStepName_ShouldFail()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        var request = new UpdateOnboardingStepRequest(onboardingId, "InvalidStep", true);

        SetupOnboardingByIdExists(onboardingId, onboarding);

        // Act
        var result = await _service.UpdateOnboardingStepAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_STEP");
    }

    #endregion

    #region CompleteOnboardingAsync Tests

    [Fact]
    public async Task CompleteOnboardingAsync_AllStepsCompleted_ShouldSucceed()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        onboarding.CompletedSteps = 6;
        var request = new CompleteOnboardingRequest(onboardingId);

        SetupOnboardingByIdExists(onboardingId, onboarding);
        SetupOnboardingUpdateRepository();

        // Act
        var result = await _service.CompleteOnboardingAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteOnboardingAsync_IncompleteSteps_ShouldFail()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        onboarding.CompletedSteps = 3; // Only 3/6 completed
        var request = new CompleteOnboardingRequest(onboardingId);

        SetupOnboardingByIdExists(onboardingId, onboarding);

        // Act
        var result = await _service.CompleteOnboardingAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INCOMPLETE_ONBOARDING");
    }

    #endregion

    #region ApproveMerchantAsync Tests

    [Fact]
    public async Task ApproveMerchantAsync_Approved_ShouldActivateMerchant()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var merchant = new Merchant
        {
            Id = merchantId,
            Name = "Test Merchant",
            IsActive = false
        };
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        onboarding.IsVerified = true;
        onboarding.Merchant = merchant;

        var request = new ApproveMerchantRequest(onboardingId, true);

        SetupOnboardingByIdExistsWithMerchant(onboardingId, onboarding);
        SetupOnboardingUpdateRepository();
        SetupMerchantUpdateRepository();

        // Act
        var result = await _service.ApproveMerchantAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveMerchantAsync_Rejected_ShouldSetRejectionReason()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var merchant = new Merchant
        {
            Id = merchantId,
            Name = "Test Merchant",
            IsActive = false
        };
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        onboarding.IsVerified = true;
        onboarding.Merchant = merchant;

        var request = new ApproveMerchantRequest(onboardingId, false, "Documents incomplete");

        SetupOnboardingByIdExistsWithMerchant(onboardingId, onboarding);
        SetupOnboardingUpdateRepository();

        // Act
        var result = await _service.ApproveMerchantAsync(request);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveMerchantAsync_NotVerified_ShouldFail()
    {
        // Arrange
        var onboardingId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var merchant = new Merchant
        {
            Id = merchantId,
            Name = "Test Merchant",
            IsActive = false
        };
        var onboarding = CreateSampleOnboarding(merchantId, onboardingId);
        onboarding.IsVerified = false; // Not verified yet
        onboarding.Merchant = merchant;

        var request = new ApproveMerchantRequest(onboardingId, true);

        SetupOnboardingByIdExistsWithMerchant(onboardingId, onboarding);

        // Act
        var result = await _service.ApproveMerchantAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_VERIFIED");
    }

    #endregion

    #region CanMerchantStartTradingAsync Tests

    [Fact]
    public async Task CanMerchantStartTradingAsync_VerifiedAndApproved_ShouldReturnTrue()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId);
        onboarding.IsVerified = true;
        onboarding.IsApproved = true;

        SetupOnboardingExists(onboarding);

        // Act
        var result = await _service.CanMerchantStartTradingAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CanMerchantStartTradingAsync_NotApproved_ShouldReturnFalse()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var onboarding = CreateSampleOnboarding(merchantId);
        onboarding.IsVerified = true;
        onboarding.IsApproved = false;

        SetupOnboardingExists(onboarding);

        // Act
        var result = await _service.CanMerchantStartTradingAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task CanMerchantStartTradingAsync_NoOnboarding_ShouldReturnFalse()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        SetupOnboardingNotExists();

        // Act
        var result = await _service.CanMerchantStartTradingAsync(merchantId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private MerchantOnboarding CreateSampleOnboarding(Guid merchantId, Guid? id = null)
    {
        return new MerchantOnboarding
        {
            Id = id ?? Guid.NewGuid(),
            MerchantId = merchantId,
            OwnerId = Guid.NewGuid(),
            BasicInfoCompleted = false,
            BusinessInfoCompleted = false,
            WorkingHoursCompleted = false,
            DeliveryZonesCompleted = false,
            ProductsAdded = false,
            DocumentsUploaded = false,
            IsVerified = false,
            IsApproved = false,
            CompletedSteps = 0,
            TotalSteps = 6,
            ProgressPercentage = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void SetupMerchantExists(Guid merchantId, Guid ownerId)
    {
        var merchantRepo = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant
            {
                Id = merchantId,
                OwnerId = ownerId,
                Name = "Test Merchant"
            });
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepo.Object);
    }

    private void SetupMerchantNotExists()
    {
        var merchantRepo = new Mock<IReadOnlyRepository<Merchant>>();
        merchantRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Merchant, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Merchant?)null);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Merchant>()).Returns(merchantRepo.Object);
    }

    private void SetupNoExistingOnboarding(Guid merchantId)
    {
        var onboardingRepo = new Mock<IReadOnlyRepository<MerchantOnboarding>>();
        onboardingRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MerchantOnboarding, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantOnboarding?)null);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MerchantOnboarding>()).Returns(onboardingRepo.Object);
    }

    private void SetupExistingOnboarding(Guid merchantId)
    {
        var onboardingRepo = new Mock<IReadOnlyRepository<MerchantOnboarding>>();
        onboardingRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MerchantOnboarding, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSampleOnboarding(merchantId));
        _unitOfWorkMock.Setup(u => u.ReadRepository<MerchantOnboarding>()).Returns(onboardingRepo.Object);
    }

    private void SetupOnboardingExists(MerchantOnboarding onboarding)
    {
        var onboardingRepo = new Mock<IReadOnlyRepository<MerchantOnboarding>>();
        onboardingRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MerchantOnboarding, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(onboarding);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MerchantOnboarding>()).Returns(onboardingRepo.Object);
    }

    private void SetupOnboardingNotExists()
    {
        var onboardingRepo = new Mock<IReadOnlyRepository<MerchantOnboarding>>();
        onboardingRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MerchantOnboarding, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantOnboarding?)null);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MerchantOnboarding>()).Returns(onboardingRepo.Object);
    }

    private void SetupOnboardingByIdExists(Guid onboardingId, MerchantOnboarding onboarding)
    {
        var onboardingRepo = new Mock<IReadOnlyRepository<MerchantOnboarding>>();
        onboardingRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MerchantOnboarding, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(onboarding);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MerchantOnboarding>()).Returns(onboardingRepo.Object);
    }

    private void SetupOnboardingByIdExistsWithMerchant(Guid onboardingId, MerchantOnboarding onboarding)
    {
        var onboardingRepo = new Mock<IReadOnlyRepository<MerchantOnboarding>>();
        onboardingRepo
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<MerchantOnboarding, bool>>>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(onboarding);
        _unitOfWorkMock.Setup(u => u.ReadRepository<MerchantOnboarding>()).Returns(onboardingRepo.Object);
    }

    private void SetupOnboardingRepository()
    {
        var genericRepo = new Mock<IGenericRepository<MerchantOnboarding>>();
        genericRepo
            .Setup(r => r.AddAsync(It.IsAny<MerchantOnboarding>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantOnboarding entity, CancellationToken ct) => entity);
        _unitOfWorkMock.Setup(u => u.Repository<MerchantOnboarding>()).Returns(genericRepo.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupOnboardingUpdateRepository()
    {
        var genericRepo = new Mock<IGenericRepository<MerchantOnboarding>>();
        genericRepo.Setup(r => r.Update(It.IsAny<MerchantOnboarding>()));
        _unitOfWorkMock.Setup(u => u.Repository<MerchantOnboarding>()).Returns(genericRepo.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupMerchantUpdateRepository()
    {
        var genericRepo = new Mock<IGenericRepository<Merchant>>();
        genericRepo.Setup(r => r.Update(It.IsAny<Merchant>()));
        _unitOfWorkMock.Setup(u => u.Repository<Merchant>()).Returns(genericRepo.Object);
    }

    #endregion
}

