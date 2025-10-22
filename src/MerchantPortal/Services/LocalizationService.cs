using System.Globalization;
using System.Text.Json;

namespace Getir.MerchantPortal.Services;

/// <summary>
/// Simple JSON-based localization service
/// </summary>
public interface ILocalizationService
{
    string GetString(string key);
    string GetString(string key, string culture);
    Task LoadTranslationsAsync();
}

public class LocalizationService : ILocalizationService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalizationService> _logger;
    private Dictionary<string, Dictionary<string, string>> _translations = new();
    private readonly object _lock = new();

    public LocalizationService(IWebHostEnvironment environment, ILogger<LocalizationService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task LoadTranslationsAsync()
    {
        lock (_lock)
        {
            try
            {
                var jsonPath = Path.Combine(_environment.ContentRootPath, "Resources", "localization.json");
                
                if (!File.Exists(jsonPath))
                {
                    _logger.LogWarning("Localization file not found: {Path}", jsonPath);
                    return;
                }

                var jsonContent = File.ReadAllText(jsonPath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent) 
                    ?? new Dictionary<string, Dictionary<string, string>>();
                    
                _logger.LogInformation("Loaded {Count} cultures with {TotalKeys} total keys", 
                    _translations.Count, 
                    _translations.Values.Sum(x => x.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load localization file");
            }
        }
        await Task.CompletedTask;
    }

    public string GetString(string key)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        return GetString(key, culture);
    }

    public string GetString(string key, string culture)
    {
        // Ensure translations are loaded
        if (_translations.Count == 0)
        {
            LoadTranslationsAsync().Wait();
        }

        // Fallback chain: culture -> tr-TR -> key
        if (_translations.ContainsKey(culture) && _translations[culture].ContainsKey(key))
            return _translations[culture][key];

        if (_translations.ContainsKey("tr-TR") && _translations["tr-TR"].ContainsKey(key))
            return _translations["tr-TR"][key];

        return key; // Return key as fallback
    }
}
