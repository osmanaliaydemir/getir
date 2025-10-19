using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.ProductCategories;

public class ProductCategoryService : BaseService, IProductCategoryService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    public ProductCategoryService(IUnitOfWork unitOfWork, ILogger<ProductCategoryService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    public async Task<Result<List<ProductCategoryResponse>>> GetMerchantCategoriesAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantCategoriesInternalAsync(merchantId, cancellationToken),
            "GetMerchantCategories",
            new { MerchantId = merchantId },
            cancellationToken);
    }
    private async Task<Result<List<ProductCategoryResponse>>> GetMerchantCategoriesInternalAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.CategoriesByMerchant(merchantId);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var categories = await _unitOfWork.ReadRepository<ProductCategory>()
                        .ListAsync(
                            c => c.MerchantId == merchantId && c.IsActive,
                            include: "ParentCategory,Products",
                            cancellationToken: cancellationToken);

                    var response = categories.Select(c => new ProductCategoryResponse(
                        c.Id,
                        c.MerchantId,
                        "", // MerchantName - optional
                        c.ParentCategoryId,
                        c.ParentCategory?.Name,
                        c.Name,
                        c.Description,
                        c.ImageUrl,
                        c.DisplayOrder,
                        c.IsActive,
                        c.SubCategories?.Count ?? 0,
                        c.Products?.Count ?? 0
                    )).ToList();

                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL for categories
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant categories: {MerchantId}", merchantId);
            return ServiceResult.HandleException<List<ProductCategoryResponse>>(ex, _logger, "GetMerchantCategories");
        }
    }
    public async Task<Result<List<ProductCategoryTreeResponse>>> GetMerchantCategoryTreeAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantCategoryTreeInternalAsync(merchantId, cancellationToken),
            "GetMerchantCategoryTree",
            new { MerchantId = merchantId },
            cancellationToken);
    }
    private async Task<Result<List<ProductCategoryTreeResponse>>> GetMerchantCategoryTreeInternalAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use cache key for category tree
            var cacheKey = $"{CacheKeys.CategoriesByMerchant(merchantId)}:tree";

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var categories = await _unitOfWork.ReadRepository<ProductCategory>()
                        .ListAsync(
                            c => c.MerchantId == merchantId && c.IsActive,
                            cancellationToken: cancellationToken);

                    // Root categories (parent yok)
                    var rootCategories = categories.Where(c => !c.ParentCategoryId.HasValue).ToList();

                    var tree = rootCategories.Select(root => BuildCategoryTree(root, categories.ToList())).ToList();

                    return ServiceResult.Success(tree);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL for category tree
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchant category tree: {MerchantId}", merchantId);
            return ServiceResult.HandleException<List<ProductCategoryTreeResponse>>(ex, _logger, "GetMerchantCategoryTree");
        }
    }
    private ProductCategoryTreeResponse BuildCategoryTree(ProductCategory category, List<ProductCategory> allCategories)
    {
        var subCategories = allCategories
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(sub => BuildCategoryTree(sub, allCategories))
            .ToList();

        return new ProductCategoryTreeResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.Products?.Count ?? 0,
            subCategories
        );
    }
    public async Task<Result<ProductCategoryResponse>> GetProductCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetProductCategoryByIdInternalAsync(id, cancellationToken),
            "GetProductCategoryById",
            new { CategoryId = id },
            cancellationToken);
    }
    private async Task<Result<ProductCategoryResponse>> GetProductCategoryByIdInternalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ProductCategory(id);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var category = await _unitOfWork.Repository<ProductCategory>()
                        .GetAsync(
                            c => c.Id == id,
                            include: "Merchant,ParentCategory,SubCategories,Products",
                            cancellationToken: cancellationToken);

                    if (category == null)
                    {
                        return Result.Fail<ProductCategoryResponse>("Product category not found", "NOT_FOUND_PRODUCT_CATEGORY");
                    }

                    var response = new ProductCategoryResponse(
                        category.Id,
                        category.MerchantId,
                        category.Merchant.Name,
                        category.ParentCategoryId,
                        category.ParentCategory?.Name,
                        category.Name,
                        category.Description,
                        category.ImageUrl,
                        category.DisplayOrder,
                        category.IsActive,
                        category.SubCategories?.Count ?? 0,
                        category.Products?.Count ?? 0
                    );

                    return Result.Ok(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL for single category
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product category by id: {CategoryId}", id);
            return ServiceResult.HandleException<ProductCategoryResponse>(ex, _logger, "GetProductCategoryById");
        }
    }
    public async Task<Result<ProductCategoryResponse>> CreateProductCategoryAsync(CreateProductCategoryRequest request, Guid merchantId, CancellationToken cancellationToken = default)
    {
        // Merchant var mı kontrol et
        var merchantExists = await _unitOfWork.ReadRepository<Merchant>()
            .AnyAsync(m => m.Id == merchantId && m.IsActive, cancellationToken);

        if (!merchantExists)
        {
            return Result.Fail<ProductCategoryResponse>("Merchant not found or inactive", "NOT_FOUND_MERCHANT");
        }

        // Parent category kontrolü
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _unitOfWork.ReadRepository<ProductCategory>()
                .FirstOrDefaultAsync(
                    c => c.Id == request.ParentCategoryId.Value && c.MerchantId == merchantId,
                    cancellationToken: cancellationToken);

            if (parentCategory == null)
            {
                return Result.Fail<ProductCategoryResponse>(
                    "Parent category not found or belongs to different merchant",
                    "INVALID_PARENT_CATEGORY");
            }
        }

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId,
            ParentCategoryId = request.ParentCategoryId,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ProductCategory>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdCategory = await _unitOfWork.Repository<ProductCategory>()
            .GetAsync(
                c => c.Id == category.Id,
                include: "Merchant,ParentCategory",
                cancellationToken: cancellationToken);

        var response = new ProductCategoryResponse(
            createdCategory!.Id,
            createdCategory.MerchantId,
            createdCategory.Merchant.Name,
            createdCategory.ParentCategoryId,
            createdCategory.ParentCategory?.Name,
            createdCategory.Name,
            createdCategory.Description,
            createdCategory.ImageUrl,
            createdCategory.DisplayOrder,
            createdCategory.IsActive,
            0,
            0
        );

        return Result.Ok(response);
    }
    public async Task<Result<ProductCategoryResponse>> UpdateProductCategoryAsync(Guid id, UpdateProductCategoryRequest request, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ProductCategory>()
            .GetAsync(c => c.Id == id, include: "Merchant", cancellationToken: cancellationToken);

        if (category == null)
        {
            return Result.Fail<ProductCategoryResponse>("Product category not found", "NOT_FOUND_PRODUCT_CATEGORY");
        }

        // Sadece merchant sahibi güncelleyebilir
        if (category.Merchant.OwnerId != currentUserId)
        {
            return Result.Fail<ProductCategoryResponse>(
                "You are not authorized to update this category",
                "FORBIDDEN_NOT_OWNER");
        }

        // Parent category kontrolü
        if (request.ParentCategoryId.HasValue)
        {
            if (request.ParentCategoryId == id)
            {
                return Result.Fail<ProductCategoryResponse>(
                    "Category cannot be its own parent",
                    "INVALID_PARENT_CATEGORY");
            }

            var parentCategory = await _unitOfWork.ReadRepository<ProductCategory>()
                .FirstOrDefaultAsync(
                    c => c.Id == request.ParentCategoryId.Value && c.MerchantId == category.MerchantId,
                    cancellationToken: cancellationToken);

            if (parentCategory == null)
            {
                return Result.Fail<ProductCategoryResponse>(
                    "Parent category not found or belongs to different merchant",
                    "INVALID_PARENT_CATEGORY");
            }
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        category.ImageUrl = request.ImageUrl;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ProductCategory>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single category cache
        await _cacheService.RemoveAsync(CacheKeys.ProductCategory(id), cancellationToken);

        // Invalidate all category lists and tree for this merchant
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllCategoriesPattern(),
            cancellationToken);

        var updatedCategory = await _unitOfWork.Repository<ProductCategory>()
            .GetAsync(
                c => c.Id == id,
                include: "Merchant,ParentCategory,SubCategories,Products",
                cancellationToken: cancellationToken);

        var response = new ProductCategoryResponse(
            updatedCategory!.Id,
            updatedCategory.MerchantId,
            updatedCategory.Merchant.Name,
            updatedCategory.ParentCategoryId,
            updatedCategory.ParentCategory?.Name,
            updatedCategory.Name,
            updatedCategory.Description,
            updatedCategory.ImageUrl,
            updatedCategory.DisplayOrder,
            updatedCategory.IsActive,
            updatedCategory.SubCategories?.Count ?? 0,
            updatedCategory.Products?.Count ?? 0
        );

        return Result.Ok(response);
    }
    public async Task<Result> DeleteProductCategoryAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ProductCategory>()
            .GetAsync(
                c => c.Id == id,
                include: "Merchant,SubCategories,Products",
                cancellationToken: cancellationToken);

        if (category == null)
        {
            return Result.Fail("Product category not found", "NOT_FOUND_PRODUCT_CATEGORY");
        }

        // Sadece merchant sahibi silebilir
        if (category.Merchant.OwnerId != currentUserId)
        {
            return Result.Fail("You are not authorized to delete this category", "FORBIDDEN_NOT_OWNER");
        }

        // Alt kategorisi var mı kontrol et
        if (category.SubCategories != null && category.SubCategories.Any())
        {
            return Result.Fail(
                "Cannot delete category with sub-categories. Delete sub-categories first.",
                "HAS_SUB_CATEGORIES");
        }

        // Ürünü var mı kontrol et
        if (category.Products != null && category.Products.Any())
        {
            return Result.Fail(
                "Cannot delete category with products. Move or delete products first.",
                "HAS_PRODUCTS");
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ProductCategory>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single category cache
        await _cacheService.RemoveAsync(CacheKeys.ProductCategory(id), cancellationToken);

        // Invalidate all category lists and tree for this merchant
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllCategoriesPattern(),
            cancellationToken);

        return Result.Ok();
    }
}

