using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.ServiceCategories;

/// <summary>
/// Servis kategorisi servisi implementasyonu: cache stratejisi, tip bazlı filtreleme, merchant count.
/// </summary>
public class ServiceCategoryService : BaseService, IServiceCategoryService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    public ServiceCategoryService(IUnitOfWork unitOfWork, ILogger<ServiceCategoryService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    /// <summary>
    /// Servis kategorilerini sayfalama ile getirir (cache, performance tracking, extra long TTL).
    /// </summary>
    public async Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetServiceCategoriesInternalAsync(query, cancellationToken),
            "GetServiceCategories",
            new { Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    
    private async Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesInternalAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.AllServiceCategories(query.Page, query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var categories = await _unitOfWork.Repository<ServiceCategory>().GetPagedAsync(
                        filter: c => c.IsActive,
                        orderBy: c => c.DisplayOrder,
                        ascending: true,
                        page: query.Page,
                        pageSize: query.PageSize,
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<ServiceCategory>()
                        .CountAsync(c => c.IsActive, cancellationToken);

                    var response = categories.Select(c => new ServiceCategoryResponse(
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Type,
                        c.ImageUrl,
                        c.IconUrl,
                        c.DisplayOrder,
                        c.IsActive,
                        c.Merchants?.Count ?? 0
                    )).ToList();

                    var pagedResult = PagedResult<ServiceCategoryResponse>.Create(response, total, query.Page, query.PageSize);

                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL - very static data
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service categories: Page {Page}, PageSize {PageSize}", query.Page, query.PageSize);
            return ServiceResult.HandleException<PagedResult<ServiceCategoryResponse>>(ex, _logger, "GetServiceCategories");
        }
    }
    /// <summary>
    /// Servis kategorisini ID ile getirir (merchant count dahil, cache).
    /// </summary>
    public async Task<Result<ServiceCategoryResponse>> GetServiceCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetServiceCategoryByIdInternalAsync(id, cancellationToken),
            "GetServiceCategoryById",
            new { CategoryId = id },
            cancellationToken);
    }
    
    private async Task<Result<ServiceCategoryResponse>> GetServiceCategoryByIdInternalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ServiceCategory(id);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var category = await _unitOfWork.Repository<ServiceCategory>()
                        .GetAsync(c => c.Id == id, include: "Merchants", cancellationToken: cancellationToken);

                    if (category == null)
                    {
                        return Result.Fail<ServiceCategoryResponse>("Service category not found", "NOT_FOUND_SERVICE_CATEGORY");
                    }

                    var response = new ServiceCategoryResponse(
                        category.Id,
                        category.Name,
                        category.Description,
                        category.Type,
                        category.ImageUrl,
                        category.IconUrl,
                        category.DisplayOrder,
                        category.IsActive,
                        category.Merchants?.Count ?? 0
                    );

                    return Result.Ok(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service category by id: {CategoryId}", id);
            return ServiceResult.HandleException<ServiceCategoryResponse>(ex, _logger, "GetServiceCategoryById");
        }
    }
    /// <summary>
    /// Yeni servis kategorisi oluşturur (cache invalidation).
    /// </summary>
    public async Task<Result<ServiceCategoryResponse>> CreateServiceCategoryAsync(CreateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            ImageUrl = request.ImageUrl,
            IconUrl = request.IconUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ServiceCategory>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate all service category caches
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllServiceCategoriesPattern(), cancellationToken);

        var response = new ServiceCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.Type,
            category.ImageUrl,
            category.IconUrl,
            category.DisplayOrder,
            category.IsActive,
            0
        );

        return Result.Ok(response);
    }
    /// <summary>
    /// Servis kategorisini günceller (merchant count hesaplama, cache invalidation).
    /// </summary>
    public async Task<Result<ServiceCategoryResponse>> UpdateServiceCategoryAsync(Guid id, UpdateServiceCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ServiceCategory>()
            .GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            return Result.Fail<ServiceCategoryResponse>("Service category not found", "NOT_FOUND_SERVICE_CATEGORY");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.Type = request.Type;
        category.ImageUrl = request.ImageUrl;
        category.IconUrl = request.IconUrl;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ServiceCategory>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single category cache
        await _cacheService.RemoveAsync(CacheKeys.ServiceCategory(id), cancellationToken);

        // Invalidate all service category lists
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllServiceCategoriesPattern(), cancellationToken);

        var merchantCount = await _unitOfWork.ReadRepository<Merchant>()
            .CountAsync(m => m.ServiceCategoryId == id, cancellationToken);

        var response = new ServiceCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.Type,
            category.ImageUrl,
            category.IconUrl,
            category.DisplayOrder,
            category.IsActive,
            merchantCount
        );

        return Result.Ok(response);
    }
    /// <summary>
    /// Servis kategorisini siler (soft delete, cache invalidation).
    /// </summary>
    public async Task<Result> DeleteServiceCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ServiceCategory>()
            .GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            return Result.Fail("Service category not found", "NOT_FOUND_SERVICE_CATEGORY");
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ServiceCategory>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single category cache
        await _cacheService.RemoveAsync(CacheKeys.ServiceCategory(id), cancellationToken);

        // Invalidate all service category lists
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllServiceCategoriesPattern(), cancellationToken);

        return Result.Ok();
    }
    /// <summary>
    /// Tip bazlı servis kategorilerini getirir (Food/Market/Pharmacy/Service, sayfalama, cache).
    /// </summary>
    public async Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesByTypeAsync(ServiceCategoryType type, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetServiceCategoriesByTypeInternalAsync(type, query, cancellationToken),
            "GetServiceCategoriesByType",
            new { Type = type, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    
    private async Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesByTypeInternalAsync(ServiceCategoryType type, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ServiceCategoriesByType(type.ToString(), query.Page, query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var categories = await _unitOfWork.Repository<ServiceCategory>().GetPagedAsync(
                        filter: c => c.IsActive && c.Type == type,
                        orderBy: c => c.DisplayOrder,
                        ascending: true,
                        page: query.Page,
                        pageSize: query.PageSize,
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<ServiceCategory>()
                        .CountAsync(c => c.IsActive && c.Type == type, cancellationToken);

                    var response = categories.Select(c => new ServiceCategoryResponse(
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Type,
                        c.ImageUrl,
                        c.IconUrl,
                        c.DisplayOrder,
                        c.IsActive,
                        c.Merchants?.Count ?? 0
                    )).ToList();

                    var pagedResult = PagedResult<ServiceCategoryResponse>.Create(response, total, query.Page, query.PageSize);

                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service categories by type: {Type}", type);
            return ServiceResult.HandleException<PagedResult<ServiceCategoryResponse>>(ex, _logger, "GetServiceCategoriesByType");
        }
    }
    /// <summary>
    /// Tip bazlı aktif kategorileri getirir (tüm kayıtlar, sayfalama yok, cache).
    /// </summary>
    public async Task<Result<IEnumerable<ServiceCategoryResponse>>> GetActiveServiceCategoriesByTypeAsync(ServiceCategoryType type, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetActiveServiceCategoriesByTypeInternalAsync(type, cancellationToken),
            "GetActiveServiceCategoriesByType",
            new { Type = type },
            cancellationToken);
    }
    
    private async Task<Result<IEnumerable<ServiceCategoryResponse>>> GetActiveServiceCategoriesByTypeInternalAsync(ServiceCategoryType type, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ActiveServiceCategoriesByType(type.ToString());

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var categories = await _unitOfWork.Repository<ServiceCategory>().GetPagedAsync(
                        filter: c => c.IsActive && c.Type == type,
                        orderBy: c => c.DisplayOrder,
                        ascending: true,
                        page: 1,
                        pageSize: 1000, // Büyük sayı ile tüm kayıtları al
                        cancellationToken: cancellationToken);

                    var response = categories.Select(c => new ServiceCategoryResponse(
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Type,
                        c.ImageUrl,
                        c.IconUrl,
                        c.DisplayOrder,
                        c.IsActive,
                        c.Merchants?.Count ?? 0
                    )).ToList();

                    return ServiceResult.Success((IEnumerable<ServiceCategoryResponse>)response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active service categories by type: {Type}", type);
            return ServiceResult.HandleException<IEnumerable<ServiceCategoryResponse>>(ex, _logger, "GetActiveServiceCategoriesByType");
        }
    }
}

