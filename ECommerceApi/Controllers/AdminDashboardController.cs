using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Authorize(Roles = "Admin,Manager,Staff")]
[Route("api/admin/dashboard")]
public class AdminDashboardController(IDashboardService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => OkResponse(await service.GetAsync());
}
