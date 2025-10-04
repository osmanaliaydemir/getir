using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Review report entity for reporting inappropriate reviews
/// </summary>
public class ReviewReport
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid ReviewId { get; set; }
    
    [Required]
    public Guid ReporterId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Reason { get; set; } = string.Empty; // "Inappropriate", "Spam", "Fake", "Offensive"
    
    [MaxLength(500)]
    public string? Details { get; set; }
    
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // "Pending", "Reviewed", "Resolved", "Dismissed"
    
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    [MaxLength(500)]
    public string? ReviewNotes { get; set; }
    
    // Navigation properties
    public virtual Review Review { get; set; } = null!;
    public virtual User Reporter { get; set; } = null!;
    public virtual User? Reviewer { get; set; }
}
