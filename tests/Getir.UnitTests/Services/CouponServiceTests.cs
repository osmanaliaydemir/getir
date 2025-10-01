using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Coupons;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class CouponServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CouponService _couponService;

    public CouponServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _couponService = new CouponService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ValidateCouponAsync_ValidPercentageCoupon_ShouldCalculateDiscount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var coupon = TestDataGenerator.CreateCoupon("WELCOME50", "Percentage");
        coupon.DiscountValue = 20; // %20
        coupon.MinimumOrderAmount = 50;
        coupon.MaximumDiscountAmount = 100;

        var request = new ValidateCouponRequest("WELCOME50", 200); // 200 TL sipariş

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.ValidateCouponAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsValid.Should().BeTrue();
        result.Value.DiscountAmount.Should().Be(40); // %20 of 200 = 40
    }

    [Fact]
    public async Task ValidateCouponAsync_ExceedsMaxDiscount_ShouldCapDiscount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var coupon = TestDataGenerator.CreateCoupon("BIGDISCOUNT", "Percentage");
        coupon.DiscountValue = 50; // %50
        coupon.MinimumOrderAmount = 100;
        coupon.MaximumDiscountAmount = 50; // Max 50 TL

        var request = new ValidateCouponRequest("BIGDISCOUNT", 200); // 200 TL sipariş

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.ValidateCouponAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.DiscountAmount.Should().Be(50); // Capped at 50, not 100 (50% of 200)
    }

    [Fact]
    public async Task ValidateCouponAsync_FixedAmountCoupon_ShouldReturnFixedDiscount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var coupon = TestDataGenerator.CreateCoupon("FIXED25", "FixedAmount");
        coupon.DiscountValue = 25; // 25 TL sabit indirim
        coupon.MinimumOrderAmount = 50;

        var request = new ValidateCouponRequest("FIXED25", 100);

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.ValidateCouponAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.DiscountAmount.Should().Be(25); // Fixed amount
    }

    [Fact]
    public async Task ValidateCouponAsync_BelowMinimumAmount_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var coupon = TestDataGenerator.CreateCoupon();
        coupon.MinimumOrderAmount = 100;

        var request = new ValidateCouponRequest(coupon.Code, 50); // Only 50 TL

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.ValidateCouponAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.ErrorMessage.Should().Contain("Minimum order amount");
    }

    [Fact]
    public async Task ValidateCouponAsync_ExpiredCoupon_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var coupon = TestDataGenerator.CreateCoupon();
        coupon.StartDate = DateTime.UtcNow.AddDays(-30);
        coupon.EndDate = DateTime.UtcNow.AddDays(-1); // Expired yesterday

        var request = new ValidateCouponRequest(coupon.Code, 100);

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.ValidateCouponAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task ValidateCouponAsync_UsageLimitReached_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var coupon = TestDataGenerator.CreateCoupon();
        coupon.UsageLimit = 100;
        coupon.UsageCount = 100; // Limit reached

        var request = new ValidateCouponRequest(coupon.Code, 150);

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(coupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.ValidateCouponAsync(userId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.ErrorMessage.Should().Contain("usage limit");
    }

    [Fact]
    public async Task CreateCouponAsync_WithUniqueCode_ShouldSucceed()
    {
        // Arrange
        var request = new CreateCouponRequest(
            "NEWCODE123",
            "Test Coupon",
            "Description",
            "Percentage",
            20,
            50,
            100,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            null);

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Coupon?)null);

        var couponRepoMock = new Mock<IGenericRepository<Coupon>>();

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Coupon>()).Returns(couponRepoMock.Object);

        // Act
        var result = await _couponService.CreateCouponAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be("NEWCODE123");
        
        couponRepoMock.Verify(r => r.AddAsync(It.IsAny<Coupon>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCouponAsync_WithDuplicateCode_ShouldFail()
    {
        // Arrange
        var existingCoupon = TestDataGenerator.CreateCoupon("DUPLICATE");
        var request = new CreateCouponRequest(
            "DUPLICATE",
            "Test",
            null,
            "Percentage",
            10,
            0,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            null);

        var readRepoMock = new Mock<IReadOnlyRepository<Coupon>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Coupon, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCoupon);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Coupon>()).Returns(readRepoMock.Object);

        // Act
        var result = await _couponService.CreateCouponAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("CONFLICT_COUPON_CODE");
    }
}
