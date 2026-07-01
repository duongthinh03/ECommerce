namespace ECommerceApi.Models
{
    public class Order : BaseEntity
    {
        public string OrderCode { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // snapshot địa chỉ giao (copy lúc đặt)
        public string ShipRecipient { get; set; } = null!;
        public string ShipPhone { get; set; } = null!;
        public string ShipProvince { get; set; } = null!;
        public string ShipDistrict { get; set; } = null!;
        public string ShipWard { get; set; } = null!;
        public string ShipAddressLine { get; set; } = null!;

        public decimal TotalAmount { get; set; }      // subtotal (tổng tiền hàng)
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }      // = Total + Ship - Discount

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string PaymentMethod { get; set; } = "COD";
        public DateTime? PaidAt { get; set; }          // thời điểm nhận thanh toán (SePay)
        public string? Note { get; set; }

        public ICollection<OrderItem> Items { get; set; } = [];
        public ICollection<OrderStatusHistory> StatusHistories { get; set; } = [];
    }
}
