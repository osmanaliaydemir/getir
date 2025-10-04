namespace Getir.Domain.Entities;

/// <summary>
/// Review like entity for tracking user likes on reviews
/// </summary>
public class ReviewLike
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Review Review { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
