namespace ECommerceApi.Models;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = null!;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }        // % nếu Percent, số tiền nếu Fixed
    public decimal? MaxDiscountAmount { get; set; }   // trần giảm (cho Percent)
    public decimal MinOrderAmount { get; set; }       // đơn tối thiểu để dùng
    public int? UsageLimit { get; set; }              // tổng lượt (null = vô hạn)
    public int UsedCount { get; set; }
    public int? UserUsageLimit { get; set; }          // lượt/user (null = vô hạn)
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CouponUsage> Usages { get; set; } = [];
}
