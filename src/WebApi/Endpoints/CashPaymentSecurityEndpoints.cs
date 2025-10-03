using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Payments;
using Getir.Domain.Enums;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

/// <summary>
/// Nakit ödeme güvenlik endpoint'leri
/// </summary>
public static class CashPaymentSecurityEndpoints
{
    public static void MapCashPaymentSecurityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/cash-payment-security")
            .WithTags("Cash Payment Security")
            .RequireAuthorization();

        // === EVIDENCE ENDPOINTS ===

        // Kanıt oluştur
        group.MapPost("/evidence", async (
            [FromBody] CreateCashPaymentEvidenceRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateEvidenceAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateCashPaymentEvidence")
        .Produces<CashPaymentEvidenceResponse>(201)
        .Produces(400)
        .RequireAuthorization("Courier");

        // Kanıt güncelle (Admin only)
        group.MapPut("/evidence/{evidenceId}", async (
            [FromRoute] Guid evidenceId,
            [FromBody] UpdateCashPaymentEvidenceRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.UpdateEvidenceAsync(evidenceId, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateCashPaymentEvidence")
        .Produces<CashPaymentEvidenceResponse>(200)
        .Produces(400)
        .RequireAuthorization("Admin");

        // Ödeme kanıtlarını getir
        group.MapGet("/evidence/payment/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [AsParameters] PaginationQuery query,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.GetPaymentEvidenceAsync(paymentId, query, ct);
            return result.ToIResult();
        })
        .WithName("GetPaymentEvidence")
        .Produces<PagedResult<CashPaymentEvidenceResponse>>(200)
        .Produces(400);

        // === SECURITY ENDPOINTS ===

        // Güvenlik kaydı oluştur
        group.MapPost("/security", async (
            [FromBody] CreateCashPaymentSecurityRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateSecurityRecordAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateCashPaymentSecurity")
        .Produces<CashPaymentSecurityResponse>(201)
        .Produces(400)
        .RequireAuthorization("Courier");

        // Güvenlik kaydını güncelle
        group.MapPut("/security/{securityId}", async (
            [FromRoute] Guid securityId,
            [FromBody] UpdateCashPaymentSecurityRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.UpdateSecurityRecordAsync(securityId, request, ct);
            return result.ToIResult();
        })
        .WithName("UpdateCashPaymentSecurity")
        .Produces<CashPaymentSecurityResponse>(200)
        .Produces(400)
        .RequireAuthorization("Admin");

        // Ödeme güvenlik kaydını getir
        group.MapGet("/security/payment/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.GetPaymentSecurityAsync(paymentId, ct);
            return result.ToIResult();
        })
        .WithName("GetPaymentSecurity")
        .Produces<CashPaymentSecurityResponse>(200)
        .Produces(400);

        // === PAYMENT COLLECTION WITH SECURITY ===

        // Güvenlik ile nakit ödeme topla
        group.MapPost("/collect/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [FromBody] CollectCashPaymentWithSecurityRequest request,
            [FromServices] ICashPaymentSecurityService service,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var courierId = user.GetUserId();
            var result = await service.CollectCashPaymentWithSecurityAsync(paymentId, courierId, request, ct);
            return result.ToIResult();
        })
        .WithName("CollectCashPaymentWithSecurity")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("Courier");

        // === UTILITY ENDPOINTS ===

        // Para üstü hesapla
        group.MapPost("/calculate-change", async (
            [FromBody] CalculateChangeRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.CalculateChangeAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CalculateChange")
        .Produces<CalculateChangeResponse>(200)
        .Produces(400);

        // Sahte para kontrolü yap
        group.MapPost("/fake-money-check/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [FromBody] FakeMoneyCheckRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.PerformFakeMoneyCheckAsync(paymentId, request.Notes, ct);
            return result.ToIResult();
        })
        .WithName("PerformFakeMoneyCheck")
        .Produces<bool>(200)
        .Produces(400)
        .RequireAuthorization("Courier");

        // Müşteri kimlik doğrulaması yap
        group.MapPost("/verify-identity/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [FromBody] VerifyIdentityRequest request,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.VerifyCustomerIdentityAsync(paymentId, request.IdentityType, request.IdentityNumber, ct);
            return result.ToIResult();
        })
        .WithName("VerifyCustomerIdentity")
        .Produces<bool>(200)
        .Produces(400)
        .RequireAuthorization("Courier");

        // Güvenlik riski değerlendirmesi yap
        group.MapPost("/assess-risk/{paymentId}", async (
            [FromRoute] Guid paymentId,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.AssessSecurityRiskAsync(paymentId, ct);
            return result.ToIResult();
        })
        .WithName("AssessSecurityRisk")
        .Produces<SecurityRiskLevel>(200)
        .Produces(400)
        .RequireAuthorization("Admin");

        // === ADMIN ENDPOINTS ===

        // Manuel onay gerektiren ödemeleri getir
        group.MapGet("/pending-approvals", async (
            [AsParameters] PaginationQuery query,
            [FromServices] ICashPaymentSecurityService service,
            CancellationToken ct) =>
        {
            var result = await service.GetPaymentsRequiringApprovalAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetPaymentsRequiringApproval")
        .Produces<PagedResult<CashPaymentSecurityResponse>>(200)
        .Produces(400)
        .RequireAuthorization("Admin");

        // Güvenlik kaydını onayla
        group.MapPost("/approve/{securityId}", async (
            [FromRoute] Guid securityId,
            [FromBody] ApproveSecurityRequest request,
            [FromServices] ICashPaymentSecurityService service,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var adminId = user.GetUserId();
            var result = await service.ApproveSecurityRecordAsync(securityId, adminId, request.Notes, ct);
            return result.ToIResult();
        })
        .WithName("ApproveSecurityRecord")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("Admin");

        // Güvenlik kaydını reddet
        group.MapPost("/reject/{securityId}", async (
            [FromRoute] Guid securityId,
            [FromBody] RejectSecurityRequest request,
            [FromServices] ICashPaymentSecurityService service,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var adminId = user.GetUserId();
            var result = await service.RejectSecurityRecordAsync(securityId, adminId, request.Reason, ct);
            return result.ToIResult();
        })
        .WithName("RejectSecurityRecord")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("Admin");
    }
}

// Request DTO'ları
public record FakeMoneyCheckRequest(string Notes);
public record VerifyIdentityRequest(string IdentityType, string IdentityNumber);
public record ApproveSecurityRequest(string Notes);
public record RejectSecurityRequest(string Reason);
