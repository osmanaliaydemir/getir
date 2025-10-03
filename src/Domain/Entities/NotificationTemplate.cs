using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Type { get; set; } = default!; // Order, Promotion, System, Payment
    public string? Category { get; set; }
    public string? Language { get; set; } = "tr-TR";
    public bool IsActive { get; set; } = true;
    public string? Variables { get; set; } // JSON string for template variables
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
}
