using Microsoft.Extensions.Logging;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.ProductOptions;

public class MarketProductVariantService : BaseService, IMarketProductVariantService
{
    public MarketProductVariantService(IUnitOfWork unitOfWork, ILogger<MarketProductVariantService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    public async Task<Result<PagedResult<MarketProductVariantResponse>>> GetProductVariantsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetProductVariantsInternalAsync(productId, query, cancellationToken),
            "GetProductVariants",
            new { ProductId = productId, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    private async Task<Result<PagedResult<MarketProductVariantResponse>>> GetProductVariantsInternalAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Verify product exists
            var product = await _unitOfWork.ReadRepository<MarketProduct>()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken: cancellationToken);

            if (product == null)
            {
                return Result.Fail<PagedResult<MarketProductVariantResponse>>("Product not found", "PRODUCT_NOT_FOUND");
            }

            var variants = await _unitOfWork.ReadRepository<MarketProductVariant>()
                .ListAsync(
                    v => v.ProductId == productId,
                    orderBy: v => v.DisplayOrder,
                    cancellationToken: cancellationToken);

            var responses = variants.Select(MapToResponse).ToList();

            var pagedResult = PagedResult<MarketProductVariantResponse>.Create(
                responses,
                responses.Count,
                query.Page,
                query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product variants for product {ProductId}", productId);
            return Result.Fail<PagedResult<MarketProductVariantResponse>>("Failed to get product variants", "GET_VARIANTS_ERROR");
        }
    }
    public async Task<Result<MarketProductVariantResponse>> GetProductVariantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetProductVariantInternalAsync(id, cancellationToken),
            "GetProductVariant",
            new { Id = id },
            cancellationToken);
    }
    private async Task<Result<MarketProductVariantResponse>> GetProductVariantInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken: cancellationToken);

            if (variant == null)
            {
                return Result.Fail<MarketProductVariantResponse>("Product variant not found", "VARIANT_NOT_FOUND");
            }

            var response = MapToResponse(variant);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product variant {Id}", id);
            return Result.Fail<MarketProductVariantResponse>("Failed to get product variant", "GET_VARIANT_ERROR");
        }
    }
    public async Task<Result<MarketProductVariantResponse>> CreateProductVariantAsync(CreateMarketProductVariantRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CreateProductVariantInternalAsync(request, merchantOwnerId, cancellationToken),
            "CreateProductVariant",
            new { ProductId = request.ProductId, Name = request.Name },
            cancellationToken);
    }
    private async Task<Result<MarketProductVariantResponse>> CreateProductVariantInternalAsync(CreateMarketProductVariantRequest request, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            // Verify product exists and user owns it
            var product = await _unitOfWork.ReadRepository<MarketProduct>()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.Market.Merchant.OwnerId == merchantOwnerId,
                    include: "Market.Merchant", cancellationToken: cancellationToken);

            if (product == null)
            {
                return Result.Fail<MarketProductVariantResponse>("Product not found or access denied", "PRODUCT_NOT_FOUND");
            }

            // Check if variant with same SKU already exists
            if (!string.IsNullOrEmpty(request.SKU))
            {
                var existingVariant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                    .FirstOrDefaultAsync(v => v.SKU == request.SKU, cancellationToken: cancellationToken);

                if (existingVariant != null)
                {
                    return Result.Fail<MarketProductVariantResponse>("Variant with this SKU already exists", "DUPLICATE_SKU");
                }
            }

            var variant = new MarketProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Name = request.Name,
                Description = request.Description,
                SKU = request.SKU,
                Price = request.Price,
                DiscountedPrice = request.DiscountedPrice,
                StockQuantity = request.StockQuantity,
                IsAvailable = true,
                DisplayOrder = request.DisplayOrder,
                Size = request.Size,
                Color = request.Color,
                Flavor = request.Flavor,
                Material = request.Material,
                Weight = request.Weight,
                Volume = request.Volume,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<MarketProductVariant>().AddAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(variant);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product variant for product {ProductId}", request.ProductId);
            return Result.Fail<MarketProductVariantResponse>("Failed to create product variant", "CREATE_VARIANT_ERROR");
        }
    }
    public async Task<Result<MarketProductVariantResponse>> UpdateProductVariantAsync(Guid id, UpdateMarketProductVariantRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await UpdateProductVariantInternalAsync(id, request, merchantOwnerId, cancellationToken),
            "UpdateProductVariant",
            new { Id = id, Name = request.Name },
            cancellationToken);
    }
    private async Task<Result<MarketProductVariantResponse>> UpdateProductVariantInternalAsync(Guid id, UpdateMarketProductVariantRequest request, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                .FirstOrDefaultAsync(v => v.Id == id, include: "Product.Market.Merchant", cancellationToken: cancellationToken);

            if (variant == null)
            {
                return Result.Fail<MarketProductVariantResponse>("Product variant not found", "VARIANT_NOT_FOUND");
            }

            if (variant.Product.Market.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail<MarketProductVariantResponse>("Access denied", "ACCESS_DENIED");
            }

            // Check if SKU is being changed and if new SKU already exists
            if (!string.IsNullOrEmpty(request.SKU) && request.SKU != variant.SKU)
            {
                var existingVariant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                    .FirstOrDefaultAsync(v => v.SKU == request.SKU && v.Id != id, cancellationToken: cancellationToken);

                if (existingVariant != null)
                {
                    return Result.Fail<MarketProductVariantResponse>("Variant with this SKU already exists", "DUPLICATE_SKU");
                }
            }

            variant.Name = request.Name;
            variant.Description = request.Description;
            variant.SKU = request.SKU;
            variant.Price = request.Price;
            variant.DiscountedPrice = request.DiscountedPrice;
            variant.StockQuantity = request.StockQuantity;
            variant.IsAvailable = request.IsAvailable;
            variant.DisplayOrder = request.DisplayOrder;
            variant.Size = request.Size;
            variant.Color = request.Color;
            variant.Flavor = request.Flavor;
            variant.Material = request.Material;
            variant.Weight = request.Weight;
            variant.Volume = request.Volume;
            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<MarketProductVariant>().Update(variant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(variant);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product variant {Id}", id);
            return Result.Fail<MarketProductVariantResponse>("Failed to update product variant", "UPDATE_VARIANT_ERROR");
        }
    }
    public async Task<Result> DeleteProductVariantAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await DeleteProductVariantInternalAsync(id, merchantOwnerId, cancellationToken),
            "DeleteProductVariant",
            new { Id = id },
            cancellationToken);
    }
    private async Task<Result> DeleteProductVariantInternalAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                .FirstOrDefaultAsync(v => v.Id == id, include: "Product.Market.Merchant", cancellationToken: cancellationToken);

            if (variant == null)
            {
                return Result.Fail("Product variant not found", "VARIANT_NOT_FOUND");
            }

            if (variant.Product.Market.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail("Access denied", "ACCESS_DENIED");
            }

            _unitOfWork.Repository<MarketProductVariant>().Delete(variant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product variant {Id}", id);
            return Result.Fail("Failed to delete product variant", "DELETE_VARIANT_ERROR");
        }
    }
    public async Task<Result> UpdateVariantStockAsync(Guid id, int newStockQuantity, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await UpdateVariantStockInternalAsync(id, newStockQuantity, merchantOwnerId, cancellationToken),
            "UpdateVariantStock",
            new { Id = id, NewStock = newStockQuantity },
            cancellationToken);
    }
    private async Task<Result> UpdateVariantStockInternalAsync(Guid id, int newStockQuantity, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            var variant = await _unitOfWork.ReadRepository<MarketProductVariant>()
                .FirstOrDefaultAsync(v => v.Id == id, include: "Product.Market.Merchant", cancellationToken: cancellationToken);

            if (variant == null)
            {
                return Result.Fail("Product variant not found", "VARIANT_NOT_FOUND");
            }

            if (variant.Product.Market.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail("Access denied", "ACCESS_DENIED");
            }

            variant.StockQuantity = newStockQuantity;
            variant.IsAvailable = newStockQuantity > 0;
            variant.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<MarketProductVariant>().Update(variant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating variant stock for {Id}", id);
            return Result.Fail("Failed to update variant stock", "UPDATE_STOCK_ERROR");
        }
    }
    public async Task<Result> BulkUpdateVariantStockAsync(List<UpdateVariantStockRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await BulkUpdateVariantStockInternalAsync(requests, merchantOwnerId, cancellationToken),
            "BulkUpdateVariantStock",
            new { RequestCount = requests.Count },
            cancellationToken);
    }
    private async Task<Result> BulkUpdateVariantStockInternalAsync(List<UpdateVariantStockRequest> requests, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            var variantIds = requests.Select(r => r.VariantId).ToList();
            var variants = await _unitOfWork.ReadRepository<MarketProductVariant>()
                .ListAsync(v => variantIds.Contains(v.Id),
                    include: "Product.Market.Merchant",
                    cancellationToken: cancellationToken);

            if (variants.Count != requests.Count)
            {
                return Result.Fail("Some variants not found", "VARIANTS_NOT_FOUND");
            }

            // Verify ownership for all variants
            var firstVariant = variants.First();
            if (firstVariant.Product.Market.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail("Access denied", "ACCESS_DENIED");
            }

            foreach (var variant in variants)
            {
                var request = requests.First(r => r.VariantId == variant.Id);
                variant.StockQuantity = request.NewStockQuantity;
                variant.IsAvailable = request.NewStockQuantity > 0;
                variant.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<MarketProductVariant>().Update(variant);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating variant stock");
            return Result.Fail("Failed to bulk update variant stock", "BULK_UPDATE_STOCK_ERROR");
        }
    }
    private static MarketProductVariantResponse MapToResponse(MarketProductVariant variant)
    {
        return new MarketProductVariantResponse(
            variant.Id,
            variant.ProductId,
            variant.Name,
            variant.Description,
            variant.SKU,
            variant.Price,
            variant.DiscountedPrice,
            variant.StockQuantity,
            variant.IsAvailable,
            variant.DisplayOrder,
            variant.Size,
            variant.Color,
            variant.Flavor,
            variant.Material,
            variant.Weight,
            variant.Volume,
            variant.CreatedAt,
            variant.UpdatedAt);
    }
}
