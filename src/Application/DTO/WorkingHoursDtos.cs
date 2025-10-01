namespace Getir.Application.DTO;

public record CreateWorkingHoursRequest(
    Guid MerchantId,
    DayOfWeek DayOfWeek,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    bool IsClosed);

public record UpdateWorkingHoursRequest(
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    bool IsClosed);

public record WorkingHoursResponse(
    Guid Id,
    Guid MerchantId,
    DayOfWeek DayOfWeek,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    bool IsClosed,
    DateTime CreatedAt);

public record BulkUpdateWorkingHoursRequest(
    List<WorkingHoursDayRequest> WorkingHours);

public record WorkingHoursDayRequest(
    DayOfWeek DayOfWeek,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    bool IsClosed);
