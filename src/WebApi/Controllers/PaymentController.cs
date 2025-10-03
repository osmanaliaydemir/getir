using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Payment controller for managing payments
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Payments")]
public class PaymentController : BaseController
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    #region Customer Endpoints

    /// <summary>
    /// Create a new payment
    /// </summary>
    /// <param name="request">Create payment request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created payment</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _paymentService.CreatePaymentAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get payment details by ID
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Payment details</returns>
    [HttpGet("{paymentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(
        [FromRoute] Guid paymentId,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetPaymentByIdAsync(paymentId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get all payments for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged payments</returns>
    [HttpGet("order/{orderId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderPayments(
        [FromRoute] Guid orderId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetOrderPaymentsAsync(orderId, query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Courier Endpoints

    /// <summary>
    /// Get pending cash payments for courier
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged pending payments</returns>
    [HttpGet("courier/pending")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingCashPayments(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetPendingCashPaymentsAsync(courierId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mark cash payment as collected
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="request">Collect payment request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("courier/{paymentId:guid}/collect")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CollectCashPayment(
        [FromRoute] Guid paymentId,
        [FromBody] CollectCashPaymentRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.MarkCashPaymentAsCollectedAsync(paymentId, courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mark cash payment as failed
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="request">Fail payment request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("courier/{paymentId:guid}/fail")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FailCashPayment(
        [FromRoute] Guid paymentId,
        [FromBody] FailPaymentRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.MarkCashPaymentAsFailedAsync(paymentId, courierId, request.Reason, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get courier daily cash collection summary
    /// </summary>
    /// <param name="date">Date (optional, defaults to today)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cash summary</returns>
    [HttpGet("courier/summary")]
    [Authorize]
    [ProducesResponseType(typeof(CourierCashSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCourierCashSummary(
        [FromQuery] DateTime? date = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetCourierCashSummaryAsync(courierId, date, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Merchant Endpoints

    /// <summary>
    /// Get merchant cash payment summary
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Cash summary</returns>
    [HttpGet("merchant/summary")]
    [Authorize]
    [ProducesResponseType(typeof(MerchantCashSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMerchantCashSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        // TODO: Get merchant ID from user claims or merchant ownership
        var merchantId = Guid.NewGuid(); // Temporary - should get from user claims
        var result = await _paymentService.GetMerchantCashSummaryAsync(merchantId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get merchant settlement history
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged settlements</returns>
    [HttpGet("merchant/settlements")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<SettlementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMerchantSettlements(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        // TODO: Get merchant ID from user claims or merchant ownership
        var merchantId = Guid.NewGuid(); // Temporary - should get from user claims
        var result = await _paymentService.GetMerchantSettlementsAsync(merchantId, query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all cash payments (admin only)
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="status">Payment status filter</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged cash payments</returns>
    [HttpGet("admin/cash-collections")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllCashPayments(
        [FromQuery] PaginationQuery query,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _paymentService.GetAllCashPaymentsAsync(query, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Process settlement for merchant (admin only)
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="request">Process settlement request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("admin/settlements/{merchantId:guid}/process")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessSettlement(
        [FromRoute] Guid merchantId,
        [FromBody] ProcessSettlementRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.ProcessSettlementAsync(merchantId, request, adminId, ct);
        return ToActionResult(result);
    }

    #endregion
}

// Additional DTOs for payment operations
public record FailPaymentRequest(string Reason);
