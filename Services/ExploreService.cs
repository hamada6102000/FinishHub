using Microsoft.EntityFrameworkCore;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public class ExploreService
{
    private readonly EngineerService _engineers;
    private readonly ProjectService _projects;

    public ExploreService(EngineerService engineers, ProjectService projects)
    {
        _engineers = engineers;
        _projects  = projects;
    }

    public async Task<PagedResult<ExploreItemDto>> SearchAsync(ExploreQuery query, string lang = "en")
    {
        return query.Tab switch
        {
            ExploreTab.Engineers => await SearchEngineersOnlyAsync(query, query.CityId),
            ExploreTab.Projects  => await SearchProjectsOnlyAsync(query, query.CityId, lang),
            _                    => await SearchAllAsync(query, query.CityId, lang),
        };
    }

    private async Task<PagedResult<ExploreItemDto>> SearchEngineersOnlyAsync(ExploreQuery query, int? cityId)
    {
        var page = await OrderEngineers(_engineers.BuildExploreQuery(query.Keyword, cityId), query.Sort)
            .ToPagedResultAsync(query);

        return page.Map(u => new ExploreItemDto { Type = "Engineer", Engineer = EngineerService.MapSummary(u) });
    }

    private async Task<PagedResult<ExploreItemDto>> SearchProjectsOnlyAsync(ExploreQuery query, int? cityId, string lang)
    {
        var page = await OrderProjects(_projects.BuildExploreQuery(query.Keyword, cityId, query.PropertyType), query.Sort)
            .ToPagedResultAsync(query);

        return page.Map(p => new ExploreItemDto { Type = "Project", Project = ProjectService.Map(p, lang) });
    }

    /// <summary>
    /// EF Core can't page a UNION across two unrelated entity types, so both sides are queried
    /// (filtered + ordered) for enough rows to cover every candidate for the requested page,
    /// merged in memory by the common sort key, then sliced to the actual page.
    /// </summary>
    private async Task<PagedResult<ExploreItemDto>> SearchAllAsync(ExploreQuery query, int? cityId, string lang)
    {
        var engineerQuery = _engineers.BuildExploreQuery(query.Keyword, cityId);
        var projectQuery  = _projects.BuildExploreQuery(query.Keyword, cityId, query.PropertyType);

        var engineerTotal = await engineerQuery.CountAsync();
        var projectTotal  = await projectQuery.CountAsync();
        var totalCount     = engineerTotal + projectTotal;

        var take = query.Skip + query.PageSize;

        var engineers = await OrderEngineers(engineerQuery, query.Sort).Take(take).ToListAsync();
        var projects  = await OrderProjects(projectQuery, query.Sort).Take(take).ToListAsync();

        var candidates = engineers
            .Select(u => new ExploreCandidate(u.Id, u.NameEn, u.CreatedAt, u, null))
            .Concat(projects.Select(p => new ExploreCandidate(p.Id, p.Title, p.CreatedAt, null, p)));

        var ordered = query.Sort == ExploreSort.NameAsc
            ? candidates.OrderBy(c => c.Name).ThenBy(c => c.Id)
            : candidates.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id);

        var items = ordered
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(c => c.Engineer != null
                ? new ExploreItemDto { Type = "Engineer", Engineer = EngineerService.MapSummary(c.Engineer) }
                : new ExploreItemDto { Type = "Project", Project = ProjectService.Map(c.Project!, lang) })
            .ToList();

        return PagedResult<ExploreItemDto>.Create(items, query, totalCount);
    }

    private static IOrderedQueryable<User> OrderEngineers(IQueryable<User> query, ExploreSort sort) =>
        sort == ExploreSort.NameAsc
            ? query.OrderBy(u => u.NameEn).ThenBy(u => u.Id)
            : query.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id);

    private static IOrderedQueryable<Project> OrderProjects(IQueryable<Project> query, ExploreSort sort) =>
        sort == ExploreSort.NameAsc
            ? query.OrderBy(p => p.Title).ThenBy(p => p.Id)
            : query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

    private sealed record ExploreCandidate(int Id, string Name, DateTime CreatedAt, User? Engineer, Project? Project);
}
