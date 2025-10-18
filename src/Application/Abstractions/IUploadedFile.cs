namespace Getir.Application.Abstractions;

/// <summary>
/// Framework-agnostic file upload abstraction
/// Replaces IFormFile to keep Application layer independent of ASP.NET Core
/// </summary>
public interface IUploadedFile
{
    /// <summary>
    /// Gets the file name from the Content-Disposition header
    /// </summary>
    string FileName { get; }
    
    /// <summary>
    /// Gets the MIME content type from the Content-Type header
    /// </summary>
    string ContentType { get; }
    
    /// <summary>
    /// Gets the file length in bytes
    /// </summary>
    long Length { get; }
    
    /// <summary>
    /// Opens the request stream for reading the uploaded file
    /// </summary>
    Stream OpenReadStream();
    
    /// <summary>
    /// Copies the file contents to a target stream
    /// </summary>
    Task CopyToAsync(Stream target, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reads the file contents as a byte array
    /// </summary>
    Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default);
}

