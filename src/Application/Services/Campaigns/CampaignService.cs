using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Campaigns;

public class CampaignService : BaseService, ICampaignService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    public CampaignService(IUnitOfWork unitOfWork, ILogger<CampaignService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    public async Task<Result<PagedResult<CampaignResponse>>> GetActiveCampaignsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetActiveCampaignsInternalAsync(query, cancellationToken),
            "GetActiveCampaigns",
            new { Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }
    private async Task<Result<PagedResult<CampaignResponse>>> GetActiveCampaignsInternalAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.ActiveCampaigns(query.Page, query.PageSize);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
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

                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.Short), // 5 minutes TTL - campaigns can change frequently
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active campaigns: Page {Page}, PageSize {PageSize}", query.Page, query.PageSize);
            return ServiceResult.HandleException<PagedResult<CampaignResponse>>(ex, _logger, "GetActiveCampaigns");
        }
    }
}
