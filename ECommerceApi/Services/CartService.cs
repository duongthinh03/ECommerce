using ECommerceApi.DTOs.Cart;
using ECommerceApi.Models;
using ECommerceApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;  // alias GreenDonut

namespace ECommerceApi.Services
{
    public class CartService(IUnitOfWork uow) : ICartService
    {
        public async Task<CartDto> GetCartAsync(int? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            return BuildDto(cart);
        }

        public async Task<CartDto> AddItemAsync(int? userId, string? sessionId, AddCartItemRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);

            // validate variant: tồn tại + đang bán
            var variant = await uow.Repository<ProductVariant>().Query()
                .FirstOrDefaultAsync(v => v.Id == request.VariantId)
                ?? throw new KeyNotFoundException($"Không tìm thấy variant id={request.VariantId}");
            if (!variant.IsActive)
                throw new InvalidOperationException("Sản phẩm này hiện không bán");

            var itemRepo = uow.Repository<CartItem>();
            var existing = cart.Items.FirstOrDefault(i => i.VariantId == request.VariantId);
            if (existing is not null)
            {
                existing.Quantity += request.Quantity;       // CỘNG DỒN (không đẻ dòng mới)
                existing.UpdatedAt = DateTime.UtcNow;
                itemRepo.Update(existing);
            }
            else
            {
                await itemRepo.AddAsync(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = variant.ProductId,
                    VariantId = variant.Id,
                    Quantity = request.Quantity,
                    Price = variant.Price                    // snapshot giá hiện tại
                });
            }

            await uow.CommitAsync();
            return await GetCartAsync(userId, sessionId);     // lấy lại giỏ tươi (kèm tên SP)
        }

        public async Task<CartDto> UpdateItemAsync(int? userId, string? sessionId, int variantId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.VariantId == variantId)
                ?? throw new KeyNotFoundException("Item không có trong giỏ");

            var itemRepo = uow.Repository<CartItem>();
            if (quantity <= 0)
            {
                itemRepo.Delete(item);                        // qty <= 0 → xóa khỏi giỏ
            }
            else
            {
                item.Quantity = quantity;
                item.UpdatedAt = DateTime.UtcNow;
                itemRepo.Update(item);
            }

            await uow.CommitAsync();
            return await GetCartAsync(userId, sessionId);
        }

        public async Task<CartDto> RemoveItemAsync(int? userId, string? sessionId, int variantId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.VariantId == variantId)
                ?? throw new KeyNotFoundException("Item không có trong giỏ");

            uow.Repository<CartItem>().Delete(item);
            await uow.CommitAsync();
            return await GetCartAsync(userId, sessionId);
        }

        public async Task ClearAsync(int? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartAsync(userId, sessionId);
            var itemRepo = uow.Repository<CartItem>();
            foreach (var item in cart.Items.ToList())
                itemRepo.Delete(item);
            await uow.CommitAsync();
        }

        // FIND-OR-CREATE: tìm giỏ theo user/session, chưa có thì tạo
        private async Task<Cart> GetOrCreateCartAsync(int? userId, string? sessionId)
        {
            if (userId is null && string.IsNullOrWhiteSpace(sessionId))
                throw new InvalidOperationException("Thiếu định danh giỏ (cần đăng nhập hoặc X-Session-Id)");

            var repo = uow.Repository<Cart>();
            var query = repo.Query()
                .Include(c => c.Items).ThenInclude(i => i.Variant)
                .Include(c => c.Items).ThenInclude(i => i.Product);

            var cart = userId is not null
                ? await query.FirstOrDefaultAsync(c => c.UserId == userId)
                : await query.FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId, SessionId = userId is null ? sessionId : null };
                await repo.AddAsync(cart);
                await uow.CommitAsync();
            }
            return cart;
        }

        public async Task<CartDto> MergeAsync(int userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new InvalidOperationException("Thiếu sessionId của giỏ guest");

            var repo = uow.Repository<Cart>();

            // giỏ guest (kèm items)
            var guestCart = await repo.Query()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            // không có giỏ guest hoặc rỗng -> chỉ trả giỏ user
            if (guestCart is null || guestCart.Items.Count == 0)
                return await GetCartAsync(userId, null);

            // giỏ user (find-or-create)
            var userCart = await GetOrCreateCartAsync(userId, null);
            var itemRepo = uow.Repository<CartItem>();

            foreach (var g in guestCart.Items)
            {
                var existing = userCart.Items.FirstOrDefault(i => i.VariantId == g.VariantId);
                if (existing is not null)
                {
                    existing.Quantity += g.Quantity;        // cộng dồn nếu trùng variant
                    existing.UpdatedAt = DateTime.UtcNow;
                    itemRepo.Update(existing);
                }
                else
                {
                    await itemRepo.AddAsync(new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = g.ProductId,
                        VariantId = g.VariantId,
                        Quantity = g.Quantity,
                        Price = g.Price
                    });
                }
            }

            repo.Delete(guestCart);   // xóa giỏ guest (items cascade theo)
            await uow.CommitAsync();

            return await GetCartAsync(userId, null);
        }

        private static CartDto BuildDto(Cart cart)
        {
            var items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                Thumbnail = i.Variant?.ImageUrl ?? i.Product?.Thumbnail,
                VariantId = i.VariantId,
                Sku = i.Variant?.Sku ?? "",
                Quantity = i.Quantity,
                Price = i.Price,
                LineTotal = i.Price * i.Quantity,
                Stock = i.Variant?.Stock ?? 0
            }).ToList();

            return new CartDto
            {
                Id = cart.Id,
                Items = items,
                TotalQuantity = items.Sum(x => x.Quantity),
                TotalAmount = items.Sum(x => x.LineTotal)
            };
        }
    }
}
