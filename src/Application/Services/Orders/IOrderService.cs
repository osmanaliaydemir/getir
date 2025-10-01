using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Orders;

public interface IOrderService
{
    Task<Result<OrderResponse>> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);
    
    Task<Result<OrderResponse>> GetOrderByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default);
    
    Task<Result<PagedResult<OrderResponse>>> GetUserOrdersAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);
}
