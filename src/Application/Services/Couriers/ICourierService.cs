using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Couriers;

public interface ICourierService
{
    // Existing methods
    Task<Result<PagedResult<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result> UpdateLocationAsync(Guid courierId, CourierLocationUpdateRequest request, CancellationToken cancellationToken = default);
    Task<Result> SetAvailabilityAsync(Guid courierId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
    
    // Courier Panel methods
    Task<Result<CourierDashboardResponse>> GetCourierDashboardAsync(Guid courierId, CancellationToken cancellationToken = default);
    Task<Result<CourierStatsResponse>> GetCourierStatsAsync(Guid courierId, CancellationToken cancellationToken = default);
    Task<Result<CourierEarningsResponse>> GetCourierEarningsAsync(Guid courierId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Result> AcceptOrderAsync(Guid courierId, AcceptOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result> StartDeliveryAsync(Guid courierId, StartDeliveryRequest request, CancellationToken cancellationToken = default);
    Task<Result> CompleteDeliveryAsync(Guid courierId, CompleteDeliveryRequest request, CancellationToken cancellationToken = default);
    
    // Order Assignment methods
    Task<Result<CourierAssignmentResponse>> AssignOrderAsync(AssignOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<FindNearestCouriersResponse>> FindNearestCouriersAsync(FindNearestCouriersRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<CourierPerformanceResponse>>> GetTopPerformersAsync(int count = 10, CancellationToken cancellationToken = default);
    
    // Earnings methods
    Task<Result<CourierEarningsDetailResponse>> GetEarningsDetailAsync(CourierEarningsQuery query, CancellationToken cancellationToken = default);
    
    // SignalR Hub methods
    Task<Result> UpdateLocationAsync(CourierLocationUpdateWithOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<CourierLocationResponse>> GetCurrentLocationAsync(Guid courierId, CancellationToken cancellationToken = default);
    Task<Result<List<CourierLocationHistoryItem>>> GetLocationHistoryAsync(Guid courierId, Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<List<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, CancellationToken cancellationToken = default);
    Task<Result> UpdateAvailabilityAsync(Guid courierId, Domain.Enums.CourierAvailabilityStatus status, CancellationToken cancellationToken = default);
}
