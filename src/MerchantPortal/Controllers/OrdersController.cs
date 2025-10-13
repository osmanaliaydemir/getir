using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, string? status = null)
    {
        var orders = await _orderService.GetOrdersAsync(page, 20, status);
        ViewBag.CurrentStatus = status;
        
        return View(orders);
    }

    public async Task<IActionResult> Pending(int page = 1)
    {
        var orders = await _orderService.GetPendingOrdersAsync(page, 20);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _orderService.GetOrderDetailsAsync(id);
        
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status, string? notes = null)
    {
        var request = new UpdateOrderStatusRequest
        {
            Status = status,
            Notes = notes
        };

        var result = await _orderService.UpdateOrderStatusAsync(id, request);

        if (result)
        {
            TempData["SuccessMessage"] = "Sipariş durumu güncellendi";
        }
        else
        {
            TempData["ErrorMessage"] = "Sipariş durumu güncellenirken bir hata oluştu";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}

