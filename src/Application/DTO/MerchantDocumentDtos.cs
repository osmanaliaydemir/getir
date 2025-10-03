using Getir.Domain.Enums;

namespace Getir.Application.DTO;

/// <summary>
/// Request to upload a merchant document
/// </summary>
public record UploadMerchantDocumentRequest
{
    public Guid MerchantId { get; init; }
    public DocumentType DocumentType { get; init; }
    public string DocumentName { get; init; } = default!;
    public string? Description { get; init; }
    public DateTime ExpiryDate { get; init; }
    public bool IsRequired { get; init; } = true;
}

/// <summary>
/// Response for merchant document
/// </summary>
public record MerchantDocumentResponse
{
    public Guid Id { get; init; }
    public Guid MerchantId { get; init; }
    public Guid UploadedBy { get; init; }
    public DocumentType DocumentType { get; init; }
    public string DocumentName { get; init; } = default!;
    public string FileName { get; init; } = default!;
    public string FileUrl { get; init; } = default!;
    public string MimeType { get; init; } = default!;
    public long FileSize { get; init; }
    public string? Description { get; init; }
    public DateTime ExpiryDate { get; init; }
    public bool IsRequired { get; init; }
    public bool IsVerified { get; init; }
    public bool IsApproved { get; init; }
    public string? VerificationNotes { get; init; }
    public Guid? VerifiedBy { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public DocumentStatus Status { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Additional info
    public string UploadedByUserName { get; init; } = default!;
    public string? VerifiedByUserName { get; init; }
    public bool IsExpired { get; init; }
    public int DaysUntilExpiry { get; init; }
}

/// <summary>
/// Request to verify a merchant document
/// </summary>
public record VerifyMerchantDocumentRequest
{
    public Guid DocumentId { get; init; }
    public bool IsApproved { get; init; }
    public string? VerificationNotes { get; init; }
    public string? RejectionReason { get; init; }
}

/// <summary>
/// Request to get merchant documents
/// </summary>
public record GetMerchantDocumentsRequest
{
    public Guid? MerchantId { get; init; }
    public DocumentType? DocumentType { get; init; }
    public DocumentStatus? Status { get; init; }
    public bool? IsRequired { get; init; }
    public bool? IsExpired { get; init; }
    public DateTime? ExpiryDateFrom { get; init; }
    public DateTime? ExpiryDateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Document upload progress response
/// </summary>
public record DocumentUploadProgressResponse
{
    public Guid MerchantId { get; init; }
    public int TotalRequiredDocuments { get; init; }
    public int UploadedDocuments { get; init; }
    public int VerifiedDocuments { get; init; }
    public int ApprovedDocuments { get; init; }
    public int RejectedDocuments { get; init; }
    public int ExpiredDocumentsCount { get; init; }
    public decimal CompletionPercentage { get; init; }
    public List<DocumentType> MissingDocuments { get; init; } = new();
    public List<DocumentType> ExpiredDocumentTypes { get; init; } = new();
    public bool CanProceedToNextStep { get; init; }
}

/// <summary>
/// Document type configuration
/// </summary>
public record DocumentTypeConfig
{
    public DocumentType Type { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public bool IsRequired { get; init; }
    public int MaxFileSizeMB { get; init; }
    public List<string> AllowedExtensions { get; init; } = new();
    public List<string> AllowedMimeTypes { get; init; } = new();
    public int ValidityDays { get; init; } // How many days the document is valid
    public string? SampleImageUrl { get; init; }
}

public record MerchantDocumentQuery
{
    public Guid? MerchantId { get; init; }
    public string? DocumentType { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record BulkVerifyDocumentsRequest
{
    public List<Guid> DocumentIds { get; init; } = new();
    public bool IsApproved { get; init; }
    public string? VerificationNotes { get; init; }
}

public record BulkVerifyDocumentsResponse
{
    public int TotalDocuments { get; init; }
    public int SuccessfulVerifications { get; init; }
    public int FailedVerifications { get; init; }
    public List<string> Errors { get; init; } = new();
}

public record MerchantDocumentProgressResponse
{
    public Guid MerchantId { get; init; }
    public int TotalRequiredDocuments { get; init; }
    public int UploadedDocuments { get; init; }
    public int VerifiedDocuments { get; init; }
    public decimal CompletionPercentage { get; init; }
}

public record DocumentTypeResponse
{
    public string Type { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsRequired { get; init; }
}

public record MerchantDocumentStatisticsResponse
{
    public int TotalDocuments { get; init; }
    public int PendingDocuments { get; init; }
    public int ApprovedDocuments { get; init; }
    public int RejectedDocuments { get; init; }
    public int ExpiredDocuments { get; init; }
}