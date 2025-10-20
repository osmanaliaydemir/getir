using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Notifications;

/// <summary>
/// Bildirim tercihleri servisi: kullanıcıların bildirim kanalları ve sessiz saatler tercihleri.
/// </summary>
public interface INotificationPreferencesService
{
    /// <summary>Kullanıcı bildirim tercihlerini getirir (yoksa varsayılan oluşturur).</summary>
    Task<Result<NotificationPreferencesResponse>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı bildirim tercihlerini günceller.</summary>
    Task<Result> UpdateUserPreferencesAsync(Guid userId, UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı bildirim tercihlerini varsayılana sıfırlar.</summary>
    Task<Result> ResetToDefaultsAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Yeni kullanıcı için varsayılan bildirim tercihleri oluşturur.</summary>
    Task<Result> CreateDefaultPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Bildirim tercihleri özetini getirir.</summary>
    Task<Result<NotificationPreferencesSummary>> GetPreferencesSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcının belirtilen tip ve kanal için bildirim alıp alamayacağını kontrol eder.</summary>
    Task<Result<bool>> CanReceiveNotificationAsync(Guid userId, NotificationType type, NotificationChannel channel, CancellationToken cancellationToken = default);
    /// <summary>Geçerli zamanın sessiz saatler içinde olup olmadığını kontrol eder.</summary>
    Task<Result<bool>> IsWithinQuietHoursAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Toplu bildirim tercihi günceller.</summary>
    Task<Result> BulkUpdatePreferencesAsync(BulkNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
    /// <summary>Belirtilen bildirim tipini alabilen kullanıcıları getirir.</summary>
    Task<Result<IEnumerable<Guid>>> GetEligibleUsersAsync(NotificationType type, NotificationChannel channel, CancellationToken cancellationToken = default);
}
