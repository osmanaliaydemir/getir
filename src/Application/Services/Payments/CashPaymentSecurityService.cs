using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Common.Exceptions;
using Getir.Application.Common.Extensions;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Getir.Application.Services.Payments;

/// <summary>
/// Nakit ödeme güvenlik servisi implementasyonu
/// </summary>
public class CashPaymentSecurityService : BaseService, ICashPaymentSecurityService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ICashPaymentAuditService _auditService;

    public CashPaymentSecurityService(
        IUnitOfWork unitOfWork,
        ILogger<CashPaymentSecurityService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IFileStorageService fileStorageService,
        ICashPaymentAuditService auditService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _fileStorageService = fileStorageService;
        _auditService = auditService;
    }

    public async Task<Result<CashPaymentEvidenceResponse>> CreateEvidenceAsync(
        CreateCashPaymentEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.Repository<Payment>()
                .GetByIdAsync(request.PaymentId, cancellationToken);

            if (payment == null)
            {
                return Result.Fail<CashPaymentEvidenceResponse>("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Dosya hash'ini doğrula
            var calculatedHash = await CalculateFileHashAsync(request.FileUrl);
            if (calculatedHash != request.FileHash)
            {
                return Result.Fail<CashPaymentEvidenceResponse>("File hash mismatch", "INVALID_FILE_HASH");
            }

            var evidence = new CashPaymentEvidence
            {
                Id = Guid.NewGuid(),
                PaymentId = request.PaymentId,
                CourierId = payment.CollectedByCourierId ?? Guid.Empty,
                EvidenceType = request.EvidenceType,
                FileUrl = request.FileUrl,
                FileSize = request.FileSize,
                MimeType = request.MimeType,
                FileHash = request.FileHash,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                DeviceInfo = request.DeviceInfo,
                Status = EvidenceStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<CashPaymentEvidence>().AddAsync(evidence, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log oluştur
            await CreateAuditLogAsync(
                request.PaymentId,
                evidence.CourierId,
                AuditEventType.EvidenceCreated,
                AuditSeverityLevel.Information,
                "Kanıt Oluşturuldu",
                $"Nakit ödeme kanıtı oluşturuldu. Tip: {request.EvidenceType}",
                cancellationToken: cancellationToken);

            var response = new CashPaymentEvidenceResponse(
                evidence.Id,
                evidence.PaymentId,
                evidence.CourierId,
                evidence.EvidenceType,
                evidence.FileUrl,
                evidence.FileSize,
                evidence.MimeType,
                evidence.FileHash,
                evidence.Description,
                evidence.Latitude,
                evidence.Longitude,
                evidence.DeviceInfo,
                evidence.Status,
                evidence.VerificationNotes,
                evidence.VerifiedByAdminId,
                evidence.VerifiedAt,
                evidence.CreatedAt,
                evidence.UpdatedAt
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CreateEvidence", new { PaymentId = request.PaymentId, EvidenceType = request.EvidenceType });
            return Result.Fail<CashPaymentEvidenceResponse>("Failed to create evidence", "CREATE_EVIDENCE_FAILED");
        }
    }

    public async Task<Result<CashPaymentSecurityResponse>> CreateSecurityRecordAsync(
        CreateCashPaymentSecurityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.Repository<Payment>()
                .GetByIdAsync(request.PaymentId, cancellationToken);

            if (payment == null)
            {
                return Result.Fail<CashPaymentSecurityResponse>("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Para üstü hesaplama doğruluğunu kontrol et
            var changeDifference = request.GivenChange - request.CalculatedChange;
            var changeCalculationVerified = Math.Abs(changeDifference) <= 0.01m; // 1 kuruş tolerans

            // Kimlik numarasını hash'le
            string? identityNumberHash = null;
            if (!string.IsNullOrEmpty(request.IdentityNumberHash))
            {
                identityNumberHash = HashIdentityNumber(request.IdentityNumberHash);
            }

            var security = new CashPaymentSecurity
            {
                Id = Guid.NewGuid(),
                PaymentId = request.PaymentId,
                ChangeCalculationVerified = changeCalculationVerified,
                CalculatedChange = request.CalculatedChange,
                GivenChange = request.GivenChange,
                ChangeDifference = changeDifference,
                FakeMoneyCheckPerformed = request.FakeMoneyCheckPerformed,
                FakeMoneyDetected = request.FakeMoneyDetected,
                FakeMoneyNotes = request.FakeMoneyNotes,
                CustomerIdentityVerified = request.CustomerIdentityVerified,
                IdentityVerificationType = request.IdentityVerificationType,
                IdentityNumberHash = identityNumberHash,
                RiskLevel = SecurityRiskLevel.Low, // Başlangıçta düşük risk
                SecurityNotes = request.SecurityNotes,
                RequiresManualApproval = false, // Risk değerlendirmesi sonrası belirlenecek
                CreatedAt = DateTime.UtcNow
            };

            // Risk değerlendirmesi yap
            security.RiskLevel = await AssessRiskLevelAsync(security);
            security.RequiresManualApproval = security.RiskLevel >= SecurityRiskLevel.High;

            await _unitOfWork.Repository<CashPaymentSecurity>().AddAsync(security, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log oluştur
            await CreateAuditLogAsync(
                request.PaymentId,
                null,
                AuditEventType.SecurityRiskDetected,
                security.RiskLevel >= SecurityRiskLevel.High ? AuditSeverityLevel.Warning : AuditSeverityLevel.Information,
                "Güvenlik Kaydı Oluşturuldu",
                $"Nakit ödeme güvenlik kaydı oluşturuldu. Risk seviyesi: {security.RiskLevel}",
                security.RiskLevel,
                cancellationToken: cancellationToken);

            var response = new CashPaymentSecurityResponse(
                security.Id,
                security.PaymentId,
                security.ChangeCalculationVerified,
                security.CalculatedChange,
                security.GivenChange,
                security.ChangeDifference,
                security.FakeMoneyCheckPerformed,
                security.FakeMoneyDetected,
                security.FakeMoneyNotes,
                security.CustomerIdentityVerified,
                security.IdentityVerificationType,
                security.IdentityNumberHash,
                security.RiskLevel,
                security.RiskFactors,
                security.SecurityNotes,
                security.RequiresManualApproval,
                security.ApprovedByAdminId,
                security.ApprovedAt,
                security.CreatedAt,
                security.UpdatedAt
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CreateSecurityRecord", new { PaymentId = request.PaymentId });
            return Result.Fail<CashPaymentSecurityResponse>("Failed to create security record", "CREATE_SECURITY_RECORD_FAILED");
        }
    }

    public async Task<Result<CalculateChangeResponse>> CalculateChangeAsync(
        CalculateChangeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var changeAmount = request.GivenAmount - request.OrderAmount;
            var isValid = changeAmount >= 0;

            var response = new CalculateChangeResponse(
                request.OrderAmount,
                request.GivenAmount,
                changeAmount,
                isValid,
                isValid ? null : "Verilen miktar sipariş tutarından az olamaz"
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CalculateChange", new { OrderAmount = request.OrderAmount, GivenAmount = request.GivenAmount });
            return Result.Fail<CalculateChangeResponse>("Failed to calculate change", "CALCULATE_CHANGE_FAILED");
        }
    }

    public async Task<Result> CollectCashPaymentWithSecurityAsync(
        Guid paymentId,
        Guid courierId,
        CollectCashPaymentWithSecurityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.Repository<Payment>()
                .GetByIdAsync(paymentId, cancellationToken);

            if (payment == null)
            {
                return Result.Fail("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Para üstü hesapla
            var changeCalculation = await CalculateChangeAsync(
                new CalculateChangeRequest(payment.Amount, request.CollectedAmount),
                cancellationToken);

            if (!changeCalculation.Success)
            {
                return Result.Fail("Failed to calculate change", "CALCULATE_CHANGE_FAILED");
            }

            // Güvenlik kaydı oluştur
            var securityRequest = new CreateCashPaymentSecurityRequest(
                paymentId,
                changeCalculation.Value.ChangeAmount,
                request.GivenChange,
                request.FakeMoneyCheckPerformed,
                request.FakeMoneyDetected,
                request.FakeMoneyNotes,
                request.CustomerIdentityVerified,
                request.IdentityVerificationType,
                request.IdentityNumberHash,
                request.Notes
            );

            var securityResult = await CreateSecurityRecordAsync(securityRequest, cancellationToken);
            if (!securityResult.Success)
            {
                return Result.Fail("Failed to create security record", "CREATE_SECURITY_RECORD_FAILED");
            }

            // Kanıtları oluştur
            foreach (var evidenceRequest in request.EvidenceList)
            {
                var evidenceResult = await CreateEvidenceAsync(evidenceRequest, cancellationToken);
                if (!evidenceResult.Success)
                {
                    _logger.LogWarning("Failed to create evidence for payment {PaymentId}", paymentId);
                }
            }

            // Payment'ı güncelle
            payment.Status = PaymentStatus.Completed;
            payment.CollectedAt = DateTime.UtcNow;
            payment.CollectedByCourierId = courierId;
            payment.ChangeAmount = request.GivenChange;
            payment.Notes = request.Notes;
            payment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CollectCashPaymentWithSecurity", new { PaymentId = paymentId, CourierId = courierId });
            return Result.Fail("Failed to collect cash payment with security", "COLLECT_CASH_PAYMENT_FAILED");
        }
    }

    public async Task<Result<SecurityRiskLevel>> AssessSecurityRiskAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var security = await _unitOfWork.Repository<CashPaymentSecurity>()
                .GetAsync(s => s.PaymentId == paymentId, cancellationToken: cancellationToken);

            if (security == null)
            {
                return Result.Fail<SecurityRiskLevel>("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            var riskLevel = await AssessRiskLevelAsync(security);
            return Result.Ok(riskLevel);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "AssessSecurityRisk", new { PaymentId = paymentId });
            return Result.Fail<SecurityRiskLevel>("Failed to assess security risk", "ASSESS_SECURITY_RISK_FAILED");
        }
    }

    // Private helper methods
    private async Task<string> CalculateFileHashAsync(string fileUrl)
    {
        try
        {
            // Şimdilik basit hash hesaplama - dosya URL'ini hash'le
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fileUrl));
            return Convert.ToBase64String(hashBytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string HashIdentityNumber(string identityNumber)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(identityNumber));
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<SecurityRiskLevel> AssessRiskLevelAsync(CashPaymentSecurity security)
    {
        var riskFactors = new List<string>();
        var riskScore = 0;

        // Para üstü hesaplama hatası
        if (!security.ChangeCalculationVerified)
        {
            riskScore += 2;
            riskFactors.Add("Para üstü hesaplama hatası");
        }

        // Sahte para tespit edildi
        if (security.FakeMoneyDetected)
        {
            riskScore += 4;
            riskFactors.Add("Sahte para tespit edildi");
        }

        // Kimlik doğrulaması yapılmadı
        if (!security.CustomerIdentityVerified)
        {
            riskScore += 1;
            riskFactors.Add("Müşteri kimlik doğrulaması yapılmadı");
        }

        // Büyük tutar (1000 TL üzeri)
        var payment = await _unitOfWork.Repository<Payment>()
            .GetByIdAsync(security.PaymentId);
        if (payment != null && payment.Amount > 1000)
        {
            riskScore += 1;
            riskFactors.Add("Büyük tutar ödemesi");
        }

        // Risk seviyesini belirle
        var riskLevel = riskScore switch
        {
            <= 1 => SecurityRiskLevel.Low,
            <= 3 => SecurityRiskLevel.Medium,
            <= 5 => SecurityRiskLevel.High,
            _ => SecurityRiskLevel.Critical
        };

        // Risk faktörlerini kaydet
        security.RiskFactors = string.Join(", ", riskFactors);
        security.RequiresManualApproval = riskLevel >= SecurityRiskLevel.High;

        return riskLevel;
    }

    private async Task CreateAuditLogAsync(
        Guid paymentId,
        Guid? courierId,
        AuditEventType eventType,
        AuditSeverityLevel severityLevel,
        string title,
        string description,
        SecurityRiskLevel? riskLevel = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditRequest = new CreateAuditLogRequest(
                paymentId,
                courierId,
                null, // customerId
                null, // adminId
                eventType,
                severityLevel,
                title,
                description,
                details,
                riskLevel
            );

            await _auditService.CreateAuditLogAsync(auditRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create audit log for payment {PaymentId}", paymentId);
            // Audit log hatası ana işlemi etkilememeli
        }
    }

    // Diğer metodlar için placeholder'lar (implementasyon devam edecek)
    public Task<Result<CashPaymentEvidenceResponse>> UpdateEvidenceAsync(Guid evidenceId, UpdateCashPaymentEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedResult<CashPaymentEvidenceResponse>>> GetPaymentEvidenceAsync(Guid paymentId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CashPaymentSecurityResponse>> UpdateSecurityRecordAsync(Guid securityId, UpdateCashPaymentSecurityRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CashPaymentSecurityResponse>> GetPaymentSecurityAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> PerformFakeMoneyCheckAsync(Guid paymentId, string notes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> VerifyCustomerIdentityAsync(Guid paymentId, string identityType, string identityNumber, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedResult<CashPaymentSecurityResponse>>> GetPaymentsRequiringApprovalAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ApproveSecurityRecordAsync(Guid securityId, Guid adminId, string notes, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> RejectSecurityRecordAsync(Guid securityId, Guid adminId, string reason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
