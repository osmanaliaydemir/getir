using Getir.Domain.Enums;

namespace Getir.Application.DTO;

// === REQUEST DTOs ===

public record CreatePaymentRequest(
    Guid OrderId,
    PaymentMethod PaymentMethod,
    decimal Amount,
    decimal? ChangeAmount = null,
    string? Notes = null
);

public record PaymentStatusUpdateRequest(
    PaymentStatus Status,
    string? Notes = null,
    string? FailureReason = null,
    string? ExternalTransactionId = null,
    string? ExternalResponse = null
);

public record CollectCashPaymentRequest(
    decimal CollectedAmount,
    string? Notes = null
);

public record ProcessSettlementRequest(
    decimal CommissionRate,
    string? Notes = null,
    string? BankTransferReference = null
);

// === RESPONSE DTOs ===

public record PaymentResponse(
    Guid Id,
    Guid OrderId,
    PaymentMethod PaymentMethod,
    PaymentStatus Status,
    decimal Amount,
    decimal? ChangeAmount,
    DateTime? ProcessedAt,
    DateTime? CompletedAt,
    DateTime? CollectedAt,
    DateTime? SettledAt,
    Guid? CollectedByCourierId,
    string? CollectedByCourierName,
    string? Notes,
    string? FailureReason,
    string? ExternalTransactionId,
    decimal? RefundAmount,
    DateTime? RefundedAt,
    string? RefundReason,
    DateTime CreatedAt
);

public record CourierCashSummaryResponse(
    Guid CourierId,
    string CourierName,
    DateTime Date,
    decimal TotalCollected,
    int TotalOrders,
    int SuccessfulCollections,
    int FailedCollections,
    List<PaymentResponse> Collections
);

public record MerchantCashSummaryResponse(
    Guid MerchantId,
    string MerchantName,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal TotalAmount,
    decimal TotalCommission,
    decimal NetAmount,
    int TotalOrders,
    List<PaymentResponse> Payments
);

public record SettlementResponse(
    Guid Id,
    Guid MerchantId,
    string MerchantName,
    decimal TotalAmount,
    decimal Commission,
    decimal NetAmount,
    DateTime SettlementDate,
    string Status,
    string? Notes,
    Guid? ProcessedByAdminId,
    string? ProcessedByAdminName,
    DateTime? CompletedAt,
    string? BankTransferReference,
    DateTime CreatedAt
);

// === ENUM EXTENSIONS ===

public static class PaymentMethodExtensions
{
    public static string GetDisplayName(this PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => "Kapıda Nakit",
            PaymentMethod.CreditCard => "Kredi Kartı",
            PaymentMethod.VodafonePay => "Vodafone Pay",
            PaymentMethod.BankTransfer => "Havale/EFT",
            PaymentMethod.BkmExpress => "BKM Express",
            PaymentMethod.Papara => "Papara",
            PaymentMethod.QrCode => "QR Code Ödeme",
            _ => "Bilinmeyen"
        };
    }
    
    public static bool IsCashPayment(this PaymentMethod method)
    {
        return method == PaymentMethod.Cash;
    }
    
    public static bool RequiresCourierCollection(this PaymentMethod method)
    {
        return method == PaymentMethod.Cash;
    }
}

public static class PaymentStatusExtensions
{
    public static string GetDisplayName(this PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => "Bekliyor",
            PaymentStatus.Completed => "Tamamlandı",
            PaymentStatus.Failed => "Başarısız",
            PaymentStatus.Cancelled => "İptal Edildi",
            PaymentStatus.Refunded => "İade Edildi",
            PaymentStatus.Processing => "İşleniyor",
            PaymentStatus.Expired => "Süresi Doldu",
            _ => "Bilinmeyen"
        };
    }
    
    public static bool IsFinalStatus(this PaymentStatus status)
    {
        return status is PaymentStatus.Completed or PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Expired;
    }
    
    public static bool IsSuccessfulStatus(this PaymentStatus status)
    {
        return status == PaymentStatus.Completed;
    }
}
