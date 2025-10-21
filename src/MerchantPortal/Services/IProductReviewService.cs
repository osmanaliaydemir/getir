using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

/// <summary>
/// Product review servisi - Merchant'ın ürünlerine gelen yorumları yönetir
/// </summary>
public interface IProductReviewService
{
    /// <summary>
    /// Merchant'ın ürünlerine gelen yorumları getirir (paginated).
    /// </summary>
    Task<PagedResult<ProductReviewResponse>?> GetMerchantProductReviewsAsync(
        Guid merchantId, 
        int page = 1, 
        int pageSize = 20,
        int? rating = null,
        bool? isApproved = null,
        CancellationToken ct = default);

    /// <summary>
    /// Belirli bir ürünün yorumlarını getirir.
    /// </summary>
    Task<PagedResult<ProductReviewResponse>?> GetProductReviewsAsync(
        Guid productId, 
        int page = 1, 
        int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Review detayını getirir.
    /// </summary>
    Task<ProductReviewResponse?> GetReviewByIdAsync(Guid reviewId, CancellationToken ct = default);

    /// <summary>
    /// Ürün review istatistiklerini getirir.
    /// </summary>
    Task<ProductReviewStatsResponse?> GetProductReviewStatsAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Merchant'ın tüm ürünleri için review istatistiklerini getirir.
    /// </summary>
    Task<ProductReviewStatsResponse?> GetMerchantReviewStatsAsync(Guid merchantId, CancellationToken ct = default);

    /// <summary>
    /// Review'a merchant yanıtı ekler.
    /// </summary>
    Task<bool> RespondToReviewAsync(Guid reviewId, string response, CancellationToken ct = default);

    /// <summary>
    /// Review'ı onaylar (merchant moderasyonu).
    /// </summary>
    Task<bool> ApproveReviewAsync(Guid reviewId, CancellationToken ct = default);

    /// <summary>
    /// Review'ı reddeder (merchant moderasyonu).
    /// </summary>
    Task<bool> RejectReviewAsync(Guid reviewId, string reason, CancellationToken ct = default);
}

