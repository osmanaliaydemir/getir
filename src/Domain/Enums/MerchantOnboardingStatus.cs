namespace Getir.Domain.Enums;

/// <summary>
/// Merchant onboarding statusları
/// </summary>
public enum MerchantOnboardingStatus
{
    /// <summary>
    /// Başlangıçta
    /// </summary>
    NotStarted = 0,
    /// <summary>
    /// Temel bilgiler tamamlandı
    /// </summary>
    BasicInfoCompleted = 1,
    /// <summary>
    /// Belgeler yüklendi
    /// </summary>
    DocumentsUploaded = 2,
    /// <summary>
    /// Ödeme bilgileri tamamlandı
    /// </summary>
    PaymentInfoCompleted = 3,
    /// <summary>
    /// İş bilgileri tamamlandı
    /// </summary>
    BusinessInfoCompleted = 4,
    /// <summary>
    /// Onay bekliyor
    /// </summary>
    PendingApproval = 5,
    /// <summary>
    /// Onaylandı
    /// </summary>
    Approved = 6,
    /// <summary>
    /// Reddedildi
    /// </summary>
    Rejected = 7,
    /// <summary>
    /// Askıya alındı
    /// </summary>
    Suspended = 8
}
/// <summary>
/// Merchant onboarding statusları uzantıları
/// </summary>
public static class MerchantOnboardingStatusExtensions
{
    /// <summary>
    /// Merchant onboarding statusunun string değerini döndürür
    /// </summary>
    /// <param name="status">Merchant onboarding statusu</param>
    /// <returns>Merchant onboarding statusunun string değeri</returns>
    public static string ToStringValue(this MerchantOnboardingStatus status)
    {
        return status switch
        {
            MerchantOnboardingStatus.NotStarted => "NotStarted",
            MerchantOnboardingStatus.BasicInfoCompleted => "BasicInfoCompleted",
            MerchantOnboardingStatus.DocumentsUploaded => "DocumentsUploaded",
            MerchantOnboardingStatus.PaymentInfoCompleted => "PaymentInfoCompleted",
            MerchantOnboardingStatus.BusinessInfoCompleted => "BusinessInfoCompleted",
            MerchantOnboardingStatus.PendingApproval => "PendingApproval",
            MerchantOnboardingStatus.Approved => "Approved",
            MerchantOnboardingStatus.Rejected => "Rejected",
            MerchantOnboardingStatus.Suspended => "Suspended",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    /// <summary>
    /// Merchant onboarding statusunun string değerinden döndürür
    /// </summary>
    /// <param name="status">Merchant onboarding statusunun string değeri</param>
    /// <returns>Merchant onboarding statusu</returns>
    public static MerchantOnboardingStatus FromString(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "notstarted" => MerchantOnboardingStatus.NotStarted,
            "basicinfocompleted" => MerchantOnboardingStatus.BasicInfoCompleted,
            "documentsuploaded" => MerchantOnboardingStatus.DocumentsUploaded,
            "paymentinfocompleted" => MerchantOnboardingStatus.PaymentInfoCompleted,
            "businessinfocompleted" => MerchantOnboardingStatus.BusinessInfoCompleted,
            "pendingapproval" => MerchantOnboardingStatus.PendingApproval,
            "approved" => MerchantOnboardingStatus.Approved,
            "rejected" => MerchantOnboardingStatus.Rejected,
            "suspended" => MerchantOnboardingStatus.Suspended,
            _ => throw new ArgumentException($"Invalid merchant onboarding status: {status}", nameof(status))
        };
    }

    /// <summary>
    /// Merchant onboarding statusunun bir sonraki statusa geçebilip geçemeyeceğini döndürür
    /// </summary>
    /// <param name="from">Önceki merchant onboarding statusu</param>
    /// <param name="to">Sonraki merchant onboarding statusu</param>
    /// <returns>Merchant onboarding statusunun bir sonraki statusa geçebilip geçemeyeceği</returns>
    public static bool CanTransitionTo(this MerchantOnboardingStatus from, MerchantOnboardingStatus to)
    {
        return (from, to) switch
        {
            (MerchantOnboardingStatus.NotStarted, MerchantOnboardingStatus.BasicInfoCompleted) => true,
            (MerchantOnboardingStatus.BasicInfoCompleted, MerchantOnboardingStatus.DocumentsUploaded) => true,
            (MerchantOnboardingStatus.DocumentsUploaded, MerchantOnboardingStatus.PaymentInfoCompleted) => true,
            (MerchantOnboardingStatus.PaymentInfoCompleted, MerchantOnboardingStatus.BusinessInfoCompleted) => true,
            (MerchantOnboardingStatus.BusinessInfoCompleted, MerchantOnboardingStatus.PendingApproval) => true,
            (MerchantOnboardingStatus.PendingApproval, MerchantOnboardingStatus.Approved) => true,
            (MerchantOnboardingStatus.PendingApproval, MerchantOnboardingStatus.Rejected) => true,
            (MerchantOnboardingStatus.Approved, MerchantOnboardingStatus.Suspended) => true,
            (MerchantOnboardingStatus.Rejected, MerchantOnboardingStatus.NotStarted) => true,
            (MerchantOnboardingStatus.Suspended, MerchantOnboardingStatus.Approved) => true,
            _ => false
        };
    }
}
