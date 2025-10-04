using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Review moderation log entity for tracking review moderation activities
/// </summary>
public class ReviewModerationLog
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ReviewId { get; set; }
    
    [Required]
    public Guid ModeratorId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // "approve", "reject", "flag"
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime ModeratedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Review Review { get; set; } = null!;
    public virtual User Moderator { get; set; } = null!;
}
