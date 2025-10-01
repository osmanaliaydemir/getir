using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Couriers;

public interface ICourierService
{
    Task<Result<PagedResult<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result> UpdateLocationAsync(Guid courierId, CourierLocationUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Result> SetAvailabilityAsync(Guid courierId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
}
