namespace Getir.Application.Abstractions;

/// <summary>
/// Adapter interface for converting framework-specific file upload types
/// to framework-agnostic IUploadedFile
/// </summary>
public interface IFileUploadAdapter
{
    /// <summary>
    /// Converts a framework-specific file upload object to IUploadedFile
    /// </summary>
    IUploadedFile Adapt(object frameworkFile);
    
    /// <summary>
    /// Converts multiple framework-specific file upload objects to IUploadedFile array
    /// </summary>
    IUploadedFile[] AdaptMultiple(object[] frameworkFiles);
}

