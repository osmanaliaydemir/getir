using Getir.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Internationalization;

public class LocalizationService : ILocalizationService
{
    private readonly ITranslationService _translationService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LocalizationService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public LocalizationService(
        ITranslationService translationService,
        IMemoryCache cache,
        ILogger<LocalizationService> logger)
    {
        _translationService = translationService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetLocalizedStringAsync(string key, LanguageCode languageCode, params object[] args)
    {
        try
        {
            var cacheKey = $"translation_{languageCode}_{key}";
            
            if (_cache.TryGetValue(cacheKey, out string? cachedValue) && !string.IsNullOrEmpty(cachedValue))
            {
                return FormatString(cachedValue, args);
            }

            var result = await _translationService.GetTranslationAsync(key, languageCode);
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data?.Value))
            {
                _cache.Set(cacheKey, result.Data.Value, _cacheExpiration);
                return FormatString(result.Data.Value, args);
            }

            // Fallback to default language
            if (languageCode != LanguageCode.Turkish)
            {
                return await GetLocalizedStringAsync(key, LanguageCode.Turkish, args);
            }

            return key; // Return key if no translation found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized string: {Key}, {LanguageCode}", key, languageCode);
            return key;
        }
    }

    public async Task<Dictionary<string, string>> GetLocalizedStringsAsync(string[] keys, LanguageCode languageCode)
    {
        try
        {
            var cacheKey = $"translations_{languageCode}_{string.Join(",", keys.OrderBy(k => k))}";
            
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedTranslations) && cachedTranslations != null)
            {
                return cachedTranslations;
            }

            var request = new GetTranslationsByKeysRequest
            {
                Keys = keys.ToList(),
                LanguageCode = languageCode
            };

            var result = await _translationService.GetTranslationsByKeysAsync(request);
            if (result.IsSuccess && result.Data != null)
            {
                var translations = result.Data.Translations;
                
                // Add fallback for missing translations
                foreach (var key in keys)
                {
                    if (!translations.ContainsKey(key))
                    {
                        if (languageCode != LanguageCode.Turkish)
                        {
                            var fallbackResult = await _translationService.GetTranslationAsync(key, LanguageCode.Turkish);
                            if (fallbackResult.IsSuccess && !string.IsNullOrEmpty(fallbackResult.Data?.Value))
                            {
                                translations[key] = fallbackResult.Data.Value;
                            }
                            else
                            {
                                translations[key] = key; // Use key as fallback
                            }
                        }
                        else
                        {
                            translations[key] = key; // Use key as fallback
                        }
                    }
                }

                _cache.Set(cacheKey, translations, _cacheExpiration);
                return translations;
            }

            // Fallback to default language
            if (languageCode != LanguageCode.Turkish)
            {
                return await GetLocalizedStringsAsync(keys, LanguageCode.Turkish);
            }

            return keys.ToDictionary(k => k, k => k);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized strings: {Keys}, {LanguageCode}", string.Join(",", keys), languageCode);
            return keys.ToDictionary(k => k, k => k);
        }
    }

    public async Task<string> GetLocalizedStringAsync(string key, LanguageCode languageCode, string? category = null, params object[] args)
    {
        try
        {
            var cacheKey = $"translation_{languageCode}_{category}_{key}";
            
            if (_cache.TryGetValue(cacheKey, out string? cachedValue) && !string.IsNullOrEmpty(cachedValue))
            {
                return FormatString(cachedValue, args);
            }

            var result = await _translationService.GetTranslationAsync(key, languageCode);
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data?.Value) && 
                (string.IsNullOrEmpty(category) || result.Data.Category == category))
            {
                _cache.Set(cacheKey, result.Data.Value, _cacheExpiration);
                return FormatString(result.Data.Value, args);
            }

            // Fallback to default language
            if (languageCode != LanguageCode.Turkish)
            {
                return await GetLocalizedStringAsync(key, LanguageCode.Turkish, category, args);
            }

            return key; // Return key if no translation found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized string: {Key}, {LanguageCode}, {Category}", key, languageCode, category);
            return key;
        }
    }

    public async Task<Dictionary<string, string>> GetLocalizedStringsByCategoryAsync(string category, LanguageCode languageCode)
    {
        try
        {
            var cacheKey = $"translations_category_{languageCode}_{category}";
            
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedTranslations) && cachedTranslations != null)
            {
                return cachedTranslations;
            }

            var result = await _translationService.GetTranslationsDictionaryAsync(languageCode, category);
            if (result.IsSuccess && result.Data != null)
            {
                _cache.Set(cacheKey, result.Data, _cacheExpiration);
                return result.Data;
            }

            // Fallback to default language
            if (languageCode != LanguageCode.Turkish)
            {
                return await GetLocalizedStringsByCategoryAsync(category, LanguageCode.Turkish);
            }

            return new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized strings by category: {Category}, {LanguageCode}", category, languageCode);
            return new Dictionary<string, string>();
        }
    }

    public async Task<string> GetLocalizedStringWithFallbackAsync(string key, LanguageCode languageCode, string fallbackValue, params object[] args)
    {
        try
        {
            var result = await _translationService.GetTranslationAsync(key, languageCode);
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data?.Value))
            {
                return FormatString(result.Data.Value, args);
            }

            // Fallback to default language
            if (languageCode != LanguageCode.Turkish)
            {
                var fallbackResult = await _translationService.GetTranslationAsync(key, LanguageCode.Turkish);
                if (fallbackResult.IsSuccess && !string.IsNullOrEmpty(fallbackResult.Data?.Value))
                {
                    return FormatString(fallbackResult.Data.Value, args);
                }
            }

            return FormatString(fallbackValue, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting localized string with fallback: {Key}, {LanguageCode}", key, languageCode);
            return FormatString(fallbackValue, args);
        }
    }

    public async Task<bool> IsTranslationAvailableAsync(string key, LanguageCode languageCode)
    {
        try
        {
            var result = await _translationService.GetTranslationAsync(key, languageCode);
            return result.IsSuccess && !string.IsNullOrEmpty(result.Data?.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking translation availability: {Key}, {LanguageCode}", key, languageCode);
            return false;
        }
    }

    public async Task<List<string>> GetAvailableCategoriesAsync(LanguageCode languageCode)
    {
        try
        {
            var cacheKey = $"categories_{languageCode}";
            
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedCategories) && cachedCategories != null)
            {
                return cachedCategories;
            }

            var result = await _translationService.GetTranslationsDictionaryAsync(languageCode);
            if (result.IsSuccess && result.Data != null)
            {
                // This is a simplified approach - in a real implementation, you might want to query categories directly
                var categories = new List<string> { "UI", "API", "Email", "SMS", "Notification", "Error" };
                _cache.Set(cacheKey, categories, _cacheExpiration);
                return categories;
            }

            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available categories: {LanguageCode}", languageCode);
            return new List<string>();
        }
    }

    private static string FormatString(string format, object[] args)
    {
        if (args == null || args.Length == 0)
        {
            return format;
        }

        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format; // Return original format if formatting fails
        }
    }
}
