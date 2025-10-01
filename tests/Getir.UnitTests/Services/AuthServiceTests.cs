using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.DTO;
using Getir.Application.Services.Auth;
using Getir.Domain.Entities;
using Getir.UnitTests.Helpers;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        
        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _jwtTokenServiceMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ShouldSucceed()
    {
        // Arrange
        var request = new RegisterRequest(
            "test@example.com",
            "Test123!",
            "John",
            "Doe",
            "+905551234567");

        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var userRepoMock = new Mock<IGenericRepository<User>>();
        var refreshTokenRepoMock = new Mock<IGenericRepository<RefreshToken>>();

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<RefreshToken>()).Returns(refreshTokenRepoMock.Object);
        
        _passwordHasherMock.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");
        
        _jwtTokenServiceMock.Setup(j => j.CreateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), null))
            .Returns("access_token");
        
        _jwtTokenServiceMock.Setup(j => j.CreateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        
        userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldFail()
    {
        // Arrange
        var request = new RegisterRequest("existing@example.com", "Test123!", "John", "Doe", null);
        var existingUser = TestDataGenerator.CreateUser("existing@example.com");

        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("AUTH_EMAIL_EXISTS");
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Test123!");
        var user = TestDataGenerator.CreateUser("test@example.com", "hashed_password");

        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var userRepoMock = new Mock<IGenericRepository<User>>();
        var refreshTokenRepoMock = new Mock<IGenericRepository<RefreshToken>>();

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<RefreshToken>()).Returns(refreshTokenRepoMock.Object);

        _passwordHasherMock.Setup(p => p.VerifyPassword("Test123!", "hashed_password"))
            .Returns(true);

        _jwtTokenServiceMock.Setup(j => j.CreateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), null))
            .Returns("access_token");

        _jwtTokenServiceMock.Setup(j => j.CreateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access_token");
        
        userRepoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "WrongPassword");
        var user = TestDataGenerator.CreateUser("test@example.com", "hashed_password");

        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);

        _passwordHasherMock.Setup(p => p.VerifyPassword("WrongPassword", "hashed_password"))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("AUTH_INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ShouldFail()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Test123!");
        var user = TestDataGenerator.CreateUser("test@example.com");
        user.IsActive = false;

        var readRepoMock = new Mock<IReadOnlyRepository<User>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _unitOfWorkMock.Setup(u => u.ReadRepository<User>()).Returns(readRepoMock.Object);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("AUTH_ACCOUNT_DEACTIVATED");
    }
}
