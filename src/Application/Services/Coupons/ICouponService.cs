using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Coupons;

/// <summary>
/// Kupon servisi interface'i: kupon doğrulama, oluşturma, listeleme ve kullanım kaydı işlemleri.
/// </summary>
public interface ICouponService
{
    /// <summary>Kupon kodunu doğrular ve indirim miktarını hesaplar.</summary>
    Task<Result<CouponValidationResponse>> ValidateCouponAsync(Guid userId, ValidateCouponRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Yeni kupon oluşturur.</summary>
    Task<Result<CouponResponse>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>Kuponları sayfalama ile listeler.</summary>
    Task<Result<PagedResult<CouponResponse>>> GetCouponsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    
    /// <summary>Kupon kullanımını kaydeder ve kullanım sayacını arttırır.</summary>
    Task<Result> RecordCouponUsageAsync(Guid couponId, Guid userId, Guid orderId, decimal discountAmount, CancellationToken cancellationToken = default);
}
