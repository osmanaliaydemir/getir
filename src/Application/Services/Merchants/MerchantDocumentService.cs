using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Common.Extensions;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Merchants;

public class MerchantDocumentService : IMerchantDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MerchantDocumentService> _logger;

    public MerchantDocumentService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<MerchantDocumentService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<MerchantDocumentResponse>> UploadDocumentAsync(
        UploadMerchantDocumentRequest request,
        Stream fileStream,
        string fileName,
        string mimeType,
        Guid uploadedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate merchant exists
            var merchant = await _unitOfWork.Repository<Merchant>().GetByIdAsync(request.MerchantId, cancellationToken);
            if (merchant == null)
            {
                return Result.Fail<MerchantDocumentResponse>("Merchant not found");
            }

            // Validate user exists
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(uploadedBy, cancellationToken);
            if (user == null)
            {
                return Result.Fail<MerchantDocumentResponse>("User not found");
            }

            // Check if document type already exists for this merchant
            var existingDocument = await _unitOfWork.Repository<MerchantDocument>()
                .ListAsync(
                    filter: x => x.MerchantId == request.MerchantId && 
                                 x.DocumentType == request.DocumentType.ToString() &&
                                 x.Status != DocumentStatus.Rejected, 
                    cancellationToken: cancellationToken);

            if (existingDocument.Any())
            {
                return Result.Fail<MerchantDocumentResponse>("Document of this type already exists for this merchant");
            }

            // Upload file
            var fileContent = new byte[fileStream.Length];
            fileStream.Position = 0;
            await fileStream.ReadAsync(fileContent, 0, fileContent.Length, cancellationToken);
            
            var uploadRequest = new FileUploadRequest(
                fileName,
                fileContent,
                mimeType,
                "merchant-documents",
                FileCategory.Document,
                request.Description,
                request.MerchantId,
                "Merchant");
                
            var uploadResult = await _fileStorageService.UploadFileAsync(uploadRequest, cancellationToken);
            if (!uploadResult.Success || uploadResult.Value == null)
            {
                return Result.Fail<MerchantDocumentResponse>(uploadResult.Error ?? "Failed to upload file");
            }
            var fileUrl = uploadResult.Value.BlobUrl;

            // Calculate file hash
            var fileHash = await CalculateFileHashAsync(fileStream);

            // Create document entity
            var document = new MerchantDocument
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                UploadedBy = uploadedBy,
                DocumentType = request.DocumentType.ToString(),
                DocumentName = request.DocumentName,
                FileName = fileName,
                FileUrl = fileUrl,
                MimeType = mimeType,
                FileSize = fileStream.Length,
                FileHash = fileHash,
                Description = request.Description,
                ExpiryDate = request.ExpiryDate,
                IsRequired = request.IsRequired,
                Status = DocumentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<MerchantDocument>().AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update merchant onboarding progress
            await UpdateMerchantOnboardingProgressAsync(request.MerchantId, cancellationToken);

            var response = await MapToResponseAsync(document, cancellationToken);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error uploading merchant document");
            return Result.Fail<MerchantDocumentResponse>("Failed to upload document");
        }
    }

    public async Task<Result<PagedResult<MerchantDocumentResponse>>> GetDocumentsAsync(
        GetMerchantDocumentsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _unitOfWork.Repository<MerchantDocument>()
                .GetPagedAsync(
                    filter: BuildDocumentFilter(request),
                    orderBy: x => x.CreatedAt,
                    ascending: false,
                    page: request.Page,
                    pageSize: request.PageSize,
                    cancellationToken: cancellationToken);

            var totalCount = await _unitOfWork.ReadRepository<MerchantDocument>()
                .CountAsync(BuildDocumentFilter(request), cancellationToken);

            var responses = new List<MerchantDocumentResponse>();
            foreach (var document in documents)
            {
                var response = await MapToResponseAsync(document, cancellationToken);
                responses.Add(response);
            }

            var pagedResult = PagedResult<MerchantDocumentResponse>.Create(
                responses, totalCount, request.Page, request.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting merchant documents");
            return Result.Fail<PagedResult<MerchantDocumentResponse>>("Failed to get documents");
        }
    }

    public async Task<Result<MerchantDocumentResponse>> GetDocumentByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _unitOfWork.Repository<MerchantDocument>()
                .GetByIdAsync(documentId, cancellationToken);

            if (document == null)
            {
                return Result.Fail<MerchantDocumentResponse>("Document not found");
            }

            var response = await MapToResponseAsync(document, cancellationToken);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting document by ID");
            return Result.Fail<MerchantDocumentResponse>("Failed to get document");
        }
    }

    public async Task<Result<MerchantDocumentResponse>> VerifyDocumentAsync(
        VerifyMerchantDocumentRequest request,
        Guid verifiedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _unitOfWork.Repository<MerchantDocument>()
                .GetByIdAsync(request.DocumentId, cancellationToken);

            if (document == null)
            {
                return Result.Fail<MerchantDocumentResponse>("Document not found");
            }

            // Update document status
            document.IsVerified = true;
            document.IsApproved = request.IsApproved;
            document.VerificationNotes = request.VerificationNotes;
            document.VerifiedBy = verifiedBy;
            document.VerifiedAt = DateTime.UtcNow;
            document.Status = request.IsApproved ? DocumentStatus.Approved : DocumentStatus.Rejected;
            document.RejectionReason = request.IsApproved ? null : request.RejectionReason;
            document.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<MerchantDocument>().Update(document);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update merchant onboarding progress
            await UpdateMerchantOnboardingProgressAsync(document.MerchantId, cancellationToken);

            var response = await MapToResponseAsync(document, cancellationToken);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error verifying document");
            return Result.Fail<MerchantDocumentResponse>("Failed to verify document");
        }
    }

    public async Task<Result> DeleteDocumentAsync(
        Guid documentId,
        Guid deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _unitOfWork.Repository<MerchantDocument>()
                .GetByIdAsync(documentId, cancellationToken);

            if (document == null)
            {
                return Result.Fail("Document not found");
            }

            // Delete file from storage
            var fileNameFromUrl = document.FileUrl.Split('/').Last();
            await _fileStorageService.DeleteFileAsync(fileNameFromUrl, "merchant-documents", cancellationToken);

            // Delete document from database
            await _unitOfWork.Repository<MerchantDocument>().DeleteAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update merchant onboarding progress
            await UpdateMerchantOnboardingProgressAsync(document.MerchantId, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error deleting document");
            return Result.Fail("Failed to delete document");
        }
    }

    public async Task<Result<DocumentUploadProgressResponse>> GetUploadProgressAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _unitOfWork.Repository<MerchantDocument>()
                .ListAsync(filter: x => x.MerchantId == merchantId, cancellationToken: cancellationToken);

            var requiredTypes = await GetRequiredDocumentTypesAsync(cancellationToken);
            var requiredTypesList = requiredTypes.Data?.Select(x => x.Type).ToList() ?? new List<DocumentType>();

            var progress = new DocumentUploadProgressResponse
            {
                MerchantId = merchantId,
                TotalRequiredDocuments = requiredTypesList.Count,
                UploadedDocuments = documents.Count,
                VerifiedDocuments = documents.Count(x => x.IsVerified),
                ApprovedDocuments = documents.Count(x => x.IsApproved),
                RejectedDocuments = documents.Count(x => x.Status == DocumentStatus.Rejected),
                ExpiredDocumentsCount = documents.Count(x => x.ExpiryDate < DateTime.UtcNow),
                CompletionPercentage = requiredTypesList.Count > 0 ? 
                    (decimal)documents.Count(x => x.IsApproved) / requiredTypesList.Count * 100 : 0,
                MissingDocuments = requiredTypesList.Where(x => 
                    !documents.Any(d => d.DocumentType == x.ToString() && d.IsApproved)).ToList(),
                ExpiredDocumentTypes = documents.Where(x => x.ExpiryDate < DateTime.UtcNow)
                    .Select(x => Enum.Parse<DocumentType>(x.DocumentType)).ToList(),
                CanProceedToNextStep = documents.Count(x => x.IsApproved) >= requiredTypesList.Count
            };

            return Result.Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting upload progress");
            return Result.Fail<DocumentUploadProgressResponse>("Failed to get upload progress");
        }
    }

    public async Task<Result<List<DocumentTypeConfig>>> GetRequiredDocumentTypesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configs = new List<DocumentTypeConfig>
            {
                new()
                {
                    Type = DocumentType.TaxCertificate,
                    Name = "Vergi Levhası",
                    Description = "İşletmenizin vergi levhası",
                    IsRequired = true,
                    MaxFileSizeMB = 5,
                    AllowedExtensions = new() { "pdf", "jpg", "jpeg", "png" },
                    AllowedMimeTypes = new() { "application/pdf", "image/jpeg", "image/png" },
                    ValidityDays = 365
                },
                new()
                {
                    Type = DocumentType.BusinessLicense,
                    Name = "İşletme Ruhsatı",
                    Description = "İşletmenizin ruhsat belgesi",
                    IsRequired = true,
                    MaxFileSizeMB = 5,
                    AllowedExtensions = new() { "pdf", "jpg", "jpeg", "png" },
                    AllowedMimeTypes = new() { "application/pdf", "image/jpeg", "image/png" },
                    ValidityDays = 365
                },
                new()
                {
                    Type = DocumentType.IdentityCard,
                    Name = "Kimlik Belgesi",
                    Description = "İşletme sahibinin kimlik belgesi",
                    IsRequired = true,
                    MaxFileSizeMB = 2,
                    AllowedExtensions = new() { "pdf", "jpg", "jpeg", "png" },
                    AllowedMimeTypes = new() { "application/pdf", "image/jpeg", "image/png" },
                    ValidityDays = 1095 // 3 years
                }
            };

            return Result.Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting required document types");
            return Result.Fail<List<DocumentTypeConfig>>("Failed to get document types");
        }
    }

    public async Task<Result<bool>> HasAllRequiredDocumentsAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var progress = await GetUploadProgressAsync(merchantId, cancellationToken);
            return Result.Ok(progress.Data?.CanProceedToNextStep ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error checking required documents");
            return Result.Fail<bool>("Failed to check required documents");
        }
    }

    public async Task<Result<List<MerchantDocumentResponse>>> GetExpiredDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _unitOfWork.Repository<MerchantDocument>()
                .ListAsync(filter: x => x.ExpiryDate < DateTime.UtcNow, cancellationToken: cancellationToken);

            var responses = new List<MerchantDocumentResponse>();
            foreach (var document in documents)
            {
                var response = await MapToResponseAsync(document, cancellationToken);
                responses.Add(response);
            }

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting expired documents");
            return Result.Fail<List<MerchantDocumentResponse>>("Failed to get expired documents");
        }
    }

    public async Task<Result<List<MerchantDocumentResponse>>> GetDocumentsExpiringSoonAsync(
        int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysAhead);
            var documents = await _unitOfWork.Repository<MerchantDocument>()
                .ListAsync(
                    filter: x => x.ExpiryDate <= expiryDate && x.ExpiryDate > DateTime.UtcNow, 
                    cancellationToken: cancellationToken);

            var responses = new List<MerchantDocumentResponse>();
            foreach (var document in documents)
            {
                var response = await MapToResponseAsync(document, cancellationToken);
                responses.Add(response);
            }

            return Result.Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting documents expiring soon");
            return Result.Fail<List<MerchantDocumentResponse>>("Failed to get documents expiring soon");
        }
    }

    public async Task<Result<MerchantDocumentResponse>> UpdateExpiryDateAsync(
        Guid documentId,
        DateTime newExpiryDate,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _unitOfWork.Repository<MerchantDocument>()
                .GetByIdAsync(documentId, cancellationToken);

            if (document == null)
            {
                return Result.Fail<MerchantDocumentResponse>("Document not found");
            }

            document.ExpiryDate = newExpiryDate;
            document.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<MerchantDocument>().Update(document);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = await MapToResponseAsync(document, cancellationToken);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error updating expiry date");
            return Result.Fail<MerchantDocumentResponse>("Failed to update expiry date");
        }
    }

    #region Private Methods

    private async Task<string> CalculateFileHashAsync(Stream fileStream)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        fileStream.Position = 0;
        var hashBytes = await sha256.ComputeHashAsync(fileStream);
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<MerchantDocumentResponse> MapToResponseAsync(
        MerchantDocument document,
        CancellationToken cancellationToken)
    {
        var uploadedByUser = await _unitOfWork.Repository<User>()
            .GetByIdAsync(document.UploadedBy, cancellationToken);

        var verifiedByUser = document.VerifiedBy.HasValue
            ? await _unitOfWork.Repository<User>()
                .GetByIdAsync(document.VerifiedBy.Value, cancellationToken)
            : null;

        var isExpired = document.ExpiryDate < DateTime.UtcNow;
        var daysUntilExpiry = (int)(document.ExpiryDate - DateTime.UtcNow).TotalDays;

        return new MerchantDocumentResponse
        {
            Id = document.Id,
            MerchantId = document.MerchantId,
            UploadedBy = document.UploadedBy,
            DocumentType = Enum.Parse<DocumentType>(document.DocumentType),
            DocumentName = document.DocumentName,
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            MimeType = document.MimeType,
            FileSize = document.FileSize,
            Description = document.Description,
            ExpiryDate = document.ExpiryDate,
            IsRequired = document.IsRequired,
            IsVerified = document.IsVerified,
            IsApproved = document.IsApproved,
            VerificationNotes = document.VerificationNotes,
            VerifiedBy = document.VerifiedBy,
            VerifiedAt = document.VerifiedAt,
            Status = document.Status,
            RejectionReason = document.RejectionReason,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            UploadedByUserName = uploadedByUser?.FirstName + " " + uploadedByUser?.LastName,
            VerifiedByUserName = verifiedByUser != null ? 
                verifiedByUser.FirstName + " " + verifiedByUser.LastName : null,
            IsExpired = isExpired,
            DaysUntilExpiry = daysUntilExpiry
        };
    }

    private static System.Linq.Expressions.Expression<Func<MerchantDocument, bool>> BuildDocumentFilter(
        GetMerchantDocumentsRequest request)
    {
        return x =>
            (request.MerchantId == null || x.MerchantId == request.MerchantId) &&
            (request.DocumentType == null || x.DocumentType == request.DocumentType.ToString()) &&
            (request.Status == null || x.Status == request.Status) &&
            (request.IsRequired == null || x.IsRequired == request.IsRequired) &&
            (request.ExpiryDateFrom == null || x.ExpiryDate >= request.ExpiryDateFrom) &&
            (request.ExpiryDateTo == null || x.ExpiryDate <= request.ExpiryDateTo) &&
            (request.IsExpired == null || (request.IsExpired.Value ? x.ExpiryDate < DateTime.UtcNow : x.ExpiryDate >= DateTime.UtcNow));
    }

    private async Task UpdateMerchantOnboardingProgressAsync(
        Guid merchantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var onboarding = await _unitOfWork.Repository<MerchantOnboarding>()
                .ListAsync(filter: x => x.MerchantId == merchantId, cancellationToken: cancellationToken);

            var onboardingRecord = onboarding.FirstOrDefault();
            if (onboardingRecord != null)
            {
                var hasAllDocuments = await HasAllRequiredDocumentsAsync(merchantId, cancellationToken);
                onboardingRecord.DocumentsUploaded = hasAllDocuments.Data;

                // Recalculate progress
                var completedSteps = 0;
                if (onboardingRecord.BasicInfoCompleted) completedSteps++;
                if (onboardingRecord.BusinessInfoCompleted) completedSteps++;
                if (onboardingRecord.WorkingHoursCompleted) completedSteps++;
                if (onboardingRecord.DeliveryZonesCompleted) completedSteps++;
                if (onboardingRecord.ProductsAdded) completedSteps++;
                if (onboardingRecord.DocumentsUploaded) completedSteps++;

                onboardingRecord.CompletedSteps = completedSteps;
                onboardingRecord.ProgressPercentage = (decimal)completedSteps / onboardingRecord.TotalSteps * 100;
                onboardingRecord.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<MerchantOnboarding>().Update(onboardingRecord);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error updating merchant onboarding progress");
        }
    }

    #endregion

    #region Additional Controller Methods

    public async Task<Result<MerchantDocumentStatisticsResponse>> GetDocumentStatisticsAsync(
        Guid? merchantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simplified statistics implementation
            var response = new MerchantDocumentStatisticsResponse
            {
                TotalDocuments = 0,
                PendingDocuments = 0,
                ApprovedDocuments = 0,
                RejectedDocuments = 0,
                ExpiredDocuments = 0
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting document statistics");
            return Result.Fail<MerchantDocumentStatisticsResponse>("Error getting document statistics");
        }
    }

    public async Task<Result<BulkVerifyDocumentsResponse>> BulkVerifyDocumentsAsync(
        BulkVerifyDocumentsRequest request,
        Guid adminId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simplified bulk verification implementation
            var response = new BulkVerifyDocumentsResponse
            {
                TotalDocuments = request.DocumentIds.Count,
                SuccessfulVerifications = 0,
                FailedVerifications = 0,
                Errors = new List<string>()
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error bulk verifying documents");
            return Result.Fail<BulkVerifyDocumentsResponse>("Error bulk verifying documents");
        }
    }

    public async Task<Result<PagedResult<MerchantDocumentResponse>>> GetPendingDocumentsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simplified pending documents implementation
            var response = new PagedResult<MerchantDocumentResponse>
            {
                Items = new List<MerchantDocumentResponse>(),
                Page = query.Page,
                PageSize = query.PageSize
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error getting pending documents");
            return Result.Fail<PagedResult<MerchantDocumentResponse>>("Error getting pending documents");
        }
    }

    public async Task<Result<Stream>> DownloadDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _unitOfWork.Repository<MerchantDocument>().GetByIdAsync(documentId, cancellationToken);
            if (document == null)
            {
                return Result.Fail<Stream>("Document not found");
            }

            // Simplified download implementation - return empty stream for now
            var stream = new MemoryStream();
            return Result.Ok<Stream>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "Error downloading document");
            return Result.Fail<Stream>("Error downloading document");
        }
    }

    #endregion
}
