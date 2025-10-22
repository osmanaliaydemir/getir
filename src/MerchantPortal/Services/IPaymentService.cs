using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IPaymentService
{
    /// <summary>
    /// Gets payment history with filtering
    /// </summary>
    Task<List<PaymentListItemModel>> GetPaymentHistoryAsync(Guid merchantId, PaymentFilterModel filter);

    /// <summary>
    /// Gets payment details by ID
    /// </summary>
    Task<PaymentResponse?> GetPaymentByIdAsync(Guid paymentId);

    /// <summary>
    /// Gets settlement report for date range
    /// </summary>
    Task<SettlementReportModel> GetSettlementReportAsync(Guid merchantId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets revenue analytics
    /// </summary>
    Task<RevenueAnalyticsModel> GetRevenueAnalyticsAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets payment method breakdown
    /// </summary>
    Task<List<PaymentMethodBreakdownModel>> GetPaymentMethodBreakdownAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Exports payments to Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync(Guid merchantId, PaymentExportRequest request);

    /// <summary>
    /// Exports payments to PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync(Guid merchantId, PaymentExportRequest request);

    /// <summary>
    /// Gets merchant settlements from API
    /// </summary>
    Task<List<SettlementResponse>> GetMerchantSettlementsAsync(Guid merchantId, int page = 1, int pageSize = 50);
}
