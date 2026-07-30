using ECommerceApi.Models;

namespace ECommerceApi.Services;

/// <summary>
/// Gửi email thông báo liên quan đơn hàng. Mọi lỗi gửi được nuốt bên trong (không chặn nghiệp vụ).
/// </summary>
public interface IOrderNotifier
{
    Task OrderPlacedAsync(Order order, string toEmail, string toName);
    Task PaymentReceivedAsync(Order order, string toEmail, string toName);
    Task StatusChangedAsync(Order order, string toEmail, string toName);
}
