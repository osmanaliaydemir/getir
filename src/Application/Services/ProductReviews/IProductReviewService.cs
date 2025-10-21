using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductReviews;

/// <summary>
/// Product review servisi: ürün yorumları, rating hesaplama, moderation
/// </summary>
public interface IProductReviewService
{
    /// <summary>Ürüne review ekle (verified purchase kontrolü, rating güncelle)</summary>
    Task<Result<ProductReviewResponse>> CreateProductReviewAsync(CreateProductReviewRequest request, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>Kendi review'unu güncelle (rating yeniden hesapla)</summary>
    Task<Result<ProductReviewResponse>> UpdateProductReviewAsync(Guid id, UpdateProductReviewRequest request, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>Kendi review'unu sil (rating yeniden hesapla)</summary>
    Task<Result> DeleteProductReviewAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>Ürünün review'larını getir (sayfalama, onaylı review'lar)</summary>
    Task<Result<PagedResult<ProductReviewResponse>>> GetProductReviewsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>Kullanıcının review'larını getir</summary>
    Task<Result<PagedResult<ProductReviewResponse>>> GetUserReviewsAsync(Guid userId, PaginationQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>Review'a helpful/not helpful oy ver</summary>
    Task<Result> VoteReviewHelpfulAsync(Guid reviewId, Guid userId, bool isHelpful, CancellationToken cancellationToken = default);
    
    /// <summary>Ürün rating'ini yeniden hesapla (internal use)</summary>
    Task<Result> RecalculateProductRatingAsync(Guid productId, CancellationToken cancellationToken = default);
    
    /// <summary>Merchant'ın tüm ürünlerine gelen review'ları getir (sayfalama, filtreleme)</summary>
    Task<Result<PagedResult<ProductReviewResponse>>> GetMerchantProductReviewsAsync(Guid merchantId, PaginationQuery query, int? rating = null, bool? isApproved = null, CancellationToken cancellationToken = default);
    
    /// <summary>Merchant'ın tüm ürünleri için review istatistikleri</summary>
    Task<Result<ProductReviewStatsResponse>> GetMerchantReviewStatsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    
    /// <summary>Ürün review istatistiklerini getir</summary>
    Task<Result<ProductReviewStatsResponse>> GetProductReviewStatsAsync(Guid productId, CancellationToken cancellationToken = default);
    
    /// <summary>Review'a merchant yanıtı ekle</summary>
    Task<Result<ProductReviewResponse>> RespondToReviewAsync(Guid reviewId, RespondToReviewRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Review'ı onayla (merchant moderasyonu)</summary>
    Task<Result> ApproveProductReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
    
    /// <summary>Review'ı reddet (merchant moderasyonu)</summary>
    Task<Result> RejectProductReviewAsync(Guid reviewId, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Respond to review request DTO
/// </summary>
public record RespondToReviewRequest(string Response);

