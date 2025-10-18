namespace Getir.Domain.Enums;

/// <summary>
/// Type of review (merchant, courier, product)
/// </summary>
public enum ReviewType
{
    /// <summary>
    /// Review for merchant/restaurant
    /// </summary>
    MerchantReview = 0,

    /// <summary>
    /// Review for courier/delivery
    /// </summary>
    CourierReview = 1,

    /// <summary>
    /// Review for specific product
    /// </summary>
    ProductReview = 2
}

