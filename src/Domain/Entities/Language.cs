using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class Language
{
    public Guid Id { get; set; }
    public LanguageCode Code { get; set; }
    public string Name { get; set; } = default!;
    public string NativeName { get; set; } = default!;
    public string CultureCode { get; set; } = default!;
    public bool IsRtl { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public string? FlagIcon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
    public virtual ICollection<UserLanguagePreference> UserLanguagePreferences { get; set; } = new List<UserLanguagePreference>();
}