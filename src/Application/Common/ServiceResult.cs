using Microsoft.Extensions.Logging;

namespace Getir.Application.Common;

/// <summary>
/// Servis operasyonları için standardize edilmiş result wrapper
/// </summary>
public static class ServiceResult
{
    /// <summary>
    /// Başarılı operasyon için result oluşturur
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="value">Veri değeri</param>
    /// <returns>Başarılı Result<T></returns>
    public static Result<T> Success<T>(T value) => Result.Ok(value);
    
    /// <summary>
    /// Başarılı operasyon için result oluşturur (void)
    /// </summary>
    /// <returns>Başarılı Result</returns>
    public static Result Success() => Result.Ok();
    
    /// <summary>
    /// Hata için result oluşturur
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Başarısız Result<T></returns>
    public static Result<T> Failure<T>(string error, string errorCode = ErrorCodes.INTERNAL_ERROR) => 
        Result.Fail<T>(error, errorCode);
    
    /// <summary>
    /// Hata için result oluşturur (void)
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Başarısız Result</returns>
    public static Result Failure(string error, string errorCode = ErrorCodes.INTERNAL_ERROR) => 
        Result.Fail(error, errorCode);
    
    /// <summary>
    /// Exception'ı handle eder ve uygun result döndürür
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="ex">Exception</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="operation">İşlem adı</param>
    /// <returns>Hata Result<T></returns>
    public static Result<T> HandleException<T>(Exception ex, ILogger logger, string operation = "Operation")
    {
        logger.LogError(ex, "Exception occurred during {Operation}", operation);
        
        return ex switch
        {
            ArgumentException => Failure<T>(ex.Message, ErrorCodes.BAD_REQUEST),
            InvalidOperationException => Failure<T>(ex.Message, ErrorCodes.BAD_REQUEST),
            UnauthorizedAccessException => Failure<T>("Unauthorized access", ErrorCodes.UNAUTHORIZED),
            NotImplementedException => Failure<T>("Feature not implemented", ErrorCodes.INTERNAL_ERROR),
            TimeoutException => Failure<T>("Operation timed out", ErrorCodes.SERVICE_UNAVAILABLE),
            _ => Failure<T>($"An unexpected error occurred: {ex.Message}", ErrorCodes.INTERNAL_ERROR)
        };
    }
    
    /// <summary>
    /// Exception'ı handle eder ve uygun result döndürür (void)
    /// </summary>
    public static Result HandleException(Exception ex, ILogger logger, string operation = "Operation")
    {
        logger.LogError(ex, "Exception occurred during {Operation}", operation);
        
        return ex switch
        {
            ArgumentException => Failure(ex.Message, ErrorCodes.BAD_REQUEST),
            InvalidOperationException => Failure(ex.Message, ErrorCodes.BAD_REQUEST),
            UnauthorizedAccessException => Failure("Unauthorized access", ErrorCodes.UNAUTHORIZED),
            NotImplementedException => Failure("Feature not implemented", ErrorCodes.INTERNAL_ERROR),
            TimeoutException => Failure("Operation timed out", ErrorCodes.SERVICE_UNAVAILABLE),
            _ => Failure($"An unexpected error occurred: {ex.Message}", ErrorCodes.INTERNAL_ERROR)
        };
    }
}
