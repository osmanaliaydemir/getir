using Getir.Application.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Base controller class that provides common functionality for all controllers
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Converts Result<T> to IActionResult with standardized ApiResponse
    /// </summary>
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.Success)
        {
            return Ok(ApiResponse<T>.Success(result.Value!));
        }
        else
        {
            return BadRequest(ApiResponse<T>.Fail(result.Error ?? "Operation failed", result.ErrorCode));
        }
    }

    /// <summary>
    /// Converts Result to IActionResult with standardized ApiResponse
    /// </summary>
    protected IActionResult ToActionResult(Result result)
    {
        if (result.Success)
        {
            return Ok(ApiResponse.Success());
        }
        else
        {
            return BadRequest(ApiResponse.Fail(result.Error ?? "Operation failed", result.ErrorCode));
        }
    }

    /// <summary>
    /// Gets the current user ID from claims
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Gets the current user ID or returns unauthorized
    /// </summary>
    protected IActionResult? GetCurrentUserIdOrUnauthorized(out Guid userId)
    {
        userId = Guid.Empty;
        var currentUserId = GetCurrentUserId();
        
        if (currentUserId == null)
        {
            return Unauthorized("User not authenticated");
        }
        
        userId = currentUserId.Value;
        return null;
    }

    /// <summary>
    /// Gets the current user role from claims
    /// </summary>
    protected string? GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
        return roleClaim?.Value;
    }

    /// <summary>
    /// Handles validation errors with standardized response
    /// </summary>
    protected IActionResult? HandleValidationErrors()
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse.Fail(errors, ErrorCodes.VALIDATION_ERROR));
        }
        return null;
    }

    /// <summary>
    /// Returns standardized success response with data
    /// </summary>
    protected IActionResult SuccessResponse<T>(T data, Dictionary<string, object>? metadata = null)
    {
        return Ok(ApiResponse<T>.Success(data, metadata));
    }

    /// <summary>
    /// Returns standardized success response without data
    /// </summary>
    protected IActionResult SuccessResponse()
    {
        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Returns standardized error response
    /// </summary>
    protected IActionResult ErrorResponse(string error, string? errorCode = null, int statusCode = 400)
    {
        var response = ApiResponse.Fail(error, errorCode);
        return StatusCode(statusCode, response);
    }
}
