using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Campaigns;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Kampanyaları yönetmek için kampanya controller'ı
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
    /// Sayfalama ile aktif kampanyaları getir
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış aktif kampanyalar listesi</returns>
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
