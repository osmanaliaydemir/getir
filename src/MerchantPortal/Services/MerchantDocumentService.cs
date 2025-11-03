using System.Net.Http.Headers;
using Getir.MerchantPortal.Models;

namespace Getir.MerchantPortal.Services;

public class MerchantDocumentService : IMerchantDocumentService
{
	private readonly HttpClient _httpClient;
	private readonly IApiClient _apiClient;
	private readonly ILogger<MerchantDocumentService> _logger;

	public MerchantDocumentService(HttpClient httpClient, IApiClient apiClient, ILogger<MerchantDocumentService> logger)
	{
		_httpClient = httpClient;
		_apiClient = apiClient;
		_logger = logger;
	}

	public async Task<PagedResult<MerchantDocumentResponse>?> GetDocumentsAsync(Guid? merchantId = null, string? documentType = null, string? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
	{
		var qs = new List<string> { $"page={page}", $"pageSize={pageSize}" };
		if (merchantId.HasValue) qs.Add($"merchantId={merchantId}");
		if (!string.IsNullOrWhiteSpace(documentType)) qs.Add($"documentType={documentType}");
		if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={status}");
		var url = "api/merchantdocument" + (qs.Count > 0 ? ("?" + string.Join("&", qs)) : string.Empty);
		try
		{
			var res = await _apiClient.GetAsync<ApiResponse<PagedResult<MerchantDocumentResponse>>>(url, ct);
			return res?.Data;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting merchant documents");
			return null;
		}
	}

	public async Task<MerchantDocumentResponse?> UploadAsync(UploadMerchantDocumentRequest request, IFormFile file, CancellationToken ct = default)
	{
		try
		{
			using var content = new MultipartFormDataContent();
			content.Add(new StringContent(request.MerchantId.ToString()), nameof(UploadMerchantDocumentRequest.MerchantId));
			content.Add(new StringContent(request.DocumentType), nameof(UploadMerchantDocumentRequest.DocumentType));
			if (!string.IsNullOrWhiteSpace(request.Notes)) content.Add(new StringContent(request.Notes), nameof(UploadMerchantDocumentRequest.Notes));

			var fileContent = new StreamContent(file.OpenReadStream());
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
			content.Add(fileContent, "file", file.FileName);

			var response = await _httpClient.PostAsync("api/merchantdocument/upload", content, ct);
			if (!response.IsSuccessStatusCode) return null;
			var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<MerchantDocumentResponse>>(cancellationToken: ct);
			return apiResponse?.Data;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error uploading merchant document");
			return null;
		}
	}

	public async Task<bool> DeleteAsync(Guid documentId, CancellationToken ct = default)
	{
		try
		{
			var res = await _apiClient.DeleteAsync($"api/merchantdocument/{documentId}", ct);
			return res;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting merchant document {DocumentId}", documentId);
			return false;
		}
	}
}


