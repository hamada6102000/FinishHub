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
            .Include(p => p.User)
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
        var project = await _db.Projects.Include(p => p.Media).Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        return project == null ? null : Map(project, lang);
    }

    public async Task<PagedResult<ProjectDto>> GetUserProjectsAsync(int userId, PaginationQuery pagination, string lang = "en")
    {
        var page = await _db.Projects
            .Include(p => p.Media)
            .Include(p => p.User)
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
            PropertyType = req.PropertyType,
            Description  = req.Description,
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        await SaveMediaAsync(project.Id, req.Images, req.Videos);
        await _db.SaveChangesAsync();

        return Map((await _db.Projects.Include(p => p.Media).Include(p => p.User).FirstAsync(p => p.Id == project.Id)), lang);
    }

    public async Task<(bool success, ProjectDto? dto)> UpdateAsync(int id, int userId, UpdateProjectRequest req, string lang = "en")
    {
        var project = await _db.Projects.Include(p => p.Media).Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null || project.UserId != userId) return (false, null);

        if (req.Title       != null) project.Title        = req.Title;
        if (req.Location    != null) project.Location     = req.Location;
        if (req.Description != null) project.Description  = req.Description;
        if (req.PropertyType.HasValue) project.PropertyType = req.PropertyType.Value;

        await _db.SaveChangesAsync();
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

    public async Task<ProjectDto?> SetFeaturedAsync(int id, bool isFeatured, string lang = "en")
    {
        var project = await _db.Projects.Include(p => p.Media).Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return null;

        project.IsFeatured = isFeatured;
        await _db.SaveChangesAsync();
        return Map(project, lang);
    }

    private static ProjectDto Map(Project p, string lang) => new()
    {
        Id           = p.Id,
        UserId       = p.UserId,
        UserName     = lang == "ar" ? (p.User?.NameAr ?? string.Empty) : (p.User?.NameEn ?? string.Empty),
        Title        = p.Title,
        Location     = p.Location,
        PropertyType = p.PropertyType,
        Description  = p.Description,
        IsFeatured   = p.IsFeatured,
        CreatedAt    = p.CreatedAt,
        Media        = p.Media.Select(m => new ProjectMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
    };
}
