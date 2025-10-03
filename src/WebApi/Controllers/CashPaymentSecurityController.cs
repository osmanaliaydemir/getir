using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Cash payment security controller for cash payment security operations
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
    /// Create cash payment evidence
    /// </summary>
    /// <param name="request">Create evidence request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created evidence</returns>
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
            return Created($"/api/v1/cash-payment-security/evidence/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update cash payment evidence (Admin only)
    /// </summary>
    /// <param name="evidenceId">Evidence ID</param>
    /// <param name="request">Update evidence request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated evidence</returns>
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
    /// Get payment evidence
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged evidence</returns>
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
    /// Create cash payment security record
    /// </summary>
    /// <param name="request">Create security request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created security record</returns>
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
            return Created($"/api/v1/cash-payment-security/security/{result.Value.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Update cash payment security record
    /// </summary>
    /// <param name="securityId">Security ID</param>
    /// <param name="request">Update security request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated security record</returns>
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
    /// Get payment security
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Payment security</returns>
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
    /// Collect cash payment with security
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="request">Collect payment request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Calculate change
    /// </summary>
    /// <param name="request">Calculate change request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Change calculation</returns>
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
    /// Perform fake money check
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="request">Fake money check request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Check result</returns>
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
    /// Verify customer identity
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="request">Verify identity request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Verification result</returns>
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
    /// Assess security risk
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Risk assessment</returns>
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
    /// Get payments requiring approval
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged payments requiring approval</returns>
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
    /// Approve security record
    /// </summary>
    /// <param name="securityId">Security ID</param>
    /// <param name="request">Approve security request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
    /// Reject security record
    /// </summary>
    /// <param name="securityId">Security ID</param>
    /// <param name="request">Reject security request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
