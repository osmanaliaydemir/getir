namespace Getir.Domain.Entities;

/// <summary>
/// Review bookmark entity for tracking user bookmarks on reviews
/// </summary>
public class ReviewBookmark
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public Guid UserId { get; set; }
    public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Review Review { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
