using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Infrastructure.Services.FileStorage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILoggingService _loggingService;
    private readonly FileUploadSettings _uploadSettings;

    public AzureBlobStorageService(
        BlobServiceClient blobServiceClient,
        ILoggingService loggingService,
        FileUploadSettings uploadSettings)
    {
        _blobServiceClient = blobServiceClient;
        _loggingService = loggingService;
        _uploadSettings = uploadSettings;
    }

    public async Task<Result<FileUploadResponse>> UploadFileAsync(
        FileUploadRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file
            var validationResult = await ValidateFileAsync(request, cancellationToken);
            if (!validationResult.Success)
            {
                return Result.Fail<FileUploadResponse>(validationResult.Error ?? "Validation failed", validationResult.ErrorCode ?? "VALIDATION_ERROR");
            }

            // Get container client
            var containerClient = await GetContainerClientAsync(request.ContainerName, cancellationToken);
            
            // Generate unique filename
            var uniqueFileName = GenerateUniqueFileName(request.FileName);
            
            // Get blob client
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Upload file
            var blobUploadOptions = new BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "OriginalFileName", request.FileName },
                    { "Category", request.Category.ToString() },
                    { "ContentType", request.ContentType },
                    { "UploadedAt", DateTime.UtcNow.ToString("O") }
                }
            };

            if (!string.IsNullOrEmpty(request.Description))
            {
                blobUploadOptions.Metadata["Description"] = request.Description;
            }

            if (request.RelatedEntityId.HasValue)
            {
                blobUploadOptions.Metadata["RelatedEntityId"] = request.RelatedEntityId.Value.ToString();
            }

            if (!string.IsNullOrEmpty(request.RelatedEntityType))
            {
                blobUploadOptions.Metadata["RelatedEntityType"] = request.RelatedEntityType;
            }

            using var stream = new MemoryStream(request.FileContent);
            await blobClient.UploadAsync(stream, blobUploadOptions, cancellationToken);

            // TODO: Implement thumbnail generation when ImageSharp is available
            string? thumbnailUrl = null;

            var response = new FileUploadResponse(
                uniqueFileName,
                blobClient.Uri.ToString(),
                request.ContainerName,
                request.FileContent.Length,
                request.ContentType,
                DateTime.UtcNow,
                thumbnailUrl);

            _loggingService.LogBusinessEvent("FileUploaded", new { 
                request.FileName, 
                uniqueFileName, 
                request.ContainerName,
                request.FileContent.Length,
                request.Category 
            });

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error uploading file", ex, new { 
                request.FileName, 
                request.ContainerName,
                request.Category 
            });
            return Result.Fail<FileUploadResponse>("Failed to upload file", "FILE_UPLOAD_ERROR");
        }
    }

    public async Task<Result<IEnumerable<FileUploadResponse>>> UploadMultipleFilesAsync(
        IEnumerable<FileUploadRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<FileUploadResponse>();
        var errors = new List<string>();
        var requestsList = requests.ToList();

        foreach (var request in requestsList)
        {
            var result = await UploadFileAsync(request, cancellationToken);
            if (result.Success)
            {
                results.Add(result.Value!);
            }
            else
            {
                errors.Add($"Failed to upload {request.FileName}: {result.Error}");
            }
        }

        if (errors.Any())
        {
            _loggingService.LogError("Some files failed to upload", null, new { 
                Errors = errors,
                SuccessCount = results.Count,
                TotalCount = requestsList.Count
            });
        }

        return Result.Ok(results.AsEnumerable());
    }

    public async Task<Result> DeleteFileAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            _loggingService.LogBusinessEvent("FileDeleted", new { 
                fileName, 
                containerName,
                WasDeleted = response.Value 
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error deleting file", ex, new { fileName, containerName });
            return Result.Fail("Failed to delete file", "FILE_DELETE_ERROR");
        }
    }

    public async Task<Result> DeleteMultipleFilesAsync(
        IEnumerable<string> fileNames, 
        string containerName,
        CancellationToken cancellationToken = default)
    {
        var results = new List<Result>();
        
        foreach (var fileName in fileNames)
        {
            var result = await DeleteFileAsync(fileName, containerName, cancellationToken);
            results.Add(result);
        }

        var failedDeletions = results.Where(r => !r.Success).ToList();
        if (failedDeletions.Any())
        {
            return Result.Fail($"Failed to delete {failedDeletions.Count} files", "MULTIPLE_FILE_DELETE_ERROR");
        }

        return Result.Ok();
    }

    public async Task<Result<string>> GetFileUrlAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Check if file exists
            var exists = await blobClient.ExistsAsync(cancellationToken);
            if (!exists.Value)
            {
                return Result.Fail<string>("File not found", "FILE_NOT_FOUND");
            }

            return Result.Ok(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting file URL", ex, new { fileName, containerName });
            return Result.Fail<string>("Failed to get file URL", "FILE_URL_ERROR");
        }
    }

    public async Task<Result<bool>> FileExistsAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.ExistsAsync(cancellationToken);
            return Result.Ok(response.Value);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error checking file existence", ex, new { fileName, containerName });
            return Result.Fail<bool>("Failed to check file existence", "FILE_EXISTS_ERROR");
        }
    }

    public async Task<Result<FileMetadata>> GetFileMetadataAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClientAsync(containerName, cancellationToken);
            var blobClient = containerClient.GetBlobClient(fileName);

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            var metadata = new FileMetadata(
                fileName,
                properties.Value.ContentLength,
                properties.Value.ContentType,
                properties.Value.CreatedOn.UtcDateTime,
                properties.Value.LastModified.UtcDateTime,
                properties.Value.ETag.ToString(),
                properties.Value.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            return Result.Ok(metadata);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error getting file metadata", ex, new { fileName, containerName });
            return Result.Fail<FileMetadata>("Failed to get file metadata", "FILE_METADATA_ERROR");
        }
    }

    public async Task<Result<FileUploadResponse>> GenerateThumbnailAsync(
        FileUploadRequest request,
        int thumbnailWidth = 300,
        int thumbnailHeight = 300,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement when ImageSharp is available
        return Result.Fail<FileUploadResponse>("Thumbnail generation not implemented yet", "NOT_IMPLEMENTED");
    }

    public async Task<Result<FileUploadResponse>> CompressImageAsync(
        FileUploadRequest request,
        int qualityPercentage = 80,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement when ImageSharp is available
        return Result.Fail<FileUploadResponse>("Image compression not implemented yet", "NOT_IMPLEMENTED");
    }

    public Task<Result> ValidateFileAsync(
        FileUploadRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check file size
            if (request.FileContent.Length > _uploadSettings.MaxFileSizeBytes)
            {
                return Task.FromResult(Result.Fail($"File size exceeds maximum allowed size of {_uploadSettings.MaxFileSizeBytes} bytes", "FILE_SIZE_EXCEEDED"));
            }

            // Check file extension
            var fileExtension = Path.GetExtension(request.FileName).ToLowerInvariant().TrimStart('.');
            if (!_uploadSettings.AllowedExtensions.Contains(fileExtension))
            {
                return Task.FromResult(Result.Fail($"File extension '{fileExtension}' is not allowed", "INVALID_FILE_EXTENSION"));
            }

            // Check MIME type
            if (!_uploadSettings.AllowedMimeTypes.Contains(request.ContentType))
            {
                return Task.FromResult(Result.Fail($"Content type '{request.ContentType}' is not allowed", "INVALID_CONTENT_TYPE"));
            }

            // TODO: Add image validation when ImageSharp is available

            return Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error validating file", ex, new { request.FileName });
            return Task.FromResult(Result.Fail("Failed to validate file", "FILE_VALIDATION_ERROR"));
        }
    }

    #region Helper Methods

    private async Task<BlobContainerClient> GetContainerClientAsync(string containerName, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        
        return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
    }

    private static bool IsImageFile(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
