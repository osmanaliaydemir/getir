using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Campaign controller for managing campaigns
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Campaigns")]
public class CampaignController : BaseController
{
    private readonly ICampaignService _campaignService;

    public CampaignController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    /// <summary>
    /// Get active campaigns with pagination
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of active campaigns</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CampaignResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveCampaigns(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _campaignService.GetActiveCampaignsAsync(query, ct);
        return ToActionResult(result);
    }
}
