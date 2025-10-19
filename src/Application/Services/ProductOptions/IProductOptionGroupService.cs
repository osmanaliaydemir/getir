using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductOptions;

public interface IProductOptionGroupService
{
    Task<Result<PagedResult<ProductOptionGroupResponse>>> GetProductOptionGroupsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<ProductOptionGroupResponse>> GetProductOptionGroupAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductOptionGroupResponse>> CreateProductOptionGroupAsync(CreateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result<ProductOptionGroupResponse>> UpdateProductOptionGroupAsync(Guid id, UpdateProductOptionGroupRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductOptionGroupAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    Task<Result> ReorderProductOptionGroupsAsync(Guid productId, List<Guid> orderedGroupIds, Guid merchantOwnerId, CancellationToken cancellationToken = default);
}
