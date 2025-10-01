using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenMinutes;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _accessTokenMinutes = 60; // Bu değerler configuration'dan gelecek şekilde iyileştirilebilir
        _refreshTokenMinutes = 10080; // 7 days
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        // Email zaten var mı kontrol et
        var existingUser = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

        if (existingUser != null)
        {
            return Result.Fail<AuthResponse>("A user with this email already exists", "AUTH_EMAIL_EXISTS");
        }

        // Yeni kullanıcı oluştur
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Token oluştur
        var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email);
        var refreshTokenValue = _jwtTokenService.CreateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenMinutes),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new AuthResponse(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(_accessTokenMinutes)
        ));
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.ReadRepository<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

        if (user == null)
        {
            return Result.Fail<AuthResponse>("Invalid email or password", "AUTH_INVALID_CREDENTIALS");
        }

        if (!user.IsActive)
        {
            return Result.Fail<AuthResponse>("Your account has been deactivated", "AUTH_ACCOUNT_DEACTIVATED");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result.Fail<AuthResponse>("Invalid email or password", "AUTH_INVALID_CREDENTIALS");
        }

        // LastLogin güncelle
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Repository<User>().Update(user);

        // Token oluştur
        var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email);
        var refreshTokenValue = _jwtTokenService.CreateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenMinutes),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new AuthResponse(
            accessToken,
            refreshTokenValue,
            DateTime.UtcNow.AddMinutes(_accessTokenMinutes)
        ));
    }

    public async Task<Result<AuthResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _unitOfWork.ReadRepository<RefreshToken>()
            .FirstOrDefaultAsync(
                rt => rt.Token == request.RefreshToken && !rt.IsRevoked,
                include: "User",
                cancellationToken: cancellationToken);

        if (refreshToken == null)
        {
            return Result.Fail<AuthResponse>("Invalid refresh token", "AUTH_INVALID_REFRESH_TOKEN");
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result.Fail<AuthResponse>("Refresh token has expired", "AUTH_REFRESH_TOKEN_EXPIRED");
        }

        // Eski token'ı iptal et
        var oldToken = await _unitOfWork.Repository<RefreshToken>()
            .GetByIdAsync(refreshToken.Id, cancellationToken);
        
        if (oldToken != null)
        {
            oldToken.IsRevoked = true;
            oldToken.RevokedAt = DateTime.UtcNow;
            _unitOfWork.Repository<RefreshToken>().Update(oldToken);
        }

        // Yeni token oluştur
        var accessToken = _jwtTokenService.CreateAccessToken(refreshToken.UserId, refreshToken.User.Email);
        var newRefreshTokenValue = _jwtTokenService.CreateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = refreshToken.UserId,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_refreshTokenMinutes),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(new AuthResponse(
            accessToken,
            newRefreshTokenValue,
            DateTime.UtcNow.AddMinutes(_accessTokenMinutes)
        ));
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _unitOfWork.ReadRepository<RefreshToken>()
            .ListAsync(rt => rt.UserId == userId && !rt.IsRevoked, cancellationToken: cancellationToken);

        if (tokens.Any())
        {
            foreach (var token in tokens)
            {
                var tokenToRevoke = await _unitOfWork.Repository<RefreshToken>()
                    .GetByIdAsync(token.Id, cancellationToken);
                
                if (tokenToRevoke != null)
                {
                    tokenToRevoke.IsRevoked = true;
                    tokenToRevoke.RevokedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<RefreshToken>().Update(tokenToRevoke);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
