using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

/// <summary>
/// Kullanıcı dil tercihi servisi: kullanıcıların dil tercihlerini yönetir.
/// </summary>
public class UserLanguageService : IUserLanguageService
{
    /// <summary>Kullanıcının dil tercihini getirir (mock).</summary>
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

    /// <summary>Kullanıcının dil tercihini ayarlar (mock).</summary>
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

    /// <summary>Kullanıcının tüm dil tercihlerini getirir (mock).</summary>
    public Task<List<UserLanguagePreferenceDto>> GetUserLanguagePreferencesAsync(Guid userId)
    {
        var preferences = new List<UserLanguagePreferenceDto>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, LanguageId = Guid.NewGuid(), LanguageCode = LanguageCode.Turkish, LanguageName = "Turkish", LanguageNativeName = "Türkçe", IsPrimary = true, IsActive = true },
            new() { Id = Guid.NewGuid(), UserId = userId, LanguageId = Guid.NewGuid(), LanguageCode = LanguageCode.English, LanguageName = "English", LanguageNativeName = "English", IsPrimary = false, IsActive = true }
        };
        return Task.FromResult(preferences);
    }

    /// <summary>Kullanıcının dil tercihini kaldırır (mock).</summary>
    public Task<bool> RemoveUserLanguagePreferenceAsync(Guid userId, LanguageCode languageCode)
    {
        return Task.FromResult(true);
    }

    /// <summary>Kullanıcının tercih ettiği dili getirir (mock).</summary>
    public Task<LanguageCode> GetUserPreferredLanguageAsync(Guid userId)
    {
        return Task.FromResult(LanguageCode.Turkish);
    }

    /// <summary>Kullanıcının tercih ettiği dili ayarlar (mock).</summary>
    public Task<bool> SetUserPreferredLanguageAsync(Guid userId, LanguageCode languageCode)
    {
        return Task.FromResult(true);
    }
}
