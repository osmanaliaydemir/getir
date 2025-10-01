using Microsoft.AspNetCore.Http;

namespace Getir.Application.Common;

public static class ResultExtensions
{
    public static IResult ToIResult(this Result result)
    {
        if (result.Success)
        {
            return Results.Ok(new { success = true });
        }

        var statusCode = GetStatusCode(result.ErrorCode);
        
        return Results.Problem(
            detail: result.Error,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = result.ErrorCode
            });
    }

    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.Success)
        {
            return Results.Ok(result.Value);
        }

        var statusCode = GetStatusCode(result.ErrorCode);
        
        return Results.Problem(
            detail: result.Error,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = result.ErrorCode
            });
    }

    private static int GetStatusCode(string? errorCode)
    {
        return errorCode switch
        {
            var code when code?.StartsWith("AUTH_") == true => StatusCodes.Status401Unauthorized,
            var code when code?.StartsWith("FORBIDDEN_") == true => StatusCodes.Status403Forbidden,
            var code when code?.StartsWith("NOT_FOUND_") == true => StatusCodes.Status404NotFound,
            var code when code?.StartsWith("VALIDATION_") == true => StatusCodes.Status400BadRequest,
            var code when code?.StartsWith("CONFLICT_") == true => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
