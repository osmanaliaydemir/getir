using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Campaigns;

public class CampaignService : ICampaignService
{
    private readonly IUnitOfWork _unitOfWork;

    public CampaignService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<CampaignResponse>>> GetActiveCampaignsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var campaigns = await _unitOfWork.Repository<Campaign>().GetPagedAsync(
            filter: c => c.IsActive && c.StartDate <= now && c.EndDate >= now,
            orderBy: c => c.DisplayOrder,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Merchant",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Campaign>()
            .CountAsync(c => c.IsActive && c.StartDate <= now && c.EndDate >= now, cancellationToken);

        var response = campaigns.Select(c => new CampaignResponse(
            c.Id,
            c.Title,
            c.Description,
            c.ImageUrl,
            c.MerchantId,
            c.Merchant?.Name,
            c.DiscountType,
            c.DiscountValue,
            c.StartDate,
            c.EndDate,
            c.IsActive,
            c.DisplayOrder
        )).ToList();

        var pagedResult = PagedResult<CampaignResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }
}
