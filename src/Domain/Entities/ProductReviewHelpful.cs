namespace Getir.Domain.Entities;

/// <summary>
/// Product review helpful votes - kullanıcıların yorumları faydalı/faydasız olarak işaretlemesi
/// </summary>
public class ProductReviewHelpful
{
    public Guid Id { get; set; }
    public Guid ProductReviewId { get; set; }
    public Guid UserId { get; set; }
    public bool IsHelpful { get; set; } // true = helpful, false = not helpful
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ProductReview ProductReview { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

