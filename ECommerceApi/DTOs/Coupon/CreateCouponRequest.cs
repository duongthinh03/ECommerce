using ECommerceApi.Models;

namespace ECommerceApi.DTOs.Coupon;

public class CreateCouponRequest
{
    public string Code { get; set; } = null!;
    public DiscountType DiscountType { get; set; }   // "Percent" hoặc "Fixed"
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? UserUsageLimit { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public bool IsActive { get; set; } = true;
}
