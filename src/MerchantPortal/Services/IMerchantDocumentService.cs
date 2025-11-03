using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IMerchantDocumentService
{
	Task<PagedResult<MerchantDocumentResponse>?> GetDocumentsAsync(Guid? merchantId = null, string? documentType = null, string? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
	Task<MerchantDocumentResponse?> UploadAsync(UploadMerchantDocumentRequest request, IFormFile file, CancellationToken ct = default);
	Task<bool> DeleteAsync(Guid documentId, CancellationToken ct = default);
}


