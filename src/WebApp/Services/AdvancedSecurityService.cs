using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApp.Services;

public interface IAdvancedSecurityService
{
    string SanitizeInput(string input);
    string EncodeHtml(string input);
    string EncodeUrl(string input);
    string EncodeJavaScript(string input);
    bool IsValidEmail(string email);
    bool IsValidPhoneNumber(string phoneNumber);
    bool IsValidUrl(string url);
    string GenerateSecureToken(int length = 32);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string EncryptSensitiveData(string data);
    string DecryptSensitiveData(string encryptedData);
    bool IsSqlInjectionSafe(string input);
    bool IsXssSafe(string input);
    string GenerateCsrfToken();
    bool ValidateCsrfToken(string token);
    Dictionary<string, string> GetSecurityHeaders();
    bool IsRateLimitExceeded(string identifier, int maxRequests, TimeSpan window);
    void LogSecurityEvent(string eventType, string details, string? userId = null);
}

public class AdvancedSecurityService : IAdvancedSecurityService
{
    private readonly IAntiforgery _antiforgery;
    private readonly IDataProtector _dataProtector;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AdvancedSecurityService> _logger;
    private readonly Dictionary<string, List<DateTime>> _rateLimitStore;

    // SQL Injection patterns
    private static readonly string[] SqlInjectionPatterns = {
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|UNION|SCRIPT)\b)",
        @"(\b(OR|AND)\s+\d+\s*=\s*\d+)",
        @"(\b(OR|AND)\s+'.*'\s*=\s*'.*')",
        @"(;|\-\-|\/\*|\*\/)",
        @"(\b(UNION|SELECT)\b.*\b(FROM|WHERE)\b)",
        @"(\b(INSERT|UPDATE|DELETE)\b.*\b(INTO|SET|FROM)\b)",
        @"(\b(DROP|CREATE|ALTER)\b.*\b(TABLE|DATABASE|INDEX)\b)"
    };

    // XSS patterns
    private static readonly string[] XssPatterns = {
        @"<script[^>]*>.*?</script>",
        @"<iframe[^>]*>.*?</iframe>",
        @"<object[^>]*>.*?</object>",
        @"<embed[^>]*>.*?</embed>",
        @"<link[^>]*>.*?</link>",
        @"<meta[^>]*>.*?</meta>",
        @"<style[^>]*>.*?</style>",
        @"javascript:",
        @"vbscript:",
        @"onload\s*=",
        @"onerror\s*=",
        @"onclick\s*=",
        @"onmouseover\s*=",
        @"onfocus\s*=",
        @"onblur\s*=",
        @"onchange\s*=",
        @"onsubmit\s*=",
        @"onreset\s*=",
        @"onselect\s*=",
        @"onkeydown\s*=",
        @"onkeyup\s*=",
        @"onkeypress\s*="
    };

    public AdvancedSecurityService(
        IAntiforgery antiforgery,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AdvancedSecurityService> logger)
    {
        _antiforgery = antiforgery;
        _dataProtector = dataProtectionProvider.CreateProtector("AdvancedSecurity");
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _rateLimitStore = new Dictionary<string, List<DateTime>>();
    }

    public string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove null characters
        input = input.Replace("\0", string.Empty);

        // Trim whitespace
        input = input.Trim();

        // Remove control characters except newlines and tabs
        input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);

        return input;
    }

    public string EncodeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("/", "&#x2F;");
    }

    public string EncodeUrl(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Uri.EscapeDataString(input);
    }

    public string EncodeJavaScript(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f");
    }

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        try
        {
            var regex = new Regex(@"^\+?[1-9]\d{1,14}$");
            return regex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", ""));
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        try
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateSecureToken(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            var testHash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != testHash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string EncryptSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return string.Empty;

        try
        {
            return _dataProtector.Protect(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting sensitive data");
            return string.Empty;
        }
    }

    public string DecryptSensitiveData(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData))
            return string.Empty;

        try
        {
            return _dataProtector.Unprotect(encryptedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting sensitive data");
            return string.Empty;
        }
    }

    public bool IsSqlInjectionSafe(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        var upperInput = input.ToUpperInvariant();
        
        foreach (var pattern in SqlInjectionPatterns)
        {
            if (Regex.IsMatch(upperInput, pattern, RegexOptions.IgnoreCase))
            {
                LogSecurityEvent("SQL_INJECTION_ATTEMPT", $"Potential SQL injection detected: {input}");
                return false;
            }
        }

        return true;
    }

    public bool IsXssSafe(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        foreach (var pattern in XssPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
            {
                LogSecurityEvent("XSS_ATTEMPT", $"Potential XSS attack detected: {input}");
                return false;
            }
        }

        return true;
    }

    public string GenerateCsrfToken()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var tokens = _antiforgery.GetAndStoreTokens(context);
                return tokens.RequestToken ?? string.Empty;
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CSRF token");
            return string.Empty;
        }
    }

    public bool ValidateCsrfToken(string token)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                return _antiforgery.IsRequestValidAsync(context).Result;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating CSRF token");
            return false;
        }
    }

    public Dictionary<string, string> GetSecurityHeaders()
    {
        return new Dictionary<string, string>
        {
            ["X-Content-Type-Options"] = "nosniff",
            ["X-Frame-Options"] = "DENY",
            ["X-XSS-Protection"] = "1; mode=block",
            ["Referrer-Policy"] = "strict-origin-when-cross-origin",
            ["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()",
            ["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains",
            ["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https:; frame-ancestors 'none';"
        };
    }

    public bool IsRateLimitExceeded(string identifier, int maxRequests, TimeSpan window)
    {
        var now = DateTime.UtcNow;
        var windowStart = now - window;

        lock (_rateLimitStore)
        {
            if (!_rateLimitStore.ContainsKey(identifier))
            {
                _rateLimitStore[identifier] = new List<DateTime>();
            }

            var requests = _rateLimitStore[identifier];
            
            // Remove old requests outside the window
            requests.RemoveAll(r => r < windowStart);

            if (requests.Count >= maxRequests)
            {
                LogSecurityEvent("RATE_LIMIT_EXCEEDED", $"Rate limit exceeded for identifier: {identifier}");
                return true;
            }

            requests.Add(now);
            return false;
        }
    }

    public void LogSecurityEvent(string eventType, string details, string? userId = null)
    {
        var logMessage = $"Security Event - Type: {eventType}, Details: {details}";
        
        if (!string.IsNullOrEmpty(userId))
        {
            logMessage += $", UserId: {userId}";
        }

        _logger.LogWarning(logMessage);

        // Here you could also send to external security monitoring systems
        // e.g., SIEM, Security Center, etc.
    }
}
