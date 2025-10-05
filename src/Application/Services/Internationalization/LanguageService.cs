using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public class LanguageService : ILanguageService
{
    public Task<List<LanguageDto>> GetAllLanguagesAsync()
    {
        // Mock data for now
        var languages = new List<LanguageDto>
        {
            new() { Id = Guid.NewGuid(), Code = LanguageCode.Turkish, Name = "Turkish", NativeName = "Türkçe", CultureCode = "tr-TR", IsRtl = false, IsActive = true, IsDefault = true, SortOrder = 1, FlagIcon = "🇹🇷" },
            new() { Id = Guid.NewGuid(), Code = LanguageCode.English, Name = "English", NativeName = "English", CultureCode = "en-US", IsRtl = false, IsActive = true, IsDefault = false, SortOrder = 2, FlagIcon = "🇺🇸" },
            new() { Id = Guid.NewGuid(), Code = LanguageCode.Arabic, Name = "Arabic", NativeName = "العربية", CultureCode = "ar-SA", IsRtl = true, IsActive = true, IsDefault = false, SortOrder = 3, FlagIcon = "🇸🇦" }
        };
        return Task.FromResult(languages);
    }

    public Task<LanguageDto?> GetLanguageByIdAsync(Guid id)
    {
        var languages = GetAllLanguagesAsync().Result;
        return Task.FromResult(languages.FirstOrDefault(l => l.Id == id));
    }

    public Task<LanguageDto?> GetLanguageByCodeAsync(LanguageCode code)
    {
        var languages = GetAllLanguagesAsync().Result;
        return Task.FromResult(languages.FirstOrDefault(l => l.Code == code));
    }

    public Task<LanguageDto?> GetDefaultLanguageAsync()
    {
        var languages = GetAllLanguagesAsync().Result;
        return Task.FromResult(languages.FirstOrDefault(l => l.IsDefault));
    }

    public Task<LanguageDto> CreateLanguageAsync(CreateLanguageRequest request)
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
        return Task.FromResult(language);
    }

    public Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageRequest request)
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
        return Task.FromResult(language);
    }

    public Task<bool> DeleteLanguageAsync(Guid id)
    {
        return Task.FromResult(true);
    }

    public Task<bool> SetDefaultLanguageAsync(Guid id)
    {
        return Task.FromResult(true);
    }

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
