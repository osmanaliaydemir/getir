using Getir.Application.Common;

namespace Getir.Application.DTO;

/// <summary>
/// File upload request
/// </summary>
public record FileUploadRequest(
    string FileName,
    byte[] FileContent,
    string ContentType,
    string ContainerName,
    FileCategory Category,
    string? Description = null,
    Guid? RelatedEntityId = null,
    string? RelatedEntityType = null);

/// <summary>
/// File upload response
/// </summary>
public record FileUploadResponse(
    string FileName,
    string BlobUrl,
    string ContainerName,
    long FileSizeBytes,
    string ContentType,
    DateTime UploadedAt,
    string? ThumbnailUrl = null);

/// <summary>
/// File metadata
/// </summary>
public record FileMetadata(
    string FileName,
    long FileSizeBytes,
    string ContentType,
    DateTime CreatedAt,
    DateTime LastModified,
    string ETag,
    Dictionary<string, string> Metadata);

/// <summary>
/// File deletion request
/// </summary>
public record FileDeletionRequest(
    string FileName,
    string ContainerName);

/// <summary>
/// Multiple file deletion request
/// </summary>
public record MultipleFileDeletionRequest(
    IEnumerable<string> FileNames,
    string ContainerName);

/// <summary>
/// File validation result
/// </summary>
public record FileValidationResult(
    bool IsValid,
    string? ErrorMessage,
    long FileSizeBytes,
    string ContentType,
    string FileExtension);

/// <summary>
/// File upload settings
/// </summary>
public record FileUploadSettings(
    long MaxFileSizeBytes,
    IEnumerable<string> AllowedExtensions,
    IEnumerable<string> AllowedMimeTypes,
    bool GenerateThumbnail,
    bool CompressImages,
    int ThumbnailWidth,
    int ThumbnailHeight,
    int ImageQualityPercentage);

/// <summary>
/// File category enum
/// </summary>
public enum FileCategory
{
    MerchantLogo = 1,
    MerchantCover = 2,
    ProductImage = 3,
    UserAvatar = 4,
    Document = 5,
    CategoryIcon = 6
}

/// <summary>
/// File container names
/// </summary>
public static class FileContainers
{
    public const string MerchantLogos = "merchant-logos";
    public const string MerchantCovers = "merchant-covers";
    public const string ProductImages = "product-images";
    public const string UserAvatars = "user-avatars";
    public const string Documents = "documents";
    public const string CategoryIcons = "category-icons";
    public const string Thumbnails = "thumbnails";
}

/// <summary>
/// File upload statistics
/// </summary>
public record FileUploadStats(
    long TotalFiles,
    long TotalSizeBytes,
    int FilesByCategory,
    DateTime LastUploadDate);

/// <summary>
/// CDN upload request
/// </summary>
public record CdnUploadRequest(
    string FileName,
    byte[] FileContent,
    string ContentType,
    string ContainerName,
    Dictionary<string, string>? Metadata = null);

/// <summary>
/// CDN upload response
/// </summary>
public record CdnUploadResponse(
    string CdnUrl,
    string OriginalUrl,
    string ContainerName,
    long FileSizeBytes,
    DateTime UploadedAt);

/// <summary>
/// CDN statistics
/// </summary>
public record CdnStats(
    long TotalFiles,
    long TotalSizeBytes,
    long CacheHitRate,
    DateTime LastActivityDate);
