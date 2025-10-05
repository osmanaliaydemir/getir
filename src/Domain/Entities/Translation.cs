using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class Translation
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!; // Unique key for the translation
    public string Value { get; set; } = default!; // Translated text
    public LanguageCode LanguageCode { get; set; }
    public string? Category { get; set; } // UI, API, Email, SMS, etc.
    public string? Context { get; set; } // Additional context for the translation
    public string? Description { get; set; } // Description of what this translation is for
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Language Language { get; set; } = default!;
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
}
