namespace Getir.Domain.Entities;

/// <summary>
/// Kullanıcının favori ürünleri
/// </summary>
public class FavoriteProduct
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

