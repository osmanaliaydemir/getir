using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Merchants;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Merchant document controller for merchant document management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Merchant Documents")]
public class MerchantDocumentController : BaseController
{
    private readonly IMerchantDocumentService _merchantDocumentService;

    public MerchantDocumentController(IMerchantDocumentService merchantDocumentService)
    {
        _merchantDocumentService = merchantDocumentService;
    }

    /// <summary>
    /// Upload a document for merchant onboarding
    /// </summary>
    /// <param name="request">Upload document request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Uploaded document</returns>
    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(MerchantDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ApiExplorerSettings(IgnoreApi = true)] // Swagger'dan gizle
    public async Task<IActionResult> UploadDocument(
        [FromForm] UploadMerchantDocumentRequest request,
        [FromForm] IFormFile file,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _merchantDocumentService.UploadDocumentAsync(request, file.OpenReadStream(), file.FileName, file.ContentType, GetCurrentUserId() ?? Guid.Empty, ct);
        if (result.Success)
        {
            return Created($"/api/merchant-documents/{result.Value!.Id}", result.Value);
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Get merchant documents with filtering
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="documentType">Document type</param>
    /// <param name="status">Document status</param>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged documents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MerchantDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] Guid? merchantId = null,
        [FromQuery] string? documentType = null,
        [FromQuery] string? status = null,
        [FromQuery] PaginationQuery? query = null,
        CancellationToken ct = default)
    {
        var filterQuery = new MerchantDocumentQuery
        {
            MerchantId = merchantId,
            DocumentType = documentType,
            Status = status,
            Page = query?.Page ?? 1,
            PageSize = query?.PageSize ?? 20
        };

        var getRequest = new GetMerchantDocumentsRequest
        {
            MerchantId = merchantId,
            DocumentType = !string.IsNullOrEmpty(documentType) && Enum.TryParse<DocumentType>(documentType, out var docType) ? docType : null,
            Status = !string.IsNullOrEmpty(status) && Enum.TryParse<DocumentStatus>(status, out var docStatus) ? docStatus : null,
            Page = query?.Page ?? 1,
            PageSize = query?.PageSize ?? 20
        };
        var result = await _merchantDocumentService.GetDocumentsAsync(getRequest, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Document details</returns>
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(MerchantDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById(
        [FromRoute] Guid documentId,
        CancellationToken ct = default)
    {
        var result = await _merchantDocumentService.GetDocumentByIdAsync(documentId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Verify/approve or reject a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="request">Verify document request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated document</returns>
    [HttpPost("{documentId:guid}/verify")]
    [Authorize]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MerchantDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyDocument(
        [FromRoute] Guid documentId,
        [FromBody] VerifyMerchantDocumentRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDocumentService.VerifyDocumentAsync(request, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{documentId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteDocument(
        [FromRoute] Guid documentId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDocumentService.DeleteDocumentAsync(documentId, userId, ct);
        if (result.Success)
        {
            return NoContent();
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// Get document upload progress for a merchant
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Upload progress</returns>
    [HttpGet("progress/{merchantId:guid}")]
    [ProducesResponseType(typeof(MerchantDocumentProgressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUploadProgress(
        [FromRoute] Guid merchantId,
        CancellationToken ct = default)
    {
        var result = await _merchantDocumentService.GetUploadProgressAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get required document types
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Required document types</returns>
    [HttpGet("required-types")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequiredDocumentTypes(CancellationToken ct = default)
    {
        var result = await _merchantDocumentService.GetRequiredDocumentTypesAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get document statistics
    /// </summary>
    /// <param name="merchantId">Merchant ID (optional)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Document statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MerchantDocumentStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocumentStatistics(
        [FromQuery] Guid? merchantId = null,
        CancellationToken ct = default)
    {
        var result = await _merchantDocumentService.GetDocumentStatisticsAsync(merchantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Bulk verify documents
    /// </summary>
    /// <param name="request">Bulk verify request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Bulk verification results</returns>
    [HttpPost("bulk-verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(BulkVerifyDocumentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BulkVerifyDocuments(
        [FromBody] BulkVerifyDocumentsRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var adminId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _merchantDocumentService.BulkVerifyDocumentsAsync(request, adminId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get pending documents for review
    /// </summary>
    /// <param name="query">Pagination query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged pending documents</returns>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<MerchantDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingDocuments(
        [FromQuery] PaginationQuery query,
        CancellationToken ct = default)
    {
        var result = await _merchantDocumentService.GetPendingDocumentsAsync(query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Download document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Document file</returns>
    [HttpGet("{documentId:guid}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DownloadDocument(
        [FromRoute] Guid documentId,
        CancellationToken ct = default)
    {
        var result = await _merchantDocumentService.DownloadDocumentAsync(documentId, ct);
        if (result.Success)
        {
            return File(result.Value!, "application/octet-stream", $"document_{documentId}.pdf");
        }
        return ToActionResult(result);
    }
}
