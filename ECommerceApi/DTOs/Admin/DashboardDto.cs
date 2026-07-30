namespace ECommerceApi.DTOs.Admin;

public class DashboardDto
{
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueAllTime { get; set; }

    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }     // đơn chờ xử lý (Pending)
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }

    public List<StatusCountDto> OrdersByStatus { get; set; } = [];
    public List<DailyRevenueDto> RevenueDaily { get; set; } = [];     // 30 ngày (FE cắt 7 ngày từ đây)
    public List<MonthlyRevenueDto> RevenueMonthly { get; set; } = []; // 12 tháng
    public List<TopProductDto> TopProducts { get; set; } = [];
    public List<LowStockDto> LowStock { get; set; } = [];
}

public class StatusCountDto
{
    public string Status { get; set; } = null!;
    public int Count { get; set; }
}

public class DailyRevenueDto
{
    public string Date { get; set; } = null!;   // yyyy-MM-dd (giờ VN)
    public decimal Revenue { get; set; }
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = null!;  // yyyy-MM (giờ VN)
    public decimal Revenue { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public int SoldQty { get; set; }
    public decimal Revenue { get; set; }
}

public class LowStockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? OptionName { get; set; }
    public int Stock { get; set; }
}
