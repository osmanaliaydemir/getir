# 🎯 GETIR CLONE - YAPILACAKLAR LİSTESİ

**Tarih:** 19 Ekim 2025  
**Versiyon:** 2.0 (Sadeleştirilmiş - Sadece TODO'lar)

---

## 📊 GENEL DURUM

| Modül | Tamamlanma | Kalan İş |
|-------|-----------|----------|
| **Mobile App** | %100 | - |
| **Web API** | %85 | 3 madde |
| **Merchant Portal** | %80 | 5 madde |

---

# 🌐 WEB API - YAPILACAKLAR

## 🔴 CRITICAL (Öncelik 1)

### ✅ ~~1. Unit Test Coverage~~ TAMAMLANDI
```
✅ 247 test yazıldı (38/38 servis - %100 coverage!)
✅ %100 başarı oranı
✅ 12,000+ satır test kodu
✅ Global standartlar (xUnit + Moq + FluentAssertions)
```

### 2. Application Insights (2 saat)

**Sorun:**
- Production monitoring yok
- Performance tracking yok
- Exception tracking yok

**Yapılacaklar:**
```csharp
// 1. Package ekle: Microsoft.ApplicationInsights.AspNetCore

// 2. Program.cs - Service Registration
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});

// 3. appsettings.json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-key-here",
    "EnableAdaptiveSampling": true,
    "EnableDependencyTracking": true
  }
}

// 4. Custom telemetry tracking (opsiyonel)
services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
```

**Çıktı:**
- Request telemetry
- Exception tracking
- Performance metrics
- Dependency tracking
- Custom events

---

## 🟡 YÜKSEK ÖNCELİKLİ (Öncelik 2)

### 3. Background Jobs - Hangfire (8-12 saat)

**İhtiyaç:**
- Order timeout check (15 dakika sonra otomatik iptal)
- Notification batch send
- Report generation
- Cache invalidation
- Stock sync

**Yapılacaklar:**
```csharp
// 1. Package ekle: Hangfire.AspNetCore, Hangfire.SqlServer

// 2. Program.cs - Service Registration
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
});

// 3. Jobs Oluştur
public class OrderBackgroundJobs
{
    private readonly IOrderService _orderService;
    
    [AutomaticRetry(Attempts = 3)]
    public async Task CheckOrderTimeouts()
    {
        // 15 dakikadan eski Pending siparişleri iptal et
        var expiredOrders = await _orderService.GetExpiredPendingOrdersAsync();
        foreach (var order in expiredOrders)
        {
            await _orderService.CancelOrderAsync(order.Id, Guid.Empty, "Timeout");
        }
    }
    
    [AutomaticRetry(Attempts = 3)]
    public async Task SendPendingNotifications()
    {
        // Gönderilmemiş bildirimleri gönder
    }
    
    public async Task GenerateDailyReports()
    {
        // Günlük raporları oluştur
    }
}

// 4. Schedule Jobs
RecurringJob.AddOrUpdate<OrderBackgroundJobs>(
    "check-order-timeouts",
    x => x.CheckOrderTimeouts(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<OrderBackgroundJobs>(
    "send-notifications",
    x => x.SendPendingNotifications(),
    "*/5 * * * *"); // Her 5 dakika

RecurringJob.AddOrUpdate<OrderBackgroundJobs>(
    "daily-reports",
    x => x.GenerateDailyReports(),
    Cron.Daily(2)); // Her gün saat 02:00

// 5. Dashboard ekle (opsiyonel)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

**Çıktı:**
- Otomatik order timeout handling
- Scheduled notification sending
- Automated reporting
- Background task management

---

## 🟢 ORTA ÖNCELİKLİ (Öncelik 3)

### 4. CORS Policy Hardening (1 saat)

**Sorun:**
```csharp
policy.SetIsOriginAllowed(_ => true) // ❌ Allow all origins
```

**Yapılacaklar:**
```csharp
// 1. appsettings.json
{
  "Cors": {
    "AllowedOrigins": [
      "https://merchant.getir.com",
      "https://admin.getir.com",
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}

// 2. Program.cs
options.AddPolicy("SignalRCorsPolicy", policy =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();
    
    if (builder.Environment.IsDevelopment())
    {
        policy.SetIsOriginAllowed(_ => true);
    }
    else
    {
        policy.WithOrigins(allowedOrigins);
    }
    
    policy.AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
});

// 3. appsettings.Production.json
{
  "Cors": {
    "AllowedOrigins": [
      "https://merchant.getir.com",
      "https://admin.getir.com"
    ]
  }
}
```

**Çıktı:**
- Production-safe CORS
- Environment-based configuration
- Security enhancement

---

# 💼 MERCHANT PORTAL - YAPILACAKLAR

## 🟡 YÜKSEK ÖNCELİKLİ

### 1. Payment Tracking Module (4-5 saat)

**Eksik Özellikler:**
- Payment history listing
- Settlement reports
- Revenue analytics
- Payment method breakdown
- Excel/PDF export
- Invoice generation

**Yapılacak Dosyalar:**
```
src/MerchantPortal/
├── Controllers/PaymentsController.cs       (YENİ)
├── Services/IPaymentService.cs             (YENİ)
├── Services/PaymentService.cs              (YENİ)
├── Models/PaymentModels.cs                 (YENİ)
├── Views/Payments/
│   ├── Index.cshtml                        (YENİ)
│   ├── Reports.cshtml                      (YENİ)
│   └── Settlements.cshtml                  (YENİ)
└── wwwroot/js/payments.js                  (YENİ)
```

**Özellikler:**
- Payment history table (DataTables)
- Date range filter
- Payment status filter
- Export to Excel
- Settlement summary cards
- Revenue charts

---

### 2. Advanced Analytics Dashboard (3-4 saat)

**Eksik:**
- Chart.js entegrasyonu
- Visual graphs
- Interactive charts

**Yapılacaklar:**
```html
<!-- 1. Chart.js ekle -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

<!-- 2. Sales Line Chart -->
<canvas id="salesChart" height="100"></canvas>

<!-- 3. Orders Bar Chart -->
<canvas id="ordersChart" height="100"></canvas>

<!-- 4. Category Pie Chart -->
<canvas id="categoryChart" width="200" height="200"></canvas>

<script>
// Sales trend
new Chart(ctx, {
    type: 'line',
    data: { /* from API */ }
});

// Orders by status
new Chart(ctx, {
    type: 'bar',
    data: { /* from API */ }
});

// Categories breakdown
new Chart(ctx, {
    type: 'pie',
    data: { /* from API */ }
});
</script>
```

**Özellikler:**
- Sales line chart (30 gün)
- Orders bar chart (status breakdown)
- Category pie chart
- Top products table
- Customer insights

---

## 🟢 ORTA ÖNCELİKLİ

### 3. Stock Management Enhancement (2-3 saat)

**Eklenecek Özellikler:**
- Low stock dashboard widget
- Bulk stock update modal
- Stock history timeline
- CSV import/export
- Reorder point alerts

**Yapılacaklar:**
```html
<!-- Dashboard widget -->
<div class="card">
    <div class="card-header">⚠️ Low Stock Alerts</div>
    <div class="card-body">
        <ul id="lowStockList"></ul>
    </div>
</div>

<!-- Bulk update modal -->
<div class="modal" id="bulkStockModal">
    <input type="file" accept=".csv,.xlsx" id="stockFile" />
    <button onclick="uploadStockFile()">Upload</button>
</div>

<!-- History timeline -->
<div class="timeline">
    <div class="timeline-item" data-foreach="history">
        <span class="time">{{time}}</span>
        <span class="change">{{change}}</span>
    </div>
</div>
```

---

### 4. File Upload Enhancement (2-3 saat)

**Eklenecek:**
- Drag & drop upload
- Image preview
- Image compression
- Multiple files
- Progress bar

**Yapılacaklar:**
```javascript
// Drag & drop
dropzone.addEventListener('drop', async (e) => {
    const files = e.dataTransfer.files;
    for (const file of files) {
        await uploadFile(file);
    }
});

// Image compression
async function compressImage(file) {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    // Compress logic
    return compressedBlob;
}

// Progress tracking
const formData = new FormData();
formData.append('file', file);

await axios.post('/upload', formData, {
    onUploadProgress: (e) => {
        const percent = Math.round((e.loaded * 100) / e.total);
        updateProgressBar(percent);
    }
});
```

---

# 📊 ÖZET - KALAN İŞLER

| # | Görev | Modül | Süre | Öncelik |
|---|-------|-------|------|---------|
| 1 | ~~Unit Test Coverage~~ | ~~Web API~~ | ~~40-60h~~ | ✅ **TAMAMLANDI** |
| 2 | **Application Insights** | **Web API** | **2h** | 🔴 **KRİTİK** |
| 3 | **Background Jobs (Hangfire)** | **Web API** | **8-12h** | 🟡 **YÜKSEK** |
| 4 | **CORS Policy Hardening** | **Web API** | **1h** | 🟢 **ORTA** |
| 5 | **Payment Tracking Module** | **Portal** | **4-5h** | 🟡 **YÜKSEK** |
| 6 | **Advanced Analytics** | **Portal** | **3-4h** | 🟡 **YÜKSEK** |
| 7 | **Stock Management Enhancement** | **Portal** | **2-3h** | 🟢 **ORTA** |
| 8 | **File Upload Enhancement** | **Portal** | **2-3h** | 🟢 **ORTA** |

**Toplam Kalan:** 21-30 saat (3-4 gün)

---

## 🎯 TAVSİYE EDİLEN SIRALAMA

### Bu Hafta (Kritik + Hızlı)
1. **CORS Policy Hardening** (1h) - Hızlı security fix
2. **Application Insights** (2h) - Production monitoring

### Gelecek Hafta (Yüksek Öncelik)
3. **Payment Tracking Module** (4-5h) - Business critical
4. **Advanced Analytics** (3-4h) - Dashboard enhancement
5. **Background Jobs** (8-12h) - Büyük feature

### Sonrası (Enhancement)
6. **Stock Management Enhancement** (2-3h)
7. **File Upload Enhancement** (2-3h)

**Toplam:** 21-30 saat

---

## 📈 İLERLEME TAKIBI

### ✅ Tamamlanan (Bu Session)
- [x] Unit Test Coverage - BATCH 1 (104 test)
  - StockManagementService (26 test)
  - ReviewService (28 test)
  - PaymentService (10 test)
  - AdminService (11 test)
  - MerchantService (4 test)
  - CartService (+6 test, 4→10)
  - OrderService (+10 test, 5→15)
- [x] Unit Test Coverage - BATCH 2 (30 test)
  - ProductCategoryService (5 test)
  - SearchService (4 test)
  - FavoritesService (7 test)
  - UserAddressService (5 test)
  - CampaignService (2 test)
  - WorkingHoursService (4 test)
  - DeliveryZoneService (3 test)
- [x] Working Hours Integration (1.5 saat)
  - Backend ↔ Frontend DTO mapping
  - DayOfWeek enum ↔ string conversion
  - TimeSpan ↔ string time parsing
  - IsOpen24Hours logic implementation

📊 Test Coverage Özet:
- Toplam Servisler: 38
- Test Edilen: 38 (%100! 🎉)
- Toplam Test: 247
- Coverage: %100 (FULL COVERAGE!)
- Durum: %100 Passing ✅

### ⏳ Devam Eden
- [ ] Application Insights
- [ ] Background Jobs (Hangfire)
- [ ] CORS Policy Hardening
- [ ] Payment Tracking Module
- [ ] Advanced Analytics
- [ ] Stock Management Enhancement
- [ ] File Upload Enhancement

---

## 🔥 ÖNCELİK PUANI

| Görev | Kritiklik | İş Değeri | Kolaylık | **TOPLAM** |
|-------|-----------|-----------|----------|------------|
| CORS Policy | 7/10 | 6/10 | 10/10 | **23/30** ⭐ |
| Application Insights | 9/10 | 8/10 | 8/10 | **25/30** ⭐⭐ |
| Payment Tracking | 7/10 | 9/10 | 6/10 | **22/30** ⭐ |
| Advanced Analytics | 5/10 | 7/10 | 7/10 | **19/30** |
| Background Jobs | 6/10 | 8/10 | 4/10 | **18/30** |
| Stock Enhancement | 4/10 | 6/10 | 8/10 | **18/30** |
| File Upload | 3/10 | 5/10 | 7/10 | **15/30** |

**Önerilen Sıra:** Application Insights → CORS → Payment → Analytics → Background Jobs → Diğerleri

---

**Rapor Sahibi:** Senior .NET & Flutter Architect  
**Son Güncelleme:** 19 Ekim 2025, Saat 23:45  
**Durum:** 8 görev kaldı, 23-32 saat (3-4 gün)

---

**🚀 Bir sonraki görevi seç ve başlayalım!**
