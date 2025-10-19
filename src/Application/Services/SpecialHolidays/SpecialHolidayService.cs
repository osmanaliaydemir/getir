using System.Linq;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.SpecialHolidays;

public class SpecialHolidayService : BaseService, ISpecialHolidayService
{
    public SpecialHolidayService(IUnitOfWork unitOfWork, ILogger<SpecialHolidayService> logger, ILoggingService loggingService, ICacheService cacheService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }
    public async Task<Result<List<SpecialHolidayResponse>>> GetAllSpecialHolidaysAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetAllSpecialHolidaysInternalAsync(cancellationToken),
            "GetAllSpecialHolidays",
            null,
            cancellationToken);
    }
    private async Task<Result<List<SpecialHolidayResponse>>> GetAllSpecialHolidaysInternalAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.AllSpecialHolidays();

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var specialHolidays = await _unitOfWork.ReadRepository<SpecialHoliday>()
                        .ListAsync(
                            filter: sh => sh.IsActive,
                            orderBy: sh => sh.StartDate,
                            cancellationToken: cancellationToken);

                    var response = specialHolidays.Select(MapToResponse).ToList();
                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL - very static
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting all special holidays", ex);
            return ServiceResult.HandleException<List<SpecialHolidayResponse>>(ex, _logger, "GetAllSpecialHolidays");
        }
    }
    public async Task<Result<List<SpecialHolidayResponse>>> GetSpecialHolidaysByMerchantAsync(Guid merchantId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetSpecialHolidaysByMerchantInternalAsync(merchantId, includeInactive, cancellationToken),
            "GetSpecialHolidaysByMerchant",
            new { MerchantId = merchantId, IncludeInactive = includeInactive },
            cancellationToken);
    }
    private async Task<Result<List<SpecialHolidayResponse>>> GetSpecialHolidaysByMerchantInternalAsync(Guid merchantId, bool includeInactive, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.SpecialHolidaysByMerchant(merchantId);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var specialHolidays = await _unitOfWork.ReadRepository<SpecialHoliday>()
                        .ListAsync(
                            filter: sh => sh.MerchantId == merchantId && (includeInactive || sh.IsActive),
                            orderBy: sh => sh.StartDate,
                            cancellationToken: cancellationToken);

                    var response = specialHolidays.Select(MapToResponse).ToList();
                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting special holidays by merchant", ex, new { MerchantId = merchantId });
            return ServiceResult.HandleException<List<SpecialHolidayResponse>>(ex, _logger, "GetSpecialHolidaysByMerchant");
        }
    }
    public async Task<Result<List<SpecialHolidayResponse>>> GetSpecialHolidaysByDateRangeAsync(Guid merchantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetSpecialHolidaysByDateRangeInternalAsync(merchantId, startDate, endDate, cancellationToken),
            "GetSpecialHolidaysByDateRange",
            new { MerchantId = merchantId, StartDate = startDate, EndDate = endDate },
            cancellationToken);
    }
    private async Task<Result<List<SpecialHolidayResponse>>> GetSpecialHolidaysByDateRangeInternalAsync(Guid merchantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.SpecialHolidaysByDateRange(merchantId, startDate, endDate);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var specialHolidays = await _unitOfWork.ReadRepository<SpecialHoliday>()
                        .ListAsync(
                            filter: sh => sh.MerchantId == merchantId &&
                                         sh.IsActive &&
                                         sh.StartDate <= endDate &&
                                         sh.EndDate >= startDate,
                            orderBy: sh => sh.StartDate,
                            cancellationToken: cancellationToken);

                    var response = specialHolidays.Select(MapToResponse).ToList();
                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting special holidays by date range", ex,
                new { MerchantId = merchantId, StartDate = startDate, EndDate = endDate });
            return ServiceResult.HandleException<List<SpecialHolidayResponse>>(ex, _logger, "GetSpecialHolidaysByDateRange");
        }
    }
    public async Task<Result<SpecialHolidayResponse>> GetSpecialHolidayByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetSpecialHolidayByIdInternalAsync(id, cancellationToken),
            "GetSpecialHolidayById",
            new { Id = id },
            cancellationToken);
    }
    private async Task<Result<SpecialHolidayResponse>> GetSpecialHolidayByIdInternalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.SpecialHoliday(id);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var specialHoliday = await _unitOfWork.ReadRepository<SpecialHoliday>()
                        .FirstOrDefaultAsync(sh => sh.Id == id, cancellationToken: cancellationToken);

                    if (specialHoliday == null)
                    {
                        return Result.Fail<SpecialHolidayResponse>("Özel tatil bulunamadı", ErrorCodes.NOT_FOUND);
                    }

                    return Result.Ok(MapToResponse(specialHoliday));
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.ExtraLong), // 4 hours TTL
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting special holiday by id", ex, new { Id = id });
            return ServiceResult.HandleException<SpecialHolidayResponse>(ex, _logger, "GetSpecialHolidayById");
        }
    }
    public async Task<Result<SpecialHolidayResponse>> CreateSpecialHolidayAsync(CreateSpecialHolidayRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Merchant ownership kontrolü
            var merchant = await _unitOfWork.ReadRepository<Merchant>()
                .FirstOrDefaultAsync(
                    m => m.Id == request.MerchantId && m.OwnerId == merchantOwnerId,
                    cancellationToken: cancellationToken);

            if (merchant == null)
            {
                return Result.Fail<SpecialHolidayResponse>(
                    "Merchant bulunamadı veya erişim yetkiniz yok",
                    ErrorCodes.FORBIDDEN);
            }

            // Çakışan tatil var mı kontrol et
            var overlappingHolidays = await _unitOfWork.ReadRepository<SpecialHoliday>()
                .ListAsync(
                    filter: sh => sh.MerchantId == request.MerchantId &&
                                 sh.IsActive &&
                                 sh.StartDate <= request.EndDate &&
                                 sh.EndDate >= request.StartDate,
                    cancellationToken: cancellationToken);

            if (overlappingHolidays.Any())
            {
                return Result.Fail<SpecialHolidayResponse>(
                    "Bu tarih aralığında zaten bir özel tatil tanımlı",
                    ErrorCodes.VALIDATION_ERROR);
            }

            var specialHoliday = new SpecialHoliday
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsClosed = request.IsClosed,
                SpecialOpenTime = request.SpecialOpenTime,
                SpecialCloseTime = request.SpecialCloseTime,
                IsRecurring = request.IsRecurring,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<SpecialHoliday>().AddAsync(specialHoliday, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ============= CACHE INVALIDATION =============
            // Invalidate all holiday caches for this merchant
            await _cacheService.RemoveByPatternAsync(CacheKeys.AllHolidaysPattern(), cancellationToken);

            _loggingService.LogBusinessEvent(
                "SpecialHolidayCreated",
                new
                {
                    SpecialHolidayId = specialHoliday.Id,
                    MerchantId = request.MerchantId,
                    Title = request.Title,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                },
                LogLevel.Information);

            return Result.Ok(MapToResponse(specialHoliday));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error creating special holiday", ex, new { Request = request });
            return ServiceResult.HandleException<SpecialHolidayResponse>(ex, _logger, "CreateSpecialHoliday");
        }
    }
    public async Task<Result<SpecialHolidayResponse>> UpdateSpecialHolidayAsync(Guid id, UpdateSpecialHolidayRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialHoliday = await _unitOfWork.ReadRepository<SpecialHoliday>()
                .FirstOrDefaultAsync(sh => sh.Id == id, include: "Merchant", cancellationToken: cancellationToken);

            if (specialHoliday == null)
            {
                return Result.Fail<SpecialHolidayResponse>("Özel tatil bulunamadı", ErrorCodes.NOT_FOUND);
            }

            // Merchant ownership kontrolü
            if (specialHoliday.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail<SpecialHolidayResponse>("Bu özel tatili güncelleme yetkiniz yok", ErrorCodes.FORBIDDEN);
            }

            // Çakışan tatil var mı kontrol et (kendi haricinde)
            var overlappingHolidays = await _unitOfWork.ReadRepository<SpecialHoliday>()
                .ListAsync(
                    filter: sh => sh.MerchantId == specialHoliday.MerchantId &&
                                 sh.Id != id &&
                                 sh.IsActive &&
                                 sh.StartDate <= request.EndDate &&
                                 sh.EndDate >= request.StartDate,
                    cancellationToken: cancellationToken);

            if (overlappingHolidays.Any())
            {
                return Result.Fail<SpecialHolidayResponse>(
                    "Bu tarih aralığında zaten başka bir özel tatil tanımlı",
                    ErrorCodes.VALIDATION_ERROR);
            }

            specialHoliday.Title = request.Title;
            specialHoliday.Description = request.Description;
            specialHoliday.StartDate = request.StartDate;
            specialHoliday.EndDate = request.EndDate;
            specialHoliday.IsClosed = request.IsClosed;
            specialHoliday.SpecialOpenTime = request.SpecialOpenTime;
            specialHoliday.SpecialCloseTime = request.SpecialCloseTime;
            specialHoliday.IsRecurring = request.IsRecurring;
            specialHoliday.IsActive = request.IsActive;
            specialHoliday.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<SpecialHoliday>().Update(specialHoliday);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ============= CACHE INVALIDATION =============
            // Invalidate single holiday cache
            await _cacheService.RemoveAsync(CacheKeys.SpecialHoliday(id), cancellationToken);

            // Invalidate all holiday caches
            await _cacheService.RemoveByPatternAsync(CacheKeys.AllHolidaysPattern(), cancellationToken);

            _loggingService.LogBusinessEvent(
                "SpecialHolidayUpdated",
                new { SpecialHolidayId = id, Request = request },
                LogLevel.Information);

            return Result.Ok(MapToResponse(specialHoliday));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error updating special holiday", ex, new { Id = id, Request = request });
            return ServiceResult.HandleException<SpecialHolidayResponse>(ex, _logger, "UpdateSpecialHoliday");
        }
    }
    public async Task<Result> DeleteSpecialHolidayAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialHoliday = await _unitOfWork.ReadRepository<SpecialHoliday>()
                .FirstOrDefaultAsync(sh => sh.Id == id, include: "Merchant", cancellationToken: cancellationToken);

            if (specialHoliday == null)
            {
                return Result.Fail("Özel tatil bulunamadı", ErrorCodes.NOT_FOUND);
            }

            // Merchant ownership kontrolü
            if (specialHoliday.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail("Bu özel tatili silme yetkiniz yok", ErrorCodes.FORBIDDEN);
            }

            _unitOfWork.Repository<SpecialHoliday>().Delete(specialHoliday);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ============= CACHE INVALIDATION =============
            // Invalidate single holiday cache
            await _cacheService.RemoveAsync(CacheKeys.SpecialHoliday(id), cancellationToken);

            // Invalidate all holiday caches
            await _cacheService.RemoveByPatternAsync(CacheKeys.AllHolidaysPattern(), cancellationToken);

            _loggingService.LogBusinessEvent(
                "SpecialHolidayDeleted",
                new { SpecialHolidayId = id, MerchantId = specialHoliday.MerchantId },
                LogLevel.Information);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error deleting special holiday", ex, new { Id = id });
            return ServiceResult.HandleException(ex, _logger, "DeleteSpecialHoliday");
        }
    }
    public async Task<Result> ToggleSpecialHolidayStatusAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var specialHoliday = await _unitOfWork.ReadRepository<SpecialHoliday>()
                .FirstOrDefaultAsync(sh => sh.Id == id, include: "Merchant", cancellationToken: cancellationToken);

            if (specialHoliday == null)
            {
                return Result.Fail("Özel tatil bulunamadı", ErrorCodes.NOT_FOUND);
            }

            // Merchant ownership kontrolü
            if (specialHoliday.Merchant.OwnerId != merchantOwnerId)
            {
                return Result.Fail("Bu özel tatili değiştirme yetkiniz yok", ErrorCodes.FORBIDDEN);
            }

            specialHoliday.IsActive = !specialHoliday.IsActive;
            specialHoliday.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<SpecialHoliday>().Update(specialHoliday);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ============= CACHE INVALIDATION =============
            // Invalidate single holiday cache
            await _cacheService.RemoveAsync(CacheKeys.SpecialHoliday(id), cancellationToken);

            // Invalidate all holiday caches
            await _cacheService.RemoveByPatternAsync(CacheKeys.AllHolidaysPattern(), cancellationToken);

            _loggingService.LogBusinessEvent(
                "SpecialHolidayStatusToggled",
                new { SpecialHolidayId = id, IsActive = specialHoliday.IsActive },
                LogLevel.Information);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error toggling special holiday status", ex, new { Id = id });
            return ServiceResult.HandleException(ex, _logger, "ToggleSpecialHolidayStatus");
        }
    }
    public async Task<Result<MerchantAvailabilityResponse>> CheckMerchantAvailabilityAsync(Guid merchantId, DateTime checkDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Özel tatil var mı kontrol et
            var specialHoliday = await _unitOfWork.ReadRepository<SpecialHoliday>()
                .FirstOrDefaultAsync(
                    sh => sh.MerchantId == merchantId &&
                         sh.IsActive &&
                         sh.StartDate <= checkDate &&
                         sh.EndDate >= checkDate,
                    cancellationToken: cancellationToken);

            if (specialHoliday != null)
            {
                string message;
                if (specialHoliday.IsClosed)
                {
                    message = $"{specialHoliday.Title} nedeniyle kapalıdır";
                }
                else
                {
                    var openTime = specialHoliday.SpecialOpenTime?.ToString(@"hh\:mm") ?? "00:00";
                    var closeTime = specialHoliday.SpecialCloseTime?.ToString(@"hh\:mm") ?? "00:00";
                    message = $"{specialHoliday.Title} - Özel çalışma saatleri: {openTime} - {closeTime}";
                }

                var response = new MerchantAvailabilityResponse(
                    IsOpen: !specialHoliday.IsClosed,
                    Status: specialHoliday.IsClosed ? "Tatilde Kapalı" : "Özel Çalışma Saatleri",
                    SpecialHoliday: MapToResponse(specialHoliday),
                    Message: message
                );

                return Result.Ok(response);
            }

            // Özel tatil yoksa normal çalışma saatlerine göre kontrol et
            var dayOfWeek = checkDate.DayOfWeek;
            var workingHours = await _unitOfWork.ReadRepository<Domain.Entities.WorkingHours>()
                .FirstOrDefaultAsync(
                    wh => wh.MerchantId == merchantId && wh.DayOfWeek == dayOfWeek,
                    cancellationToken: cancellationToken);

            if (workingHours == null || workingHours.IsClosed)
            {
                return Result.Ok(new MerchantAvailabilityResponse(
                    IsOpen: false,
                    Status: "Kapalı",
                    SpecialHoliday: null,
                    Message: "Bu gün için çalışma saati tanımlanmamış veya kapalı"
                ));
            }

            var normalOpenTime = workingHours.OpenTime?.ToString(@"hh\:mm") ?? "00:00";
            var normalCloseTime = workingHours.CloseTime?.ToString(@"hh\:mm") ?? "00:00";

            return Result.Ok(new MerchantAvailabilityResponse(
                IsOpen: true,
                Status: "Açık",
                SpecialHoliday: null,
                Message: $"Normal çalışma saatleri: {normalOpenTime} - {normalCloseTime}"
            ));
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error checking merchant availability", ex,
                new { MerchantId = merchantId, CheckDate = checkDate });
            return ServiceResult.HandleException<MerchantAvailabilityResponse>(ex, _logger, "CheckMerchantAvailability");
        }
    }
    public async Task<Result<List<SpecialHolidayResponse>>> GetUpcomingSpecialHolidaysAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetUpcomingSpecialHolidaysInternalAsync(merchantId, cancellationToken),
            "GetUpcomingSpecialHolidays",
            new { MerchantId = merchantId },
            cancellationToken);
    }
    private async Task<Result<List<SpecialHolidayResponse>>> GetUpcomingSpecialHolidaysInternalAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use centralized cache key strategy
            var cacheKey = CacheKeys.UpcomingHolidays(merchantId);

            return await GetOrSetCacheAsync(
                cacheKey,
                async () =>
                {
                    var now = DateTime.UtcNow;
                    var specialHolidays = await _unitOfWork.ReadRepository<SpecialHoliday>()
                        .ListAsync(
                            filter: sh => sh.MerchantId == merchantId &&
                                         sh.IsActive &&
                                         sh.EndDate >= now,
                            orderBy: sh => sh.StartDate,
                            cancellationToken: cancellationToken);

                    var response = specialHolidays.Select(MapToResponse).ToList();
                    return ServiceResult.Success(response);
                },
                TimeSpan.FromMinutes(CacheKeys.TTL.VeryLong), // 1 hour TTL for upcoming holidays
                cancellationToken);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting upcoming special holidays", ex, new { MerchantId = merchantId });
            return ServiceResult.HandleException<List<SpecialHolidayResponse>>(ex, _logger, "GetUpcomingSpecialHolidays");
        }
    }
    private static SpecialHolidayResponse MapToResponse(SpecialHoliday specialHoliday)
    {
        return new SpecialHolidayResponse(
            specialHoliday.Id,
            specialHoliday.MerchantId,
            specialHoliday.Title,
            specialHoliday.Description,
            specialHoliday.StartDate,
            specialHoliday.EndDate,
            specialHoliday.IsClosed,
            specialHoliday.SpecialOpenTime,
            specialHoliday.SpecialCloseTime,
            specialHoliday.IsRecurring,
            specialHoliday.IsActive,
            specialHoliday.CreatedAt,
            specialHoliday.UpdatedAt
        );
    }
}
