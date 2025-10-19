using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Orders;

public interface IOrderService
{
    Task<Result<OrderResponse>> CreateOrderAsync(Guid userId, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> GetOrderByIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderResponse>>> GetUserOrdersAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    // Merchant-specific methods
    Task<Result<PagedResult<OrderResponse>>> GetMerchantOrdersAsync(Guid merchantOwnerId, PaginationQuery query, string? status = null, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> AcceptOrderAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> RejectOrderAsync(Guid orderId, Guid merchantOwnerId, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result> StartPreparingOrderAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> MarkOrderAsReadyAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> CancelOrderAsync(Guid orderId, Guid merchantOwnerId, string reason, CancellationToken cancellationToken = default);
    Task<Result<OrderStatisticsResponse>> GetOrderStatisticsAsync(Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    // Additional merchant order methods
    Task<Result<OrderDetailsResponse>> GetMerchantOrderDetailsAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<OrderAnalyticsResponse>> GetOrderAnalyticsAsync(Guid merchantOwnerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderResponse>>> GetPendingOrdersAsync(Guid merchantOwnerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<OrderTimelineResponse>> GetOrderTimelineAsync(Guid orderId, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    // SignalR Hub methods
    Task<Result<OrderResponse>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<List<OrderResponse>>> GetUserActiveOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> UpdateOrderStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> CancelOrderAsync(CancelOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrderResponse>> RateOrderAsync(RateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<OrderResponse>>> GetMerchantPendingOrdersAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
