using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Hizmet Kategorileri (Market, Yemek, Su, Eczane, vb.)
/// Getir'in ana hizmet tipleri
/// </summary>
public class ServiceCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public ServiceCategoryType Type { get; set; } = ServiceCategoryType.Restaurant;
    public string? ImageUrl { get; set; }
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Merchant> Merchants { get; set; } = new List<Merchant>();
}

