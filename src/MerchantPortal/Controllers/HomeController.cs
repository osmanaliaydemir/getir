using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Ana sayfayı gösterir.
    /// </summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Ana sayfayı gösterir.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Gizlilik politikasını gösterir.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Hata sayfasını gösterir.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
