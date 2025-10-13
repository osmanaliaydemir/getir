using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var products = await _productService.GetProductsAsync(page, 20);
        var categories = await _productService.GetCategoriesAsync();

        ViewBag.Categories = categories ?? new();
        
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = categories ?? new();
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductRequest model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _productService.GetCategoriesAsync();
            ViewBag.Categories = categories ?? new();
            return View(model);
        }

        var result = await _productService.CreateProductAsync(model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Ürün oluşturulurken bir hata oluştu");
            var categories = await _productService.GetCategoriesAsync();
            ViewBag.Categories = categories ?? new();
            return View(model);
        }

        TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
        {
            return NotFound();
        }

        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = categories ?? new();

        var model = new UpdateProductRequest
        {
            ProductCategoryId = product.ProductCategoryId,
            Name = product.Name,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            DiscountedPrice = product.DiscountedPrice,
            StockQuantity = product.StockQuantity,
            Unit = product.Unit,
            IsAvailable = product.IsAvailable,
            IsActive = product.IsActive,
            DisplayOrder = product.DisplayOrder
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateProductRequest model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _productService.GetCategoriesAsync();
            ViewBag.Categories = categories ?? new();
            return View(model);
        }

        var result = await _productService.UpdateProductAsync(id, model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Ürün güncellenirken bir hata oluştu");
            var categories = await _productService.GetCategoriesAsync();
            ViewBag.Categories = categories ?? new();
            return View(model);
        }

        TempData["SuccessMessage"] = "Ürün başarıyla güncellendi";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteProductAsync(id);

        if (result)
        {
            TempData["SuccessMessage"] = "Ürün başarıyla silindi";
        }
        else
        {
            TempData["ErrorMessage"] = "Ürün silinirken bir hata oluştu";
        }

        return RedirectToAction(nameof(Index));
    }
}

