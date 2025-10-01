namespace Getir.Domain.Entities;

public class OrderLineOption
{
    public Guid Id { get; set; }
    public Guid OrderLineId { get; set; }
    public Guid ProductOptionId { get; set; }
    public string OptionName { get; set; } = string.Empty; // Snapshot of option name
    public decimal ExtraPrice { get; set; } = 0; // Snapshot of extra price
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual OrderLine OrderLine { get; set; } = default!;
    public virtual ProductOption ProductOption { get; set; } = default!;
}
