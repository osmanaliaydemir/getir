namespace Getir.Domain.Enums;

/// <summary>
/// Courier availability status for real-time tracking
/// </summary>
public enum CourierAvailabilityStatus
{
    /// <summary>
    /// Courier is available for new deliveries
    /// </summary>
    Available = 0,

    /// <summary>
    /// Courier is currently on a delivery
    /// </summary>
    Busy = 1,

    /// <summary>
    /// Courier is offline/not working
    /// </summary>
    Offline = 2,

    /// <summary>
    /// Courier is on break
    /// </summary>
    OnBreak = 3,

    /// <summary>
    /// Courier is unavailable (temporary)
    /// </summary>
    Unavailable = 4
}

