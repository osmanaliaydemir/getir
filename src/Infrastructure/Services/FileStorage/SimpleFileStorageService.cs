using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Microsoft.Extensions.Options;

namespace Getir.Infrastructure.Services.FileStorage;

/// <summary>
/// Simple file storage service implementation for development/testing
/// In production, this should be replaced with Azure Blob Storage or similar
/// </summary>
public class SimpleFileStorageService : IFileStorageService
{
    private readonly ILoggingService _loggingService;
    private readonly FileUploadSettings _uploadSettings;
    private readonly string _uploadPath;

    public SimpleFileStorageService(
        ILoggingService loggingService,
        IOptions<FileUploadSettings> uploadSettings)
    {
        _loggingService = loggingService;
        _uploadSettings = uploadSettings.Value;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        // Ensure upload directory exists
        Directory.CreateDirectory(_uploadPath);
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

            // Create container directory
            var containerPath = Path.Combine(_uploadPath, request.ContainerName);
            Directory.CreateDirectory(containerPath);

            // Generate unique filename
            var uniqueFileName = GenerateUniqueFileName(request.FileName);
            var filePath = Path.Combine(containerPath, uniqueFileName);

            // Write file
            await File.WriteAllBytesAsync(filePath, request.FileContent, cancellationToken);

            // Generate file URL (in production, this would be a CDN URL)
            var fileUrl = $"/uploads/{request.ContainerName}/{uniqueFileName}";

            var response = new FileUploadResponse(
                uniqueFileName,
                fileUrl,
                request.ContainerName,
                request.FileContent.Length,
                request.ContentType,
                DateTime.UtcNow);

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
            var filePath = Path.Combine(_uploadPath, containerName, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _loggingService.LogBusinessEvent("FileDeleted", new { 
                fileName, 
                containerName
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
            var filePath = Path.Combine(_uploadPath, containerName, fileName);
            
            if (!File.Exists(filePath))
            {
                return Result.Fail<string>("File not found", "FILE_NOT_FOUND");
            }

            var fileUrl = $"/uploads/{containerName}/{fileName}";
            return Result.Ok(fileUrl);
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
            var filePath = Path.Combine(_uploadPath, containerName, fileName);
            var exists = File.Exists(filePath);
            return Result.Ok(exists);
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
            var filePath = Path.Combine(_uploadPath, containerName, fileName);
            
            if (!File.Exists(filePath))
            {
                return Result.Fail<FileMetadata>("File not found", "FILE_NOT_FOUND");
            }

            var fileInfo = new FileInfo(filePath);
            var metadata = new FileMetadata(
                fileName,
                fileInfo.Length,
                GetContentType(fileName),
                fileInfo.CreationTimeUtc,
                fileInfo.LastWriteTimeUtc,
                fileInfo.LastWriteTimeUtc.Ticks.ToString(),
                new Dictionary<string, string>());

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
        // TODO: Implement thumbnail generation
        return Result.Fail<FileUploadResponse>("Thumbnail generation not implemented", "NOT_IMPLEMENTED");
    }

    public async Task<Result<FileUploadResponse>> CompressImageAsync(
        FileUploadRequest request,
        int qualityPercentage = 80,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement image compression
        return Result.Fail<FileUploadResponse>("Image compression not implemented", "NOT_IMPLEMENTED");
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

            return Task.FromResult(Result.Ok());
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error validating file", ex, new { request.FileName });
            return Task.FromResult(Result.Fail("Failed to validate file", "FILE_VALIDATION_ERROR"));
        }
    }

    #region Helper Methods

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        
        return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
