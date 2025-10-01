namespace Getir.Domain.Entities;

public class MerchantOnboarding
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid OwnerId { get; set; }
    
    // Onboarding Steps
    public bool BasicInfoCompleted { get; set; }
    public bool BusinessInfoCompleted { get; set; }
    public bool WorkingHoursCompleted { get; set; }
    public bool DeliveryZonesCompleted { get; set; }
    public bool ProductsAdded { get; set; }
    public bool DocumentsUploaded { get; set; }
    
    // Verification
    public bool IsVerified { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Progress
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public decimal ProgressPercentage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual User Owner { get; set; } = default!;
}
