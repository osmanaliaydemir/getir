using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class SystemNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Type { get; set; } = default!; // System, Maintenance, Update, Announcement
    public string TargetRoles { get; set; } = default!; // Comma-separated roles
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High, 4=Critical
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public Guid? ReadBy { get; set; }

    // Navigation properties
    public virtual User? Creator { get; set; }
    public virtual User? ReadByUser { get; set; }
}
