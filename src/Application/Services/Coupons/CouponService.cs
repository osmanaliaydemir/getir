using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Coupons;

/// <summary>
/// Kupon servisi: kupon doğrulama, oluşturma, listeleme ve kullanım kaydı işlemleri.
/// </summary>
public class CouponService : BaseService, ICouponService
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    public CouponService(IUnitOfWork unitOfWork, ILogger<CouponService> logger, ILoggingService loggingService, ICacheService cacheService, IBackgroundTaskService backgroundTaskService)
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }
    /// <summary>
    /// Kupon kodunu doğrular: geçerlilik, süre, kullanım limiti, kullanıcı kullanımı ve minimum tutar kontrolü yapar.
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Doğrulama isteği</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Doğrulama yanıtı ve hesaplanan indirim</returns>
    public async Task<Result<CouponValidationResponse>> ValidateCouponAsync(Guid userId, ValidateCouponRequest request, CancellationToken cancellationToken = default)
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

        // Check if user has already used this coupon
        var existingUsage = await _unitOfWork.ReadRepository<CouponUsage>()
            .FirstOrDefaultAsync(cu => cu.CouponId == coupon.Id && cu.UserId == userId, cancellationToken: cancellationToken);

        if (existingUsage != null)
        {
            return Result.Ok(new CouponValidationResponse(false, "You have already used this coupon", 0, request.Code));
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
    /// <summary>
    /// Yeni kupon oluşturur (kod benzersizliği kontrol eder).
    /// </summary>
    /// <param name="request">Kupon oluşturma isteği</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Oluşturulan kupon</returns>
    public async Task<Result<CouponResponse>> CreateCouponAsync(CreateCouponRequest request, CancellationToken cancellationToken = default)
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
    /// <summary>
    /// Aktif kuponları sayfalama ile listeler.
    /// </summary>
    /// <param name="query">Sayfalama sorgusu</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Sayfalanmış kuponlar</returns>
    public async Task<Result<PagedResult<CouponResponse>>> GetCouponsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
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
    /// <summary>
    /// Kupon kullanımını kaydeder ve kuponun kullanım sayacını arttırır.
    /// </summary>
    /// <param name="couponId">Kupon ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="discountAmount">İndirim miktarı</param>
    /// <param name="cancellationToken">İptal belirteci</param>
    /// <returns>Başarı durumu</returns>
    public async Task<Result> RecordCouponUsageAsync(Guid couponId, Guid userId, Guid orderId, decimal discountAmount, CancellationToken cancellationToken = default)
    {
        var couponUsage = new CouponUsage
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            DiscountAmount = discountAmount,
            UsedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CouponUsage>().AddAsync(couponUsage, cancellationToken);

        // Update coupon usage count
        var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(couponId, cancellationToken);
        if (coupon != null)
        {
            coupon.UsageCount++;
            _unitOfWork.Repository<Coupon>().Update(coupon);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
