using Getir.MerchantPortal.Models;
using Getir.MerchantPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.MerchantPortal.Controllers;

[Authorize]
public class DocumentsController : Controller
{
	private readonly IMerchantDocumentService _documentService;
	private readonly IMerchantService _merchantService;
	private readonly ILogger<DocumentsController> _logger;

	public DocumentsController(IMerchantDocumentService documentService, IMerchantService merchantService, ILogger<DocumentsController> logger)
	{
		_documentService = documentService;
		_merchantService = merchantService;
		_logger = logger;
	}

	public async Task<IActionResult> Index(int page = 1)
	{
		var me = await _merchantService.GetMyMerchantAsync();
		if (me == null) return NotFound();
		var docs = await _documentService.GetDocumentsAsync(me.Id, page: page, pageSize: 20);
		ViewBag.MerchantId = me.Id;
		return View(docs ?? new PagedResult<MerchantDocumentResponse>());
	}

	[HttpGet]
	public IActionResult Upload(Guid merchantId)
	{
		return View(new UploadMerchantDocumentRequest { MerchantId = merchantId });
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Upload(UploadMerchantDocumentRequest model, IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			ModelState.AddModelError(string.Empty, "Dosya seçiniz");
			return View(model);
		}
		var created = await _documentService.UploadAsync(model, file);
		if (created == null)
		{
			ModelState.AddModelError(string.Empty, "Belge yüklenemedi");
			return View(model);
		}
		TempData["SuccessMessage"] = "Belge yüklendi";
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id)
	{
		var ok = await _documentService.DeleteAsync(id);
		TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Belge silindi" : "Belge silinemedi";
		return RedirectToAction(nameof(Index));
	}
}


