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
    public CashPaymentSecurityService(IUnitOfWork unitOfWork, ILogger<CashPaymentSecurityService> logger, ILoggingService loggingService, ICacheService cacheService, IFileStorageService fileStorageService, ICashPaymentAuditService auditService)
    : base(unitOfWork, logger, loggingService, cacheService)
    {
        _fileStorageService = fileStorageService;
        _auditService = auditService;
    }
    public async Task<Result<CashPaymentEvidenceResponse>> CreateEvidenceAsync(CreateCashPaymentEvidenceRequest request, CancellationToken cancellationToken = default)
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
    public async Task<Result<CashPaymentSecurityResponse>> CreateSecurityRecordAsync(CreateCashPaymentSecurityRequest request, CancellationToken cancellationToken = default)
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
                security.Payment.CollectedByCourierId ?? Guid.Empty, // CourierId
                security.GivenChange,
                security.CalculatedChange,
                security.ChangeCalculationVerified,
                security.IdentityVerificationType ?? "", // IdentityType
                security.CustomerIdentityVerified, // IdentityVerified
                security.FakeMoneyCheckPerformed,
                security.FakeMoneyDetected,
                security.SecurityNotes ?? "", // SecurityNotes
                security.RiskLevel,
                security.RequiresManualApproval ? "Pending" : "Approved", // Status
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
    public async Task<Result<CalculateChangeResponse>> CalculateChangeAsync(CalculateChangeRequest request, CancellationToken cancellationToken = default)
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
    public async Task<Result> CollectCashPaymentWithSecurityAsync(Guid paymentId, Guid courierId, CollectCashPaymentWithSecurityRequest request, CancellationToken cancellationToken = default)
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
    public async Task<Result<SecurityRiskLevel>> AssessSecurityRiskAsync(Guid paymentId, CancellationToken cancellationToken = default)
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
    private async Task CreateAuditLogAsync(Guid paymentId, Guid? courierId, AuditEventType eventType, AuditSeverityLevel severityLevel,
        string title, string description, SecurityRiskLevel? riskLevel = null, string? details = null, CancellationToken cancellationToken = default)
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
    private string ComputeFileHash(byte[] fileData)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileData);
        return Convert.ToBase64String(hashBytes);
    }
    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes);
    }
    private async Task<bool> PerformIdentityVerification(string identityType, string identityNumber, CancellationToken cancellationToken)
    {
        // Basit kimlik doğrulama - gerçek implementasyonda external API kullanılabilir
        await Task.Delay(100, cancellationToken); // Simulate API call

        // Basit validasyon kuralları
        return identityType.ToLower() switch
        {
            "tc" => identityNumber.Length == 11 && identityNumber.All(char.IsDigit),
            "passport" => identityNumber.Length >= 6,
            "driving_license" => identityNumber.Length >= 8,
            _ => false
        };
    }
    // Diğer metodlar için placeholder'lar (implementasyon devam edecek)
    public async Task<Result<CashPaymentEvidenceResponse>> UpdateEvidenceAsync(Guid evidenceId, UpdateCashPaymentEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Evidence var mı kontrol et
            var evidence = await _unitOfWork.ReadRepository<CashPaymentEvidence>()
                .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken: cancellationToken);

            if (evidence == null)
            {
                return Result.Fail<CashPaymentEvidenceResponse>("Evidence not found", "EVIDENCE_NOT_FOUND");
            }

            // Güncelleme izinleri kontrol et
            if (evidence.Status == EvidenceStatus.Verified || evidence.Status == EvidenceStatus.Rejected)
            {
                return Result.Fail<CashPaymentEvidenceResponse>("Cannot update verified or rejected evidence", "EVIDENCE_LOCKED");
            }

            // Dosya güncelleme
            if (request.FileData != null)
            {
                var fileHash = ComputeFileHash(request.FileData);

                // Dosyayı storage'a yükle
                var uploadRequest = new FileUploadRequest(
                    request.FileName ?? "evidence_update.jpg",
                    request.FileData,
                    request.MimeType ?? "image/jpeg",
                    "cash-payment-evidence",
                    FileCategory.Document,
                    request.Description,
                    evidence.PaymentId,
                    "Payment"
                );

                var uploadResult = await _fileStorageService.UploadFileAsync(uploadRequest, cancellationToken);
                if (!uploadResult.Success)
                {
                    return Result.Fail<CashPaymentEvidenceResponse>("Failed to upload file", "FILE_UPLOAD_FAILED");
                }

                evidence.FileUrl = uploadResult.Value.BlobUrl;
                evidence.FileSize = request.FileData.Length;
                evidence.MimeType = request.MimeType ?? "image/jpeg";
                evidence.FileHash = fileHash;
            }

            // Diğer alanları güncelle
            if (!string.IsNullOrEmpty(request.Description))
                evidence.Description = request.Description;

            if (request.Latitude.HasValue)
                evidence.Latitude = request.Latitude.Value;

            if (request.Longitude.HasValue)
                evidence.Longitude = request.Longitude.Value;

            if (!string.IsNullOrEmpty(request.DeviceInfo))
                evidence.DeviceInfo = request.DeviceInfo;

            evidence.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentEvidence>().Update(evidence);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await CreateAuditLogAsync(
                evidence.PaymentId,
                evidence.CourierId,
                AuditEventType.EvidenceUpdated,
                AuditSeverityLevel.Information,
                "Evidence Updated",
                $"Evidence {evidenceId} was updated",
                cancellationToken: cancellationToken
            );

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
            _logger.LogException(ex, "UpdateEvidence", new { EvidenceId = evidenceId });
            return Result.Fail<CashPaymentEvidenceResponse>("Failed to update evidence", "UPDATE_EVIDENCE_FAILED");
        }
    }
    public async Task<Result<PagedResult<CashPaymentEvidenceResponse>>> GetPaymentEvidenceAsync(Guid paymentId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.ReadRepository<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken: cancellationToken);

            if (payment == null)
            {
                return Result.Fail<PagedResult<CashPaymentEvidenceResponse>>("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Evidence'ları getir
            var evidenceList = await _unitOfWork.ReadRepository<CashPaymentEvidence>()
                .GetPagedAsync(
                    e => e.PaymentId == paymentId,
                    e => e.CreatedAt,
                    false, // descending order
                    query.Page,
                    query.PageSize,
                    cancellationToken: cancellationToken
                );

            var totalCount = await _unitOfWork.ReadRepository<CashPaymentEvidence>()
                .CountAsync(e => e.PaymentId == paymentId, cancellationToken);

            var responses = evidenceList.Select(e => new CashPaymentEvidenceResponse(
                e.Id,
                e.PaymentId,
                e.CourierId,
                e.EvidenceType,
                e.FileUrl,
                e.FileSize,
                e.MimeType,
                e.FileHash,
                e.Description,
                e.Latitude,
                e.Longitude,
                e.DeviceInfo,
                e.Status,
                e.VerificationNotes,
                e.VerifiedByAdminId,
                e.VerifiedAt,
                e.CreatedAt,
                e.UpdatedAt
            )).ToList();

            var pagedResult = PagedResult<CashPaymentEvidenceResponse>.Create(
                responses,
                totalCount,
                query.Page,
                query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetPaymentEvidence", new { PaymentId = paymentId });
            return Result.Fail<PagedResult<CashPaymentEvidenceResponse>>("Failed to get payment evidence", "GET_EVIDENCE_FAILED");
        }
    }
    public async Task<Result<CashPaymentSecurityResponse>> UpdateSecurityRecordAsync(Guid securityId, UpdateCashPaymentSecurityRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Security record var mı kontrol et
            var securityRecord = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .FirstOrDefaultAsync(s => s.Id == securityId, cancellationToken: cancellationToken);

            if (securityRecord == null)
            {
                return Result.Fail<CashPaymentSecurityResponse>("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            // Güncelleme izinleri kontrol et - Status property'si yok, RequiresManualApproval kullan
            if (securityRecord.RequiresManualApproval && securityRecord.ApprovedAt.HasValue)
            {
                return Result.Fail<CashPaymentSecurityResponse>("Cannot update approved security record", "SECURITY_RECORD_LOCKED");
            }

            // Güncelleme alanları
            if (request.GivenChange.HasValue)
                securityRecord.GivenChange = request.GivenChange.Value;

            if (request.CalculatedChange.HasValue)
                securityRecord.CalculatedChange = request.CalculatedChange.Value;

            if (request.ChangeCalculationVerified.HasValue)
                securityRecord.ChangeCalculationVerified = request.ChangeCalculationVerified.Value;

            if (!string.IsNullOrEmpty(request.IdentityType))
                securityRecord.IdentityVerificationType = request.IdentityType;

            if (!string.IsNullOrEmpty(request.IdentityNumberHash))
                securityRecord.IdentityNumberHash = request.IdentityNumberHash;

            if (request.IdentityVerified.HasValue)
                securityRecord.CustomerIdentityVerified = request.IdentityVerified.Value;

            if (request.FakeMoneyCheckPerformed.HasValue)
                securityRecord.FakeMoneyCheckPerformed = request.FakeMoneyCheckPerformed.Value;

            if (request.FakeMoneyDetected.HasValue)
                securityRecord.FakeMoneyDetected = request.FakeMoneyDetected.Value;

            if (!string.IsNullOrEmpty(request.SecurityNotes))
                securityRecord.SecurityNotes = request.SecurityNotes;

            if (request.RiskLevel.HasValue)
                securityRecord.RiskLevel = request.RiskLevel.Value;

            securityRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentSecurity>().Update(securityRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await CreateAuditLogAsync(
                securityRecord.PaymentId,
                securityRecord.Payment.CollectedByCourierId,
                AuditEventType.SecurityRecordUpdated,
                AuditSeverityLevel.Information,
                "Security Record Updated",
                $"Security record {securityId} was updated",
                securityRecord.RiskLevel,
                cancellationToken: cancellationToken
            );

            var response = new CashPaymentSecurityResponse(
                securityRecord.Id,
                securityRecord.PaymentId,
                securityRecord.Payment.CollectedByCourierId ?? Guid.Empty, // CourierId
                securityRecord.GivenChange,
                securityRecord.CalculatedChange,
                securityRecord.ChangeCalculationVerified,
                securityRecord.IdentityVerificationType ?? "", // IdentityType
                securityRecord.CustomerIdentityVerified, // IdentityVerified
                securityRecord.FakeMoneyCheckPerformed,
                securityRecord.FakeMoneyDetected,
                securityRecord.SecurityNotes ?? "", // SecurityNotes
                securityRecord.RiskLevel,
                securityRecord.RequiresManualApproval ? "Pending" : "Approved", // Status
                securityRecord.ApprovedByAdminId,
                securityRecord.ApprovedAt,
                securityRecord.CreatedAt,
                securityRecord.UpdatedAt
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "UpdateSecurityRecord", new { SecurityId = securityId });
            return Result.Fail<CashPaymentSecurityResponse>("Failed to update security record", "UPDATE_SECURITY_RECORD_FAILED");
        }
    }
    public async Task<Result<CashPaymentSecurityResponse>> GetPaymentSecurityAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.ReadRepository<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken: cancellationToken);

            if (payment == null)
            {
                return Result.Fail<CashPaymentSecurityResponse>("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Security record getir
            var securityRecord = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .FirstOrDefaultAsync(s => s.PaymentId == paymentId, cancellationToken: cancellationToken);

            if (securityRecord == null)
            {
                return Result.Fail<CashPaymentSecurityResponse>("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            var response = new CashPaymentSecurityResponse(
                securityRecord.Id,
                securityRecord.PaymentId,
                securityRecord.Payment.CollectedByCourierId ?? Guid.Empty, // CourierId
                securityRecord.GivenChange,
                securityRecord.CalculatedChange,
                securityRecord.ChangeCalculationVerified,
                securityRecord.IdentityVerificationType ?? "",
                securityRecord.CustomerIdentityVerified,
                securityRecord.FakeMoneyCheckPerformed,
                securityRecord.FakeMoneyDetected,
                securityRecord.SecurityNotes ?? "",
                securityRecord.RiskLevel,
                securityRecord.RequiresManualApproval ? "Pending" : "Approved", // Status
                securityRecord.ApprovedByAdminId,
                securityRecord.ApprovedAt,
                securityRecord.CreatedAt,
                securityRecord.UpdatedAt
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetPaymentSecurity", new { PaymentId = paymentId });
            return Result.Fail<CashPaymentSecurityResponse>("Failed to get payment security", "GET_PAYMENT_SECURITY_FAILED");
        }
    }
    public async Task<Result<bool>> PerformFakeMoneyCheckAsync(Guid paymentId, string notes, CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.ReadRepository<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken: cancellationToken);

            if (payment == null)
            {
                return Result.Fail<bool>("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Security record getir
            var securityRecord = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .FirstOrDefaultAsync(s => s.PaymentId == paymentId, cancellationToken: cancellationToken);

            if (securityRecord == null)
            {
                return Result.Fail<bool>("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            // Sahte para kontrolü yapıldı olarak işaretle
            securityRecord.FakeMoneyCheckPerformed = true;
            securityRecord.SecurityNotes = string.IsNullOrEmpty(securityRecord.SecurityNotes)
                ? notes
                : $"{securityRecord.SecurityNotes}\nFake Money Check: {notes}";
            securityRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentSecurity>().Update(securityRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await CreateAuditLogAsync(
                paymentId,
                securityRecord.Payment.CollectedByCourierId,
                AuditEventType.FakeMoneyCheckPerformed,
                AuditSeverityLevel.Information,
                "Fake Money Check Performed",
                $"Fake money check performed for payment {paymentId}. Notes: {notes}",
                securityRecord.RiskLevel,
                cancellationToken: cancellationToken
            );

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "PerformFakeMoneyCheck", new { PaymentId = paymentId });
            return Result.Fail<bool>("Failed to perform fake money check", "FAKE_MONEY_CHECK_FAILED");
        }
    }
    public async Task<Result<bool>> VerifyCustomerIdentityAsync(Guid paymentId, string identityType, string identityNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            // Payment var mı kontrol et
            var payment = await _unitOfWork.ReadRepository<Payment>()
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken: cancellationToken);

            if (payment == null)
            {
                return Result.Fail<bool>("Payment not found", "PAYMENT_NOT_FOUND");
            }

            // Security record getir
            var securityRecord = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .FirstOrDefaultAsync(s => s.PaymentId == paymentId, cancellationToken: cancellationToken);

            if (securityRecord == null)
            {
                return Result.Fail<bool>("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            // Kimlik numarasını hash'le
            var identityHash = ComputeHash(identityNumber);

            // Kimlik doğrulama işlemi
            var isVerified = await PerformIdentityVerification(identityType, identityNumber, cancellationToken);

            // Security record'u güncelle
            securityRecord.IdentityVerificationType = identityType;
            securityRecord.IdentityNumberHash = identityHash;
            securityRecord.CustomerIdentityVerified = isVerified;
            securityRecord.SecurityNotes = string.IsNullOrEmpty(securityRecord.SecurityNotes)
                ? $"Identity verification: {identityType} - {(isVerified ? "Verified" : "Failed")}"
                : $"{securityRecord.SecurityNotes}\nIdentity verification: {identityType} - {(isVerified ? "Verified" : "Failed")}";
            securityRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentSecurity>().Update(securityRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await CreateAuditLogAsync(
                paymentId,
                securityRecord.Payment.CollectedByCourierId,
                AuditEventType.IdentityVerificationPerformed,
                isVerified ? AuditSeverityLevel.Information : AuditSeverityLevel.Warning,
                "Identity Verification Performed",
                $"Identity verification performed for payment {paymentId}. Type: {identityType}, Result: {(isVerified ? "Verified" : "Failed")}",
                securityRecord.RiskLevel,
                cancellationToken: cancellationToken
            );

            return Result.Ok(isVerified);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "VerifyCustomerIdentity", new { PaymentId = paymentId, IdentityType = identityType });
            return Result.Fail<bool>("Failed to verify customer identity", "IDENTITY_VERIFICATION_FAILED");
        }
    }
    public async Task<Result<PagedResult<CashPaymentSecurityResponse>>> GetPaymentsRequiringApprovalAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Onay bekleyen security record'ları getir
            var securityRecords = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .GetPagedAsync(
                    s => s.RequiresManualApproval && !s.ApprovedAt.HasValue,
                    s => s.CreatedAt,
                    false, // descending order
                    query.Page,
                    query.PageSize,
                    cancellationToken: cancellationToken
                );

            var totalCount = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .CountAsync(s => s.RequiresManualApproval && !s.ApprovedAt.HasValue, cancellationToken);

            var responses = securityRecords.Select(s => new CashPaymentSecurityResponse(
                s.Id,
                s.PaymentId,
                s.Payment.CollectedByCourierId ?? Guid.Empty, // CourierId
                s.GivenChange,
                s.CalculatedChange,
                s.ChangeCalculationVerified,
                s.IdentityVerificationType ?? "", // IdentityType
                s.CustomerIdentityVerified, // IdentityVerified
                s.FakeMoneyCheckPerformed,
                s.FakeMoneyDetected,
                s.SecurityNotes ?? "", // SecurityNotes
                s.RiskLevel,
                s.RequiresManualApproval ? "Pending" : "Approved", // Status
                s.ApprovedByAdminId,
                s.ApprovedAt,
                s.CreatedAt,
                s.UpdatedAt
            )).ToList();

            var pagedResult = PagedResult<CashPaymentSecurityResponse>.Create(
                responses,
                totalCount,
                query.Page,
                query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetPaymentsRequiringApproval");
            return Result.Fail<PagedResult<CashPaymentSecurityResponse>>("Failed to get payments requiring approval", "GET_PAYMENTS_REQUIRING_APPROVAL_FAILED");
        }
    }
    public async Task<Result> ApproveSecurityRecordAsync(Guid securityId, Guid adminId, string notes, CancellationToken cancellationToken = default)
    {
        try
        {
            // Security record var mı kontrol et
            var securityRecord = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .FirstOrDefaultAsync(s => s.Id == securityId, cancellationToken: cancellationToken);

            if (securityRecord == null)
            {
                return Result.Fail("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            // Zaten onaylanmış mı kontrol et
            if (securityRecord.ApprovedAt.HasValue)
            {
                return Result.Fail("Security record is already approved", "ALREADY_APPROVED");
            }

            // Security record'u onayla
            securityRecord.ApprovedByAdminId = adminId;
            securityRecord.ApprovedAt = DateTime.UtcNow;
            securityRecord.SecurityNotes = string.IsNullOrEmpty(securityRecord.SecurityNotes)
                ? $"Approved by admin {adminId}: {notes}"
                : $"{securityRecord.SecurityNotes}\nApproved by admin {adminId}: {notes}";
            securityRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentSecurity>().Update(securityRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await CreateAuditLogAsync(
                securityRecord.PaymentId,
                securityRecord.Payment.CollectedByCourierId,
                AuditEventType.SecurityRecordApproved,
                AuditSeverityLevel.Information,
                "Security Record Approved",
                $"Security record {securityId} was approved by admin {adminId}. Notes: {notes}",
                securityRecord.RiskLevel,
                cancellationToken: cancellationToken
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "ApproveSecurityRecord", new { SecurityId = securityId, AdminId = adminId });
            return Result.Fail("Failed to approve security record", "APPROVE_SECURITY_RECORD_FAILED");
        }
    }
    public async Task<Result> RejectSecurityRecordAsync(Guid securityId, Guid adminId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            // Security record var mı kontrol et
            var securityRecord = await _unitOfWork.ReadRepository<CashPaymentSecurity>()
                .FirstOrDefaultAsync(s => s.Id == securityId, cancellationToken: cancellationToken);

            if (securityRecord == null)
            {
                return Result.Fail("Security record not found", "SECURITY_RECORD_NOT_FOUND");
            }

            // Zaten reddedilmiş mi kontrol et - reddedilme durumu için özel bir field yok, sadece onaylanmamış olması yeterli
            if (securityRecord.ApprovedAt.HasValue)
            {
                return Result.Fail("Security record is already processed", "ALREADY_PROCESSED");
            }

            // Security record'u reddet - reddedilme durumu için özel bir field yok, sadece not ekleyebiliriz
            securityRecord.ApprovedByAdminId = adminId;
            securityRecord.ApprovedAt = DateTime.UtcNow;
            securityRecord.SecurityNotes = string.IsNullOrEmpty(securityRecord.SecurityNotes)
                ? $"Rejected by admin {adminId}: {reason}"
                : $"{securityRecord.SecurityNotes}\nRejected by admin {adminId}: {reason}";
            securityRecord.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CashPaymentSecurity>().Update(securityRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await CreateAuditLogAsync(
                securityRecord.PaymentId,
                securityRecord.Payment.CollectedByCourierId,
                AuditEventType.SecurityRecordRejected,
                AuditSeverityLevel.Warning,
                "Security Record Rejected",
                $"Security record {securityId} was rejected by admin {adminId}. Reason: {reason}",
                securityRecord.RiskLevel,
                cancellationToken: cancellationToken
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "RejectSecurityRecord", new { SecurityId = securityId, AdminId = adminId });
            return Result.Fail("Failed to reject security record", "REJECT_SECURITY_RECORD_FAILED");
        }
    }
}
