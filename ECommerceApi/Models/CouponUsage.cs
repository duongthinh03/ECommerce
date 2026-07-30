namespace ECommerceApi.Models;

public class CouponUsage
{
    public int Id { get; set; }

    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;

    public int UserId { get; set; }      // tham chiếu (snapshot)
    public int OrderId { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
