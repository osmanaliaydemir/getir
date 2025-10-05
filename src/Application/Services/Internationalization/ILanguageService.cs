using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface ILanguageService
{
    Task<Result<List<LanguageDto>>> GetAllLanguagesAsync();
    Task<Result<LanguageDto>> GetLanguageByIdAsync(Guid id);
    Task<Result<LanguageDto>> GetLanguageByCodeAsync(LanguageCode code);
    Task<Result<LanguageDto>> GetDefaultLanguageAsync();
    Task<Result<LanguageDto>> CreateLanguageAsync(CreateLanguageRequest request);
    Task<Result<LanguageDto>> UpdateLanguageAsync(Guid id, UpdateLanguageRequest request);
    Task<Result<bool>> DeleteLanguageAsync(Guid id);
    Task<Result<bool>> SetDefaultLanguageAsync(Guid id);
    Task<Result<List<LanguageStatisticsDto>>> GetLanguageStatisticsAsync();
}
