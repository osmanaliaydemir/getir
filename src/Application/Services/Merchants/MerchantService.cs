using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Merchants;

public class MerchantService : IMerchantService
{
    private readonly IUnitOfWork _unitOfWork;

    public MerchantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<MerchantResponse>>> GetMerchantsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var merchants = await _unitOfWork.Repository<Merchant>().GetPagedAsync(
            filter: m => m.IsActive,
            orderBy: m => m.CreatedAt,
            ascending: query.IsAscending,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Category",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Merchant>()
            .CountAsync(m => m.IsActive, cancellationToken);

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

    public async Task<Result<MerchantResponse>> GetMerchantByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _unitOfWork.Repository<Merchant>()
            .GetAsync(m => m.Id == id, include: "Category", cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantResponse>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var response = new MerchantResponse(
            merchant.Id,
            merchant.Name,
            merchant.Description,
            merchant.CategoryId,
            merchant.Category.Name,
            merchant.LogoUrl,
            merchant.Address,
            merchant.Latitude,
            merchant.Longitude,
            merchant.MinimumOrderAmount,
            merchant.DeliveryFee,
            merchant.AverageDeliveryTime,
            merchant.Rating,
            merchant.IsActive,
            merchant.IsOpen
        );

        return Result.Ok(response);
    }

    public async Task<Result<MerchantResponse>> CreateMerchantAsync(
        CreateMerchantRequest request,
        CancellationToken cancellationToken = default)
    {
        // Category var mı kontrol et
        var categoryExists = await _unitOfWork.ReadRepository<Category>()
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Fail<MerchantResponse>("Category not found", "NOT_FOUND_CATEGORY");
        }

        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            MinimumOrderAmount = request.MinimumOrderAmount,
            DeliveryFee = request.DeliveryFee,
            AverageDeliveryTime = 30,
            IsActive = true,
            IsOpen = true,
            IsBusy = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Merchant>().AddAsync(merchant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Category bilgisi için yeniden çek
        var createdMerchant = await _unitOfWork.Repository<Merchant>()
            .GetAsync(m => m.Id == merchant.Id, include: "Category", cancellationToken: cancellationToken);

        var response = new MerchantResponse(
            createdMerchant!.Id,
            createdMerchant.Name,
            createdMerchant.Description,
            createdMerchant.CategoryId,
            createdMerchant.Category.Name,
            createdMerchant.LogoUrl,
            createdMerchant.Address,
            createdMerchant.Latitude,
            createdMerchant.Longitude,
            createdMerchant.MinimumOrderAmount,
            createdMerchant.DeliveryFee,
            createdMerchant.AverageDeliveryTime,
            createdMerchant.Rating,
            createdMerchant.IsActive,
            createdMerchant.IsOpen
        );

        return Result.Ok(response);
    }

    public async Task<Result<MerchantResponse>> UpdateMerchantAsync(
        Guid id,
        UpdateMerchantRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _unitOfWork.Repository<Merchant>()
            .GetByIdAsync(id, cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantResponse>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        merchant.Name = request.Name;
        merchant.Description = request.Description;
        merchant.Address = request.Address;
        merchant.PhoneNumber = request.PhoneNumber;
        merchant.Email = request.Email;
        merchant.MinimumOrderAmount = request.MinimumOrderAmount;
        merchant.DeliveryFee = request.DeliveryFee;
        merchant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Merchant>().Update(merchant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Category bilgisi için yeniden çek
        var updatedMerchant = await _unitOfWork.Repository<Merchant>()
            .GetAsync(m => m.Id == id, include: "Category", cancellationToken: cancellationToken);

        var response = new MerchantResponse(
            updatedMerchant!.Id,
            updatedMerchant.Name,
            updatedMerchant.Description,
            updatedMerchant.CategoryId,
            updatedMerchant.Category.Name,
            updatedMerchant.LogoUrl,
            updatedMerchant.Address,
            updatedMerchant.Latitude,
            updatedMerchant.Longitude,
            updatedMerchant.MinimumOrderAmount,
            updatedMerchant.DeliveryFee,
            updatedMerchant.AverageDeliveryTime,
            updatedMerchant.Rating,
            updatedMerchant.IsActive,
            updatedMerchant.IsOpen
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteMerchantAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _unitOfWork.Repository<Merchant>()
            .GetByIdAsync(id, cancellationToken);

        if (merchant == null)
        {
            return Result.Fail("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        // Soft delete
        merchant.IsActive = false;
        merchant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Merchant>().Update(merchant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
