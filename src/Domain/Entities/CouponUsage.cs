namespace Getir.Domain.Entities;

public class CouponUsage
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; }

    // Navigation
    public virtual Coupon Coupon { get; set; } = default!;
    public virtual User User { get; set; } = default!;
    public virtual Order Order { get; set; } = default!;
}
