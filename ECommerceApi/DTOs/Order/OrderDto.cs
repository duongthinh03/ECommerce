namespace ECommerceApi.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string ShipRecipient { get; set; } = null!;
        public string ShipPhone { get; set; } = null!;
        public string ShipAddress { get; set; } = null!;
        public string? Note { get; set; }
        public string? PaymentQrUrl { get; set; }   // URL ảnh VietQR (đơn SePay chưa thanh toán)
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = [];
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
