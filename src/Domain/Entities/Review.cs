using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Review entity for merchants and couriers
/// </summary>
public class Review
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ReviewerId { get; set; } // User who wrote the review
    
    [Required]
    public Guid RevieweeId { get; set; } // Merchant or Courier being reviewed
    
    [Required]
    public string RevieweeType { get; set; } = string.Empty; // "Merchant" or "Courier"
    
    [Required]
    public Guid OrderId { get; set; } // Order this review is for
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; } // 1-5 stars
    
    [Required]
    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Moderation fields
    public bool IsApproved { get; set; } = true; // Auto-approve for now
    public bool IsModerated { get; set; } = false;
    public string? ModerationNotes { get; set; }
    public string? ModeratorNotes { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual User Reviewer { get; set; } = null!;
    public virtual User Reviewee { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual User? Moderator { get; set; }
    
    // Review tags/categories
    public virtual ICollection<ReviewTag> ReviewTags { get; set; } = new List<ReviewTag>();
    
    // Helpful votes
    public virtual ICollection<ReviewHelpful> ReviewHelpfuls { get; set; } = new List<ReviewHelpful>();
}