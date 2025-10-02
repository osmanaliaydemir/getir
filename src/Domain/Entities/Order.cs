using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? CourierId { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = default!;
    public string PaymentStatus { get; set; } = default!;
    public string DeliveryAddress { get; set; } = default!;
    public decimal DeliveryLatitude { get; set; }
    public decimal DeliveryLongitude { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = default!;
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual Courier? Courier { get; set; }
    public virtual Coupon? Coupon { get; set; }
    public virtual ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
}
