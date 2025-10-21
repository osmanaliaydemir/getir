using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Ödemeleri yönetmek için ödeme controller'ı
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
    /// Yeni ödeme oluştur
    /// </summary>
    /// <param name="request">Ödeme oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan ödeme</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _paymentService.CreatePaymentAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// ID'ye göre ödeme detaylarını getir
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Ödeme detayları</returns>
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
    /// Bir siparişin tüm ödemelerini getir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış ödemeler</returns>
    [HttpGet("order/{orderId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderPayments([FromRoute] Guid orderId, [FromQuery] PaginationQuery query, CancellationToken ct = default)
    {
        var result = await _paymentService.GetOrderPaymentsAsync(orderId, query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Courier Endpoints

    /// <summary>
    /// Kurye için bekleyen nakit ödemeleri getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış bekleyen ödemeler</returns>
    [HttpGet("courier/pending")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPendingCashPayments([FromQuery] PaginationQuery query, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetPendingCashPaymentsAsync(courierId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Nakit ödemeyi tahsil edildi olarak işaretle
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="request">Tahsilat isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("courier/{paymentId:guid}/collect")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CollectCashPayment([FromRoute] Guid paymentId, [FromBody] CollectCashPaymentRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.MarkCashPaymentAsCollectedAsync(paymentId, courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Nakit ödemeyi başarısız olarak işaretle
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="request">Başarısız ödeme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("courier/{paymentId:guid}/fail")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FailCashPayment([FromRoute] Guid paymentId, [FromBody] FailPaymentRequest request, CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.MarkCashPaymentAsFailedAsync(paymentId, courierId, request.Reason, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Kurye günlük nakit tahsilat özetini getir
    /// </summary>
    /// <param name="date">Tarih (isteğe bağlı, varsayılan bugün)</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Nakit özeti</returns>
    [HttpGet("courier/summary")]
    [Authorize]
    [ProducesResponseType(typeof(CourierCashSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCourierCashSummary([FromQuery] DateTime? date = null, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetCourierCashSummaryAsync(courierId, date, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Merchant Endpoints

    /// <summary>
    /// Mağaza ödeme geçmişini getir (tüm ödeme yöntemleri)
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="paymentMethod">Ödeme yöntemi filtresi</param>
    /// <param name="status">Ödeme durumu filtresi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış ödeme listesi</returns>
    [HttpGet("merchant/{merchantId:guid}/transactions")]
    [Authorize]
    [Authorize(Roles = "MerchantOwner,Admin")]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantPayments([FromRoute] Guid merchantId, [FromQuery] PaginationQuery query, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] PaymentMethod? paymentMethod = null, [FromQuery] PaymentStatus? status = null, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetMerchantPaymentsAsync(merchantId, query, startDate, endDate, paymentMethod, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza nakit ödeme özetini getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="startDate">Başlangıç tarihi</param>
    /// <param name="endDate">Bitiş tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Nakit özeti</returns>
    [HttpGet("merchant/{merchantId:guid}/summary")]
    [Authorize]
    [Authorize(Roles = "MerchantOwner,Admin")]
    [ProducesResponseType(typeof(MerchantCashSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantCashSummary([FromRoute] Guid merchantId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetMerchantCashSummaryAsync(merchantId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza mutabakat geçmişini getir
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış mutabakatlar</returns>
    [HttpGet("merchant/{merchantId:guid}/settlements")]
    [Authorize]
    [Authorize(Roles = "MerchantOwner,Admin")]
    [ProducesResponseType(typeof(PagedResult<SettlementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMerchantSettlements([FromRoute] Guid merchantId, [FromQuery] PaginationQuery query, CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _paymentService.GetMerchantSettlementsAsync(merchantId, query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Tüm nakit ödemeleri getir (sadece admin)
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="status">Ödeme durumu filtresi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış nakit ödemeler</returns>
    [HttpGet("admin/cash-collections")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<PaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllCashPayments([FromQuery] PaginationQuery query, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        var result = await _paymentService.GetAllCashPaymentsAsync(query, status, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza için mutabakatı işle (sadece admin)
    /// </summary>
    /// <param name="merchantId">Mağaza ID'si</param>
    /// <param name="request">Mutabakat işleme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("admin/settlements/{merchantId:guid}/process")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessSettlement([FromRoute] Guid merchantId, [FromBody] ProcessSettlementRequest request, CancellationToken ct = default)
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
