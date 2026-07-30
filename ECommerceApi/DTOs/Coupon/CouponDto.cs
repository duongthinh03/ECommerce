namespace ECommerceApi.DTOs.Coupon;

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int? UserUsageLimit { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public bool IsActive { get; set; }
}
