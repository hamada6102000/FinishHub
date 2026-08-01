namespace FinishHub.Admin.Models;

/// <summary>Mirrors the paged payload the API returns inside its response envelope.</summary>
public class PagedResult<T>
{
    public const int DefaultPageSize = 10;

    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    /// <summary>First and last 1-based item numbers on the current page, for the "showing x-y of z" label.</summary>
    public int FirstItemNumber => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;
    public int LastItemNumber => TotalCount == 0 ? 0 : FirstItemNumber + Items.Count - 1;

    /// <summary>An empty page that still carries the requested page/size, used when the API call fails.</summary>
    public static PagedResult<T> Empty(int page, int pageSize) => new() { Page = page, PageSize = pageSize };
}
