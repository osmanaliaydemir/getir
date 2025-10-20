# 🎯 GETIR CLONE - YAPILACAKLAR LİSTESİ

**Tarih:** 19 Ekim 2025  
**Versiyon:** 2.0 (Sadeleştirilmiş - Sadece TODO'lar)

---

## 📊 GENEL DURUM

| Modül | Tamamlanma | Kalan İş |
|-------|-----------|----------|
| **Mobile App** | %100 | - |
| **Web API** | %98 | 1 madde |
| **Merchant Portal** | %100 | - |

---

# 🌐 WEB API - YAPILACAKLAR

## 🟡 YÜKSEK ÖNCELİKLİ (Öncelik 1)

### 1. Background Jobs - Hangfire (8-12 saat)

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

# 📊 ÖZET - KALAN İŞLER

| # | Görev | Modül | Süre | Öncelik |
|---|-------|-------|------|---------|
| 1 | **Background Jobs (Hangfire)** | **Web API** | **8-12h** | 🟡 **YÜKSEK** |

**Toplam Kalan:** 8-12 saat (1 gün) 🎉

---

## 🎯 TAVSİYE EDİLEN SIRALAMA

### 🔥 KALAN GÖREV
1. **Background Jobs (Hangfire)** (8-12h) - Otomasyon için kritik

**Toplam Kalan:** 8-12 saat

---

## 📈 İLERLEME TAKIBI

### ✅ Tamamlanan (Bu Oturum)
- [x] **CORS Policy Hardening** (1 saat) ✅
  - `appsettings.json`: Local URLs (localhost:7001, localhost:7169)
  - `appsettings.Production.json`: Production URLs (ajilgo.runasp.net, ajilgo-portal.runasp.net)
  - `Program.cs`: Environment-based CORS policy
  - Development: Allow all origins (testing)
  - Production: Restricted to configured origins only
  - ✅ Build successful

### ⏳ Yapılacaklar
- [ ] Background Jobs (Hangfire)

---

**Rapor Sahibi:** Senior .NET & Flutter Architect  
**Son Güncelleme:** 20 Ekim 2025, Saat 03:00  
**Durum:** 1 görev kaldı, 8-12 saat (1 gün) - %98 tamamlandı! 🎉

---

**🚀 Bir sonraki görevi seç ve başlayalım!**
