// System namespaces
using Microsoft.Extensions.Logging;

// Application namespaces
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;

// Domain namespaces
using Getir.Domain.Entities;

namespace Getir.Application.Services.Search;

public class SearchService : BaseService, ISearchService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    public SearchService(IUnitOfWork unitOfWork, ILogger<SearchService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    public async Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(SearchProductsQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await SearchProductsInternalAsync(query, cancellationToken),
            "SearchProducts",
            new { Query = query.Query, MerchantId = query.MerchantId, Page = query.Page },
            cancellationToken);
    }
    private async Task<Result<PagedResult<ProductResponse>>> SearchProductsInternalAsync(SearchProductsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.SearchResults(
                query.Query ?? string.Empty,
                query.MerchantId,
                query.Page,
                query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var products = await _unitOfWork.Repository<Product>().GetPagedAsync(
                        filter: p => p.IsActive &&
                                    (string.IsNullOrEmpty(query.Query) || p.Name.Contains(query.Query)) &&
                                    (!query.MerchantId.HasValue || p.MerchantId == query.MerchantId) &&
                                    (!query.CategoryId.HasValue || p.ProductCategoryId == query.CategoryId) &&
                                    (!query.MinPrice.HasValue || p.Price >= query.MinPrice) &&
                                    (!query.MaxPrice.HasValue || p.Price <= query.MaxPrice),
                        orderBy: p => p.Name,
                        ascending: true,
                        page: query.Page,
                        pageSize: query.PageSize,
                        include: "Merchant",
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<Product>()
                        .CountAsync(p => p.IsActive &&
                                       (string.IsNullOrEmpty(query.Query) || p.Name.Contains(query.Query)),
                                    cancellationToken);

                    var response = products.Select(p => new ProductResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        IsActive = p.IsActive,
                        IsDeleted = false,
                        MerchantId = p.MerchantId,
                        MerchantName = p.Merchant.Name,
                        ProductCategoryId = p.ProductCategoryId,
                        ProductCategoryName = p.ProductCategory?.Name,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        DiscountedPrice = p.DiscountedPrice,
                        StockQuantity = p.StockQuantity,
                        Unit = p.Unit,
                        IsAvailable = p.IsAvailable
                    }).ToList();

                    var pagedResult = PagedResult<ProductResponse>.Create(response, total, query.Page, query.PageSize);

                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.Short), // 5 minutes TTL for search results
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products: {Query}", query.Query);
            return ServiceResult.HandleException<PagedResult<ProductResponse>>(ex, _logger, "SearchProducts");
        }
    }
    public async Task<Result<PagedResult<MerchantResponse>>> SearchMerchantsAsync(SearchMerchantsQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await SearchMerchantsInternalAsync(query, cancellationToken),
            "SearchMerchants",
            new { Query = query.Query, CategoryId = query.CategoryId, Page = query.Page },
            cancellationToken);
    }
    private async Task<Result<PagedResult<MerchantResponse>>> SearchMerchantsInternalAsync(SearchMerchantsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy (search results short TTL)
            var cacheKey = CacheKeys.SearchResults(
                $"merchant-{query.Query ?? string.Empty}",
                query.CategoryId,
                query.Page,
                query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var merchants = await _unitOfWork.Repository<Merchant>().GetPagedAsync(
                        filter: m => m.IsActive &&
                                    (string.IsNullOrEmpty(query.Query) || m.Name.Contains(query.Query)) &&
                                    (!query.CategoryId.HasValue || m.ServiceCategoryId == query.CategoryId),
                        orderBy: m => m.Rating,
                        ascending: false,
                        page: query.Page,
                        pageSize: query.PageSize,
                        include: "ServiceCategory,Owner",
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<Merchant>()
                        .CountAsync(m => m.IsActive &&
                                       (string.IsNullOrEmpty(query.Query) || m.Name.Contains(query.Query)),
                                    cancellationToken);

                    var response = merchants.Select(m => new MerchantResponse
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        IsActive = m.IsActive,
                        IsDeleted = false,
                        Rating = m.Rating,
                        TotalReviews = m.TotalReviews,
                        OwnerId = m.OwnerId,
                        OwnerName = string.Concat(m.Owner.FirstName, " ", m.Owner.LastName),
                        ServiceCategoryId = m.ServiceCategoryId,
                        ServiceCategoryName = m.ServiceCategory.Name,
                        LogoUrl = m.LogoUrl,
                        Address = m.Address,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        MinimumOrderAmount = m.MinimumOrderAmount,
                        DeliveryFee = m.DeliveryFee,
                        AverageDeliveryTime = m.AverageDeliveryTime,
                        IsOpen = m.IsOpen
                    }).ToList();

                    var pagedResult = PagedResult<MerchantResponse>.Create(response, total, query.Page, query.PageSize);

                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.Short), // 5 minutes TTL for merchant search
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching merchants: {Query}", query.Query);
            return ServiceResult.HandleException<PagedResult<MerchantResponse>>(ex, _logger, "SearchMerchants");
        }
    }
}
