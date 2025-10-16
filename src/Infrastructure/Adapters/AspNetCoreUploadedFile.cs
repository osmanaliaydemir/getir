using Getir.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Getir.Infrastructure.Adapters;

/// <summary>
/// ASP.NET Core implementation of IUploadedFile
/// Adapts IFormFile to framework-agnostic interface
/// </summary>
public class AspNetCoreUploadedFile : IUploadedFile
{
    private readonly IFormFile _formFile;

    public AspNetCoreUploadedFile(IFormFile formFile)
    {
        _formFile = formFile ?? throw new ArgumentNullException(nameof(formFile));
    }

    public string FileName => _formFile.FileName;

    public string ContentType => _formFile.ContentType;

    public long Length => _formFile.Length;

    public Stream OpenReadStream()
    {
        return _formFile.OpenReadStream();
    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        return _formFile.CopyToAsync(target, cancellationToken);
    }

    public async Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await _formFile.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }
}

