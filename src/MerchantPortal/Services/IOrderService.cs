using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IOrderService
{
    Task<PagedResult<OrderResponse>?> GetOrdersAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default);
    Task<OrderDetailsResponse?> GetOrderDetailsAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task<PagedResult<OrderResponse>?> GetPendingOrdersAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}

