using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Internationalization;

/// <summary>
/// Çeviri yönetimi servisi: key-value bazlı çevirilerin cache'li yönetimi, arama, toplu işlemler, import/export.
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(ICacheService cacheService, ILogger<TranslationService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Belirtilen anahtar ve dil için çeviri getirir (cache).
    /// </summary>
    public async Task<TranslationDto?> GetTranslationAsync(string key, LanguageCode languageCode)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.Translation(languageCode.ToString(), key);

            var cached = await _cacheService.GetAsync<TranslationDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // Mock translation data (future: database lookup)
            var translations = GetMockTranslations();
            var translation = translations.FirstOrDefault(t => t.Key == key && t.LanguageCode == languageCode);

            if (translation != null)
            {
                // Cache for 4 hours
                await _cacheService.SetAsync(cacheKey, translation, TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong));
            }

            return translation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translation: {Key}, {LanguageCode}", key, languageCode);
            return null;
        }
    }

    /// <summary>
    /// Birden fazla anahtar için çevirileri toplu getirir (cache).
    /// </summary>
    public async Task<GetTranslationsByKeysResponse> GetTranslationsByKeysAsync(GetTranslationsByKeysRequest request)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.AllTranslations(request.LanguageCode.ToString());

            var cached = await _cacheService.GetAsync<Dictionary<string, string>>(cacheKey);
            
            if (cached == null)
            {
                var translations = GetMockTranslations();
                cached = translations
                    .Where(t => t.LanguageCode == request.LanguageCode)
                    .ToDictionary(t => t.Key, t => t.Value);

                // Cache for 4 hours
                await _cacheService.SetAsync(cacheKey, cached, TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong));
            }

            var foundTranslations = cached
                .Where(t => request.Keys.Contains(t.Key))
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

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations by keys: {LanguageCode}", request.LanguageCode);
            return new GetTranslationsByKeysResponse 
            { 
                Translations = new Dictionary<string, string>(),
                LanguageCode = request.LanguageCode 
            };
        }
    }

    /// <summary>
    /// Çevirilerde arama yapar (filtreleme ve sayfalama).
    /// </summary>
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

    /// <summary>Yeni çeviri oluşturur (cache invalidation).</summary>
    public async Task<TranslationDto> CreateTranslationAsync(CreateTranslationRequest request)
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

        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return translation;
    }

    /// <summary>Çeviri günceller (cache invalidation).</summary>
    public async Task<TranslationDto> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request)
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

        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return translation;
    }

    /// <summary>Çeviri siler (cache invalidation).</summary>
    public async Task<bool> DeleteTranslationAsync(Guid id)
    {
        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return true;
    }

    /// <summary>Toplu çeviri oluşturur (cache invalidation).</summary>
    public async Task<bool> BulkCreateTranslationsAsync(BulkTranslationRequest request)
    {
        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return true;
    }

    /// <summary>Toplu çeviri günceller (cache invalidation).</summary>
    public async Task<bool> BulkUpdateTranslationsAsync(BulkTranslationRequest request)
    {
        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return true;
    }

    /// <summary>Kategoriye göre çevirileri getirir.</summary>
    public async Task<List<TranslationDto>> GetTranslationsByCategoryAsync(string category, LanguageCode languageCode)
    {
        var translations = GetMockTranslations();
        var filtered = translations.Where(t => t.Category == category && t.LanguageCode == languageCode).ToList();
        return filtered;
    }

    /// <summary>Çevirileri sözlük formatında getirir (cache).</summary>
    public async Task<Dictionary<string, string>> GetTranslationsDictionaryAsync(LanguageCode languageCode, string? category = null)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.AllTranslations(languageCode.ToString());

            var cached = await _cacheService.GetAsync<Dictionary<string, string>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var translations = GetMockTranslations();
            var dictionary = translations
                .Where(t => t.LanguageCode == languageCode && (category == null || t.Category == category))
                .ToDictionary(t => t.Key, t => t.Value);

            // Cache for 4 hours
            await _cacheService.SetAsync(cacheKey, dictionary, TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong));

            return dictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations dictionary: {LanguageCode}", languageCode);
            return new Dictionary<string, string>();
        }
    }

    /// <summary>Eksik çevirileri getirir.</summary>
    public Task<List<string>> GetMissingTranslationsAsync(LanguageCode languageCode, string? category = null)
    {
        // Mock missing keys
        var missingKeys = new List<string> { "missing.key.1", "missing.key.2" };
        return Task.FromResult(missingKeys);
    }

    /// <summary>JSON'dan çevirileri içe aktarır.</summary>
    public Task<bool> ImportTranslationsFromJsonAsync(string jsonContent, LanguageCode languageCode, string? category = null)
    {
        return Task.FromResult(true);
    }

    /// <summary>Çevirileri JSON formatında dışa aktarır.</summary>
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
