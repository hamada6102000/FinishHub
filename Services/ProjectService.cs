using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public class ProjectService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProjectService(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    /// <summary>
    /// Default visibility rule: normal application responses contain active projects owned by
    /// active users only, so deactivating an owner also hides their work. Dashboard/Admin
    /// callers opt in to inactive records with includeInactive.
    /// </summary>
    internal static IQueryable<Project> ApplyActiveFilter(IQueryable<Project> query, bool includeInactive) =>
        includeInactive ? query : query.Where(p => p.IsActive && p.User.IsActive);

    public async Task<PagedResult<ProjectDto>> GetAllAsync(PaginationQuery pagination, string lang = "en")
    {
        var query = ApplyActiveFilter(
            _db.Projects
                .Include(p => p.Media)
                .Include(p => p.Materials)
                .Include(p => p.User)
                .Include(p => p.City),
            pagination.IncludeInactive);

        if (pagination.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == pagination.IsFeatured.Value);

        var ordered = pagination.TopRated == true
            ? query.OrderByDescending(p => p.Rate).ThenByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id)
            : query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

        var page = await ordered.ToPagedResultAsync(pagination);
        return page.Map(p => Map(p, lang));
    }

    public async Task<ProjectDto?> GetByIdAsync(int id, string lang = "en", bool includeInactive = false)
    {
        var query = ApplyActiveFilter(
            _db.Projects.Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City),
            includeInactive);

        var project = await query.FirstOrDefaultAsync(p => p.Id == id);
        return project == null ? null : Map(project, lang);
    }

    public async Task<PagedResult<ProjectDto>> GetUserProjectsAsync(int userId, PaginationQuery pagination, string lang = "en")
    {
        var query = ApplyActiveFilter(
            _db.Projects
                .Include(p => p.Media)
                .Include(p => p.Materials)
                .Include(p => p.User)
                .Include(p => p.City),
            pagination.IncludeInactive);

        var page = await query
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToPagedResultAsync(pagination);
        return page.Map(p => Map(p, lang));
    }

    public async Task<ProjectDto> CreateAsync(int userId, CreateProjectRequest req, string lang = "en")
    {
        var project = new Project
        {
            UserId       = userId,
            Title        = req.Title,
            Location     = req.Location,
            CityId       = req.CityId,
            PropertyType = req.PropertyType,
            Description  = req.Description,
            Area         = req.Area,
            Timeline     = req.Timeline,
            Budget       = req.Budget,
            Category     = req.Category,
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        await SaveMediaAsync(project.Id, req.Images);
        SaveMaterials(project.Id, req.Materials);
        await _db.SaveChangesAsync();

        return Map((await _db.Projects.Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City).FirstAsync(p => p.Id == project.Id)), lang);
    }

    public async Task<(bool success, ProjectDto? dto)> UpdateAsync(int id, int userId, UpdateProjectRequest req, string lang = "en")
    {
        var project = await _db.Projects.Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null || project.UserId != userId) return (false, null);

        if (req.Title       != null) project.Title        = req.Title;
        if (req.Location    != null) project.Location     = req.Location;
        if (req.CityId.HasValue)     project.CityId       = req.CityId.Value;
        if (req.Description != null) project.Description  = req.Description;
        if (req.Area        != null) project.Area         = req.Area;
        if (req.Timeline    != null) project.Timeline     = req.Timeline;
        if (req.Budget      != null) project.Budget       = req.Budget;
        if (req.Category    != null) project.Category     = req.Category;
        if (req.PropertyType.HasValue) project.PropertyType = req.PropertyType.Value;

        if (req.Materials != null)
        {
            if (project.Materials != null)
            {
                _db.ProjectMaterials.RemoveRange(project.Materials);
                project.Materials.Clear();
            }
            SaveMaterials(project.Id, req.Materials);
        }

        await _db.SaveChangesAsync();

        if (req.Materials != null)
            await _db.Entry(project).Collection(p => p.Materials).LoadAsync();

        return (true, Map(project, lang));
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (project == null || project.UserId != userId) return false;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return true;
    }

    // ---------- helpers ----------

    private async Task SaveMediaAsync(int projectId, List<IFormFile>? images)
    {
        var imageUrls = await FileUploadHelper.SaveFilesAsync(images, "projects/images", _env);

        foreach (var url in imageUrls)
            _db.ProjectMedia.Add(new ProjectMedia { ProjectId = projectId, Url = url, MediaType = MediaType.Image });
    }

    private void SaveMaterials(int projectId, List<ProjectMaterialRequest>? materials)
    {
        if (materials == null) return;

        foreach (var material in materials)
            _db.ProjectMaterials.Add(new ProjectMaterial { ProjectId = projectId, MaterialName = material.MaterialName, Description = material.Description });
    }

    public async Task<ProjectDto?> SetFeaturedAsync(int id, bool isFeatured, string lang = "en")
    {
        var project = await _db.Projects.Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return null;

        project.IsFeatured = isFeatured;
        await _db.SaveChangesAsync();
        return Map(project, lang);
    }

    /// <summary>
    /// Activates or deactivates a project. Touches only IsActive and never deletes the record,
    /// so reactivating restores it to the normal Get/List responses unchanged. Idempotent:
    /// setting the value it already has is a no-op that still returns the current state.
    /// The lookup deliberately ignores the active filter — administrators must be able to
    /// reach inactive projects in order to reactivate them.
    /// </summary>
    public async Task<ProjectDto?> SetActiveAsync(int id, bool isActive, string lang = "en")
    {
        var project = await _db.Projects
            .Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return null;

        if (project.IsActive == isActive)
            return Map(project, lang);

        project.IsActive = isActive;
        await _db.SaveChangesAsync();
        return Map(project, lang);
    }

    public async Task<(bool success, string message, ProjectDto? dto)> RateAsync(int projectId, int userId, double value, string lang = "en")
    {
        if (value < 1 || value > 5)
            return (false, "Rate must be between 1 and 5.", null);

        // Inactive projects (or projects of a deactivated owner) are invisible to the app,
        // so they cannot be rated either.
        var project = await ApplyActiveFilter(
                _db.Projects.Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City),
                includeInactive: false)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return (false, "Project not found.", null);

        _db.ProjectRates.Add(new ProjectRate { ProjectId = projectId, UserId = userId, Value = value });
        await _db.SaveChangesAsync();

        project.Rate = await _db.ProjectRates.Where(r => r.ProjectId == projectId).AverageAsync(r => r.Value);
        await _db.SaveChangesAsync();

        return (true, "Project rated.", Map(project, lang));
    }

    /// <summary>Builds the filtered, unordered queryable used by both the plain project list and Explore search.</summary>
    internal IQueryable<Project> BuildExploreQuery(string? keyword, int? cityId, PropertyType? propertyType,
        bool includeInactive = false)
    {
        var query = ApplyActiveFilter(
            _db.Projects
                .Include(p => p.Media)
                .Include(p => p.Materials)
                .Include(p => p.User)
                .Include(p => p.City),
            includeInactive);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.Title.Contains(keyword));

        if (cityId.HasValue)
            query = query.Where(p => p.CityId == cityId.Value);

        if (propertyType.HasValue)
            query = query.Where(p => p.PropertyType == propertyType.Value);

        return query;
    }

    internal static ProjectDto Map(Project p, string lang) => new()
    {
        Id           = p.Id,
        UserId       = p.UserId,
        UserName     = lang == "ar" ? (p.User?.NameAr ?? string.Empty) : (p.User?.NameEn ?? string.Empty),
        Title        = p.Title,
        Location     = p.Location,
        City         = p.City?.NameEn,
        PropertyType = p.PropertyType,
        Description  = p.Description,
        Area         = p.Area,
        Timeline     = p.Timeline,
        Budget       = p.Budget,
        Category     = p.Category,
        IsFeatured   = p.IsFeatured,
        IsActive     = p.IsActive,
        Rate         = p.Rate,
        CreatedAt    = p.CreatedAt,
        Media        = p.Media.Select(m => new ProjectMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
        Materials    = p.Materials?.Select(m => new ProjectMaterialDto { Id = m.Id, MaterialName = m.MaterialName, Description = m.Description }).ToList() ?? new(),
    };
}
