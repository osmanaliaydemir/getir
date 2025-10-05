using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public class UserLanguageService : IUserLanguageService
{
    public Task<UserLanguagePreferenceDto?> GetUserLanguagePreferenceAsync(Guid userId)
    {
        // Mock data
        var preference = new UserLanguagePreferenceDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LanguageId = Guid.NewGuid(),
            LanguageCode = LanguageCode.Turkish,
            LanguageName = "Turkish",
            LanguageNativeName = "Türkçe",
            IsPrimary = true,
            IsActive = true
        };
        return Task.FromResult(preference);
    }

    public Task<UserLanguagePreferenceDto> SetUserLanguagePreferenceAsync(Guid userId, SetUserLanguageRequest request)
    {
        var preference = new UserLanguagePreferenceDto
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LanguageId = Guid.NewGuid(),
            LanguageCode = request.LanguageCode,
            LanguageName = request.LanguageCode.ToString(),
            LanguageNativeName = request.LanguageCode.GetNativeName(),
            IsPrimary = request.IsPrimary,
            IsActive = true
        };
        return Task.FromResult(preference);
    }

    public Task<List<UserLanguagePreferenceDto>> GetUserLanguagePreferencesAsync(Guid userId)
    {
        var preferences = new List<UserLanguagePreferenceDto>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, LanguageId = Guid.NewGuid(), LanguageCode = LanguageCode.Turkish, LanguageName = "Turkish", LanguageNativeName = "Türkçe", IsPrimary = true, IsActive = true },
            new() { Id = Guid.NewGuid(), UserId = userId, LanguageId = Guid.NewGuid(), LanguageCode = LanguageCode.English, LanguageName = "English", LanguageNativeName = "English", IsPrimary = false, IsActive = true }
        };
        return Task.FromResult(preferences);
    }

    public Task<bool> RemoveUserLanguagePreferenceAsync(Guid userId, LanguageCode languageCode)
    {
        return Task.FromResult(true);
    }

    public Task<LanguageCode> GetUserPreferredLanguageAsync(Guid userId)
    {
        return Task.FromResult(LanguageCode.Turkish);
    }

    public Task<bool> SetUserPreferredLanguageAsync(Guid userId, LanguageCode languageCode)
    {
        return Task.FromResult(true);
    }
}
