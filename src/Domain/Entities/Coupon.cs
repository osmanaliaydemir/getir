namespace Getir.Domain.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = default!; // Percentage, FixedAmount
    public decimal DiscountValue { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public virtual ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}
