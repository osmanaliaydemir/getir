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

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Kategori listesini gösterir.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Standart kategorileri getir (ServiceCategory bazlı)
        var standardCategories = await _categoryService.GetStandardCategoriesAsync();
        
        // Merchant'ın özel kategorilerini getir
        var customCategories = await _categoryService.GetMyCategoriesAsync();
        
        ViewBag.StandardCategories = standardCategories ?? new();
        ViewBag.CustomCategories = customCategories ?? new();
        ViewBag.IsAdmin = IsAdminUser();
        
        return View();
    }

    /// <summary>
    /// Yeni kategori oluşturmak için gösterilecek sayfa.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryService.GetMyCategoriesAsync();
        ViewBag.Categories = categories ?? new();
        
        return View();
    }

    /// <summary>
    /// Yeni kategori oluşturmak için gönderilen isteği işler.
    /// </summary>
    /// <param name="model">Kategori oluşturmak için gerekli bilgiler</param>
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

    /// <summary>
    /// Mevcut kategoriyi düzenlemek için gösterilecek sayfa.
    /// </summary>
    /// <param name="id">Düzenlenecek kategori ID'si</param>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (category == null)
        {
            return NotFound();
        }

        // Standart kategorileri düzenleyemez (admin hariç)
        if (category.MerchantId == null && !IsAdminUser())
        {
            TempData["ErrorMessage"] = "Standart kategoriler düzenlenemez";
            return RedirectToAction(nameof(Index));
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

    /// <summary>
    /// Mevcut kategoriyi düzenlemek için gönderilen isteği işler.
    /// </summary>
    /// <param name="id">Düzenlenecek kategori ID'si</param>
    /// <param name="model">Kategori düzenleme bilgileri</param>
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

    /// <summary>
    /// Mevcut kategoriyi silmek için gönderilen isteği işler.
    /// </summary>
    /// <param name="id">Silinecek kategori ID'si</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (category == null)
        {
            TempData["ErrorMessage"] = "Kategori bulunamadı";
            return RedirectToAction(nameof(Index));
        }

        // Standart kategorileri silemez (admin hariç)
        if (category.MerchantId == null && !IsAdminUser())
        {
            TempData["ErrorMessage"] = "Standart kategoriler silinemez";
            return RedirectToAction(nameof(Index));
        }

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

    /// <summary>
    /// Kullanıcının admin olup olmadığını kontrol eder.
    /// </summary>
    /// <returns>Admin ise true, değilse false</returns>
    private bool IsAdminUser()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        return userRole == "Admin";
    }

    /// <summary>
    /// Kategori sıralamasını güncellemek için gönderilen isteği işler.
    /// </summary>
    /// <param name="updates">Güncellenecek kategori sıralama bilgileri</param>
    [HttpPost]
    public async Task<IActionResult> UpdateOrder([FromBody] List<CategoryOrderUpdate> updates)
    {
        try
        {
            if (updates == null || !updates.Any())
            {
                return Json(new { success = false, message = "Güncelleme verisi bulunamadı" });
            }

            // Her kategori için DisplayOrder'ı güncelle
            foreach (var update in updates)
            {
                var updateRequest = new UpdateCategoryRequest
                {
                    Name = string.Empty, // Bu değerler güncellenmeyecek, sadece order
                    DisplayOrder = update.DisplayOrder
                };

                var category = await _categoryService.GetCategoryByIdAsync(update.CategoryId);
                if (category != null)
                {
                    // Mevcut değerleri koru, sadece DisplayOrder değiştir
                    updateRequest.Name = category.Name;
                    updateRequest.Description = category.Description;
                    updateRequest.ParentCategoryId = update.ParentCategoryId;
                    updateRequest.IsActive = category.IsActive;
                    updateRequest.ImageUrl = category.ImageUrl;

                    await _categoryService.UpdateCategoryAsync(update.CategoryId, updateRequest);
                }
            }

            return Json(new { success = true, message = "Sıralama başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category order");
            return Json(new { success = false, message = "Sıralama güncellenirken hata oluştu: " + ex.Message });
        }
    }
}

public class CategoryOrderUpdate
{
    public Guid CategoryId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DisplayOrder { get; set; }
}

