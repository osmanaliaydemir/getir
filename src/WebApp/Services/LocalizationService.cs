using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace WebApp.Services;

public class LocalizationService
{
    private readonly IStringLocalizer<LocalizationService> _localizer;
    private readonly ILogger<LocalizationService> _logger;
    private readonly Dictionary<string, Dictionary<string, string>> _resources;
    private string _currentCulture = "tr";

    public LocalizationService(IStringLocalizer<LocalizationService> localizer, ILogger<LocalizationService> logger)
    {
        _localizer = localizer;
        _logger = logger;
        _resources = new Dictionary<string, Dictionary<string, string>>();
        LoadResources();
    }

    public string GetString(string key, params object[] args)
    {
        try
        {
            if (_resources.ContainsKey(_currentCulture) && 
                _resources[_currentCulture].ContainsKey(key))
            {
                var value = _resources[_currentCulture][key];
                return args.Length > 0 ? string.Format(value, args) : value;
            }

            // Fallback to default culture (Turkish)
            if (_currentCulture != "tr" && 
                _resources.ContainsKey("tr") && 
                _resources["tr"].ContainsKey(key))
            {
                var value = _resources["tr"][key];
                return args.Length > 0 ? string.Format(value, args) : value;
            }

            // Fallback to key if not found
            _logger.LogWarning("Localization key not found: {Key} for culture: {Culture}", key, _currentCulture);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized string for key: {Key}", key);
            return key;
        }
    }

    public void SetCulture(string culture)
    {
        if (string.IsNullOrEmpty(culture))
            culture = "tr";

        _currentCulture = culture.ToLower();
        _logger.LogInformation("Culture changed to: {Culture}", _currentCulture);
    }

    public string GetCurrentCulture()
    {
        return _currentCulture;
    }

    public List<string> GetSupportedCultures()
    {
        return _resources.Keys.ToList();
    }

    public bool IsRtlLanguage(string culture)
    {
        return culture?.ToLower() == "ar";
    }

    public string GetDirection(string culture)
    {
        return IsRtlLanguage(culture) ? "rtl" : "ltr";
    }

    public string GetLanguageName(string culture)
    {
        return culture?.ToLower() switch
        {
            "tr" => "Türkçe",
            "en" => "English", 
            "ar" => "العربية",
            _ => "Türkçe"
        };
    }

    public string GetLanguageCode(string culture)
    {
        return culture?.ToUpper() ?? "TR";
    }

    private void LoadResources()
    {
        try
        {
            // Load Turkish resources
            var trPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "tr.json");
            if (File.Exists(trPath))
            {
                var trContent = File.ReadAllText(trPath);
                var trResources = JsonSerializer.Deserialize<Dictionary<string, object>>(trContent);
                if (trResources != null)
                {
                    _resources["tr"] = FlattenDictionary(trResources);
                }
            }

            // Load English resources
            var enPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "en.json");
            if (File.Exists(enPath))
            {
                var enContent = File.ReadAllText(enPath);
                var enResources = JsonSerializer.Deserialize<Dictionary<string, object>>(enContent);
                if (enResources != null)
                {
                    _resources["en"] = FlattenDictionary(enResources);
                }
            }

            // Load Arabic resources
            var arPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ar.json");
            if (File.Exists(arPath))
            {
                var arContent = File.ReadAllText(arPath);
                var arResources = JsonSerializer.Deserialize<Dictionary<string, object>>(arContent);
                if (arResources != null)
                {
                    _resources["ar"] = FlattenDictionary(arResources);
                }
            }

            _logger.LogInformation("Loaded resources for cultures: {Cultures}", string.Join(", ", _resources.Keys));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading localization resources");
        }
    }

    private Dictionary<string, string> FlattenDictionary(Dictionary<string, object> dictionary, string prefix = "")
    {
        var result = new Dictionary<string, string>();
        
        foreach (var kvp in dictionary)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
            
            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var nestedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                    if (nestedDict != null)
                    {
                        var flattened = FlattenDictionary(nestedDict, key);
                        foreach (var nested in flattened)
                        {
                            result[nested.Key] = nested.Value;
                        }
                    }
                }
                else
                {
                    result[key] = element.GetString() ?? string.Empty;
                }
            }
        }
        
        return result;
    }

    // Convenience methods for common translations
    public string Loading => GetString("Common.Loading");
    public string Error => GetString("Common.Error");
    public string Success => GetString("Common.Success");
    public string Warning => GetString("Common.Warning");
    public string Info => GetString("Common.Info");
    public string Yes => GetString("Common.Yes");
    public string No => GetString("Common.No");
    public string Cancel => GetString("Common.Cancel");
    public string Save => GetString("Common.Save");
    public string Delete => GetString("Common.Delete");
    public string Edit => GetString("Common.Edit");
    public string Add => GetString("Common.Add");
    public string Search => GetString("Common.Search");
    public string Filter => GetString("Common.Filter");
    public string Sort => GetString("Common.Sort");
    public string Close => GetString("Common.Close");
    public string Back => GetString("Common.Back");
    public string Next => GetString("Common.Next");
    public string Previous => GetString("Common.Previous");
    public string Submit => GetString("Common.Submit");
    public string Reset => GetString("Common.Reset");
    public string Confirm => GetString("Common.Confirm");
    public string Retry => GetString("Common.Retry");
    public string Refresh => GetString("Common.Refresh");
}
