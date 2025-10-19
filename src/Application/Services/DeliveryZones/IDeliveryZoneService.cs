using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.DeliveryZones;

public interface IDeliveryZoneService
{
    Task<Result<List<DeliveryZoneResponse>>> GetDeliveryZonesByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<Result<DeliveryZoneResponse>> GetDeliveryZoneByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DeliveryZoneResponse>> CreateDeliveryZoneAsync(CreateDeliveryZoneRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<DeliveryZoneResponse>> UpdateDeliveryZoneAsync(Guid id, UpdateDeliveryZoneRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> DeleteDeliveryZoneAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<CheckDeliveryZoneResponse>> CheckDeliveryZoneAsync(Guid merchantId, CheckDeliveryZoneRequest request, CancellationToken cancellationToken = default);
}
