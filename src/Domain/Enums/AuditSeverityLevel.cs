namespace Getir.Domain.Enums;

/// <summary>
/// Audit log severity seviyeleri
/// </summary>
public enum AuditSeverityLevel
{
    /// <summary>
    /// Bilgi (Information)
    /// </summary>
    Information = 1,
    
    /// <summary>
    /// Uyarı (Warning)
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// Hata (Error)
    /// </summary>
    Error = 3,
    
    /// <summary>
    /// Kritik (Critical)
    /// </summary>
    Critical = 4,
    
    /// <summary>
    /// Güvenlik (Security)
    /// </summary>
    Security = 5
}
