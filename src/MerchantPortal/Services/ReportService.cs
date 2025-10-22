using Getir.MerchantPortal.Models;
using System.Text;

namespace Getir.MerchantPortal.Services;

public class ReportService : IReportService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IApiClient apiClient, ILogger<ReportService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<SalesDashboardModel> GetSalesDashboardAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            // Get orders data
            var ordersResponse = await _apiClient.GetAsync<ApiResponse<PagedResult<OrderResponse>>>(
                $"api/v1/orders/merchant/{merchantId}?Page=1&PageSize=1000&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            var orders = ordersResponse?.Data?.Items ?? new List<OrderResponse>();

            // Get payments data
            var paymentsResponse = await _apiClient.GetAsync<ApiResponse<PagedResult<PaymentResponse>>>(
                $"api/v1/payment/merchant/{merchantId}/transactions?Page=1&PageSize=1000&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            var payments = paymentsResponse?.Data?.Items ?? new List<PaymentResponse>();

            var completedOrders = orders.Where(o => o.Status == "Completed").ToList();
            var completedPayments = payments.Where(p => p.Status == "Completed").ToList();

            return new SalesDashboardModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = completedPayments.Sum(p => p.Amount),
                TotalOrders = orders.Count,
                CompletedOrders = completedOrders.Count,
                AverageOrderValue = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0,
                RevenueGrowth = CalculateGrowthRate(completedPayments, start, end),
                OrderGrowth = CalculateOrderGrowth(completedOrders, start, end),
                TopProducts = GetTopProducts(completedOrders),
                RevenueByDay = GetRevenueByDay(completedPayments, start, end),
                OrdersByDay = GetOrdersByDay(completedOrders, start, end),
                PaymentMethodBreakdown = GetPaymentMethodBreakdown(completedPayments),
                CategoryBreakdown = GetCategoryBreakdown(completedOrders)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales dashboard for merchant {MerchantId}", merchantId);
            return CreateEmptySalesDashboard(startDate, endDate);
        }
    }

    public async Task<RevenueAnalyticsModel> GetRevenueAnalyticsAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var now = DateTime.Now;
            var start = startDate ?? now.AddDays(-30);
            var end = endDate ?? now;

            var paymentsResponse = await _apiClient.GetAsync<ApiResponse<PagedResult<PaymentResponse>>>(
                $"api/v1/payment/merchant/{merchantId}/transactions?Page=1&PageSize=1000&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            var payments = paymentsResponse?.Data?.Items ?? new List<PaymentResponse>();
            var completedPayments = payments.Where(p => p.Status == "Completed").ToList();

            return new RevenueAnalyticsModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = completedPayments.Sum(p => p.Amount),
                DailyRevenue = GetDailyRevenue(completedPayments, start, end),
                WeeklyRevenue = GetWeeklyRevenue(completedPayments, start, end),
                MonthlyRevenue = GetMonthlyRevenue(completedPayments, start, end),
                RevenueTrend = CalculateRevenueTrend(completedPayments, start, end),
                PaymentMethodDistribution = GetPaymentMethodDistribution(completedPayments),
                RevenueByHour = GetRevenueByHour(completedPayments),
                TopRevenueDays = GetTopRevenueDays(completedPayments, start, end)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics for merchant {MerchantId}", merchantId);
            return CreateEmptyRevenueAnalytics(startDate, endDate);
        }
    }

    public async Task<CustomerAnalyticsModel> GetCustomerAnalyticsAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var ordersResponse = await _apiClient.GetAsync<ApiResponse<PagedResult<OrderResponse>>>(
                $"api/v1/orders/merchant/{merchantId}?Page=1&PageSize=1000&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            var orders = ordersResponse?.Data?.Items ?? new List<OrderResponse>();

            return new CustomerAnalyticsModel
            {
                StartDate = start,
                EndDate = end,
                TotalCustomers = orders.Select(o => o.UserId).Distinct().Count(),
                NewCustomers = GetNewCustomers(orders, start, end),
                ReturningCustomers = GetReturningCustomers(orders, start, end),
                CustomerRetentionRate = CalculateRetentionRate(orders, start, end),
                AverageOrderFrequency = CalculateOrderFrequency(orders),
                CustomerLifetimeValue = CalculateCustomerLifetimeValue(orders),
                TopCustomers = GetTopCustomers(orders),
                CustomerSegments = GetCustomerSegments(orders)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer analytics for merchant {MerchantId}", merchantId);
            return CreateEmptyCustomerAnalytics(startDate, endDate);
        }
    }

    public async Task<ProductPerformanceModel> GetProductPerformanceAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var ordersResponse = await _apiClient.GetAsync<ApiResponse<PagedResult<OrderResponse>>>(
                $"api/v1/orders/merchant/{merchantId}?Page=1&PageSize=1000&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            var orders = ordersResponse?.Data?.Items ?? new List<OrderResponse>();

            return new ProductPerformanceModel
            {
                StartDate = start,
                EndDate = end,
                TotalProducts = GetTotalProducts(orders),
                BestSellers = GetBestSellers(orders),
                LowPerformers = GetLowPerformers(orders),
                CategoryPerformance = GetCategoryPerformance(orders),
                ProductTrends = GetProductTrends(orders, start, end),
                InventoryTurnover = GetInventoryTurnover(orders),
                ProfitMargins = GetProfitMargins(orders)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product performance for merchant {MerchantId}", merchantId);
            return CreateEmptyProductPerformance(startDate, endDate);
        }
    }

    public async Task<ChartDataModel> GetChartDataAsync(Guid merchantId, string chartType, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            return chartType.ToLower() switch
            {
                "revenue" => await GetRevenueChartData(merchantId, start, end),
                "orders" => await GetOrdersChartData(merchantId, start, end),
                "customers" => await GetCustomersChartData(merchantId, start, end),
                "products" => await GetProductsChartData(merchantId, start, end),
                "paymentmethods" => await GetPaymentMethodsChartData(merchantId, start, end),
                "categories" => await GetCategoriesChartData(merchantId, start, end),
                _ => new ChartDataModel { ChartType = chartType, Data = new List<object>() }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chart data for merchant {MerchantId}, chart type {ChartType}", merchantId, chartType);
            return new ChartDataModel { ChartType = chartType, Data = new List<object>() };
        }
    }

    public async Task<byte[]> ExportReportToPdfAsync(Guid merchantId, ReportExportRequest request)
    {
        try
        {
            // TODO: Implement PDF export with iTextSharp or similar library
            // For now, return Excel as fallback
            return await ExportReportToExcelAsync(merchantId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to PDF for merchant {MerchantId}", merchantId);
            return Encoding.UTF8.GetBytes("Error generating PDF report");
        }
    }

    public async Task<byte[]> ExportReportToExcelAsync(Guid merchantId, ReportExportRequest request)
    {
        try
        {
            var data = await GetReportData(merchantId, request);
            
            var csv = new StringBuilder();
            csv.AppendLine("Report Type,Date,Value,Details");
            
            foreach (var item in data)
            {
                csv.AppendLine($"{item.ReportType},{item.Date:yyyy-MM-dd},{item.Value:F2},{item.Details}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report to Excel for merchant {MerchantId}", merchantId);
            return Encoding.UTF8.GetBytes("Error generating Excel report");
        }
    }

    #region Helper Methods

    private SalesDashboardModel CreateEmptySalesDashboard(DateTime? startDate, DateTime? endDate)
    {
        return new SalesDashboardModel
        {
            StartDate = startDate ?? DateTime.Now.AddDays(-30),
            EndDate = endDate ?? DateTime.Now,
            TotalRevenue = 0,
            TotalOrders = 0,
            CompletedOrders = 0,
            AverageOrderValue = 0,
            RevenueGrowth = 0,
            OrderGrowth = 0,
            TopProducts = new List<ProductPerformanceItem>(),
            RevenueByDay = new List<DailyData>(),
            OrdersByDay = new List<DailyData>(),
            PaymentMethodBreakdown = new List<BreakdownItem>(),
            CategoryBreakdown = new List<BreakdownItem>()
        };
    }

    private RevenueAnalyticsModel CreateEmptyRevenueAnalytics(DateTime? startDate, DateTime? endDate)
    {
        return new RevenueAnalyticsModel
        {
            StartDate = startDate ?? DateTime.Now.AddDays(-30),
            EndDate = endDate ?? DateTime.Now,
            TotalRevenue = 0,
            DailyRevenue = new List<DailyData>(),
            WeeklyRevenue = new List<WeeklyData>(),
            MonthlyRevenue = new List<MonthlyData>(),
            RevenueTrend = 0,
            PaymentMethodDistribution = new List<BreakdownItem>(),
            RevenueByHour = new List<HourlyData>(),
            TopRevenueDays = new List<DailyData>()
        };
    }

    private CustomerAnalyticsModel CreateEmptyCustomerAnalytics(DateTime? startDate, DateTime? endDate)
    {
        return new CustomerAnalyticsModel
        {
            StartDate = startDate ?? DateTime.Now.AddDays(-30),
            EndDate = endDate ?? DateTime.Now,
            TotalCustomers = 0,
            NewCustomers = 0,
            ReturningCustomers = 0,
            CustomerRetentionRate = 0,
            AverageOrderFrequency = 0,
            CustomerLifetimeValue = 0,
            TopCustomers = new List<CustomerItem>(),
            CustomerSegments = new List<CustomerSegment>()
        };
    }

    private ProductPerformanceModel CreateEmptyProductPerformance(DateTime? startDate, DateTime? endDate)
    {
        return new ProductPerformanceModel
        {
            StartDate = startDate ?? DateTime.Now.AddDays(-30),
            EndDate = endDate ?? DateTime.Now,
            TotalProducts = 0,
            BestSellers = new List<ProductPerformanceItem>(),
            LowPerformers = new List<ProductPerformanceItem>(),
            CategoryPerformance = new List<CategoryPerformance>(),
            ProductTrends = new List<ProductTrend>(),
            InventoryTurnover = new List<InventoryTurnover>(),
            ProfitMargins = new List<ProfitMargin>()
        };
    }

    // Additional helper methods would be implemented here...
    private decimal CalculateGrowthRate(List<PaymentResponse> payments, DateTime start, DateTime end)
    {
        // Implementation for growth rate calculation
        return 0;
    }

    private decimal CalculateOrderGrowth(List<OrderResponse> orders, DateTime start, DateTime end)
    {
        // Implementation for order growth calculation
        return 0;
    }

    private List<ProductPerformanceItem> GetTopProducts(List<OrderResponse> orders)
    {
        // Implementation for top products
        return new List<ProductPerformanceItem>();
    }

    private List<DailyData> GetRevenueByDay(List<PaymentResponse> payments, DateTime start, DateTime end)
    {
        // Implementation for revenue by day
        return new List<DailyData>();
    }

    private List<DailyData> GetOrdersByDay(List<OrderResponse> orders, DateTime start, DateTime end)
    {
        // Implementation for orders by day
        return new List<DailyData>();
    }

    private List<BreakdownItem> GetPaymentMethodBreakdown(List<PaymentResponse> payments)
    {
        // Implementation for payment method breakdown
        return new List<BreakdownItem>();
    }

    private List<BreakdownItem> GetCategoryBreakdown(List<OrderResponse> orders)
    {
        // Implementation for category breakdown
        return new List<BreakdownItem>();
    }

    private async Task<List<ReportDataItem>> GetReportData(Guid merchantId, ReportExportRequest request)
    {
        // Implementation for report data
        return new List<ReportDataItem>();
    }

    // Additional helper methods...
    private List<DailyData> GetDailyRevenue(List<PaymentResponse> payments, DateTime start, DateTime end) => new();
    private List<WeeklyData> GetWeeklyRevenue(List<PaymentResponse> payments, DateTime start, DateTime end) => new();
    private List<MonthlyData> GetMonthlyRevenue(List<PaymentResponse> payments, DateTime start, DateTime end) => new();
    private decimal CalculateRevenueTrend(List<PaymentResponse> payments, DateTime start, DateTime end) => 0;
    private List<BreakdownItem> GetPaymentMethodDistribution(List<PaymentResponse> payments) => new();
    private List<HourlyData> GetRevenueByHour(List<PaymentResponse> payments) => new();
    private List<DailyData> GetTopRevenueDays(List<PaymentResponse> payments, DateTime start, DateTime end) => new();
    private int GetNewCustomers(List<OrderResponse> orders, DateTime start, DateTime end) => 0;
    private int GetReturningCustomers(List<OrderResponse> orders, DateTime start, DateTime end) => 0;
    private decimal CalculateRetentionRate(List<OrderResponse> orders, DateTime start, DateTime end) => 0;
    private decimal CalculateOrderFrequency(List<OrderResponse> orders) => 0;
    private decimal CalculateCustomerLifetimeValue(List<OrderResponse> orders) => 0;
    private List<CustomerItem> GetTopCustomers(List<OrderResponse> orders) => new();
    private List<CustomerSegment> GetCustomerSegments(List<OrderResponse> orders) => new();
    private int GetTotalProducts(List<OrderResponse> orders) => 0;
    private List<ProductPerformanceItem> GetBestSellers(List<OrderResponse> orders) => new();
    private List<ProductPerformanceItem> GetLowPerformers(List<OrderResponse> orders) => new();
    private List<CategoryPerformance> GetCategoryPerformance(List<OrderResponse> orders) => new();
    private List<ProductTrend> GetProductTrends(List<OrderResponse> orders, DateTime start, DateTime end) => new();
    private List<InventoryTurnover> GetInventoryTurnover(List<OrderResponse> orders) => new();
    private List<ProfitMargin> GetProfitMargins(List<OrderResponse> orders) => new();

    private async Task<ChartDataModel> GetRevenueChartData(Guid merchantId, DateTime start, DateTime end) => new();
    private async Task<ChartDataModel> GetOrdersChartData(Guid merchantId, DateTime start, DateTime end) => new();
    private async Task<ChartDataModel> GetCustomersChartData(Guid merchantId, DateTime start, DateTime end) => new();
    private async Task<ChartDataModel> GetProductsChartData(Guid merchantId, DateTime start, DateTime end) => new();
    private async Task<ChartDataModel> GetPaymentMethodsChartData(Guid merchantId, DateTime start, DateTime end) => new();
    private async Task<ChartDataModel> GetCategoriesChartData(Guid merchantId, DateTime start, DateTime end) => new();

    #endregion
}
