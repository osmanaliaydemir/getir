using Getir.Application.DTO;

namespace Getir.Application.Services.RealtimeTracking;

public class ETAEstimationService : IETAEstimationService
{
    private readonly List<ETAEstimationDto> _mockETAEstimations;
    public ETAEstimationService()
    {
        // Mock ETA estimations
        _mockETAEstimations = new List<ETAEstimationDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderTrackingId = Guid.NewGuid(),
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(15),
                EstimatedMinutesRemaining = 15,
                DistanceRemaining = 2.5,
                AverageSpeed = 25.0,
                CalculationMethod = "algorithm",
                Confidence = 0.85,
                Notes = "Based on current traffic conditions",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderTrackingId = Guid.NewGuid(),
                EstimatedArrivalTime = DateTime.UtcNow.AddMinutes(45),
                EstimatedMinutesRemaining = 45,
                DistanceRemaining = 8.2,
                AverageSpeed = 20.0,
                CalculationMethod = "historical",
                Confidence = 0.75,
                Notes = "Based on historical delivery data",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        };
    }
    public Task<ETAEstimationDto?> GetCurrentETAAsync(Guid orderTrackingId)
    {
        var eta = _mockETAEstimations
            .Where(e => e.OrderTrackingId == orderTrackingId && e.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault();

        return Task.FromResult(eta);
    }
    public Task<ETAEstimationDto> CreateETAEstimationAsync(CreateETAEstimationRequest request)
    {
        var eta = new ETAEstimationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = request.OrderTrackingId,
            EstimatedArrivalTime = request.EstimatedArrivalTime,
            EstimatedMinutesRemaining = request.EstimatedMinutesRemaining,
            DistanceRemaining = request.DistanceRemaining,
            AverageSpeed = request.AverageSpeed,
            CalculationMethod = request.CalculationMethod,
            Confidence = request.Confidence,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockETAEstimations.Add(eta);
        return Task.FromResult(eta);
    }
    public Task<ETAEstimationDto> UpdateETAEstimationAsync(Guid id, UpdateETAEstimationRequest request)
    {
        var eta = _mockETAEstimations.FirstOrDefault(e => e.Id == id);
        if (eta == null)
        {
            throw new ArgumentException("ETA estimation not found");
        }

        eta.EstimatedArrivalTime = request.EstimatedArrivalTime;
        eta.EstimatedMinutesRemaining = request.EstimatedMinutesRemaining;
        eta.DistanceRemaining = request.DistanceRemaining;
        eta.AverageSpeed = request.AverageSpeed;
        eta.CalculationMethod = request.CalculationMethod;
        eta.Confidence = request.Confidence;
        eta.Notes = request.Notes;

        return Task.FromResult(eta);
    }
    public Task<bool> DeleteETAEstimationAsync(Guid id)
    {
        var eta = _mockETAEstimations.FirstOrDefault(e => e.Id == id);
        if (eta != null)
        {
            eta.IsActive = false;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
    public Task<List<ETAEstimationDto>> GetETAHistoryAsync(Guid orderTrackingId)
    {
        var history = _mockETAEstimations
            .Where(e => e.OrderTrackingId == orderTrackingId)
            .OrderByDescending(e => e.CreatedAt)
            .ToList();

        return Task.FromResult(history);
    }
    public Task<ETAEstimationDto> CalculateETAAsync(Guid orderTrackingId, double? currentLatitude = null, double? currentLongitude = null)
    {
        // Mock ETA calculation
        var distance = currentLatitude.HasValue && currentLongitude.HasValue 
            ? CalculateDistance(currentLatitude.Value, currentLongitude.Value, 41.0082, 28.9784)
            : 5.0; // Default distance

        var averageSpeed = 25.0; // km/h
        var estimatedMinutes = (int)(distance / averageSpeed * 60);
        var estimatedArrivalTime = DateTime.UtcNow.AddMinutes(estimatedMinutes);

        var eta = new ETAEstimationDto
        {
            Id = Guid.NewGuid(),
            OrderTrackingId = orderTrackingId,
            EstimatedArrivalTime = estimatedArrivalTime,
            EstimatedMinutesRemaining = estimatedMinutes,
            DistanceRemaining = distance,
            AverageSpeed = averageSpeed,
            CalculationMethod = "algorithm",
            Confidence = 0.8,
            Notes = "Calculated based on current location and traffic conditions",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockETAEstimations.Add(eta);
        return Task.FromResult(eta);
    }
    public Task<bool> ValidateETAAsync(Guid orderTrackingId, DateTime estimatedArrivalTime)
    {
        // Mock validation - check if ETA is reasonable
        var now = DateTime.UtcNow;
        var timeDifference = estimatedArrivalTime - now;

        // ETA should be between 5 minutes and 2 hours from now
        return Task.FromResult(timeDifference.TotalMinutes >= 5 && timeDifference.TotalMinutes <= 120);
    }
    public Task<List<ETAEstimationDto>> GetActiveETAEstimationsAsync()
    {
        var activeETAs = _mockETAEstimations.Where(e => e.IsActive).ToList();
        return Task.FromResult(activeETAs);
    }
    public Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for calculating distance between two points
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Task.FromResult(R * c);
    }
    public Task<int> CalculateEstimatedMinutesAsync(double distanceKm, double? averageSpeed = null)
    {
        var speed = averageSpeed ?? 25.0; // Default speed 25 km/h
        var minutes = (int)(distanceKm / speed * 60);
        return Task.FromResult(Math.Max(minutes, 5)); // Minimum 5 minutes
    }
    private double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
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
}
