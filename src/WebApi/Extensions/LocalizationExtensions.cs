using Getir.Application.Services.Internationalization;
using Getir.WebApi.Middleware;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Getir.WebApi.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        // Add localization services
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Add supported cultures
        var supportedCultures = new[]
        {
            new CultureInfo("tr-TR"), // Turkish
            new CultureInfo("en-US"), // English
            new CultureInfo("ar-SA")  // Arabic
        };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("tr-TR");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            // Add request culture providers
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };
        });

        return services;
    }

    public static IApplicationBuilder UseLocalizationMiddleware(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();
        app.UseMiddleware<LocalizationMiddleware>();
        return app;
    }
}
