using Getir.Domain.Enums;

namespace Getir.Domain.Entities;

/// <summary>
/// Nakit ödeme kanıtları - güvenlik için
/// </summary>
public class CashPaymentEvidence
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid CourierId { get; set; }
    
    /// <summary>
    /// Kanıt türü (Photo, Signature, Video, Audio)
    /// </summary>
    public EvidenceType EvidenceType { get; set; }
    
    /// <summary>
    /// Dosya URL'i (fotoğraf, video, ses kaydı)
    /// </summary>
    public string FileUrl { get; set; } = default!;
    
    /// <summary>
    /// Dosya boyutu (bytes)
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Dosya türü (image/jpeg, video/mp4, audio/mp3)
    /// </summary>
    public string MimeType { get; set; } = default!;
    
    /// <summary>
    /// Dosya hash'i (bütünlük kontrolü için)
    /// </summary>
    public string FileHash { get; set; } = default!;
    
    /// <summary>
    /// Kanıt açıklaması
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// GPS koordinatları (fotoğraf çekim yeri)
    /// </summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    /// <summary>
    /// Cihaz bilgileri (IMEI, model, OS)
    /// </summary>
    public string? DeviceInfo { get; set; }
    
    /// <summary>
    /// Kanıt durumu (Pending, Verified, Rejected)
    /// </summary>
    public EvidenceStatus Status { get; set; } = EvidenceStatus.Pending;
    
    /// <summary>
    /// Doğrulama notları (admin tarafından)
    /// </summary>
    public string? VerificationNotes { get; set; }
    
    /// <summary>
    /// Doğrulayan admin ID
    /// </summary>
    public Guid? VerifiedByAdminId { get; set; }
    
    /// <summary>
    /// Doğrulama tarihi
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Payment Payment { get; set; } = default!;
    public virtual Courier Courier { get; set; } = default!;
    public virtual User? VerifiedByAdmin { get; set; }
}

