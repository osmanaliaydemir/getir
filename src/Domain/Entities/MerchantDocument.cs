using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

public class MerchantDocument
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public Guid UploadedBy { get; set; } // User ID who uploaded
    
    public string DocumentType { get; set; } = default!; // TaxCertificate, BusinessLicense, IdentityCard, etc.
    public string DocumentName { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string FileUrl { get; set; } = default!;
    public string MimeType { get; set; } = default!;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = default!;
    
    public string? Description { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRequired { get; set; } = true;
    
    // Verification
    public bool IsVerified { get; set; } = false;
    public bool IsApproved { get; set; } = false;
    public string? VerificationNotes { get; set; }
    public Guid? VerifiedBy { get; set; } // Admin ID
    public DateTime? VerifiedAt { get; set; }
    
    // Status
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public string? RejectionReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Merchant Merchant { get; set; } = default!;
    public virtual User UploadedByUser { get; set; } = default!;
    public virtual User? VerifiedByUser { get; set; }
}
