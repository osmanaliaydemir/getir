using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/search")
            .WithTags("Search");

        group.MapGet("/products", async (
            [AsParameters] SearchProductsQuery query,
            [FromServices] ISearchService searchService,
            CancellationToken ct) =>
        {
            var result = await searchService.SearchProductsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("SearchProducts")
        .Produces<PagedResult<ProductResponse>>(200);

        group.MapGet("/merchants", async (
            [AsParameters] SearchMerchantsQuery query,
            [FromServices] ISearchService searchService,
            CancellationToken ct) =>
        {
            var result = await searchService.SearchMerchantsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("SearchMerchants")
        .Produces<PagedResult<MerchantResponse>>(200);
    }
}
