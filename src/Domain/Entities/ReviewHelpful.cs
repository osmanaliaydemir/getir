using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class ReviewHelpful
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public bool IsHelpful { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Review Review { get; set; } = default!;
    public virtual User User { get; set; } = default!;
}
