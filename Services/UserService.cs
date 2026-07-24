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
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        return user == null ? null : AuthService.MapUser(user);
    }

    public async Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        if (req.NameAr      != null) user.NameAr      = req.NameAr;
        if (req.NameEn      != null) user.NameEn      = req.NameEn;
        if (req.PhoneNumber != null) user.PhoneNumber = req.PhoneNumber;
        if (req.City        != null) user.City        = req.City;
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
        return AuthService.MapUser(user);
    }
}
