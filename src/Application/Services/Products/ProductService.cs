// System namespaces
using Microsoft.Extensions.Logging;

// Application namespaces
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;

// Domain namespaces
using Getir.Domain.Entities;

namespace Getir.Application.Services.Products;

/// <summary>
/// Service for managing products and their operations
/// </summary>
public class ProductService : BaseService, IProductService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    /// <summary>
    /// Initializes a new instance of the ProductService class
    /// </summary>
    /// <param name="unitOfWork">The unit of work for database operations</param>
    /// <param name="logger">The logger for logging operations</param>
    /// <param name="loggingService">The logging service for structured logging</param>
    /// <param name="cacheService">The cache service for caching operations</param>
    /// <param name="backgroundTaskService">The background task service for async operations</param>
    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    public async Task<Result<PagedResult<ProductResponse>>> GetProductsByMerchantAsync(Guid merchantId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetProductsByMerchantInternalAsync(merchantId, query, cancellationToken),
            "GetProductsByMerchant",
            new { MerchantId = merchantId, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    private async Task<Result<PagedResult<ProductResponse>>> GetProductsByMerchantInternalAsync(Guid merchantId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ProductsByMerchant(merchantId, query.Page, query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var products = await _unitOfWork.Repository<Product>().GetPagedAsync(
                        filter: p => p.MerchantId == merchantId && p.IsActive,
                        orderBy: p => p.DisplayOrder,
                        ascending: true,
                        page: query.Page,
                        pageSize: query.PageSize,
                        include: "Merchant",
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<Product>()
                        .CountAsync(p => p.MerchantId == merchantId && p.IsActive, cancellationToken);

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
                TimeSpan.FromMinutes(CacheKeys.TTL.Medium), // 15 minutes TTL for product lists
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting products by merchant", ex, new { MerchantId = merchantId, Page = query.Page, PageSize = query.PageSize });
            return ServiceResult.HandleException<PagedResult<ProductResponse>>(ex, _logger, "GetProductsByMerchant");
        }
    }
    public async Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Cache-Aside Pattern: Try cache first
        var cacheKey = CacheKeys.Product(id);

        return await GetOrSetCacheAsync(
            cacheKey,
            async () =>
            {
                var product = await _unitOfWork.Repository<Product>()
                    .GetAsync(p => p.Id == id, include: "Merchant", cancellationToken: cancellationToken);

                if (product == null)
                {
                    return Result.Fail<ProductResponse>("Product not found", "NOT_FOUND_PRODUCT");
                }

                var response = new ProductResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    IsActive = product.IsActive,
                    IsDeleted = false,
                    MerchantId = product.MerchantId,
                    MerchantName = product.Merchant.Name,
                    ProductCategoryId = product.ProductCategoryId,
                    ProductCategoryName = product.ProductCategory?.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    DiscountedPrice = product.DiscountedPrice,
                    StockQuantity = product.StockQuantity,
                    Unit = product.Unit,
                    IsAvailable = product.IsAvailable
                };

                return Result.Ok(response);
            },
            TimeSpan.FromMinutes(CacheKeys.TTL.Medium), // 15 minutes TTL for single product
            cancellationToken);
    }
    public async Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var merchantExists = await _unitOfWork.ReadRepository<Merchant>()
            .AnyAsync(m => m.Id == request.MerchantId, cancellationToken);

        if (!merchantExists)
        {
            return Result.Fail<ProductResponse>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            ProductCategoryId = request.ProductCategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountedPrice = request.DiscountedPrice,
            StockQuantity = request.StockQuantity,
            Unit = request.Unit,
            IsAvailable = request.StockQuantity > 0,
            IsActive = true,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdProduct = await _unitOfWork.Repository<Product>()
            .GetAsync(p => p.Id == product.Id, include: "Merchant", cancellationToken: cancellationToken);

        var response = new ProductResponse
        {
            Id = createdProduct!.Id,
            Name = createdProduct.Name,
            Description = createdProduct.Description,
            CreatedAt = createdProduct.CreatedAt,
            UpdatedAt = createdProduct.UpdatedAt,
            IsActive = createdProduct.IsActive,
            IsDeleted = false,
            MerchantId = createdProduct.MerchantId,
            MerchantName = createdProduct.Merchant.Name,
            ProductCategoryId = createdProduct.ProductCategoryId,
            ProductCategoryName = createdProduct.ProductCategory?.Name,
            ImageUrl = createdProduct.ImageUrl,
            Price = createdProduct.Price,
            DiscountedPrice = createdProduct.DiscountedPrice,
            StockQuantity = createdProduct.StockQuantity,
            Unit = createdProduct.Unit,
            IsAvailable = createdProduct.IsAvailable
        };

        return Result.Ok(response);
    }
    public async Task<Result<ProductResponse>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return Result.Fail<ProductResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountedPrice = request.DiscountedPrice;
        product.StockQuantity = request.StockQuantity;
        product.Unit = request.Unit;
        product.IsAvailable = request.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single product cache
        await _cacheService.RemoveAsync(CacheKeys.Product(id), cancellationToken);

        // Invalidate all product lists for this merchant (pattern-based removal)
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllProductsByMerchant(product.MerchantId),
            cancellationToken);

        var updatedProduct = await _unitOfWork.Repository<Product>()
            .GetAsync(p => p.Id == id, include: "Merchant", cancellationToken: cancellationToken);

        var response = new ProductResponse
        {
            Id = updatedProduct!.Id,
            Name = updatedProduct.Name,
            Description = updatedProduct.Description,
            CreatedAt = updatedProduct.CreatedAt,
            UpdatedAt = updatedProduct.UpdatedAt,
            IsActive = updatedProduct.IsActive,
            IsDeleted = false,
            MerchantId = updatedProduct.MerchantId,
            MerchantName = updatedProduct.Merchant.Name,
            ProductCategoryId = updatedProduct.ProductCategoryId,
            ProductCategoryName = updatedProduct.ProductCategory?.Name,
            ImageUrl = updatedProduct.ImageUrl,
            Price = updatedProduct.Price,
            DiscountedPrice = updatedProduct.DiscountedPrice,
            StockQuantity = updatedProduct.StockQuantity,
            Unit = updatedProduct.Unit,
            IsAvailable = updatedProduct.IsAvailable
        };

        return Result.Ok(response);
    }
    public async Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single product cache
        await _cacheService.RemoveAsync(CacheKeys.Product(id), cancellationToken);

        // Invalidate all product lists for this merchant
        await _cacheService.RemoveByPatternAsync(
            CacheKeys.AllProductsByMerchant(product.MerchantId),
            cancellationToken);

        return Result.Ok();
    }
    // Merchant-specific methods
    public async Task<Result<PagedResult<ProductResponse>>> GetMyProductsAsync(Guid merchantOwnerId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get merchant owned by this user
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<PagedResult<ProductResponse>>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var products = await _unitOfWork.Repository<Product>().GetPagedAsync(
            filter: p => p.MerchantId == merchant.Id,
            orderBy: p => p.DisplayOrder,
            ascending: query.IsAscending,
            page: query.Page,
            pageSize: query.PageSize,
            include: "ProductCategory",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.MerchantId == merchant.Id, cancellationToken);

        var responses = products.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            IsActive = p.IsActive,
            IsDeleted = false,
            MerchantId = p.MerchantId,
            MerchantName = merchant.Name,
            ProductCategoryId = p.ProductCategoryId,
            ProductCategoryName = p.ProductCategory?.Name,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            StockQuantity = p.StockQuantity,
            Unit = p.Unit,
            IsAvailable = p.IsAvailable
        }).ToList();

        var pagedResult = PagedResult<ProductResponse>.Create(
            responses,
            total,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }
    public async Task<Result<ProductResponse>> CreateMyProductAsync(CreateProductRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify merchant ownership
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.OwnerId == merchantOwnerId,
                cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<ProductResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            ProductCategoryId = request.ProductCategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountedPrice = request.DiscountedPrice,
            StockQuantity = request.StockQuantity,
            Unit = request.Unit,
            IsAvailable = true,
            IsActive = true,
            DisplayOrder = 0, // Will be set by merchant
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            IsActive = product.IsActive,
            IsDeleted = false,
            MerchantId = product.MerchantId,
            MerchantName = merchant.Name,
            ProductCategoryId = product.ProductCategoryId,
            ProductCategoryName = null, // Will be loaded if needed
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            DiscountedPrice = product.DiscountedPrice,
            StockQuantity = product.StockQuantity,
            Unit = product.Unit,
            IsAvailable = product.IsAvailable
        };

        return Result.Ok(response);
    }
    public async Task<Result<ProductResponse>> UpdateMyProductAsync(Guid id, UpdateProductRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id,
                include: "Merchant,ProductCategory",
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail<ProductResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<ProductResponse>("Access denied", "FORBIDDEN_PRODUCT");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountedPrice = request.DiscountedPrice;
        product.StockQuantity = request.StockQuantity;
        product.Unit = request.Unit;
        product.IsAvailable = request.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            IsActive = product.IsActive,
            IsDeleted = false,
            MerchantId = product.MerchantId,
            MerchantName = product.Merchant.Name,
            ProductCategoryId = product.ProductCategoryId,
            ProductCategoryName = product.ProductCategory?.Name,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            DiscountedPrice = product.DiscountedPrice,
            StockQuantity = product.StockQuantity,
            Unit = product.Unit,
            IsAvailable = product.IsAvailable
        };

        return Result.Ok(response);
    }
    public async Task<Result> DeleteMyProductAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id,
                include: "Merchant",
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCT");
        }

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    public async Task<Result> UpdateProductStockAsync(Guid id, int newStockQuantity, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id,
                include: "Merchant",
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCT");
        }

        product.StockQuantity = newStockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    public async Task<Result> ToggleProductAvailabilityAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id,
                include: "Merchant",
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCT");
        }

        product.IsAvailable = !product.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
    public async Task<Result> BulkUpdateProductOrderAsync(List<UpdateProductOrderRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);

        if (!requests.Any())
        {
            return Result.Fail("No products to update", "EMPTY_REQUEST");
        }

        var productIds = requests.Select(r => r.ProductId).ToList();
        var products = await _unitOfWork.Repository<Product>().ListAsync(
            filter: p => productIds.Contains(p.Id),
            include: "Merchant",
            cancellationToken: cancellationToken);

        if (products.Count != requests.Count)
        {
            return Result.Fail("Some products not found", "NOT_FOUND_PRODUCTS");
        }

        // Verify all products belong to the merchant
        if (products.Any(p => p.Merchant.OwnerId != merchantOwnerId))
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCTS");
        }

        // Update display orders
        foreach (var request in requests)
        {
            var product = products.First(p => p.Id == request.ProductId);
            product.DisplayOrder = request.NewDisplayOrder;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Product>().Update(product);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    #region Additional Merchant Product Methods
    public async Task<Result<ProductStatisticsResponse>> GetMyProductStatisticsAsync(Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMyProductStatisticsInternalAsync(merchantOwnerId, cancellationToken),
            "GetMyProductStatistics",
            new { merchantOwnerId },
            cancellationToken);
    }
    private async Task<Result<ProductStatisticsResponse>> GetMyProductStatisticsInternalAsync(Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        // Simplified statistics implementation
        var response = new ProductStatisticsResponse(
            TotalProducts: 0,
            ActiveProducts: 0,
            InactiveProducts: 0,
            OutOfStockProducts: 0,
            LowStockProducts: 0,
            TotalInventoryValue: 0,
            LastUpdated: DateTime.UtcNow
        );

        return Result.Ok(response);
    }
    public async Task<Result<BulkUpdateProductStatusResponse>> BulkUpdateMyProductStatusAsync(BulkUpdateProductStatusRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await BulkUpdateMyProductStatusInternalAsync(request, merchantOwnerId, cancellationToken),
            "BulkUpdateMyProductStatus",
            new { merchantOwnerId, ProductCount = request.ProductIds.Count },
            cancellationToken);
    }
    private async Task<Result<BulkUpdateProductStatusResponse>> BulkUpdateMyProductStatusInternalAsync(BulkUpdateProductStatusRequest request, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        // Simplified bulk update implementation
        var response = new BulkUpdateProductStatusResponse(
            TotalUpdated: 0,
            SuccessCount: 0,
            FailureCount: 0,
            UpdatedProductIds: new List<Guid>(),
            Errors: new List<string>()
        );

        return Result.Ok(response);
    }
    #endregion

    #region Additional Methods
    public async Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(string searchQuery, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await SearchProductsInternalAsync(searchQuery, query, cancellationToken),
            "SearchProducts",
            new { searchQuery, query.Page, query.PageSize },
            cancellationToken);
    }
    private async Task<Result<PagedResult<ProductResponse>>> SearchProductsInternalAsync(string searchQuery, PaginationQuery query, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.ReadRepository<Product>()
            .GetPagedAsync(
                filter: p => p.IsActive && p.IsAvailable && (p.Name.Contains(searchQuery) || (p.Description != null && p.Description.Contains(searchQuery))),
                page: query.Page,
                pageSize: query.PageSize,
                orderBy: p => p.Name,
                ascending: true,
                cancellationToken: cancellationToken);

        var totalCount = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.IsActive && p.IsAvailable && (p.Name.Contains(searchQuery) || (p.Description != null && p.Description.Contains(searchQuery))), cancellationToken);

        var responses = (IReadOnlyList<ProductResponse>)products.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountedPrice = p.DiscountedPrice,
            MerchantId = p.MerchantId,
            StockQuantity = p.StockQuantity,
            IsAvailable = p.IsAvailable,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return Result.Ok(PagedResult<ProductResponse>.Create(responses, totalCount, query.Page, query.PageSize));
    }
    #endregion
}

// Background task data classes
public class ProductCreatedTask
{
    public Guid ProductId { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProductUpdatedTask
{
    public Guid ProductId { get; set; }
    public Guid MerchantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
}
