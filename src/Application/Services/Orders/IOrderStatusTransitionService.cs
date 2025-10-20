using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Orders;

/// <summary>
/// Sipariş durum geçiş servisi: validasyon, audit logging, rollback, geçiş geçmişi.
/// </summary>
public interface IOrderStatusTransitionService
{
    /// <summary>Sipariş durumunu değiştirir (validasyon, audit log, bildirim).</summary>
    Task<Result> ChangeOrderStatusAsync(ChangeOrderStatusRequest request, Guid userId, string userRole, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    /// <summary>Son durum değişikliğini geri alır (rollback log kaydeder).</summary>
    Task<Result> RollbackLastStatusChangeAsync(Guid orderId, Guid userId, string userRole, string? reason = null, CancellationToken cancellationToken = default);
    /// <summary>Sipariş durum geçiş geçmişini getirir.</summary>
    Task<Result<List<OrderStatusTransitionLogResponse>>> GetOrderStatusHistoryAsync(Guid orderId, CancellationToken cancellationToken = default);
    /// <summary>Sipariş için mevcut durum geçişlerini getirir (role bazlı).</summary>
    Task<Result<List<OrderStatusTransitionResponse>>> GetAvailableTransitionsAsync(Guid orderId, Guid userId, string userRole, CancellationToken cancellationToken = default);
}
