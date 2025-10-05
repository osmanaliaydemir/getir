using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.ProductOptions;

public interface IMarketProductVariantService
{
    Task<Result<PagedResult<MarketProductVariantResponse>>> GetProductVariantsAsync(
        Guid productId,
        PaginationQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<MarketProductVariantResponse>> GetProductVariantAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<MarketProductVariantResponse>> CreateProductVariantAsync(
        CreateMarketProductVariantRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result<MarketProductVariantResponse>> UpdateProductVariantAsync(
        Guid id,
        UpdateMarketProductVariantRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteProductVariantAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateVariantStockAsync(
        Guid id,
        int newStockQuantity,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);

    Task<Result> BulkUpdateVariantStockAsync(
        List<UpdateVariantStockRequest> requests,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default);
}

public record UpdateVariantStockRequest(
    Guid VariantId,
    int NewStockQuantity);
