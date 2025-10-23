using System.Text.Json.Serialization;

namespace Getir.Application.Common;

/// <summary>
/// Tüm endpoint'ler için standartlaştırılmış API response wrapper'ı
/// </summary>
/// <typeparam name="T">Veri tipi</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// İşlemin başarılı olup olmadığını belirtir
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Veri yükü (işlem başarısızsa null)
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Mobil uyumluluk için alias (JSON'a serialize edilmez)
    /// </summary>
    [JsonIgnore]
    public T? Value => Data;

    /// <summary>
    /// Hata mesajı (işlem başarılıysa null)
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// İstemci tarafı işleme için hata kodu
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Ek metadata (sayfalama, zaman damgaları vb.)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Başarılı response oluştur
    /// </summary>
    /// <param name="data">Response verisi</param>
    /// <param name="metadata">Ek metadata</param>
    /// <returns>Başarılı API response</returns>
    public static ApiResponse<T> Success(T data, Dictionary<string, object>? metadata = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Error = null,
            ErrorCode = null,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Hata response'u oluştur
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Hata API response</returns>
    public static ApiResponse<T> Fail(string error, string? errorCode = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Data = default,
            Error = error,
            ErrorCode = errorCode,
            Metadata = null
        };
    }
}

/// <summary>
/// Veri olmayan işlemler için generic olmayan API response
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Başarılı response oluştur
    /// </summary>
    /// <returns>Başarılı API response</returns>
    public static new ApiResponse Success()
    {
        return new ApiResponse
        {
            IsSuccess = true,
            Data = null,
            Error = null,
            ErrorCode = null
        };
    }

    /// <summary>
    /// Hata response'u oluştur
    /// </summary>
    /// <param name="error">Hata mesajı</param>
    /// <param name="errorCode">Hata kodu</param>
    /// <returns>Hata API response</returns>
    public static new ApiResponse Fail(string error, string? errorCode = null)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Data = null,
            Error = error,
            ErrorCode = errorCode
        };
    }
}

