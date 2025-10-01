namespace Getir.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = default!;
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual Product Product { get; set; } = default!;
}
