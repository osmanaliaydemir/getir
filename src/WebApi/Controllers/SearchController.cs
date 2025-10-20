using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Arama işlemleri için arama controller'ı
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Search")]
public class SearchController : BaseController
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Ürün ara
    /// </summary>
    /// <param name="query">Ürün arama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış arama sonuçları</returns>
    [HttpGet("products")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] SearchProductsQuery query,
        CancellationToken ct = default)
    {
        var result = await _searchService.SearchProductsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Mağaza ara
    /// </summary>
    /// <param name="query">Mağaza arama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış arama sonuçları</returns>
    [HttpGet("merchants")]
    [ProducesResponseType(typeof(PagedResult<MerchantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchMerchants(
        [FromQuery] SearchMerchantsQuery query,
        CancellationToken ct = default)
    {
        var result = await _searchService.SearchMerchantsAsync(query, ct);
        return ToActionResult(result);
    }
}
