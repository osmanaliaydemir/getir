using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Merchants;

public class MerchantService : BaseService, IMerchantService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public MerchantService(
        IUnitOfWork unitOfWork,
        ILogger<MerchantService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IBackgroundTaskService backgroundTaskService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

    public async Task<Result<PagedResult<MerchantResponse>>> GetMerchantsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantsInternalAsync(query, cancellationToken),
            "GetMerchants",
            new { Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }

    private async Task<Result<PagedResult<MerchantResponse>>> GetMerchantsInternalAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Cache key oluştur
            var cacheKey = $"merchants_{query.Page}_{query.PageSize}_{query.IsAscending}";
            
            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var merchants = await _unitOfWork.Repository<Merchant>().GetPagedAsync(
                        filter: m => m.IsActive,
                        orderBy: m => m.CreatedAt,
                        ascending: query.IsAscending,
                        page: query.Page,
                        pageSize: query.PageSize,
                        include: "ServiceCategory,Owner",
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<Merchant>()
                        .CountAsync(m => m.IsActive, cancellationToken);

                    var response = merchants.Select(m => new MerchantResponse(
                        m.Id,
                        m.OwnerId,
                        $"{m.Owner.FirstName} {m.Owner.LastName}",
                        m.Name,
                        m.Description,
                        m.ServiceCategoryId,
                        m.ServiceCategory.Name,
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
                    
                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(15), // 15 dakika cache
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting merchants", ex, new { Page = query.Page, PageSize = query.PageSize });
            return ServiceResult.HandleException<PagedResult<MerchantResponse>>(ex, _logger, "GetMerchants");
        }
    }

    public async Task<Result<MerchantResponse>> GetMerchantByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantByIdInternalAsync(id, cancellationToken),
            "GetMerchantById",
            new { MerchantId = id },
            cancellationToken);
    }

    private async Task<Result<MerchantResponse>> GetMerchantByIdInternalAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"merchant_{id}";
            
            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var merchant = await _unitOfWork.Repository<Merchant>()
                        .GetAsync(m => m.Id == id, include: "ServiceCategory,Owner", cancellationToken: cancellationToken);

                    if (merchant == null)
                    {
                        return ServiceResult.Failure<MerchantResponse>("Merchant not found", ErrorCodes.MERCHANT_NOT_FOUND);
                    }

                    var response = new MerchantResponse(
                        merchant.Id,
                        merchant.OwnerId,
                        $"{merchant.Owner.FirstName} {merchant.Owner.LastName}",
                        merchant.Name,
                        merchant.Description,
                        merchant.ServiceCategoryId,
                        merchant.ServiceCategory.Name,
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

                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(30), // 30 dakika cache
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting merchant by id", ex, new { MerchantId = id });
            return ServiceResult.HandleException<MerchantResponse>(ex, _logger, "GetMerchantById");
        }
    }

    public async Task<Result<MerchantResponse>> CreateMerchantAsync(
        CreateMerchantRequest request,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CreateMerchantInternalAsync(request, ownerId, cancellationToken),
            "CreateMerchant",
            new { OwnerId = ownerId, Name = request.Name },
            cancellationToken);
    }

    private async Task<Result<MerchantResponse>> CreateMerchantInternalAsync(
        CreateMerchantRequest request,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Owner var mı kontrol et
            var ownerExists = await _unitOfWork.ReadRepository<User>()
                .AnyAsync(u => u.Id == ownerId && u.IsActive, cancellationToken);

            if (!ownerExists)
            {
                return ServiceResult.Failure<MerchantResponse>("Owner user not found or inactive", ErrorCodes.NOT_FOUND);
            }

            // ServiceCategory var mı kontrol et
            var categoryExists = await _unitOfWork.ReadRepository<ServiceCategory>()
                .AnyAsync(c => c.Id == request.ServiceCategoryId, cancellationToken);

            if (!categoryExists)
            {
                return ServiceResult.Failure<MerchantResponse>("Category not found", ErrorCodes.NOT_FOUND);
            }

            var merchant = new Merchant
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                Name = request.Name,
                Description = request.Description,
                ServiceCategoryId = request.ServiceCategoryId,
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

            // Cache'i temizle
            await InvalidateCacheAsync("merchants_*", cancellationToken);

            // Background task - Merchant oluşturuldu bildirimi
            await _backgroundTaskService.EnqueueTaskAsync(new MerchantCreatedTask
            {
                MerchantId = merchant.Id,
                OwnerId = ownerId,
                Name = merchant.Name
            }, TaskPriority.Normal, cancellationToken);

            // Log merchant creation
            _loggingService.LogUserAction(ownerId.ToString(), "MerchantCreated", new { MerchantId = merchant.Id, Name = merchant.Name });

            // ServiceCategory ve Owner bilgisi için yeniden çek
            var createdMerchant = await _unitOfWork.Repository<Merchant>()
                .GetAsync(m => m.Id == merchant.Id, include: "ServiceCategory,Owner", cancellationToken: cancellationToken);

            var response = new MerchantResponse(
                createdMerchant!.Id,
                createdMerchant.OwnerId,
                $"{createdMerchant.Owner.FirstName} {createdMerchant.Owner.LastName}",
                createdMerchant.Name,
                createdMerchant.Description,
                createdMerchant.ServiceCategoryId,
                createdMerchant.ServiceCategory.Name,
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

            return ServiceResult.Success(response);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error creating merchant", ex, new { OwnerId = ownerId, Name = request.Name });
            return ServiceResult.HandleException<MerchantResponse>(ex, _logger, "CreateMerchant");
        }
    }

    public async Task<Result<MerchantResponse>> UpdateMerchantAsync(
        Guid id,
        UpdateMerchantRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _unitOfWork.Repository<Merchant>()
            .GetAsync(m => m.Id == id, include: "Owner", cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantResponse>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        // Sadece merchant sahibi güncelleyebilir (Admin kontrolü endpoint'te yapılacak)
        if (merchant.OwnerId != currentUserId)
        {
            return Result.Fail<MerchantResponse>("You are not authorized to update this merchant", "FORBIDDEN_NOT_OWNER");
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

        // ServiceCategory ve Owner bilgisi için yeniden çek
        var updatedMerchant = await _unitOfWork.Repository<Merchant>()
            .GetAsync(m => m.Id == id, include: "ServiceCategory,Owner", cancellationToken: cancellationToken);

        var response = new MerchantResponse(
            updatedMerchant!.Id,
            updatedMerchant.OwnerId,
            $"{updatedMerchant.Owner.FirstName} {updatedMerchant.Owner.LastName}",
            updatedMerchant.Name,
            updatedMerchant.Description,
            updatedMerchant.ServiceCategoryId,
            updatedMerchant.ServiceCategory.Name,
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

// Background task data classes
public class MerchantCreatedTask
{
    public Guid MerchantId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
}
