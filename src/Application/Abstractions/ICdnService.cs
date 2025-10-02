using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// CDN service interface for file caching and optimization
/// </summary>
public interface ICdnService
{
    /// <summary>
    /// Get CDN URL for a file
    /// </summary>
    Task<Result<string>> GetCdnUrlAsync(
        string originalUrl, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate CDN cache for a file
    /// </summary>
    Task<Result> InvalidateCacheAsync(
        string fileUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get optimized image URL with transformations
    /// </summary>
    Task<Result<string>> GetOptimizedImageUrlAsync(
        string originalUrl,
        int? width = null,
        int? height = null,
        int? quality = null,
        string? format = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload file to CDN
    /// </summary>
    Task<Result<CdnUploadResponse>> UploadToCdnAsync(
        CdnUploadRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from CDN
    /// </summary>
    Task<Result> DeleteFromCdnAsync(
        string cdnUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get CDN statistics
    /// </summary>
    Task<Result<CdnStats>> GetCdnStatsAsync(
        CancellationToken cancellationToken = default);
}
