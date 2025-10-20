using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Sipariş takip servisi implementasyonu: mock data ile tracking yönetimi, konum/durum güncelleme, Haversine.
/// </summary>
public class OrderTrackingService : IOrderTrackingService
{
    private readonly List<OrderTrackingDto> _mockTrackings;
    private readonly List<LocationHistoryDto> _mockLocationHistory;

    public OrderTrackingService()
    {
        // Mock tracking data
        _mockTrackings = new List<OrderTrackingDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CourierId = Guid.NewGuid(),
                CourierName = "Ahmet Yılmaz",
                CourierPhone = "+905551234567",
                Status = TrackingStatus.OnTheWay,
                StatusDisplayName = "Yolda",
                StatusMessage = "Siparişiniz size doğru yolda",
                Latitude = 41.0082,
                Longitude = 28.9784,
                Address = "Beşiktaş, İstanbul",
                City = "İstanbul",
                District = "Beşiktaş",
                LocationUpdateType = LocationUpdateType.GPS,
                Accuracy = 10.5,
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(15),
                EstimatedMinutesRemaining = 15,
                DistanceFromDestination = 2.5,
                IsActive = true,
                LastUpdatedAt = DateTime.UtcNow.AddMinutes(-2),
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                CourierId = Guid.NewGuid(),
                CourierName = "Mehmet Demir",
                CourierPhone = "+905559876543",
                Status = TrackingStatus.Preparing,
                StatusDisplayName = "Hazırlanıyor",
                StatusMessage = "Siparişiniz hazırlanıyor",
                Latitude = 41.0082,
                Longitude = 28.9784,
                Address = "Kadıköy, İstanbul",
                City = "İstanbul",
                District = "Kadıköy",
                LocationUpdateType = LocationUpdateType.Manual,
                Accuracy = 50.0,
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(45),
                EstimatedMinutesRemaining = 45,
                DistanceFromDestination = 8.2,
                IsActive = true,
                LastUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                CreatedAt = DateTime.UtcNow.AddMinutes(-20)
            }
        };

        // Mock location history
        _mockLocationHistory = new List<LocationHistoryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderTrackingId = _mockTrackings[0].Id,
                Latitude = 41.0082,
                Longitude = 28.9784,
                Address = "Beşiktaş, İstanbul",
                City = "İstanbul",
                District = "Beşiktaş",
                UpdateType = LocationUpdateType.GPS,
                UpdateTypeDisplayName = "GPS",
                Accuracy = 10.5,
                Speed = 25.5,
                Bearing = 45.0,
                Altitude = 100.0,
                DeviceInfo = "iPhone 13",
                AppVersion = "1.2.3",
                RecordedAt = DateTime.UtcNow.AddMinutes(-2),
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            }
        };
    }

    /// <summary>
    /// Sipariş ID'sine göre tracking kaydını getirir (mock data).
    /// </summary>
    public Task<OrderTrackingDto?> GetTrackingByOrderIdAsync(Guid orderId)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.OrderId == orderId);
        return Task.FromResult(tracking);
    }

    /// <summary>
    /// Tracking ID'sine göre kaydı getirir (mock data).
    /// </summary>
    public Task<OrderTrackingDto?> GetTrackingByIdAsync(Guid trackingId)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.Id == trackingId);
        return Task.FromResult(tracking);
    }

    /// <summary>
    /// Yeni tracking kaydı oluşturur (mock data).
    /// </summary>
    public Task<OrderTrackingDto> CreateTrackingAsync(Guid orderId, Guid? courierId = null)
    {
        var tracking = new OrderTrackingDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CourierId = courierId,
            CourierName = "Test Courier",
            CourierPhone = "+905551234567",
            Status = TrackingStatus.OrderPlaced,
            StatusDisplayName = "Sipariş Verildi",
            StatusMessage = "Siparişiniz alındı ve işleme konuldu",
            LocationUpdateType = LocationUpdateType.Manual,
            IsActive = true,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _mockTrackings.Add(tracking);
        return Task.FromResult(tracking);
    }

    /// <summary>
    /// Konum günceller (mesafe/ETA hesaplama, konum geçmişi, Haversine, mock data).
    /// </summary>
    public Task<LocationUpdateResponse> UpdateLocationAsync(LocationUpdateRequest request)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.Id == request.OrderTrackingId);
        if (tracking == null)
        {
            return Task.FromResult(new LocationUpdateResponse
            {
                Success = false,
                Message = "Tracking not found",
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Update tracking location
        tracking.Latitude = request.Latitude;
        tracking.Longitude = request.Longitude;
        tracking.Address = request.Address;
        tracking.City = request.City;
        tracking.District = request.District;
        tracking.LocationUpdateType = request.UpdateType;
        tracking.Accuracy = request.Accuracy;
        tracking.LastUpdatedAt = DateTime.UtcNow;

        // Calculate distance and ETA (mock calculation)
        tracking.DistanceFromDestination = CalculateDistance(request.Latitude, request.Longitude, 41.0082, 28.9784);
        tracking.EstimatedMinutesRemaining = (int)(tracking.DistanceFromDestination * 3); // 3 minutes per km
        tracking.EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(tracking.EstimatedMinutesRemaining ?? 0);

        // Add to location history
        var historyEntry = new LocationHistoryDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = request.OrderTrackingId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Address = request.Address,
            City = request.City,
            District = request.District,
            UpdateType = request.UpdateType,
            UpdateTypeDisplayName = request.UpdateType.GetDisplayName(),
            Accuracy = request.Accuracy,
            Speed = request.Speed,
            Bearing = request.Bearing,
            Altitude = request.Altitude,
            DeviceInfo = request.DeviceInfo,
            AppVersion = request.AppVersion,
            RecordedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _mockLocationHistory.Add(historyEntry);

        return Task.FromResult(new LocationUpdateResponse
        {
            Success = true,
            Message = "Location updated successfully",
            UpdatedAt = DateTime.UtcNow,
            DistanceFromDestination = tracking.DistanceFromDestination,
            EstimatedMinutesRemaining = tracking.EstimatedMinutesRemaining
        });
    }

    /// <summary>
    /// Durum günceller (validasyon, konum/adres güncellemesi, bildirim, mock data).
    /// </summary>
    public Task<StatusUpdateResponse> UpdateStatusAsync(StatusUpdateRequest request)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.Id == request.OrderTrackingId);
        if (tracking == null)
        {
            return Task.FromResult(new StatusUpdateResponse
            {
                Success = false,
                Message = "Tracking not found",
                UpdatedAt = DateTime.UtcNow,
                NotificationSent = false
            });
        }

        // Check if status transition is valid
        if (!tracking.Status.CanTransitionTo(request.Status))
        {
            return Task.FromResult(new StatusUpdateResponse
            {
                Success = false,
                Message = $"Invalid status transition from {tracking.Status} to {request.Status}",
                UpdatedAt = DateTime.UtcNow,
                NotificationSent = false
            });
        }

        // Update tracking status
        tracking.Status = request.Status;
        tracking.StatusDisplayName = request.Status.GetDisplayName();
        tracking.StatusMessage = request.StatusMessage ?? request.Status.GetDescription();
        tracking.LastUpdatedAt = DateTime.UtcNow;

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            tracking.Latitude = request.Latitude;
            tracking.Longitude = request.Longitude;
        }

        if (!string.IsNullOrEmpty(request.Address))
        {
            tracking.Address = request.Address;
        }

        // Set actual arrival time if status is Arrived
        if (request.Status == TrackingStatus.Arrived)
        {
            tracking.ActualArrivalTime = DateTime.UtcNow;
        }

        return Task.FromResult(new StatusUpdateResponse
        {
            Success = true,
            Message = "Status updated successfully",
            UpdatedAt = DateTime.UtcNow,
            NotificationSent = true // Mock notification sent
        });
    }

    /// <summary>
    /// Tracking kaydını siler (soft delete, mock data).
    /// </summary>
    public Task<bool> DeleteTrackingAsync(Guid trackingId)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.Id == trackingId);
        if (tracking != null)
        {
            tracking.IsActive = false;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Aktif tracking kayıtlarını getirir (mock data).
    /// </summary>
    public Task<List<OrderTrackingDto>> GetActiveTrackingsAsync()
    {
        var activeTrackings = _mockTrackings.Where(t => t.IsActive).ToList();
        return Task.FromResult(activeTrackings);
    }

    /// <summary>
    /// Kurye bazlı tracking kayıtlarını getirir (mock data).
    /// </summary>
    public Task<List<OrderTrackingDto>> GetTrackingsByCourierAsync(Guid courierId)
    {
        var courierTrackings = _mockTrackings.Where(t => t.CourierId == courierId).ToList();
        return Task.FromResult(courierTrackings);
    }

    /// <summary>
    /// Kullanıcı bazlı tracking kayıtlarını getirir (mock data).
    /// </summary>
    public Task<List<OrderTrackingDto>> GetTrackingsByUserAsync(Guid userId)
    {
        // Mock implementation - in real scenario, would filter by user's orders
        var userTrackings = _mockTrackings.Take(2).ToList();
        return Task.FromResult(userTrackings);
    }

    /// <summary>
    /// Tracking kayıtlarını arar (filtreleme ve sayfalama, mock data).
    /// </summary>
    public Task<TrackingSearchResponse> SearchTrackingsAsync(TrackingSearchRequest request)
    {
        var trackings = _mockTrackings.AsQueryable();

        if (request.OrderId.HasValue)
            trackings = trackings.Where(t => t.OrderId == request.OrderId.Value);

        if (request.CourierId.HasValue)
            trackings = trackings.Where(t => t.CourierId == request.CourierId.Value);

        if (request.Status.HasValue)
            trackings = trackings.Where(t => t.Status == request.Status.Value);

        if (request.IsActive.HasValue)
            trackings = trackings.Where(t => t.IsActive == request.IsActive.Value);

        if (request.StartDate.HasValue)
            trackings = trackings.Where(t => t.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            trackings = trackings.Where(t => t.CreatedAt <= request.EndDate.Value);

        var totalCount = trackings.Count();
        var pagedTrackings = trackings
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = new TrackingSearchResponse
        {
            Trackings = pagedTrackings,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Konum geçmişini getirir (son N kayıt, zaman sıralı, mock data).
    /// </summary>
    public Task<List<LocationHistoryDto>> GetLocationHistoryAsync(Guid trackingId, int count = 50)
    {
        var history = _mockLocationHistory
            .Where(h => h.OrderTrackingId == trackingId)
            .OrderByDescending(h => h.RecordedAt)
            .Take(count)
            .ToList();

        return Task.FromResult(history);
    }

    /// <summary>
    /// Tracking istatistiklerini getirir (durum sayıları, ortalama süre/mesafe, mock data).
    /// </summary>
    public Task<TrackingStatisticsDto> GetTrackingStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var statistics = new TrackingStatisticsDto
        {
            TotalTrackings = _mockTrackings.Count,
            ActiveTrackings = _mockTrackings.Count(t => t.IsActive),
            CompletedTrackings = _mockTrackings.Count(t => t.Status == TrackingStatus.Delivered),
            CancelledTrackings = _mockTrackings.Count(t => t.Status == TrackingStatus.Cancelled),
            AverageDeliveryTime = 25.5, // Mock average
            AverageDistance = 5.2, // Mock average
            StatusCounts = _mockTrackings.GroupBy(t => t.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            PeriodStart = startDate,
            PeriodEnd = endDate
        };

        return Task.FromResult(statistics);
    }

    /// <summary>
    /// Durum geçişinin geçerli olup olmadığını kontrol eder (mock data).
    /// </summary>
    public Task<bool> CanTransitionToStatusAsync(Guid trackingId, TrackingStatus newStatus)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.Id == trackingId);
        if (tracking == null)
            return Task.FromResult(false);

        return Task.FromResult(tracking.Status.CanTransitionTo(newStatus));
    }

    /// <summary>
    /// Mevcut geçerli durum geçişlerini getirir (mock data).
    /// </summary>
    public Task<List<TrackingStatus>> GetAvailableTransitionsAsync(Guid trackingId)
    {
        var tracking = _mockTrackings.FirstOrDefault(t => t.Id == trackingId);
        if (tracking == null)
            return Task.FromResult(new List<TrackingStatus>());

        var availableTransitions = Enum.GetValues<TrackingStatus>()
            .Where(status => tracking.Status.CanTransitionTo(status))
            .ToList();

        return Task.FromResult(availableTransitions);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for calculating distance between two points
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}
