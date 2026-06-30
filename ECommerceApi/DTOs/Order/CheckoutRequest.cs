namespace ECommerceApi.DTOs.Order
{
    public class CheckoutRequest
    {
        public string ShipRecipient { get; set; } = null!;
        public string ShipPhone { get; set; } = null!;
        public string ShipProvince { get; set; } = null!;
        public string ShipDistrict { get; set; } = null!;
        public string ShipWard { get; set; } = null!;
        public string ShipAddressLine { get; set; } = null!;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string? CouponCode { get; set; }   // mã giảm giá (tùy chọn)
    }
}
