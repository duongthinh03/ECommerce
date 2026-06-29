namespace ECommerceApi.Common;

// Bind từ query string: ?page=1&pageSize=20. Tự kẹp pageSize trong [1, 100].
public class PageRequest
{
    public const int MaxPageSize = 100;
    private int _pageSize = 20;
    private int _page = 1;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > MaxPageSize ? 20 : value;
    }
}
