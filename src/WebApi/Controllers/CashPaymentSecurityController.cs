using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Nakit ödeme güvenlik işlemleri için nakit ödeme güvenlik controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Cash Payment Security")]
[Authorize]
public class CashPaymentSecurityController : BaseController
{
    private readonly ICashPaymentSecurityService _cashPaymentSecurityService;

    public CashPaymentSecurityController(ICashPaymentSecurityService cashPaymentSecurityService)
    {
        _cashPaymentSecurityService = cashPaymentSecurityService;
    }

    #region Evidence Endpoints

    /// <summary>
    /// Nakit ödeme kanıtı oluştur
    /// </summary>
    /// <param name="request">Kanıt oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan kanıt</returns>
    [HttpPost("evidence")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(typeof(CashPaymentEvidenceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCashPaymentEvidence(
        [FromBody] CreateCashPaymentEvidenceRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.CreateEvidenceAsync(request, ct);
        if (result.Success)
        {
            return Created($"/api/v1/cash-payment-security/evidence/{result.Value!.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Nakit ödeme kanıtını güncelle (Yalnızca Admin)
    /// </summary>
    /// <param name="evidenceId">Kanıt ID'si</param>
    /// <param name="request">Kanıt güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen kanıt</returns>
    [HttpPut("evidence/{evidenceId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CashPaymentEvidenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCashPaymentEvidence(
        [FromRoute] Guid evidenceId,
        [FromBody] UpdateCashPaymentEvidenceRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.UpdateEvidenceAsync(evidenceId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Ödeme kanıtını getir
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış kanıt</returns>
    [HttpGet("evidence/payment/{paymentId}")]
    [ProducesResponseType(typeof(PagedResult<CashPaymentEvidenceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaymentEvidence(
        [FromRoute] Guid paymentId,
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentSecurityService.GetPaymentEvidenceAsync(paymentId, query, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Security Endpoints

    /// <summary>
    /// Nakit ödeme güvenlik kaydı oluştur
    /// </summary>
    /// <param name="request">Güvenlik oluşturma talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Oluşturulan güvenlik kaydı</returns>
    [HttpPost("security")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(typeof(CashPaymentSecurityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCashPaymentSecurity(
        [FromBody] CreateCashPaymentSecurityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.CreateSecurityRecordAsync(request, ct);
        if (result.Success)
        {
            return Created($"/api/v1/cash-payment-security/security/{result.Value!.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Nakit ödeme güvenlik kaydını güncelle
    /// </summary>
    /// <param name="securityId">Güvenlik ID'si</param>
    /// <param name="request">Güvenlik güncelleme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Güncellenen güvenlik kaydı</returns>
    [HttpPut("security/{securityId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CashPaymentSecurityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCashPaymentSecurity(
        [FromRoute] Guid securityId,
        [FromBody] UpdateCashPaymentSecurityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.UpdateSecurityRecordAsync(securityId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Ödeme güvenliğini getir
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Ödeme güvenliği</returns>
    [HttpGet("security/payment/{paymentId}")]
    [ProducesResponseType(typeof(CashPaymentSecurityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentSecurity(
        [FromRoute] Guid paymentId,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentSecurityService.GetPaymentSecurityAsync(paymentId, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Payment Collection with Security

    /// <summary>
    /// Güvenlik ile nakit ödeme topla
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="request">Ödeme toplama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("collect/{paymentId}")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CollectCashPaymentWithSecurity(
        [FromRoute] Guid paymentId,
        [FromBody] CollectCashPaymentWithSecurityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cashPaymentSecurityService.CollectCashPaymentWithSecurityAsync(paymentId, courierId, request, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Utility Endpoints

    /// <summary>
    /// Para üstü hesapla
    /// </summary>
    /// <param name="request">Para üstü hesaplama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Para üstü hesaplaması</returns>
    [HttpPost("calculate-change")]
    [ProducesResponseType(typeof(CalculateChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateChange(
        [FromBody] CalculateChangeRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.CalculateChangeAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sahte para kontrolü yap
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="request">Sahte para kontrolü talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kontrol sonucu</returns>
    [HttpPost("fake-money-check/{paymentId}")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PerformFakeMoneyCheck(
        [FromRoute] Guid paymentId,
        [FromBody] FakeMoneyCheckRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.PerformFakeMoneyCheckAsync(paymentId, request.Notes, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Müşteri kimliğini doğrula
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="request">Kimlik doğrulama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("verify-identity/{paymentId}")]
    [Authorize(Roles = "Courier")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCustomerIdentity(
        [FromRoute] Guid paymentId,
        [FromBody] VerifyIdentityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _cashPaymentSecurityService.VerifyCustomerIdentityAsync(paymentId, request.IdentityType, request.IdentityNumber, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Güvenlik riskini değerlendir
    /// </summary>
    /// <param name="paymentId">Ödeme ID'si</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Risk değerlendirmesi</returns>
    [HttpPost("assess-risk/{paymentId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SecurityRiskLevel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssessSecurityRisk(
        [FromRoute] Guid paymentId,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentSecurityService.AssessSecurityRiskAsync(paymentId, ct);
        return ToActionResult(result);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Onay gerektiren ödemeleri getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Onay gerektiren sayfalanmış ödemeler</returns>
    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<CashPaymentSecurityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaymentsRequiringApproval(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _cashPaymentSecurityService.GetPaymentsRequiringApprovalAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Güvenlik kaydını onayla
    /// </summary>
    /// <param name="securityId">Güvenlik ID'si</param>
    /// <param name="request">Güvenlik onaylama talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("approve/{securityId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveSecurityRecord(
        [FromRoute] Guid securityId,
        [FromBody] ApproveSecurityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cashPaymentSecurityService.ApproveSecurityRecordAsync(securityId, adminId, request.Notes, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Güvenlik kaydını reddet
    /// </summary>
    /// <param name="securityId">Güvenlik ID'si</param>
    /// <param name="request">Güvenlik reddetme talebi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("reject/{securityId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectSecurityRecord(
        [FromRoute] Guid securityId,
        [FromBody] RejectSecurityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _cashPaymentSecurityService.RejectSecurityRecordAsync(securityId, adminId, request.Reason, ct);
        return ToActionResult(result);
    }

    #endregion
}

// Request DTOs
public record FakeMoneyCheckRequest(string Notes);
public record VerifyIdentityRequest(string IdentityType, string IdentityNumber);
public record ApproveSecurityRequest(string Notes);
public record RejectSecurityRequest(string Reason);
