namespace Getir.Domain.Enums;

public enum MerchantOnboardingStatus
{
    NotStarted = 0,
    BasicInfoCompleted = 1,
    DocumentsUploaded = 2,
    PaymentInfoCompleted = 3,
    BusinessInfoCompleted = 4,
    PendingApproval = 5,
    Approved = 6,
    Rejected = 7,
    Suspended = 8
}

public static class MerchantOnboardingStatusExtensions
{
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
