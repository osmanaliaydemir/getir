using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.FileUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Dosya yönetimi işlemleri için dosya yükleme controller'ı
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Tags("File Upload")]
public class FileUploadController : BaseController
{
    private readonly IFileUploadIntegrationService _fileUploadService;
    private readonly IFileUploadAdapter _fileUploadAdapter;

    public FileUploadController(
        IFileUploadIntegrationService fileUploadService,
        IFileUploadAdapter fileUploadAdapter)
    {
        _fileUploadService = fileUploadService;
        _fileUploadAdapter = fileUploadAdapter;
    }

    #region Customer Endpoints

    /// <summary>
    /// Dosya yükle (Müşteri)
    /// </summary>
    /// <param name="file">Yüklenecek dosya</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Yükleme yanıtı</returns>
    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ApiExplorerSettings(IgnoreApi = true)] // Swagger'dan gizle
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Adapt IFormFile to framework-agnostic IUploadedFile
        var uploadedFile = _fileUploadAdapter.Adapt(file);
        
        var result = await _fileUploadService.UploadFileAsync(uploadedFile, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Birden fazla dosya yükle (Müşteri)
    /// </summary>
    /// <param name="files">Yüklenecek dosyalar</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Yükleme yanıtları</returns>
    [HttpPost("upload-multiple")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ApiExplorerSettings(IgnoreApi = true)] // Swagger'dan gizle
    public async Task<IActionResult> UploadMultipleFiles(
        [FromForm] IFormFile[] files,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Adapt IFormFile[] to framework-agnostic IUploadedFile[]
        var uploadedFiles = _fileUploadAdapter.AdaptMultiple(files);
        
        var result = await _fileUploadService.UploadMultipleFilesAsync(uploadedFiles, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Dosya indirme URL'sini al
    /// </summary>
    /// <param name="containerName">Konteyner adı</param>
    /// <param name="fileName">Dosya adı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Dosya URL'si</returns>
    [HttpGet("{containerName}/{fileName}")]
    [Authorize]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFileUrl(
        [FromRoute] string containerName,
        [FromRoute] string fileName,
        CancellationToken ct = default)
    {
        var result = await _fileUploadService.GetFileUrlAsync(containerName, fileName, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Dosya sil (Müşteri)
    /// </summary>
    /// <param name="containerName">Konteyner adı</param>
    /// <param name="fileName">Dosya adı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("{containerName}/{fileName}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFile(
        [FromRoute] string containerName,
        [FromRoute] string fileName,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _fileUploadService.DeleteFileAsync(containerName, fileName, userId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    #endregion

    #region Merchant Endpoints

    /// <summary>
    /// Merchant dosyası yükle
    /// </summary>
    /// <param name="file">Yüklenecek dosya</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Yükleme yanıtı</returns>
    [HttpPost("merchant/upload")]
    [Authorize(Roles = "MerchantOwner")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ApiExplorerSettings(IgnoreApi = true)] // Swagger'dan gizle
    public async Task<IActionResult> UploadMerchantFile(
        [FromForm] IFormFile file,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        // Adapt IFormFile to framework-agnostic IUploadedFile
        var uploadedFile = _fileUploadAdapter.Adapt(file);
        
        var result = await _fileUploadService.UploadMerchantFileAsync(uploadedFile, userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant dosyalarını al
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış merchant dosyaları</returns>
    [HttpGet("merchant")]
    [Authorize(Roles = "MerchantOwner")]
    [ProducesResponseType(typeof(PagedResult<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMerchantFiles(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _fileUploadService.GetMerchantFilesAsync(userId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Merchant dosyasını sil
    /// </summary>
    /// <param name="containerName">Konteyner adı</param>
    /// <param name="fileName">Dosya adı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("merchant/{containerName}/{fileName}")]
    [Authorize(Roles = "MerchantOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteMerchantFile(
        [FromRoute] string containerName,
        [FromRoute] string fileName,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _fileUploadService.DeleteMerchantFileAsync(containerName, fileName, userId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Tüm dosyaları al (Admin)
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Sayfalanmış dosyalar</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllFiles(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _fileUploadService.GetAllFilesAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Herhangi bir dosyayı sil (Admin)
    /// </summary>
    /// <param name="containerName">Konteyner adı</param>
    /// <param name="fileName">Dosya adı</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpDelete("admin/{containerName}/{fileName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAnyFile(
        [FromRoute] string containerName,
        [FromRoute] string fileName,
        CancellationToken ct = default)
    {
        var result = await _fileUploadService.DeleteAnyFileAsync(containerName, fileName, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Dosya istatistiklerini al (Admin)
    /// </summary>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Dosya istatistikleri</returns>
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FileStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFileStatistics(CancellationToken ct = default)
    {
        var result = await _fileUploadService.GetFileStatisticsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Eski dosyaları temizle (Admin)
    /// </summary>
    /// <param name="cutoffDate">Kesim tarihi</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Silinen dosya sayısı</returns>
    [HttpPost("admin/cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CleanupOldFiles(
        [FromQuery] DateTime cutoffDate,
        CancellationToken ct = default)
    {
        var result = await _fileUploadService.CleanupOldFilesAsync(cutoffDate, ct);
        return ToActionResult(result);
    }

    #endregion
}