namespace Getir.Domain.Enums;

/// <summary>
/// Gözden geçirme türleri (merchant, courier, product)
/// </summary>
public enum ReviewType
{
    /// <summary>
    /// Gözden geçirme için merchant/restaurant
    /// </summary>
    MerchantReview = 0,

    /// <summary>
    /// Gözden geçirme için courier/delivery
    /// </summary>
    CourierReview = 1,

    /// <summary>
    /// Gözden geçirme için belirli bir ürün
    /// </summary>
    ProductReview = 2
}

