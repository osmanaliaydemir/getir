using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Xml.Linq;

namespace WebApp.Services;

public interface IAdvancedSeoService
{
    Task<string> GenerateDynamicSitemapAsync();
    Task<string> GenerateRobotsTxtAsync();
    Task<string> GenerateSchemaMarkupAsync(string pageType, Dictionary<string, object> data);
    Task<string> GenerateBreadcrumbSchemaAsync(List<BreadcrumbItem> breadcrumbs);
    Task<string> GenerateProductSchemaAsync(ProductSchemaData product);
    Task<string> GenerateOrganizationSchemaAsync();
    Task<string> GenerateLocalBusinessSchemaAsync(LocalBusinessData business);
    Task<string> GenerateEventSchemaAsync(EventSchemaData eventData);
    Task<string> GenerateArticleSchemaAsync(ArticleSchemaData article);
    Task<Dictionary<string, string>> GenerateMetaTagsAsync(string pageType, Dictionary<string, object> data);
    Task<Dictionary<string, string>> GenerateOpenGraphTagsAsync(string pageType, Dictionary<string, object> data);
    Task<Dictionary<string, string>> GenerateTwitterCardTagsAsync(string pageType, Dictionary<string, object> data);
    Task<string> GenerateCanonicalUrlAsync(string path);
    Task<List<string>> GenerateAlternateLanguageUrlsAsync(string path);
    Task<Dictionary<string, object>> GetSeoAnalyticsAsync();
}

public class AdvancedSeoService : IAdvancedSeoService
{
    private readonly ILogger<AdvancedSeoService> _logger;
    private readonly IConfiguration _configuration;

    public AdvancedSeoService(ILogger<AdvancedSeoService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> GenerateDynamicSitemapAsync()
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("urlset",
                    new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                    new XAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"),

                    // Static pages
                    CreateSitemapUrl(baseUrl, "/", DateTime.UtcNow, "daily", 1.0),
                    CreateSitemapUrl(baseUrl, "/merchants", DateTime.UtcNow, "daily", 0.9),
                    CreateSitemapUrl(baseUrl, "/cart", DateTime.UtcNow, "weekly", 0.7),
                    CreateSitemapUrl(baseUrl, "/orders", DateTime.UtcNow, "weekly", 0.8),
                    CreateSitemapUrl(baseUrl, "/account", DateTime.UtcNow, "monthly", 0.6),
                    CreateSitemapUrl(baseUrl, "/help", DateTime.UtcNow, "monthly", 0.5),
                    CreateSitemapUrl(baseUrl, "/contact", DateTime.UtcNow, "monthly", 0.5),

                    // Category pages
                    CreateSitemapUrl(baseUrl, "/merchants?category=market", DateTime.UtcNow, "daily", 0.8),
                    CreateSitemapUrl(baseUrl, "/merchants?category=pharmacy", DateTime.UtcNow, "daily", 0.8),
                    CreateSitemapUrl(baseUrl, "/merchants?category=restaurant", DateTime.UtcNow, "daily", 0.8),

                    // Dynamic content (this would typically come from database)
                    await GenerateDynamicUrlsAsync(baseUrl)
                )
            );

            return sitemap.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dynamic sitemap");
            return string.Empty;
        }
    }

    public async Task<string> GenerateRobotsTxtAsync()
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var robotsTxt = $@"User-agent: *
Allow: /

# Sitemap
Sitemap: {baseUrl}/sitemap/sitemap.xml

# Disallow admin and private areas
Disallow: /admin/
Disallow: /api/
Disallow: /health/
Disallow: /_blazor/
Disallow: /_framework/
Disallow: /sw.js
Disallow: /manifest.json

# Allow important pages
Allow: /merchants
Allow: /products
Allow: /cart
Allow: /orders
Allow: /account
Allow: /help
Allow: /contact

# Crawl delay
Crawl-delay: 1";

            return robotsTxt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating robots.txt");
            return string.Empty;
        }
    }

    public async Task<string> GenerateSchemaMarkupAsync(string pageType, Dictionary<string, object> data)
    {
        try
        {
            return pageType.ToLower() switch
            {
                "product" => await GenerateProductSchemaAsync(new ProductSchemaData
                {
                    Name = data.GetValueOrDefault("name", "").ToString() ?? "",
                    Description = data.GetValueOrDefault("description", "").ToString() ?? "",
                    Price = Convert.ToDecimal(data.GetValueOrDefault("price", 0)),
                    Currency = data.GetValueOrDefault("currency", "TRY").ToString() ?? "TRY",
                    ImageUrl = data.GetValueOrDefault("imageUrl", "").ToString() ?? "",
                    Brand = data.GetValueOrDefault("brand", "").ToString() ?? "",
                    Availability = data.GetValueOrDefault("availability", "InStock").ToString() ?? "InStock"
                }),
                "organization" => await GenerateOrganizationSchemaAsync(),
                "localbusiness" => await GenerateLocalBusinessSchemaAsync(new LocalBusinessData
                {
                    Name = data.GetValueOrDefault("name", "").ToString() ?? "",
                    Address = data.GetValueOrDefault("address", "").ToString() ?? "",
                    Phone = data.GetValueOrDefault("phone", "").ToString() ?? "",
                    OpeningHours = data.GetValueOrDefault("openingHours", "").ToString() ?? ""
                }),
                "breadcrumb" => await GenerateBreadcrumbSchemaAsync(data.GetValueOrDefault("breadcrumbs", new List<BreadcrumbItem>()) as List<BreadcrumbItem> ?? new List<BreadcrumbItem>()),
                _ => await GenerateOrganizationSchemaAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schema markup for page type: {PageType}", pageType);
            return string.Empty;
        }
    }

    public async Task<string> GenerateBreadcrumbSchemaAsync(List<BreadcrumbItem> breadcrumbs)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var items = breadcrumbs.Select((item, index) => new
            {
                position = index + 1,
                name = item.Name,
                item = $"{baseUrl}{item.Url}"
            }).ToArray();

            var schema = new
            {
                context = "https://schema.org",
                type = "BreadcrumbList",
                itemListElement = items
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating breadcrumb schema");
            return string.Empty;
        }
    }

    public async Task<string> GenerateProductSchemaAsync(ProductSchemaData product)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var schema = new
            {
                context = "https://schema.org",
                type = "Product",
                name = product.Name,
                description = product.Description,
                image = product.ImageUrl,
                brand = new { name = product.Brand },
                offers = new
                {
                    price = product.Price.ToString("F2"),
                    priceCurrency = product.Currency,
                    availability = $"https://schema.org/{product.Availability}",
                    seller = new
                    {
                        type = "Organization",
                        name = "Getir"
                    }
                }
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product schema");
            return string.Empty;
        }
    }

    public async Task<string> GenerateOrganizationSchemaAsync()
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var schema = new
            {
                context = "https://schema.org",
                type = "Organization",
                name = "Getir",
                url = baseUrl,
                logo = $"{baseUrl}/images/getir-logo.png",
                description = "Hızlı teslimat ile market, eczane ve restoran ürünlerini kapınıza getiriyoruz.",
                contactPoint = new
                {
                    telephone = "+90-212-123-45-67",
                    contactType = "customer service",
                    availableLanguage = new[] { "Turkish", "English", "Arabic" }
                },
                sameAs = new[]
                {
                    "https://www.facebook.com/getir",
                    "https://www.twitter.com/getir",
                    "https://www.instagram.com/getir"
                }
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating organization schema");
            return string.Empty;
        }
    }

    public async Task<string> GenerateLocalBusinessSchemaAsync(LocalBusinessData business)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var schema = new
            {
                context = "https://schema.org",
                type = "LocalBusiness",
                name = business.Name,
                address = new
                {
                    streetAddress = business.Address,
                    addressCountry = "TR"
                },
                telephone = business.Phone,
                openingHours = business.OpeningHours
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating local business schema");
            return string.Empty;
        }
    }

    public async Task<string> GenerateEventSchemaAsync(EventSchemaData eventData)
    {
        try
        {
            var schema = new
            {
                context = "https://schema.org",
                type = "Event",
                name = eventData.Name,
                description = eventData.Description,
                startDate = eventData.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                endDate = eventData.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                location = new
                {
                    type = "Place",
                    name = eventData.LocationName,
                    address = eventData.LocationAddress
                },
                organizer = new
                {
                    type = "Organization",
                    name = "Getir"
                }
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating event schema");
            return string.Empty;
        }
    }

    public async Task<string> GenerateArticleSchemaAsync(ArticleSchemaData article)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var schema = new
            {
                context = "https://schema.org",
                type = "Article",
                headline = article.Headline,
                description = article.Description,
                image = article.ImageUrl,
                author = new
                {
                    type = "Organization",
                    name = "Getir"
                },
                publisher = new
                {
                    type = "Organization",
                    name = "Getir",
                    logo = new
                    {
                        type = "ImageObject",
                        url = $"{baseUrl}/images/getir-logo.png"
                    }
                },
                datePublished = article.DatePublished.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                dateModified = article.DateModified.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating article schema");
            return string.Empty;
        }
    }

    public async Task<Dictionary<string, string>> GenerateMetaTagsAsync(string pageType, Dictionary<string, object> data)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var metaTags = new Dictionary<string, string>();

            switch (pageType.ToLower())
            {
                case "product":
                    metaTags["title"] = $"{data.GetValueOrDefault("name", "")} - Getir";
                    metaTags["description"] = data.GetValueOrDefault("description", "").ToString() ?? "";
                    metaTags["keywords"] = $"getir, {data.GetValueOrDefault("name", "")}, {data.GetValueOrDefault("category", "")}, hızlı teslimat";
                    break;
                case "merchant":
                    metaTags["title"] = $"{data.GetValueOrDefault("name", "")} - Getir Mağazalar";
                    metaTags["description"] = $"{data.GetValueOrDefault("description", "")} - {data.GetValueOrDefault("name", "")} mağazasından hızlı teslimat ile sipariş verin.";
                    metaTags["keywords"] = $"getir, {data.GetValueOrDefault("name", "")}, mağaza, {data.GetValueOrDefault("category", "")}, hızlı teslimat";
                    break;
                default:
                    metaTags["title"] = "Getir - Hızlı Teslimat";
                    metaTags["description"] = "Market, eczane ve restoran ürünlerini dakikalar içinde kapınıza getiriyoruz.";
                    metaTags["keywords"] = "getir, hızlı teslimat, market, eczane, restoran, online alışveriş";
                    break;
            }

            metaTags["author"] = "Getir";
            metaTags["robots"] = "index, follow";
            metaTags["canonical"] = $"{baseUrl}{data.GetValueOrDefault("path", "")}";

            return metaTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating meta tags for page type: {PageType}", pageType);
            return new Dictionary<string, string>();
        }
    }

    public async Task<Dictionary<string, string>> GenerateOpenGraphTagsAsync(string pageType, Dictionary<string, object> data)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var ogTags = new Dictionary<string, string>
            {
                ["og:type"] = pageType.ToLower() switch
                {
                    "product" => "product",
                    "merchant" => "business.business",
                    _ => "website"
                },
                ["og:site_name"] = "Getir",
                ["og:locale"] = "tr_TR",
                ["og:url"] = $"{baseUrl}{data.GetValueOrDefault("path", "")}"
            };

            switch (pageType.ToLower())
            {
                case "product":
                    ogTags["og:title"] = $"{data.GetValueOrDefault("name", "")} - Getir";
                    ogTags["og:description"] = data.GetValueOrDefault("description", "").ToString() ?? "";
                    ogTags["og:image"] = data.GetValueOrDefault("imageUrl", $"{baseUrl}/images/product-default.jpg").ToString() ?? "";
                    break;
                case "merchant":
                    ogTags["og:title"] = $"{data.GetValueOrDefault("name", "")} - Getir Mağazalar";
                    ogTags["og:description"] = $"{data.GetValueOrDefault("description", "")} - {data.GetValueOrDefault("name", "")} mağazasından hızlı teslimat ile sipariş verin.";
                    ogTags["og:image"] = data.GetValueOrDefault("imageUrl", $"{baseUrl}/images/merchant-default.jpg").ToString() ?? "";
                    break;
                default:
                    ogTags["og:title"] = "Getir - Hızlı Teslimat";
                    ogTags["og:description"] = "Market, eczane ve restoran ürünlerini dakikalar içinde kapınıza getiriyoruz.";
                    ogTags["og:image"] = $"{baseUrl}/images/getir-og-image.jpg";
                    break;
            }

            return ogTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Open Graph tags for page type: {PageType}", pageType);
            return new Dictionary<string, string>();
        }
    }

    public async Task<Dictionary<string, string>> GenerateTwitterCardTagsAsync(string pageType, Dictionary<string, object> data)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var twitterTags = new Dictionary<string, string>
            {
                ["twitter:card"] = "summary_large_image",
                ["twitter:site"] = "@getir",
                ["twitter:creator"] = "@getir"
            };

            switch (pageType.ToLower())
            {
                case "product":
                    twitterTags["twitter:title"] = $"{data.GetValueOrDefault("name", "")} - Getir";
                    twitterTags["twitter:description"] = data.GetValueOrDefault("description", "").ToString() ?? "";
                    twitterTags["twitter:image"] = data.GetValueOrDefault("imageUrl", $"{baseUrl}/images/product-default.jpg").ToString() ?? "";
                    break;
                case "merchant":
                    twitterTags["twitter:title"] = $"{data.GetValueOrDefault("name", "")} - Getir Mağazalar";
                    twitterTags["twitter:description"] = $"{data.GetValueOrDefault("description", "")} - {data.GetValueOrDefault("name", "")} mağazasından hızlı teslimat ile sipariş verin.";
                    twitterTags["twitter:image"] = data.GetValueOrDefault("imageUrl", $"{baseUrl}/images/merchant-default.jpg").ToString() ?? "";
                    break;
                default:
                    twitterTags["twitter:title"] = "Getir - Hızlı Teslimat";
                    twitterTags["twitter:description"] = "Market, eczane ve restoran ürünlerini dakikalar içinde kapınıza getiriyoruz.";
                    twitterTags["twitter:image"] = $"{baseUrl}/images/getir-og-image.jpg";
                    break;
            }

            return twitterTags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Twitter Card tags for page type: {PageType}", pageType);
            return new Dictionary<string, string>();
        }
    }

    public async Task<string> GenerateCanonicalUrlAsync(string path)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            return $"{baseUrl}{path}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating canonical URL for path: {Path}", path);
            return string.Empty;
        }
    }

    public async Task<List<string>> GenerateAlternateLanguageUrlsAsync(string path)
    {
        try
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ajilgo.runasp.net";
            var languages = new[] { "tr", "en", "ar" };
            
            return languages.Select(lang => $"{baseUrl}/{lang}{path}").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating alternate language URLs for path: {Path}", path);
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, object>> GetSeoAnalyticsAsync()
    {
        try
        {
            // This would typically come from analytics data
            var analytics = new Dictionary<string, object>
            {
                ["total_pages"] = 100,
                ["indexed_pages"] = 95,
                ["mobile_friendly"] = true,
                ["page_speed_score"] = 85,
                ["seo_score"] = 92,
                ["last_updated"] = DateTime.UtcNow
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SEO analytics");
            return new Dictionary<string, object>();
        }
    }

    private XElement CreateSitemapUrl(string baseUrl, string path, DateTime lastModified, string changeFrequency, double priority)
    {
        return new XElement("url",
            new XElement("loc", $"{baseUrl}{path}"),
            new XElement("lastmod", lastModified.ToString("yyyy-MM-ddTHH:mm:ssZ")),
            new XElement("changefreq", changeFrequency),
            new XElement("priority", priority.ToString("F1"))
        );
    }

    private async Task<XElement[]> GenerateDynamicUrlsAsync(string baseUrl)
    {
        // This would typically query a database for dynamic content
        var dynamicUrls = new List<XElement>();
        
        // Example: Generate URLs for merchants
        for (int i = 1; i <= 10; i++)
        {
            dynamicUrls.Add(CreateSitemapUrl(baseUrl, $"/merchants/{i}", DateTime.UtcNow.AddDays(-1), "daily", 0.8));
        }

        // Example: Generate URLs for products
        for (int i = 1; i <= 50; i++)
        {
            dynamicUrls.Add(CreateSitemapUrl(baseUrl, $"/products/{i}", DateTime.UtcNow.AddDays(-2), "weekly", 0.7));
        }

        return dynamicUrls.ToArray();
    }
}

// Data models
public class BreadcrumbItem
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class ProductSchemaData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public string ImageUrl { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Availability { get; set; } = "InStock";
}

public class LocalBusinessData
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string OpeningHours { get; set; } = string.Empty;
}

public class EventSchemaData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
}

public class ArticleSchemaData
{
    public string Headline { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime DatePublished { get; set; }
    public DateTime DateModified { get; set; }
}
