using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize(Roles = "Admin,MerchantOwner")]
public class RealtimeTrackingController : Controller
{
    private readonly IRealtimeTrackingPortalService _realtimeTrackingService;
    private readonly ILocalizationService _localizationService;

    public RealtimeTrackingController(
        IRealtimeTrackingPortalService realtimeTrackingService,
        ILocalizationService localizationService)
    {
        _realtimeTrackingService = realtimeTrackingService;
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? trackingId = null, Guid? orderId = null)
    {
        var active = await _realtimeTrackingService.GetActiveTrackingsAsync();

        OrderTrackingResponse? selected = null;
        if (trackingId.HasValue)
        {
            selected = await _realtimeTrackingService.GetTrackingByIdAsync(trackingId.Value);
        }
        else if (orderId.HasValue)
        {
            selected = await _realtimeTrackingService.GetTrackingByOrderIdAsync(orderId.Value);
        }
        else
        {
            selected = active.FirstOrDefault();
        }

        List<TrackingNotificationResponse> notifications = new();
        if (selected != null)
        {
            notifications = await _realtimeTrackingService.GetNotificationsAsync(selected.Id);
        }

        var viewModel = new RealtimeTrackingViewModel
        {
            ActiveTrackings = active,
            SelectedTracking = selected,
            Notifications = notifications
        };

        ViewBag.Title = _localizationService.GetString("RealtimeTracking");

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(StatusUpdateRequestModel request)
    {
        if (request.OrderTrackingId == Guid.Empty)
        {
            TempData["Error"] = _localizationService.GetString("TrackingInvalidTracking");
            return RedirectToAction(nameof(Index));
        }

        var success = await _realtimeTrackingService.UpdateStatusAsync(request);
        TempData[success ? "Success" : "Error"] = success
            ? _localizationService.GetString("TrackingStatusUpdated")
            : _localizationService.GetString("TrackingStatusUpdateFailed");

        return RedirectToAction(nameof(Index), new { trackingId = request.OrderTrackingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLocation(LocationUpdateRequestModel request)
    {
        if (request.OrderTrackingId == Guid.Empty)
        {
            TempData["Error"] = _localizationService.GetString("TrackingInvalidTracking");
            return RedirectToAction(nameof(Index));
        }

        var success = await _realtimeTrackingService.UpdateLocationAsync(request);
        TempData[success ? "Success" : "Error"] = success
            ? _localizationService.GetString("TrackingLocationUpdated")
            : _localizationService.GetString("TrackingLocationUpdateFailed");

        return RedirectToAction(nameof(Index), new { trackingId = request.OrderTrackingId });
    }
}

