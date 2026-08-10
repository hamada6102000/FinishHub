using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;
using test.Models;

namespace test.Services;

public class EngineerService
{
    private readonly AppDbContext _db;

    public EngineerService(AppDbContext db) => _db = db;

    public async Task<PagedResult<EngineerSummaryDto>> GetAllAsync(PaginationQuery pagination)
    {
        var query = UserService.ApplyActiveFilter(
            _db.Users
                .AsNoTracking()
                .Include(u => u.Reviews)
                .Include(u => u.City)
                .Where(u => u.UserType == UserType.Engineer),
            pagination.IncludeInactive);

        if (pagination.IsTrusted.HasValue && pagination.IsTrusted.Value)
        {
            query = query.Where(u => u.IsTrusted);
        }
        else if (pagination.IsTrusted.HasValue && !pagination.IsTrusted.Value)
        {
            query = query.Where(u => !u.IsTrusted);
        }
        var page = await query
            .OrderBy(u => u.NameEn)
            .ThenBy(u => u.Id)
            .ToPagedResultAsync(pagination);

        return page.Map(MapSummary);
    }

    public async Task<EngineerProfileDto?> GetProfileAsync(int id, string lang = "en", bool includeInactive = false)
    {
        var query = UserService.ApplyActiveFilter(_db.Users.AsNoTracking(), includeInactive);

        var engineer = await query
            // Only active projects surface in the profile response.
            .Include(u => u.Projects.Where(p => p.IsActive)).ThenInclude(p => p.Media)
            .Include(u => u.Portfolio).ThenInclude(p => p!.Media)
            .Include(u => u.Reviews).ThenInclude(r => r.Reviewer)
            .Include(u => u.City)
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Engineer);

        return engineer == null ? null : Map(engineer, lang);
    }

    /// <summary>Builds the filtered, unordered queryable used by Explore search.</summary>
    internal IQueryable<User> BuildExploreQuery(string? keyword, int? cityId, bool includeInactive = false)
    {
        var query = UserService.ApplyActiveFilter(
            _db.Users
                .AsNoTracking()
                .Include(u => u.Reviews)
                .Include(u => u.City)
                .Where(u => u.UserType == UserType.Engineer),
            includeInactive);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(u => u.NameEn.Contains(keyword) || u.NameAr.Contains(keyword));

        if (cityId.HasValue)
            query = query.Where(u => u.CityId == cityId.Value);

        return query;
    }

    internal static EngineerSummaryDto MapSummary(User engineer) => new()
    {
        Id              = engineer.Id,
        NameAr          = engineer.NameAr,
        NameEn          = engineer.NameEn,
        Position        = engineer.Position,
        Bio             = engineer.Bio,
        TotalExperience = engineer.TotalExperience,
        Rating          = engineer.Reviews.Count == 0 ? 0 : Math.Round(engineer.Reviews.Average(r => (double)r.Rate), 1),
        ReviewsCount    = engineer.Reviews.Count,
        City            = engineer.City?.NameEn,
        Country         = engineer.Country,
        ProfileImageUrl = engineer.ProfileImageUrl,
        CoverImageUrl   = engineer.CoverImageUrl,
        IsFavourite     = engineer.IsFavourite,
    };

    private static EngineerProfileDto Map(User engineer, string lang) => new()
    {
        Id              = engineer.Id,
        NameAr          = engineer.NameAr,
        NameEn          = engineer.NameEn,
        Position        = engineer.Position,
        Bio             = engineer.Bio,
        TotalExperience = engineer.TotalExperience,
        Rating          = engineer.Reviews.Count == 0 ? 0 : Math.Round(engineer.Reviews.Average(r => (double)r.Rate), 1),
        ReviewsCount    = engineer.Reviews.Count,
        City            = engineer.City?.NameEn,
        Country         = engineer.Country,
        ProfileImageUrl = engineer.ProfileImageUrl,
        CoverImageUrl   = engineer.CoverImageUrl,
        IsFavourite     = engineer.IsFavourite,
        Projects        = engineer.Projects.Select(p => new ProjectDto
        {
            Id           = p.Id,
            UserId       = p.UserId,
            UserName     = lang == "ar" ? engineer.NameAr : engineer.NameEn,
            Title        = p.Title,
            Location     = p.Location,
            PropertyType = p.PropertyType,
            Description  = p.Description,
            CreatedAt    = p.CreatedAt,
            Media        = p.Media.Select(m => new ProjectMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
        }).ToList(),
        Portfolio = engineer.Portfolio == null ? null : new PortfolioDto
        {
            Id     = engineer.Portfolio.Id,
            UserId = engineer.Portfolio.UserId,
            Media  = engineer.Portfolio.Media.Select(m => new PortfolioMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
        },
        Reviews = engineer.Reviews.Select(r => new ReviewDto
        {
            Id          = r.Id,
            UserId      = r.UserId,
            Reviewer    = r.Reviewer == null ? null : new ReviewerInfo
            {
                Id              = r.Reviewer.Id,
                NameAr          = r.Reviewer.NameAr,
                NameEn          = r.Reviewer.NameEn,
                ProfileImageUrl = r.Reviewer.ProfileImageUrl,
            },
            Description = r.Description,
            Rate        = r.Rate,
            CreatedAt   = r.CreatedAt,
        }).ToList(),
    };
}
