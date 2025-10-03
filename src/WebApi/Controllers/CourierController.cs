using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Couriers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Courier controller for managing courier operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Courier Panel")]
[Authorize]
public class CourierController : BaseController
{
    private readonly ICourierService _courierService;

    public CourierController(ICourierService courierService)
    {
        _courierService = courierService;
    }

    /// <summary>
    /// Get courier dashboard
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Courier dashboard data</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(CourierDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierDashboard(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.GetCourierDashboardAsync(courierId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get courier statistics
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Courier statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(CourierStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierStats(CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.GetCourierStatsAsync(courierId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get courier earnings
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Courier earnings</returns>
    [HttpGet("earnings")]
    [ProducesResponseType(typeof(CourierEarningsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierEarnings(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.GetCourierEarningsAsync(courierId, startDate, endDate, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get assigned orders
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged assigned orders</returns>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(PagedResult<CourierOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCourierOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var query = new PaginationQuery { Page = page, PageSize = pageSize };
        var result = await _courierService.GetAssignedOrdersAsync(courierId, query, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Accept order
    /// </summary>
    /// <param name="request">Accept order request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("orders/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AcceptOrder(
        [FromBody] AcceptOrderRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.AcceptOrderAsync(courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Start delivery
    /// </summary>
    /// <param name="request">Start delivery request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("orders/start-delivery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartDelivery(
        [FromBody] StartDeliveryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.StartDeliveryAsync(courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Complete delivery
    /// </summary>
    /// <param name="request">Complete delivery request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("orders/complete-delivery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteDelivery(
        [FromBody] CompleteDeliveryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.CompleteDeliveryAsync(courierId, request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Update courier location
    /// </summary>
    /// <param name="request">Update location request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPut("location")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCourierLocation(
        [FromBody] UpdateCourierLocationRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var locationRequest = new CourierLocationUpdateRequest(
            request.Latitude,
            request.Longitude);
        var result = await _courierService.UpdateLocationAsync(courierId, locationRequest, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Set courier availability
    /// </summary>
    /// <param name="request">Set availability request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPut("availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetCourierAvailability(
        [FromBody] SetAvailabilityRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var courierId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _courierService.SetAvailabilityAsync(courierId, request, ct);
        return ToActionResult(result);
    }
}

/// <summary>
/// Admin courier controller for admin courier management
/// </summary>
[ApiController]
[Route("api/v1/admin/[controller]")]
[Tags("Admin - Courier Management")]
[Authorize(Policy = "Admin")]
public class AdminCourierController : BaseController
{
    private readonly ICourierService _courierService;

    public AdminCourierController(ICourierService courierService)
    {
        _courierService = courierService;
    }

    /// <summary>
    /// Assign order to courier
    /// </summary>
    /// <param name="request">Assign order request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Assignment response</returns>
    [HttpPost("assign-order")]
    [ProducesResponseType(typeof(CourierAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignOrder(
        [FromBody] AssignOrderRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _courierService.AssignOrderAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Find nearest couriers
    /// </summary>
    /// <param name="request">Find nearest couriers request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Nearest couriers</returns>
    [HttpPost("find-nearest")]
    [ProducesResponseType(typeof(FindNearestCouriersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FindNearestCouriers(
        [FromBody] FindNearestCouriersRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var result = await _courierService.FindNearestCouriersAsync(request, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get top performing couriers
    /// </summary>
    /// <param name="count">Number of couriers to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Top performers</returns>
    [HttpGet("top-performers")]
    [ProducesResponseType(typeof(List<CourierPerformanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopPerformers(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        var result = await _courierService.GetTopPerformersAsync(count, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get courier earnings detail
    /// </summary>
    /// <param name="courierId">Courier ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Earnings detail</returns>
    [HttpGet("earnings-detail")]
    [ProducesResponseType(typeof(CourierEarningsDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourierEarningsDetail(
        [FromQuery] Guid courierId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken ct = default)
    {
        var query = new CourierEarningsQuery(courierId, startDate, endDate);
        var result = await _courierService.GetEarningsDetailAsync(query, ct);
        return ToActionResult(result);
    }
}
