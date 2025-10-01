namespace Getir.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? MerchantId { get; set; }
    public string DiscountType { get; set; } = default!;
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public virtual Merchant? Merchant { get; set; }
}
