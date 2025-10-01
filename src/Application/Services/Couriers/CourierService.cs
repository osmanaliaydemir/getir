using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Couriers;

public class CourierService : ICourierService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<CourierOrderResponse>>> GetAssignedOrdersAsync(
        Guid courierId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<Order>().GetPagedAsync(
            filter: o => o.CourierId == courierId && (o.Status == "Ready" || o.Status == "OnTheWay"),
            orderBy: o => o.CreatedAt,
            ascending: false,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.CourierId == courierId, cancellationToken);

        var response = orders.Select(o => new CourierOrderResponse(
            o.Id,
            o.OrderNumber,
            o.Status,
            o.DeliveryAddress,
            o.DeliveryLatitude,
            o.DeliveryLongitude,
            o.Total,
            o.EstimatedDeliveryTime
        )).ToList();

        var pagedResult = PagedResult<CourierOrderResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }

    public async Task<Result> UpdateLocationAsync(
        Guid courierId,
        CourierLocationUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.Repository<Courier>()
            .GetByIdAsync(courierId, cancellationToken);

        if (courier == null)
        {
            return Result.Fail("Courier not found", "NOT_FOUND_COURIER");
        }

        courier.CurrentLatitude = request.Latitude;
        courier.CurrentLongitude = request.Longitude;
        courier.LastLocationUpdate = DateTime.UtcNow;

        _unitOfWork.Repository<Courier>().Update(courier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> SetAvailabilityAsync(
        Guid courierId,
        SetAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var courier = await _unitOfWork.Repository<Courier>()
            .GetByIdAsync(courierId, cancellationToken);

        if (courier == null)
        {
            return Result.Fail("Courier not found", "NOT_FOUND_COURIER");
        }

        courier.IsAvailable = request.IsAvailable;

        _unitOfWork.Repository<Courier>().Update(courier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
