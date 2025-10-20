using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Orders;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Sipariş durumu geçiş yönetimi için controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Order Status Transitions")]
[Authorize]
public class OrderStatusTransitionController : BaseController
{
    private readonly IOrderStatusTransitionService _transitionService;
    private readonly IOrderStatusValidatorService _validatorService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderStatusTransitionController(
        IOrderStatusTransitionService transitionService,
        IOrderStatusValidatorService validatorService,
        IUnitOfWork unitOfWork)
    {
        _transitionService = transitionService;
        _validatorService = validatorService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Doğrulama ve denetim kaydı ile sipariş durumunu değiştir
    /// </summary>
    /// <param name="request">Durum değiştirme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Durum değişikliğinin sonucu</returns>
    [HttpPost("change-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeOrderStatus(
        [FromBody] ChangeOrderStatusRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _transitionService.ChangeOrderStatusAsync(
            request, userId, userRole, ipAddress, userAgent, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Son durum değişikliğini geri al
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="reason">Geri alma nedeni</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Geri alma sonucu</returns>
    [HttpPost("rollback/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RollbackStatusChange(
        [FromRoute] Guid orderId,
        [FromQuery] string? reason = null,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";

        var result = await _transitionService.RollbackLastStatusChangeAsync(
            orderId, userId, userRole, reason, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş durum geçiş geçmişini getir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Durum geçiş geçmişi</returns>
    [HttpGet("history/{orderId}")]
    [ProducesResponseType(typeof(List<OrderStatusTransitionLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderStatusHistory(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var result = await _transitionService.GetOrderStatusHistoryAsync(orderId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş için kullanılabilir durum geçişlerini getir
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Kullanılabilir geçişler</returns>
    [HttpGet("available-transitions/{orderId}")]
    [ProducesResponseType(typeof(List<OrderStatusTransitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableTransitions(
        [FromRoute] Guid orderId,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";

        var result = await _transitionService.GetAvailableTransitionsAsync(
            orderId, userId, userRole, ct);

        return ToActionResult(result);
    }

    /// <summary>
    /// Sipariş durumu geçişini doğrula
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="newStatus">Doğrulanacak yeni durum</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Doğrulama sonucu</returns>
    [HttpPost("validate/{orderId}")]
    [ProducesResponseType(typeof(OrderStatusValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ValidateStatusTransition(
        [FromRoute] Guid orderId,
        [FromBody] OrderStatus newStatus,
        CancellationToken ct = default)
    {
        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";

        // Get current order status
        var order = await _unitOfWork.ReadRepository<Order>()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: ct);

        if (order == null)
        {
            return BadRequest(new OrderStatusValidationResponse(
                false, "Order not found", "ORDER_NOT_FOUND", new List<string>(), new List<OrderStatusTransitionResponse>()));
        }

        // Validate transition
        var validationResult = await _validatorService.ValidateStatusTransitionAsync(
            orderId, order.Status, newStatus, userId, userRole, ct);

        // Get required data
        var requiredDataResult = await _validatorService.GetRequiredTransitionDataAsync(
            order.Status, newStatus, ct);

        // Get available transitions
        var availableTransitionsResult = await _transitionService.GetAvailableTransitionsAsync(
            orderId, userId, userRole, ct);

        var response = new OrderStatusValidationResponse(
            validationResult.Success,
            validationResult.Error,
            validationResult.ErrorCode,
            requiredDataResult.Success ? requiredDataResult.Value! : new List<string>(),
            availableTransitionsResult.Success ? availableTransitionsResult.Value! : new List<OrderStatusTransitionResponse>());

        return Ok(response);
    }

    /// <summary>
    /// Sipariş durumlarını toplu değiştir
    /// </summary>
    /// <param name="request">Toplu durum değiştirme isteği</param>
    /// <param name="ct">İptal token'ı</param>
    /// <returns>Toplu işlem sonucu</returns>
    [HttpPost("bulk-change")]
    [Authorize(Roles = "Admin,MerchantOwner")]
    [ProducesResponseType(typeof(BulkOrderStatusChangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkChangeOrderStatus(
        [FromBody] BulkOrderStatusChangeRequest request,
        CancellationToken ct = default)
    {
        var validationResult = HandleValidationErrors();
        if (validationResult != null) return validationResult;

        var unauthorizedResult = GetCurrentUserIdOrUnauthorized(out var userId);
        if (unauthorizedResult != null) return unauthorizedResult;

        var userRole = GetCurrentUserRole() ?? "Customer";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var results = new List<OrderStatusChangeResult>();
        int successCount = 0;
        int failureCount = 0;

        foreach (var orderId in request.OrderIds)
        {
            var changeRequest = new ChangeOrderStatusRequest(
                orderId, request.NewStatus, request.Reason, request.Notes);

            var result = await _transitionService.ChangeOrderStatusAsync(
                changeRequest, userId, userRole, ipAddress, userAgent, ct);

            if (result.Success)
            {
                successCount++;
                results.Add(new OrderStatusChangeResult(orderId, true, null, null));
            }
            else
            {
                failureCount++;
                results.Add(new OrderStatusChangeResult(orderId, false, result.Error, result.ErrorCode));
            }
        }

        var response = new BulkOrderStatusChangeResponse(successCount, failureCount, results);
        return Ok(response);
    }
}
