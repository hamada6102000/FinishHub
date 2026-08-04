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

    public async Task<PagedResult<ProjectDto>> GetAllAsync(PaginationQuery pagination, string lang = "en")
    {
        var query = _db.Projects
            .Include(p => p.Media)
            .Include(p => p.Materials)
            .Include(p => p.User)
            .Include(p => p.City)
            .AsQueryable();

        if (pagination.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == pagination.IsFeatured.Value);

        var page = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToPagedResultAsync(pagination);
        return page.Map(p => Map(p, lang));
    }

    public async Task<ProjectDto?> GetByIdAsync(int id, string lang = "en")
    {
        var project = await _db.Projects.Include(p => p.Media).Include(p => p.Materials).Include(p => p.User).Include(p => p.City).FirstOrDefaultAsync(p => p.Id == id);
        return project == null ? null : Map(project, lang);
    }

    public async Task<PagedResult<ProjectDto>> GetUserProjectsAsync(int userId, PaginationQuery pagination, string lang = "en")
    {
        var page = await _db.Projects
            .Include(p => p.Media)
            .Include(p => p.Materials)
            .Include(p => p.User)
            .Include(p => p.City)
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

        await SaveMediaAsync(project.Id, req.Images, req.Videos);
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
            _db.ProjectMaterials.RemoveRange(project.Materials);
            project.Materials.Clear();
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

    private async Task SaveMediaAsync(int projectId, List<IFormFile>? images, List<IFormFile>? videos)
    {
        var imageUrls = await FileUploadHelper.SaveFilesAsync(images, "projects/images", _env);
        var videoUrls = await FileUploadHelper.SaveFilesAsync(videos, "projects/videos", _env);

        foreach (var url in imageUrls)
            _db.ProjectMedia.Add(new ProjectMedia { ProjectId = projectId, Url = url, MediaType = MediaType.Image });
        foreach (var url in videoUrls)
            _db.ProjectMedia.Add(new ProjectMedia { ProjectId = projectId, Url = url, MediaType = MediaType.Video });
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

    /// <summary>Builds the filtered, unordered queryable used by both the plain project list and Explore search.</summary>
    internal IQueryable<Project> BuildExploreQuery(string? keyword, int? cityId, PropertyType? propertyType)
    {
        var query = _db.Projects
            .Include(p => p.Media)
            .Include(p => p.Materials)
            .Include(p => p.User)
            .Include(p => p.City)
            .AsQueryable();

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
        CreatedAt    = p.CreatedAt,
        Media        = p.Media.Select(m => new ProjectMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
        Materials    = p.Materials.Select(m => new ProjectMaterialDto { Id = m.Id, MaterialName = m.MaterialName, Description = m.Description }).ToList(),
    };
}
