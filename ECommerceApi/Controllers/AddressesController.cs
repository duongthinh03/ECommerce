using ECommerceApi.DTOs.Address;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[Authorize]   // sổ địa chỉ luôn gắn với user đang đăng nhập
public class AddressesController(IAddressService service) : ApiControllerBase
{
    // userId chắc chắn có (đã [Authorize]); guard cho chắc
    private int UserId => CurrentUserId
        ?? throw new UnauthorizedAccessException("Cần đăng nhập");

    [HttpGet]
    public async Task<IActionResult> GetMine() =>
        OkResponse(await service.GetMyAddressesAsync(UserId));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) =>
        OkResponse(await service.GetByIdAsync(UserId, id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressRequest request) =>
        CreatedResponse(await service.CreateAsync(UserId, request), "Đã thêm địa chỉ");

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressRequest request) =>
        OkResponse(await service.UpdateAsync(UserId, id, request), "Đã cập nhật địa chỉ");

    [HttpPut("{id:int}/default")]
    public async Task<IActionResult> SetDefault(int id) =>
        OkResponse(await service.SetDefaultAsync(UserId, id), "Đã đặt làm mặc định");

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(UserId, id);
        return OkResponse<object?>(null, "Đã xóa địa chỉ");
    }
}
