using Microsoft.AspNetCore.Components.Web;

namespace WebApp.Services;

public interface ISeoService
{
    Dictionary<string, string> GetDefaultMetaTags();
    Dictionary<string, string> GetOpenGraphTags(string? title = null, string? description = null, string? image = null, string? url = null);
    Dictionary<string, string> GetProductMetaTags(string productName, string productDescription, decimal price, string? imageUrl = null);
    Dictionary<string, string> GetMerchantMetaTags(string merchantName, string merchantDescription, string? imageUrl = null);
    Dictionary<string, string> GetPageMetaTags(string pageTitle, string pageDescription, string? imageUrl = null);
    string GenerateStructuredData(string type, Dictionary<string, object> data);
    string GenerateProductStructuredData(string productName, string description, decimal price, string? imageUrl = null, string? brand = null);
    string GenerateOrganizationStructuredData();
    string GenerateBreadcrumbStructuredData(List<(string Name, string Url)> breadcrumbs);
    string GenerateLocalBusinessStructuredData(string businessName, string address, string phone, string? openingHours = null);
}

public class SeoService : ISeoService
{
    private readonly ILogger<SeoService> _logger;

    public SeoService(ILogger<SeoService> logger)
    {
        _logger = logger;
    }

    public Dictionary<string, string> GetDefaultMetaTags()
    {
        return new Dictionary<string, string>
        {
            ["title"] = "Getir - Hızlı Teslimat | Market, Eczane, Restoran",
            ["description"] = "Getir ile market, eczane ve restoran ürünlerini dakikalar içinde kapınıza getiriyoruz. Hızlı, güvenli ve kolay alışveriş deneyimi.",
            ["keywords"] = "getir, hızlı teslimat, market, eczane, restoran, online alışveriş, kapıda ödeme, güvenli ödeme",
            ["author"] = "Getir",
            ["robots"] = "index, follow",
            ["viewport"] = "width=device-width, initial-scale=1.0",
            ["theme-color"] = "#5d3ebc",
            ["msapplication-TileColor"] = "#5d3ebc",
            ["msapplication-config"] = "/browserconfig.xml"
        };
    }

    public Dictionary<string, string> GetOpenGraphTags(string? title = null, string? description = null, string? image = null, string? url = null)
    {
        var defaultTitle = "Getir - Hızlı Teslimat";
        var defaultDescription = "Market, eczane ve restoran ürünlerini dakikalar içinde kapınıza getiriyoruz.";
        var defaultImage = "https://ajilgo.runasp.net/images/getir-og-image.jpg";
        var defaultUrl = "https://ajilgo.runasp.net";

        return new Dictionary<string, string>
        {
            ["og:title"] = title ?? defaultTitle,
            ["og:description"] = description ?? defaultDescription,
            ["og:image"] = image ?? defaultImage,
            ["og:url"] = url ?? defaultUrl,
            ["og:type"] = "website",
            ["og:site_name"] = "Getir",
            ["og:locale"] = "tr_TR",
            ["og:locale:alternate"] = "en_US",
            ["fb:app_id"] = "your-facebook-app-id",
            ["twitter:card"] = "summary_large_image",
            ["twitter:title"] = title ?? defaultTitle,
            ["twitter:description"] = description ?? defaultDescription,
            ["twitter:image"] = image ?? defaultImage,
            ["twitter:site"] = "@getir",
            ["twitter:creator"] = "@getir"
        };
    }

    public Dictionary<string, string> GetProductMetaTags(string productName, string productDescription, decimal price, string? imageUrl = null)
    {
        var title = $"{productName} - Getir";
        var description = $"{productDescription} - ₺{price:F2} - Hızlı teslimat ile Getir'den sipariş verin.";
        var image = imageUrl ?? "https://ajilgo.runasp.net/images/product-default.jpg";

        var metaTags = GetDefaultMetaTags();
        metaTags["title"] = title;
        metaTags["description"] = description;

        var ogTags = GetOpenGraphTags(title, description, image);
        
        foreach (var ogTag in ogTags)
        {
            metaTags[ogTag.Key] = ogTag.Value;
        }

        // Product specific tags
        metaTags["product:price:amount"] = price.ToString("F2");
        metaTags["product:price:currency"] = "TRY";
        metaTags["product:availability"] = "in stock";
        metaTags["product:condition"] = "new";

        return metaTags;
    }

    public Dictionary<string, string> GetMerchantMetaTags(string merchantName, string merchantDescription, string? imageUrl = null)
    {
        var title = $"{merchantName} - Getir Mağazalar";
        var description = $"{merchantDescription} - {merchantName} mağazasından hızlı teslimat ile sipariş verin.";
        var image = imageUrl ?? "https://ajilgo.runasp.net/images/merchant-default.jpg";

        var metaTags = GetDefaultMetaTags();
        metaTags["title"] = title;
        metaTags["description"] = description;

        var ogTags = GetOpenGraphTags(title, description, image);
        
        foreach (var ogTag in ogTags)
        {
            metaTags[ogTag.Key] = ogTag.Value;
        }

        return metaTags;
    }

    public Dictionary<string, string> GetPageMetaTags(string pageTitle, string pageDescription, string? imageUrl = null)
    {
        var title = $"{pageTitle} - Getir";
        var description = pageDescription;
        var image = imageUrl ?? "https://ajilgo.runasp.net/images/getir-og-image.jpg";

        var metaTags = GetDefaultMetaTags();
        metaTags["title"] = title;
        metaTags["description"] = description;

        var ogTags = GetOpenGraphTags(title, description, image);
        
        foreach (var ogTag in ogTags)
        {
            metaTags[ogTag.Key] = ogTag.Value;
        }

        return metaTags;
    }

    public string GenerateStructuredData(string type, Dictionary<string, object> data)
    {
        try
        {
            var structuredData = new
            {
                context = "https://schema.org",
                type = type,
                data
            };

            return System.Text.Json.JsonSerializer.Serialize(structuredData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating structured data for type: {Type}", type);
            return string.Empty;
        }
    }

    public string GenerateProductStructuredData(string productName, string description, decimal price, string? imageUrl = null, string? brand = null)
    {
        var data = new Dictionary<string, object>
        {
            ["name"] = productName,
            ["description"] = description,
            ["offers"] = new
            {
                price = price.ToString("F2"),
                priceCurrency = "TRY",
                availability = "https://schema.org/InStock"
            }
        };

        if (!string.IsNullOrEmpty(imageUrl))
        {
            data["image"] = imageUrl;
        }

        if (!string.IsNullOrEmpty(brand))
        {
            data["brand"] = new { name = brand };
        }

        return GenerateStructuredData("Product", data);
    }

    public string GenerateOrganizationStructuredData()
    {
        var data = new Dictionary<string, object>
        {
            ["name"] = "Getir",
            ["url"] = "https://ajilgo.runasp.net",
            ["logo"] = "https://ajilgo.runasp.net/images/getir-logo.png",
            ["description"] = "Hızlı teslimat ile market, eczane ve restoran ürünlerini kapınıza getiriyoruz.",
            ["contactPoint"] = new
            {
                telephone = "+90-212-123-45-67",
                contactType = "customer service",
                availableLanguage = new[] { "Turkish", "English" }
            },
            ["sameAs"] = new[]
            {
                "https://www.facebook.com/getir",
                "https://www.twitter.com/getir",
                "https://www.instagram.com/getir"
            }
        };

        return GenerateStructuredData("Organization", data);
    }

    public string GenerateBreadcrumbStructuredData(List<(string Name, string Url)> breadcrumbs)
    {
        var items = breadcrumbs.Select((breadcrumb, index) => new
        {
            position = index + 1,
            name = breadcrumb.Name,
            item = breadcrumb.Url
        }).ToArray();

        var data = new Dictionary<string, object>
        {
            ["itemListElement"] = items
        };

        return GenerateStructuredData("BreadcrumbList", data);
    }

    public string GenerateLocalBusinessStructuredData(string businessName, string address, string phone, string? openingHours = null)
    {
        var data = new Dictionary<string, object>
        {
            ["name"] = businessName,
            ["address"] = new
            {
                streetAddress = address,
                addressCountry = "TR"
            },
            ["telephone"] = phone
        };

        if (!string.IsNullOrEmpty(openingHours))
        {
            data["openingHours"] = openingHours;
        }

        return GenerateStructuredData("LocalBusiness", data);
    }
}
