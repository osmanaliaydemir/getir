using Getir.MerchantPortal.Models;
using System.Text;

namespace Getir.MerchantPortal.Services;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(HttpClient httpClient, ILogger<PaymentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<PaymentListItemModel>> GetPaymentHistoryAsync(Guid merchantId, PaymentFilterModel filter)
    {
        try
        {
            // Build query string
            var queryParams = new List<string> { $"merchantId={merchantId}" };
            
            if (filter.StartDate.HasValue)
                queryParams.Add($"startDate={filter.StartDate:yyyy-MM-dd}");
            if (filter.EndDate.HasValue)
                queryParams.Add($"endDate={filter.EndDate:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(filter.PaymentMethod))
                queryParams.Add($"paymentMethod={filter.PaymentMethod}");
            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                queryParams.Add($"status={filter.PaymentStatus}");

            var query = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"/api/payments/merchant?{query}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get payment history: {StatusCode}", response.StatusCode);
                return new List<PaymentListItemModel>();
            }

            var payments = await response.Content.ReadFromJsonAsync<List<PaymentResponse>>() ?? new();
            
            // Map to list item model
            return payments.Select(p => new PaymentListItemModel
            {
                Id = p.Id,
                OrderId = p.OrderId,
                OrderNumber = $"ORD-{p.OrderId.ToString().Substring(0, 8)}",
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                Amount = p.Amount,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt,
                CustomerName = "Customer" // Would come from order details
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for merchant {MerchantId}", merchantId);
            return new List<PaymentListItemModel>();
        }
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(Guid paymentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/payments/{paymentId}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PaymentResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
            return null;
        }
    }

    public async Task<SettlementReportModel> GetSettlementReportAsync(Guid merchantId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/payments/settlement-report?merchantId={merchantId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

            if (!response.IsSuccessStatusCode)
            {
                return CreateEmptySettlementReport(startDate, endDate);
            }

            var settlement = await response.Content.ReadFromJsonAsync<MerchantCashSummaryResponse>();
            if (settlement == null)
            {
                return CreateEmptySettlementReport(startDate, endDate);
            }

            // Generate daily breakdown
            var dailyBreakdown = new List<DailySettlementModel>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayPayments = settlement.Payments.Where(p => p.CreatedAt.Date == date).ToList();
                if (dayPayments.Any())
                {
                    var dayRevenue = dayPayments.Sum(p => p.Amount);
                    var dayCommission = dayRevenue * 0.15m; // 15% commission

                    dailyBreakdown.Add(new DailySettlementModel
                    {
                        Date = date,
                        Revenue = dayRevenue,
                        Commission = dayCommission,
                        NetAmount = dayRevenue - dayCommission,
                        OrderCount = dayPayments.Count
                    });
                }
            }

            return new SettlementReportModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = settlement.TotalAmount,
                TotalCommission = settlement.TotalCommission,
                NetAmount = settlement.NetAmount,
                TotalOrders = settlement.TotalOrders,
                CompletedOrders = settlement.Payments.Count(p => p.Status.ToString() == "Completed"),
                DailyBreakdown = dailyBreakdown
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settlement report");
            return CreateEmptySettlementReport(startDate, endDate);
        }
    }

    public async Task<RevenueAnalyticsModel> GetRevenueAnalyticsAsync(Guid merchantId)
    {
        try
        {
            // Get payments for different periods
            var now = DateTime.Now;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            var response = await _httpClient.GetAsync(
                $"/api/payments/merchant?merchantId={merchantId}&startDate={yearStart:yyyy-MM-dd}");

            if (!response.IsSuccessStatusCode)
            {
                return CreateEmptyAnalytics();
            }

            var payments = await response.Content.ReadFromJsonAsync<List<PaymentResponse>>() ?? new();

            var completedPayments = payments.Where(p => p.Status.ToString() == "Completed").ToList();

            return new RevenueAnalyticsModel
            {
                GeneratedAt = now,
                TodayRevenue = completedPayments.Where(p => p.CreatedAt.Date == todayStart).Sum(p => p.Amount),
                WeekRevenue = completedPayments.Where(p => p.CreatedAt >= weekStart).Sum(p => p.Amount),
                MonthRevenue = completedPayments.Where(p => p.CreatedAt >= monthStart).Sum(p => p.Amount),
                YearRevenue = completedPayments.Sum(p => p.Amount),
                TodayOrders = completedPayments.Count(p => p.CreatedAt.Date == todayStart),
                WeekOrders = completedPayments.Count(p => p.CreatedAt >= weekStart),
                MonthOrders = completedPayments.Count(p => p.CreatedAt >= monthStart),
                YearOrders = completedPayments.Count,
                AverageOrderValue = completedPayments.Any() ? completedPayments.Average(p => p.Amount) : 0,
                RevenueByMethod = completedPayments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                OrdersByMethod = completedPayments
                    .GroupBy(p => p.PaymentMethod.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics");
            return CreateEmptyAnalytics();
        }
    }

    public async Task<List<PaymentMethodBreakdownModel>> GetPaymentMethodBreakdownAsync(Guid merchantId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var response = await _httpClient.GetAsync(
                $"/api/payments/merchant?merchantId={merchantId}&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<PaymentMethodBreakdownModel>();
            }

            var payments = await response.Content.ReadFromJsonAsync<List<PaymentResponse>>() ?? new();
            var completedPayments = payments.Where(p => p.Status.ToString() == "Completed").ToList();

            if (!completedPayments.Any())
                return new List<PaymentMethodBreakdownModel>();

            var totalAmount = completedPayments.Sum(p => p.Amount);
            var breakdown = completedPayments
                .GroupBy(p => p.PaymentMethod.ToString())
                .Select(g => new PaymentMethodBreakdownModel
                {
                    Method = g.Key,
                    DisplayName = GetPaymentMethodDisplayName(g.Key),
                    OrderCount = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount),
                    Percentage = (g.Sum(p => p.Amount) / totalAmount) * 100,
                    Color = GetPaymentMethodColor(g.Key)
                })
                .OrderByDescending(b => b.TotalAmount)
                .ToList();

            return breakdown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment method breakdown");
            return new List<PaymentMethodBreakdownModel>();
        }
    }

    public async Task<byte[]> ExportToExcelAsync(Guid merchantId, PaymentExportRequest request)
    {
        try
        {
            var filter = new PaymentFilterModel
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = request.Status
            };

            var payments = await GetPaymentHistoryAsync(merchantId, filter);

            // Simple CSV export (can be upgraded to proper Excel with EPPlus or ClosedXML)
            var csv = new StringBuilder();
            csv.AppendLine("Order Number,Payment Method,Status,Amount,Created At,Completed At");

            foreach (var payment in payments)
            {
                csv.AppendLine($"{payment.OrderNumber},{payment.PaymentMethod},{payment.Status},{payment.Amount:F2},{payment.CreatedAt:yyyy-MM-dd HH:mm},{payment.CompletedAt:yyyy-MM-dd HH:mm}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payments to Excel");
            return Encoding.UTF8.GetBytes("Error generating export");
        }
    }

    public async Task<byte[]> ExportToPdfAsync(Guid merchantId, PaymentExportRequest request)
    {
        // TODO: Implement PDF export with iTextSharp or similar library
        // For now, return CSV as fallback
        return await ExportToExcelAsync(merchantId, request);
    }

    #region Helper Methods

    private SettlementReportModel CreateEmptySettlementReport(DateTime startDate, DateTime endDate)
    {
        return new SettlementReportModel
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = 0,
            TotalCommission = 0,
            NetAmount = 0,
            TotalOrders = 0,
            CompletedOrders = 0,
            RevenueByMethod = new Dictionary<string, decimal>(),
            DailyBreakdown = new List<DailySettlementModel>()
        };
    }

    private RevenueAnalyticsModel CreateEmptyAnalytics()
    {
        return new RevenueAnalyticsModel
        {
            GeneratedAt = DateTime.Now,
            TodayRevenue = 0,
            WeekRevenue = 0,
            MonthRevenue = 0,
            YearRevenue = 0,
            TodayOrders = 0,
            WeekOrders = 0,
            MonthOrders = 0,
            YearOrders = 0,
            AverageOrderValue = 0,
            RevenueByMethod = new Dictionary<string, decimal>(),
            OrdersByMethod = new Dictionary<string, int>(),
            TopCustomers = new List<TopCustomerModel>()
        };
    }

    private string GetPaymentMethodDisplayName(string method)
    {
        return method switch
        {
            "Cash" => "Kapıda Nakit",
            "CreditCard" => "Kredi Kartı",
            "VodafonePay" => "Vodafone Pay",
            "BankTransfer" => "Havale/EFT",
            "BkmExpress" => "BKM Express",
            "Papara" => "Papara",
            "QrCode" => "QR Code",
            _ => method
        };
    }

    private string GetPaymentMethodColor(string method)
    {
        return method switch
        {
            "Cash" => "#28a745",
            "CreditCard" => "#007bff",
            "VodafonePay" => "#e60000",
            "BankTransfer" => "#6c757d",
            "BkmExpress" => "#ffc107",
            "Papara" => "#9c27b0",
            "QrCode" => "#17a2b8",
            _ => "#6c757d"
        };
    }

    #endregion
}
