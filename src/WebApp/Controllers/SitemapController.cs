using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("sitemap")]
public class SitemapController : Controller
{
    private readonly ILogger<SitemapController> _logger;

    public SitemapController(ILogger<SitemapController> logger)
    {
        _logger = logger;
    }

    [HttpGet("sitemap.xml")]
    public IActionResult Sitemap()
    {
        try
        {
            var baseUrl = "https://ajilgo.runasp.net";
            var sitemap = GenerateSitemap(baseUrl);
            
            return Content(sitemap, "application/xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sitemap");
            return StatusCode(500);
        }
    }

    [HttpGet("robots.txt")]
    public IActionResult Robots()
    {
        try
        {
            var robots = @"User-agent: *
Allow: /

# Sitemap
Sitemap: https://ajilgo.runasp.net/sitemap/sitemap.xml

# Disallow admin and private areas
Disallow: /admin/
Disallow: /api/
Disallow: /health/
Disallow: /_blazor/
Disallow: /_framework/

# Allow important pages
Allow: /merchants
Allow: /products
Allow: /cart
Allow: /orders
Allow: /account
Allow: /help
Allow: /contact";

            return Content(robots, "text/plain");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating robots.txt");
            return StatusCode(500);
        }
    }

    private string GenerateSitemap(string baseUrl)
    {
        var urls = new List<SitemapUrl>
        {
            new SitemapUrl($"{baseUrl}/", DateTime.UtcNow, ChangeFrequency.Daily, 1.0),
            new SitemapUrl($"{baseUrl}/merchants", DateTime.UtcNow, ChangeFrequency.Daily, 0.9),
            new SitemapUrl($"{baseUrl}/cart", DateTime.UtcNow, ChangeFrequency.Weekly, 0.7),
            new SitemapUrl($"{baseUrl}/orders", DateTime.UtcNow, ChangeFrequency.Weekly, 0.8),
            new SitemapUrl($"{baseUrl}/account", DateTime.UtcNow, ChangeFrequency.Monthly, 0.6),
            new SitemapUrl($"{baseUrl}/help", DateTime.UtcNow, ChangeFrequency.Monthly, 0.5),
            new SitemapUrl($"{baseUrl}/contact", DateTime.UtcNow, ChangeFrequency.Monthly, 0.5),
            new SitemapUrl($"{baseUrl}/login", DateTime.UtcNow, ChangeFrequency.Monthly, 0.4),
            new SitemapUrl($"{baseUrl}/register", DateTime.UtcNow, ChangeFrequency.Monthly, 0.4),
            
            // Category pages
            new SitemapUrl($"{baseUrl}/merchants?category=market", DateTime.UtcNow, ChangeFrequency.Daily, 0.8),
            new SitemapUrl($"{baseUrl}/merchants?category=pharmacy", DateTime.UtcNow, ChangeFrequency.Daily, 0.8),
            new SitemapUrl($"{baseUrl}/merchants?category=restaurant", DateTime.UtcNow, ChangeFrequency.Daily, 0.8),
            new SitemapUrl($"{baseUrl}/merchants?category=other", DateTime.UtcNow, ChangeFrequency.Daily, 0.8),
        };

        var sitemap = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
{string.Join(Environment.NewLine, urls.Select(url => url.ToXml()))}
</urlset>";

        return sitemap;
    }
}

public class SitemapUrl
{
    public string Location { get; set; }
    public DateTime LastModified { get; set; }
    public ChangeFrequency ChangeFrequency { get; set; }
    public double Priority { get; set; }

    public SitemapUrl(string location, DateTime lastModified, ChangeFrequency changeFrequency, double priority)
    {
        Location = location;
        LastModified = lastModified;
        ChangeFrequency = changeFrequency;
        Priority = priority;
    }

    public string ToXml()
    {
        return $@"  <url>
    <loc>{Location}</loc>
    <lastmod>{LastModified:yyyy-MM-ddTHH:mm:ssZ}</lastmod>
    <changefreq>{ChangeFrequency.ToString().ToLower()}</changefreq>
    <priority>{Priority:F1}</priority>
  </url>";
    }
}

public enum ChangeFrequency
{
    Always,
    Hourly,
    Daily,
    Weekly,
    Monthly,
    Yearly,
    Never
}
