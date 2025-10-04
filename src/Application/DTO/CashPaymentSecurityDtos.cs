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
    byte[]? FileData = null,
    string? FileName = null,
    string? MimeType = null,
    string? Description = null,
    double? Latitude = null,
    double? Longitude = null,
    string? DeviceInfo = null,
    EvidenceStatus? Status = null,
    string? VerificationNotes = null);

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
    decimal? GivenChange = null,
    decimal? CalculatedChange = null,
    bool? ChangeCalculationVerified = null,
    string? IdentityType = null,
    string? IdentityNumberHash = null,
    bool? IdentityVerified = null,
    bool? FakeMoneyCheckPerformed = null,
    bool? FakeMoneyDetected = null,
    string? SecurityNotes = null,
    SecurityRiskLevel? RiskLevel = null);

/// <summary>
/// Nakit ödeme güvenlik yanıtı
/// </summary>
public record CashPaymentSecurityResponse(
    Guid Id,
    Guid PaymentId,
    Guid CourierId,
    decimal GivenChange,
    decimal CalculatedChange,
    bool ChangeCalculationVerified,
    string IdentityType,
    bool IdentityVerified,
    bool FakeMoneyCheckPerformed,
    bool FakeMoneyDetected,
    string? SecurityNotes,
    SecurityRiskLevel RiskLevel,
    string Status,
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
