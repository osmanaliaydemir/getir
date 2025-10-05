using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public class TranslationService : ITranslationService
{
    public Task<TranslationDto?> GetTranslationAsync(string key, LanguageCode languageCode)
    {
        // Mock translation data
        var translations = GetMockTranslations();
        return Task.FromResult(translations.FirstOrDefault(t => t.Key == key && t.LanguageCode == languageCode));
    }

    public Task<GetTranslationsByKeysResponse> GetTranslationsByKeysAsync(GetTranslationsByKeysRequest request)
    {
        var translations = GetMockTranslations();
        var foundTranslations = translations
            .Where(t => request.Keys.Contains(t.Key) && t.LanguageCode == request.LanguageCode)
            .ToDictionary(t => t.Key, t => t.Value);

        var missingKeys = request.Keys.Except(foundTranslations.Keys).ToList();

        var response = new GetTranslationsByKeysResponse
        {
            Translations = foundTranslations,
            LanguageCode = request.LanguageCode,
            FoundCount = foundTranslations.Count,
            MissingCount = missingKeys.Count,
            MissingKeys = missingKeys
        };

        return Task.FromResult(response);
    }

    public Task<TranslationSearchResponse> SearchTranslationsAsync(TranslationSearchRequest request)
    {
        var translations = GetMockTranslations();
        
        var filtered = translations.Where(t =>
            (string.IsNullOrEmpty(request.Key) || t.Key.Contains(request.Key, StringComparison.OrdinalIgnoreCase)) &&
            (!request.LanguageCode.HasValue || t.LanguageCode == request.LanguageCode) &&
            (string.IsNullOrEmpty(request.Category) || t.Category == request.Category) &&
            (string.IsNullOrEmpty(request.Context) || t.Context == request.Context) &&
            (!request.IsActive.HasValue || t.IsActive == request.IsActive)
        ).ToList();

        var totalCount = filtered.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var pagedTranslations = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = new TranslationSearchResponse
        {
            Translations = pagedTranslations,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return Task.FromResult(response);
    }

    public Task<TranslationDto> CreateTranslationAsync(CreateTranslationRequest request)
    {
        var translation = new TranslationDto
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            Value = request.Value,
            LanguageCode = request.LanguageCode,
            Category = request.Category,
            Context = request.Context,
            Description = request.Description,
            IsActive = true
        };
        return Task.FromResult(translation);
    }

    public Task<TranslationDto> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request)
    {
        var translation = new TranslationDto
        {
            Id = id,
            Key = "mock-key", // Mock
            Value = request.Value,
            LanguageCode = LanguageCode.Turkish, // Mock
            Category = request.Category,
            Context = request.Context,
            Description = request.Description,
            IsActive = request.IsActive
        };
        return Task.FromResult(translation);
    }

    public Task<bool> DeleteTranslationAsync(Guid id)
    {
        return Task.FromResult(true);
    }

    public Task<bool> BulkCreateTranslationsAsync(BulkTranslationRequest request)
    {
        return Task.FromResult(true);
    }

    public Task<bool> BulkUpdateTranslationsAsync(BulkTranslationRequest request)
    {
        return Task.FromResult(true);
    }

    public Task<List<TranslationDto>> GetTranslationsByCategoryAsync(string category, LanguageCode languageCode)
    {
        var translations = GetMockTranslations();
        var filtered = translations.Where(t => t.Category == category && t.LanguageCode == languageCode).ToList();
        return Task.FromResult(filtered);
    }

    public Task<Dictionary<string, string>> GetTranslationsDictionaryAsync(LanguageCode languageCode, string? category = null)
    {
        var translations = GetMockTranslations();
        var filtered = translations.Where(t => t.LanguageCode == languageCode && (category == null || t.Category == category));
        var dictionary = filtered.ToDictionary(t => t.Key, t => t.Value);
        return Task.FromResult(dictionary);
    }

    public Task<List<string>> GetMissingTranslationsAsync(LanguageCode languageCode, string? category = null)
    {
        // Mock missing keys
        var missingKeys = new List<string> { "missing.key.1", "missing.key.2" };
        return Task.FromResult(missingKeys);
    }

    public Task<bool> ImportTranslationsFromJsonAsync(string jsonContent, LanguageCode languageCode, string? category = null)
    {
        return Task.FromResult(true);
    }

    public Task<string> ExportTranslationsToJsonAsync(LanguageCode languageCode, string? category = null)
    {
        var mockJson = "{\"welcome\": \"Welcome\", \"goodbye\": \"Goodbye\"}";
        return Task.FromResult(mockJson);
    }

    private List<TranslationDto> GetMockTranslations()
    {
        return new List<TranslationDto>
        {
            new() { Id = Guid.NewGuid(), Key = "welcome", Value = "Hoş Geldiniz", LanguageCode = LanguageCode.Turkish, Category = "UI", IsActive = true },
            new() { Id = Guid.NewGuid(), Key = "welcome", Value = "Welcome", LanguageCode = LanguageCode.English, Category = "UI", IsActive = true },
            new() { Id = Guid.NewGuid(), Key = "welcome", Value = "أهلاً وسهلاً", LanguageCode = LanguageCode.Arabic, Category = "UI", IsActive = true },
            new() { Id = Guid.NewGuid(), Key = "goodbye", Value = "Hoşça Kalın", LanguageCode = LanguageCode.Turkish, Category = "UI", IsActive = true },
            new() { Id = Guid.NewGuid(), Key = "goodbye", Value = "Goodbye", LanguageCode = LanguageCode.English, Category = "UI", IsActive = true },
            new() { Id = Guid.NewGuid(), Key = "goodbye", Value = "وداعاً", LanguageCode = LanguageCode.Arabic, Category = "UI", IsActive = true }
        };
    }
}
