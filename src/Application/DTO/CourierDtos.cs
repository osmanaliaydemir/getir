using Getir.Application.Common;

namespace Getir.Application.DTO;

// Existing DTOs (from previous implementation)
public record CourierOrderResponse(
    Guid Id,
    string OrderNumber,
    Guid MerchantId,
    string MerchantName,
    string Status,
    decimal Total,
    string DeliveryAddress,
    DateTime? EstimatedDeliveryTime,
    DateTime CreatedAt);

public record CourierLocationUpdateRequest(
    decimal Latitude,
    decimal Longitude);

public record SetAvailabilityRequest(
    bool IsAvailable);

// Courier Panel DTOs
public record CourierDashboardResponse(
    CourierStatsResponse Stats,
    List<CourierOrderResponse> ActiveOrders,
    List<CourierOrderResponse> RecentDeliveries,
    CourierEarningsResponse Earnings);

public record CourierStatsResponse(
    int TotalDeliveries,
    int ActiveOrders,
    int CompletedToday,
    decimal? CurrentRating,
    bool IsAvailable,
    DateTime? LastDeliveryTime);

public record CourierEarningsResponse(
    decimal TodayEarnings,
    decimal ThisWeekEarnings,
    decimal ThisMonthEarnings,
    decimal TotalEarnings,
    int TodayDeliveries,
    int ThisWeekDeliveries,
    int ThisMonthDeliveries);

public record UpdateCourierLocationRequest(
    decimal Latitude,
    decimal Longitude);

public record CourierLocationResponse(
    Guid CourierId,
    decimal Latitude,
    decimal Longitude,
    DateTime LastUpdate,
    bool IsAvailable);

public record AcceptOrderRequest(
    Guid OrderId);

public record StartDeliveryRequest(
    Guid OrderId);

public record CompleteDeliveryRequest(
    Guid OrderId,
    string? DeliveryNotes = null);

public record CourierPerformanceResponse(
    Guid CourierId,
    string CourierName,
    int TotalDeliveries,
    decimal? Rating,
    decimal TotalEarnings,
    int CompletedThisWeek,
    int CompletedThisMonth,
    DateTime? LastDeliveryTime);

// Order Assignment DTOs
public record AssignOrderRequest(
    Guid OrderId,
    Guid? PreferredCourierId = null);

public record CourierAssignmentResponse(
    Guid CourierId,
    string CourierName,
    decimal Distance,
    int EstimatedMinutes,
    bool IsAssigned);

public record FindNearestCouriersRequest(
    decimal Latitude,
    decimal Longitude,
    int MaxDistanceKm = 10,
    int MaxCouriers = 5);

public record FindNearestCouriersResponse(
    List<CourierAssignmentResponse> AvailableCouriers,
    bool HasAvailableCouriers);

// Earnings Calculation DTOs
public record CourierEarningsQuery(
    Guid CourierId,
    DateTime? StartDate = null,
    DateTime? EndDate = null);

public record CourierEarningsDetailResponse(
    Guid CourierId,
    decimal BaseEarnings,
    decimal BonusEarnings,
    decimal TotalEarnings,
    int DeliveryCount,
    List<CourierEarningsItemResponse> EarningsBreakdown);

public record CourierEarningsItemResponse(
    DateTime Date,
    int DeliveryCount,
    decimal BaseEarnings,
    decimal BonusEarnings,
    decimal TotalEarnings);