using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Campaigns;

public interface ICampaignService
{
    Task<Result<PagedResult<CampaignResponse>>> GetActiveCampaignsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
}
