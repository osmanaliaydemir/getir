using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Payments;

/// <summary>
/// Ölçeklenebilir Payment Service - tüm ödeme yöntemlerini destekler
/// Şu anda sadece Cash payment implementasyonu var
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Ödeme oluştur (tüm ödeme yöntemleri için)
    /// </summary>
    Task<Result<PaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ödeme durumunu güncelle
    /// </summary>
    Task<Result> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatusUpdateRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ödeme detaylarını getir
    /// </summary>
    Task<Result<PaymentResponse>> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sipariş ödemelerini getir
    /// </summary>
    Task<Result<PagedResult<PaymentResponse>>> GetOrderPaymentsAsync(Guid orderId, PaginationQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ödeme iptal et
    /// </summary>
    Task<Result> CancelPaymentAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ödeme iade et
    /// </summary>
    Task<Result> RefundPaymentAsync(Guid paymentId, decimal amount, string reason, CancellationToken cancellationToken = default);
    
    // === CASH PAYMENT SPECIFIC METHODS ===
    
    /// <summary>
    /// Kurye için bekleyen nakit ödemeleri getir
    /// </summary>
    Task<Result<PagedResult<PaymentResponse>>> GetPendingCashPaymentsAsync(Guid courierId, PaginationQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Nakit ödeme toplandı olarak işaretle
    /// </summary>
    Task<Result> MarkCashPaymentAsCollectedAsync(Guid paymentId, Guid courierId, CollectCashPaymentRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Nakit ödeme başarısız olarak işaretle
    /// </summary>
    Task<Result> MarkCashPaymentAsFailedAsync(Guid paymentId, Guid courierId, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Kurye günlük nakit toplama özeti
    /// </summary>
    Task<Result<CourierCashSummaryResponse>> GetCourierCashSummaryAsync(Guid courierId, DateTime? date = null, CancellationToken cancellationToken = default);
    
    // === MERCHANT METHODS ===
    
    /// <summary>
    /// Merchant nakit ödeme özeti
    /// </summary>
    Task<Result<MerchantCashSummaryResponse>> GetMerchantCashSummaryAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Merchant settlement geçmişi
    /// </summary>
    Task<Result<PagedResult<SettlementResponse>>> GetMerchantSettlementsAsync(Guid merchantId, PaginationQuery query, CancellationToken cancellationToken = default);
    
    // === ADMIN METHODS ===
    
    /// <summary>
    /// Tüm nakit ödemeleri getir (admin)
    /// </summary>
    Task<Result<PagedResult<PaymentResponse>>> GetAllCashPaymentsAsync(PaginationQuery query, string? status = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Settlement işlemi yap (admin)
    /// </summary>
    Task<Result> ProcessSettlementAsync(Guid merchantId, ProcessSettlementRequest request, Guid adminId, CancellationToken cancellationToken = default);
}
