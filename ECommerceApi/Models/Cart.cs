namespace ECommerceApi.Models;

public class Cart
{
    public int Id { get; set; }

    public int? UserId { get; set; }          // user đăng nhập (1 trong 2)
    public User? User { get; set; }
    public string? SessionId { get; set; }    // khách vãng lai (guest)

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<CartItem> Items { get; set; } = [];
}