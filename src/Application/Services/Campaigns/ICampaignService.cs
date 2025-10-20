using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Campaigns;

/// <summary>
/// Kampanya servisi interface'i: aktif kampanyaları getirme işlemleri.
/// </summary>
public interface ICampaignService
{
    /// <summary>
    /// Aktif kampanyaları sayfalama ile getirir.
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Sayfalanmış kampanyalar</returns>
    Task<Result<PagedResult<CampaignResponse>>> GetActiveCampaignsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
}
