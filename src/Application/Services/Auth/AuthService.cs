using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Auth;

public class AuthService : BaseService, IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IEmailService _emailService;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenMinutes;

    public AuthService(
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IBackgroundTaskService backgroundTaskService,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IEmailService emailService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _backgroundTaskService = backgroundTaskService;
        _emailService = emailService;
        _accessTokenMinutes = 60; // Bu değerler configuration'dan gelecek şekilde iyileştirilebilir
        _refreshTokenMinutes = 10080; // 7 days
    }

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await RegisterInternalAsync(request, cancellationToken),
            "UserRegistration",
            new { Email = request.Email, Role = request.Role?.ToString() },
            cancellationToken);
    }

    private async Task<Result<AuthResponse>> RegisterInternalAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Email zaten var mı kontrol et
            var existingUser = await _unitOfWork.ReadRepository<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

            if (existingUser != null)
            {
                _loggingService.LogSecurityEvent("RegistrationAttemptWithExistingEmail", null, new { Email = request.Email });
                return ServiceResult.Failure<AuthResponse>("A user with this email already exists", ErrorCodes.AUTH_EMAIL_EXISTS);
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
                Role = request.Role ?? Domain.Enums.UserRole.Customer, // Default Customer
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Token oluştur - Role claim'i ile
            var roleClaim = new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString());
            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email, new[] { roleClaim });
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

            // Background task - Welcome email gönder
            await _backgroundTaskService.EnqueueTaskAsync(new UserRegisteredTask
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }, TaskPriority.Normal, cancellationToken);

            // Log successful registration
            _loggingService.LogUserAction(user.Id.ToString(), "UserRegistered", new { Email = user.Email, Role = user.Role.ToString() });

            return ServiceResult.Success(new AuthResponse(
                accessToken,
                refreshTokenValue,
                DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                user.Role,
                user.Id,
                user.Email,
                $"{user.FirstName} {user.LastName}"
            ));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during user registration", ex, new { Email = request.Email });
            return ServiceResult.HandleException<AuthResponse>(ex, _logger, "UserRegistration");
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking( async () => await LoginInternalAsync(request, cancellationToken),
            "UserLogin", new { Email = request.Email },
            cancellationToken);
    }

    private async Task<Result<AuthResponse>> LoginInternalAsync(LoginRequest request,CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.ReadRepository<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);

            if (user == null)
            {
                _loggingService.LogSecurityEvent("LoginAttemptWithInvalidEmail", null, new { Email = request.Email });
                return ServiceResult.Failure<AuthResponse>("Invalid email or password", ErrorCodes.AUTH_INVALID_CREDENTIALS);
            }

            if (!user.IsActive)
            {
                _loggingService.LogSecurityEvent("LoginAttemptWithDeactivatedAccount", user.Id.ToString(), new { Email = request.Email });
                return ServiceResult.Failure<AuthResponse>("Your account has been deactivated", ErrorCodes.AUTH_ACCOUNT_DEACTIVATED);
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _loggingService.LogSecurityEvent("LoginAttemptWithInvalidPassword", user.Id.ToString(), new { Email = request.Email });
                return ServiceResult.Failure<AuthResponse>("Invalid email or password", ErrorCodes.AUTH_INVALID_CREDENTIALS);
            }

            // LastLogin güncelle
            user.LastLoginAt = DateTime.UtcNow;
            _unitOfWork.Repository<User>().Update(user);

            // Token oluştur - Role claim'i ile
            var roleClaim = new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString());
            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.Email, new[] { roleClaim });
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

            // Log successful login
            _loggingService.LogUserAction(user.Id.ToString(), "UserLoggedIn", new { Email = user.Email, Role = user.Role.ToString() });

            return ServiceResult.Success(new AuthResponse(
                accessToken,
                refreshTokenValue,
                DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                user.Role,
                user.Id,
                user.Email,
                $"{user.FirstName} {user.LastName}"
            ));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during user login", ex, new { Email = request.Email });
            return ServiceResult.HandleException<AuthResponse>(ex, _logger, "UserLogin");
        }
    }

    public async Task<Result<AuthResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await RefreshInternalAsync(request, cancellationToken),
            "TokenRefresh",
            new { RefreshToken = request.RefreshToken[..8] + "..." },
            cancellationToken);
    }

    private async Task<Result<AuthResponse>> RefreshInternalAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken = await _unitOfWork.ReadRepository<RefreshToken>()
                .FirstOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken && !rt.IsRevoked,
                    include: "User",
                    cancellationToken: cancellationToken);

            if (refreshToken == null)
            {
                _loggingService.LogSecurityEvent("InvalidRefreshTokenAttempt", null, new { Token = request.RefreshToken[..8] + "..." });
                return ServiceResult.Failure<AuthResponse>("Invalid refresh token", ErrorCodes.AUTH_INVALID_REFRESH_TOKEN);
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                _loggingService.LogSecurityEvent("ExpiredRefreshTokenAttempt", refreshToken.UserId.ToString(), new { Token = request.RefreshToken[..8] + "..." });
                return ServiceResult.Failure<AuthResponse>("Refresh token has expired", ErrorCodes.AUTH_REFRESH_TOKEN_EXPIRED);
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

            // Yeni token oluştur - Role claim'i ile
            var roleClaim = new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, refreshToken.User.Role.ToString());
            var accessToken = _jwtTokenService.CreateAccessToken(refreshToken.UserId, refreshToken.User.Email, new[] { roleClaim });
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

            // Log successful token refresh
            _loggingService.LogUserAction(refreshToken.UserId.ToString(), "TokenRefreshed", new { Email = refreshToken.User.Email });

            return ServiceResult.Success(new AuthResponse(
                accessToken,
                newRefreshTokenValue,
                DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                refreshToken.User.Role,
                refreshToken.UserId,
                refreshToken.User.Email,
                $"{refreshToken.User.FirstName} {refreshToken.User.LastName}"
            ));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during token refresh", ex, new { Token = request.RefreshToken[..8] + "..." });
            return ServiceResult.HandleException<AuthResponse>(ex, _logger, "TokenRefresh");
        }
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await LogoutInternalAsync(userId, cancellationToken),
            "UserLogout",
            new { UserId = userId },
            cancellationToken);
    }

    private async Task<Result> LogoutInternalAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
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

            // Log successful logout
            _loggingService.LogUserAction(userId.ToString(), "UserLoggedOut", new { UserId = userId });

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during user logout", ex, new { UserId = userId });
            return ServiceResult.HandleException(ex, _logger, "UserLogout");
        }
    }

    public async Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await ForgotPasswordInternalAsync(request, cancellationToken),
            "ForgotPassword",
            new { Email = request.Email },
            cancellationToken);
    }

    private async Task<Result> ForgotPasswordInternalAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken: cancellationToken);
            
            // Security: Don't reveal if user exists or not
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return ServiceResult.Success(); // Return success even if user doesn't exist
            }

            // Generate 6-digit reset code
            var resetCode = new Random().Next(100000, 999999).ToString();
            var resetToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Id}:{resetCode}:{DateTime.UtcNow:o}"));

            // Store reset token in cache (expires in 15 minutes)
            var cacheKey = $"password_reset:{user.Id}";
            await _cacheService.SetAsync(
                cacheKey, 
                resetCode, 
                TimeSpan.FromMinutes(15), 
                cancellationToken);

            // Send reset code via email
            var emailResult = await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Şifre Sıfırlama Kodu",
                $@"
                <html>
                <body>
                    <h2>Şifre Sıfırlama</h2>
                    <p>Merhaba {user.FirstName},</p>
                    <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>
                    <h1 style='color: #5D3EBC; letter-spacing: 5px;'>{resetCode}</h1>
                    <p>Bu kod 15 dakika içinde geçerliliğini yitirecektir.</p>
                    <p>Eğer bu işlemi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
                    <br>
                    <p>Getir Ekibi</p>
                </body>
                </html>",
                cancellationToken: cancellationToken);

            if (!emailResult.Success)
            {
                _logger.LogError("Failed to send password reset email to {Email}", user.Email);
                return ServiceResult.Failure("Şifre sıfırlama e-postası gönderilemedi");
            }

            _loggingService.LogBusinessEvent(
                "PasswordResetCodeSent",
                new { UserId = user.Id, Email = user.Email },
                LogLevel.Information);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during forgot password", ex, new { Email = request.Email });
            return ServiceResult.HandleException(ex, _logger, "ForgotPassword");
        }
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await ResetPasswordInternalAsync(request, cancellationToken),
            "ResetPassword",
            new { Token = request.Token },
            cancellationToken);
    }

    private async Task<Result> ResetPasswordInternalAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Decode token: format is "userId:code:timestamp"
            string decodedToken;
            try
            {
                var tokenBytes = Convert.FromBase64String(request.Token);
                decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return ServiceResult.Failure("Geçersiz sıfırlama kodu", ErrorCodes.VALIDATION_ERROR);
            }

            var parts = decodedToken.Split(':');
            if (parts.Length != 3 || !Guid.TryParse(parts[0], out var userId))
            {
                return ServiceResult.Failure("Geçersiz sıfırlama kodu formatı", ErrorCodes.VALIDATION_ERROR);
            }

            var providedCode = parts[1];

            // Get stored code from cache
            var cacheKey = $"password_reset:{userId}";
            var storedCode = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(storedCode))
            {
                return ServiceResult.Failure("Sıfırlama kodu geçersiz veya süresi dolmuş", ErrorCodes.VALIDATION_ERROR);
            }

            if (storedCode != providedCode)
            {
                return ServiceResult.Failure("Sıfırlama kodu hatalı", ErrorCodes.VALIDATION_ERROR);
            }

            // Get user
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ServiceResult.Failure("Kullanıcı bulunamadı", ErrorCodes.NOT_FOUND);
            }

            // Update password
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Remove reset code from cache
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);

            // Invalidate all refresh tokens for security
            var refreshTokenRepo = _unitOfWork.Repository<RefreshToken>();
            var refreshTokens = await refreshTokenRepo.ListAsync(
                filter: t => t.UserId == userId && !t.IsRevoked,
                cancellationToken: cancellationToken);
            
            foreach (var token in refreshTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.IsRevoked = true;
                refreshTokenRepo.Update(token);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent(
                "PasswordResetSuccess",
                new { UserId = userId, Email = user.Email },
                LogLevel.Information);

            // Send confirmation email
            await _emailService.SendHtmlEmailAsync(
                user.Email,
                "Şifreniz Değiştirildi",
                $@"
                <html>
                <body>
                    <h2>Şifre Değişikliği</h2>
                    <p>Merhaba {user.FirstName},</p>
                    <p>Şifreniz başarıyla değiştirildi.</p>
                    <p>Eğer bu işlemi siz yapmadıysanız, lütfen derhal bizimle iletişime geçin.</p>
                    <br>
                    <p>Getir Ekibi</p>
                </body>
                </html>",
                cancellationToken: cancellationToken);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error during password reset", ex, new { Token = request.Token });
            return ServiceResult.HandleException(ex, _logger, "ResetPassword");
        }
    }
}

// Background task data classes
public class UserRegisteredTask
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
