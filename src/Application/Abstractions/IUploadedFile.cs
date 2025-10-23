namespace Getir.Application.Abstractions;

/// <summary>
/// Dosya yükleme abstraction
/// </summary>
public interface IUploadedFile
{
    /// <summary>
    /// Dosya adını al
    /// </summary>
    string FileName { get; }
    
    /// <summary>
    /// MIME içerik tipini al
    /// </summary>
    string ContentType { get; }
    
    /// <summary>
    /// Dosya boyutunu al
    /// </summary>
    long Length { get; }
    
    /// <summary>
    /// Dosyayı okumak için istek akışını aç
    /// </summary>
    Stream OpenReadStream();
    
    /// <summary>
    /// Dosyayı hedef akışa kopyala
    /// </summary>
    Task CopyToAsync(Stream target, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Dosyayı byte array olarak oku
    /// </summary>
    Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default);
}

