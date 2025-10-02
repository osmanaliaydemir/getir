# 📏 Method Length Optimization

## 📋 Genel Bakış

Bu dokümantasyon, Getir API projesindeki uzun method'ları optimize etmek için detaylı implementation guide'ı sağlar.

## 🎯 Tespit Edilen Uzun Method'lar

### 1. **OrderService.CreateOrderInternalAsync (100+ satır)**

#### **Mevcut Durum**
```csharp
private async Task<Result<OrderResponse>> CreateOrderInternalAsync(
    Guid userId, CreateOrderRequest request, CancellationToken cancellationToken)
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
            return ServiceResult.Failure<OrderResponse>("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
        }

        // Product kontrolü ve stock kontrolü
        var orderLines = new List<OrderLine>();
        decimal subTotal = 0;

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
                return ServiceResult.Failure<OrderResponse>($"Insufficient stock for product {product.Name}", ErrorCodes.INSUFFICIENT_STOCK);
            }

            // Fiyat hesaplama
            var unitPrice = product.DiscountedPrice ?? product.Price;
            var lineTotal = unitPrice * item.Quantity;
            subTotal += lineTotal;

            // OrderLine oluştur
            var orderLine = new OrderLine
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = lineTotal,
                Notes = item.Notes
            };

            orderLines.Add(orderLine);

            // Stock güncelle
            product.StockQuantity -= item.Quantity;
            _unitOfWork.Repository<Product>().Update(product);
        }

        // Coupon kontrolü
        decimal discount = 0;
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var coupon = await _unitOfWork.ReadRepository<Coupon>()
                .FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive, cancellationToken: cancellationToken);

            if (coupon != null && DateTime.UtcNow >= coupon.StartDate && DateTime.UtcNow <= coupon.EndDate)
            {
                if (subTotal >= coupon.MinimumOrderAmount)
                {
                    if (coupon.DiscountType == "Percentage")
                    {
                        discount = subTotal * (coupon.DiscountValue / 100);
                        if (coupon.MaximumDiscountAmount.HasValue)
                        {
                            discount = Math.Min(discount, coupon.MaximumDiscountAmount.Value);
                        }
                    }
                    else
                    {
                        discount = coupon.DiscountValue;
                    }
                }
            }
        }

        // Delivery fee hesaplama
        var deliveryFee = merchant.DeliveryFee;
        var total = subTotal + deliveryFee - discount;

        // Order oluştur
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            MerchantId = request.MerchantId,
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            Total = total,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "Pending",
            DeliveryAddress = request.DeliveryAddress,
            DeliveryLatitude = request.DeliveryLatitude,
            DeliveryLongitude = request.DeliveryLongitude,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);

        // OrderLines ekle
        foreach (var orderLine in orderLines)
        {
            orderLine.OrderId = order.Id;
            await _unitOfWork.Repository<OrderLine>().AddAsync(orderLine, cancellationToken);
        }

        // Coupon usage kaydet
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var couponUsage = new CouponUsage
            {
                Id = Guid.NewGuid(),
                CouponId = coupon.Id,
                UserId = userId,
                OrderId = order.Id,
                UsedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<CouponUsage>().AddAsync(couponUsage, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendToGroupAsync("merchants", "OrderCreated", new
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                MerchantId = order.MerchantId,
                Total = order.Total
            });
        }

        // Background task
        await _backgroundTaskService.QueueBackgroundWorkItemAsync(new OrderCreatedTask
        {
            OrderId = order.Id,
            UserId = userId,
            MerchantId = order.MerchantId
        });

        var response = new OrderResponse(
            order.Id,
            order.OrderNumber,
            order.MerchantId,
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
                "", // Product name will be loaded separately
                ol.Quantity,
                ol.UnitPrice,
                ol.TotalPrice,
                new List<OrderLineOptionResponse>() // Options will be loaded separately
            )).ToList()
        );

        return ServiceResult.Success(response);
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync(cancellationToken);
        _logger.LogError(ex, "Error creating order for user {UserId}", userId);
        return ServiceResult.HandleException<OrderResponse>(ex, _logger, "CreateOrder");
    }
}
```

#### **Refactored Version**

```csharp
private async Task<Result<OrderResponse>> CreateOrderInternalAsync(
    Guid userId, CreateOrderRequest request, CancellationToken cancellationToken)
{
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
        // 1. Validate order request
        var validationResult = await ValidateOrderRequestAsync(userId, request, cancellationToken);
        if (!validationResult.Success) return validationResult;

        // 2. Calculate order totals
        var calculationResult = await CalculateOrderTotalsAsync(request, cancellationToken);
        if (!calculationResult.Success) return calculationResult;

        // 3. Create order entity
        var orderResult = await CreateOrderEntityAsync(userId, request, calculationResult.Value, cancellationToken);
        if (!orderResult.Success) return orderResult;

        // 4. Save order and related entities
        await SaveOrderAsync(orderResult.Value, calculationResult.Value.OrderLines, cancellationToken);

        // 5. Send notifications
        await SendOrderNotificationsAsync(orderResult.Value, cancellationToken);

        // 6. Map to response
        var response = await MapToOrderResponseAsync(orderResult.Value, cancellationToken);
        return ServiceResult.Success(response);
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackAsync(cancellationToken);
        _logger.LogError(ex, "Error creating order for user {UserId}", userId);
        return ServiceResult.HandleException<OrderResponse>(ex, _logger, "CreateOrder");
    }
}

// Helper methods
private async Task<Result> ValidateOrderRequestAsync(
    Guid userId, CreateOrderRequest request, CancellationToken cancellationToken)
{
    // Merchant validation
    var merchant = await _unitOfWork.ReadRepository<Merchant>()
        .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.IsActive, cancellationToken: cancellationToken);

    if (merchant == null)
    {
        await _unitOfWork.RollbackAsync(cancellationToken);
        return ServiceResult.Failure("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
    }

    // Product validation
    foreach (var item in request.Items)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive, cancellationToken: cancellationToken);

        if (product == null)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return ServiceResult.Failure($"Product {item.ProductId} not found", ErrorCodes.PRODUCT_NOT_FOUND);
        }

        if (product.StockQuantity < item.Quantity)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return ServiceResult.Failure($"Insufficient stock for product {product.Name}", ErrorCodes.INSUFFICIENT_STOCK);
        }
    }

    return ServiceResult.Success();
}

private async Task<Result<OrderCalculationResult>> CalculateOrderTotalsAsync(
    CreateOrderRequest request, CancellationToken cancellationToken)
{
    var orderLines = new List<OrderLine>();
    decimal subTotal = 0;

    foreach (var item in request.Items)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken: cancellationToken);

        var unitPrice = product.DiscountedPrice ?? product.Price;
        var lineTotal = unitPrice * item.Quantity;
        subTotal += lineTotal;

        orderLines.Add(new OrderLine
        {
            Id = Guid.NewGuid(),
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = unitPrice,
            TotalPrice = lineTotal,
            Notes = item.Notes
        });
    }

    // Coupon calculation
    var discount = await CalculateCouponDiscountAsync(request.CouponCode, subTotal, cancellationToken);

    // Delivery fee
    var merchant = await _unitOfWork.ReadRepository<Merchant>()
        .FirstOrDefaultAsync(m => m.Id == request.MerchantId, cancellationToken: cancellationToken);

    var deliveryFee = merchant.DeliveryFee;
    var total = subTotal + deliveryFee - discount;

    return ServiceResult.Success(new OrderCalculationResult
    {
        SubTotal = subTotal,
        DeliveryFee = deliveryFee,
        Discount = discount,
        Total = total,
        OrderLines = orderLines
    });
}

private async Task<Result<Order>> CreateOrderEntityAsync(
    Guid userId, CreateOrderRequest request, OrderCalculationResult calculation, CancellationToken cancellationToken)
{
    var order = new Order
    {
        Id = Guid.NewGuid(),
        OrderNumber = GenerateOrderNumber(),
        UserId = userId,
        MerchantId = request.MerchantId,
        Status = OrderStatus.Pending,
        SubTotal = calculation.SubTotal,
        DeliveryFee = calculation.DeliveryFee,
        Discount = calculation.Discount,
        Total = calculation.Total,
        PaymentMethod = request.PaymentMethod,
        PaymentStatus = "Pending",
        DeliveryAddress = request.DeliveryAddress,
        DeliveryLatitude = request.DeliveryLatitude,
        DeliveryLongitude = request.DeliveryLongitude,
        Notes = request.Notes,
        CreatedAt = DateTime.UtcNow
    };

    await _unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);
    return ServiceResult.Success(order);
}

private async Task SaveOrderAsync(Order order, List<OrderLine> orderLines, CancellationToken cancellationToken)
{
    // Add order lines
    foreach (var orderLine in orderLines)
    {
        orderLine.OrderId = order.Id;
        await _unitOfWork.Repository<OrderLine>().AddAsync(orderLine, cancellationToken);
    }

    // Update product stock
    await UpdateProductStockAsync(orderLines, cancellationToken);

    // Save coupon usage if applicable
    if (!string.IsNullOrEmpty(order.CouponCode))
    {
        await SaveCouponUsageAsync(order, cancellationToken);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    await _unitOfWork.CommitAsync(cancellationToken);
}

private async Task SendOrderNotificationsAsync(Order order, CancellationToken cancellationToken)
{
    // SignalR notification
    if (_signalRService != null)
    {
        await _signalRService.SendToGroupAsync("merchants", "OrderCreated", new
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            MerchantId = order.MerchantId,
            Total = order.Total
        });
    }

    // Background task
    await _backgroundTaskService.QueueBackgroundWorkItemAsync(new OrderCreatedTask
    {
        OrderId = order.Id,
        UserId = order.UserId,
        MerchantId = order.MerchantId
    });
}

private async Task<OrderResponse> MapToOrderResponseAsync(Order order, CancellationToken cancellationToken)
{
    var merchant = await _unitOfWork.ReadRepository<Merchant>()
        .FirstOrDefaultAsync(m => m.Id == order.MerchantId, cancellationToken: cancellationToken);

    var orderLines = await _unitOfWork.ReadRepository<OrderLine>()
        .ListAsync(ol => ol.OrderId == order.Id, cancellationToken: cancellationToken);

    return new OrderResponse(
        order.Id,
        order.OrderNumber,
        order.MerchantId,
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
            "", // Product name will be loaded separately
            ol.Quantity,
            ol.UnitPrice,
            ol.TotalPrice,
            new List<OrderLineOptionResponse>()
        )).ToList()
    );
}
```

### 2. **MerchantOnboardingService.ProcessApplicationAsync (80+ satır)**

#### **Mevcut Durum**
```csharp
public async Task<Result<MerchantOnboardingResponse>> ProcessApplicationAsync(
    Guid applicationId, ProcessApplicationRequest request, CancellationToken cancellationToken)
{
    // Uzun method implementation
    // 80+ satır kod
}
```

#### **Refactored Version**

```csharp
public async Task<Result<MerchantOnboardingResponse>> ProcessApplicationAsync(
    Guid applicationId, ProcessApplicationRequest request, CancellationToken cancellationToken)
{
    // 1. Validate application
    var validationResult = await ValidateApplicationAsync(applicationId, request, cancellationToken);
    if (!validationResult.Success) return validationResult;

    // 2. Process application based on action
    var processResult = request.Action switch
    {
        "approve" => await ApproveApplicationAsync(applicationId, request, cancellationToken),
        "reject" => await RejectApplicationAsync(applicationId, request, cancellationToken),
        "request_info" => await RequestAdditionalInfoAsync(applicationId, request, cancellationToken),
        _ => ServiceResult.Failure<MerchantOnboardingResponse>("Invalid action", ErrorCodes.INVALID_ACTION)
    };

    if (!processResult.Success) return processResult;

    // 3. Send notifications
    await SendApplicationNotificationAsync(applicationId, request.Action, cancellationToken);

    // 4. Map to response
    return await MapToOnboardingResponseAsync(processResult.Value, cancellationToken);
}

// Helper methods
private async Task<Result> ValidateApplicationAsync(
    Guid applicationId, ProcessApplicationRequest request, CancellationToken cancellationToken)
{
    var application = await _unitOfWork.ReadRepository<MerchantOnboarding>()
        .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken: cancellationToken);

    if (application == null)
        return ServiceResult.Failure("Application not found", ErrorCodes.APPLICATION_NOT_FOUND);

    if (application.Status != MerchantOnboardingStatus.Pending)
        return ServiceResult.Failure("Application is not pending", ErrorCodes.APPLICATION_NOT_PENDING);

    return ServiceResult.Success();
}

private async Task<Result<MerchantOnboarding>> ApproveApplicationAsync(
    Guid applicationId, ProcessApplicationRequest request, CancellationToken cancellationToken)
{
    // Approval logic
}

private async Task<Result<MerchantOnboarding>> RejectApplicationAsync(
    Guid applicationId, ProcessApplicationRequest request, CancellationToken cancellationToken)
{
    // Rejection logic
}

private async Task<Result<MerchantOnboarding>> RequestAdditionalInfoAsync(
    Guid applicationId, ProcessApplicationRequest request, CancellationToken cancellationToken)
{
    // Request additional info logic
}
```

### 3. **ReviewService.CreateReviewAsync (60+ satır)**

#### **Mevcut Durum**
```csharp
public async Task<Result<ReviewResponse>> CreateReviewAsync(
    CreateReviewRequest request, Guid reviewerId, CancellationToken cancellationToken)
{
    // Uzun method implementation
    // 60+ satır kod
}
```

#### **Refactored Version**

```csharp
public async Task<Result<ReviewResponse>> CreateReviewAsync(
    CreateReviewRequest request, Guid reviewerId, CancellationToken cancellationToken)
{
    // 1. Validate review eligibility
    var eligibilityResult = await ValidateReviewEligibilityAsync(reviewerId, request, cancellationToken);
    if (!eligibilityResult.Success) return eligibilityResult;

    // 2. Create review entity
    var reviewResult = await CreateReviewEntityAsync(request, reviewerId, cancellationToken);
    if (!reviewResult.Success) return reviewResult;

    // 3. Save review
    await SaveReviewAsync(reviewResult.Value, cancellationToken);

    // 4. Update ratings
    await UpdateEntityRatingsAsync(request.RevieweeId, request.RevieweeType, cancellationToken);

    // 5. Send notifications
    await SendReviewNotificationAsync(reviewResult.Value, cancellationToken);

    // 6. Map to response
    return await MapToReviewResponseAsync(reviewResult.Value, cancellationToken);
}

// Helper methods
private async Task<Result> ValidateReviewEligibilityAsync(
    Guid reviewerId, CreateReviewRequest request, CancellationToken cancellationToken)
{
    // Check if user can review
    var canReview = await CanUserReviewAsync(reviewerId, request.RevieweeId, request.RevieweeType, request.OrderId, cancellationToken);
    if (!canReview.Success || !canReview.Value)
        return ServiceResult.Failure("User cannot review this entity", ErrorCodes.CANNOT_REVIEW);

    // Check if user already reviewed
    var hasReviewed = await HasUserReviewedOrderAsync(reviewerId, request.OrderId, cancellationToken);
    if (!hasReviewed.Success || hasReviewed.Value)
        return ServiceResult.Failure("User has already reviewed this order", ErrorCodes.ALREADY_REVIEWED);

    return ServiceResult.Success();
}

private async Task<Result<Review>> CreateReviewEntityAsync(
    CreateReviewRequest request, Guid reviewerId, CancellationToken cancellationToken)
{
    var review = new Review
    {
        Id = Guid.NewGuid(),
        ReviewerId = reviewerId,
        RevieweeId = request.RevieweeId,
        RevieweeType = request.RevieweeType,
        OrderId = request.OrderId,
        Rating = request.Rating,
        Comment = request.Comment,
        CreatedAt = DateTime.UtcNow
    };

    return ServiceResult.Success(review);
}
```

## 🎯 Refactoring Principles

### **1. Single Responsibility Principle**
Her method tek bir sorumluluğa sahip olmalı.

### **2. Method Length Guidelines**
- **Public methods**: Max 20 satır
- **Private methods**: Max 15 satır
- **Helper methods**: Max 10 satır

### **3. Naming Conventions**
- **Validation methods**: `Validate*Async`
- **Calculation methods**: `Calculate*Async`
- **Creation methods**: `Create*Async`
- **Mapping methods**: `MapTo*Async`
- **Notification methods**: `Send*NotificationAsync`

### **4. Error Handling**
Her helper method kendi error handling'ini yapmalı.

### **5. Async/Await**
Tüm I/O operations async olmalı.

## 📊 Beklenen Faydalar

### **Code Quality**
- **Method Length**: %60 azalış
- **Cyclomatic Complexity**: %50 azalış
- **Maintainability**: %40 artış

### **Testability**
- **Unit Test Coverage**: %30 artış
- **Test Isolation**: %50 artış
- **Mock Complexity**: %40 azalış

### **Performance**
- **Memory Usage**: %20 azalış
- **Execution Time**: %15 iyileşme
- **Debugging Time**: %50 azalış

## 🚀 Implementation Plan

### **Hafta 1: OrderService Refactoring**
1. `CreateOrderInternalAsync` method'unu analiz et
2. Helper method'ları oluştur
3. Test'leri güncelle

### **Hafta 2: MerchantOnboardingService Refactoring**
1. `ProcessApplicationAsync` method'unu analiz et
2. Helper method'ları oluştur
3. Test'leri güncelle

### **Hafta 3: ReviewService Refactoring**
1. `CreateReviewAsync` method'unu analiz et
2. Helper method'ları oluştur
3. Test'leri güncelle

### **Hafta 4: Other Services**
1. Diğer uzun method'ları tespit et
2. Refactor et
3. Code review

## ✅ Success Criteria

- [ ] Tüm method'lar 20 satır altında
- [ ] Her method tek sorumluluğa sahip
- [ ] Helper method'lar reusable
- [ ] Test coverage korundu
- [ ] Performance iyileşti
- [ ] Code review geçti
