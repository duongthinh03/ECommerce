using ECommerceApi.DTOs.Admin;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Services;

public class DashboardService(IUnitOfWork uow) : IDashboardService
{
    private const int LowStockThreshold = 5;
    private const int VnOffsetHours = 7;   // giờ VN = UTC+7

    // Đơn được tính doanh thu = đã xác nhận trở đi (bỏ Pending/Cancelled/Refunded)
    private static readonly OrderStatus[] RevenueStatuses =
        [OrderStatus.Confirmed, OrderStatus.Packing, OrderStatus.Shipping, OrderStatus.Delivered, OrderStatus.Completed];

    public async Task<DashboardDto> GetAsync()
    {
        var nowVn = DateTime.UtcNow.AddHours(VnOffsetHours);
        var todayStartUtc = nowVn.Date.AddHours(-VnOffsetHours);
        var monthStartVn = new DateTime(nowVn.Year, nowVn.Month, 1);
        var monthStartUtc = monthStartVn.AddHours(-VnOffsetHours);
        var dailyStartUtc = nowVn.Date.AddDays(-29).AddHours(-VnOffsetHours);       // 30 ngày (gồm hôm nay)
        var monthlyStartUtc = monthStartVn.AddMonths(-11).AddHours(-VnOffsetHours); // 12 tháng (gồm tháng này)

        var orders = uow.Repository<Order>().Query();
        var revenueOrders = orders.Where(o => RevenueStatuses.Contains(o.Status));

        var dto = new DashboardDto
        {
            RevenueAllTime = await revenueOrders.SumAsync(o => (decimal?)o.FinalAmount) ?? 0m,
            RevenueToday = await revenueOrders.Where(o => o.CreatedAt >= todayStartUtc).SumAsync(o => (decimal?)o.FinalAmount) ?? 0m,
            RevenueThisMonth = await revenueOrders.Where(o => o.CreatedAt >= monthStartUtc).SumAsync(o => (decimal?)o.FinalAmount) ?? 0m,

            TotalOrders = await orders.CountAsync(),
            PendingOrders = await orders.CountAsync(o => o.Status == OrderStatus.Pending),
            TotalProducts = await uow.Repository<Product>().Query().CountAsync(),
            TotalCustomers = await uow.Repository<User>().Query().CountAsync(u => u.Role.Name == "Customer"),
        };

        // Đơn theo trạng thái
        var byStatus = await orders
            .GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        dto.OrdersByStatus = byStatus
            .OrderBy(s => s.Key)
            .Select(s => new StatusCountDto { Status = s.Key.ToString(), Count = s.Count })
            .ToList();

        // Doanh thu 30 ngày — gom nhóm NGAY TRONG SQL theo ngày giờ VN (DB chỉ trả ~30 dòng)
        var dailyRaw = await revenueOrders
            .Where(o => o.CreatedAt >= dailyStartUtc)
            .GroupBy(o => o.CreatedAt.AddHours(VnOffsetHours).Date)
            .Select(g => new { Day = g.Key, Sum = g.Sum(x => x.FinalAmount) })
            .ToListAsync();
        var byDay = dailyRaw.ToDictionary(x => x.Day, x => x.Sum);
        dto.RevenueDaily = Enumerable.Range(0, 30)
            .Select(i => nowVn.Date.AddDays(-29 + i))
            .Select(d => new DailyRevenueDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                Revenue = byDay.TryGetValue(d, out var r) ? r : 0m
            })
            .ToList();

        // Doanh thu 12 tháng — gom nhóm trong SQL theo (năm, tháng) giờ VN
        var monthlyRaw = await revenueOrders
            .Where(o => o.CreatedAt >= monthlyStartUtc)
            .GroupBy(o => new { o.CreatedAt.AddHours(VnOffsetHours).Year, o.CreatedAt.AddHours(VnOffsetHours).Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Sum = g.Sum(x => x.FinalAmount) })
            .ToListAsync();
        var byMonth = monthlyRaw.ToDictionary(x => (x.Year, x.Month), x => x.Sum);
        dto.RevenueMonthly = Enumerable.Range(0, 12)
            .Select(i => monthStartVn.AddMonths(-11 + i))
            .Select(d => new MonthlyRevenueDto
            {
                Month = d.ToString("yyyy-MM"),
                Revenue = byMonth.TryGetValue((d.Year, d.Month), out var r) ? r : 0m
            })
            .ToList();

        // Top 5 SP bán chạy (dùng tên snapshot trong OrderItem)
        dto.TopProducts = await uow.Repository<OrderItem>().Query()
            .Where(oi => RevenueStatuses.Contains(oi.Order.Status))
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.ProductName,
                SoldQty = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.FinalPrice)
            })
            .OrderByDescending(t => t.SoldQty)
            .Take(5)
            .ToListAsync();

        // SP sắp hết hàng (variant active, tồn <= ngưỡng)
        dto.LowStock = await uow.Repository<ProductVariant>().Query()
            .Where(v => v.IsActive && v.Stock <= LowStockThreshold)
            .OrderBy(v => v.Stock)
            .Select(v => new LowStockDto
            {
                ProductId = v.ProductId,
                ProductName = v.Product.Name,
                Sku = v.Sku,
                OptionName = v.OptionName,
                Stock = v.Stock
            })
            .Take(10)
            .ToListAsync();

        return dto;
    }
}
