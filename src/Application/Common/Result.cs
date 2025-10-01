namespace Getir.Application.Common;

public class Result
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }

    public static Result Ok() => new() { Success = true };
    
    public static Result Fail(string error, string? errorCode = null) 
        => new() { Success = false, Error = error, ErrorCode = errorCode };

    public static Result<T> Ok<T>(T value) => new() { Success = true, Value = value };
    
    public static Result<T> Fail<T>(string error, string? errorCode = null) 
        => new() { Success = false, Error = error, ErrorCode = errorCode };
}

public class Result<T> : Result
{
    public T? Value { get; set; }
}
