using Microsoft.Extensions.Logging;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.ProductOptions;

/// <summary>
/// Ürün opsiyon grubu servisi: ürün seçenekleri gruplarının yönetimi, ownership kontrolü, sıralama.
/// </summary>
public class ProductOptionGroupService : BaseService, IProductOptionGroupService
{
    public ProductOptionGroupService(IUnitOfWork unitOfWork, ILogger<ProductOptionGroupService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    /// <summary>
    /// Ürüne ait opsiyon gruplarını sayfalama ile getirir (opsiyonlar dahil, performance tracking).
    /// </summary>
    public async Task<Result<PagedResult<ProductOptionGroupResponse>>> GetProductOptionGroupsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetProductOptionGroupsInternalAsync(productId, query, cancellationToken),
            "GetProductOptionGroups",
            new { ProductId = productId, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    private async Task<Result<PagedResult<ProductOptionGroupResponse>>> GetProductOptionGroupsInternalAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken)
    {
        try
        {
            // Verify product exists and user has access
            var product = await _unitOfWork.ReadRepository<Product>()
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken: cancellationToken);

            if (product == null)
            {
                return Result.Fail<PagedResult<ProductOptionGroupResponse>>("Product not found", "PRODUCT_NOT_FOUND");
            }

            var optionGroups = await _unitOfWork.ReadRepository<ProductOptionGroup>()
                .ListAsync(
                    pog => pog.ProductId == productId && pog.IsActive,
                    orderBy: pog => pog.DisplayOrder,
                    include: "Options",
                    cancellationToken: cancellationToken);

            var responses = optionGroups.Select(MapToResponse).ToList();

            var pagedResult = PagedResult<ProductOptionGroupResponse>.Create(
                responses,
                responses.Count,
                query.Page,
                query.PageSize);

            return Result.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product option groups for product {ProductId}", productId);
            return Result.Fail<PagedResult<ProductOptionGroupResponse>>("Failed to get product option groups", "GET_OPTION_GROUPS_ERROR");
        }
    }
    public async Task<Result<ProductOptionGroupResponse>> GetProductOptionGroupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetProductOptionGroupInternalAsync(id, cancellationToken),
            "GetProductOptionGroup",
            new { Id = id },
            cancellationToken);
    }
    private async Task<Result<ProductOptionGroupResponse>> GetProductOptionGroupInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
                .FirstOrDefaultAsync(pog => pog.Id == id, include: "Options", cancellationToken: cancellationToken);

            if (optionGroup == null)
            {
                return Result.Fail<ProductOptionGroupResponse>("Product option group not found", "OPTION_GROUP_NOT_FOUND");
            }

            var response = MapToResponse(optionGroup);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product option group {Id}", id);
            return Result.Fail<ProductOptionGroupResponse>("Failed to get product option group", "GET_OPTION_GROUP_ERROR");
        }
    }
    public async Task<Result<ProductOptionGroupResponse>> CreateProductOptionGroupAsync(CreateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CreateProductOptionGroupInternalAsync(request, merchantOwnerId, cancellationToken),
            "CreateProductOptionGroup",
            new { ProductId = request.ProductId, Name = request.Name },
            cancellationToken);
    }
    private async Task<Result<ProductOptionGroupResponse>> CreateProductOptionGroupInternalAsync(CreateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            // Verify product exists and user owns it
            var product = await _unitOfWork.ReadRepository<Product>()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.Merchant.OwnerId == merchantOwnerId,
                    include: "Merchant", cancellationToken: cancellationToken);

            if (product == null)
            {
                return Result.Fail<ProductOptionGroupResponse>("Product not found or access denied", "PRODUCT_NOT_FOUND");
            }

            var optionGroup = new ProductOptionGroup
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Name = request.Name,
                Description = request.Description,
                IsRequired = request.IsRequired,
                MinSelection = request.MinSelection,
                MaxSelection = request.MaxSelection,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ProductOptionGroup>().AddAsync(optionGroup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(optionGroup);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product option group for product {ProductId}", request.ProductId);
            return Result.Fail<ProductOptionGroupResponse>("Failed to create product option group", "CREATE_OPTION_GROUP_ERROR");
        }
    }
    public async Task<Result<ProductOptionGroupResponse>> UpdateProductOptionGroupAsync(Guid id, UpdateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await UpdateProductOptionGroupInternalAsync(id, request, merchantOwnerId, cancellationToken),
            "UpdateProductOptionGroup",
            new { Id = id, Name = request.Name },
            cancellationToken);
    }
    private async Task<Result<ProductOptionGroupResponse>> UpdateProductOptionGroupInternalAsync(Guid id, UpdateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
                .FirstOrDefaultAsync(pog => pog.Id == id, include: "Product.Merchant", cancellationToken: cancellationToken);

            if (optionGroup == null)
            {
                return Result.Fail<ProductOptionGroupResponse>("Product option group not found", "OPTION_GROUP_NOT_FOUND");
            }

            if (optionGroup.Product.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail<ProductOptionGroupResponse>("Access denied", "ACCESS_DENIED");
            }

            optionGroup.Name = request.Name;
            optionGroup.Description = request.Description;
            optionGroup.IsRequired = request.IsRequired;
            optionGroup.MinSelection = request.MinSelection;
            optionGroup.MaxSelection = request.MaxSelection;
            optionGroup.DisplayOrder = request.DisplayOrder;
            optionGroup.IsActive = request.IsActive;
            optionGroup.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductOptionGroup>().Update(optionGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(optionGroup);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product option group {Id}", id);
            return Result.Fail<ProductOptionGroupResponse>("Failed to update product option group", "UPDATE_OPTION_GROUP_ERROR");
        }
    }
    public async Task<Result> DeleteProductOptionGroupAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await DeleteProductOptionGroupInternalAsync(id, merchantOwnerId, cancellationToken),
            "DeleteProductOptionGroup",
            new { Id = id },
            cancellationToken);
    }
    private async Task<Result> DeleteProductOptionGroupInternalAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
                .FirstOrDefaultAsync(pog => pog.Id == id, include: "Product.Merchant", cancellationToken: cancellationToken);

            if (optionGroup == null)
            {
                return Result.Fail("Product option group not found", "OPTION_GROUP_NOT_FOUND");
            }

            if (optionGroup.Product.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail("Access denied", "ACCESS_DENIED");
            }

            // Soft delete
            optionGroup.IsActive = false;
            optionGroup.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductOptionGroup>().Update(optionGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product option group {Id}", id);
            return Result.Fail("Failed to delete product option group", "DELETE_OPTION_GROUP_ERROR");
        }
    }
    /// <summary>
    /// Opsiyon gruplarının sırasını yeniden düzenler (ownership kontrolü, DisplayOrder güncelleme, performance tracking).
    /// </summary>
    public async Task<Result> ReorderProductOptionGroupsAsync(Guid productId, List<Guid> orderedGroupIds, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await ReorderProductOptionGroupsInternalAsync(productId, orderedGroupIds, merchantOwnerId, cancellationToken),
            "ReorderProductOptionGroups",
            new { ProductId = productId, GroupCount = orderedGroupIds.Count },
            cancellationToken);
    }
    private async Task<Result> ReorderProductOptionGroupsInternalAsync(Guid productId, List<Guid> orderedGroupIds, Guid merchantOwnerId, CancellationToken cancellationToken)
    {
        try
        {
            // Verify product exists and user owns it
            var product = await _unitOfWork.ReadRepository<Product>()
                .FirstOrDefaultAsync(p => p.Id == productId && p.Merchant.OwnerId == merchantOwnerId,
                    include: "Merchant", cancellationToken: cancellationToken);

            if (product == null)
            {
                return Result.Fail("Product not found or access denied", "PRODUCT_NOT_FOUND");
            }

            var optionGroups = await _unitOfWork.ReadRepository<ProductOptionGroup>()
                .ListAsync(pog => pog.ProductId == productId && pog.IsActive, cancellationToken: cancellationToken);

            foreach (var optionGroup in optionGroups)
            {
                var newOrder = orderedGroupIds.IndexOf(optionGroup.Id);
                if (newOrder >= 0)
                {
                    optionGroup.DisplayOrder = newOrder;
                    optionGroup.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<ProductOptionGroup>().Update(optionGroup);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering product option groups for product {ProductId}", productId);
            return Result.Fail("Failed to reorder product option groups", "REORDER_OPTION_GROUPS_ERROR");
        }
    }
    private static ProductOptionGroupResponse MapToResponse(ProductOptionGroup optionGroup)
    {
        return new ProductOptionGroupResponse(
            optionGroup.Id,
            optionGroup.ProductId,
            optionGroup.Name,
            optionGroup.Description,
            optionGroup.IsRequired,
            optionGroup.MinSelection,
            optionGroup.MaxSelection,
            optionGroup.DisplayOrder,
            optionGroup.IsActive,
            optionGroup.CreatedAt,
            optionGroup.UpdatedAt,
            optionGroup.Options?.Select(MapOptionToResponse).ToList() ?? new List<ProductOptionResponse>());
    }
    private static ProductOptionResponse MapOptionToResponse(ProductOption option)
    {
        return new ProductOptionResponse(
            option.Id,
            option.ProductOptionGroupId,
            option.Name,
            option.Description,
            option.ExtraPrice,
            option.IsDefault,
            option.DisplayOrder,
            option.IsActive,
            option.CreatedAt,
            option.UpdatedAt);
    }
}
