using System.Security.Claims;

namespace Getir.Application.Abstractions;

/// <summary>
/// JWT token servisi interface
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Access token oluştur
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="email">Email</param>
    /// <param name="additionalClaims">Ekstra claimler</param>
    /// <returns>Access token</returns>
    string CreateAccessToken(Guid userId, string email, IEnumerable<Claim>? additionalClaims = null);
    /// <summary>
    /// Refresh token oluştur
    /// </summary>
    /// <returns>Refresh token</returns>
    string CreateRefreshToken();
    /// <summary>
    /// Token doğrula
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Doğrulama sonucu</returns>
    ClaimsPrincipal? ValidateToken(string token);
}
