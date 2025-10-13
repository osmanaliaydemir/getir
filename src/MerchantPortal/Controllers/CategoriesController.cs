using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var categoryTree = await _categoryService.GetCategoryTreeAsync();
        var allCategories = await _categoryService.GetMyCategoriesAsync();
        
        ViewBag.AllCategories = allCategories ?? new();
        
        return View(categoryTree ?? new());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetMyCategoriesAsync();
        ViewBag.Categories = categories ?? new();
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryRequest model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetMyCategoriesAsync();
            ViewBag.Categories = categories ?? new();
            return View(model);
        }

        var result = await _categoryService.CreateCategoryAsync(model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Kategori oluşturulurken bir hata oluştu");
            var categories = await _categoryService.GetMyCategoriesAsync();
            ViewBag.Categories = categories ?? new();
            return View(model);
        }

        TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (category == null)
        {
            return NotFound();
        }

        var categories = await _categoryService.GetMyCategoriesAsync();
        // Exclude current category and its descendants to prevent circular reference
        ViewBag.Categories = categories?.Where(c => c.Id != id).ToList() ?? new();

        var model = new UpdateCategoryRequest
        {
            ParentCategoryId = category.ParentCategoryId,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCategoryRequest model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetMyCategoriesAsync();
            ViewBag.Categories = categories?.Where(c => c.Id != id).ToList() ?? new();
            return View(model);
        }

        var result = await _categoryService.UpdateCategoryAsync(id, model);

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Kategori güncellenirken bir hata oluştu");
            var categories = await _categoryService.GetMyCategoriesAsync();
            ViewBag.Categories = categories?.Where(c => c.Id != id).ToList() ?? new();
            return View(model);
        }

        TempData["SuccessMessage"] = "Kategori başarıyla güncellendi";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);

        if (result)
        {
            TempData["SuccessMessage"] = "Kategori başarıyla silindi";
        }
        else
        {
            TempData["ErrorMessage"] = "Kategori silinirken bir hata oluştu. Bu kategoriye bağlı ürünler olabilir.";
        }

        return RedirectToAction(nameof(Index));
    }
}

