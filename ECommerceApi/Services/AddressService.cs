using ECommerceApi.DTOs.Address;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;  // tránh nhập nhằng GreenDonut

namespace ECommerceApi.Services;

public class AddressService(IUnitOfWork uow) : IAddressService
{
    public async Task<IEnumerable<AddressDto>> GetMyAddressesAsync(int userId)
    {
        var list = await uow.Repository<Address>().Query()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)        // mặc định lên đầu
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<AddressDto> GetByIdAsync(int userId, int id)
    {
        var address = await FindOwnedAsync(userId, id);
        return ToDto(address);
    }

    public async Task<AddressDto> CreateAsync(int userId, CreateAddressRequest request)
    {
        var repo = uow.Repository<Address>();

        // Địa chỉ đầu tiên của user → ép làm mặc định. Hoặc client yêu cầu mặc định.
        var isFirst = !await repo.Query().AnyAsync(a => a.UserId == userId);
        var makeDefault = request.IsDefault || isFirst;

        if (makeDefault)
            await ClearDefaultAsync(userId);

        var address = new Address
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            Province = request.Province,
            District = request.District,
            Ward = request.Ward,
            AddressLine = request.AddressLine,
            IsDefault = makeDefault
        };

        await repo.AddAsync(address);
        await uow.CommitAsync();
        return ToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(int userId, int id, UpdateAddressRequest request)
    {
        var address = await FindOwnedAsync(userId, id);

        // Bật mặc định cho địa chỉ này → tắt mặc định ở các địa chỉ khác
        if (request.IsDefault && !address.IsDefault)
            await ClearDefaultAsync(userId);

        address.FullName = request.FullName;
        address.Phone = request.Phone;
        address.Province = request.Province;
        address.District = request.District;
        address.Ward = request.Ward;
        address.AddressLine = request.AddressLine;
        address.IsDefault = request.IsDefault;

        uow.Repository<Address>().Update(address);
        await uow.CommitAsync();
        return ToDto(address);
    }

    public async Task<AddressDto> SetDefaultAsync(int userId, int id)
    {
        var address = await FindOwnedAsync(userId, id);
        await ClearDefaultAsync(userId);

        address.IsDefault = true;
        uow.Repository<Address>().Update(address);
        await uow.CommitAsync();
        return ToDto(address);
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var repo = uow.Repository<Address>();
        var address = await FindOwnedAsync(userId, id);
        var wasDefault = address.IsDefault;

        repo.Delete(address);   // soft-delete (BaseEntity)

        // Nếu xóa địa chỉ mặc định → đề cử địa chỉ mới nhất còn lại làm mặc định
        if (wasDefault)
        {
            var next = await repo.Query()
                .Where(a => a.UserId == userId && a.Id != id)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
            if (next is not null)
            {
                next.IsDefault = true;
                repo.Update(next);
            }
        }

        await uow.CommitAsync();
    }

    // Lấy địa chỉ CHẮC CHẮN thuộc về user (chặn IDOR: user A đụng địa chỉ user B)
    private async Task<Address> FindOwnedAsync(int userId, int id)
    {
        return await uow.Repository<Address>().Query()
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
            ?? throw new KeyNotFoundException($"Không tìm thấy địa chỉ id={id}");
    }

    // Tắt cờ mặc định ở mọi địa chỉ đang mặc định của user
    private async Task ClearDefaultAsync(int userId)
    {
        var repo = uow.Repository<Address>();
        var current = await repo.Query()
            .Where(a => a.UserId == userId && a.IsDefault)
            .ToListAsync();
        foreach (var a in current)
        {
            a.IsDefault = false;
            repo.Update(a);
        }
    }

    private static AddressDto ToDto(Address a) => new()
    {
        Id = a.Id,
        FullName = a.FullName,
        Phone = a.Phone,
        Province = a.Province,
        District = a.District,
        Ward = a.Ward,
        AddressLine = a.AddressLine,
        IsDefault = a.IsDefault
    };
}
