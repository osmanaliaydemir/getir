using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.DeliveryOptimization;

/// <summary>
/// Teslimat kapasitesi yönetimi servisi
/// </summary>
public interface IDeliveryCapacityService
{
    /// <summary>
    /// Teslimat kapasitesi ayarlarını oluşturur
    /// </summary>
    Task<Result<DeliveryCapacityResponse>> CreateCapacityAsync(DeliveryCapacityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Teslimat kapasitesi ayarlarını günceller
    /// </summary>
    Task<Result<DeliveryCapacityResponse>> UpdateCapacityAsync(Guid capacityId, DeliveryCapacityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Merchant'ın teslimat kapasitesini kontrol eder
    /// </summary>
    Task<Result<DeliveryCapacityCheckResponse>> CheckCapacityAsync(DeliveryCapacityCheckRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Aktif teslimat sayısını artırır
    /// </summary>
    Task<Result> IncrementActiveDeliveriesAsync(Guid merchantId, Guid? deliveryZoneId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Aktif teslimat sayısını azaltır
    /// </summary>
    Task<Result> DecrementActiveDeliveriesAsync(Guid merchantId, Guid? deliveryZoneId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Günlük teslimat sayısını artırır
    /// </summary>
    Task<Result> IncrementDailyDeliveriesAsync(Guid merchantId, Guid? deliveryZoneId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Kapasite istatistiklerini getirir
    /// </summary>
    Task<Result<DeliveryCapacityResponse>> GetCapacityAsync(Guid merchantId, Guid? deliveryZoneId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Dinamik kapasite ayarı yapar
    /// </summary>
    Task<Result> AdjustCapacityAsync(DynamicCapacityAdjustmentRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Kapasite uyarıları gönderir
    /// </summary>
    Task<Result> SendCapacityAlertAsync(CapacityAlertRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Günlük kapasite sayaçlarını sıfırlar
    /// </summary>
    Task<Result> ResetDailyCountersAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Haftalık kapasite sayaçlarını sıfırlar
    /// </summary>
    Task<Result> ResetWeeklyCountersAsync(CancellationToken cancellationToken = default);
}
