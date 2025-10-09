namespace Getir.Application.DTO;

/// <summary>
/// Özel tatil günü yanıt modeli
/// </summary>
public record SpecialHolidayResponse(
    Guid Id,
    Guid MerchantId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsClosed,
    TimeSpan? SpecialOpenTime,
    TimeSpan? SpecialCloseTime,
    bool IsRecurring,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Özel tatil günü oluşturma isteği
/// </summary>
public record CreateSpecialHolidayRequest(
    Guid MerchantId,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsClosed,
    TimeSpan? SpecialOpenTime,
    TimeSpan? SpecialCloseTime,
    bool IsRecurring
);

/// <summary>
/// Özel tatil günü güncelleme isteği
/// </summary>
public record UpdateSpecialHolidayRequest(
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsClosed,
    TimeSpan? SpecialOpenTime,
    TimeSpan? SpecialCloseTime,
    bool IsRecurring,
    bool IsActive
);

/// <summary>
/// Belirli bir tarihte merchant'ın durumunu kontrol etme yanıtı
/// </summary>
public record MerchantAvailabilityResponse(
    bool IsOpen,
    string Status,
    SpecialHolidayResponse? SpecialHoliday,
    string? Message
);

