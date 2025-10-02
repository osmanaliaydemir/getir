using System.Linq.Expressions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Payments;

/// <summary>
/// Payment Service implementation - şu anda sadece Cash payment destekleniyor
/// Gelecekte CreditCard, VodafonePay vb. eklenecek
/// </summary>
public class PaymentService : BaseService, IPaymentService
{
    private readonly ISignalRService? _signalRService;

    public PaymentService(
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        ISignalRService? signalRService = null) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _signalRService = signalRService;
    }

    public async Task<Result<PaymentResponse>> CreatePaymentAsync(
        CreatePaymentRequest request, 
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CreatePaymentInternalAsync(request, cancellationToken),
            "CreatePayment",
            new { OrderId = request.OrderId, PaymentMethod = request.PaymentMethod, Amount = request.Amount },
            cancellationToken);
    }

    private async Task<Result<PaymentResponse>> CreatePaymentInternalAsync(
        CreatePaymentRequest request, 
        CancellationToken cancellationToken)
    {
        // 1. Order validation
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken: cancellationToken);

        if (order == null)
        {
            return Result.Fail<PaymentResponse>("Order not found", ErrorCodes.ORDER_NOT_FOUND);
        }

        // 2. Payment method validation
        if (!IsPaymentMethodSupported(request.PaymentMethod))
        {
            return Result.Fail<PaymentResponse>(
                $"Payment method {request.PaymentMethod.GetDisplayName()} is not supported yet", 
                "PAYMENT_METHOD_NOT_SUPPORTED");
        }

        // 3. Create payment record
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Pending,
            Amount = request.Amount,
            ChangeAmount = request.ChangeAmount,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);

        // 4. Update order payment information
        order.PaymentMethod = request.PaymentMethod.ToString();
        order.PaymentStatus = PaymentStatus.Pending.ToString();
        _unitOfWork.Repository<Order>().Update(order);

        // 5. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Send notification based on payment method
        await SendPaymentNotificationAsync(payment, order, cancellationToken);
        
        // 7. Send SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendPaymentCreatedNotificationAsync(
                payment.Id, 
                order.Id, 
                order.UserId, 
                payment.PaymentMethod, 
                payment.Amount);
        }

        // 7. Return response
        var response = await MapToPaymentResponseAsync(payment, cancellationToken);
        return Result.Ok(response);
    }

    public async Task<Result> UpdatePaymentStatusAsync(
        Guid paymentId, 
        PaymentStatusUpdateRequest request, 
        CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return Result.Fail("Payment not found", "NOT_FOUND_PAYMENT");
        }

        // Update payment status
        payment.Status = request.Status;
        payment.UpdatedAt = DateTime.UtcNow;

        // Set timestamps based on status
        switch (request.Status)
        {
            case PaymentStatus.Completed:
                payment.CompletedAt = DateTime.UtcNow;
                break;
            case PaymentStatus.Failed:
                payment.FailureReason = request.FailureReason;
                break;
        }

        // Update external information if provided
        if (!string.IsNullOrEmpty(request.ExternalTransactionId))
        {
            payment.ExternalTransactionId = request.ExternalTransactionId;
        }

        if (!string.IsNullOrEmpty(request.ExternalResponse))
        {
            payment.ExternalResponse = request.ExternalResponse;
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            payment.Notes = request.Notes;
        }

        _unitOfWork.Repository<Payment>().Update(payment);

        // Update order payment status
        var order = await _unitOfWork.Repository<Order>()
            .GetByIdAsync(payment.OrderId, cancellationToken);
        
        if (order != null)
        {
            order.PaymentStatus = request.Status.ToString();
            _unitOfWork.Repository<Order>().Update(order);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendPaymentStatusUpdateAsync(
                payment.Id, 
                payment.OrderId, 
                order?.UserId ?? Guid.Empty, 
                request.Status, 
                request.Notes ?? "Payment status updated");
        }

        return Result.Ok();
    }

    public async Task<Result<PaymentResponse>> GetPaymentByIdAsync(
        Guid paymentId, 
        CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.ReadRepository<Payment>()
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken: cancellationToken);

        if (payment == null)
        {
            return Result.Fail<PaymentResponse>("Payment not found", "NOT_FOUND_PAYMENT");
        }

        var response = await MapToPaymentResponseAsync(payment, cancellationToken);
        return Result.Ok(response);
    }

    public async Task<Result<PagedResult<PaymentResponse>>> GetOrderPaymentsAsync(
        Guid orderId, 
        PaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.ReadRepository<Payment>()
            .GetPagedAsync(
                filter: p => p.OrderId == orderId,
                orderBy: p => p.CreatedAt,
                ascending: false,
                page: query.Page,
                pageSize: query.PageSize,
                cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Payment>()
            .CountAsync(p => p.OrderId == orderId, cancellationToken: cancellationToken);

        var responses = new List<PaymentResponse>();
        foreach (var payment in payments)
        {
            var response = await MapToPaymentResponseAsync(payment, cancellationToken);
            responses.Add(response);
        }

        var pagedResult = PagedResult<PaymentResponse>.Create(responses, total, query.Page, query.PageSize);
        return Result.Ok(pagedResult);
    }

    public async Task<Result> CancelPaymentAsync(
        Guid paymentId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return Result.Fail("Payment not found", "NOT_FOUND_PAYMENT");
        }

        if (payment.Status.IsFinalStatus())
        {
            return Result.Fail("Cannot cancel payment with final status", "INVALID_PAYMENT_STATUS");
        }

        payment.Status = PaymentStatus.Cancelled;
        payment.Notes = reason;
        payment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Payment>().Update(payment);

        // Update order status
        var order = await _unitOfWork.Repository<Order>()
            .GetByIdAsync(payment.OrderId, cancellationToken);
        
        if (order != null)
        {
            order.PaymentStatus = PaymentStatus.Cancelled.ToString();
            _unitOfWork.Repository<Order>().Update(order);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> RefundPaymentAsync(
        Guid paymentId, 
        decimal amount, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return Result.Fail("Payment not found", "NOT_FOUND_PAYMENT");
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            return Result.Fail("Can only refund completed payments", "INVALID_PAYMENT_STATUS");
        }

        if (amount > payment.Amount)
        {
            return Result.Fail("Refund amount cannot exceed payment amount", "INVALID_REFUND_AMOUNT");
        }

        payment.RefundAmount = amount;
        payment.RefundReason = reason;
        payment.RefundedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Payment>().Update(payment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    // === CASH PAYMENT SPECIFIC METHODS ===

    public async Task<Result<PagedResult<PaymentResponse>>> GetPendingCashPaymentsAsync(
        Guid courierId, 
        PaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.ReadRepository<Payment>()
            .GetPagedAsync(
                filter: p => p.PaymentMethod == PaymentMethod.Cash && 
                           p.Status == PaymentStatus.Pending &&
                           p.Order.CourierId == courierId,
                orderBy: p => p.CreatedAt,
                ascending: true,
                page: query.Page,
                pageSize: query.PageSize,
                cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Payment>()
            .CountAsync(p => p.PaymentMethod == PaymentMethod.Cash && 
                           p.Status == PaymentStatus.Pending &&
                           p.Order.CourierId == courierId, 
                       cancellationToken: cancellationToken);

        var responses = new List<PaymentResponse>();
        foreach (var payment in payments)
        {
            var response = await MapToPaymentResponseAsync(payment, cancellationToken);
            responses.Add(response);
        }

        var pagedResult = PagedResult<PaymentResponse>.Create(responses, total, query.Page, query.PageSize);
        return Result.Ok(pagedResult);
    }

    public async Task<Result> MarkCashPaymentAsCollectedAsync(
        Guid paymentId, 
        Guid courierId, 
        CollectCashPaymentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return Result.Fail("Payment not found", "NOT_FOUND_PAYMENT");
        }

        if (payment.PaymentMethod != PaymentMethod.Cash)
        {
            return Result.Fail("Payment is not a cash payment", "INVALID_PAYMENT_TYPE");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Fail("Payment is not in pending status", "INVALID_PAYMENT_STATUS");
        }

        // Update payment
        payment.Status = PaymentStatus.Completed;
        payment.CollectedAt = DateTime.UtcNow;
        payment.CollectedByCourierId = courierId;
        payment.Notes = request.Notes;
        payment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Payment>().Update(payment);

        // Create cash collection record
        var collection = new CourierCashCollection
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            CourierId = courierId,
            CollectedAmount = request.CollectedAmount,
            CollectedAt = DateTime.UtcNow,
            Notes = request.Notes,
            Status = "Collected",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CourierCashCollection>().AddAsync(collection, cancellationToken);

        // Update order status
        var order = await _unitOfWork.Repository<Order>()
            .GetByIdAsync(payment.OrderId, cancellationToken);
        
        if (order != null)
        {
            order.PaymentStatus = PaymentStatus.Completed.ToString();
            _unitOfWork.Repository<Order>().Update(order);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send SignalR notifications
        if (_signalRService != null)
        {
            await _signalRService.SendPaymentCollectedNotificationAsync(
                payment.Id, 
                payment.OrderId, 
                order?.UserId ?? Guid.Empty, 
                request.CollectedAmount, 
                "Courier");
                
            await _signalRService.SendMerchantPaymentNotificationAsync(
                order?.MerchantId ?? Guid.Empty, 
                payment.OrderId, 
                request.CollectedAmount, 
                "Collected");
        }

        return Result.Ok();
    }

    public async Task<Result> MarkCashPaymentAsFailedAsync(
        Guid paymentId, 
        Guid courierId, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(paymentId, cancellationToken);

        if (payment == null)
        {
            return Result.Fail("Payment not found", "NOT_FOUND_PAYMENT");
        }

        if (payment.PaymentMethod != PaymentMethod.Cash)
        {
            return Result.Fail("Payment is not a cash payment", "INVALID_PAYMENT_TYPE");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Fail("Payment is not in pending status", "INVALID_PAYMENT_STATUS");
        }

        // Update payment
        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = reason;
        payment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Payment>().Update(payment);

        // Update order status
        var order = await _unitOfWork.Repository<Order>()
            .GetByIdAsync(payment.OrderId, cancellationToken);
        
        if (order != null)
        {
            order.PaymentStatus = PaymentStatus.Failed.ToString();
            _unitOfWork.Repository<Order>().Update(order);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send SignalR notification
        if (_signalRService != null)
        {
            await _signalRService.SendPaymentFailedNotificationAsync(
                payment.Id, 
                payment.OrderId, 
                order?.UserId ?? Guid.Empty, 
                reason);
        }

        return Result.Ok();
    }

    public async Task<Result<CourierCashSummaryResponse>> GetCourierCashSummaryAsync(
        Guid courierId, 
        DateTime? date = null, 
        CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var startDate = targetDate.Date;
        var endDate = startDate.AddDays(1);

        var courier = await _unitOfWork.ReadRepository<Courier>()
            .FirstOrDefaultAsync(c => c.Id == courierId, cancellationToken: cancellationToken);

        if (courier == null)
        {
            return Result.Fail<CourierCashSummaryResponse>("Courier not found", "NOT_FOUND_COURIER");
        }

        var collections = await _unitOfWork.ReadRepository<CourierCashCollection>()
            .ListAsync(
                filter: c => c.CourierId == courierId && 
                           c.CollectedAt >= startDate && 
                           c.CollectedAt < endDate,
                cancellationToken: cancellationToken);

        var totalCollected = collections.Sum(c => c.CollectedAmount);
        var successfulCollections = collections.Count(c => c.Status == "Collected");
        var failedCollections = collections.Count(c => c.Status == "Failed");

        // Get payment details
        var paymentIds = collections.Select(c => c.PaymentId).ToList();
        var payments = await _unitOfWork.ReadRepository<Payment>()
            .ListAsync(
                filter: p => paymentIds.Contains(p.Id),
                cancellationToken: cancellationToken);

        var paymentResponses = new List<PaymentResponse>();
        foreach (var payment in payments)
        {
            var response = await MapToPaymentResponseAsync(payment, cancellationToken);
            paymentResponses.Add(response);
        }

        var summary = new CourierCashSummaryResponse(
            courierId,
            courier.User?.FirstName + " " + courier.User?.LastName ?? "Unknown",
            targetDate,
            totalCollected,
            collections.Count,
            successfulCollections,
            failedCollections,
            paymentResponses
        );

        return Result.Ok(summary);
    }

    // === MERCHANT METHODS ===

    public async Task<Result<MerchantCashSummaryResponse>> GetMerchantCashSummaryAsync(
        Guid merchantId, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow.Date.AddDays(1);

        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantCashSummaryResponse>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var payments = await _unitOfWork.ReadRepository<Payment>()
            .ListAsync(
                filter: p => p.PaymentMethod == PaymentMethod.Cash && 
                           p.Status == PaymentStatus.Completed &&
                           p.Order.MerchantId == merchantId &&
                           p.CompletedAt >= start && 
                           p.CompletedAt < end,
                cancellationToken: cancellationToken);

        var totalAmount = payments.Sum(p => p.Amount);
        var totalCommission = totalAmount * 0.15m; // %15 komisyon (configurable olmalı)
        var netAmount = totalAmount - totalCommission;

        var paymentResponses = new List<PaymentResponse>();
        foreach (var payment in payments)
        {
            var response = await MapToPaymentResponseAsync(payment, cancellationToken);
            paymentResponses.Add(response);
        }

        var summary = new MerchantCashSummaryResponse(
            merchantId,
            merchant.Name,
            startDate,
            endDate,
            totalAmount,
            totalCommission,
            netAmount,
            payments.Count,
            paymentResponses
        );

        return Result.Ok(summary);
    }

    public async Task<Result<PagedResult<SettlementResponse>>> GetMerchantSettlementsAsync(
        Guid merchantId, 
        PaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        var settlements = await _unitOfWork.ReadRepository<CashSettlement>()
            .GetPagedAsync(
                filter: s => s.MerchantId == merchantId,
                orderBy: s => s.SettlementDate,
                ascending: false,
                page: query.Page,
                pageSize: query.PageSize,
                cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<CashSettlement>()
            .CountAsync(s => s.MerchantId == merchantId, cancellationToken: cancellationToken);

        var responses = new List<SettlementResponse>();
        foreach (var settlement in settlements)
        {
            var response = new SettlementResponse(
                settlement.Id,
                settlement.MerchantId,
                settlement.Merchant?.Name ?? "Unknown",
                settlement.TotalAmount,
                settlement.Commission,
                settlement.NetAmount,
                settlement.SettlementDate,
                settlement.Status,
                settlement.Notes,
                settlement.ProcessedByAdminId,
                settlement.ProcessedByAdmin?.FirstName + " " + settlement.ProcessedByAdmin?.LastName ?? "Unknown",
                settlement.CompletedAt,
                settlement.BankTransferReference,
                settlement.CreatedAt
            );
            responses.Add(response);
        }

        var pagedResult = PagedResult<SettlementResponse>.Create(responses, total, query.Page, query.PageSize);
        return Result.Ok(pagedResult);
    }

    // === ADMIN METHODS ===

    public async Task<Result<PagedResult<PaymentResponse>>> GetAllCashPaymentsAsync(
        PaginationQuery query, 
        string? status = null, 
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Payment, bool>> filter = p => p.PaymentMethod == PaymentMethod.Cash;
        
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, out var statusEnum))
        {
            filter = p => p.PaymentMethod == PaymentMethod.Cash && p.Status == statusEnum;
        }

        var payments = await _unitOfWork.ReadRepository<Payment>()
            .GetPagedAsync(
                filter: filter,
                orderBy: p => p.CreatedAt,
                ascending: false,
                page: query.Page,
                pageSize: query.PageSize,
                cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Payment>()
            .CountAsync(filter, cancellationToken: cancellationToken);

        var responses = new List<PaymentResponse>();
        foreach (var payment in payments)
        {
            var response = await MapToPaymentResponseAsync(payment, cancellationToken);
            responses.Add(response);
        }

        var pagedResult = PagedResult<PaymentResponse>.Create(responses, total, query.Page, query.PageSize);
        return Result.Ok(pagedResult);
    }

    public async Task<Result> ProcessSettlementAsync(
        Guid merchantId, 
        ProcessSettlementRequest request, 
        Guid adminId, 
        CancellationToken cancellationToken = default)
    {
        // Get merchant's completed cash payments that haven't been settled
        var payments = await _unitOfWork.ReadRepository<Payment>()
            .ListAsync(
                filter: p => p.PaymentMethod == PaymentMethod.Cash && 
                           p.Status == PaymentStatus.Completed &&
                           p.Order.MerchantId == merchantId &&
                           p.SettledAt == null,
                cancellationToken: cancellationToken);

        if (!payments.Any())
        {
            return Result.Fail("No payments to settle", "NO_PAYMENTS_TO_SETTLE");
        }

        var totalAmount = payments.Sum(p => p.Amount);
        var commission = totalAmount * (request.CommissionRate / 100m);
        var netAmount = totalAmount - commission;

        // Create settlement record
        var settlement = new CashSettlement
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            TotalAmount = totalAmount,
            Commission = commission,
            NetAmount = netAmount,
            SettlementDate = DateTime.UtcNow,
            Status = "Pending",
            Notes = request.Notes,
            ProcessedByAdminId = adminId,
            BankTransferReference = request.BankTransferReference,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CashSettlement>().AddAsync(settlement, cancellationToken);

        // Update payments as settled
        foreach (var payment in payments)
        {
            payment.SettledAt = DateTime.UtcNow;
            _unitOfWork.Repository<Payment>().Update(payment);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    // === PRIVATE HELPER METHODS ===

    private bool IsPaymentMethodSupported(PaymentMethod method)
    {
        // Şu anda sadece Cash destekleniyor
        return method == PaymentMethod.Cash;
        
        // Gelecekte diğer methodlar eklenecek:
        // return method is PaymentMethod.Cash or PaymentMethod.CreditCard or PaymentMethod.VodafonePay;
    }

    private async Task SendPaymentNotificationAsync(Payment payment, Order order, CancellationToken cancellationToken)
    {
        if (_signalRService == null) return;

        switch (payment.PaymentMethod)
        {
            case PaymentMethod.Cash:
                // Courier'a nakit ödeme toplama bildirimi
                if (order.CourierId.HasValue)
                {
                    await _signalRService.SendNotificationToUserAsync(
                        order.CourierId.Value,
                        "Nakit Ödeme Toplama",
                        $"Sipariş #{order.OrderNumber} için {payment.Amount:C} nakit ödeme toplamanız gerekiyor.",
                        "CashCollection");
                }
                break;
                
            // Gelecekte diğer ödeme yöntemleri için bildirimler
            case PaymentMethod.CreditCard:
                // Kredi kartı ödeme bildirimi
                break;
                
            case PaymentMethod.VodafonePay:
                // Vodafone Pay ödeme bildirimi
                break;
        }
    }

    private async Task<PaymentResponse> MapToPaymentResponseAsync(Payment payment, CancellationToken cancellationToken)
    {
        var courierName = "";
        if (payment.CollectedByCourierId.HasValue)
        {
            var courier = await _unitOfWork.ReadRepository<Courier>()
                .FirstOrDefaultAsync(c => c.Id == payment.CollectedByCourierId.Value, cancellationToken: cancellationToken);
            courierName = courier?.User?.FirstName + " " + courier?.User?.LastName ?? "Unknown";
        }

        return new PaymentResponse(
            payment.Id,
            payment.OrderId,
            payment.PaymentMethod,
            payment.Status,
            payment.Amount,
            payment.ChangeAmount,
            payment.ProcessedAt,
            payment.CompletedAt,
            payment.CollectedAt,
            payment.SettledAt,
            payment.CollectedByCourierId,
            courierName,
            payment.Notes,
            payment.FailureReason,
            payment.ExternalTransactionId,
            payment.RefundAmount,
            payment.RefundedAt,
            payment.RefundReason,
            payment.CreatedAt
        );
    }
}
