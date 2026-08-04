using Microsoft.AspNetCore.Mvc;
using test.Helpers;
using test.Models;

namespace test.DTOs;

// ---------- Explore / Search ----------

public enum ExploreTab
{
    All,
    Engineers,
    Projects,
}

public enum ExploreSort
{
    Newest,
    NameAsc,
}

public class ExploreQuery : PaginationQuery
{
    [FromQuery(Name = "tab")]
    public ExploreTab Tab { get; set; } = ExploreTab.All;

    [FromQuery(Name = "keyword")]
    public string? Keyword { get; set; }

    [FromQuery(Name = "cityId")]
    public int? CityId { get; set; }

    [FromQuery(Name = "propertyType")]
    public PropertyType? PropertyType { get; set; }

    [FromQuery(Name = "sort")]
    public ExploreSort Sort { get; set; } = ExploreSort.Newest;
}

public class ExploreItemDto
{
    public string Type { get; set; } = string.Empty;
    public EngineerSummaryDto? Engineer { get; set; }
    public ProjectDto? Project { get; set; }
}
