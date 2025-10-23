namespace Getir.Domain.Enums;

/// <summary>
/// Kurye mevcutluk durumları - Gerçek zamanlı takip için
/// </summary>
public enum CourierAvailabilityStatus
{
    /// <summary>
    /// Kurye yeni teslimatlar için mevcut
    /// </summary>
    Available = 0,

    /// <summary>
    /// Kurye şu anda bir teslimatta
    /// </summary>
    Busy = 1,

    /// <summary>
    /// Kurye çevrimdışı/çalışmıyor
    /// </summary>
    Offline = 2,

    /// <summary>
    /// Kurye mola veriyor
    /// </summary>
    OnBreak = 3,

    /// <summary>
    /// Kurye mevcut değil (geçici)
    /// </summary>
    Unavailable = 4
}

