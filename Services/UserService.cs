using Microsoft.EntityFrameworkCore;
using test.Data;
using test.DTOs;
using test.Helpers;

namespace test.Services;

public class UserService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public UserService(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    public async Task<UserDto?> GetProfileAsync(int userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.City)
            .Include(u => u.Projects).ThenInclude(p => p.Media)
            .Include(u => u.Portfolio).ThenInclude(p => p!.Media)
            .Include(u => u.Reviews).ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var dto = AuthService.MapUser(user);
        dto.Rating       = user.Reviews.Count == 0 ? 0 : Math.Round(user.Reviews.Average(r => (double)r.Rate), 1);
        dto.ReviewsCount = user.Reviews.Count;
        dto.Reviews      = user.Reviews.Select(r => new ReviewDto
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
        }).ToList();
        dto.Projects = user.Projects.Select(p => new ProjectDto
        {
            Id           = p.Id, 
            UserId       = p.UserId,
            Title        = p.Title,
            Location     = p.Location,
            PropertyType = p.PropertyType,
            Description  = p.Description,
            CreatedAt    = p.CreatedAt,
            Media        = p.Media.Select(m => new ProjectMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
        }).ToList();
        dto.Portfolio = user.Portfolio == null ? new() : new List<PortfolioDto>
        {
            new PortfolioDto
            {
                Id     = user.Portfolio.Id,
                UserId = user.Portfolio.UserId,
                Media  = user.Portfolio.Media.Select(m => new PortfolioMediaDto { Id = m.Id, Url = m.Url, MediaType = m.MediaType }).ToList(),
            }
        };
        return dto;
    }

    public async Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        if (req.NameAr      != null) user.NameAr      = req.NameAr;
        if (req.NameEn      != null) user.NameEn      = req.NameEn;
        if (req.PhoneNumber != null) user.PhoneNumber = req.PhoneNumber;
        if (req.CityId.HasValue) user.CityId          = req.CityId;
        if (req.Country     != null) user.Country     = req.Country;
        if (req.Bio         != null) user.Bio         = req.Bio;
        if (req.Position    != null) user.Position    = req.Position;
        if (req.TotalExperience.HasValue) user.TotalExperience = req.TotalExperience;

        if (req.ProfileImage != null)
            user.ProfileImageUrl = await FileUploadHelper.SaveFileAsync(req.ProfileImage, "profiles", _env) ?? user.ProfileImageUrl;
        if (req.CoverImage != null)
            user.CoverImageUrl = await FileUploadHelper.SaveFileAsync(req.CoverImage, "covers", _env) ?? user.CoverImageUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (req.CityId.HasValue)
            await _db.Entry(user).Reference(u => u.City).LoadAsync();

        return AuthService.MapUser(user);
    }
}
