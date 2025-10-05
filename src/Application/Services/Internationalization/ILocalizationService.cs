using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface ILocalizationService
{
    Task<string> GetLocalizedStringAsync(string key, LanguageCode languageCode, params object[] args);
    Task<Dictionary<string, string>> GetLocalizedStringsAsync(string[] keys, LanguageCode languageCode);
    Task<string> GetLocalizedStringAsync(string key, LanguageCode languageCode, string? category = null, params object[] args);
    Task<Dictionary<string, string>> GetLocalizedStringsByCategoryAsync(string category, LanguageCode languageCode);
    Task<string> GetLocalizedStringWithFallbackAsync(string key, LanguageCode languageCode, string fallbackValue, params object[] args);
    Task<bool> IsTranslationAvailableAsync(string key, LanguageCode languageCode);
    Task<List<string>> GetAvailableCategoriesAsync(LanguageCode languageCode);
}
