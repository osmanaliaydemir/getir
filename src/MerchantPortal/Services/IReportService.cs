using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public interface IReportService
{
    /// <summary>
    /// Sales dashboard verilerini getirir
    /// </summary>
    Task<SalesDashboardModel> GetSalesDashboardAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Revenue analytics verilerini getirir
    /// </summary>
    Task<RevenueAnalyticsModel> GetRevenueAnalyticsAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Customer analytics verilerini getirir
    /// </summary>
    Task<CustomerAnalyticsModel> GetCustomerAnalyticsAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Product performance verilerini getirir
    /// </summary>
    Task<ProductPerformanceModel> GetProductPerformanceAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Chart verilerini getirir
    /// </summary>
    Task<ChartDataModel> GetChartDataAsync(Guid merchantId, string chartType, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Raporu PDF olarak export eder
    /// </summary>
    Task<byte[]> ExportReportToPdfAsync(Guid merchantId, ReportExportRequest request);

    /// <summary>
    /// Raporu Excel olarak export eder
    /// </summary>
    Task<byte[]> ExportReportToExcelAsync(Guid merchantId, ReportExportRequest request);
}
