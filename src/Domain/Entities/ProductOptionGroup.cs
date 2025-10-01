namespace Getir.Domain.Entities;

public class ProductOptionGroup
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty; // "Boyut", "Ekstra Malzemeler"
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false; // Zorunlu seçenek mi?
    public int MinSelection { get; set; } = 0; // Minimum seçim sayısı
    public int MaxSelection { get; set; } = 1; // Maksimum seçim sayısı
    public int DisplayOrder { get; set; } = 0; // Görüntüleme sırası
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = default!;
    public virtual ICollection<ProductOption> Options { get; set; } = new List<ProductOption>();
}
