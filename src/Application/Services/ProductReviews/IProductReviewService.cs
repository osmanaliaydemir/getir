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
}

