namespace Getir.Application.Common;

/// <summary>
/// İşlem sonucu için base sınıf
/// </summary>
public class Result
{
    /// <summary>
    /// İşlemin başarılı olup olmadığını belirtir
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Hata mesajı
    /// </summary>
    public string? Error { get; set; }
    
    /// <summary>
    /// Hata kodu
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Başarılı sonuç oluştur
    /// </summary>
    /// <returns>Başarılı Result</returns>
    public static Result Ok() => new() { Success = true };
    
    /// <summary>
    /// Başarısız sonuç oluştur
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Başarısız Result</returns>
    public static Result Fail(string error, string? errorCode = null) 
        => new() { Success = false, Error = error, ErrorCode = errorCode };

    /// <summary>
    /// Başarılı sonuç oluştur (generic)
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="value">Veri değeri</param>
    /// <returns>Başarılı Result<T></returns>
    public static Result<T> Ok<T>(T value) => new() { Success = true, Value = value };
    
    /// <summary>
    /// Başarısız sonuç oluştur (generic)
    /// </summary>
    /// <typeparam name="T">Veri tipi</typeparam>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Başarısız Result<T></returns>
    public static Result<T> Fail<T>(string error, string? errorCode = null) 
        => new() { Success = false, Error = error, ErrorCode = errorCode };
}

/// <summary>
/// İşlem sonucu için generic sınıf
/// </summary>
/// <typeparam name="T">Veri tipi</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Veri değeri
    /// </summary>
    public T? Value { get; set; }
    
    /// <summary>
    /// Veri değeri (alias)
    /// </summary>
    public T? Data => Value;
}
