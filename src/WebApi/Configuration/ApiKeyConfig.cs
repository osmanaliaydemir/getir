using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Getir.WebApi.Configuration;

public static class ApiKeyConfig
{
    public static IServiceCollection AddApiKeyConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("ApiKey")
            .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options =>
            {
                options.ApiKey = configuration["ApiKey:Key"] ?? "default-api-key";
                options.HeaderName = "X-API-Key";
            });

        return services;
    }
}

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string HeaderName { get; set; } = "X-API-Key";
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (providedApiKey == Options.ApiKey)
        {
            var claims = new[]
            {
                new System.Security.Claims.Claim("ApiKey", "true"),
                new System.Security.Claims.Claim("ClientId", "external-api")
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
    }
}
