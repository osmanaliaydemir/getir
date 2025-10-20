using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Product review entity - kullanıcıların ürünler hakkında yorum ve puan vermesi için
/// </summary>
public class ProductReview
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ProductId { get; set; } // Reviewed product
    
    [Required]
    public Guid UserId { get; set; } // User who wrote the review
    
    [Required]
    public Guid OrderId { get; set; } // Order this review is for (sadece satın alan kullanıcılar yorum yapabilir)
    
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; } // 1-5 stars
    
    [MaxLength(1000)]
    public string? Comment { get; set; } // Yorum (opsiyonel)
    
    // Review images (kullanıcılar ürün fotoğrafı ekleyebilir)
    public string? ImageUrls { get; set; } // JSON array of image URLs
    
    // Verified purchase
    public bool IsVerifiedPurchase { get; set; } = true; // OrderId kontrolü ile otomatik set edilir
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Moderation fields
    public bool IsApproved { get; set; } = true; // Auto-approve by default
    public bool IsModerated { get; set; } = false;
    public string? ModerationNotes { get; set; }
    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Helpful votes (kaç kişi bu yorumu faydalı buldu)
    public int HelpfulCount { get; set; } = 0;
    public int NotHelpfulCount { get; set; } = 0;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual User? Moderator { get; set; }
    
    // Helpful votes collection
    public virtual ICollection<ProductReviewHelpful> ProductReviewHelpfuls { get; set; } = new List<ProductReviewHelpful>();
}

