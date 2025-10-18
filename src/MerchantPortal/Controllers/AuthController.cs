using System.Security.Claims;
using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IApiClient _apiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IApiClient apiClient, ILogger<AuthController> logger)
    {
        _authService = authService;
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Giriş ekranını gösterir. Eğer kullanıcı giriş yaptıysa Dashboard'a yönlendirir.
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Kullanıcı giriş isteğini işler. Başarılıysa oturum açar ve yönlendirir.
    /// </summary>
    /// <param name="model">Giriş isteği için gerekli bilgiler</param>
    /// <param name="returnUrl">Başarılı girişten sonra yönlendirilecek adres (isteğe bağlı)</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz email veya şifre");
            return View(model);
        }

        // Validate result data before storing in session
        if (string.IsNullOrEmpty(result.Token) || result.User == null)
        {
            _logger.LogError("Login succeeded but token or user data is null");
            ModelState.AddModelError(string.Empty, "Giriş başarısız. Lütfen tekrar deneyin.");
            return View(model);
        }

        // Store token in session (null-safe)
        HttpContext.Session.SetString("JwtToken", result.Token ?? string.Empty);
        HttpContext.Session.SetString("UserId", result.User.Id.ToString());
        HttpContext.Session.SetString("UserName", result.User?.FullName ?? string.Empty);
        HttpContext.Session.SetString("UserEmail", result.User?.Email ?? string.Empty);

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
            new Claim(ClaimTypes.Name, result.User.FullName ?? string.Empty),
            new Claim(ClaimTypes.Email, result.User.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, result.User.Role ?? string.Empty),
            new Claim("JwtToken", result.Token ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = result.ExpiresAt
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Set token for API client
        if (!string.IsNullOrEmpty(result.Token))
        {
            _apiClient.SetAuthToken(result.Token);
        }

        // Store MerchantId from login response (if available)
        if (result.MerchantId.HasValue && result.MerchantId.Value != Guid.Empty)
        {
            HttpContext.Session.SetString("MerchantId", result.MerchantId.Value.ToString());
            _logger.LogInformation("MerchantId from login: {MerchantId}", result.MerchantId.Value);
        }
        else
        {
            // Fallback: Try to get from API
            await LoadMerchantIdToSession();
        }

        _logger.LogInformation("Login successful, redirecting to Dashboard for user: {Email}", model.Email);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            _logger.LogInformation("Redirecting to returnUrl: {ReturnUrl}", returnUrl);
            return Redirect(returnUrl);
        }

        _logger.LogInformation("Redirecting to Dashboard");
        return RedirectToAction("Index", "Dashboard");
    }

    /// <summary>
    /// Kullanıcı oturumunu kapatır.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        
        HttpContext.Session.Clear();
        
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Erişim iznin olmadığında gösterilecek sayfa.
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Load MerchantId to session from API
    /// </summary>
    private async Task LoadMerchantIdToSession()
    {
        try
        {
            // Get merchant info from API (user must be MerchantOwner)
            var apiResponse = await _apiClient.GetAsync<ApiResponse<MerchantResponse>>(
                "api/v1/merchant/my-merchant");

            if (apiResponse?.isSuccess == true && apiResponse.Data != null)
            {
                HttpContext.Session.SetString("MerchantId", apiResponse.Data.Id.ToString());
                _logger.LogInformation("MerchantId loaded to session: {MerchantId}", apiResponse.Data.Id);
            }
            else
            {
                _logger.LogWarning("Failed to load MerchantId: {Error}", apiResponse?.Error ?? "No merchant found");
                // Set a placeholder - will be handled by middleware/validation
                HttpContext.Session.SetString("MerchantId", Guid.Empty.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading MerchantId to session");
            HttpContext.Session.SetString("MerchantId", Guid.Empty.ToString());
        }
    }
}

