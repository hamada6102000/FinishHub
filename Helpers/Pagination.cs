using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace test.Helpers;

/// <summary>Query-string parameters accepted by every paginated endpoint (?page=1&amp;pageSize=10).</summary>
public class PaginationQuery
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize     = 100;

    private int _page     = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>1-based page number. Values below 1 fall back to 1.</summary>
    [FromQuery(Name = "page")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page, clamped to 1..100.</summary>
    [FromQuery(Name = "pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
    }

    /// <summary>Optional filter for trusted engineers. If true, returns only trusted engineers.</summary>
    [FromQuery(Name = "isTrusted")]
    public bool? IsTrusted { get; set; }

    /// <summary>Optional filter for featured projects. If true, returns only featured projects.</summary>
    [FromQuery(Name = "isFeatured")]
    public bool? IsFeatured { get; set; }

    /// <summary>Optional filter for pinned cities. If true, returns only pinned cities.</summary>
    [FromQuery(Name = "isPinned")]
    public bool? IsPinned { get; set; }

    /// <summary>
    /// Optional filter by user type. When set, only users belonging to that type are returned.
    /// Filtering is by id rather than name because names are editable and bilingual, so an
    /// administrator renaming a type would otherwise silently break every caller.
    /// </summary>
    [FromQuery(Name = "userTypeId")]
    public int? UserTypeId { get; set; }

    /// <summary>If true, orders results by rate descending instead of the default (most recent first).</summary>
    [FromQuery(Name = "topRated")]
    public bool? TopRated { get; set; }

    /// <summary>
    /// Dashboard/Admin only. When false (the default) reads return active records only;
    /// when true both active and inactive records are returned so administrators can
    /// manage activation status. Normal application clients never send this.
    /// </summary>
    [FromQuery(Name = "includeInactive")]
    public bool IncludeInactive { get; set; }

    /// <summary>
    /// Rows to skip for the current page. Internal so it stays out of model binding and
    /// the Swagger contract — clients send only page and pageSize.
    /// </summary>
    internal int Skip => (Page - 1) * PageSize;
}

/// <summary>A single page of results plus the metadata a client needs to render a pager.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PaginationQuery.DefaultPageSize;
    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Create(List<T> items, PaginationQuery pagination, int totalCount) => new()
    {
        Items      = items,
        Page       = pagination.Page,
        PageSize   = pagination.PageSize,
        TotalCount = totalCount,
    };

    /// <summary>Projects the items of an already-paged result into another type.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
    {
        Items      = Items.Select(selector).ToList(),
        Page       = Page,
        PageSize   = PageSize,
        TotalCount = TotalCount,
    };
}

public static class QueryablePaginationExtensions
{
    /// <summary>Counts the query, then pulls a single page out of it. The query must already be ordered.</summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PaginationQuery pagination)
    {
        var totalCount = await query.CountAsync();
        var items      = await query.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();
        return PagedResult<T>.Create(items, pagination, totalCount);
    }
}
