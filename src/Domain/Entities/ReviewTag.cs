using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class ReviewTag
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public string Tag { get; set; } = default!;
    public bool IsPositive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Review Review { get; set; } = default!;
}
