using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Endpoints;

public static class CampaignEndpoints
{
    public static void MapCampaignEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/campaigns")
            .WithTags("Campaigns");

        group.MapGet("/", async (
            [AsParameters] PaginationQuery query,
            [FromServices] ICampaignService campaignService,
            CancellationToken ct) =>
        {
            var result = await campaignService.GetActiveCampaignsAsync(query, ct);
            return result.ToIResult();
        })
        .WithName("GetActiveCampaigns")
        .Produces<PagedResult<CampaignResponse>>(200);
    }
}
