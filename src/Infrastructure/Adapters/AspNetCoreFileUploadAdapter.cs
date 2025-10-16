using Getir.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Getir.Infrastructure.Adapters;

/// <summary>
/// ASP.NET Core implementation of IFileUploadAdapter
/// Converts IFormFile to IUploadedFile
/// </summary>
public class AspNetCoreFileUploadAdapter : IFileUploadAdapter
{
    public IUploadedFile Adapt(object frameworkFile)
    {
        if (frameworkFile is not IFormFile formFile)
        {
            throw new ArgumentException(
                $"Expected IFormFile but got {frameworkFile?.GetType().Name ?? "null"}", 
                nameof(frameworkFile));
        }

        return new AspNetCoreUploadedFile(formFile);
    }

    public IUploadedFile[] AdaptMultiple(object[] frameworkFiles)
    {
        if (frameworkFiles == null || frameworkFiles.Length == 0)
        {
            return Array.Empty<IUploadedFile>();
        }

        var result = new IUploadedFile[frameworkFiles.Length];
        
        for (int i = 0; i < frameworkFiles.Length; i++)
        {
            result[i] = Adapt(frameworkFiles[i]);
        }

        return result;
    }
}

