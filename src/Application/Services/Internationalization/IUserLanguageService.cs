using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

/// <summary>
/// Kullanıcı dil tercihi servisi: kullanıcıların dil tercihlerini yönetir.
/// </summary>
public interface IUserLanguageService
{
    /// <summary>Kullanıcının dil tercihini getirir.</summary>
    Task<UserLanguagePreferenceDto?> GetUserLanguagePreferenceAsync(Guid userId);
    /// <summary>Kullanıcının dil tercihini ayarlar.</summary>
    Task<UserLanguagePreferenceDto> SetUserLanguagePreferenceAsync(Guid userId, SetUserLanguageRequest request);
    /// <summary>Kullanıcının tüm dil tercihlerini getirir.</summary>
    Task<List<UserLanguagePreferenceDto>> GetUserLanguagePreferencesAsync(Guid userId);
    /// <summary>Kullanıcının dil tercihini kaldırır.</summary>
    Task<bool> RemoveUserLanguagePreferenceAsync(Guid userId, LanguageCode languageCode);
    /// <summary>Kullanıcının tercih ettiği dili getirir.</summary>
    Task<LanguageCode> GetUserPreferredLanguageAsync(Guid userId);
    /// <summary>Kullanıcının tercih ettiği dili ayarlar.</summary>
    Task<bool> SetUserPreferredLanguageAsync(Guid userId, LanguageCode languageCode);
}
