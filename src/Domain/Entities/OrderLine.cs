namespace Getir.Domain.Entities;

public class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = default!;
    public virtual Product Product { get; set; } = default!;
    public virtual ICollection<OrderLineOption> Options { get; set; } = new List<OrderLineOption>();
}
