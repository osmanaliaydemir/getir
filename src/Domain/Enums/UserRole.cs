namespace Getir.Domain.Enums;

/// <summary>
/// Kullanıcı rolleri - Sistem genelinde yetkilendirme için kullanılır
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Müşteri - Sipariş verebilir, sepet yönetimi yapabilir (Default role)
    /// </summary>
    Customer = 1,

    /// <summary>
    /// Merchant Sahibi - Kendi işletmesini, ürünlerini ve siparişlerini yönetebilir
    /// </summary>
    MerchantOwner = 2,

    /// <summary>
    /// Kurye - Sipariş teslimatı yapabilir, konum güncellemesi yapabilir
    /// </summary>
    Courier = 3,

    /// <summary>
    /// Admin - Tüm sistemi yönetir, merchant onay/red, kullanıcı yönetimi
    /// </summary>
    Admin = 4
}

