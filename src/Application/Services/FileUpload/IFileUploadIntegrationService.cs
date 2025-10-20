using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.FileUpload;

/// <summary>
/// Dosya yükleme entegrasyon servisi: entity'lerle dosya yüklemelerini entegre eder (merchant logo/cover, ürün resimleri).
/// </summary>
public interface IFileUploadIntegrationService
{
    /// <summary>Merchant logo URL'ini günceller (eski dosyayı siler).</summary>
    Task<Result> UpdateMerchantLogoAsync(Guid merchantId, string logoUrl, CancellationToken cancellationToken = default);
    /// <summary>Merchant kapak resmi URL'ini günceller (eski dosyayı siler).</summary>
    Task<Result> UpdateMerchantCoverAsync(Guid merchantId, string coverImageUrl, CancellationToken cancellationToken = default);
    /// <summary>Ürün resmi URL'ini günceller (eski dosyayı siler).</summary>
    Task<Result> UpdateProductImageAsync(Guid productId, string imageUrl, CancellationToken cancellationToken = default);
    /// <summary>Merchant logosunu siler ve entity'yi günceller.</summary>
    Task<Result> DeleteMerchantLogoAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Merchant kapak resmini siler ve entity'yi günceller.</summary>
    Task<Result> DeleteMerchantCoverAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Ürün resmini siler ve entity'yi günceller.</summary>
    Task<Result> DeleteProductImageAsync(Guid productId, CancellationToken cancellationToken = default);
    /// <summary>Merchant logo URL'ini getirir.</summary>
    Task<Result<string?>> GetMerchantLogoAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Merchant kapak resmi URL'ini getirir.</summary>
    Task<Result<string?>> GetMerchantCoverAsync(Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Ürün resmi URL'ini getirir.</summary>
    Task<Result<string?>> GetProductImageAsync(Guid productId, CancellationToken cancellationToken = default);
    // Additional methods for FileUploadController
    /// <summary>Genel dosya yükler.</summary>
    Task<Result<FileUploadResponse>> UploadFileAsync(IUploadedFile file, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Çoklu dosya yükler.</summary>
    Task<Result<IEnumerable<FileUploadResponse>>> UploadMultipleFilesAsync(IUploadedFile[] files, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Dosya URL'i oluşturur.</summary>
    Task<Result<string>> GetFileUrlAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    /// <summary>Dosya siler (kullanıcı).</summary>
    Task<Result> DeleteFileAsync(string containerName, string fileName, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Merchant dosyası yükler.</summary>
    Task<Result<FileUploadResponse>> UploadMerchantFileAsync(IUploadedFile file, Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Merchant dosyalarını sayfalama ile getirir.</summary>
    Task<Result<PagedResult<FileUploadResponse>>> GetMerchantFilesAsync(Guid merchantId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Merchant dosyasını siler.</summary>
    Task<Result> DeleteMerchantFileAsync(string containerName, string fileName, Guid merchantId, CancellationToken cancellationToken = default);
    /// <summary>Tüm dosyaları sayfalama ile getirir (admin).</summary>
    Task<Result<PagedResult<FileUploadResponse>>> GetAllFilesAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Herhangi bir dosyayı siler (admin).</summary>
    Task<Result> DeleteAnyFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    /// <summary>Dosya istatistiklerini getirir.</summary>
    Task<Result<FileStatisticsResponse>> GetFileStatisticsAsync(CancellationToken cancellationToken = default);
    /// <summary>Eski dosyaları temizler.</summary>
    Task<Result<int>> CleanupOldFilesAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

}
