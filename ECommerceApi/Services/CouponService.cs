using ECommerceApi.DTOs.Coupon;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace ECommerceApi.Services;

public class CouponService(IUnitOfWork uow) : ICouponService
{
    public async Task<(Coupon coupon, decimal discount)> ValidateAsync(string code, int userId, decimal subtotal)
    {
        var coupon = await uow.Repository<Coupon>().Query()
            .Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Code == code)
            ?? throw new InvalidOperationException("Mã giảm giá không tồn tại");

        var now = DateTime.UtcNow;
        if (!coupon.IsActive)
            throw new InvalidOperationException("Mã giảm giá đã bị tắt");
        if (coupon.StartsAt is not null && now < coupon.StartsAt)
            throw new InvalidOperationException("Mã giảm giá chưa có hiệu lực");
        if (coupon.ExpiredAt is not null && now > coupon.ExpiredAt)
            throw new InvalidOperationException("Mã giảm giá đã hết hạn");
        if (subtotal < coupon.MinOrderAmount)
            throw new InvalidOperationException($"Đơn tối thiểu {coupon.MinOrderAmount:#,##0}đ để dùng mã này");
        if (coupon.UsageLimit is not null && coupon.UsedCount >= coupon.UsageLimit)
            throw new InvalidOperationException("Mã giảm giá đã hết lượt");
        if (coupon.UserUsageLimit is not null)
        {
            var userUsed = coupon.Usages.Count(u => u.UserId == userId);
            if (userUsed >= coupon.UserUsageLimit)
                throw new InvalidOperationException("Bạn đã dùng hết lượt cho mã này");
        }

        // tính giảm
        decimal discount = coupon.DiscountType == DiscountType.Percent
            ? subtotal * coupon.DiscountValue / 100m
            : coupon.DiscountValue;

        // chặn trần (cho Percent) + không giảm quá tiền hàng
        if (coupon.MaxDiscountAmount is not null && discount > coupon.MaxDiscountAmount.Value)
            discount = coupon.MaxDiscountAmount.Value;
        if (discount > subtotal)
            discount = subtotal;

        return (coupon, Math.Round(discount, 2));
    }

    public async Task<IEnumerable<CouponDto>> GetAllAsync()
    {
        var coupons = await uow.Repository<Coupon>().Query()
            .OrderByDescending(c => c.Id)
            .ToListAsync();
        return coupons.Select(ToDto);
    }

    public async Task<CouponDto> CreateAsync(CreateCouponRequest request)
    {
        var repo = uow.Repository<Coupon>();
        if (await repo.Query().AnyAsync(c => c.Code == request.Code))
            throw new InvalidOperationException($"Mã '{request.Code}' đã tồn tại");

        var coupon = new Coupon
        {
            Code = request.Code,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinOrderAmount = request.MinOrderAmount,
            UsageLimit = request.UsageLimit,
            UserUsageLimit = request.UserUsageLimit,
            StartsAt = request.StartsAt,
            ExpiredAt = request.ExpiredAt,
            IsActive = request.IsActive
        };
        await repo.AddAsync(coupon);
        await uow.CommitAsync();
        return ToDto(coupon);
    }

    public async Task DeleteAsync(int id)
    {
        var repo = uow.Repository<Coupon>();
        var coupon = await repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy coupon id={id}");
        repo.Delete(coupon);
        await uow.CommitAsync();
    }

    private static CouponDto ToDto(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        DiscountType = c.DiscountType.ToString(),
        DiscountValue = c.DiscountValue,
        MaxDiscountAmount = c.MaxDiscountAmount,
        MinOrderAmount = c.MinOrderAmount,
        UsageLimit = c.UsageLimit,
        UsedCount = c.UsedCount,
        UserUsageLimit = c.UserUsageLimit,
        StartsAt = c.StartsAt,
        ExpiredAt = c.ExpiredAt,
        IsActive = c.IsActive
    };
}
