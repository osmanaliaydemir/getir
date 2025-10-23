namespace Getir.Application.Abstractions;

/// <summary>
/// Framework-specific dosya yükleme tiplerini framework-agnostic IUploadedFile'a dönüştürme için adapter interface'i
/// </summary>
public interface IFileUploadAdapter
{
    /// <summary>
    /// Framework-specific dosya yükleme objesini IUploadedFile'a dönüştür
    /// </summary>
    IUploadedFile Adapt(object frameworkFile);
    
    /// <summary>
    /// Framework-specific dosya yükleme objelerini IUploadedFile dizisine dönüştür
    /// </summary>
    IUploadedFile[] AdaptMultiple(object[] frameworkFiles);
}

