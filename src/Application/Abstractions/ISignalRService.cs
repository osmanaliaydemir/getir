using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Abstractions;

/// <summary>
/// SignalR service interface
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Kullanıcıya notification gönder
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="title">Başlık</param>
    /// <param name="message">Mesaj</param>
    /// <param name="type">Tip</param>
    Task SendNotificationToUserAsync(Guid userId, string title, string message, string type);
    /// <summary>
    /// Sipariş durumu güncellendi
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="status">Durum</param>
    /// <param name="message">Mesaj</param>
    Task SendOrderStatusUpdateAsync(Guid orderId, Guid userId, string status, string message);
    /// <summary>
    /// Konum güncellendi
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="latitude">Enlem</param>
    /// <param name="longitude">Boylam</param>
    Task SendCourierLocationUpdateAsync(Guid orderId, decimal latitude, decimal longitude);
    
    // Enhanced Order Tracking
    /// <summary>
    /// Sipariş durumu güncellendi
    /// </summary>
    /// <param name="orderEvent">Sipariş event</param>
    Task SendOrderStatusUpdateEventAsync(OrderStatusUpdateEvent orderEvent);
    /// <summary>
    /// Sipariş takip güncellendi
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="tracking">Sipariş takip</param>
    Task SendOrderTrackingUpdateAsync(Guid orderId, OrderTrackingResponse tracking);
    /// <summary>
    /// Sipariş durumu değişti
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="status">Durum</param>
    /// <param name="message">Mesaj</param>
    Task BroadcastOrderStatusChangeAsync(Guid orderId, string status, string message);
    
    // Enhanced Courier Tracking
    /// <summary>
    /// Konum güncellendi
    /// </summary>
    /// <param name="locationEvent">Konum event</param>
    Task SendCourierLocationEventAsync(CourierLocationEvent locationEvent);
    /// <summary>
    /// Kurye takip güncellendi
    /// </summary>
    /// <param name="courierId">Kurye ID</param>
    /// <param name="tracking">Kurye takip</param>
    Task SendCourierTrackingUpdateAsync(Guid courierId, CourierTrackingResponse tracking);
    /// <summary>
    /// Konum güncellendi
    /// </summary>
    /// <param name="courierId">Kurye ID</param>
    /// <param name="latitude">Enlem</param>
    /// <param name="longitude">Boylam</param>
    Task BroadcastCourierLocationAsync(Guid courierId, decimal latitude, decimal longitude);
    
    // Enhanced Notifications
    /// <summary>
    /// Gerçek zamanlı notification gönder
    /// </summary>
    /// <param name="notification">Gerçek zamanlı notification event</param>
    Task SendRealtimeNotificationAsync(RealtimeNotificationEvent notification);
    /// <summary>
    /// Grup için notification gönder
    /// </summary>
    /// <param name="groupName">Grup adı</param>
    /// <param name="notification">Gerçek zamanlı notification event</param>
    Task BroadcastNotificationToGroupAsync(string groupName, RealtimeNotificationEvent notification);
    /// <summary>
    /// Rol için notification gönder
    /// </summary>
    /// <param name="role">Rol</param>
    /// <param name="notification">Gerçek zamanlı notification event</param>
    Task SendNotificationToRoleAsync(string role, RealtimeNotificationEvent notification);
    
    // Dashboard Updates
    /// <summary>
    /// Dashboard güncellendi
    /// </summary>
    /// <param name="eventType">Event tipi</param>
    /// <param name="data">Data</param>
    Task SendDashboardUpdateAsync(string eventType, object data);
    /// <summary>
    /// Sipariş takip güncellendi
    /// </summary>
    Task SendOrderStatsUpdateAsync(OrderRealtimeStats stats);
    /// <summary>
    /// Kurye takip güncellendi
    /// </summary>
    /// <param name="stats">Kurye takip</param>
    Task SendCourierStatsUpdateAsync(CourierRealtimeStats stats);
    
    // Connection Management
    /// <summary>
    /// Bağlantı istatistiklerini getir
    /// </summary>
    /// <returns>Bağlantı istatistikleri</returns>
    Task<ConnectionStats> GetConnectionStatsAsync();
    /// <summary>
    /// Aktif bağlantıları getir
    /// </summary>
    /// <returns>Aktif bağlantılar</returns>
    Task<List<ConnectionInfo>> GetActiveConnectionsAsync();
    /// <summary>
    /// Kullanıcı bağlantılı mı
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Kullanıcı bağlantılı mı</returns>
    Task<bool> IsUserConnectedAsync(Guid userId);
    /// <summary>
    /// Kullanıcı bağlantısını kes
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="reason">Sebep</param>
    Task DisconnectUserAsync(Guid userId, string reason = "Manual disconnect");
    
    // Payment Notifications
    /// <summary>
    /// Ödeme durumu güncellendi
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="status">Durum</param>
    /// <param name="message">Mesaj</param>
    Task SendPaymentStatusUpdateAsync(Guid paymentId, Guid orderId, Guid userId, PaymentStatus status, string message);
    /// <summary>
    /// Ödeme oluşturuldu
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="method">Ödeme methodu</param>
    /// <param name="amount">Miktar</param>
    Task SendPaymentCreatedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, PaymentMethod method, decimal amount);
    /// <summary>
    /// Ödeme toplandı
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="amount">Miktar</param>
    /// <param name="courierName">Kurye adı</param>
    Task SendPaymentCollectedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, decimal amount, string courierName);
    /// <summary>
    /// Ödeme başarısız oldu
    /// </summary>
    /// <param name="paymentId">Ödeme ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="reason">Sebep</param>
    Task SendPaymentFailedNotificationAsync(Guid paymentId, Guid orderId, Guid userId, string reason);
    /// <summary>
    /// Kurye için ödeme gönder
    /// </summary>
    /// <param name="courierId">Kurye ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="amount">Miktar</param>
    /// <param name="customerName">Müşteri adı</param>
    Task SendCourierPaymentNotificationAsync(Guid courierId, Guid orderId, decimal amount, string customerName);
    /// <summary>
    /// Merchant için ödeme gönder
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="amount">Miktar</param>
    /// <param name="status">Durum</param>
    Task SendMerchantPaymentNotificationAsync(Guid merchantId, Guid orderId, decimal amount, string status);
    /// <summary>
    /// Settlement gönder
    /// </summary>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="totalAmount">Toplam miktar</param>
    /// <param name="netAmount">Net miktar</param>
    /// <param name="status">Durum</param>
    Task SendSettlementNotificationAsync(Guid merchantId, decimal totalAmount, decimal netAmount, string status);
}
