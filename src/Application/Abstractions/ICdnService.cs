using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// Dosya önbellekleme ve optimizasyon için CDN servisi interface'i
/// </summary>
public interface ICdnService
{
    /// <summary>
    /// Dosya için CDN URL'i getir
    /// </summary>
    /// <param name="originalUrl">Orijinal URL</param>
    /// <param name="containerName">Container adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>CDN URL sonucu</returns>
    Task<Result<string>> GetCdnUrlAsync(
        string originalUrl, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosya için CDN cache'i geçersiz kıl
    /// </summary>
    /// <param name="fileUrl">Dosya URL'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    Task<Result> InvalidateCacheAsync(
        string fileUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dönüşümlerle optimize edilmiş resim URL'i getir
    /// </summary>
    /// <param name="originalUrl">Orijinal URL</param>
    /// <param name="width">Genişlik</param>
    /// <param name="height">Yükseklik</param>
    /// <param name="quality">Kalite</param>
    /// <param name="format">Format</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Optimize edilmiş URL sonucu</returns>
    Task<Result<string>> GetOptimizedImageUrlAsync(
        string originalUrl,
        int? width = null,
        int? height = null,
        int? quality = null,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CDN'e dosya yükle
    /// </summary>
    /// <param name="request">Yükleme isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Yükleme sonucu</returns>
    Task<Result<CdnUploadResponse>> UploadToCdnAsync(
        CdnUploadRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CDN'den dosya sil
    /// </summary>
    /// <param name="cdnUrl">CDN URL'i</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    Task<Result> DeleteFromCdnAsync(
        string cdnUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// CDN istatistiklerini getir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>CDN istatistikleri</returns>
    Task<Result<CdnStats>> GetCdnStatsAsync(
        CancellationToken cancellationToken = default);
}
