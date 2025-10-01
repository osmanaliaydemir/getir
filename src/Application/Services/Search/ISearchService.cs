using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Search;

public interface ISearchService
{
    Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(SearchProductsQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<MerchantResponse>>> SearchMerchantsAsync(SearchMerchantsQuery query, CancellationToken cancellationToken = default);
}
