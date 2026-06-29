using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Common;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > PageRequest.MaxPageSize ? 20 : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public static Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PageRequest request, CancellationToken cancellationToken = default) =>
        query.ToPagedResultAsync(request.Page, request.PageSize, cancellationToken);
}
