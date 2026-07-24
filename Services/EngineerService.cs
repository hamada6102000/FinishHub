using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Models;

namespace test.Services;

public class EngineerService
{
    private readonly AppDbContext _db;

    public EngineerService(AppDbContext db) => _db = db;

    public async Task<EngineerProfileDto?> GetProfileAsync(int id)
    {
        var engineer = await _db.Users
            .AsNoTracking()
            .Include(u => u.Projects).ThenInclude(p => p.Media)
            .Include(u => u.Portfolio).ThenInclude(p => p!.Media)
            .Include(u => u.Reviews)
            .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Engineer);

        return engineer == null ? null : Map(engineer);
    }

    private static EngineerProfileDto Map(User engineer) => new()
    {
        Id              = engineer.Id,
        NameAr          = engineer.NameAr,
        NameEn          = engineer.NameEn,
        Position        = engineer.Position,
        Bio             = engineer.Bio,
        TotalExperience = engineer.TotalExperience,
        Rating          = engineer.Reviews.Count == 0 ? 0 : Math.Round(engineer.Reviews.Average(r => (double)r.Rate), 1),
        ReviewsCount    = engineer.Reviews.Count,
        City            = engineer.City,
        Country         = engineer.Country,
        ProfileImageUrl = engineer.ProfileImageUrl,
        CoverImageUrl   = engineer.CoverImageUrl,
        Projects        = engineer.Projects.Select(p => new ProjectDto
        {
            Id           = p.Id,
            UserId       = p.UserId,
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
            Id           = r.Id,
            UserId       = r.UserId,
            ReviewerName = r.ReviewerName,
            Description  = r.Description,
            Rate         = r.Rate,
            CreatedAt    = r.CreatedAt,
        }).ToList(),
    };
}
