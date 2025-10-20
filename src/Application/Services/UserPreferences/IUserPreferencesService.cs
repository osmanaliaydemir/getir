using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.UserPreferences;

/// <summary>
/// Kullanıcı tercihleri servisi: bildirim tercihleri (email/sms/push), sessiz saatler, merchant portal ayarları.
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>Kullanıcı bildirim tercihlerini getirir (yoksa hata).</summary>
    Task<Result<UserNotificationPreferencesResponse>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı tercihlerini getirir veya yoksa varsayılan oluşturur (her zaman döner).</summary>
    Task<Result<UserNotificationPreferencesResponse>> GetOrCreateUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı bildirim tercihlerini günceller (partial update).</summary>
    Task<Result<UserNotificationPreferencesResponse>> UpdateUserPreferencesAsync(Guid userId, UpdateUserNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
    /// <summary>Merchant portal için basitleştirilmiş tercihleri getirir.</summary>
    Task<Result<MerchantNotificationPreferencesResponse>> GetMerchantPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Merchant portal için basitleştirilmiş tercihleri günceller.</summary>
    Task<Result<MerchantNotificationPreferencesResponse>> UpdateMerchantPreferencesAsync(Guid userId, UpdateMerchantNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı tercihlerini siler.</summary>
    Task<Result> DeleteUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

