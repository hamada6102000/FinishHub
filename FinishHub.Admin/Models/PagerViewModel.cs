namespace FinishHub.Admin.Models;

/// <summary>The subset of a <see cref="PagedResult{T}"/> the _Pager partial needs.</summary>
public class PagerViewModel
{
    /// <summary>How many numbered page links to show around the current page.</summary>
    private const int Window = 2;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = PagedResult<object>.DefaultPageSize;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int FirstItemNumber { get; set; }
    public int LastItemNumber { get; set; }

    /// <summary>Page-size options offered in the dropdown.</summary>
    public static readonly int[] PageSizeOptions = { 10, 25, 50, 100 };

    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public int FirstPageLink => Math.Max(1, Page - Window);
    public int LastPageLink => Math.Min(TotalPages, Page + Window);

    public static PagerViewModel From<T>(PagedResult<T> result) => new()
    {
        Page            = result.Page,
        PageSize        = result.PageSize,
        TotalCount      = result.TotalCount,
        TotalPages      = result.TotalPages,
        FirstItemNumber = result.FirstItemNumber,
        LastItemNumber  = result.LastItemNumber,
    };
}
