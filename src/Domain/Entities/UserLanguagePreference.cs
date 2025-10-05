namespace Getir.Domain.Entities;

public class UserLanguagePreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LanguageId { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = default!;
    public virtual Language Language { get; set; } = default!;
}