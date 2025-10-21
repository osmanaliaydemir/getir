using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

/// <summary>
/// Merchant Portal için Review Service Interface
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Merchant'ın aldığı değerlendirmeleri getir
    /// </summary>
    Task<List<ReviewResponse>> GetMerchantReviewsAsync(Guid merchantId, ReviewFilterModel filter, CancellationToken ct = default);
    
    /// <summary>
    /// Merchant'ın kuryelerini değerlendirmeleri getir
    /// </summary>
    Task<List<ReviewResponse>> GetCourierReviewsAsync(Guid merchantId, ReviewFilterModel filter, CancellationToken ct = default);
    
    /// <summary>
    /// Merchant review istatistiklerini getir
    /// </summary>
    Task<ReviewStatsResponse?> GetMerchantReviewStatsAsync(Guid merchantId, CancellationToken ct = default);
    
    /// <summary>
    /// Courier review istatistiklerini getir
    /// </summary>
    Task<ReviewStatsResponse?> GetCourierReviewStatsAsync(Guid merchantId, CancellationToken ct = default);
    
    /// <summary>
    /// Review'a yanıt ver
    /// </summary>
    Task<bool> RespondToReviewAsync(Guid reviewId, string response, CancellationToken ct = default);
    
    /// <summary>
    /// Review'ı beğen/beğenme
    /// </summary>
    Task<bool> LikeReviewAsync(Guid reviewId, bool isLiked, CancellationToken ct = default);
    
    /// <summary>
    /// Review'ı rapor et
    /// </summary>
    Task<bool> ReportReviewAsync(Guid reviewId, string reason, CancellationToken ct = default);
    
    /// <summary>
    /// Genel review dashboard verilerini getir
    /// </summary>
    Task<ReviewDashboardModel> GetReviewDashboardAsync(Guid merchantId, CancellationToken ct = default);
}
