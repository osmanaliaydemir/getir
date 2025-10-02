using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Enums;
using Getir.WebApi.Extensions;
using Getir.Application.Common;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payments");

        // === CUSTOMER ENDPOINTS ===
        
        group.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .WithSummary("Create a new payment")
            .RequireAuthorization()
            .Accepts<CreatePaymentRequest>("application/json")
            .Produces<PaymentResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(404);

        group.MapGet("/{paymentId:guid}", GetPaymentById)
            .WithName("GetPaymentById")
            .WithSummary("Get payment details by ID")
            .RequireAuthorization()
            .Produces<PaymentResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(404);

        group.MapGet("/order/{orderId:guid}", GetOrderPayments)
            .WithName("GetOrderPayments")
            .WithSummary("Get all payments for an order")
            .RequireAuthorization()
            .Produces<PagedResult<PaymentResponse>>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(404);

        // === COURIER ENDPOINTS ===
        
        group.MapGet("/courier/pending", GetPendingCashPayments)
            .WithName("GetPendingCashPayments")
            .WithSummary("Get pending cash payments for courier")
            .RequireAuthorization()
            .Produces<PagedResult<PaymentResponse>>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403);

        group.MapPost("/courier/{paymentId:guid}/collect", CollectCashPayment)
            .WithName("CollectCashPayment")
            .WithSummary("Mark cash payment as collected")
            .RequireAuthorization()
            .Accepts<CollectCashPaymentRequest>("application/json")
            .Produces(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);

        group.MapPost("/courier/{paymentId:guid}/fail", FailCashPayment)
            .WithName("FailCashPayment")
            .WithSummary("Mark cash payment as failed")
            .RequireAuthorization()
            .Accepts<object>("application/json")
            .Produces(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);

        group.MapGet("/courier/summary", GetCourierCashSummary)
            .WithName("GetCourierCashSummary")
            .WithSummary("Get courier daily cash collection summary")
            .RequireAuthorization()
            .Produces<CourierCashSummaryResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403);

        // === MERCHANT ENDPOINTS ===
        
        group.MapGet("/merchant/summary", GetMerchantCashSummary)
            .WithName("GetMerchantCashSummary")
            .WithSummary("Get merchant cash payment summary")
            .RequireAuthorization()
            .Produces<MerchantCashSummaryResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403);

        group.MapGet("/merchant/settlements", GetMerchantSettlements)
            .WithName("GetMerchantSettlements")
            .WithSummary("Get merchant settlement history")
            .RequireAuthorization()
            .Produces<PagedResult<SettlementResponse>>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403);

        // === ADMIN ENDPOINTS ===
        
        group.MapGet("/admin/cash-collections", GetAllCashPayments)
            .WithName("GetAllCashPayments")
            .WithSummary("Get all cash payments (admin only)")
            .RequireAuthorization()
            .Produces<PagedResult<PaymentResponse>>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403);

        group.MapPost("/admin/settlements/{merchantId:guid}/process", ProcessSettlement)
            .WithName("ProcessSettlement")
            .WithSummary("Process settlement for merchant (admin only)")
            .RequireAuthorization()
            .Accepts<ProcessSettlementRequest>("application/json")
            .Produces(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(403)
            .Produces<ProblemDetails>(404);
    }

    // === CUSTOMER HANDLERS ===

    private static async Task<IResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.CreatePaymentAsync(request, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetPaymentById(
        Guid paymentId,
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.GetPaymentByIdAsync(paymentId, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetOrderPayments(
        Guid orderId,
        [FromServices] IPaymentService paymentService,
        [FromServices] PaginationQuery query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.GetOrderPaymentsAsync(orderId, query, cancellationToken);
        return result.ToApiResult();
    }

    // === COURIER HANDLERS ===

    private static async Task<IResult> GetPendingCashPayments(
        [FromServices] IPaymentService paymentService,
        [FromServices] PaginationQuery query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var courierId = user.GetUserId();
        var result = await paymentService.GetPendingCashPaymentsAsync(courierId, query, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> CollectCashPayment(
        Guid paymentId,
        [FromBody] CollectCashPaymentRequest request,
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var courierId = user.GetUserId();
        var result = await paymentService.MarkCashPaymentAsCollectedAsync(paymentId, courierId, request, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> FailCashPayment(
        Guid paymentId,
        [FromBody] FailPaymentRequest request,
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var courierId = user.GetUserId();
        var result = await paymentService.MarkCashPaymentAsFailedAsync(paymentId, courierId, request.Reason, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetCourierCashSummary(
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        var courierId = user.GetUserId();
        var result = await paymentService.GetCourierCashSummaryAsync(courierId, date, cancellationToken);
        return result.ToApiResult();
    }

    // === MERCHANT HANDLERS ===

    private static async Task<IResult> GetMerchantCashSummary(
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        // TODO: Get merchant ID from user claims or merchant ownership
        var merchantId = Guid.NewGuid(); // Temporary - should get from user claims
        var result = await paymentService.GetMerchantCashSummaryAsync(merchantId, startDate, endDate, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetMerchantSettlements(
        [FromServices] IPaymentService paymentService,
        [FromServices] PaginationQuery query,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // TODO: Get merchant ID from user claims or merchant ownership
        var merchantId = Guid.NewGuid(); // Temporary - should get from user claims
        var result = await paymentService.GetMerchantSettlementsAsync(merchantId, query, cancellationToken);
        return result.ToApiResult();
    }

    // === ADMIN HANDLERS ===

    private static async Task<IResult> GetAllCashPayments(
        [FromServices] IPaymentService paymentService,
        [FromServices] PaginationQuery query,
        ClaimsPrincipal user,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.GetAllCashPaymentsAsync(query, status, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> ProcessSettlement(
        Guid merchantId,
        [FromBody] ProcessSettlementRequest request,
        [FromServices] IPaymentService paymentService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var adminId = user.GetUserId();
        var result = await paymentService.ProcessSettlementAsync(merchantId, request, adminId, cancellationToken);
        return result.ToApiResult();
    }
}

// === ADDITIONAL DTOs ===

public record FailPaymentRequest(string Reason);
