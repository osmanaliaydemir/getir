using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Orders;

/// <summary>
/// Sipariş servisi: sipariş oluşturma, durum yönetimi, merchant/kullanıcı işlemleri, SignalR entegrasyonu.
/// </summary>
public interface IOrderService
{
    /// <summary>Yeni sipariş oluşturur (stok kontrolü, ödeme, SignalR bildirimi).</summary>
    Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    /// <summary>Siparişi ID ile getirir (kullanıcı kontrolü).</summary>
    Task<Result<OrderResponse>> GetOrderByIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcı siparişlerini sayfalama ile getirir.</summary>
    Task<Result<PagedResult<OrderResponse>>> GetUserOrdersAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    // Merchant-specific methods
    /// <summary>Merchant siparişlerini getirir (ownership kontrolü, durum filtresi).</summary>
    Task<Result<PagedResult<OrderResponse>>> GetMerchantOrdersAsync(Guid merchantOwnerId, PaginationQuery query, string? status = null, CancellationToken cancellationToken = default);
    /// <summary>Siparişi kabul eder (ownership kontrolü, SignalR).</summary>
    Task<Result<OrderResponse>> AcceptOrderAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Siparişi reddeder (ownership kontrolü, SignalR).</summary>
    Task<Result> RejectOrderAsync(Guid orderId, Guid merchantOwnerId, string? reason = null, CancellationToken cancellationToken = default);
    /// <summary>Sipariş hazırlanmaya başlar (ownership kontrolü, SignalR).</summary>
    Task<Result> StartPreparingOrderAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Siparişi hazır olarak işaretler (ownership kontrolü, SignalR).</summary>
    Task<Result> MarkOrderAsReadyAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Siparişi iptal eder (ownership kontrolü, SignalR).</summary>
    Task<Result> CancelOrderAsync(Guid orderId, Guid merchantOwnerId, string reason, CancellationToken cancellationToken = default);
    /// <summary>Sipariş istatistiklerini getirir (ownership kontrolü).</summary>
    Task<Result<OrderStatisticsResponse>> GetOrderStatisticsAsync(Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Additional merchant order methods
    /// <summary>Merchant için sipariş detaylarını getirir (timeline dahil).</summary>
    Task<Result<OrderDetailsResponse>> GetMerchantOrderDetailsAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Sipariş durumunu günceller (ownership kontrolü).</summary>
    Task<Result> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>Sipariş analizlerini getirir.</summary>
    Task<Result<OrderAnalyticsResponse>> GetOrderAnalyticsAsync(Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    /// <summary>Bekleyen siparişleri getirir.</summary>
    Task<Result<PagedResult<OrderResponse>>> GetPendingOrdersAsync(Guid merchantOwnerId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Sipariş zaman çizelgesini getirir.</summary>
    Task<Result<OrderTimelineResponse>> GetOrderTimelineAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    // SignalR Hub methods
    /// <summary>Siparişi ID ile getirir.</summary>
    Task<Result<OrderResponse>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcının aktif siparişlerini getirir.</summary>
    Task<Result<List<OrderResponse>>> GetUserActiveOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Sipariş durumunu günceller (SignalR).</summary>
    Task<Result<OrderResponse>> UpdateOrderStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    /// <summary>Siparişi iptal eder (SignalR).</summary>
    Task<Result<OrderResponse>> CancelOrderAsync(CancelOrderRequest request, CancellationToken cancellationToken = default);
    /// <summary>Siparişe puan verir (review oluşturur).</summary>
    Task<Result<OrderResponse>> RateOrderAsync(RateOrderRequest request, CancellationToken cancellationToken = default);
    /// <summary>Merchant'ın bekleyen siparişlerini getirir.</summary>
    Task<Result<List<OrderResponse>>> GetMerchantPendingOrdersAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
