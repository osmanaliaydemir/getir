using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class DeviceToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public string DeviceType { get; set; } = default!; // iOS, Android, Web
    public string DeviceId { get; set; } = default!;
    public string? DeviceName { get; set; }
    public string? AppVersion { get; set; }
    public string? OsVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = default!;
}
