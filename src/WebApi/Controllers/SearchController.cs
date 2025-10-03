using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Search controller for search operations
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
    /// Search products
    /// </summary>
    /// <param name="query">Search products query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged search results</returns>
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
    /// Search merchants
    /// </summary>
    /// <param name="query">Search merchants query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged search results</returns>
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
