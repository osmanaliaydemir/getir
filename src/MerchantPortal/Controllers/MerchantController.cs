using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class MerchantController : Controller
{
    private readonly IMerchantService _merchantService;
    private readonly IWorkingHoursService _workingHoursService;
    private readonly ILogger<MerchantController> _logger;

    public MerchantController(IMerchantService merchantService, IWorkingHoursService workingHoursService, ILogger<MerchantController> logger)
    {
        _merchantService = merchantService;
        _workingHoursService = workingHoursService;
        _logger = logger;
    }

    public IActionResult Profile()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            TempData["ErrorMessage"] = "Merchant bilgisi bulunamadı";
            return RedirectToAction("Index", "Dashboard");
        }

        // Get merchant from session or API (implement later)
        // For now, redirect to edit with merchantId
        return RedirectToAction(nameof(Edit), new { id = merchantId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var sessionMerchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Security check - can only edit own merchant
        if (id != sessionMerchantId)
        {
            TempData["ErrorMessage"] = "Bu işlemi yapmaya yetkiniz yok";
            return RedirectToAction("Index", "Dashboard");
        }

        // Get merchant details from API
        var merchant = await _merchantService.GetMyMerchantAsync();
        
        if (merchant == null)
        {
            TempData["ErrorMessage"] = "Mağaza bilgileri yüklenemedi";
            return RedirectToAction("Index", "Dashboard");
        }

        var model = new UpdateMerchantRequest
        {
            Name = merchant.Name,
            Description = merchant.Description,
            Address = merchant.Address,
            Latitude = merchant.Latitude,
            Longitude = merchant.Longitude,
            PhoneNumber = merchant.PhoneNumber,
            Email = merchant.Email,
            MinimumOrderAmount = merchant.MinimumOrderAmount,
            DeliveryFee = merchant.DeliveryFee,
            AverageDeliveryTime = merchant.AverageDeliveryTime,
            IsActive = merchant.IsActive,
            IsBusy = merchant.IsBusy,
            LogoUrl = merchant.LogoUrl,
            CoverImageUrl = merchant.CoverImageUrl
        };

        ViewBag.MerchantId = id;
        ViewBag.MerchantName = merchant.Name;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateMerchantRequest model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.MerchantId = id;
            return View(model);
        }

        var result = await _merchantService.UpdateMerchantAsync(id, model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Profil güncellenirken bir hata oluştu");
            ViewBag.MerchantId = id;
            return View(model);
        }

        TempData["SuccessMessage"] = "Profil başarıyla güncellendi";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> WorkingHours()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Get working hours from API
        var workingHours = await _workingHoursService.GetWorkingHoursByMerchantAsync(merchantId);
        
        // If no working hours exist, create default schedule
        if (workingHours == null || !workingHours.Any())
        {
            workingHours = new List<WorkingHoursResponse>
            {
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Monday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, IsOpen24Hours = false },
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Tuesday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, IsOpen24Hours = false },
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Wednesday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, IsOpen24Hours = false },
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Thursday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, IsOpen24Hours = false },
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Friday", OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(18, 0, 0), IsClosed = false, IsOpen24Hours = false },
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Saturday", OpenTime = new TimeSpan(10, 0, 0), CloseTime = new TimeSpan(16, 0, 0), IsClosed = false, IsOpen24Hours = false },
                new() { Id = Guid.Empty, MerchantId = merchantId, DayOfWeek = "Sunday", OpenTime = new TimeSpan(0, 0, 0), CloseTime = new TimeSpan(0, 0, 0), IsClosed = true, IsOpen24Hours = false }
            };
        }

        ViewBag.MerchantId = merchantId;
        return View(workingHours);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateWorkingHours(List<UpdateWorkingHoursRequest> workingHours)
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Call API to update working hours
        var result = await _workingHoursService.BulkUpdateWorkingHoursAsync(merchantId, workingHours);

        if (result)
        {
            TempData["SuccessMessage"] = "Çalışma saatleri başarıyla güncellendi";
        }
        else
        {
            TempData["ErrorMessage"] = "Çalışma saatleri güncellenirken bir hata oluştu";
        }
        
        return RedirectToAction(nameof(WorkingHours));
    }

    [HttpGet]
    public IActionResult Settings()
    {
        var merchantIdStr = HttpContext.Session.GetString("MerchantId");
        if (string.IsNullOrEmpty(merchantIdStr) || !Guid.TryParse(merchantIdStr, out var merchantId))
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.MerchantId = merchantId;
        return View();
    }
}

