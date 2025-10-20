using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

/// <summary>
/// Gerçek zamanlı takip servisi implementasyonu: mock data ile realtime tracking, mesafe hesaplama, Haversine.
/// </summary>
public class RealtimeTrackingService : IRealtimeTrackingService
{
    private readonly List<RealtimeTrackingData> _mockRealtimeData;

    public RealtimeTrackingService()
    {
        // Mock realtime tracking data
        _mockRealtimeData = new List<RealtimeTrackingData>
        {
            new()
            {
                OrderTrackingId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Status = Domain.Enums.TrackingStatus.OnTheWay,
                StatusDisplayName = "Yolda",
                Latitude = 41.0082,
                Longitude = 28.9784,
                Address = "Beşiktaş, İstanbul",
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(15),
                EstimatedMinutesRemaining = 15,
                DistanceFromDestination = 2.5,
                LastUpdatedAt = DateTime.UtcNow.AddMinutes(-2),
                IsActive = true
            },
            new()
            {
                OrderTrackingId = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Status = Domain.Enums.TrackingStatus.Preparing,
                StatusDisplayName = "Hazırlanıyor",
                Latitude = 41.0082,
                Longitude = 28.9784,
                Address = "Kadıköy, İstanbul",
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(45),
                EstimatedMinutesRemaining = 45,
                DistanceFromDestination = 8.2,
                LastUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                IsActive = true
            }
        };
    }

    /// <summary>
    /// Gerçek zamanlı tracking verisini getirir (mock data).
    /// </summary>
    public Task<RealtimeTrackingData?> GetRealtimeDataAsync(Guid orderTrackingId)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        return Task.FromResult(data);
    }

    /// <summary>
    /// Aktif gerçek zamanlı tracking verilerini getirir (mock data).
    /// </summary>
    public Task<List<RealtimeTrackingData>> GetActiveRealtimeDataAsync()
    {
        var activeData = _mockRealtimeData.Where(d => d.IsActive).ToList();
        return Task.FromResult(activeData);
    }

    /// <summary>
    /// Kullanıcı bazlı gerçek zamanlı verileri getirir (mock data).
    /// </summary>
    public Task<List<RealtimeTrackingData>> GetRealtimeDataByUserAsync(Guid userId)
    {
        // Mock implementation - in real scenario, would filter by user's orders
        var userData = _mockRealtimeData.Take(2).ToList();
        return Task.FromResult(userData);
    }

    /// <summary>
    /// Kurye bazlı gerçek zamanlı verileri getirir (mock data).
    /// </summary>
    public Task<List<RealtimeTrackingData>> GetRealtimeDataByCourierAsync(Guid courierId)
    {
        // Mock implementation - in real scenario, would filter by courier's orders
        var courierData = _mockRealtimeData.Take(1).ToList();
        return Task.FromResult(courierData);
    }

    /// <summary>
    /// Tracking başlatır (duplicate kontrolü, mock data).
    /// </summary>
    public Task<bool> StartTrackingAsync(Guid orderId, Guid? courierId = null)
    {
        // Check if tracking already exists
        var existingData = _mockRealtimeData.FirstOrDefault(d => d.OrderId == orderId);
        if (existingData != null)
        {
            return Task.FromResult(false); // Already tracking
        }

        // Create new tracking data
        var newData = new RealtimeTrackingData
        {
            OrderTrackingId = Guid.NewGuid(),
            OrderId = orderId,
            Status = Domain.Enums.TrackingStatus.OrderPlaced,
            StatusDisplayName = "Sipariş Verildi",
            Latitude = 41.0082, // Default location
            Longitude = 28.9784,
            Address = "İstanbul",
            EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(30),
            EstimatedMinutesRemaining = 30,
            DistanceFromDestination = 5.0,
            LastUpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _mockRealtimeData.Add(newData);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Tracking durdurur (soft delete, mock data).
    /// </summary>
    public Task<bool> StopTrackingAsync(Guid orderTrackingId)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        if (data != null)
        {
            data.IsActive = false;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Tracking duraklatır (mock data).
    /// </summary>
    public Task<bool> PauseTrackingAsync(Guid orderTrackingId)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        if (data != null && data.IsActive)
        {
            // In a real implementation, you might set a paused status
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Tracking devam ettirir (mock data).
    /// </summary>
    public Task<bool> ResumeTrackingAsync(Guid orderTrackingId)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        if (data != null)
        {
            data.IsActive = true;
            data.LastUpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Tracking aktif mi kontrol eder (mock data).
    /// </summary>
    public Task<bool> IsTrackingActiveAsync(Guid orderTrackingId)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        return Task.FromResult(data?.IsActive ?? false);
    }

    /// <summary>
    /// Tracking metriklerini getirir (hız, bearing, accuracy, süre, mock data).
    /// </summary>
    public Task<Dictionary<string, object>> GetTrackingMetricsAsync(Guid orderTrackingId)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        if (data == null)
        {
            return Task.FromResult(new Dictionary<string, object>());
        }

        var metrics = new Dictionary<string, object>
        {
            ["orderTrackingId"] = data.OrderTrackingId,
            ["orderId"] = data.OrderId,
            ["status"] = data.Status.ToString(),
            ["statusDisplayName"] = data.StatusDisplayName,
            ["latitude"] = data.Latitude ?? 0.0,
            ["longitude"] = data.Longitude ?? 0.0,
            ["address"] = data.Address ?? "",
            ["estimatedArrivalTime"] = data.EstimatedArrivalTime?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "",
            ["estimatedMinutesRemaining"] = data.EstimatedMinutesRemaining ?? 0,
            ["distanceFromDestination"] = data.DistanceFromDestination ?? 0.0,
            ["lastUpdatedAt"] = data.LastUpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["isActive"] = data.IsActive,
            ["trackingDuration"] = (DateTime.UtcNow - data.LastUpdatedAt).TotalMinutes,
            ["accuracy"] = 95.5, // Mock accuracy
            ["speed"] = 25.0, // Mock speed in km/h
            ["bearing"] = 45.0 // Mock bearing in degrees
        };

        return Task.FromResult(metrics);
    }

    /// <summary>
    /// Koordinatların geçerli olup olmadığını kontrol eder (Türkiye sınırları: 35-42 lat, 25-45 lon).
    /// </summary>
    public Task<bool> ValidateLocationAsync(double latitude, double longitude)
    {
        // Validate coordinates are within reasonable bounds
        if (latitude < -90 || latitude > 90)
            return Task.FromResult(false);

        if (longitude < -180 || longitude > 180)
            return Task.FromResult(false);

        // Check if location is within Turkey bounds (rough approximation)
        if (latitude < 35.0 || latitude > 42.0 || longitude < 25.0 || longitude > 45.0)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    /// <summary>
    /// Hedefe olan mesafeyi hesaplar (Haversine formula, mock destination: İstanbul merkez).
    /// </summary>
    public Task<double> CalculateDistanceToDestinationAsync(Guid orderTrackingId, double latitude, double longitude)
    {
        var data = _mockRealtimeData.FirstOrDefault(d => d.OrderTrackingId == orderTrackingId);
        if (data == null)
            return Task.FromResult(0.0);

        // Mock destination coordinates (Istanbul center)
        const double destinationLat = 41.0082;
        const double destinationLon = 28.9784;

        // Calculate distance using Haversine formula
        var distance = CalculateDistance(latitude, longitude, destinationLat, destinationLon);
        return Task.FromResult(distance);
    }

    /// <summary>
    /// Hedefe yakın olup olmadığını kontrol eder (varsayılan eşik: 500 metre).
    /// </summary>
    public Task<bool> IsNearDestinationAsync(Guid orderTrackingId, double latitude, double longitude, double thresholdMeters = 500)
    {
        var distance = CalculateDistanceToDestinationAsync(orderTrackingId, latitude, longitude);
        var distanceKm = distance.Result;
        var thresholdKm = thresholdMeters / 1000.0;

        return Task.FromResult(distanceKm <= thresholdKm);
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
