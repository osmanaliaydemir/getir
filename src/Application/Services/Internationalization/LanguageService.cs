using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Internationalization;

/// <summary>
/// Dil yönetimi servisi: desteklenen dillerin cache'li yönetimi.
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<LanguageService> _logger;

    public LanguageService(ICacheService cacheService, ILogger<LanguageService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm dilleri getirir (cache).
    /// </summary>
    public async Task<List<LanguageDto>> GetAllLanguagesAsync()
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.SupportedLanguages();

            var cached = await _cacheService.GetAsync<List<LanguageDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            // Mock data for now (future: database lookup)
            var languages = new List<LanguageDto>
            {
                new() { Id = Guid.NewGuid(), Code = LanguageCode.Turkish, Name = "Turkish", NativeName = "Türkçe", CultureCode = "tr-TR", IsRtl = false, IsActive = true, IsDefault = true, SortOrder = 1, FlagIcon = "🇹🇷" },
                new() { Id = Guid.NewGuid(), Code = LanguageCode.English, Name = "English", NativeName = "English", CultureCode = "en-US", IsRtl = false, IsActive = true, IsDefault = false, SortOrder = 2, FlagIcon = "🇺🇸" },
                new() { Id = Guid.NewGuid(), Code = LanguageCode.Arabic, Name = "Arabic", NativeName = "العربية", CultureCode = "ar-SA", IsRtl = true, IsActive = true, IsDefault = false, SortOrder = 3, FlagIcon = "🇸🇦" }
            };

            // Cache for 4 hours (very static data)
            await _cacheService.SetAsync(cacheKey, languages, TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong));

            return languages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all languages");
            return new List<LanguageDto>();
        }
    }

    /// <summary>Dili ID ile getirir.</summary>
    public async Task<LanguageDto?> GetLanguageByIdAsync(Guid id)
    {
        var languages = await GetAllLanguagesAsync();
        return languages.FirstOrDefault(l => l.Id == id);
    }

    /// <summary>Dili kod ile getirir.</summary>
    public async Task<LanguageDto?> GetLanguageByCodeAsync(LanguageCode code)
    {
        var languages = await GetAllLanguagesAsync();
        return languages.FirstOrDefault(l => l.Code == code);
    }

    /// <summary>Varsayılan dili getirir.</summary>
    public async Task<LanguageDto?> GetDefaultLanguageAsync()
    {
        var languages = await GetAllLanguagesAsync();
        return languages.FirstOrDefault(l => l.IsDefault);
    }

    /// <summary>Yeni dil oluşturur (cache invalidation).</summary>
    public async Task<LanguageDto> CreateLanguageAsync(CreateLanguageRequest request)
    {
        var language = new LanguageDto
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            NativeName = request.NativeName,
            CultureCode = request.CultureCode,
            IsRtl = request.IsRtl,
            IsActive = true,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,
            FlagIcon = request.FlagIcon
        };

        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveAsync(CacheKeys.SupportedLanguages());
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return language;
    }

    /// <summary>Dil günceller (cache invalidation).</summary>
    public async Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageRequest request)
    {
        var language = new LanguageDto
        {
            Id = id,
            Code = LanguageCode.Turkish, // Mock
            Name = request.Name,
            NativeName = request.NativeName,
            CultureCode = request.CultureCode,
            IsRtl = request.IsRtl,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,
            FlagIcon = request.FlagIcon
        };

        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveAsync(CacheKeys.SupportedLanguages());
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return language;
    }

    /// <summary>Dil siler (cache invalidation).</summary>
    public async Task<bool> DeleteLanguageAsync(Guid id)
    {
        // ============= CACHE INVALIDATION =============
        await _cacheService.RemoveAsync(CacheKeys.SupportedLanguages());
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllTranslationsPattern());

        return true;
    }

    /// <summary>Varsayılan dil ayarlar.</summary>
    public Task<bool> SetDefaultLanguageAsync(Guid id)
    {
        return Task.FromResult(true);
    }

    /// <summary>Dil istatistiklerini getirir (çeviri tamamlanma oranları).</summary>
    public Task<List<LanguageStatisticsDto>> GetLanguageStatisticsAsync()
    {
        var statistics = new List<LanguageStatisticsDto>
        {
            new() { LanguageCode = LanguageCode.Turkish, LanguageName = "Turkish", TotalTranslations = 100, ActiveTranslations = 95, InactiveTranslations = 5, UserCount = 1000, CompletionPercentage = 95.0 },
            new() { LanguageCode = LanguageCode.English, LanguageName = "English", TotalTranslations = 80, ActiveTranslations = 75, InactiveTranslations = 5, UserCount = 500, CompletionPercentage = 75.0 },
            new() { LanguageCode = LanguageCode.Arabic, LanguageName = "Arabic", TotalTranslations = 60, ActiveTranslations = 50, InactiveTranslations = 10, UserCount = 200, CompletionPercentage = 50.0 }
        };
        return Task.FromResult(statistics);
    }
}
