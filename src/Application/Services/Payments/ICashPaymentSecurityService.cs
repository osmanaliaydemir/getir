using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Payments;

/// <summary>
/// Nakit ödeme güvenlik servisi
/// </summary>
public interface ICashPaymentSecurityService
{
    /// <summary>
    /// Nakit ödeme kanıtı oluştur
    /// </summary>
    Task<Result<CashPaymentEvidenceResponse>> CreateEvidenceAsync(CreateCashPaymentEvidenceRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Nakit ödeme kanıtını güncelle
    /// </summary>
    Task<Result<CashPaymentEvidenceResponse>> UpdateEvidenceAsync(Guid evidenceId, UpdateCashPaymentEvidenceRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Ödeme kanıtlarını getir
    /// </summary>
    Task<Result<PagedResult<CashPaymentEvidenceResponse>>> GetPaymentEvidenceAsync(Guid paymentId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>
    /// Nakit ödeme güvenlik kaydı oluştur
    /// </summary>
    Task<Result<CashPaymentSecurityResponse>> CreateSecurityRecordAsync(CreateCashPaymentSecurityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Nakit ödeme güvenlik kaydını güncelle
    /// </summary>
    Task<Result<CashPaymentSecurityResponse>> UpdateSecurityRecordAsync(Guid securityId, UpdateCashPaymentSecurityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Ödeme güvenlik kaydını getir
    /// </summary>
    Task<Result<CashPaymentSecurityResponse>> GetPaymentSecurityAsync(Guid paymentId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Güvenlik ile nakit ödeme topla
    /// </summary>
    Task<Result> CollectCashPaymentWithSecurityAsync(Guid paymentId, Guid courierId, CollectCashPaymentWithSecurityRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Para üstü hesapla
    /// </summary>
    Task<Result<CalculateChangeResponse>> CalculateChangeAsync(CalculateChangeRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Sahte para kontrolü yap
    /// </summary>
    Task<Result<bool>> PerformFakeMoneyCheckAsync(Guid paymentId, string notes, CancellationToken cancellationToken = default);
    /// <summary>
    /// Müşteri kimlik doğrulaması yap
    /// </summary>
    Task<Result<bool>> VerifyCustomerIdentityAsync(Guid paymentId, string identityType, string identityNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Güvenlik riski değerlendirmesi yap
    /// </summary>
    Task<Result<SecurityRiskLevel>> AssessSecurityRiskAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manuel onay gerektiren ödemeleri getir
    /// </summary>
    Task<Result<PagedResult<CashPaymentSecurityResponse>>> GetPaymentsRequiringApprovalAsync(PaginationQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Güvenlik kaydını onayla
    /// </summary>
    Task<Result> ApproveSecurityRecordAsync(Guid securityId, Guid adminId, string notes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Güvenlik kaydını reddet
    /// </summary>
    Task<Result> RejectSecurityRecordAsync(Guid securityId, Guid adminId, string reason, CancellationToken cancellationToken = default);
}
