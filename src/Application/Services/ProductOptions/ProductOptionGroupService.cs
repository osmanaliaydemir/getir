using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Getir.Application.Services.ProductOptions;

public class ProductOptionGroupService : IProductOptionGroupService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductOptionGroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PagedResult<ProductOptionGroupResponse>>> GetProductOptionGroupsAsync(
        Guid productId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var optionGroups = await _unitOfWork.Repository<ProductOptionGroup>().GetPagedAsync(
            filter: pog => pog.ProductId == productId,
            orderBy: pog => pog.DisplayOrder,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Options",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<ProductOptionGroup>()
            .CountAsync(pog => pog.ProductId == productId, cancellationToken);

        var responses = optionGroups.Select(pog => new ProductOptionGroupResponse(
            pog.Id,
            pog.ProductId,
            pog.Name,
            pog.Description,
            pog.IsRequired,
            pog.MinSelection,
            pog.MaxSelection,
            pog.DisplayOrder,
            pog.IsActive,
            pog.CreatedAt,
            pog.UpdatedAt,
            pog.Options.Select(o => new ProductOptionResponse(
                o.Id,
                o.ProductOptionGroupId,
                o.Name,
                o.Description,
                o.ExtraPrice,
                o.IsDefault,
                o.IsActive,
                o.DisplayOrder,
                o.CreatedAt,
                o.UpdatedAt)).ToList())).ToList();

        var pagedResult = PagedResult<ProductOptionGroupResponse>.Create(
            responses,
            total,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<ProductOptionGroupResponse>> GetProductOptionGroupAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
            .FirstOrDefaultAsync(pog => pog.Id == id,
                include: "Options",
                cancellationToken: cancellationToken);

        if (optionGroup == null)
        {
            return Result.Fail<ProductOptionGroupResponse>("Product option group not found", "NOT_FOUND");
        }

        var response = new ProductOptionGroupResponse(
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
            optionGroup.Options.Select(o => new ProductOptionResponse(
                o.Id,
                o.ProductOptionGroupId,
                o.Name,
                o.Description,
                o.ExtraPrice,
                o.IsDefault,
                o.IsActive,
                o.DisplayOrder,
                o.CreatedAt,
                o.UpdatedAt)).ToList());

        return Result.Ok(response);
    }

    public async Task<Result<ProductOptionGroupResponse>> CreateProductOptionGroupAsync(
        CreateProductOptionGroupRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify product ownership
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId,
                include: "Merchant",
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail<ProductOptionGroupResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<ProductOptionGroupResponse>("Access denied", "FORBIDDEN");
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

        var response = new ProductOptionGroupResponse(
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
            new List<ProductOptionResponse>());

        return Result.Ok(response);
    }

    public async Task<Result<ProductOptionGroupResponse>> UpdateProductOptionGroupAsync(
        Guid id,
        UpdateProductOptionGroupRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var optionGroup = await _unitOfWork.Repository<ProductOptionGroup>()
            .FirstOrDefaultAsync(pog => pog.Id == id,
                include: "Product.Merchant,Options",
                cancellationToken: cancellationToken);

        if (optionGroup == null)
        {
            return Result.Fail<ProductOptionGroupResponse>("Product option group not found", "NOT_FOUND");
        }

        // Verify product ownership
        if (optionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<ProductOptionGroupResponse>("Access denied", "FORBIDDEN");
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

        var response = new ProductOptionGroupResponse(
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
            optionGroup.Options.Select(o => new ProductOptionResponse(
                o.Id,
                o.ProductOptionGroupId,
                o.Name,
                o.Description,
                o.ExtraPrice,
                o.IsDefault,
                o.IsActive,
                o.DisplayOrder,
                o.CreatedAt,
                o.UpdatedAt)).ToList());

        return Result.Ok(response);
    }

    public async Task<Result> DeleteProductOptionGroupAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
            .FirstOrDefaultAsync(pog => pog.Id == id,
                include: "Product.Merchant",
                cancellationToken: cancellationToken);

        if (optionGroup == null)
        {
            return Result.Fail("Product option group not found", "NOT_FOUND");
        }

        // Verify product ownership
        if (optionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN");
        }

        _unitOfWork.Repository<ProductOptionGroup>().Delete(optionGroup);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<List<ProductOptionGroupResponse>>> GetProductOptionGroupsWithOptionsAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var optionGroups = await _unitOfWork.Repository<ProductOptionGroup>()
            .ListAsync(
                filter: pog => pog.ProductId == productId && pog.IsActive,
                orderBy: pog => pog.DisplayOrder,
                ascending: true,
                include: "Options",
                cancellationToken: cancellationToken);

        var responses = optionGroups.Select(pog => new ProductOptionGroupResponse(
            pog.Id,
            pog.ProductId,
            pog.Name,
            pog.Description,
            pog.IsRequired,
            pog.MinSelection,
            pog.MaxSelection,
            pog.DisplayOrder,
            pog.IsActive,
            pog.CreatedAt,
            pog.UpdatedAt,
            pog.Options.Where(o => o.IsActive).Select(o => new ProductOptionResponse(
                o.Id,
                o.ProductOptionGroupId,
                o.Name,
                o.Description,
                o.ExtraPrice,
                o.IsDefault,
                o.IsActive,
                o.DisplayOrder,
                o.CreatedAt,
                o.UpdatedAt)).ToList())).ToList();

        return Result.Ok(responses);
    }
}
