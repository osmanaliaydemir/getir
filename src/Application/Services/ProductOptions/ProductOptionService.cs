using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.ProductOptions;

public class ProductOptionService : IProductOptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductOptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<PagedResult<ProductOptionResponse>>> GetProductOptionsAsync(
        Guid productOptionGroupId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var options = await _unitOfWork.Repository<ProductOption>().GetPagedAsync(
            filter: po => po.ProductOptionGroupId == productOptionGroupId,
            orderBy: po => po.DisplayOrder,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<ProductOption>()
            .CountAsync(po => po.ProductOptionGroupId == productOptionGroupId, cancellationToken);

        var responses = options.Select(po => new ProductOptionResponse(
            po.Id,
            po.ProductOptionGroupId,
            po.Name,
            po.Description,
            po.ExtraPrice,
            po.IsDefault,
            po.IsActive,
            po.DisplayOrder,
            po.CreatedAt,
            po.UpdatedAt)).ToList();

        var pagedResult = PagedResult<ProductOptionResponse>.Create(
            responses,
            total,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<ProductOptionResponse>> GetProductOptionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var option = await _unitOfWork.ReadRepository<ProductOption>()
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken: cancellationToken);

        if (option == null)
        {
            return Result.Fail<ProductOptionResponse>("Product option not found", "NOT_FOUND");
        }

        var response = new ProductOptionResponse(
            option.Id,
            option.ProductOptionGroupId,
            option.Name,
            option.Description,
            option.ExtraPrice,
            option.IsDefault,
            option.IsActive,
            option.DisplayOrder,
            option.CreatedAt,
            option.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result<ProductOptionResponse>> CreateProductOptionAsync(
        CreateProductOptionRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify option group ownership
        var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
            .FirstOrDefaultAsync(pog => pog.Id == request.ProductOptionGroupId,
                include: "Product.Merchant",
                cancellationToken: cancellationToken);

        if (optionGroup == null)
        {
            return Result.Fail<ProductOptionResponse>("Product option group not found", "NOT_FOUND_OPTION_GROUP");
        }

        if (optionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<ProductOptionResponse>("Access denied", "FORBIDDEN");
        }

        // If this is set as default, unset other defaults in the same group
        if (request.IsDefault)
        {
            var existingDefaults = await _unitOfWork.Repository<ProductOption>()
                .ListAsync(po => po.ProductOptionGroupId == request.ProductOptionGroupId && po.IsDefault,
                    cancellationToken: cancellationToken);

            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.IsDefault = false;
                _unitOfWork.Repository<ProductOption>().Update(existingDefault);
            }
        }

        var option = new ProductOption
        {
            Id = Guid.NewGuid(),
            ProductOptionGroupId = request.ProductOptionGroupId,
            Name = request.Name,
            Description = request.Description,
            ExtraPrice = request.ExtraPrice,
            IsDefault = request.IsDefault,
            IsActive = true,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ProductOption>().AddAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ProductOptionResponse(
            option.Id,
            option.ProductOptionGroupId,
            option.Name,
            option.Description,
            option.ExtraPrice,
            option.IsDefault,
            option.IsActive,
            option.DisplayOrder,
            option.CreatedAt,
            option.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result<ProductOptionResponse>> UpdateProductOptionAsync(
        Guid id,
        UpdateProductOptionRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var option = await _unitOfWork.ReadRepository<ProductOption>()
            .FirstOrDefaultAsync(po => po.Id == id,
                include: "ProductOptionGroup.Product.Merchant",
                cancellationToken: cancellationToken);

        if (option == null)
        {
            return Result.Fail<ProductOptionResponse>("Product option not found", "NOT_FOUND");
        }

        // Verify ownership
        if (option.ProductOptionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<ProductOptionResponse>("Access denied", "FORBIDDEN");
        }

        // If this is set as default, unset other defaults in the same group
        if (request.IsDefault && !option.IsDefault)
        {
            var existingDefaults = await _unitOfWork.Repository<ProductOption>()
                .ListAsync(po => po.ProductOptionGroupId == option.ProductOptionGroupId && po.Id != id && po.IsDefault,
                    cancellationToken: cancellationToken);

            foreach (var existingDefault in existingDefaults)
            {
                existingDefault.IsDefault = false;
                _unitOfWork.Repository<ProductOption>().Update(existingDefault);
            }
        }

        option.Name = request.Name;
        option.Description = request.Description;
        option.ExtraPrice = request.ExtraPrice;
        option.IsDefault = request.IsDefault;
        option.IsActive = request.IsActive;
        option.DisplayOrder = request.DisplayOrder;
        option.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ProductOption>().Update(option);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ProductOptionResponse(
            option.Id,
            option.ProductOptionGroupId,
            option.Name,
            option.Description,
            option.ExtraPrice,
            option.IsDefault,
            option.IsActive,
            option.DisplayOrder,
            option.CreatedAt,
            option.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result> DeleteProductOptionAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var option = await _unitOfWork.ReadRepository<ProductOption>()
            .FirstOrDefaultAsync(po => po.Id == id,
                include: "ProductOptionGroup.Product.Merchant",
                cancellationToken: cancellationToken);

        if (option == null)
        {
            return Result.Fail("Product option not found", "NOT_FOUND");
        }

        // Verify ownership
        if (option.ProductOptionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN");
        }

        _unitOfWork.Repository<ProductOption>().Delete(option);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> BulkCreateProductOptionsAsync(
        BulkCreateProductOptionsRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify option group ownership
        var optionGroup = await _unitOfWork.ReadRepository<ProductOptionGroup>()
            .FirstOrDefaultAsync(pog => pog.Id == request.ProductOptionGroupId,
                include: "Product.Merchant",
                cancellationToken: cancellationToken);

        if (optionGroup == null)
        {
            return Result.Fail("Product option group not found", "NOT_FOUND_OPTION_GROUP");
        }

        if (optionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN");
        }

        var options = request.Options.Select(req => new ProductOption
        {
            Id = Guid.NewGuid(),
            ProductOptionGroupId = request.ProductOptionGroupId,
            Name = req.Name,
            Description = req.Description,
            ExtraPrice = req.ExtraPrice,
            IsDefault = req.IsDefault,
            IsActive = true,
            DisplayOrder = req.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        // Ensure only one default option
        var defaultOptions = options.Where(o => o.IsDefault).ToList();
        if (defaultOptions.Count > 1)
        {
            for (int i = 1; i < defaultOptions.Count; i++)
            {
                defaultOptions[i].IsDefault = false;
            }
        }

        await _unitOfWork.Repository<ProductOption>().AddRangeAsync(options, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> BulkUpdateProductOptionsAsync(
        BulkUpdateProductOptionsRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var optionIds = request.Options.Select(o => o.Id).ToList();
        var options = await _unitOfWork.Repository<ProductOption>()
            .ListAsync(po => optionIds.Contains(po.Id),
                include: "ProductOptionGroup.Product.Merchant",
                cancellationToken: cancellationToken);

        if (options.Count != request.Options.Count)
        {
            return Result.Fail("Some product options not found", "NOT_FOUND");
        }

        // Verify ownership for all options
        var firstOption = options.First();
        if (firstOption.ProductOptionGroup.Product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN");
        }

        foreach (var option in options)
        {
            var requestOption = request.Options.First(o => o.Id == option.Id);
            option.Name = requestOption.Name;
            option.Description = requestOption.Description;
            option.ExtraPrice = requestOption.ExtraPrice;
            option.IsDefault = requestOption.IsDefault;
            option.IsActive = requestOption.IsActive;
            option.DisplayOrder = requestOption.DisplayOrder;
            option.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductOption>().Update(option);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
