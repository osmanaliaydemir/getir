using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Getir.Application.Services.DeliveryOptimization;

/// <summary>
/// Teslimat kapasitesi yönetimi servisi implementasyonu
/// </summary>
public class DeliveryCapacityService : BaseService, IDeliveryCapacityService
{
    public DeliveryCapacityService(
        IUnitOfWork unitOfWork,
        ILogger<DeliveryCapacityService> logger,
        ILoggingService loggingService,
        ICacheService cacheService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
    }

    public async Task<Result<DeliveryCapacityResponse>> CreateCapacityAsync(
        DeliveryCapacityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Mevcut kapasiteyi kontrol et
            var existingCapacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == request.MerchantId && 
                                 dc.DeliveryZoneId == request.DeliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (existingCapacity != null)
            {
                return Result.Fail<DeliveryCapacityResponse>(
                    "Delivery capacity already exists for this merchant and zone", 
                    "CAPACITY_ALREADY_EXISTS");
            }

            var capacity = new DeliveryCapacity
            {
                Id = Guid.NewGuid(),
                MerchantId = request.MerchantId,
                DeliveryZoneId = request.DeliveryZoneId,
                MaxConcurrentDeliveries = request.MaxConcurrentDeliveries,
                MaxDailyDeliveries = request.MaxDailyDeliveries,
                MaxWeeklyDeliveries = request.MaxWeeklyDeliveries,
                PeakStartTime = request.PeakStartTime,
                PeakEndTime = request.PeakEndTime,
                PeakHourCapacityReduction = request.PeakHourCapacityReduction,
                MaxDeliveryDistanceKm = request.MaxDeliveryDistanceKm,
                DistanceBasedFeeMultiplier = request.DistanceBasedFeeMultiplier,
                IsDynamicCapacityEnabled = request.IsDynamicCapacityEnabled,
                CurrentActiveDeliveries = 0,
                TodayDeliveryCount = 0,
                ThisWeekDeliveryCount = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastResetDate = DateTime.UtcNow.Date
            };

            await _unitOfWork.Repository<DeliveryCapacity>().AddAsync(capacity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(capacity);

            _loggingService.LogBusinessEvent("DeliveryCapacityCreated", new
            {
                capacity.Id,
                request.MerchantId,
                request.DeliveryZoneId,
                request.MaxConcurrentDeliveries,
                request.MaxDailyDeliveries
            });

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating delivery capacity for merchant {MerchantId}", request.MerchantId);
            _loggingService.LogError("Create delivery capacity failed", ex, new { request.MerchantId });
            return Result.Fail<DeliveryCapacityResponse>("Failed to create delivery capacity", "CREATE_CAPACITY_ERROR");
        }
    }

    public async Task<Result<DeliveryCapacityResponse>> UpdateCapacityAsync(
        Guid capacityId,
        DeliveryCapacityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.Id == capacityId,
                    cancellationToken: cancellationToken);

            if (capacity == null)
            {
                return Result.Fail<DeliveryCapacityResponse>("Delivery capacity not found", "CAPACITY_NOT_FOUND");
            }

            // Güncelleme
            capacity.MaxConcurrentDeliveries = request.MaxConcurrentDeliveries;
            capacity.MaxDailyDeliveries = request.MaxDailyDeliveries;
            capacity.MaxWeeklyDeliveries = request.MaxWeeklyDeliveries;
            capacity.PeakStartTime = request.PeakStartTime;
            capacity.PeakEndTime = request.PeakEndTime;
            capacity.PeakHourCapacityReduction = request.PeakHourCapacityReduction;
            capacity.MaxDeliveryDistanceKm = request.MaxDeliveryDistanceKm;
            capacity.DistanceBasedFeeMultiplier = request.DistanceBasedFeeMultiplier;
            capacity.IsDynamicCapacityEnabled = request.IsDynamicCapacityEnabled;
            capacity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = MapToResponse(capacity);

            _loggingService.LogBusinessEvent("DeliveryCapacityUpdated", new
            {
                capacityId,
                request.MerchantId,
                request.MaxConcurrentDeliveries,
                request.MaxDailyDeliveries
            });

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery capacity {CapacityId}", capacityId);
            _loggingService.LogError("Update delivery capacity failed", ex, new { capacityId });
            return Result.Fail<DeliveryCapacityResponse>("Failed to update delivery capacity", "UPDATE_CAPACITY_ERROR");
        }
    }

    public async Task<Result<DeliveryCapacityCheckResponse>> CheckCapacityAsync(
        DeliveryCapacityCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == request.MerchantId && 
                                 dc.DeliveryZoneId == request.DeliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (capacity == null)
            {
                // Varsayılan kapasite ayarları
                return Result.Ok(new DeliveryCapacityCheckResponse(
                    CanAcceptDelivery: true,
                    Reason: null,
                    AvailableCapacity: 10, // Varsayılan kapasite
                    CurrentLoad: 0,
                    AdjustedDeliveryFee: null,
                    EstimatedWaitTimeMinutes: 0,
                    NextAvailableSlot: null));
            }

            // Kapasite kontrolleri
            var canAccept = true;
            var reason = "";
            var availableCapacity = capacity.MaxConcurrentDeliveries - capacity.CurrentActiveDeliveries;
            var adjustedFee = (decimal?)null;

            // Eşzamanlı teslimat kontrolü
            if (capacity.CurrentActiveDeliveries >= capacity.MaxConcurrentDeliveries)
            {
                canAccept = false;
                reason = "Maximum concurrent deliveries reached";
            }

            // Günlük kapasite kontrolü
            if (capacity.TodayDeliveryCount >= capacity.MaxDailyDeliveries)
            {
                canAccept = false;
                reason = "Daily delivery limit reached";
            }

            // Haftalık kapasite kontrolü
            if (capacity.ThisWeekDeliveryCount >= capacity.MaxWeeklyDeliveries)
            {
                canAccept = false;
                reason = "Weekly delivery limit reached";
            }

            // Mesafe kontrolü
            if (request.DeliveryDistanceKm.HasValue && 
                capacity.MaxDeliveryDistanceKm.HasValue &&
                request.DeliveryDistanceKm.Value > capacity.MaxDeliveryDistanceKm.Value)
            {
                canAccept = false;
                reason = "Delivery distance exceeds maximum allowed distance";
            }

            // Mesafeye göre ücret ayarlama
            if (request.DeliveryDistanceKm.HasValue && 
                capacity.DistanceBasedFeeMultiplier.HasValue &&
                capacity.DistanceBasedFeeMultiplier.Value != 1.0m)
            {
                // Burada gerçek ücret hesaplaması yapılacak
                adjustedFee = (decimal)request.DeliveryDistanceKm.Value * capacity.DistanceBasedFeeMultiplier.Value;
            }

            // Yoğun saat kontrolü
            var isPeakHour = false;
            if (capacity.PeakStartTime.HasValue && capacity.PeakEndTime.HasValue)
            {
                var currentTime = DateTime.Now.TimeOfDay;
                isPeakHour = IsWithinTimeRange(currentTime, capacity.PeakStartTime.Value, capacity.PeakEndTime.Value);
                
                if (isPeakHour && capacity.PeakHourCapacityReduction > 0)
                {
                    availableCapacity = (int)(availableCapacity * (100 - capacity.PeakHourCapacityReduction) / 100.0);
                }
            }

            var response = new DeliveryCapacityCheckResponse(
                CanAcceptDelivery: canAccept,
                Reason: canAccept ? null : reason,
                AvailableCapacity: availableCapacity,
                CurrentLoad: capacity.CurrentActiveDeliveries,
                AdjustedDeliveryFee: adjustedFee,
                EstimatedWaitTimeMinutes: canAccept ? 0 : CalculateWaitTime(capacity),
                NextAvailableSlot: canAccept ? null : DateTime.UtcNow.AddMinutes(30));

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking delivery capacity for merchant {MerchantId}", request.MerchantId);
            _loggingService.LogError("Check delivery capacity failed", ex, new { request.MerchantId });
            return Result.Fail<DeliveryCapacityCheckResponse>("Failed to check delivery capacity", "CHECK_CAPACITY_ERROR");
        }
    }

    public async Task<Result> IncrementActiveDeliveriesAsync(
        Guid merchantId,
        Guid? deliveryZoneId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == merchantId && 
                                 dc.DeliveryZoneId == deliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (capacity != null)
            {
                capacity.CurrentActiveDeliveries++;
                capacity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _loggingService.LogBusinessEvent("ActiveDeliveriesIncremented", new
                {
                    merchantId,
                    deliveryZoneId,
                    NewCount = capacity.CurrentActiveDeliveries
                });
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing active deliveries for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to increment active deliveries", "INCREMENT_ACTIVE_DELIVERIES_ERROR");
        }
    }

    public async Task<Result> DecrementActiveDeliveriesAsync(
        Guid merchantId,
        Guid? deliveryZoneId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == merchantId && 
                                 dc.DeliveryZoneId == deliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (capacity != null && capacity.CurrentActiveDeliveries > 0)
            {
                capacity.CurrentActiveDeliveries--;
                capacity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _loggingService.LogBusinessEvent("ActiveDeliveriesDecremented", new
                {
                    merchantId,
                    deliveryZoneId,
                    NewCount = capacity.CurrentActiveDeliveries
                });
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing active deliveries for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to decrement active deliveries", "DECREMENT_ACTIVE_DELIVERIES_ERROR");
        }
    }

    public async Task<Result> IncrementDailyDeliveriesAsync(
        Guid merchantId,
        Guid? deliveryZoneId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == merchantId && 
                                 dc.DeliveryZoneId == deliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (capacity != null)
            {
                capacity.TodayDeliveryCount++;
                capacity.ThisWeekDeliveryCount++;
                capacity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _loggingService.LogBusinessEvent("DailyDeliveriesIncremented", new
                {
                    merchantId,
                    deliveryZoneId,
                    TodayCount = capacity.TodayDeliveryCount,
                    WeekCount = capacity.ThisWeekDeliveryCount
                });
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing daily deliveries for merchant {MerchantId}", merchantId);
            return Result.Fail("Failed to increment daily deliveries", "INCREMENT_DAILY_DELIVERIES_ERROR");
        }
    }

    public async Task<Result<DeliveryCapacityResponse>> GetCapacityAsync(
        Guid merchantId,
        Guid? deliveryZoneId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == merchantId && 
                                 dc.DeliveryZoneId == deliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (capacity == null)
            {
                return Result.Fail<DeliveryCapacityResponse>("Delivery capacity not found", "CAPACITY_NOT_FOUND");
            }

            var response = MapToResponse(capacity);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery capacity for merchant {MerchantId}", merchantId);
            return Result.Fail<DeliveryCapacityResponse>("Failed to get delivery capacity", "GET_CAPACITY_ERROR");
        }
    }

    public async Task<Result> AdjustCapacityAsync(
        DynamicCapacityAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var capacity = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .FirstOrDefaultAsync(
                    filter: dc => dc.MerchantId == request.MerchantId && 
                                 dc.DeliveryZoneId == request.DeliveryZoneId &&
                                 dc.IsActive,
                    cancellationToken: cancellationToken);

            if (capacity == null)
            {
                return Result.Fail("Delivery capacity not found", "CAPACITY_NOT_FOUND");
            }

            // Dinamik kapasite ayarı
            if (request.CapacityAdjustment > 0)
            {
                capacity.MaxConcurrentDeliveries += request.CapacityAdjustment;
            }
            else if (request.CapacityAdjustment < 0)
            {
                var reduction = Math.Abs(request.CapacityAdjustment);
                capacity.MaxConcurrentDeliveries = Math.Max(1, capacity.MaxConcurrentDeliveries - reduction);
            }

            capacity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("CapacityAdjusted", new
            {
                request.MerchantId,
                request.DeliveryZoneId,
                request.CapacityAdjustment,
                request.Reason,
                NewCapacity = capacity.MaxConcurrentDeliveries
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting capacity for merchant {MerchantId}", request.MerchantId);
            return Result.Fail("Failed to adjust capacity", "ADJUST_CAPACITY_ERROR");
        }
    }

    public async Task<Result> SendCapacityAlertAsync(
        CapacityAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Burada bildirim servisi kullanılarak uyarı gönderilecek
            _loggingService.LogBusinessEvent("CapacityAlertSent", new
            {
                request.MerchantId,
                request.AlertType,
                request.Message,
                request.CurrentLoad,
                request.MaxCapacity,
                request.AlertTime
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending capacity alert for merchant {MerchantId}", request.MerchantId);
            return Result.Fail("Failed to send capacity alert", "SEND_ALERT_ERROR");
        }
    }

    public async Task<Result> ResetDailyCountersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var capacities = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .ListAsync(
                    filter: dc => dc.LastResetDate < today,
                    cancellationToken: cancellationToken);

            foreach (var capacity in capacities)
            {
                capacity.TodayDeliveryCount = 0;
                capacity.LastResetDate = today;
                capacity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("DailyCountersReset", new
            {
                ResetCount = capacities.Count,
                ResetDate = today
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting daily counters");
            return Result.Fail("Failed to reset daily counters", "RESET_DAILY_COUNTERS_ERROR");
        }
    }

    public async Task<Result> ResetWeeklyCountersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var thisWeek = GetWeekStart(DateTime.UtcNow);
            var capacities = await _unitOfWork.ReadRepository<DeliveryCapacity>()
                .ListAsync(
                    filter: dc => dc.LastResetDate < thisWeek,
                    cancellationToken: cancellationToken);

            foreach (var capacity in capacities)
            {
                capacity.ThisWeekDeliveryCount = 0;
                capacity.LastResetDate = thisWeek;
                capacity.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<DeliveryCapacity>().Update(capacity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _loggingService.LogBusinessEvent("WeeklyCountersReset", new
            {
                ResetCount = capacities.Count,
                ResetDate = thisWeek
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting weekly counters");
            return Result.Fail("Failed to reset weekly counters", "RESET_WEEKLY_COUNTERS_ERROR");
        }
    }

    #region Helper Methods

    private DeliveryCapacityResponse MapToResponse(DeliveryCapacity capacity)
    {
        return new DeliveryCapacityResponse(
            capacity.Id,
            capacity.MerchantId,
            capacity.DeliveryZoneId,
            capacity.MaxConcurrentDeliveries,
            capacity.MaxDailyDeliveries,
            capacity.MaxWeeklyDeliveries,
            capacity.PeakStartTime,
            capacity.PeakEndTime,
            capacity.PeakHourCapacityReduction,
            capacity.MaxDeliveryDistanceKm,
            capacity.DistanceBasedFeeMultiplier,
            capacity.IsDynamicCapacityEnabled,
            capacity.CurrentActiveDeliveries,
            capacity.TodayDeliveryCount,
            capacity.ThisWeekDeliveryCount,
            capacity.IsActive,
            capacity.CreatedAt,
            capacity.UpdatedAt);
    }

    private bool IsWithinTimeRange(TimeSpan currentTime, TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime <= endTime)
        {
            return currentTime >= startTime && currentTime <= endTime;
        }
        else
        {
            // Gece yarısını geçen saatler (örn: 22:00 - 06:00)
            return currentTime >= startTime || currentTime <= endTime;
        }
    }

    private int CalculateWaitTime(DeliveryCapacity capacity)
    {
        // Basit bekleme süresi hesaplama
        var loadPercentage = (double)capacity.CurrentActiveDeliveries / capacity.MaxConcurrentDeliveries;
        
        if (loadPercentage < 0.5) return 0;
        if (loadPercentage < 0.8) return 15;
        if (loadPercentage < 0.95) return 30;
        return 60;
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return date.AddDays(-dayOfWeek).Date;
    }

    #endregion
}
