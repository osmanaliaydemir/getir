// System namespaces
using Microsoft.Extensions.Logging;

// Application namespaces
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.Common.Exceptions;
using Getir.Application.Common.Extensions;
using Getir.Application.DTO;

// Domain namespaces
using Getir.Domain.Entities;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Merchants;

/// <summary>
/// Service for managing merchants and their operations
/// </summary>
public class MerchantService : BaseService, IMerchantService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    /// <summary>
    /// Initializes a new instance of the MerchantService class
    /// </summary>
    /// <param name="unitOfWork">The unit of work for database operations</param>
    /// <param name="logger">The logger for logging operations</param>
    /// <param name="loggingService">The logging service for structured logging</param>
    /// <param name="cacheService">The cache service for caching operations</param>
    /// <param name="backgroundTaskService">The background task service for async operations</param>
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

    /// <summary>
    /// Gets a paginated list of active merchants
    /// </summary>
    /// <param name="query">The pagination query parameters</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the paginated list of merchants</returns>
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

                    var response = merchants.Select(m => new MerchantResponse
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        IsActive = m.IsActive,
                        IsDeleted = false,
                        Rating = m.Rating,
                        TotalReviews = m.TotalReviews,
                        OwnerId = m.OwnerId,
                        OwnerName = $"{m.Owner.FirstName} {m.Owner.LastName}",
                        ServiceCategoryId = m.ServiceCategoryId,
                        ServiceCategoryName = m.ServiceCategory.Name,
                        LogoUrl = m.LogoUrl,
                        Address = m.Address,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        MinimumOrderAmount = m.MinimumOrderAmount,
                        DeliveryFee = m.DeliveryFee,
                        AverageDeliveryTime = m.AverageDeliveryTime,
                        IsOpen = m.IsOpen
                    }).ToList();

                    var pagedResult = PagedResult<MerchantResponse>.Create(response, total, query.Page, query.PageSize);
                    
                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(15), // 15 dakika cache
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetMerchants", new { Page = query.Page, PageSize = query.PageSize });
            return ServiceResult.HandleException<PagedResult<MerchantResponse>>(ex, _logger, "GetMerchants");
        }
    }

    /// <summary>
    /// Gets a merchant by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the merchant</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the merchant details or an error if not found</returns>
    /// <exception cref="EntityNotFoundException">Thrown when the merchant is not found</exception>
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
                        throw new EntityNotFoundException("Merchant", id);
                    }

                    var response = new MerchantResponse
                    {
                        Id = merchant.Id,
                        Name = merchant.Name,
                        Description = merchant.Description,
                        CreatedAt = merchant.CreatedAt,
                        UpdatedAt = merchant.UpdatedAt,
                        IsActive = merchant.IsActive,
                        IsDeleted = false,
                        Rating = merchant.Rating,
                        TotalReviews = merchant.TotalReviews,
                        OwnerId = merchant.OwnerId,
                        OwnerName = string.Concat(merchant.Owner.FirstName, " ", merchant.Owner.LastName),
                        ServiceCategoryId = merchant.ServiceCategoryId,
                        ServiceCategoryName = merchant.ServiceCategory.Name,
                        LogoUrl = merchant.LogoUrl,
                        Address = merchant.Address,
                        Latitude = merchant.Latitude,
                        Longitude = merchant.Longitude,
                        MinimumOrderAmount = merchant.MinimumOrderAmount,
                        DeliveryFee = merchant.DeliveryFee,
                        AverageDeliveryTime = merchant.AverageDeliveryTime,
                        IsOpen = merchant.IsOpen
                    };

                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(ApplicationConstants.DefaultCacheMinutes), // Default cache duration
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetMerchantById", new { MerchantId = id });
            return ServiceResult.HandleException<MerchantResponse>(ex, _logger, "GetMerchantById");
        }
    }

    /// <summary>
    /// Creates a new merchant for the specified owner
    /// </summary>
    /// <param name="request">The merchant creation request</param>
    /// <param name="ownerId">The unique identifier of the owner</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result containing the created merchant details or an error if creation fails</returns>
    /// <exception cref="EntityNotFoundException">Thrown when the owner or service category is not found</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when business rules are violated</exception>
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
                throw new EntityNotFoundException("User", ownerId);
            }

            // ServiceCategory var mı kontrol et
            var categoryExists = await _unitOfWork.ReadRepository<ServiceCategory>()
                .AnyAsync(c => c.Id == request.ServiceCategoryId, cancellationToken);

            if (!categoryExists)
            {
                throw new EntityNotFoundException("ServiceCategory", request.ServiceCategoryId);
            }

            // Business rule: Minimum order amount validation
            if (request.MinimumOrderAmount < ApplicationConstants.MinOrderAmount)
            {
                throw new BusinessRuleViolationException(
                    "MinimumOrderAmount", 
                    $"Minimum order amount must be at least {ApplicationConstants.MinOrderAmount:C}");
            }

            // Business rule: Delivery fee validation
            if (request.DeliveryFee < 0)
            {
                throw new BusinessRuleViolationException(
                    "DeliveryFee", 
                    "Delivery fee cannot be negative");
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
                AverageDeliveryTime = ApplicationConstants.DefaultDeliveryTimeMinutes,
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

            var response = new MerchantResponse
            {
                Id = createdMerchant!.Id,
                Name = createdMerchant.Name,
                Description = createdMerchant.Description,
                CreatedAt = createdMerchant.CreatedAt,
                UpdatedAt = createdMerchant.UpdatedAt,
                IsActive = createdMerchant.IsActive,
                IsDeleted = false,
                Rating = createdMerchant.Rating,
                TotalReviews = createdMerchant.TotalReviews,
                OwnerId = createdMerchant.OwnerId,
                OwnerName = $"{createdMerchant.Owner.FirstName} {createdMerchant.Owner.LastName}",
                ServiceCategoryId = createdMerchant.ServiceCategoryId,
                ServiceCategoryName = createdMerchant.ServiceCategory.Name,
                LogoUrl = createdMerchant.LogoUrl,
                Address = createdMerchant.Address,
                Latitude = createdMerchant.Latitude,
                Longitude = createdMerchant.Longitude,
                MinimumOrderAmount = createdMerchant.MinimumOrderAmount,
                DeliveryFee = createdMerchant.DeliveryFee,
                AverageDeliveryTime = createdMerchant.AverageDeliveryTime,
                IsOpen = createdMerchant.IsOpen
            };

            return ServiceResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "CreateMerchant", new { OwnerId = ownerId, Name = request.Name });
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

        var response = new MerchantResponse
        {
            Id = updatedMerchant!.Id,
            Name = updatedMerchant.Name,
            Description = updatedMerchant.Description,
            CreatedAt = updatedMerchant.CreatedAt,
            UpdatedAt = updatedMerchant.UpdatedAt,
            IsActive = updatedMerchant.IsActive,
            IsDeleted = false,
            Rating = updatedMerchant.Rating,
            TotalReviews = updatedMerchant.TotalReviews,
            OwnerId = updatedMerchant.OwnerId,
            OwnerName = $"{updatedMerchant.Owner.FirstName} {updatedMerchant.Owner.LastName}",
            ServiceCategoryId = updatedMerchant.ServiceCategoryId,
            ServiceCategoryName = updatedMerchant.ServiceCategory.Name,
            LogoUrl = updatedMerchant.LogoUrl,
            Address = updatedMerchant.Address,
            Latitude = updatedMerchant.Latitude,
            Longitude = updatedMerchant.Longitude,
            MinimumOrderAmount = updatedMerchant.MinimumOrderAmount,
            DeliveryFee = updatedMerchant.DeliveryFee,
            AverageDeliveryTime = updatedMerchant.AverageDeliveryTime,
            IsOpen = updatedMerchant.IsOpen
        };

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

    public async Task<Result<PagedResult<MerchantResponse>>> GetMerchantsByCategoryTypeAsync(
        ServiceCategoryType categoryType,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetMerchantsByCategoryTypeInternalAsync(categoryType, query, cancellationToken),
            "GetMerchantsByCategoryType",
            new { CategoryType = categoryType, Page = query.Page, PageSize = query.PageSize },
            cancellationToken);
    }

    private async Task<Result<PagedResult<MerchantResponse>>> GetMerchantsByCategoryTypeInternalAsync(
        ServiceCategoryType categoryType,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"merchants_by_type_{categoryType}_{query.Page}_{query.PageSize}";
            
            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var merchants = await _unitOfWork.Repository<Merchant>().GetPagedAsync(
                        filter: m => m.IsActive && m.ServiceCategory.Type == categoryType,
                        orderBy: m => m.CreatedAt,
                        ascending: query.IsAscending,
                        page: query.Page,
                        pageSize: query.PageSize,
                        include: "ServiceCategory,Owner",
                        cancellationToken: cancellationToken);

                    var total = await _unitOfWork.ReadRepository<Merchant>()
                        .CountAsync(m => m.IsActive && m.ServiceCategory.Type == categoryType, cancellationToken);

                    var response = merchants.Select(m => new MerchantResponse
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        IsActive = m.IsActive,
                        IsDeleted = false,
                        Rating = m.Rating,
                        TotalReviews = m.TotalReviews,
                        OwnerId = m.OwnerId,
                        OwnerName = $"{m.Owner.FirstName} {m.Owner.LastName}",
                        ServiceCategoryId = m.ServiceCategoryId,
                        ServiceCategoryName = m.ServiceCategory.Name,
                        LogoUrl = m.LogoUrl,
                        Address = m.Address,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        MinimumOrderAmount = m.MinimumOrderAmount,
                        DeliveryFee = m.DeliveryFee,
                        AverageDeliveryTime = m.AverageDeliveryTime,
                        IsOpen = m.IsOpen
                    }).ToList();

                    var pagedResult = PagedResult<MerchantResponse>.Create(response, total, query.Page, query.PageSize);
                    
                    return ServiceResult.Success(pagedResult);
                },
                TimeSpan.FromMinutes(10), // 10 dakika cache
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetMerchantsByCategoryType", new { CategoryType = categoryType, Page = query.Page, PageSize = query.PageSize });
            return ServiceResult.HandleException<PagedResult<MerchantResponse>>(ex, _logger, "GetMerchantsByCategoryType");
        }
    }

    public async Task<Result<IEnumerable<MerchantResponse>>> GetActiveMerchantsByCategoryTypeAsync(
        ServiceCategoryType categoryType,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetActiveMerchantsByCategoryTypeInternalAsync(categoryType, cancellationToken),
            "GetActiveMerchantsByCategoryType",
            new { CategoryType = categoryType },
            cancellationToken);
    }

    private async Task<Result<IEnumerable<MerchantResponse>>> GetActiveMerchantsByCategoryTypeInternalAsync(
        ServiceCategoryType categoryType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"active_merchants_by_type_{categoryType}";
            
            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var merchants = await _unitOfWork.Repository<Merchant>().GetPagedAsync(
                        filter: m => m.IsActive && m.IsOpen && m.ServiceCategory.Type == categoryType,
                        orderBy: m => m.Rating,
                        ascending: false,
                        page: 1,
                        pageSize: 1000, // Büyük sayı ile tüm kayıtları al
                        include: "ServiceCategory,Owner",
                        cancellationToken: cancellationToken);

                    var response = merchants.Select(m => new MerchantResponse
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt,
                        IsActive = m.IsActive,
                        IsDeleted = false,
                        Rating = m.Rating,
                        TotalReviews = m.TotalReviews,
                        OwnerId = m.OwnerId,
                        OwnerName = $"{m.Owner.FirstName} {m.Owner.LastName}",
                        ServiceCategoryId = m.ServiceCategoryId,
                        ServiceCategoryName = m.ServiceCategory.Name,
                        LogoUrl = m.LogoUrl,
                        Address = m.Address,
                        Latitude = m.Latitude,
                        Longitude = m.Longitude,
                        MinimumOrderAmount = m.MinimumOrderAmount,
                        DeliveryFee = m.DeliveryFee,
                        AverageDeliveryTime = m.AverageDeliveryTime,
                        IsOpen = m.IsOpen
                    }).ToList();

                    return ServiceResult.Success((IEnumerable<MerchantResponse>)response);
                },
                TimeSpan.FromMinutes(5), // 5 dakika cache
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex, "GetActiveMerchantsByCategoryType", new { CategoryType = categoryType });
            return ServiceResult.HandleException<IEnumerable<MerchantResponse>>(ex, _logger, "GetActiveMerchantsByCategoryType");
        }
    }
}

// Background task data classes
public class MerchantCreatedTask
{
    public Guid MerchantId { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
}
