using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Merchants;

public interface IMerchantDocumentService
{
    /// <summary>
    /// Upload a document for merchant onboarding
    /// </summary>
    Task<Result<MerchantDocumentResponse>> UploadDocumentAsync(
        UploadMerchantDocumentRequest request,
        Stream fileStream,
        string fileName,
        string mimeType,
        Guid uploadedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get merchant documents with filtering
    /// </summary>
    Task<Result<PagedResult<MerchantDocumentResponse>>> GetDocumentsAsync(
        GetMerchantDocumentsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get document by ID
    /// </summary>
    Task<Result<MerchantDocumentResponse>> GetDocumentByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify/approve or reject a document
    /// </summary>
    Task<Result<MerchantDocumentResponse>> VerifyDocumentAsync(
        VerifyMerchantDocumentRequest request,
        Guid verifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a document
    /// </summary>
    Task<Result> DeleteDocumentAsync(
        Guid documentId,
        Guid deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get document upload progress for a merchant
    /// </summary>
    Task<Result<DocumentUploadProgressResponse>> GetUploadProgressAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get required document types for merchant onboarding
    /// </summary>
    Task<Result<List<DocumentTypeConfig>>> GetRequiredDocumentTypesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if merchant has all required documents
    /// </summary>
    Task<Result<bool>> HasAllRequiredDocumentsAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expired documents
    /// </summary>
    Task<Result<List<MerchantDocumentResponse>>> GetExpiredDocumentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents expiring soon
    /// </summary>
    Task<Result<List<MerchantDocumentResponse>>> GetDocumentsExpiringSoonAsync(
        int daysAhead = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update document expiry date
    /// </summary>
    Task<Result<MerchantDocumentResponse>> UpdateExpiryDateAsync(
        Guid documentId,
        DateTime newExpiryDate,
        Guid updatedBy,
        CancellationToken cancellationToken = default);

}
