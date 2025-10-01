using System.Security.Claims;

namespace Getir.Application.Abstractions;

public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email, IEnumerable<Claim>? additionalClaims = null);
    string CreateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
