using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// File storage service interface for Azure Blob Storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file to blob storage
    /// </summary>
    Task<Result<FileUploadResponse>> UploadFileAsync(
        FileUploadRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload multiple files to blob storage
    /// </summary>
    Task<Result<IEnumerable<FileUploadResponse>>> UploadMultipleFilesAsync(
        IEnumerable<FileUploadRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file from blob storage
    /// </summary>
    Task<Result> DeleteFileAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete multiple files from blob storage
    /// </summary>
    Task<Result> DeleteMultipleFilesAsync(
        IEnumerable<string> fileNames, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file download URL
    /// </summary>
    Task<Result<string>> GetFileUrlAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists in blob storage
    /// </summary>
    Task<Result<bool>> FileExistsAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file metadata
    /// </summary>
    Task<Result<FileMetadata>> GetFileMetadataAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate thumbnail for image
    /// </summary>
    Task<Result<FileUploadResponse>> GenerateThumbnailAsync(
        FileUploadRequest request,
        int thumbnailWidth = 300,
        int thumbnailHeight = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compress image file
    /// </summary>
    Task<Result<FileUploadResponse>> CompressImageAsync(
        FileUploadRequest request,
        int qualityPercentage = 80,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate file before upload
    /// </summary>
    Task<Result> ValidateFileAsync(
        FileUploadRequest request, 
        CancellationToken cancellationToken = default);
}
