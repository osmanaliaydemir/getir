namespace Getir.Domain.Entities;

public abstract class TranslatableEntity
{
    public Guid Id { get; set; }
    public string DefaultLanguageKey { get; set; } = default!; // Key for default language content
    public bool IsTranslatable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
