using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Couriers;

/// <summary>
/// Kurye servisi interface'i: sipariş atama, konum/müsaitlik yönetimi, dashboard, kazanç, performans ve gerçek zamanlı takip işlemleri.
/// </summary>
public interface ICourierService
{
    // Existing methods
    /// <summary>Kuryeye atanan siparişleri sayfalı getirir.</summary>
    Task<Result<PagedResult<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Kurye konumunu günceller ve SignalR ile bildirim gönderir.</summary>
    Task<Result> UpdateLocationAsync(Guid courierId, CourierLocationUpdateRequest request, CancellationToken cancellationToken = default);
    /// <summary>Kurye müsaitlik durumunu ayarlar.</summary>
    Task<Result> SetAvailabilityAsync(Guid courierId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
    
    // Courier Panel methods
    /// <summary>Kurye dashboard verilerini getirir.</summary>
    Task<Result<CourierDashboardResponse>> GetCourierDashboardAsync(Guid courierId, CancellationToken cancellationToken = default);
    /// <summary>Kurye istatistiklerini getirir.</summary>
    Task<Result<CourierStatsResponse>> GetCourierStatsAsync(Guid courierId, CancellationToken cancellationToken = default);
    /// <summary>Kurye kazançlarını getirir.</summary>
    Task<Result<CourierEarningsResponse>> GetCourierEarningsAsync(Guid courierId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    /// <summary>Sipariş kabul eder.</summary>
    Task<Result> AcceptOrderAsync(Guid courierId, AcceptOrderRequest request, CancellationToken cancellationToken = default);
    /// <summary>Teslimatı başlatır.</summary>
    Task<Result> StartDeliveryAsync(Guid courierId, StartDeliveryRequest request, CancellationToken cancellationToken = default);
    /// <summary>Teslimatı tamamlar.</summary>
    Task<Result> CompleteDeliveryAsync(Guid courierId, CompleteDeliveryRequest request, CancellationToken cancellationToken = default);
    
    // Order Assignment methods
    /// <summary>Siparişi kuryeye atar.</summary>
    Task<Result<CourierAssignmentResponse>> AssignOrderAsync(AssignOrderRequest request, CancellationToken cancellationToken = default);
    /// <summary>En yakın kuryeleri bulur.</summary>
    Task<Result<FindNearestCouriersResponse>> FindNearestCouriersAsync(FindNearestCouriersRequest request, CancellationToken cancellationToken = default);
    /// <summary>En iyi performans gösteren kuryeleri getirir.</summary>
    Task<Result<List<CourierPerformanceResponse>>> GetTopPerformersAsync(int count = 10, CancellationToken cancellationToken = default);
    
    // Earnings methods
    /// <summary>Kurye kazanç detaylarını getirir.</summary>
    Task<Result<CourierEarningsDetailResponse>> GetEarningsDetailAsync(CourierEarningsQuery query, CancellationToken cancellationToken = default);
    
    // SignalR Hub methods
    /// <summary>Konum günceller (sipariş ile).</summary>
    Task<Result> UpdateLocationAsync(CourierLocationUpdateWithOrderRequest request, CancellationToken cancellationToken = default);
    /// <summary>Güncel konumu getirir.</summary>
    Task<Result<CourierLocationResponse>> GetCurrentLocationAsync(Guid courierId, CancellationToken cancellationToken = default);
    /// <summary>Konum geçmişini getirir.</summary>
    Task<Result<List<CourierLocationHistoryItem>>> GetLocationHistoryAsync(Guid courierId, Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Atanan siparişleri getirir.</summary>
    Task<Result<List<CourierOrderResponse>>> GetAssignedOrdersAsync(Guid courierId, CancellationToken cancellationToken = default);
    /// <summary>Müsaitlik durumunu günceller.</summary>
    Task<Result> UpdateAvailabilityAsync(Guid courierId, Domain.Enums.CourierAvailabilityStatus status, CancellationToken cancellationToken = default);
    
    // Additional methods for testing and admin panel
    /// <summary>Kurye bilgisini getirir.</summary>
    Task<Result<CourierResponse>> GetCourierByIdAsync(Guid courierId, CancellationToken cancellationToken = default);
    /// <summary>Müsaitlik durumuna göre kuryeleri getirir.</summary>
    Task<Result<PagedResult<CourierResponse>>> GetCouriersByAvailabilityAsync(bool isAvailable, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Siparişe kurye atar.</summary>
    Task<Result> AssignCourierToOrderAsync(Guid orderId, Guid courierId, CancellationToken cancellationToken = default);
}
