namespace Getir.Domain.Entities;

public class ProductOption
{
    public Guid Id { get; set; }
    public Guid ProductOptionGroupId { get; set; }
    public string Name { get; set; } = string.Empty; // "Küçük", "Ekstra Peynir"
    public string Description { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; } = 0; // Ekstra ücret
    public bool IsDefault { get; set; } = false; // Varsayılan seçenek mi?
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0; // Görüntüleme sırası
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ProductOptionGroup ProductOptionGroup { get; set; } = default!;
}
