using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface ILanguageService
{
    Task<List<LanguageDto>> GetAllLanguagesAsync();
    Task<LanguageDto?> GetLanguageByIdAsync(Guid id);
    Task<LanguageDto?> GetLanguageByCodeAsync(LanguageCode code);
    Task<LanguageDto?> GetDefaultLanguageAsync();
    Task<LanguageDto> CreateLanguageAsync(CreateLanguageRequest request);
    Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageRequest request);
    Task<bool> DeleteLanguageAsync(Guid id);
    Task<bool> SetDefaultLanguageAsync(Guid id);
    Task<List<LanguageStatisticsDto>> GetLanguageStatisticsAsync();
}
