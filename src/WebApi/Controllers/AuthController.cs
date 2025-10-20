using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Kullanıcı kaydı, giriş ve token yönetimi için kimlik doğrulama controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Authentication")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Yeni kullanıcı kaydet
    /// </summary>
    /// <param name="request">Kayıt talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Token'larla kimlik doğrulama yanıtı</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _authService.RegisterAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// E-posta ve şifre ile giriş yap
    /// </summary>
    /// <param name="request">Giriş talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Token'larla kimlik doğrulama yanıtı</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request,CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();

        if (validationResult != null)
            return validationResult;

        var result = await _authService.LoginAsync(request, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Refresh token kullanarak erişim token'ını yenile
    /// </summary>
    /// <param name="request">Token yenileme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Yeni token'larla kimlik doğrulama yanıtı</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _authService.RefreshAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mevcut kullanıcıyı çıkış yap
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _authService.LogoutAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Şifre sıfırlama talebi (e-posta ile 6 haneli kod gönderir)
    /// </summary>
    /// <param name="request">Şifremi unuttum talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _authService.ForgotPasswordAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// E-postadaki kod ile şifreyi sıfırla
    /// </summary>
    /// <param name="request">Şifre sıfırlama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _authService.ResetPasswordAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kimlik doğrulanmış kullanıcı için şifre değiştir
    /// </summary>
    /// <param name="request">Şifre değiştirme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _authService.ChangePasswordAsync(userId, request, ct);
        return ToActionResult(result);
    }
}
