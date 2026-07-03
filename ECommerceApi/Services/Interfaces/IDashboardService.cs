using ECommerceApi.DTOs.Admin;

namespace ECommerceApi.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync();
}
