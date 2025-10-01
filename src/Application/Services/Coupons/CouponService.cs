using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Coupons;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;

    public CouponService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CouponValidationResponse>> ValidateCouponAsync(
        Guid userId,
        ValidateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        var coupon = await _unitOfWork.ReadRepository<Coupon>()
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.IsActive, cancellationToken: cancellationToken);

        if (coupon == null)
        {
            return Result.Ok(new CouponValidationResponse(false, "Invalid coupon code", 0, request.Code));
        }

        if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
        {
            return Result.Ok(new CouponValidationResponse(false, "Coupon has expired or not yet valid", 0, request.Code));
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            return Result.Ok(new CouponValidationResponse(false, "Coupon usage limit reached", 0, request.Code));
        }

        if (request.OrderAmount < coupon.MinimumOrderAmount)
        {
            return Result.Ok(new CouponValidationResponse(
                false,
                $"Minimum order amount is {coupon.MinimumOrderAmount:C}",
                0,
                request.Code));
        }

        decimal discountAmount = 0;
        if (coupon.DiscountType == "Percentage")
        {
            discountAmount = request.OrderAmount * (coupon.DiscountValue / 100);
            if (coupon.MaximumDiscountAmount.HasValue && discountAmount > coupon.MaximumDiscountAmount.Value)
            {
                discountAmount = coupon.MaximumDiscountAmount.Value;
            }
        }
        else if (coupon.DiscountType == "FixedAmount")
        {
            discountAmount = coupon.DiscountValue;
        }

        return Result.Ok(new CouponValidationResponse(true, null, discountAmount, request.Code));
    }

    public async Task<Result<CouponResponse>> CreateCouponAsync(
        CreateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingCoupon = await _unitOfWork.ReadRepository<Coupon>()
            .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken: cancellationToken);

        if (existingCoupon != null)
        {
            return Result.Fail<CouponResponse>("Coupon code already exists", "CONFLICT_COUPON_CODE");
        }

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = request.Code.ToUpper(),
            Title = request.Title,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            MaximumDiscountAmount = request.MaximumDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            UsageCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Coupon>().AddAsync(coupon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CouponResponse(
            coupon.Id,
            coupon.Code,
            coupon.Title,
            coupon.Description,
            coupon.DiscountType,
            coupon.DiscountValue,
            coupon.MinimumOrderAmount,
            coupon.MaximumDiscountAmount,
            coupon.StartDate,
            coupon.EndDate,
            coupon.UsageLimit,
            coupon.UsageCount,
            coupon.IsActive
        );

        return Result.Ok(response);
    }

    public async Task<Result<PagedResult<CouponResponse>>> GetCouponsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var coupons = await _unitOfWork.Repository<Coupon>().GetPagedAsync(
            filter: c => c.IsActive,
            orderBy: c => c.CreatedAt,
            ascending: false,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Coupon>()
            .CountAsync(c => c.IsActive, cancellationToken);

        var response = coupons.Select(c => new CouponResponse(
            c.Id,
            c.Code,
            c.Title,
            c.Description,
            c.DiscountType,
            c.DiscountValue,
            c.MinimumOrderAmount,
            c.MaximumDiscountAmount,
            c.StartDate,
            c.EndDate,
            c.UsageLimit,
            c.UsageCount,
            c.IsActive
        )).ToList();

        var pagedResult = PagedResult<CouponResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }
}
