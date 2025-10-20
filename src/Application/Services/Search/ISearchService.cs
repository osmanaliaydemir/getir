using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Search;

/// <summary>
/// Arama servisi: ürün/merchant arama (filtreleme, sıralama, sayfalama).
/// </summary>
public interface ISearchService
{
    /// <summary>Ürünleri arar (isim/açıklama, kategori/fiyat filtresi).</summary>
    Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(SearchProductsQuery query, CancellationToken cancellationToken = default);
    /// <summary>Merchantları arar (isim/kategori, konum bazlı).</summary>
    Task<Result<PagedResult<MerchantResponse>>> SearchMerchantsAsync(SearchMerchantsQuery query, CancellationToken cancellationToken = default);
}
