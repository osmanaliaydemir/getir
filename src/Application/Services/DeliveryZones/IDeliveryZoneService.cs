using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.DeliveryZones;

/// <summary>
/// Teslimat bölgesi servisi: merchant'a ait teslimat alanlarının yönetimi ve nokta-poligon kontrolü.
/// </summary>
public interface IDeliveryZoneService
{
    /// <summary>Merchant'a ait aktif teslimat bölgelerini getirir.</summary>
    Task<Result<List<DeliveryZoneResponse>>> GetDeliveryZonesByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Teslimat bölgesini ID ile getirir.</summary>
    Task<Result<DeliveryZoneResponse>> GetDeliveryZoneByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>Yeni teslimat bölgesi oluşturur (min 3 nokta gerekli).</summary>
    Task<Result<DeliveryZoneResponse>> CreateDeliveryZoneAsync(CreateDeliveryZoneRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Teslimat bölgesini günceller (noktalar dahil).</summary>
    Task<Result<DeliveryZoneResponse>> UpdateDeliveryZoneAsync(Guid id, UpdateDeliveryZoneRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Teslimat bölgesini siler.</summary>
    Task<Result> DeleteDeliveryZoneAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Verilen koordinatın teslimat bölgesi içinde olup olmadığını kontrol eder.</summary>
    Task<Result<CheckDeliveryZoneResponse>> CheckDeliveryZoneAsync(Guid merchantId, CheckDeliveryZoneRequest request, CancellationToken cancellationToken = default);
}
