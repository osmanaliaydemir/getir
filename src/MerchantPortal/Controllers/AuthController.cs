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

    public AuthController(
        IAuthService authService,
        IApiClient apiClient,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _apiClient = apiClient;
        _logger = logger;
    }

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

        // Store token in session
        HttpContext.Session.SetString("JwtToken", result.Token);
        HttpContext.Session.SetString("MerchantId", result.User.Id.ToString());
        HttpContext.Session.SetString("UserName", result.User.FullName);
        HttpContext.Session.SetString("UserEmail", result.User.Email);

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
            new Claim(ClaimTypes.Name, result.User.FullName),
            new Claim(ClaimTypes.Email, result.User.Email),
            new Claim(ClaimTypes.Role, result.User.Role),
            new Claim("JwtToken", result.Token)
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
        _apiClient.SetAuthToken(result.Token);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

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

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

