using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.Application.Services.Payments;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Orders;

public class OrderService : BaseService, IOrderService
{
    private readonly ISignalRService? _signalRService;
    private readonly ISignalROrderSender? _signalROrderSender;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IPaymentService _paymentService;

    public OrderService(
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IBackgroundTaskService backgroundTaskService,
        IPaymentService paymentService,
        ISignalRService? signalRService = null,
        ISignalROrderSender? signalROrderSender = null) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
        _signalROrderSender = signalROrderSender;
        _backgroundTaskService = backgroundTaskService;
        _paymentService = paymentService;
    }

    public async Task<Result<OrderResponse>> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CreateOrderInternalAsync(userId, request, cancellationToken),
            "CreateOrder",
            new { userId, request.MerchantId, ItemCount = request.Items.Count },
            cancellationToken);
    }

    private async Task<Result<OrderResponse>> CreateOrderInternalAsync(
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
                await _unitOfWork.RollbackAsync(cancellationToken);
                return ServiceResult.Failure<OrderResponse>("Merchant not found or inactive", ErrorCodes.MERCHANT_NOT_FOUND);
            }

            // User kontrolü
            var user = await _unitOfWork.ReadRepository<User>()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

            if (user == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return ServiceResult.Failure<OrderResponse>("User not found", ErrorCodes.UNAUTHORIZED);
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
                    return ServiceResult.Failure<OrderResponse>($"Product {item.ProductId} not found", ErrorCodes.PRODUCT_NOT_FOUND);
                }

                if (product.StockQuantity < item.Quantity)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return ServiceResult.Failure<OrderResponse>($"Insufficient stock for {product.Name}", ErrorCodes.INSUFFICIENT_STOCK);
                }

                // Check if this is a market product with variant
                decimal unitPrice;
                string? variantName = null;
                
                if (item.ProductVariantId.HasValue)
                {
                    // Get variant price for market products
                    var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                        .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId.Value && v.ProductId == item.ProductId,
                            cancellationToken: cancellationToken);
                    
                    if (variant == null)
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        return ServiceResult.Failure<OrderResponse>($"Product variant not found", ErrorCodes.PRODUCT_VARIANT_NOT_FOUND);
                    }
                    
                    unitPrice = variant.DiscountedPrice ?? variant.Price;
                    variantName = variant.Name;
                    
                    // Check variant stock
                    if (variant.StockQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                        return ServiceResult.Failure<OrderResponse>($"Insufficient stock for {variant.Name}", ErrorCodes.INSUFFICIENT_STOCK);
                    }
                }
                else
                {
                    // Use product base price
                    unitPrice = product.DiscountedPrice ?? product.Price;
                }
                
                var optionsTotal = 0m;
                var orderLineOptions = new List<OrderLineOption>();

                // Calculate options total
                if (item.Options != null && item.Options.Any())
                {
                    var productOptions = await _unitOfWork.ReadRepository<ProductOption>()
                        .ListAsync(po => item.Options.Select(opt => opt.ProductOptionId).Contains(po.Id),
                            cancellationToken: cancellationToken);

                    foreach (var optionRequest in item.Options)
                    {
                        var productOption = productOptions.FirstOrDefault(po => po.Id == optionRequest.ProductOptionId);
                        if (productOption != null)
                        {
                            optionsTotal += productOption.ExtraPrice;
                            orderLineOptions.Add(new OrderLineOption
                            {
                                Id = Guid.NewGuid(),
                                ProductOptionId = productOption.Id,
                                OptionName = productOption.Name,
                                ExtraPrice = productOption.ExtraPrice,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                var totalPrice = (unitPrice + optionsTotal) * item.Quantity;
                subTotal += totalPrice;

                var orderLine = new OrderLine
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = product.Name,
                    VariantName = variantName,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Notes = item.Notes
                };

                orderLines.Add(orderLine);

                // Add options to order line
                foreach (var option in orderLineOptions)
                {
                    option.OrderLineId = orderLine.Id;
                    await _unitOfWork.Repository<OrderLineOption>().AddAsync(option, cancellationToken);
                }

                // Stok güncelle
                if (item.ProductVariantId.HasValue)
                {
                    // Variant stok güncelle
                    var variantToUpdate = await _unitOfWork.Repository<MarketProductVariant>()
                        .GetByIdAsync(item.ProductVariantId.Value, cancellationToken);
                    
                    if (variantToUpdate != null)
                    {
                        variantToUpdate.StockQuantity -= item.Quantity;
                        variantToUpdate.IsAvailable = variantToUpdate.StockQuantity > 0;
                        variantToUpdate.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Repository<MarketProductVariant>().Update(variantToUpdate);
                    }
                }
                else
                {
                    // Product stok güncelle
                    var productToUpdate = await _unitOfWork.Repository<Product>()
                        .GetByIdAsync(product.Id, cancellationToken);
                    
                    if (productToUpdate != null)
                    {
                        productToUpdate.StockQuantity -= item.Quantity;
                        productToUpdate.IsAvailable = productToUpdate.StockQuantity > 0;
                        productToUpdate.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Repository<Product>().Update(productToUpdate);
                    }
                }
            }

            // Minimum sipariş tutarı kontrolü
            if (subTotal < merchant.MinimumOrderAmount)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return ServiceResult.Failure<OrderResponse>(
                    $"Minimum order amount is {merchant.MinimumOrderAmount:C}",
                    ErrorCodes.BELOW_MINIMUM_ORDER);
            }

            var total = subTotal + merchant.DeliveryFee;

            // Order oluştur
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                MerchantId = request.MerchantId,
                Status = OrderStatus.Pending,
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

            // Payment oluştur
            var paymentResult = await CreatePaymentForOrderAsync(order, cancellationToken);
            if (!paymentResult.Success)
            {
                // Payment oluşturulamadı, order'ı iptal et
                await CancelOrderDueToPaymentFailureAsync(order.Id, paymentResult.Error ?? "Unknown error", cancellationToken);
                return ServiceResult.Failure<OrderResponse>(paymentResult.Error ?? "Unknown error", paymentResult.ErrorCode ?? "PAYMENT_ERROR");
            }

            // Background task - Order oluşturuldu bildirimi
            await _backgroundTaskService.EnqueueTaskAsync(new OrderCreatedTask
            {
                OrderId = order.Id,
                UserId = userId,
                MerchantId = merchant.Id,
                OrderNumber = order.OrderNumber
            }, TaskPriority.High, cancellationToken);

            // Send real-time notification via SignalR
            if (_signalRService != null)
            {
                await _signalRService.SendOrderStatusUpdateAsync(
                    order.Id,
                    userId,
                    order.Status.ToStringValue(),
                    $"Your order {order.OrderNumber} has been created successfully!");

                await _signalRService.SendNotificationToUserAsync(
                    userId,
                    "Order Created",
                    $"Your order {order.OrderNumber} has been placed successfully. Estimated delivery: {order.EstimatedDeliveryTime:HH:mm}",
                    "Order");
            }

            // Send new order notification to merchant via SignalR
            if (_signalROrderSender != null)
            {
                await _signalROrderSender.SendNewOrderToMerchantAsync(
                    merchant.Id,
                    new
                    {
                        orderId = order.Id,
                        orderNumber = order.OrderNumber,
                        customerName = $"{user.FirstName} {user.LastName}",
                        totalAmount = order.Total,
                        createdAt = order.CreatedAt,
                        status = order.Status.ToStringValue()
                    });
            }

            // Log successful order creation
            _loggingService.LogUserAction(userId.ToString(), "OrderCreated", new { OrderId = order.Id, order.OrderNumber, MerchantId = merchant.Id });

            // Response oluştur
            var response = new OrderResponse(
                order.Id,
                order.OrderNumber,
                merchant.Id,
                merchant.Name,
                order.Status.ToStringValue(),
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
                    ol.ProductVariantId,
                    ol.ProductName,
                    ol.VariantName,
                    ol.Quantity,
                    ol.UnitPrice,
                    ol.TotalPrice,
                    ol.Options.Select(opt => new OrderLineOptionResponse(
                        opt.Id,
                        opt.ProductOptionId,
                        opt.OptionName,
                        opt.ExtraPrice,
                        opt.CreatedAt)).ToList()
                )).ToList()
            );

            return ServiceResult.Success(response);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _loggingService.LogError("Error creating order", ex, new { userId, request.MerchantId });
            return ServiceResult.HandleException<OrderResponse>(ex, _logger, "CreateOrder");
        }
    }

    public async Task<Result<OrderResponse>> GetOrderByIdAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetOrderByIdInternalAsync(orderId, userId, cancellationToken),
            "GetOrderById",
            new { OrderId = orderId, UserId = userId },
            cancellationToken);
    }

    private async Task<Result<OrderResponse>> GetOrderByIdInternalAsync(
        Guid orderId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _unitOfWork.Repository<Order>()
                .GetAsync(
                    o => o.Id == orderId && o.UserId == userId,
                    include: "Merchant,OrderLines",
                    cancellationToken: cancellationToken);

            if (order == null)
            {
                return ServiceResult.Failure<OrderResponse>("Order not found", ErrorCodes.ORDER_NOT_FOUND);
            }

            var response = new OrderResponse(
                order.Id,
                order.OrderNumber,
                order.MerchantId,
                order.Merchant.Name,
                order.Status.ToStringValue(),
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
                    ol.ProductVariantId,
                    ol.ProductName,
                    ol.VariantName,
                    ol.Quantity,
                    ol.UnitPrice,
                    ol.TotalPrice,
                    ol.Options.Select(opt => new OrderLineOptionResponse(
                        opt.Id,
                        opt.ProductOptionId,
                        opt.OptionName,
                        opt.ExtraPrice,
                        opt.CreatedAt)).ToList()
                )).ToList()
            );

            return ServiceResult.Success(response);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting order by id", ex, new { OrderId = orderId, UserId = userId });
            return ServiceResult.HandleException<OrderResponse>(ex, _logger, "GetOrderById");
        }
    }

    public async Task<Result<PagedResult<OrderResponse>>> GetUserOrdersAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetUserOrdersInternalAsync(userId, query, cancellationToken),
            "GetUserOrders",
            new { userId, query.Page, query.PageSize },
            cancellationToken);
    }

    private async Task<Result<PagedResult<OrderResponse>>> GetUserOrdersInternalAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        try
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
                o.Status.ToStringValue(),
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
                    ol.ProductVariantId,
                    ol.ProductName,
                    ol.VariantName,
                    ol.Quantity,
                    ol.UnitPrice,
                    ol.TotalPrice,
                    ol.Options.Select(opt => new OrderLineOptionResponse(
                        opt.Id,
                        opt.ProductOptionId,
                        opt.OptionName,
                        opt.ExtraPrice,
                        opt.CreatedAt)).ToList()
                )).ToList()
            )).ToList();

            var pagedResult = PagedResult<OrderResponse>.Create(response, total, query.Page, query.PageSize);
            
            return ServiceResult.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting user orders", ex, new { userId, query.Page, query.PageSize });
            return ServiceResult.HandleException<PagedResult<OrderResponse>>(ex, _logger, "GetUserOrders");
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    // Merchant-specific methods
    public async Task<Result<PagedResult<OrderResponse>>> GetMerchantOrdersAsync(
        Guid merchantOwnerId,
        PaginationQuery query,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get merchant owned by this user
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return ServiceResult.Failure<PagedResult<OrderResponse>>("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
        }

        Expression<Func<Order, bool>> filter = o => o.MerchantId == merchant.Id;
        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = OrderStatusExtensions.FromString(status);
            filter = o => o.MerchantId == merchant.Id && o.Status == statusEnum;
        }

        var orders = await _unitOfWork.Repository<Order>().GetPagedAsync(
            filter: filter,
            orderBy: o => o.CreatedAt,
            ascending: false,
            page: query.Page,
            pageSize: query.PageSize,
            include: "User,OrderLines",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Order>()
            .CountAsync(filter, cancellationToken);

        var responses = orders.Select(o => new OrderResponse(
            o.Id,
            o.OrderNumber,
            o.MerchantId,
            merchant.Name,
            o.Status.ToStringValue(),
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
                ol.ProductVariantId,
                ol.ProductName,
                ol.VariantName,
                ol.Quantity,
                ol.UnitPrice,
                ol.TotalPrice,
                ol.Options.Select(opt => new OrderLineOptionResponse(
                    opt.Id,
                    opt.ProductOptionId,
                    opt.OptionName,
                    opt.ExtraPrice,
                    opt.CreatedAt)).ToList())).ToList())).ToList();

        var pagedResult = PagedResult<OrderResponse>.Create(
            responses,
            total,
            query.Page,
            query.PageSize);

        return ServiceResult.Success(pagedResult);
    }

    public async Task<Result<OrderResponse>> AcceptOrderAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, 
                include: "Merchant,User,OrderLines", 
                cancellationToken: cancellationToken);

        if (order == null)
        {
            return ServiceResult.Failure<OrderResponse>("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Verify merchant ownership
        if (order.Merchant.OwnerId != merchantOwnerId)
        {
            return ServiceResult.Failure<OrderResponse>("Access denied", ErrorCodes.FORBIDDEN);
        }

        // Check if order can be accepted
        if (order.Status != OrderStatus.Pending)
        {
            return ServiceResult.Failure<OrderResponse>("Order cannot be accepted in current status", ErrorCodes.BAD_REQUEST);
        }

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(
                order.Id,
                order.UserId,
                order.Status.ToStringValue(),
                $"Your order {order.OrderNumber} has been confirmed by {order.Merchant.Name}!");
        }

        // Send status change notification to merchant
        if (_signalROrderSender != null)
        {
            await _signalROrderSender.SendOrderStatusChangedToMerchantAsync(
                order.MerchantId,
                order.Id,
                order.OrderNumber,
                order.Status.ToStringValue());
        }

        var response = new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.MerchantId,
            order.Merchant.Name,
            order.Status.ToStringValue(),
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
                    ol.ProductVariantId,
                    ol.ProductName,
                    ol.VariantName,
                    ol.Quantity,
                    ol.UnitPrice,
                    ol.TotalPrice,
                    ol.Options.Select(opt => new OrderLineOptionResponse(
                        opt.Id,
                        opt.ProductOptionId,
                        opt.OptionName,
                        opt.ExtraPrice,
                        opt.CreatedAt)).ToList())).ToList());

        return ServiceResult.Success(response);
    }

    public async Task<Result> RejectOrderAsync(
        Guid orderId,
        Guid merchantOwnerId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, 
                include: "Merchant,User", 
                cancellationToken: cancellationToken);

        if (order == null)
        {
            return ServiceResult.Failure("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Verify merchant ownership
        if (order.Merchant.OwnerId != merchantOwnerId)
        {
            return ServiceResult.Failure("Access denied", ErrorCodes.FORBIDDEN);
        }

        // Check if order can be rejected
        if (order.Status != OrderStatus.Pending)
        {
            return ServiceResult.Failure("Order cannot be rejected in current status", ErrorCodes.BAD_REQUEST);
        }

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason ?? "Order rejected by merchant";
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(
                order.Id,
                order.UserId,
                order.Status.ToStringValue(),
                $"Your order {order.OrderNumber} has been rejected by {order.Merchant.Name}. Reason: {order.CancellationReason}");
        }

        // Send cancellation notification to merchant
        if (_signalROrderSender != null)
        {
            await _signalROrderSender.SendOrderCancelledToMerchantAsync(
                order.MerchantId,
                order.Id,
                order.OrderNumber,
                order.CancellationReason);
        }

        return ServiceResult.Success();
    }

    public async Task<Result> StartPreparingOrderAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, 
                include: "Merchant,User", 
                cancellationToken: cancellationToken);

        if (order == null)
        {
            return ServiceResult.Failure("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Verify merchant ownership
        if (order.Merchant.OwnerId != merchantOwnerId)
        {
            return ServiceResult.Failure("Access denied", ErrorCodes.FORBIDDEN);
        }

        // Check if order can start preparing
        if (order.Status != OrderStatus.Confirmed)
        {
            return ServiceResult.Failure("Order cannot start preparing in current status", ErrorCodes.BAD_REQUEST);
        }

        order.Status = OrderStatus.Preparing;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(
                order.Id,
                order.UserId,
                order.Status.ToStringValue(),
                $"Your order {order.OrderNumber} is now being prepared by {order.Merchant.Name}!");
        }

        // Send status change notification to merchant
        if (_signalROrderSender != null)
        {
            await _signalROrderSender.SendOrderStatusChangedToMerchantAsync(
                order.MerchantId,
                order.Id,
                order.OrderNumber,
                order.Status.ToStringValue());
        }

        return ServiceResult.Success();
    }

    public async Task<Result> MarkOrderAsReadyAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, 
                include: "Merchant,User", 
                cancellationToken: cancellationToken);

        if (order == null)
        {
            return ServiceResult.Failure("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Verify merchant ownership
        if (order.Merchant.OwnerId != merchantOwnerId)
        {
            return ServiceResult.Failure("Access denied", ErrorCodes.FORBIDDEN);
        }

        // Check if order can be marked as ready
        if (order.Status != OrderStatus.Preparing)
        {
            return ServiceResult.Failure("Order cannot be marked as ready in current status", ErrorCodes.BAD_REQUEST);
        }

        order.Status = OrderStatus.Ready;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(
                order.Id,
                order.UserId,
                order.Status.ToStringValue(),
                $"Your order {order.OrderNumber} is ready for pickup from {order.Merchant.Name}!");
        }

        // Send status change notification to merchant
        if (_signalROrderSender != null)
        {
            await _signalROrderSender.SendOrderStatusChangedToMerchantAsync(
                order.MerchantId,
                order.Id,
                order.OrderNumber,
                order.Status.ToStringValue());
        }

        return ServiceResult.Success();
    }

    public async Task<Result> CancelOrderAsync(
        Guid orderId,
        Guid merchantOwnerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reason);

        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, 
                include: "Merchant,User", 
                cancellationToken: cancellationToken);

        if (order == null)
        {
            return ServiceResult.Failure("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Verify merchant ownership
        if (order.Merchant.OwnerId != merchantOwnerId)
        {
            return ServiceResult.Failure("Access denied", ErrorCodes.FORBIDDEN);
        }

        // Check if order can be cancelled
        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            return ServiceResult.Failure("Order cannot be cancelled in current status", ErrorCodes.BAD_REQUEST);
        }

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason;
        order.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Order>().Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification
        if (_signalRService != null)
        {
            await _signalRService.SendOrderStatusUpdateAsync(
                order.Id,
                order.UserId,
                order.Status.ToStringValue(),
                $"Your order {order.OrderNumber} has been cancelled by {order.Merchant.Name}. Reason: {reason}");
        }

        // Send cancellation notification to merchant
        if (_signalROrderSender != null)
        {
            await _signalROrderSender.SendOrderCancelledToMerchantAsync(
                order.MerchantId,
                order.Id,
                order.OrderNumber,
                reason);
        }

        return ServiceResult.Success();
    }

    public async Task<Result<OrderStatisticsResponse>> GetOrderStatisticsAsync(
        Guid merchantOwnerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Get merchant owned by this user
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return ServiceResult.Failure<OrderStatisticsResponse>("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
        }

        var start = startDate ?? DateTime.UtcNow.Date.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow.Date.AddDays(1);

        Expression<Func<Order, bool>> dateFilter = o => o.MerchantId == merchant.Id && o.CreatedAt >= start && o.CreatedAt < end;
        var orders = await _unitOfWork.Repository<Order>().ListAsync(
            filter: dateFilter,
            cancellationToken: cancellationToken);

        var today = DateTime.UtcNow.Date;
        var thisWeek = today.AddDays(-(int)today.DayOfWeek);
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        var stats = new OrderStatisticsResponse(
            TotalOrders: orders.Count,
            PendingOrders: orders.Count(o => o.Status == OrderStatus.Pending),
            ConfirmedOrders: orders.Count(o => o.Status == OrderStatus.Confirmed),
            PreparingOrders: orders.Count(o => o.Status == OrderStatus.Preparing),
            ReadyOrders: orders.Count(o => o.Status == OrderStatus.Ready),
            DeliveredOrders: orders.Count(o => o.Status == OrderStatus.Delivered),
            CancelledOrders: orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue: orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.Total),
            AverageOrderValue: orders.Any() ? orders.Average(o => o.Total) : 0,
            TodayOrders: orders.Count(o => o.CreatedAt.Date == today),
            ThisWeekOrders: orders.Count(o => o.CreatedAt.Date >= thisWeek),
            ThisMonthOrders: orders.Count(o => o.CreatedAt.Date >= thisMonth),
            GeneratedAt: DateTime.UtcNow);

        return ServiceResult.Success(stats);
    }

    #region Payment Integration Helper Methods

    private async Task<Result> CreatePaymentForOrderAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            // PaymentMethod string'ini enum'a çevir
            if (!Enum.TryParse<PaymentMethod>(order.PaymentMethod, out var paymentMethod))
            {
                return Result.Fail($"Invalid payment method: {order.PaymentMethod}", "INVALID_PAYMENT_METHOD");
            }

            // Para üstü hesapla (sadece Cash için)
            decimal? changeAmount = null;
            if (paymentMethod == PaymentMethod.Cash)
            {
                // Cash payment için para üstü hesaplama (örnek: 100 TL verildi, 85.50 TL ödeme)
                // Bu gerçek uygulamada frontend'den gelecek
                changeAmount = 0; // Şimdilik 0, gerçek uygulamada hesaplanacak
            }

            var paymentRequest = new CreatePaymentRequest(
                OrderId: order.Id,
                PaymentMethod: paymentMethod,
                Amount: order.Total,
                ChangeAmount: changeAmount,
                Notes: $"Payment for order {order.OrderNumber}"
            );

            var paymentResult = await _paymentService.CreatePaymentAsync(paymentRequest, cancellationToken);
            
            if (paymentResult.Success)
            {
                // Order'ın payment status'unu güncelle
                order.PaymentStatus = PaymentStatus.Pending.ToString();
                _unitOfWork.Repository<Order>().Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment created successfully for order {OrderId}: {PaymentId}", 
                    order.Id, paymentResult.Value?.Id);
            }

            return paymentResult.Success ? Result.Ok() : Result.Fail(paymentResult.Error ?? "Unknown error", paymentResult.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order {OrderId}", order.Id);
            return Result.Fail("Failed to create payment", "PAYMENT_CREATION_ERROR");
        }
    }

    private async Task CancelOrderDueToPaymentFailureAsync(Guid orderId, string paymentError, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var order = await _unitOfWork.Repository<Order>()
                .GetByIdAsync(orderId, cancellationToken);

            if (order != null)
            {
                order.Status = OrderStatus.Cancelled;
                order.CancellationReason = $"Payment failed: {paymentError}";
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Order>().Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);

                _logger.LogWarning("Order {OrderId} cancelled due to payment failure: {PaymentError}", 
                    orderId, paymentError);

                // SignalR notification
                if (_signalRService != null)
                {
                    await _signalRService.SendOrderStatusUpdateAsync(
                        orderId,
                        order.UserId,
                        OrderStatus.Cancelled.ToStringValue(),
                        $"Order cancelled due to payment failure: {paymentError}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId} due to payment failure", orderId);
            await _unitOfWork.RollbackAsync(cancellationToken);
        }
    }

    #endregion

    #region Additional Merchant Order Methods

    public async Task<Result<OrderDetailsResponse>> GetMerchantOrderDetailsAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantOrderDetailsInternalAsync(orderId, merchantOwnerId, cancellationToken),
            "GetMerchantOrderDetails",
            new { orderId, merchantOwnerId },
            cancellationToken);
    }

    private async Task<Result<OrderDetailsResponse>> GetMerchantOrderDetailsInternalAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Merchant.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail<OrderDetailsResponse>("Order not found or access denied", ErrorCodes.ORDER_NOT_FOUND);
        }

        var response = new OrderDetailsResponse(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            MerchantId: order.MerchantId,
            MerchantName: order.Merchant?.Name ?? "Unknown", // Nullable reference type warning suppressed
            CustomerId: order.UserId,
            CustomerName: "Customer", // TODO: Get from user service
            Status: order.Status.ToString(),
            SubTotal: order.SubTotal,
            DeliveryFee: order.DeliveryFee,
            Discount: 0, // TODO: Add DiscountAmount property to Order entity
            Total: order.Total,
            PaymentMethod: "Cash", // TODO: Get from payment
            PaymentStatus: "Pending", // TODO: Get from payment
            DeliveryAddress: order.DeliveryAddress ?? "Unknown", // Nullable reference type warning suppressed
            EstimatedDeliveryTime: order.EstimatedDeliveryTime,
            CreatedAt: order.CreatedAt,
            CompletedAt: null, // TODO: Add CompletedAt property to Order entity
            Items: new List<OrderLineResponse>(), // TODO: Get order items
            Timeline: new List<OrderTimelineResponse>() // TODO: Get timeline
        );

        return Result.Ok(response);
    }

    public async Task<Result> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await UpdateOrderStatusInternalAsync(orderId, request, merchantOwnerId, cancellationToken),
            "UpdateOrderStatus",
            new { orderId, merchantOwnerId, request.NewStatus },
            cancellationToken);
    }

    private async Task<Result> UpdateOrderStatusInternalAsync(
        Guid orderId,
        UpdateOrderStatusRequest _,
        Guid merchantOwnerId,
        CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Merchant.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail("Order not found or access denied", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Update order status logic here
        // This is a simplified implementation
        return Result.Ok();
    }

    public async Task<Result<OrderAnalyticsResponse>> GetOrderAnalyticsAsync(
        Guid merchantOwnerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetOrderAnalyticsInternalAsync(merchantOwnerId, startDate, endDate, cancellationToken),
            "GetOrderAnalytics",
            new { merchantOwnerId, startDate, endDate },
            cancellationToken);
    }

    private Task<Result<OrderAnalyticsResponse>> GetOrderAnalyticsInternalAsync(
        Guid _merchantOwnerId,
        DateTime? _startDate,
        DateTime? _endDate,
        CancellationToken _cancellationToken)
    {
        // Simplified analytics implementation
        var response = new OrderAnalyticsResponse(
            TotalOrders: 0,
            TotalRevenue: 0,
            AverageOrderValue: 0,
            OrdersByStatus: new Dictionary<string, int>(),
            RevenueByDay: new Dictionary<string, decimal>(),
            GeneratedAt: DateTime.UtcNow
        );

        return Task.FromResult(Result.Ok(response));
    }

    public async Task<Result<PagedResult<OrderResponse>>> GetPendingOrdersAsync(
        Guid merchantOwnerId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetPendingOrdersInternalAsync(merchantOwnerId, query, cancellationToken),
            "GetPendingOrders",
            new { merchantOwnerId, query.Page, query.PageSize },
            cancellationToken);
    }

    private Task<Result<PagedResult<OrderResponse>>> GetPendingOrdersInternalAsync(
        Guid _merchantOwnerId,
        PaginationQuery query,
        CancellationToken _cancellationToken)
    {
        // Simplified pending orders implementation
        var response = new PagedResult<OrderResponse>
        {
            Items = new List<OrderResponse>(),
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Task.FromResult(Result.Ok(response));
    }

    public async Task<Result<OrderTimelineResponse>> GetOrderTimelineAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetOrderTimelineInternalAsync(orderId, merchantOwnerId, cancellationToken),
            "GetOrderTimeline",
            new { orderId, merchantOwnerId },
            cancellationToken);
    }

    private async Task<Result<OrderTimelineResponse>> GetOrderTimelineInternalAsync(
        Guid orderId,
        Guid merchantOwnerId,
        CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Merchant.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail<OrderTimelineResponse>("Order not found or access denied", ErrorCodes.ORDER_NOT_FOUND);
        }

        // Simplified timeline implementation
        var response = new OrderTimelineResponse(
            Timestamp: order.CreatedAt,
            Status: "Created",
            Description: "Order created",
            ActorName: "System"
        );

        return Result.Ok(response);
    }

    #endregion
}

// Background task data classes
public class OrderCreatedTask
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
}

public class OrderStatusChangedTask
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
}
