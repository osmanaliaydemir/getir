using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task LogoutAsync();
}

