namespace Getir.Application.Abstractions;

/// <summary>
/// SignalR notification sender interface
/// </summary>
public interface ISignalRNotificationSender
{
    /// <summary>
    /// Kullanıcıya notification gönder
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="title">Başlık</param>
    /// <param name="message">Mesaj</param>
    /// <param name="type">Tip</param>
    Task SendToUserAsync(Guid userId, string title, string message, string type);
}

/// <summary>
/// SignalR order sender interface
/// </summary>
public interface ISignalROrderSender
{
    /// <summary>
    /// Sipariş durumu güncellendi
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="status">Durum</param>
    /// <param name="message">Mesaj</param>
    Task SendStatusUpdateAsync(Guid orderId, Guid userId, string status, string message);

    /// <summary>
    /// Yeni sipariş merchant'a gönder
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderData">Sipariş verisi</param>
    Task SendNewOrderToMerchantAsync(Guid merchantId, object orderData);

    /// <summary>
    /// Sipariş durumu merchant'a güncellendi
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="orderNumber">Sipariş numarası</param>
    /// <param name="status">Durum</param>
    Task SendOrderStatusChangedToMerchantAsync(Guid merchantId, Guid orderId, string orderNumber, string status);

    /// <summary>
    /// Sipariş iptal edildi merchant'a gönder
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="orderNumber">Sipariş numarası</param>
    /// <param name="reason">Sebep</param>
    Task SendOrderCancelledToMerchantAsync(Guid merchantId, Guid orderId, string orderNumber, string? reason);
}

/// <summary>
/// SignalR courier sender interface
/// </summary>
public interface ISignalRCourierSender
{
    /// <summary>
    /// Konum güncellendi
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="latitude">Enlem</param>
    /// <param name="longitude">Boylam</param>
    Task SendLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude);
}
