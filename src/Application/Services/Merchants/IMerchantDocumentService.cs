using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Merchants;

/// <summary>
/// Merchant doküman yönetimi servisi: onboarding dokümanlarının yükleme, doğrulama, takip işlemleri.
/// </summary>
public interface IMerchantDocumentService
{
    /// <summary>Merchant onboarding için doküman yükler (duplicate kontrolü, file hash).</summary>
    Task<Result<MerchantDocumentResponse>> UploadDocumentAsync(UploadMerchantDocumentRequest request, Stream fileStream, string fileName, string mimeType, Guid uploadedBy, CancellationToken cancellationToken = default);

    /// <summary>Merchant dokümanlarını filtreleme ile getirir.</summary>
    Task<Result<PagedResult<MerchantDocumentResponse>>> GetDocumentsAsync(GetMerchantDocumentsRequest request, CancellationToken cancellationToken = default);

    /// <summary>Dokümanı ID ile getirir.</summary>
    Task<Result<MerchantDocumentResponse>> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>Dokümanı doğrular/onaylar veya reddeder.</summary>
    Task<Result<MerchantDocumentResponse>> VerifyDocumentAsync(VerifyMerchantDocumentRequest request, Guid verifiedBy, CancellationToken cancellationToken = default);

    /// <summary>Dokümanı siler (storage'dan da siler).</summary>
    Task<Result> DeleteDocumentAsync(Guid documentId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>Merchant için doküman yükleme ilerlemesini getirir.</summary>
    Task<Result<DocumentUploadProgressResponse>> GetUploadProgressAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>Merchant onboarding için gerekli doküman tiplerini getirir.</summary>
    Task<Result<List<DocumentTypeConfig>>> GetRequiredDocumentTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>Merchant'ın tüm gerekli dokümanlara sahip olup olmadığını kontrol eder.</summary>
    Task<Result<bool>> HasAllRequiredDocumentsAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>Süresi dolmuş dokümanları getirir.</summary>
    Task<Result<List<MerchantDocumentResponse>>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>Yakında süresi dolacak dokümanları getirir.</summary>
    Task<Result<List<MerchantDocumentResponse>>> GetDocumentsExpiringSoonAsync(int daysAhead = 30, CancellationToken cancellationToken = default);

    /// <summary>Doküman son kullanma tarihini günceller.</summary>
    Task<Result<MerchantDocumentResponse>> UpdateExpiryDateAsync(Guid documentId, DateTime newExpiryDate, Guid updatedBy, CancellationToken cancellationToken = default);

    // Additional methods for controller
    /// <summary>Doküman istatistiklerini getirir.</summary>
    Task<Result<MerchantDocumentStatisticsResponse>> GetDocumentStatisticsAsync(Guid? merchantId = null, CancellationToken cancellationToken = default);

    /// <summary>Toplu doküman doğrulama yapar.</summary>
    Task<Result<BulkVerifyDocumentsResponse>> BulkVerifyDocumentsAsync(BulkVerifyDocumentsRequest request, Guid adminId, CancellationToken cancellationToken = default);

    /// <summary>Bekleyen dokümanları getirir.</summary>
    Task<Result<PagedResult<MerchantDocumentResponse>>> GetPendingDocumentsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Dokümanı indirir.</summary>
    Task<Result<Stream>> DownloadDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);

}
