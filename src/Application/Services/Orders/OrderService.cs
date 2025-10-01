using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderResponse>> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Transaction başlat
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Merchant kontrolü
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.IsActive, cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<OrderResponse>("Merchant not found or inactive", "NOT_FOUND_MERCHANT");
            }

            // Ürünleri kontrol et ve fiyatları hesapla
            decimal subTotal = 0;
            var orderLines = new List<OrderLine>();

            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.ReadRepository<Product>()
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive, cancellationToken: cancellationToken);

                if (product == null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result.Fail<OrderResponse>($"Product {item.ProductId} not found", "NOT_FOUND_PRODUCT");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result.Fail<OrderResponse>($"Insufficient stock for {product.Name}", "INSUFFICIENT_STOCK");
                }

                var unitPrice = product.DiscountedPrice ?? product.Price;
                var totalPrice = unitPrice * item.Quantity;
                subTotal += totalPrice;

                orderLines.Add(new OrderLine
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Notes = item.Notes
                });

                // Stok güncelle
                var productToUpdate = await _unitOfWork.Repository<Product>()
                    .GetByIdAsync(product.Id, cancellationToken);
                
                if (productToUpdate != null)
                {
                    productToUpdate.StockQuantity -= item.Quantity;
                    _unitOfWork.Repository<Product>().Update(productToUpdate);
                }
            }

            // Minimum sipariş tutarı kontrolü
            if (subTotal < merchant.MinimumOrderAmount)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result.Fail<OrderResponse>(
                    $"Minimum order amount is {merchant.MinimumOrderAmount:C}",
                    "BELOW_MINIMUM_ORDER");
            }

            var total = subTotal + merchant.DeliveryFee;

            // Order oluştur
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                MerchantId = request.MerchantId,
                Status = "Pending",
                SubTotal = subTotal,
                DeliveryFee = merchant.DeliveryFee,
                Discount = 0,
                Total = total,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Pending",
                DeliveryAddress = request.DeliveryAddress,
                DeliveryLatitude = request.DeliveryLatitude,
                DeliveryLongitude = request.DeliveryLongitude,
                EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(merchant.AverageDeliveryTime),
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);

            // OrderLines ekle
            foreach (var line in orderLines)
            {
                line.OrderId = order.Id;
            }
            await _unitOfWork.Repository<OrderLine>().AddRangeAsync(orderLines, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Response oluştur
            var response = new OrderResponse(
                order.Id,
                order.OrderNumber,
                merchant.Id,
                merchant.Name,
                order.Status,
                order.SubTotal,
                order.DeliveryFee,
                order.Discount,
                order.Total,
                order.PaymentMethod,
                order.PaymentStatus,
                order.DeliveryAddress,
                order.EstimatedDeliveryTime,
                order.CreatedAt,
                orderLines.Select(ol => new OrderLineResponse(
                    ol.Id,
                    ol.ProductId,
                    ol.ProductName,
                    ol.Quantity,
                    ol.UnitPrice,
                    ol.TotalPrice
                )).ToList()
            );

            return Result.Ok(response);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<OrderResponse>> GetOrderByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<Order>()
            .GetAsync(
                o => o.Id == orderId && o.UserId == userId,
                include: "Merchant,OrderLines",
                cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail<OrderResponse>("Order not found", "NOT_FOUND_ORDER");
        }

        var response = new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.MerchantId,
            order.Merchant.Name,
            order.Status,
            order.SubTotal,
            order.DeliveryFee,
            order.Discount,
            order.Total,
            order.PaymentMethod,
            order.PaymentStatus,
            order.DeliveryAddress,
            order.EstimatedDeliveryTime,
            order.CreatedAt,
            order.OrderLines.Select(ol => new OrderLineResponse(
                ol.Id,
                ol.ProductId,
                ol.ProductName,
                ol.Quantity,
                ol.UnitPrice,
                ol.TotalPrice
            )).ToList()
        );

        return Result.Ok(response);
    }

    public async Task<Result<PagedResult<OrderResponse>>> GetUserOrdersAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<Order>().GetPagedAsync(
            filter: o => o.UserId == userId,
            orderBy: o => o.CreatedAt,
            ascending: query.IsAscending,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Merchant,OrderLines",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(o => o.UserId == userId, cancellationToken);

        var response = orders.Select(o => new OrderResponse(
            o.Id,
            o.OrderNumber,
            o.MerchantId,
            o.Merchant.Name,
            o.Status,
            o.SubTotal,
            o.DeliveryFee,
            o.Discount,
            o.Total,
            o.PaymentMethod,
            o.PaymentStatus,
            o.DeliveryAddress,
            o.EstimatedDeliveryTime,
            o.CreatedAt,
            o.OrderLines.Select(ol => new OrderLineResponse(
                ol.Id,
                ol.ProductId,
                ol.ProductName,
                ol.Quantity,
                ol.UnitPrice,
                ol.TotalPrice
            )).ToList()
        )).ToList();

        var pagedResult = PagedResult<OrderResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
