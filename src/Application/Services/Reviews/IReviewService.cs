using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Reviews;

/// <summary>
/// Review servisi: değerlendirme CRUD, rating hesaplama, moderasyon, tag yönetimi, analizler.
/// </summary>
public interface IReviewService
{
    // Review CRUD operations
    /// <summary>Yeni değerlendirme oluşturur (order tamamlanma kontrolü, duplicate kontrolü, tag ekleme, rating güncelleme, cache invalidation).</summary>
    Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, Guid reviewerId, CancellationToken cancellationToken = default);
    /// <summary>Değerlendirmeyi günceller (ownership kontrolü, tag yönetimi, rating güncelleme).</summary>
    Task<Result<ReviewResponse>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Değerlendirmeyi siler (ownership kontrolü, soft delete, rating güncelleme).</summary>
    Task<Result> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Değerlendirmeyi ID ile getirir (tag/helpful count dahil).</summary>
    Task<Result<ReviewResponse>> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
    
    // Review queries
    /// <summary>Değerlendirmeleri arar (filtreleme ile).</summary>
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default);
    /// <summary>Entity için değerlendirmeleri getirir (cache).</summary>
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsByRevieweeAsync(Guid revieweeId, string revieweeType, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcının yaptığı değerlendirmeleri getirir.</summary>
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsByReviewerAsync(Guid reviewerId, PaginationQuery query, CancellationToken cancellationToken = default);
    /// <summary>Sipariş için değerlendirmeleri getirir.</summary>
    Task<Result<PagedResult<ReviewResponse>>> GetReviewsByOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    
    // Rating operations
    /// <summary>Entity rating'ini getirir (ortalama, breakdown, cache).</summary>
    Task<Result<RatingResponse>> GetRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    /// <summary>Review istatistiklerini getirir.</summary>
    Task<Result<ReviewStatsResponse>> GetReviewStatsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    /// <summary>Rating hesaplar (tarih aralığı ile).</summary>
    Task<Result<RatingCalculationResponse>> CalculateRatingAsync(RatingCalculationRequest request, CancellationToken cancellationToken = default);
    /// <summary>Entity rating'ini günceller (ortalama, breakdown, son performans).</summary>
    Task<Result> UpdateRatingAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    
    // Review moderation
    /// <summary>Moderasyon bekleyen değerlendirmeleri getirir.</summary>
    Task<Result<PagedResult<PendingReviewResponse>>> GetPendingReviewsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    
    // Additional review methods
    /// <summary>Değerlendirmeyi raporlar (duplicate kontrolü).</summary>
    Task<Result> ReportReviewAsync(Guid reviewId, ReportReviewRequest request, Guid reporterId, CancellationToken cancellationToken = default);
    /// <summary>Değerlendirmeyi beğenir (duplicate kontrolü).</summary>
    Task<Result> LikeReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Beğeniyi geri alır.</summary>
    Task<Result> UnlikeReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Değerlendirmeyi modere eder (approve/reject, moderation log).</summary>
    Task<Result<ReviewModerationResponse>> ModerateReviewAsync(Guid reviewId, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default);
    /// <summary>Toplu moderasyon (hata yönetimi).</summary>
    Task<Result> BulkModerateReviewsAsync(List<Guid> reviewIds, ReviewModerationRequest request, Guid moderatorId, CancellationToken cancellationToken = default);
    
    // Review helpful votes
    /// <summary>Faydalı/faydasız oylar (duplicate kontrolü).</summary>
    Task<Result<ReviewHelpfulResponse>> VoteHelpfulAsync(Guid reviewId, ReviewHelpfulRequest request, Guid userId, CancellationToken cancellationToken = default);
    /// <summary>Faydalı oyunu geri alır.</summary>
    Task<Result> RemoveHelpfulVoteAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default);
    
    // Review analytics
    /// <summary>Review analizlerini getirir (rating dağılımı, tag frekansları).</summary>
    Task<Result<ReviewAnalyticsResponse>> GetReviewAnalyticsAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    /// <summary>Değerlendirmeleri detaylı arar (sıralama/filtreleme).</summary>
    Task<Result<ReviewSearchResponse>> SearchReviewsAsync(ReviewSearchQuery query, CancellationToken cancellationToken = default);
    
    // Review validation
    /// <summary>Kullanıcının değerlendirme yapıp yapamayacağını kontrol eder (order tamamlanma).</summary>
    Task<Result<bool>> CanUserReviewAsync(Guid userId, Guid revieweeId, string revieweeType, Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcının order'ı daha önce değerlendirip değerlendirmediğini kontrol eder.</summary>
    Task<Result<bool>> HasUserReviewedOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
    
    // Review tags
    /// <summary>Entity tipi için mevcut tag'leri getirir (merchant/courier).</summary>
    Task<Result<List<string>>> GetAvailableTagsAsync(string revieweeType, CancellationToken cancellationToken = default);
    /// <summary>Tag frekanslarını getirir (en çok kullanılan tag'ler).</summary>
    Task<Result<List<TagFrequencyResponse>>> GetTagFrequenciesAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
}
