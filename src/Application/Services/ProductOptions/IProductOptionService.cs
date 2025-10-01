using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductOptions;

public interface IProductOptionService
{
    Task<Result<PagedResult<ProductOptionResponse>>> GetProductOptionsAsync(
        Guid productOptionGroupId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<ProductOptionResponse>> GetProductOptionAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<ProductOptionResponse>> CreateProductOptionAsync(
        CreateProductOptionRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result<ProductOptionResponse>> UpdateProductOptionAsync(
        Guid id,
        UpdateProductOptionRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteProductOptionAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result> BulkCreateProductOptionsAsync(
        BulkCreateProductOptionsRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result> BulkUpdateProductOptionsAsync(
        BulkUpdateProductOptionsRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);
}
