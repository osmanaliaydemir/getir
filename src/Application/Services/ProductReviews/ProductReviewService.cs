using Microsoft.Extensions.Logging;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;

namespace Getir.Application.Services.ProductReviews;

/// <summary>
/// Product review servisi: review yönetimi ve otomatik rating hesaplama
/// </summary>
public class ProductReviewService : BaseService, IProductReviewService
{
    public ProductReviewService(
        IUnitOfWork unitOfWork,
        ILogger<ProductReviewService> logger,
        ILoggingService loggingService,
        ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }

    /// <summary>
    /// Yeni ürün review'u oluştur ve Product rating'ini güncelle
    /// </summary>
    public async Task<Result<ProductReviewResponse>> CreateProductReviewAsync(
        CreateProductReviewRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CreateProductReviewInternalAsync(request, userId, cancellationToken),
            "CreateProductReview",
            new { request.ProductId, userId, request.Rating },
            cancellationToken);
    }

    private async Task<Result<ProductReviewResponse>> CreateProductReviewInternalAsync(
        CreateProductReviewRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Product var mı kontrol et
            var product = await _unitOfWork.ReadRepository<Product>()
                .FirstOrDefaultAsync(
                    p => p.Id == request.ProductId && p.IsActive,
                    cancellationToken: cancellationToken);

            if (product == null)
                return Result.Fail<ProductReviewResponse>("Product not found", "PRODUCT_NOT_FOUND");

            // 2. Order var mı ve bu kullanıcıya ait mi kontrol et
            var order = await _unitOfWork.ReadRepository<Order>()
                .FirstOrDefaultAsync(
                    o => o.Id == request.OrderId &&
                         o.UserId == userId &&
                         o.Status == OrderStatus.Delivered,
                    include: "OrderLines",
                    cancellationToken: cancellationToken);

            if (order == null)
                return Result.Fail<ProductReviewResponse>(
                    "Order not found or not delivered yet",
                    "ORDER_NOT_FOUND");

            // 3. Bu order'da bu product var mı kontrol et
            var hasProduct = order.OrderLines.Any(ol => ol.ProductId == request.ProductId);
            if (!hasProduct)
                return Result.Fail<ProductReviewResponse>(
                    "Product not found in this order",
                    "PRODUCT_NOT_IN_ORDER");

            // 4. Kullanıcı bu ürün için daha önce review yapmış mı kontrol et
            var existingReview = await _unitOfWork.ReadRepository<ProductReview>()
                .FirstOrDefaultAsync(
                    pr => pr.ProductId == request.ProductId &&
                          pr.UserId == userId &&
                          !pr.IsDeleted,
                    cancellationToken: cancellationToken);

            if (existingReview != null)
                return Result.Fail<ProductReviewResponse>(
                    "You have already reviewed this product",
                    "REVIEW_ALREADY_EXISTS");

            // 5. Review oluştur
            var productReview = new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                UserId = userId,
                OrderId = request.OrderId,
                Rating = request.Rating,
                Comment = request.Comment,
                ImageUrls = request.ImageUrls,
                IsVerifiedPurchase = true, // OrderId ile doğrulandı
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ProductReview>().AddAsync(productReview, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Product rating'ini yeniden hesapla
            await RecalculateProductRatingInternalAsync(request.ProductId, cancellationToken);

            // 7. Cache'i temizle
            await InvalidateProductCacheAsync(request.ProductId);

            // 8. Response oluştur
            var user = await _unitOfWork.ReadRepository<User>()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: cancellationToken);

            var response = new ProductReviewResponse(
                productReview.Id,
                product.Id,
                product.Name,
                userId,
                $"{user?.FirstName} {user?.LastName}",
                request.OrderId,
                productReview.Rating,
                productReview.Comment,
                productReview.ImageUrls,
                productReview.IsVerifiedPurchase,
                productReview.CreatedAt,
                productReview.UpdatedAt,
                0,
                0,
                productReview.IsApproved);

            return ServiceResult.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductReviewResponse>(
                $"Failed to create product review: {ex.Message}",
                "CREATE_REVIEW_ERROR");
        }
    }

    /// <summary>
    /// Review güncelle ve rating'i yeniden hesapla
    /// </summary>
    public async Task<Result<ProductReviewResponse>> UpdateProductReviewAsync(
        Guid id,
        UpdateProductReviewRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _unitOfWork.Repository<ProductReview>()
                .FirstOrDefaultAsync(
                    pr => pr.Id == id && pr.UserId == userId && !pr.IsDeleted,
                    include: "Product,User",
                    cancellationToken: cancellationToken);

            if (review == null)
                return Result.Fail<ProductReviewResponse>("Review not found", "REVIEW_NOT_FOUND");

            // Update fields
            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.ImageUrls = request.ImageUrls;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Yeniden rating hesapla
            await RecalculateProductRatingInternalAsync(review.ProductId, cancellationToken);

            // Cache temizle
            await InvalidateProductCacheAsync(review.ProductId);

            var response = new ProductReviewResponse(
                review.Id,
                review.Product.Id,
                review.Product.Name,
                review.UserId,
                $"{review.User.FirstName} {review.User.LastName}",
                review.OrderId,
                review.Rating,
                review.Comment,
                review.ImageUrls,
                review.IsVerifiedPurchase,
                review.CreatedAt,
                review.UpdatedAt,
                review.HelpfulCount,
                review.NotHelpfulCount,
                review.IsApproved);

            return ServiceResult.Success(response);
        }
        catch (Exception ex)
        {
            return Result.Fail<ProductReviewResponse>(
                $"Failed to update review: {ex.Message}",
                "UPDATE_REVIEW_ERROR");
        }
    }

    /// <summary>
    /// Review sil (soft delete) ve rating'i yeniden hesapla
    /// </summary>
    public async Task<Result> DeleteProductReviewAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _unitOfWork.Repository<ProductReview>()
                .FirstOrDefaultAsync(
                    pr => pr.Id == id && pr.UserId == userId && !pr.IsDeleted,
                    cancellationToken: cancellationToken);

            if (review == null)
                return Result.Fail("Review not found", "REVIEW_NOT_FOUND");

            var productId = review.ProductId;

            // Soft delete
            review.IsDeleted = true;
            review.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Rating yeniden hesapla
            await RecalculateProductRatingInternalAsync(productId, cancellationToken);

            // Cache temizle
            await InvalidateProductCacheAsync(productId);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete review: {ex.Message}", "DELETE_REVIEW_ERROR");
        }
    }

    /// <summary>
    /// Ürünün tüm review'larını getir
    /// </summary>
    public async Task<Result<PagedResult<ProductReviewResponse>>> GetProductReviewsAsync(
        Guid productId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reviews = await _unitOfWork.ReadRepository<ProductReview>()
                .GetPagedAsync(
                    filter: pr => pr.ProductId == productId &&
                                  !pr.IsDeleted &&
                                  pr.IsApproved,
                    orderBy: pr => pr.CreatedAt,
                    ascending: false,
                    page: query.Page,
                    pageSize: query.PageSize,
                    include: "Product,User",
                    cancellationToken: cancellationToken);

            var total = await _unitOfWork.ReadRepository<ProductReview>()
                .CountAsync(
                    pr => pr.ProductId == productId && !pr.IsDeleted && pr.IsApproved,
                    cancellationToken);

            var responses = reviews.Select(pr => new ProductReviewResponse(
                pr.Id,
                pr.Product.Id,
                pr.Product.Name,
                pr.UserId,
                $"{pr.User.FirstName} {pr.User.LastName}",
                pr.OrderId,
                pr.Rating,
                pr.Comment,
                pr.ImageUrls,
                pr.IsVerifiedPurchase,
                pr.CreatedAt,
                pr.UpdatedAt,
                pr.HelpfulCount,
                pr.NotHelpfulCount,
                pr.IsApproved)).ToList();

            var pagedResult = PagedResult<ProductReviewResponse>.Create(
                responses,
                total,
                query.Page,
                query.PageSize);

            return ServiceResult.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result.Fail<PagedResult<ProductReviewResponse>>(
                $"Failed to fetch product reviews: {ex.Message}",
                "GET_REVIEWS_ERROR");
        }
    }

    /// <summary>
    /// Kullanıcının tüm review'larını getir
    /// </summary>
    public async Task<Result<PagedResult<ProductReviewResponse>>> GetUserReviewsAsync(
        Guid userId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reviews = await _unitOfWork.ReadRepository<ProductReview>()
                .GetPagedAsync(
                    filter: pr => pr.UserId == userId && !pr.IsDeleted,
                    orderBy: pr => pr.CreatedAt,
                    ascending: false,
                    page: query.Page,
                    pageSize: query.PageSize,
                    include: "Product,User",
                    cancellationToken: cancellationToken);

            var total = await _unitOfWork.ReadRepository<ProductReview>()
                .CountAsync(pr => pr.UserId == userId && !pr.IsDeleted, cancellationToken);

            var responses = reviews.Select(pr => new ProductReviewResponse(
                pr.Id,
                pr.Product.Id,
                pr.Product.Name,
                pr.UserId,
                $"{pr.User.FirstName} {pr.User.LastName}",
                pr.OrderId,
                pr.Rating,
                pr.Comment,
                pr.ImageUrls,
                pr.IsVerifiedPurchase,
                pr.CreatedAt,
                pr.UpdatedAt,
                pr.HelpfulCount,
                pr.NotHelpfulCount,
                pr.IsApproved)).ToList();

            var pagedResult = PagedResult<ProductReviewResponse>.Create(
                responses,
                total,
                query.Page,
                query.PageSize);

            return ServiceResult.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result.Fail<PagedResult<ProductReviewResponse>>(
                $"Failed to fetch user reviews: {ex.Message}",
                "GET_USER_REVIEWS_ERROR");
        }
    }

    /// <summary>
    /// Review'a helpful/not helpful oy ver
    /// </summary>
    public async Task<Result> VoteReviewHelpfulAsync(
        Guid reviewId,
        Guid userId,
        bool isHelpful,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _unitOfWork.Repository<ProductReview>()
                .FirstOrDefaultAsync(
                    pr => pr.Id == reviewId && !pr.IsDeleted,
                    cancellationToken: cancellationToken);

            if (review == null)
                return Result.Fail("Review not found", "REVIEW_NOT_FOUND");

            // Daha önce oy vermiş mi kontrol et
            var existingVote = await _unitOfWork.ReadRepository<ProductReviewHelpful>()
                .FirstOrDefaultAsync(
                    h => h.ProductReviewId == reviewId && h.UserId == userId,
                    cancellationToken: cancellationToken);

            if (existingVote != null)
            {
                // Mevcut oyu güncelle
                if (existingVote.IsHelpful != isHelpful)
                {
                    // Eski oyu geri al
                    if (existingVote.IsHelpful)
                        review.HelpfulCount--;
                    else
                        review.NotHelpfulCount--;

                    // Yeni oyu ekle
                    existingVote.IsHelpful = isHelpful;
                    if (isHelpful)
                        review.HelpfulCount++;
                    else
                        review.NotHelpfulCount++;

                    _unitOfWork.Repository<ProductReviewHelpful>()
                        .Update(existingVote);
                }
            }
            else
            {
                // Yeni oy ekle
                var vote = new ProductReviewHelpful
                {
                    Id = Guid.NewGuid(),
                    ProductReviewId = reviewId,
                    UserId = userId,
                    IsHelpful = isHelpful
                };

                await _unitOfWork.Repository<ProductReviewHelpful>()
                    .AddAsync(vote, cancellationToken);

                if (isHelpful)
                    review.HelpfulCount++;
                else
                    review.NotHelpfulCount++;
            }

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to vote review: {ex.Message}", "VOTE_REVIEW_ERROR");
        }
    }

    /// <summary>
    /// Product rating'ini yeniden hesapla (public - controller'dan da çağrılabilir)
    /// </summary>
    public async Task<Result> RecalculateProductRatingAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await RecalculateProductRatingInternalAsync(productId, cancellationToken),
            "RecalculateProductRating",
            new { productId },
            cancellationToken);
    }

    /// <summary>
    /// Product rating'ini hesaplayan core logic
    /// </summary>
    private async Task<Result> RecalculateProductRatingInternalAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>()
                .FirstOrDefaultAsync(
                    p => p.Id == productId,
                    cancellationToken: cancellationToken);

            if (product == null)
                return Result.Fail("Product not found", "PRODUCT_NOT_FOUND");

            // Onaylı ve silinmemiş review'ları getir
            var reviews = await _unitOfWork.ReadRepository<ProductReview>()
                .ListAsync(
                    pr => pr.ProductId == productId &&
                          !pr.IsDeleted &&
                          pr.IsApproved,
                    cancellationToken: cancellationToken);

            if (reviews.Any())
            {
                // Ortalama rating hesapla
                var averageRating = reviews.Average(r => r.Rating);
                product.Rating = Math.Round((decimal)averageRating, 2);
                product.ReviewCount = reviews.Count;
            }
            else
            {
                // Hiç review yoksa null yap
                product.Rating = null;
                product.ReviewCount = 0;
            }

            product.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail(
                $"Failed to recalculate product rating: {ex.Message}",
                "RECALCULATE_RATING_ERROR");
        }
    }

    /// <summary>
    /// Product cache'ini temizle
    /// </summary>
    private async Task InvalidateProductCacheAsync(Guid productId)
    {
        try
        {
            await _cacheService.RemoveAsync($"product_{productId}");
            // Popular products cache'ini de temizle (rating değiştiği için)
            // Her limit için ayrı cache key olduğu için 5-50 arası temizle
            for (int i = 5; i <= 50; i += 5)
            {
                await _cacheService.RemoveAsync($"popular_products_{i}");
            }
        }
        catch
        {
            // Cache temizleme hatası kritik değil, log et ve devam et
        }
    }

    /// <summary>
    /// Merchant'ın tüm ürünlerine gelen review'ları getir (filtreleme, cache).
    /// </summary>
    public async Task<Result<PagedResult<ProductReviewResponse>>> GetMerchantProductReviewsAsync(
        Guid merchantId, 
        PaginationQuery query, 
        int? rating = null, 
        bool? isApproved = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant'ın ürünlerini bul
            var merchantProducts = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(
                    p => p.MerchantId == merchantId && p.IsActive,
                    cancellationToken: cancellationToken);

            if (!merchantProducts.Any())
            {
                return Result.Ok(PagedResult<ProductReviewResponse>.Create(
                    new List<ProductReviewResponse>(), 0, query.Page, query.PageSize));
            }

            var productIds = merchantProducts.Select(p => p.Id).ToList();

            // Review'ları getir
            var reviews = await _unitOfWork.ReadRepository<ProductReview>()
                .GetPagedAsync(
                    page: query.Page,
                    pageSize: query.PageSize,
                    orderBy: r => r.CreatedAt,
                    ascending: false,
                    filter: r => productIds.Contains(r.ProductId) &&
                                (!rating.HasValue || r.Rating == rating.Value) &&
                                (!isApproved.HasValue || r.IsApproved == isApproved.Value),
                    include: "Product,User,ProductReviewHelpfuls",
                    cancellationToken: cancellationToken);

            var totalCount = await _unitOfWork.ReadRepository<ProductReview>()
                .CountAsync(
                    r => productIds.Contains(r.ProductId) &&
                        (!rating.HasValue || r.Rating == rating.Value) &&
                        (!isApproved.HasValue || r.IsApproved == isApproved.Value),
                    cancellationToken);

            var reviewResponses = reviews.Select(r => new ProductReviewResponse(
                r.Id,
                r.ProductId,
                r.Product?.Name ?? "Unknown",
                r.UserId,
                r.User?.FirstName + " " + r.User?.LastName ?? "Anonymous",
                r.OrderId,
                r.Rating,
                r.Comment,
                r.ImageUrls,
                r.IsVerifiedPurchase,
                r.CreatedAt,
                r.UpdatedAt,
                r.ProductReviewHelpfuls.Count(h => h.IsHelpful),
                r.ProductReviewHelpfuls.Count(h => !h.IsHelpful),
                r.IsApproved,
                r.MerchantResponse,
                r.MerchantRespondedAt
            )).ToList();

            var pagedResult = PagedResult<ProductReviewResponse>.Create(
                reviewResponses, totalCount, query.Page, query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant product reviews for merchant {MerchantId}", merchantId);
            return Result.Fail<PagedResult<ProductReviewResponse>>("Failed to get merchant product reviews", "MERCHANT_REVIEWS_ERROR");
        }
    }

    /// <summary>
    /// Merchant'ın tüm ürünleri için review istatistikleri.
    /// </summary>
    public async Task<Result<ProductReviewStatsResponse>> GetMerchantReviewStatsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant'ın ürünlerini bul
            var merchantProducts = await _unitOfWork.ReadRepository<Product>()
                .ListAsync(
                    p => p.MerchantId == merchantId && p.IsActive,
                    cancellationToken: cancellationToken);

            if (!merchantProducts.Any())
            {
                return Result.Ok(new ProductReviewStatsResponse(0, 0, 0, 0, 0, 0, 0, 0, 0));
            }

            var productIds = merchantProducts.Select(p => p.Id).ToList();

            // Tüm review'ları getir
            var allReviews = await _unitOfWork.ReadRepository<ProductReview>()
                .ListAsync(
                    r => productIds.Contains(r.ProductId),
                    cancellationToken: cancellationToken);

            if (!allReviews.Any())
            {
                return Result.Ok(new ProductReviewStatsResponse(0, 0, 0, 0, 0, 0, 0, 0, 0));
            }

            var stats = new ProductReviewStatsResponse(
                (decimal)allReviews.Average(r => r.Rating),
                allReviews.Count,
                allReviews.Count(r => r.Rating == 5),
                allReviews.Count(r => r.Rating == 4),
                allReviews.Count(r => r.Rating == 3),
                allReviews.Count(r => r.Rating == 2),
                allReviews.Count(r => r.Rating == 1),
                allReviews.Count(r => r.IsVerifiedPurchase),
                allReviews.Count(r => !r.IsApproved)
            );

            return Result.Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant review stats for {MerchantId}", merchantId);
            return Result.Fail<ProductReviewStatsResponse>("Failed to get merchant review stats", "MERCHANT_REVIEW_STATS_ERROR");
        }
    }

    /// <summary>
    /// Ürün review istatistiklerini getir.
    /// </summary>
    public async Task<Result<ProductReviewStatsResponse>> GetProductReviewStatsAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reviews = await _unitOfWork.ReadRepository<ProductReview>()
                .ListAsync(
                    r => r.ProductId == productId,
                    cancellationToken: cancellationToken);

            if (!reviews.Any())
            {
                return Result.Ok(new ProductReviewStatsResponse(0, 0, 0, 0, 0, 0, 0, 0, 0));
            }

            var stats = new ProductReviewStatsResponse(
                (decimal)reviews.Average(r => r.Rating),
                reviews.Count,
                reviews.Count(r => r.Rating == 5),
                reviews.Count(r => r.Rating == 4),
                reviews.Count(r => r.Rating == 3),
                reviews.Count(r => r.Rating == 2),
                reviews.Count(r => r.Rating == 1),
                reviews.Count(r => r.IsVerifiedPurchase),
                reviews.Count(r => !r.IsApproved)
            );

            return Result.Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product review stats for {ProductId}", productId);
            return Result.Fail<ProductReviewStatsResponse>("Failed to get product review stats", "PRODUCT_REVIEW_STATS_ERROR");
        }
    }

    /// <summary>
    /// Review'a merchant yanıtı ekle.
    /// </summary>
    public async Task<Result<ProductReviewResponse>> RespondToReviewAsync(Guid reviewId, RespondToReviewRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _unitOfWork.ReadRepository<ProductReview>()
                .FirstOrDefaultAsync(
                    r => r.Id == reviewId,
                    include: "Product,User,ProductReviewHelpfuls",
                    cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail<ProductReviewResponse>("Review not found", "REVIEW_NOT_FOUND");
            }

            // Yanıt ekle
            review.MerchantResponse = request.Response;
            review.MerchantRespondedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("MerchantRespondedToReview", new 
            { 
                ReviewId = reviewId, 
                ProductId = review.ProductId,
                MerchantId = review.Product?.MerchantId
            });

            var response = new ProductReviewResponse(
                review.Id,
                review.ProductId,
                review.Product?.Name ?? "Unknown",
                review.UserId,
                review.User?.FirstName + " " + review.User?.LastName ?? "Anonymous",
                review.OrderId,
                review.Rating,
                review.Comment,
                review.ImageUrls,
                review.IsVerifiedPurchase,
                review.CreatedAt,
                review.UpdatedAt,
                review.ProductReviewHelpfuls.Count(h => h.IsHelpful),
                review.ProductReviewHelpfuls.Count(h => !h.IsHelpful),
                review.IsApproved,
                review.MerchantResponse,
                review.MerchantRespondedAt
            );

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to review {ReviewId}", reviewId);
            return Result.Fail<ProductReviewResponse>("Failed to respond to review", "RESPOND_REVIEW_ERROR");
        }
    }

    /// <summary>
    /// Review'ı onayla.
    /// </summary>
    public async Task<Result> ApproveProductReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _unitOfWork.ReadRepository<ProductReview>()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail("Review not found", "REVIEW_NOT_FOUND");
            }

            review.IsApproved = true;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Cache'i temizle
            await _cacheService.RemoveAsync($"product:{review.ProductId}:reviews", cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
            return Result.Fail("Failed to approve review", "APPROVE_REVIEW_ERROR");
        }
    }

    /// <summary>
    /// Review'ı reddet.
    /// </summary>
    public async Task<Result> RejectProductReviewAsync(Guid reviewId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var review = await _unitOfWork.ReadRepository<ProductReview>()
                .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken: cancellationToken);

            if (review == null)
            {
                return Result.Fail("Review not found", "REVIEW_NOT_FOUND");
            }

            review.IsApproved = false;
            review.RejectionReason = reason;
            review.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Cache'i temizle
            await _cacheService.RemoveAsync($"product:{review.ProductId}:reviews", cancellationToken);

            _loggingService.LogBusinessEvent("ReviewRejected", new { ReviewId = reviewId, Reason = reason });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting review {ReviewId}", reviewId);
            return Result.Fail("Failed to reject review", "REJECT_REVIEW_ERROR");
        }
    }
}

