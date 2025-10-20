using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Auth;

/// <summary>
/// Kimlik doğrulama servisi interface'i: kayıt, giriş, token yönetimi, şifre işlemleri ve profil yönetimi.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur.
    /// </summary>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kullanıcı girişi yapar.
    /// </summary>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Token yeniler.
    /// </summary>
    Task<Result<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kullanıcı çıkışı yapar.
    /// </summary>
    Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Şifre sıfırlama sürecini başlatır.
    /// </summary>
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Şifreyi sıfırlar.
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Şifre değiştirir.
    /// </summary>
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kullanıcı profilini getirir.
    /// </summary>
    Task<Result<UserProfileResponse>> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kullanıcı profilini günceller.
    /// </summary>
    Task<Result<UserProfileResponse>> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
}
