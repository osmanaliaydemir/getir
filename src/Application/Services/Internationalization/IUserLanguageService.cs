using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface IUserLanguageService
{
    Task<Result<UserLanguagePreferenceDto>> GetUserLanguagePreferenceAsync(Guid userId);
    Task<Result<UserLanguagePreferenceDto>> SetUserLanguagePreferenceAsync(Guid userId, SetUserLanguageRequest request);
    Task<Result<List<UserLanguagePreferenceDto>>> GetUserLanguagePreferencesAsync(Guid userId);
    Task<Result<bool>> RemoveUserLanguagePreferenceAsync(Guid userId, LanguageCode languageCode);
    Task<Result<LanguageCode>> GetUserPreferredLanguageAsync(Guid userId);
    Task<Result<bool>> SetUserPreferredLanguageAsync(Guid userId, LanguageCode languageCode);
}
