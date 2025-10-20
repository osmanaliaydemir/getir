using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

/// <summary>
/// Dil yönetimi servisi: desteklenen dillerin CRUD işlemleri ve istatistikler.
/// </summary>
public interface ILanguageService
{
    /// <summary>Tüm dilleri getirir (cache).</summary>
    Task<List<LanguageDto>> GetAllLanguagesAsync();
    /// <summary>Dili ID ile getirir.</summary>
    Task<LanguageDto?> GetLanguageByIdAsync(Guid id);
    /// <summary>Dili kod ile getirir.</summary>
    Task<LanguageDto?> GetLanguageByCodeAsync(LanguageCode code);
    /// <summary>Varsayılan dili getirir.</summary>
    Task<LanguageDto?> GetDefaultLanguageAsync();
    /// <summary>Yeni dil oluşturur (cache invalidation).</summary>
    Task<LanguageDto> CreateLanguageAsync(CreateLanguageRequest request);
    /// <summary>Dil günceller (cache invalidation).</summary>
    Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageRequest request);
    /// <summary>Dil siler (cache invalidation).</summary>
    Task<bool> DeleteLanguageAsync(Guid id);
    /// <summary>Varsayılan dil ayarlar.</summary>
    Task<bool> SetDefaultLanguageAsync(Guid id);
    /// <summary>Dil istatistiklerini getirir.</summary>
    Task<List<LanguageStatisticsDto>> GetLanguageStatisticsAsync();
}
