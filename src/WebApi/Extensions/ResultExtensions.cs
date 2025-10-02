using Getir.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Result'ı HTTP response'a dönüştürür
    /// </summary>
    public static IResult ToApiResult<T>(this Result<T> result)
    {
        if (result.Success)
        {
            return Results.Ok(result.Value);
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" or "NOT_FOUND_ORDER" or "NOT_FOUND_PAYMENT" or "NOT_FOUND_MERCHANT" or "NOT_FOUND_COURIER" => Results.NotFound(new { error = result.Error, code = result.ErrorCode }),
            "UNAUTHORIZED" => Results.Unauthorized(),
            "FORBIDDEN" => Results.Forbid(),
            "VALIDATION_ERROR" => Results.BadRequest(new { error = result.Error, code = result.ErrorCode }),
            "CONFLICT" => Results.Conflict(new { error = result.Error, code = result.ErrorCode }),
            _ => Results.BadRequest(new { error = result.Error, code = result.ErrorCode })
        };
    }

    /// <summary>
    /// Result'ı HTTP response'a dönüştürür (void result için)
    /// </summary>
    public static IResult ToApiResult(this Result result)
    {
        if (result.Success)
        {
            return Results.Ok();
        }

        return result.ErrorCode switch
        {
            "NOT_FOUND" or "NOT_FOUND_ORDER" or "NOT_FOUND_PAYMENT" or "NOT_FOUND_MERCHANT" or "NOT_FOUND_COURIER" => Results.NotFound(new { error = result.Error, code = result.ErrorCode }),
            "UNAUTHORIZED" => Results.Unauthorized(),
            "FORBIDDEN" => Results.Forbid(),
            "VALIDATION_ERROR" => Results.BadRequest(new { error = result.Error, code = result.ErrorCode }),
            "CONFLICT" => Results.Conflict(new { error = result.Error, code = result.ErrorCode }),
            _ => Results.BadRequest(new { error = result.Error, code = result.ErrorCode })
        };
    }
}
