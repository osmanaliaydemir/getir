using Getir.Application.Common;
using Microsoft.AspNetCore.Mvc;

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
    /// Converts Result<T> to IActionResult
    /// </summary>
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        return result.Success 
            ? Ok(result.Value) 
            : BadRequest(result.Error);
    }

    /// <summary>
    /// Converts Result to IActionResult
    /// </summary>
    protected IActionResult ToActionResult(Result result)
    {
        return result.Success 
            ? Ok() 
            : BadRequest(result.Error);
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
    /// Handles validation errors
    /// </summary>
    protected IActionResult? HandleValidationErrors()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return null;
    }
}
