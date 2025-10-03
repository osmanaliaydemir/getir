using Getir.Domain.Enums;

namespace Getir.Application.DTO;

/// <summary>
/// Nakit ödeme kanıtı oluşturma isteği
/// </summary>
public record CreateCashPaymentEvidenceRequest(
    Guid PaymentId,
    EvidenceType EvidenceType,
    string FileUrl,
    long FileSize,
    string MimeType,
    string FileHash,
    string? Description,
    double? Latitude,
    double? Longitude,
    string? DeviceInfo);

/// <summary>
/// Nakit ödeme kanıtı güncelleme isteği
/// </summary>
public record UpdateCashPaymentEvidenceRequest(
    EvidenceStatus Status,
    string? VerificationNotes);

/// <summary>
/// Nakit ödeme kanıtı yanıtı
/// </summary>
public record CashPaymentEvidenceResponse(
    Guid Id,
    Guid PaymentId,
    Guid CourierId,
    EvidenceType EvidenceType,
    string FileUrl,
    long FileSize,
    string MimeType,
    string FileHash,
    string? Description,
    double? Latitude,
    double? Longitude,
    string? DeviceInfo,
    EvidenceStatus Status,
    string? VerificationNotes,
    Guid? VerifiedByAdminId,
    DateTime? VerifiedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Nakit ödeme güvenlik oluşturma isteği
/// </summary>
public record CreateCashPaymentSecurityRequest(
    Guid PaymentId,
    decimal CalculatedChange,
    decimal GivenChange,
    bool FakeMoneyCheckPerformed,
    bool FakeMoneyDetected,
    string? FakeMoneyNotes,
    bool CustomerIdentityVerified,
    string? IdentityVerificationType,
    string? IdentityNumberHash,
    string? SecurityNotes);

/// <summary>
/// Nakit ödeme güvenlik güncelleme isteği
/// </summary>
public record UpdateCashPaymentSecurityRequest(
    SecurityRiskLevel RiskLevel,
    string? RiskFactors,
    string? SecurityNotes,
    bool RequiresManualApproval);

/// <summary>
/// Nakit ödeme güvenlik yanıtı
/// </summary>
public record CashPaymentSecurityResponse(
    Guid Id,
    Guid PaymentId,
    bool ChangeCalculationVerified,
    decimal CalculatedChange,
    decimal GivenChange,
    decimal ChangeDifference,
    bool FakeMoneyCheckPerformed,
    bool FakeMoneyDetected,
    string? FakeMoneyNotes,
    bool CustomerIdentityVerified,
    string? IdentityVerificationType,
    string? IdentityNumberHash,
    SecurityRiskLevel RiskLevel,
    string? RiskFactors,
    string? SecurityNotes,
    bool RequiresManualApproval,
    Guid? ApprovedByAdminId,
    DateTime? ApprovedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Nakit ödeme toplama isteği (güvenlik ile)
/// </summary>
public record CollectCashPaymentWithSecurityRequest(
    decimal CollectedAmount,
    decimal GivenChange,
    bool FakeMoneyCheckPerformed,
    bool FakeMoneyDetected,
    string? FakeMoneyNotes,
    bool CustomerIdentityVerified,
    string? IdentityVerificationType,
    string? IdentityNumberHash,
    string? Notes,
    List<CreateCashPaymentEvidenceRequest> EvidenceList);

/// <summary>
/// Para üstü hesaplama isteği
/// </summary>
public record CalculateChangeRequest(
    decimal OrderAmount,
    decimal GivenAmount);

/// <summary>
/// Para üstü hesaplama yanıtı
/// </summary>
public record CalculateChangeResponse(
    decimal OrderAmount,
    decimal GivenAmount,
    decimal ChangeAmount,
    bool IsValid,
    string? ValidationMessage);
