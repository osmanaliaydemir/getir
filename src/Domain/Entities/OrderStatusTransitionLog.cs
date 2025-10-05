using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Audit log for order status transitions
/// </summary>
public class OrderStatusTransitionLog
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByRole { get; set; } = default!;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsRollback { get; set; } = false;
    public Guid? RollbackFromLogId { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = default!;
    public virtual User ChangedByUser { get; set; } = default!;
    public virtual OrderStatusTransitionLog? RollbackFromLog { get; set; }
}
