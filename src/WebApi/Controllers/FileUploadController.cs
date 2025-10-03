using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.FileUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// File upload controller for file management operations
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Tags("File Upload")]
public class FileUploadController : BaseController
{
    private readonly IFileUploadIntegrationService _fileUploadService;

    public FileUploadController(IFileUploadIntegrationService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    #region Customer Endpoints

    /// <summary>
    /// Upload a file (Customer)
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Upload response</returns>
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

        return BadRequest("Upload file functionality not implemented yet");
    }

    /// <summary>
    /// Upload multiple files (Customer)
    /// </summary>
    /// <param name="files">Files to upload</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Upload responses</returns>
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

        return BadRequest("Upload multiple files functionality not implemented yet");
    }

    /// <summary>
    /// Get file download URL
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="fileName">File name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File URL</returns>
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
        return BadRequest("Get file URL functionality not implemented yet");
    }

    /// <summary>
    /// Delete a file (Customer)
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="fileName">File name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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

        return BadRequest("Delete file functionality not implemented yet");
    }

    #endregion

    #region Merchant Endpoints

    /// <summary>
    /// Upload merchant file
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Upload response</returns>
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

        return BadRequest("Upload merchant file functionality not implemented yet");
    }

    /// <summary>
    /// Get merchant files
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged merchant files</returns>
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

        return BadRequest("Get merchant files functionality not implemented yet");
    }

    /// <summary>
    /// Delete merchant file
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="fileName">File name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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

        return BadRequest("Delete merchant file functionality not implemented yet");
    }

    #endregion

    #region Admin Endpoints

    /// <summary>
    /// Get all files (Admin)
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged files</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllFiles(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        return BadRequest("Get all files functionality not implemented yet");
    }

    /// <summary>
    /// Delete any file (Admin)
    /// </summary>
    /// <param name="containerName">Container name</param>
    /// <param name="fileName">File name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
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
        return BadRequest("Delete any file functionality not implemented yet");
    }

    /// <summary>
    /// Get file statistics (Admin)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>File statistics</returns>
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FileStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFileStatistics(CancellationToken ct = default)
    {
        return BadRequest("Get file statistics functionality not implemented yet");
    }

    /// <summary>
    /// Cleanup old files (Admin)
    /// </summary>
    /// <param name="cutoffDate">Cutoff date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of deleted files</returns>
    [HttpPost("admin/cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CleanupOldFiles(
        [FromQuery] DateTime cutoffDate,
        CancellationToken ct = default)
    {
        return BadRequest("Cleanup old files functionality not implemented yet");
    }

    #endregion
}