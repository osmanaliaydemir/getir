using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.Coupons;

public interface ICouponService
{
    Task<Result<CouponValidationResponse>> ValidateCouponAsync(Guid userId, ValidateCouponRequest request, CancellationToken cancellationToken = default);
    Task<Result<CouponResponse>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CouponResponse>>> GetCouponsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result> RecordCouponUsageAsync(Guid couponId, Guid userId, Guid orderId, decimal discountAmount, CancellationToken cancellationToken = default);
}
