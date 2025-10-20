using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.DeliveryZones;

/// <summary>
/// Teslimat bölgesi servisi: merchant bazlı teslimat alanlarının yönetimi, cache ve nokta-poligon kontrolü.
/// </summary>
public class DeliveryZoneService : BaseService, IDeliveryZoneService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public DeliveryZoneService(IUnitOfWork unitOfWork, ILogger<DeliveryZoneService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

    /// <summary>
    /// Merchant'a ait aktif teslimat bölgelerini cache'den getirir.
    /// </summary>
    public async Task<Result<List<DeliveryZoneResponse>>> GetDeliveryZonesByMerchantAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetDeliveryZonesByMerchantInternalAsync(merchantId, cancellationToken),
            "GetDeliveryZonesByMerchant",
            new { MerchantId = merchantId },
            cancellationToken);
    }

    private async Task<Result<List<DeliveryZoneResponse>>> GetDeliveryZonesByMerchantInternalAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.DeliveryZonesByMerchant(merchantId);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var deliveryZones = await _unitOfWork.ReadRepository<DeliveryZone>()
                        .ListAsync(
                            filter: dz => dz.MerchantId == merchantId && dz.IsActive,
                            include: "Points",
                            cancellationToken: cancellationToken);

                    var response = deliveryZones.Select(dz => new DeliveryZoneResponse(
                        dz.Id,
                        dz.MerchantId,
                        dz.Name,
                        dz.Description,
                        dz.DeliveryFee,
                        dz.EstimatedDeliveryTime,
                        dz.IsActive,
                        dz.Points.OrderBy(p => p.Order).Select(p => new DeliveryZonePointResponse(
                            p.Id,
                            p.DeliveryZoneId,
                            p.Latitude,
                            p.Longitude,
                            p.Order
                        )).ToList(),
                        dz.CreatedAt
                    )).ToList();

                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL - rarely changes
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery zones by merchant: {MerchantId}", merchantId);
            return ServiceResult.HandleException<List<DeliveryZoneResponse>>(ex, _logger, "GetDeliveryZonesByMerchant");
        }
    }

    /// <summary>
    /// Teslimat bölgesini ID ile getirir (cache).
    /// </summary>
    public async Task<Result<DeliveryZoneResponse>> GetDeliveryZoneByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetDeliveryZoneByIdInternalAsync(id, cancellationToken),
            "GetDeliveryZoneById",
            new { ZoneId = id },
            cancellationToken);
    }

    private async Task<Result<DeliveryZoneResponse>> GetDeliveryZoneByIdInternalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.DeliveryZone(id);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var deliveryZone = await _unitOfWork.Repository<DeliveryZone>()
                        .GetAsync(dz => dz.Id == id, include: "Points,Merchant", cancellationToken: cancellationToken);

                    if (deliveryZone == null)
                    {
                        return Result.Fail<DeliveryZoneResponse>("Delivery zone not found", "NOT_FOUND_DELIVERY_ZONE");
                    }

                    var response = new DeliveryZoneResponse(
                        deliveryZone.Id,
                        deliveryZone.MerchantId,
                        deliveryZone.Name,
                        deliveryZone.Description,
                        deliveryZone.DeliveryFee,
                        deliveryZone.EstimatedDeliveryTime,
                        deliveryZone.IsActive,
                        deliveryZone.Points.OrderBy(p => p.Order).Select(p => new DeliveryZonePointResponse(
                            p.Id,
                            p.DeliveryZoneId,
                            p.Latitude,
                            p.Longitude,
                            p.Order
                        )).ToList(),
                        deliveryZone.CreatedAt
                    );

                    return Result.Ok(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery zone by id: {ZoneId}", id);
            return ServiceResult.HandleException<DeliveryZoneResponse>(ex, _logger, "GetDeliveryZoneById");
        }
    }

    /// <summary>
    /// Yeni teslimat bölgesi oluşturur (min 3 nokta gerekli, cache invalidation yapar).
    /// </summary>
    public async Task<Result<DeliveryZoneResponse>> CreateDeliveryZoneAsync(CreateDeliveryZoneRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<DeliveryZoneResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        // Minimum 3 nokta gerekli (triangle)
        if (request.Points.Count < 3)
        {
            return Result.Fail<DeliveryZoneResponse>("At least 3 points are required for a delivery zone", "INVALID_DELIVERY_ZONE");
        }

        var deliveryZoneId = Guid.NewGuid();
        var deliveryZone = new DeliveryZone
        {
            Id = deliveryZoneId,
            MerchantId = request.MerchantId,
            Name = request.Name,
            Description = request.Description,
            DeliveryFee = request.DeliveryFee,
            EstimatedDeliveryTime = request.EstimatedDeliveryTime,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<DeliveryZone>().AddAsync(deliveryZone, cancellationToken);

        // Points ekle
        foreach (var pointRequest in request.Points)
        {
            var point = new DeliveryZonePoint
            {
                Id = Guid.NewGuid(),
                DeliveryZoneId = deliveryZoneId,
                Latitude = pointRequest.Latitude,
                Longitude = pointRequest.Longitude,
                Order = pointRequest.Order,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<DeliveryZonePoint>().AddAsync(point, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate delivery zones for this merchant
        await _cacheService.RemoveAsync(CacheKeys.DeliveryZonesByMerchant(request.MerchantId), cancellationToken);
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllDeliveryZonesPattern(), cancellationToken);

        var response = new DeliveryZoneResponse(
            deliveryZone.Id,
            deliveryZone.MerchantId,
            deliveryZone.Name,
            deliveryZone.Description,
            deliveryZone.DeliveryFee,
            deliveryZone.EstimatedDeliveryTime,
            deliveryZone.IsActive,
            request.Points.Select(p => new DeliveryZonePointResponse(
                Guid.NewGuid(), // Bu gerçek ID olacak, şimdilik placeholder
                deliveryZoneId,
                p.Latitude,
                p.Longitude,
                p.Order
            )).ToList(),
            deliveryZone.CreatedAt
        );

        return Result.Ok(response);
    }

    /// <summary>
    /// Teslimat bölgesini günceller (noktalar dahil, cache invalidation yapar).
    /// </summary>
    public async Task<Result<DeliveryZoneResponse>> UpdateDeliveryZoneAsync(Guid id, UpdateDeliveryZoneRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        var deliveryZone = await _unitOfWork.Repository<DeliveryZone>()
            .GetAsync(
                dz => dz.Id == id,
                include: "Points,Merchant",
                cancellationToken: cancellationToken);

        if (deliveryZone == null)
        {
            return Result.Fail<DeliveryZoneResponse>("Delivery zone not found", "NOT_FOUND_DELIVERY_ZONE");
        }

        // Merchant ownership kontrolü
        if (deliveryZone.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<DeliveryZoneResponse>("Access denied", "FORBIDDEN_DELIVERY_ZONE");
        }

        // Minimum 3 nokta gerekli
        if (request.Points.Count < 3)
        {
            return Result.Fail<DeliveryZoneResponse>("At least 3 points are required for a delivery zone", "INVALID_DELIVERY_ZONE");
        }

        // DeliveryZone güncelle
        deliveryZone.Name = request.Name;
        deliveryZone.Description = request.Description;
        deliveryZone.DeliveryFee = request.DeliveryFee;
        deliveryZone.EstimatedDeliveryTime = request.EstimatedDeliveryTime;
        deliveryZone.IsActive = request.IsActive;
        deliveryZone.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<DeliveryZone>().Update(deliveryZone);

        // Eski points'leri sil
        foreach (var point in deliveryZone.Points)
        {
            _unitOfWork.Repository<DeliveryZonePoint>().Delete(point);
        }

        // Yeni points'leri ekle
        foreach (var pointRequest in request.Points)
        {
            var point = new DeliveryZonePoint
            {
                Id = Guid.NewGuid(),
                DeliveryZoneId = id,
                Latitude = pointRequest.Latitude,
                Longitude = pointRequest.Longitude,
                Order = pointRequest.Order,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<DeliveryZonePoint>().AddAsync(point, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single zone cache
        await _cacheService.RemoveAsync(CacheKeys.DeliveryZone(id), cancellationToken);

        // Invalidate merchant's delivery zones
        await _cacheService.RemoveAsync(CacheKeys.DeliveryZonesByMerchant(deliveryZone.MerchantId), cancellationToken);

        // Invalidate all zones pattern
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllDeliveryZonesPattern(), cancellationToken);

        var response = new DeliveryZoneResponse(
            deliveryZone.Id,
            deliveryZone.MerchantId,
            deliveryZone.Name,
            deliveryZone.Description,
            deliveryZone.DeliveryFee,
            deliveryZone.EstimatedDeliveryTime,
            deliveryZone.IsActive,
            request.Points.Select(p => new DeliveryZonePointResponse(
                Guid.NewGuid(), // Bu gerçek ID olacak
                id,
                p.Latitude,
                p.Longitude,
                p.Order
            )).ToList(),
            deliveryZone.CreatedAt
        );

        return Result.Ok(response);
    }

    /// <summary>
    /// Teslimat bölgesini siler (noktalar dahil, cache invalidation yapar).
    /// </summary>
    public async Task<Result> DeleteDeliveryZoneAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        var deliveryZone = await _unitOfWork.Repository<DeliveryZone>()
            .GetAsync(
                dz => dz.Id == id,
                include: "Points,Merchant",
                cancellationToken: cancellationToken);

        if (deliveryZone == null)
        {
            return Result.Fail("Delivery zone not found", "NOT_FOUND_DELIVERY_ZONE");
        }

        // Merchant ownership kontrolü
        if (deliveryZone.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_DELIVERY_ZONE");
        }

        // Points'leri sil
        foreach (var point in deliveryZone.Points)
        {
            _unitOfWork.Repository<DeliveryZonePoint>().Delete(point);
        }

        // DeliveryZone'u sil
        _unitOfWork.Repository<DeliveryZone>().Delete(deliveryZone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ============= CACHE INVALIDATION =============
        // Invalidate single zone cache
        await _cacheService.RemoveAsync(CacheKeys.DeliveryZone(id), cancellationToken);

        // Invalidate merchant's delivery zones
        await _cacheService.RemoveAsync(CacheKeys.DeliveryZonesByMerchant(deliveryZone.MerchantId), cancellationToken);

        // Invalidate all zones pattern
        await _cacheService.RemoveByPatternAsync(CacheKeys.AllDeliveryZonesPattern(), cancellationToken);

        return Result.Ok();
    }

    /// <summary>
    /// Verilen koordinatın merchant'ın teslimat bölgelerinden herhangi birinde olup olmadığını kontrol eder (ray casting algoritması).
    /// </summary>
    public async Task<Result<CheckDeliveryZoneResponse>> CheckDeliveryZoneAsync(Guid merchantId, CheckDeliveryZoneRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CheckDeliveryZoneInternalAsync(merchantId, request, cancellationToken),
            "CheckDeliveryZone",
            new { MerchantId = merchantId, Lat = request.Latitude, Lon = request.Longitude },
            cancellationToken);
    }

    private async Task<Result<CheckDeliveryZoneResponse>> CheckDeliveryZoneInternalAsync(Guid merchantId, CheckDeliveryZoneRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cache delivery zones for this merchant (geo-spatial queries are expensive!)
            var cacheKey = CacheKeys.DeliveryZonesByMerchant(merchantId);

            var zonesResult = await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var zones = await _unitOfWork.ReadRepository<DeliveryZone>()
                        .ListAsync(
                            filter: dz => dz.MerchantId == merchantId && dz.IsActive,
                            include: "Points",
                            cancellationToken: cancellationToken);

                    return ServiceResult.Success(zones);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL
                cancellationToken);

            if (!zonesResult.Success || zonesResult.Data == null)
            {
                return Result.Ok(new CheckDeliveryZoneResponse(false, null, null, null));
            }

            var deliveryZones = zonesResult.Data;

            foreach (var zone in deliveryZones)
            {
                if (IsPointInPolygon(request.Latitude, request.Longitude, zone.Points.OrderBy(p => p.Order).ToList()))
                {
                    return Result.Ok(new CheckDeliveryZoneResponse(
                        true,
                        zone.Id,
                        zone.DeliveryFee,
                        zone.EstimatedDeliveryTime
                    ));
                }
            }

            return Result.Ok(new CheckDeliveryZoneResponse(false, null, null, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking delivery zone: {MerchantId}", merchantId);
            return ServiceResult.HandleException<CheckDeliveryZoneResponse>(ex, _logger, "CheckDeliveryZone");
        }
    }

    /// <summary>
    /// Point-in-polygon algoritması (Ray casting)
    /// </summary>
    private static bool IsPointInPolygon(decimal lat, decimal lng, List<DeliveryZonePoint> polygon)
    {
        bool inside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; i++)
        {
            var pi = polygon[i];
            var pj = polygon[j];

            if (((pi.Latitude > lat) != (pj.Latitude > lat)) &&
                (lng < (pj.Longitude - pi.Longitude) * (lat - pi.Latitude) / (pj.Latitude - pi.Latitude) + pi.Longitude))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }
}
