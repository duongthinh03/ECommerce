using ECommerceApi.Models;

namespace ECommerceApi.Services;

public class OrderNotifier(IEmailSender emailSender, ILogger<OrderNotifier> logger) : IOrderNotifier
{
    public Task OrderPlacedAsync(Order order, string toEmail, string toName)
    {
        var items = string.Join("\n", order.Items.Select(i =>
            $"  - {i.ProductName} (x{i.Quantity}): {i.FinalPrice:N0}đ"));

        var payLine = order.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase)
            ? "Phương thức: Thanh toán khi nhận hàng (COD)."
            : "Phương thức: Chuyển khoản — vui lòng quét mã QR để hoàn tất thanh toán.";

        var body =
            $"Chào {toName},\n\n" +
            $"Cảm ơn bạn đã đặt hàng tại ShopViet!\n\n" +
            $"Mã đơn: {order.OrderCode}\n" +
            $"{items}\n\n" +
            $"Tạm tính: {order.TotalAmount:N0}đ\n" +
            $"Phí vận chuyển: {order.ShippingFee:N0}đ\n" +
            (order.DiscountAmount > 0 ? $"Giảm giá: -{order.DiscountAmount:N0}đ\n" : "") +
            $"Tổng thanh toán: {order.FinalAmount:N0}đ\n\n" +
            $"{payLine}\n\n" +
            $"— ShopViet";

        return SafeSend(toEmail, $"[ShopViet] Xác nhận đơn hàng {order.OrderCode}", body);
    }

    public Task PaymentReceivedAsync(Order order, string toEmail, string toName)
    {
        var body =
            $"Chào {toName},\n\n" +
            $"ShopViet đã nhận được thanh toán cho đơn {order.OrderCode}.\n" +
            $"Số tiền: {order.FinalAmount:N0}đ\n\n" +
            $"Đơn hàng đang được xử lý. Cảm ơn bạn!\n\n" +
            $"— ShopViet";

        return SafeSend(toEmail, $"[ShopViet] Đã nhận thanh toán đơn {order.OrderCode}", body);
    }

    public Task StatusChangedAsync(Order order, string toEmail, string toName)
    {
        var status = StatusText(order.Status);
        var body =
            $"Chào {toName},\n\n" +
            $"Đơn hàng {order.OrderCode} đã chuyển sang trạng thái: {status}.\n\n" +
            $"Cảm ơn bạn đã mua sắm tại ShopViet!\n\n" +
            $"— ShopViet";

        return SafeSend(toEmail, $"[ShopViet] Cập nhật đơn {order.OrderCode}: {status}", body);
    }

    private async Task SafeSend(string to, string subject, string body)
    {
        try
        {
            await emailSender.SendAsync(to, subject, body);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gửi email đơn hàng thất bại tới {To}", to);
        }
    }

    private static string StatusText(OrderStatus s) => s switch
    {
        OrderStatus.Pending => "Chờ xử lý",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Packing => "Đang đóng gói",
        OrderStatus.Shipping => "Đang giao hàng",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Completed => "Hoàn tất",
        OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Refunded => "Đã hoàn tiền",
        _ => s.ToString()
    };
}
