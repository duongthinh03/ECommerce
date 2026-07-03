using ECommerceApi.DTOs.Coupon;
using ECommerceApi.Models;

namespace ECommerceApi.Services;

public interface ICouponService
{
    // Validate code + tính giảm. Trả coupon + số tiền giảm (ném nếu không hợp lệ).
    Task<(Coupon coupon, decimal discount)> ValidateAsync(string code, int userId, decimal subtotal);

    // Admin CRUD
    Task<IEnumerable<CouponDto>> GetAllAsync();
    Task<CouponDto> CreateAsync(CreateCouponRequest request);
    Task DeleteAsync(int id);
}
