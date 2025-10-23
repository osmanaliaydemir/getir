using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Abstractions;

/// <summary>
/// Azure Blob Storage işlemleri için dosya storage servisi interface'i
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Blob storage'a bir dosya yükle
    /// </summary>
    Task<Result<FileUploadResponse>> UploadFileAsync(
        FileUploadRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Blob storage'a birden fazla dosya yükle
    /// </summary>
    Task<Result<IEnumerable<FileUploadResponse>>> UploadMultipleFilesAsync(
        IEnumerable<FileUploadRequest> requests, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Blob storage'dan bir dosya sil
    /// </summary>
    Task<Result> DeleteFileAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Blob storage'dan birden fazla dosya sil
    /// </summary>
    Task<Result> DeleteMultipleFilesAsync(
        IEnumerable<string> fileNames, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosya indirme URL'i al
    /// </summary>
    Task<Result<string>> GetFileUrlAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Blob storage'da dosya olup olmadığını kontrol et
    /// </summary>
    Task<Result<bool>> FileExistsAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dosya meta verilerini al
    /// </summary>
    Task<Result<FileMetadata>> GetFileMetadataAsync(
        string fileName, 
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resim için thumbnail oluştur
    /// </summary>
    Task<Result<FileUploadResponse>> GenerateThumbnailAsync(
        FileUploadRequest request,
        int thumbnailWidth = 300,
        int thumbnailHeight = 300,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resim dosyasını sıkıştır
    /// </summary>
    Task<Result<FileUploadResponse>> CompressImageAsync(
        FileUploadRequest request,
        int qualityPercentage = 80,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yüklemeden önce dosya doğrulama
    /// </summary>
    Task<Result> ValidateFileAsync(
        FileUploadRequest request, 
        CancellationToken cancellationToken = default);
}
