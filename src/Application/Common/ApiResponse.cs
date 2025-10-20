using System.Text.Json.Serialization;

namespace Getir.Application.Common;

/// <summary>
/// Standardized API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// The data payload (null if operation failed)
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Alias for mobile compatibility (JSON'a serialize edilmez)
    /// </summary>
    [JsonIgnore]
    public T? Value => Data;

    /// <summary>
    /// Error message (null if operation succeeded)
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Error code for client-side handling
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Additional metadata (pagination, timestamps, etc.)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Create a successful response
    /// </summary>
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
    /// Create an error response
    /// </summary>
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
/// Non-generic API response for operations without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
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

