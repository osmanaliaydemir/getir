using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Search;

public class SearchService : ISearchService
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ProductResponse>>> SearchProductsAsync(
        SearchProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Repository<Product>().GetPagedAsync(
            filter: p => p.IsActive && 
                        (string.IsNullOrEmpty(query.Query) || p.Name.Contains(query.Query)) &&
                        (!query.MerchantId.HasValue || p.MerchantId == query.MerchantId) &&
                        (!query.CategoryId.HasValue || p.CategoryId == query.CategoryId) &&
                        (!query.MinPrice.HasValue || p.Price >= query.MinPrice) &&
                        (!query.MaxPrice.HasValue || p.Price <= query.MaxPrice),
            orderBy: p => p.Name,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Merchant",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.IsActive && 
                           (string.IsNullOrEmpty(query.Query) || p.Name.Contains(query.Query)), 
                        cancellationToken);

        var response = products.Select(p => new ProductResponse(
            p.Id,
            p.MerchantId,
            p.Merchant.Name,
            p.Name,
            p.Description,
            p.ImageUrl,
            p.Price,
            p.DiscountedPrice,
            p.StockQuantity,
            p.Unit,
            p.IsAvailable
        )).ToList();

        var pagedResult = PagedResult<ProductResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }

    public async Task<Result<PagedResult<MerchantResponse>>> SearchMerchantsAsync(
        SearchMerchantsQuery query,
        CancellationToken cancellationToken = default)
    {
        var merchants = await _unitOfWork.Repository<Merchant>().GetPagedAsync(
            filter: m => m.IsActive && 
                        (string.IsNullOrEmpty(query.Query) || m.Name.Contains(query.Query)) &&
                        (!query.CategoryId.HasValue || m.CategoryId == query.CategoryId),
            orderBy: m => m.Rating,
            ascending: false,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Category",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Merchant>()
            .CountAsync(m => m.IsActive && 
                           (string.IsNullOrEmpty(query.Query) || m.Name.Contains(query.Query)), 
                        cancellationToken);

        var response = merchants.Select(m => new MerchantResponse(
            m.Id,
            m.Name,
            m.Description,
            m.CategoryId,
            m.Category.Name,
            m.LogoUrl,
            m.Address,
            m.Latitude,
            m.Longitude,
            m.MinimumOrderAmount,
            m.DeliveryFee,
            m.AverageDeliveryTime,
            m.Rating,
            m.IsActive,
            m.IsOpen
        )).ToList();

        var pagedResult = PagedResult<MerchantResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }
}
