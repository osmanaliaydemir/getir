using Getir.Application.Common;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Orders;

/// <summary>
/// Sipariş durum validasyon servisi: durum geçiş kuralları, yetki kontrolü, business rule validasyonu.
/// </summary>
public interface IOrderStatusValidatorService
{
    /// <summary>Sipariş durum geçişinin izinli olup olmadığını kontrol eder (geçiş kuralı, yetki, business rule).</summary>
    Task<Result> ValidateStatusTransitionAsync(Guid orderId, OrderStatus fromStatus, OrderStatus toStatus, Guid userId, string userRole, CancellationToken cancellationToken = default);
    /// <summary>Sipariş için geçerli sonraki durumları getirir (role bazlı).</summary>
    Task<Result<List<OrderStatus>>> GetValidNextStatusesAsync(Guid orderId, Guid userId, string userRole, CancellationToken cancellationToken = default);
    /// <summary>Kullanıcının sipariş durumunu değiştirme yetkisi olup olmadığını kontrol eder (role bazlı).</summary>
    Task<Result> ValidateUserPermissionAsync(Guid orderId, OrderStatus toStatus, Guid userId, string userRole, CancellationToken cancellationToken = default);
    /// <summary>Durum geçişi için gerekli verileri getirir.</summary>
    Task<Result<List<string>>> GetRequiredTransitionDataAsync(OrderStatus fromStatus, OrderStatus toStatus, CancellationToken cancellationToken = default);
}
