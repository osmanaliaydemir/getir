using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface IUserLanguageService
{
    Task<UserLanguagePreferenceDto?> GetUserLanguagePreferenceAsync(Guid userId);
    Task<UserLanguagePreferenceDto> SetUserLanguagePreferenceAsync(Guid userId, SetUserLanguageRequest request);
    Task<List<UserLanguagePreferenceDto>> GetUserLanguagePreferencesAsync(Guid userId);
    Task<bool> RemoveUserLanguagePreferenceAsync(Guid userId, LanguageCode languageCode);
    Task<LanguageCode> GetUserPreferredLanguageAsync(Guid userId);
    Task<bool> SetUserPreferredLanguageAsync(Guid userId, LanguageCode languageCode);
}
